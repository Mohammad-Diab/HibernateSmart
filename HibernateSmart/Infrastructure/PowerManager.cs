using HibernateSmart.Utils;
using System.Runtime.InteropServices;

namespace HibernateSmart.Infrastructure
{
    public static class PowerManager
    {
        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        public static void Hibernate()
        {
            Logger.Info("Initiating system hibernation...", mode: AppMode.Server);
            if (!SetSuspendState(true, false, false))
            {
                Logger.Error("Hibernate command failed. Check power settings.", mode: AppMode.Server);
            }
        }
    }
}
