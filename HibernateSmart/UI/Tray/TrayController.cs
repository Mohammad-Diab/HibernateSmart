using HibernateSmart.Core;
using HibernateSmart.Infrastructure.SharedMemory;
using HibernateSmart.UI.Windows;
using System.Windows.Forms;

namespace HibernateSmart.UI.Tray
{
    public static class TrayController
    {
        private static LogWindow _logWindow;
        private static SettingsWindow _settingsWindow;

        public static void OpenLogWindow(BackgroundHost host)
        {
            if (_logWindow == null || !_logWindow.IsLoaded)
            {
                _logWindow = new LogWindow(host);
                _logWindow.Show();
            }
            else
            {
                _logWindow.Activate();
            }
        }

        public static void OpenSettingsWindow(RegistrySettingsAccessor settings, BackgroundHost host)
        {
            if (_settingsWindow == null || !_settingsWindow.IsLoaded)
            {
                _settingsWindow = new SettingsWindow(settings, host);
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Activate();
            }
        }

        public static void ToggleEnabled(SharedMemoryAccessor ram, ToolStripMenuItem enableDisableItem)
        {
            var isEnabled = ram.ReadBoolean(SharedMemoryLayout.Off_SettingsFlags, SharedMemoryLayout.Enabled);
            ram.WriteBoolean(SharedMemoryLayout.Off_SettingsFlags, SharedMemoryLayout.Enabled, !isEnabled);
            enableDisableItem.Text = !isEnabled ? "Disable" : "Enable";
        }

        public static void EndAll(SharedMemoryAccessor ram)
        {
            ram.WriteBoolean(SharedMemoryLayout.Off_SettingsFlags, SharedMemoryLayout.EndAll, true);
        }

        public static void ExitApplication(SharedMemoryAccessor ram)
        {
            var result = MessageBox.Show(
                "Closing only your instance may trigger hibernation even if you're still active. \n\nWould you like to end all instances instead? \nYes, End all instances (recommended) \nNo, End only your instance",
                "Close SmartHibernate",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);
            switch (result)
            {
                case DialogResult.Yes:
                    EndAll(ram);
                    break;
                case DialogResult.No:
                    System.Windows.Application.Current.Shutdown();
                    break;
                default:
                    break;
            }
        }
    }
}