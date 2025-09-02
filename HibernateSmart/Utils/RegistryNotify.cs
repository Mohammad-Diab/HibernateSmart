using System;

namespace HibernateSmart.Utils
{
    internal static class RegistryNotify
    {
        public const int REG_NOTIFY_CHANGE_LAST_SET = 0x00000004;

        [System.Runtime.InteropServices.DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegNotifyChangeKeyValue(
            Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey,
            bool bWatchSubtree,
            int dwNotifyFilter,
            IntPtr hEvent,
            bool fAsynchronous);
    }
}
