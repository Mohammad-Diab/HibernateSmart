using System;
using System.Diagnostics;
using HibernateSmart.Utils;

namespace HibernateSmart.Services
{
    /// <summary>
    /// Handles system hibernation.
    /// </summary>
    public static class HibernateService
    {
        public static void HibernateSystem()
        {
            try
            {
                Logger.Log("Initiating system hibernation...", writeToConsole: true);
                Process.Start("shutdown", "/h");
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to hibernate: {ex.Message}", writeToConsole: true);
            }
        }
    }
}