using System.Diagnostics;

namespace HibernateSmart.Services
{
    /// <summary>
    /// Checks for processes or drivers that are preventing the system from sleeping.
    /// </summary>
    public static class SleepBlockerService
    {
        public static string GetSleepBlockersSummary()
        {
            try
            {
                using Process p = new();
                p.StartInfo.FileName = "powercfg";
                p.StartInfo.Arguments = "/requests";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                var blockers = new List<string>();
                string? currentCategory = null;

                foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.EndsWith(":"))
                        currentCategory = line.Replace(":", "").Trim();
                    else if (!line.Trim().Equals("None.", StringComparison.OrdinalIgnoreCase))
                    {
                        string processName = line.Trim();
                        int lastSlash = processName.LastIndexOf('\\');
                        if (lastSlash >= 0 && lastSlash < processName.Length - 1)
                            processName = processName[(lastSlash + 1)..];

                        blockers.Add($"{processName} ({currentCategory})");
                    }
                }

                return blockers.Count == 0 ? "" : string.Join(", ", blockers);
            }
            catch (Exception ex)
            {
                return $"Error checking sleep blockers: {ex.Message}";
            }
        }
    }
}