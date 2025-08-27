using System;
using System.IO;

namespace HibernateSmart.Utils
{
    public static class Logger
    {
        private static readonly string LogFileName = DateTime.Now.ToString("yyyy-MM-dd");
        private static string GetLogDirectory()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);
            string logDir = Path.Combine(exeDirectory ?? Environment.CurrentDirectory, "logs");
            Directory.CreateDirectory(logDir);
            return logDir;
        }

        public static void Log(string message, bool writeToConsole = false)
        {
            string logDir = GetLogDirectory();
            string logFile = Path.Combine(logDir, $"{LogFileName}.txt");
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            File.AppendAllText(logFile, logEntry);
            if (writeToConsole)
            {
                Console.WriteLine(message);
            }
        }
    }
}
