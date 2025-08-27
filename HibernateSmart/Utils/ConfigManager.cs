using System;
using System.IO;
using System.Reflection;
using HibernateSmart.Utils; // Add Logger usage

namespace HibernateSmart.Utils
{
    /// <summary>
    /// Manages reading and writing configuration values.
    /// </summary>
    public static class ConfigManager
    {
        private static string GetConfigPath()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);
            return Path.Combine(exeDirectory ?? Environment.CurrentDirectory, "config", "hibernateSmartConfig.conf");
        }

        public static int LoadIdleThreshold(int min, int max)
        {
            const int fallback = 3600;
            string configPath = GetConfigPath();
            if (!File.Exists(configPath))
            {
                try
                {
                    Directory.CreateDirectory("config");
                    File.WriteAllText(configPath, $"IdleSecondsBeforeHibernate={fallback}");
                    Logger.Log($"Config file created at {configPath} with fallback value {fallback}.", writeToConsole: true);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to create config file: {ex.Message}", writeToConsole: true);
                }

                return fallback;
            }

            foreach (var line in File.ReadAllLines(configPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;
                if (line.StartsWith("IdleSecondsBeforeHibernate="))
                {
                    string valueStr = line.Split('=')[1].Trim();
                    if (int.TryParse(valueStr, out int value))
                    {
                        Logger.Log($"Loaded IdleSecondsBeforeHibernate value: {value}");
                        if (value == 0)
                        {
                            return 0;
                        }
                        if (value < min) value = min;
                        if (value > max) value = max;
                        return value;
                    }
                    else
                    {
                        Logger.Log($"Failed to parse IdleSecondsBeforeHibernate value: {valueStr}");
                    }
                }
                break; // read only one line
            }

            Logger.Log($"Using fallback value {fallback} for IdleSecondsBeforeHibernate.");
            return fallback;
        }
    }
}