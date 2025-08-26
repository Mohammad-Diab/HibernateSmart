using System;
using System.Runtime.InteropServices;

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
            LASTINPUTINFO lii = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO)) };
            GetLastInputInfo(ref lii);
            uint idleTicks = (uint)Environment.TickCount - lii.dwTime;
            return TimeSpan.FromMilliseconds(idleTicks);
        }
    }
}