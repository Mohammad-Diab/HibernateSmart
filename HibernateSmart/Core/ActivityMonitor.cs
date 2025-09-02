using HibernateSmart.Infrastructure.SharedMemory;
using HibernateSmart.Utils;
using HibernateSmart.Utils.Time;
using System.Runtime.InteropServices;

namespace HibernateSmart.Core
{
    public class ActivityMonitor
    {
        private readonly SharedMemoryAccessor _ram;
        private readonly string _username;
        private readonly TimeProvider _time;
        private const int DebounceIntervalMs = 500;

        private ulong _lastUpdateMs;

        public ActivityMonitor(SharedMemoryAccessor ram, string username, TimeProvider time)
        {
            _ram = ram;
            _username = username;
            _time = time;
        }

        public void UpdateUserActivity()
        {
            if (_time.NowMs - _lastUpdateMs < DebounceIntervalMs)
                return;

            int index = FindOrCreateUserIndex();
            if (index < 0) return;

            int baseOff = SharedMemoryLayout.Off_EntryBase + (index * SharedMemoryLayout.EntrySize);
            _ram.WriteInt32(baseOff + SharedMemoryLayout.Off_IdleSeconds, GetIdleSeconds());
            _ram.WriteUInt64(baseOff + SharedMemoryLayout.Off_LastUpdateMs, _time.NowMs);
            _lastUpdateMs = _time.NowMs;
        }

        private int FindOrCreateUserIndex()
        {
            byte[] target = UserSessions.Encode(_username,
                SharedMemoryLayout.UsernameMaxChars,
                SharedMemoryLayout.UsernameMaxBytes,
                out byte len);

            int firstEmpty = -1;

            for (int i = 0; i < SharedMemoryLayout.MaxUsers; i++)
            {
                int baseOff = SharedMemoryLayout.Off_EntryBase + (i * SharedMemoryLayout.EntrySize);
                byte storedLen = _ram.ReadByte(baseOff + SharedMemoryLayout.Off_UsernameLen);

                if (storedLen == 0)
                {
                    if (firstEmpty == -1) firstEmpty = i;
                    continue;
                }

                bool match = storedLen == len;
                for (int b = 0; match && b < len; b++)
                {
                    if (_ram.ReadByte(baseOff + SharedMemoryLayout.Off_UsernameBytes + b) != target[b])
                        match = false;
                }

                if (match) return i;
            }

            // Create new entry in the first empty slot
            if (firstEmpty >= 0)
            {
                Logger.Info("Client index not found, creating new one.");
                int baseOff = SharedMemoryLayout.Off_EntryBase + (firstEmpty * SharedMemoryLayout.EntrySize);
                for (int i = 0; i < SharedMemoryLayout.UsernameMaxBytes; i++)
                {
                    byte b = (i < target.Length) ? target[i] : (byte)0;
                    _ram.WriteByte(baseOff + SharedMemoryLayout.Off_UsernameBytes + i, b);
                }
                _ram.WriteByte(baseOff + SharedMemoryLayout.Off_UsernameLen, len);
                return firstEmpty;
            }

            return -1;
        }

        private int GetIdleSeconds()
        {
            LASTINPUTINFO lii = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO)) };
            if (!GetLastInputInfo(ref lii))
                return 0;

            ulong now = GetTickCount64();
            ulong last = lii.dwTime;
            ulong diffMs = (now >= last) ? (now - last) : 0UL;
            int idleSeconds = (int)(diffMs / 1000UL);
            Logger.Info($"Idle Time: {idleSeconds} seconds");
            return idleSeconds;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("kernel32.dll")]
        static extern ulong GetTickCount64();
    }
}