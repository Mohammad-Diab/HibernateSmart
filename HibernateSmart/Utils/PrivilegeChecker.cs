using System;
using System.Runtime.Versioning;
using System.Security.Principal;
using HibernateSmart.Utils;

namespace HibernateSmart.Utils
{
    /// <summary>
    /// Checks if the application is running with Administrator privileges.
    /// </summary>
    public static class PrivilegeChecker
    {
        public static bool IsRunningAsAdmin()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                    Logger.Log($"IsRunningAsAdmin check: {isAdmin}");
                    return isAdmin;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error checking admin privilege: {ex.Message}");
                return false;
            }
        }
    }
}