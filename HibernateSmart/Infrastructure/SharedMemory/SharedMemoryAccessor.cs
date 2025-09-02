using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.AccessControl;
using System.Security.Principal;

namespace HibernateSmart.Infrastructure.SharedMemory
{
    public sealed class SharedMemoryAccessor
    {
        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        private readonly string _mapName;
        private readonly long _capacity;

        public SharedMemoryAccessor(string mapName, long capacity)
        {
            _mapName = mapName;
            _capacity = capacity;
        }

        public void CreateOrOpenWithEveryone()
        {
            var security = new MemoryMappedFileSecurity();
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var rule = new AccessRule<MemoryMappedFileRights>(
                sid,
                MemoryMappedFileRights.FullControl,
                AccessControlType.Allow);
            security.AddAccessRule(rule);
            bool isNew = false;

            try
            {
                _mmf = MemoryMappedFile.OpenExisting(_mapName, MemoryMappedFileRights.ReadWrite);
            }
            catch (FileNotFoundException)
            {
                _mmf = MemoryMappedFile.CreateNew(
                    _mapName,
                    _capacity,
                    MemoryMappedFileAccess.ReadWrite,
                    MemoryMappedFileOptions.None,
                    security,
                    HandleInheritability.None);
                isNew = true;
            }

            _accessor = _mmf.CreateViewAccessor(0, _capacity, MemoryMappedFileAccess.ReadWrite);

            if (isNew)
            {
                _accessor.Write(SharedMemoryLayout.Off_SettingsFlags, (byte)1); // Enabled by default
            }
        }

        public void WriteGuid(int offset, Guid value)
        {
            var bytes = value.ToByteArray();
            _accessor.WriteArray(offset, bytes, 0, 16);
        }

        public Guid ReadGuid(int offset)
        {
            var bytes = new byte[16];
            _accessor.ReadArray(offset, bytes, 0, 16);
            return new Guid(bytes);
        }

        public void WriteUInt64(int offset, ulong value) => _accessor.Write(offset, value);
        public ulong ReadUInt64(int offset) { _accessor.Read(offset, out ulong v); return v; }

        public void WriteInt32(int offset, int value) => _accessor.Write(offset, value);
        public int ReadInt32(int offset) { _accessor.Read(offset, out int v); return v; }

        public void WriteByte(int offset, byte value) => _accessor.Write(offset, value);
        public byte ReadByte(int offset) { _accessor.Read(offset, out byte v); return v; }
        
        public bool ReadBoolean(int offset, int bitPosition)
        {
            if(bitPosition < 0 || bitPosition > 7) return false;
            _accessor.Read(offset, out byte v);
            return (v & (1 << bitPosition)) != 0;
        }
        public void WriteBoolean(int offset, int bitPosition, bool value)
        {
            if (bitPosition < 0 || bitPosition > 7) return;

            _accessor.Read(offset, out byte v);
            byte mask = (byte)(1 << bitPosition);

            if (value)
                v |= mask;
            else
                v &= (byte)~mask;

            _accessor.Write(offset, v);
        }

        public string ReadString(int offset, int length)
        {
            if (length < 0) return "";
            var bytes = new byte[length];
            _accessor.ReadArray(offset, bytes, 0, length);
            int stringLength = Array.IndexOf(bytes, (byte)0);
            if (stringLength < 0) stringLength = length;
            return System.Text.Encoding.UTF8.GetString(bytes, 0, stringLength);
        }

        public void Dispose()
        {
            _accessor?.Dispose();
            _mmf?.Dispose();
        }
    }
}