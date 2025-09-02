using HibernateSmart.Infrastructure.SharedMemory;
using HibernateSmart.Utils;
using System;

namespace HibernateSmart.Core
{
    public struct IdleStats
    {
        public int MinIdleSeconds { get; set; }
        public int ActiveUserCount { get; set; }
    }

    public class IdleDecisionEngine
    {
        private readonly SharedMemoryAccessor _ram;
        private readonly int _clientStaleMs;

        public IdleDecisionEngine(SharedMemoryAccessor ram, int clientStaleMs)
        {
            _ram = ram;
            _clientStaleMs = clientStaleMs;
        }

        public IdleStats GetMinActiveIdleSeconds(ulong now)
        {
            int min = int.MaxValue;
            int recordsCount = 0;
            for (int i = 0; i < SharedMemoryLayout.MaxUsers; i++)
            {
                int baseOff = SharedMemoryLayout.Off_EntryBase + (i * SharedMemoryLayout.EntrySize);
                byte len = _ram.ReadByte(baseOff + SharedMemoryLayout.Off_UsernameLen);
                if (len == 0) continue;

                string username = _ram.ReadString(baseOff + SharedMemoryLayout.Off_UsernameBytes, len);
                ulong lastUpdate = _ram.ReadUInt64(baseOff + SharedMemoryLayout.Off_LastUpdateMs);
                if (lastUpdate == 0 || (now - lastUpdate) >= (ulong)_clientStaleMs)
                    continue;

                int idle = _ram.ReadInt32(baseOff + SharedMemoryLayout.Off_IdleSeconds);
                if (idle < min) min = idle;
                Logger.Info($"User '{username}' idle time: {idle} seconds.", mode: AppMode.Server);
                recordsCount++;
            }
            return new IdleStats {
                MinIdleSeconds = min == int.MaxValue ? 0 : min,
                ActiveUserCount = recordsCount
            };
        }

        public bool ShouldHibernate(int minIdle)
        {
            return minIdle >= SettingsViewModel.IdleThresholdSeconds;
        }
    }
}