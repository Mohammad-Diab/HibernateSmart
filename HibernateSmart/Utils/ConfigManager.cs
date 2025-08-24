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
                Directory.CreateDirectory("config");
                File.WriteAllText(configPath, $"IdleSecondsBeforeHibernate={fallback}");
                return fallback;
            }

            foreach (var line in File.ReadAllLines(configPath))
            {
                if (line.StartsWith("IdleSecondsBeforeHibernate="))
                {
                    string valueStr = line.Split('=')[1].Trim();
                    if (int.TryParse(valueStr, out int value))
                        return Math.Clamp(value, min, max);
                }
            }

            return fallback;
        }
    }
}