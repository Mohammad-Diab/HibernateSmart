
using System.Runtime.InteropServices;

namespace HibernateSmart.Utils.Time
{
    public class TimeProvider
    {
        [DllImport("kernel32.dll")]
        private static extern ulong GetTickCount64();

        public ulong NowMs => GetTickCount64();
    }
}
