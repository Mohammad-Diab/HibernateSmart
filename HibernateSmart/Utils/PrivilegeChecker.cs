using System.Runtime.Versioning;
using System.Security.Principal;

namespace HibernateSmart.Utils
{
    /// <summary>
    /// Checks if the application is running with Administrator privileges.
    /// </summary>
    public static class PrivilegeChecker
    {
        [SupportedOSPlatform("windows")]
        public static bool IsRunningAsAdmin()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}