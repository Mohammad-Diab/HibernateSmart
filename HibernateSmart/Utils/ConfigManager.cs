namespace HibernateSmart.Utils
{
    /// <summary>
    /// Manages reading and writing configuration values.
    /// </summary>
    public static class ConfigManager
    {
        private static readonly string configPath = "config/hibernateSmartConfig.conf";

        public static int LoadIdleThreshold(int min, int max)
        {
            const int fallback = 3600;

            if (!File.Exists(configPath))
            {
                try
                {
                    Directory.CreateDirectory("config");
                    File.WriteAllText(configPath, $"IdleSecondsBeforeHibernate={fallback}");
                }
                catch
                {
                    Console.WriteLine("Failed to create config file.");
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
                        if (value == 0)
                        {
                            return 0;
                        }
                        return Math.Clamp(value, min, max);
                    }
                }
                break; // read only one line
            }

            return fallback;
        }
    }
}