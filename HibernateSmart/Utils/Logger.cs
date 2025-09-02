using System;
using System.IO;

namespace HibernateSmart.Utils
{
    public static class Logger
    {
        private static readonly string LogFileName = DateTime.Now.ToString($"yyyy-MM-dd");
        private static readonly string _userId = UserSessions.GetUsername();

        internal static Action<AppMode, LogLevel, string> Logged { get; set; }

        private static string GetLogFileName(AppMode mode)
        {
            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string exeDirectory = Path.GetDirectoryName(exePath);
                string logDir = Path.Combine(exeDirectory ?? Environment.CurrentDirectory, "logs");
                Directory.CreateDirectory(logDir);
                string logFile = Path.Combine(logDir, $"{UserSessions.GetSafeFileName(_userId)}-{LogFileName}.txt");
                return logFile;
            }
            catch (Exception ex)
            {
                Logged?.Invoke(mode, LogLevel.Error, $"Logger directory creation failed: {ex}");
                throw ex;
            }
        }

        private static string GetLogLine(string message, AppMode mode, LogLevel level)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{_userId}] [{mode}] [{level}] {message}";
        }

        public static void Error(string message, bool writeToConsole = true, AppMode mode = AppMode.Clinet)
        {
            Log(message, mode, LogLevel.Error, writeToConsole);
        }
        public static void Warn(string message, bool writeToConsole = true, AppMode mode = AppMode.Clinet)
        {
            Log(message, mode, LogLevel.Warn, writeToConsole);
        }

        public static void Info(string message, bool writeToConsole = true, AppMode mode = AppMode.Clinet)
        {
            Log(message, mode, LogLevel.Info, writeToConsole);
        }

        private static void Log(string message, AppMode mode, LogLevel level, bool writeToConsole)
        {
            string logFile = GetLogFileName(mode);

            string line = GetLogLine(message, mode, level);

            if (writeToConsole)
            {
                Logged?.Invoke(mode, level, message);
            }

            if (SettingsViewModel.EnableLogging)
            {
                try
                {
                    File.AppendAllText(logFile, line + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Logged?.Invoke(mode, LogLevel.Error, $"Logger file write failed: {ex}");
                }
            }
        }
    }
}