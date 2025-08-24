using HibernateSmart.Services;
using HibernateSmart.Utils;

const int minIdleThreshold = 60;     // Minimum allowed idle time in seconds
const int maxIdleThreshold = 86400;  // Maximum allowed idle time in seconds

#if WINDOWS

// Ensure the program is running with Administrator privileges
if (!PrivilegeChecker.IsRunningAsAdmin())
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Please run this program as Administrator to check sleep blockers.");
    Console.ResetColor();
    return;
}

#endif

// Load idle threshold from config file
int idleThreshold = ConfigManager.LoadIdleThreshold(minIdleThreshold, maxIdleThreshold);

if (idleThreshold == 0)
{
    Console.WriteLine("A value of 0 was entered. The program will terminate.");
    return;
}

bool hibernateTriggered = false;

while (true)
{
    var idle = IdleTimeService.GetIdleTime();
    var blockers = SleepBlockerService.GetSleepBlockersSummary();

    Console.Clear();
    Console.WriteLine("Smart Hibernate is monitoring your system.");
    Console.WriteLine($"Idle threshold: {idleThreshold} seconds");
    Console.WriteLine($"Idle Time: {idle.TotalSeconds:F0} seconds");

    if (string.IsNullOrEmpty(blockers))
        Console.WriteLine("Nothing is blocking sleep");
    else
        Console.WriteLine($"Sleep Blockers: {blockers}");

    // Trigger hibernation if idle time exceeds threshold and no blockers exist
    if (!hibernateTriggered && idle.TotalSeconds > idleThreshold && string.IsNullOrEmpty(blockers))
    {
        HibernateService.HibernateSystem();
        hibernateTriggered = true;
    }
    else if (hibernateTriggered && idle.TotalSeconds < idleThreshold)
    {
        Console.WriteLine("User activity detected — hibernate timer reset.");
        hibernateTriggered = false;
    }

    Thread.Sleep(5000); // Check every 5 seconds
}
