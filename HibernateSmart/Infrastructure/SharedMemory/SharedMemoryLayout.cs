
namespace HibernateSmart.Infrastructure.SharedMemory
{
    public static class SharedMemoryLayout
    {
        // Header (server state)
        public const int Off_ServerGuid = 0;            // 16 bytes
        public const int Off_LastHeartbeatMs = 16;      // 8 bytes
        public const int Off_ServerLock = 24;           // 1 byte
        public const int Off_HeaderPadA = 25;           // 3 bytes
        public const int Off_Epoch = 28;                // 4 bytes

        // Shared settings (affect all instances)
        public const int Off_SettingsIdleThreshold = 32; // 4 bytes (int)
        public const int Off_SettingsFlags = 36;         // 1 byte (bit0: Enabled, bit7: EndAll)
        public const int Off_HeaderPadB = 37;            // 3 bytes
        public const int HeaderSize = 40;

        // Users table
        public const int MaxUsers = 64;
        public const int EntrySize = 96;
        public const int Off_EntryBase = HeaderSize;
        public const int Off_UsernameBytes = 0;         // 80 bytes
        public const int Off_UsernameLen = 80;          // 1 byte
        public const int Off_EntryPad = 81;             // 3 bytes
        public const int Off_IdleSeconds = 84;          // 4 bytes
        public const int Off_LastUpdateMs = 88;         // 8 bytes

        // Username constraints
        public const int UsernameMaxChars = 20;
        public const int UsernameMaxBytes = 80;

        // Timers (ms)
        public const int ServerHeartbeatMs = 5000;
        public const int ClientUpdateMs = 2000;
        public const int KillTimeout = 5500;
        public const int ElectionSleepMs = 250;
        public const int ServerLoopSleepWhenNotServerMs = 250;
        public const int ServerDeadMs = 15000;
        public const int ClientStaleMs = 10000;

        // Total capacity
        public static readonly int TotalSize = HeaderSize + (EntrySize * MaxUsers);

        // Settings flag positions
        public const byte Enabled = 0;
        public const byte EndAll = 7;
    }
}

