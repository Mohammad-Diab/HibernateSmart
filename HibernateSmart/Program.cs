using HibernateSmart.Services;
using HibernateSmart.Utils;
using System;
using System.Threading;

namespace HibernateSmart
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("HibernateSmart started.");
            const int minIdleThreshold = 60;     // Minimum allowed idle time in seconds
            const int maxIdleThreshold = 86400;  // Maximum allowed idle time in seconds

            // Ensure the program is running with Administrator privileges
            if (!PrivilegeChecker.IsRunningAsAdmin())
            {
                Logger.Log("Program not running as Administrator. Exiting...", writeToConsole: true);
                return;
            }

            // Load idle threshold from config file
            int idleThreshold = ConfigManager.LoadIdleThreshold(minIdleThreshold, maxIdleThreshold);

            if (idleThreshold == 0)
            {
                Logger.Log("Idle threshold set to 0. Program will terminate.", writeToConsole: true);
                return;
            }

            bool hibernateTriggered = true;

            while (true)
            {
                Console.Clear();
                if (!SessionStateChecker.IsCurrentSessionActive())
                {
                    Logger.Log("Current session is not active. Skipping this check.", writeToConsole: true);
                    Thread.Sleep(5000); // Check every 5 seconds
                    continue;
                }

                var idle = IdleTimeService.GetIdleTime();
                var blockers = SleepBlockerService.GetSleepBlockersSummary();

                Logger.Log($"Idle Time: {idle.TotalSeconds:F0} seconds, Blockers: {(string.IsNullOrEmpty(blockers) ? "None" : blockers)}");

                
                Console.WriteLine("Smart Hibernate is monitoring your system.");
                Console.WriteLine($"Idle threshold: {idleThreshold} seconds");
                Console.WriteLine($"Idle Time: {idle.TotalSeconds:F0} seconds");

                if (string.IsNullOrEmpty(blockers))
                    Console.WriteLine("Nothing is blocking sleep");
                else
                    Console.WriteLine($"Sleep Blockers: {blockers}");

                // Trigger hibernation if idle time exceeds threshold and no blockers exist
                if (hibernateTriggered)
                {
                    if (idle.TotalSeconds < idleThreshold)
                    {
                        hibernateTriggered = false;
                        Logger.Log("User activity detected — hibernate timer reset.", writeToConsole: true);
                    }
                }
                else
                {
                    if (idle.TotalSeconds > idleThreshold && string.IsNullOrEmpty(blockers))
                    {
                        Logger.Log("Idle threshold exceeded and no blockers. Triggering hibernation.", writeToConsole: true);
                        HibernateService.HibernateSystem();
                        hibernateTriggered = true;
                    }
                }
                Thread.Sleep(5000); // Check every 5 seconds
            }

        }
    }
}