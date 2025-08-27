using System;
using System.Runtime.InteropServices;
using HibernateSmart.Utils;

namespace HibernateSmart.Services
{
    /// <summary>
    /// Provides functionality to determine system idle time.
    /// </summary>
    public static class IdleTimeService
    {
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static TimeSpan GetIdleTime()
        {
            try
            {

                LASTINPUTINFO lii = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO)) };
                if (!GetLastInputInfo(ref lii))
                {
                    Logger.Log("Failed to get last input info.");
                    return TimeSpan.Zero;
                }
                uint idleTicks = (uint)Environment.TickCount - lii.dwTime;
                return TimeSpan.FromMilliseconds(idleTicks);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to get last input info: {ex.Message}");
                return TimeSpan.Zero;
            }
        }
    }
}