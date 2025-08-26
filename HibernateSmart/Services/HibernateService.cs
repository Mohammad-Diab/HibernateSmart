using System;
using System.Diagnostics;

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
                Console.WriteLine("Initiating system hibernation...");
                Process.Start("shutdown", "/h");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to hibernate: {ex.Message}");
            }
        }
    }
}