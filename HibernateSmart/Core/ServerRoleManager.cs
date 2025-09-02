using HibernateSmart.Infrastructure.SharedMemory;
using HibernateSmart.Utils;
using HibernateSmart.Utils.Time;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HibernateSmart.Core
{
    public class ServerRoleManager
    {
        private readonly SharedMemoryAccessor _ram;
        private readonly Guid _myGuid;
        private readonly TimeProvider _time;
        private readonly int _serverDeadMs;
        private readonly Random _rng = new Random();
        private volatile bool _isServer;

        public bool IsServer
        {
            get
            {
                if (_isServer)
                {
                    Guid serverGuid = _ram?.ReadGuid(SharedMemoryLayout.Off_ServerGuid) ?? _myGuid;
                    return serverGuid.Equals(_myGuid);
                }
                return false;
            }
        }

        public event Action BecameServer;
        public event Action LostServer;

        public ServerRoleManager(SharedMemoryAccessor ram, Guid myGuid, TimeProvider time, int serverDeadMs)
        {
            _ram = ram;
            _myGuid = myGuid;
            _time = time;
            _serverDeadMs = serverDeadMs;
        }

        public async Task MonitorOnce(CancellationToken token)
        {
            if (_isServer) return;

            var lastHb = _ram.ReadUInt64(SharedMemoryLayout.Off_LastHeartbeatMs);
            var now = _time.NowMs;
            var serverDead = (lastHb == 0UL) || (now - lastHb >= (ulong)_serverDeadMs);

            if (serverDead)
            {
                await Task.Delay(_rng.Next(50, 200), token);
                var lastHbSec = (now - lastHb) / 1000;
                Logger.Warn($"Server heartbeat missed for {lastHbSec} seconds. Initiating takeover attempt.");
                byte currentLock = _ram.ReadByte(SharedMemoryLayout.Off_ServerLock);
                if (currentLock == 0 || serverDead)
                {
                    _ram.WriteByte(SharedMemoryLayout.Off_ServerLock, 1);
                    _ram.WriteGuid(SharedMemoryLayout.Off_ServerGuid, _myGuid);
                    _ram.WriteUInt64(SharedMemoryLayout.Off_LastHeartbeatMs, now);
                    _ram.WriteInt32(SharedMemoryLayout.Off_Epoch, _ram.ReadInt32(SharedMemoryLayout.Off_Epoch) + 1);

                    _isServer = true;
                    BecameServer?.Invoke();
                }
            }
        }

        public void RelinquishServer()
        {
            if (_isServer)
            {
                _ram.WriteByte(SharedMemoryLayout.Off_ServerLock, 0);
                _ram.WriteGuid(SharedMemoryLayout.Off_ServerGuid, Guid.Empty);
                _ram.WriteUInt64(SharedMemoryLayout.Off_LastHeartbeatMs, 0UL);
                _ram.WriteInt32(SharedMemoryLayout.Off_Epoch, _ram.ReadInt32(SharedMemoryLayout.Off_Epoch) + 1);
                _isServer = false;
                Application.Current.Dispatcher.Invoke(() => LostServer?.Invoke());
            }
        }
    }
}