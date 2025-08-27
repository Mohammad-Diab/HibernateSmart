using System;
using System.Runtime.InteropServices;

public static class SessionStateChecker
{
    private const int WTS_CURRENT_SERVER_HANDLE = 0;
    private const int WTS_CURRENT_SESSION = -1;
    private const int WTSConnectState = 4; // Info class

    [DllImport("wtsapi32.dll", SetLastError = true)]
    private static extern bool WTSQuerySessionInformation(
        IntPtr hServer,
        int sessionId,
        int wtsInfoClass,
        out IntPtr ppBuffer,
        out int pBytesReturned
    );

    [DllImport("wtsapi32.dll")]
    private static extern void WTSFreeMemory(IntPtr pMemory);

    private enum WTS_CONNECTSTATE_CLASS
    {
        WTSActive,
        WTSConnected,
        WTSConnectQuery,
        WTSShadow,
        WTSDisconnected,
        WTSIdle,
        WTSListen,
        WTSReset,
        WTSDown,
        WTSInit
    }

    /// <summary>
    /// Checks if the current session is active in the foreground.
    /// </summary>
    public static bool IsCurrentSessionActive()
    {
        if (WTSQuerySessionInformation(IntPtr.Zero, WTS_CURRENT_SESSION, WTSConnectState, out IntPtr buffer, out int bytes))
        {
            int state = Marshal.ReadInt32(buffer);
            WTSFreeMemory(buffer);
            return (WTS_CONNECTSTATE_CLASS)state == WTS_CONNECTSTATE_CLASS.WTSActive;
        }
        return false;
    }


    /// <summary>
    /// Checks if a session with a specific ID is active.
    /// </summary>
    public static bool IsSessionActive(int sessionId)
    {
        if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WTSConnectState, out IntPtr buffer, out int bytes))
        {
            int state = Marshal.ReadInt32(buffer);
            WTSFreeMemory(buffer);
            return (WTS_CONNECTSTATE_CLASS)state == WTS_CONNECTSTATE_CLASS.WTSActive;
        }
        return false;
    }
}