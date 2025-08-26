using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HibernateSmart.Services
{
    /// <summary>
    /// Checks for processes or drivers that are preventing the system from sleeping.
    /// </summary>
    public static class SleepBlockerService
    {

        public static string GetSleepBlockersSummary()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -Command \"powercfg /requests\"",
                Verb = "runas",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            try
            {
                using (Process process = Process.Start(psi))
                {
                    if (process == null) return "Unable to start powercfg";

                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    var blockers = new List<string>();
                    string currentCategory = null;

                    foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (line.EndsWith(":"))
                            currentCategory = line.Replace(":", "").Trim();
                        else if (!line.Trim().Equals("None.", StringComparison.OrdinalIgnoreCase))
                        {
                            string processName = line.Trim();
                            int lastSlash = processName.LastIndexOf('\\');
                            if (lastSlash >= 0 && lastSlash < processName.Length - 1)
                                processName = processName.Substring(lastSlash + 1);
                            blockers.Add($"{processName} ({currentCategory})");
                        }
                    }

                    return blockers.Count == 0 ? "" : string.Join(", ", blockers);
                }
            }
            catch (Exception ex)
            {
                return $"Error checking sleep blockers: {ex.Message}";
            }
        }
    }
}