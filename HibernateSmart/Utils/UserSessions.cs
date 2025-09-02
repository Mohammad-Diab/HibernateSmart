using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace HibernateSmart.Utils
{
    public static class UserSessions
    {

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(
            IntPtr hServer,
            int sessionId,
            WTS_INFO_CLASS wtsInfoClass,
            out IntPtr ppBuffer,
            out int pBytesReturned);

        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr memory);

        [DllImport("Kernel32.dll")]
        private static extern int WTSGetActiveConsoleSessionId();

        [DllImport("Wtsapi32.dll", SetLastError = true)]
        static extern bool WTSEnumerateSessions(
            IntPtr hServer,
            int Reserved,
            int Version,
            out IntPtr ppSessionInfo,
            out int pCount);

        [StructLayout(LayoutKind.Sequential)]
        struct WTS_SESSION_INFO
        {
            public int SessionID;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pWinStationName;
            public int State;
        }

        private enum WTS_INFO_CLASS
        {
            WTSUserName = 5
        }

        public static string GetUsername()
        {
            try
            {
                int sessionId = WTSGetActiveConsoleSessionId();

                if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WTS_INFO_CLASS.WTSUserName, out IntPtr buffer, out int bytesReturned) && bytesReturned > 1)
                {
                    string userName = Marshal.PtrToStringAnsi(buffer);
                    WTSFreeMemory(buffer);
                    return userName;
                }
                return Environment.UserName;
            }
            catch
            {
                return Environment.UserName;
            }
        }
        public static byte[] Encode(string username, int maxChars, int maxBytes, out byte length)
        {
            if (username == null) username = string.Empty;
            if (username.Length > maxChars)
                username = username.Substring(0, maxChars);

            var utf8 = Encoding.UTF8.GetBytes(username);
            if (utf8.Length > maxBytes)
            {
                var cut = new byte[maxBytes];
                Array.Copy(utf8, cut, maxBytes);
                utf8 = cut;
            }

            length = (byte)utf8.Length;
            return utf8;
        }

        public static string GetSafeFileName(string input, string replacement = "_")
        {
            if (string.IsNullOrWhiteSpace(input))
                return "untitled";

            var invalidChars = Path.GetInvalidFileNameChars();
            var safe = string.Join(replacement, input.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            if (safe.Length > 255)
                safe = safe.Substring(0, 255);

            return safe;
        }

        public static int GetActiveSessionCount()
        {
            // TODO: Implement this method to return the count of active user sessions.
            // try
            // {
            //     if (WTSEnumerateSessions(IntPtr.Zero, 0, 1, out IntPtr pSessions, out int count))
            //     {
            //         int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            //         int activeSessions = 0;

            //         IntPtr current = pSessions;

            //         for (int i = 0; i < count; i++)
            //         {
            //             WTS_SESSION_INFO si = Marshal.PtrToStructure<WTS_SESSION_INFO>(current);
            //             Logger.Info($"Session {i}: at {current}, sessionName: {si.pWinStationName}, sessionID={si.SessionID}, sessionState={si.State}");
            //             if (si.State == 0)
            //                 activeSessions++;

            //             current = IntPtr.Add(current, dataSize);
            //         }

            //         WTSFreeMemory(pSessions);
            //         return activeSessions;
            //     }
            // }
            // catch (Exception ex)
            // {
            //     Logger.Error($"Failed to get active session count: {ex}", mode: AppMode.Server);
            // }

            return 0;
        }

    }
}
