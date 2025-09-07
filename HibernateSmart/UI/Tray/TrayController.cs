using HibernateSmart.Core;
using HibernateSmart.Infrastructure.SharedMemory;
using HibernateSmart.UI.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
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

        [STAThread]
        public static void ExitApplication(SharedMemoryAccessor ram)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var td = new TaskDialog
                {
                    Caption = "Close SmartHibernate",
                    InstructionText = "If you close only your instance, hibernation may be triggered even while you're still active.",
                    Text = "Would you like to end all instances instead?",
                    Icon = TaskDialogStandardIcon.Warning,
                    StandardButtons = TaskDialogStandardButtons.None
                };

                var yesButton = new TaskDialogCommandLink() { Text = "Yes, End all instances (recommended)", Default = true, };
                yesButton.Click += (s, e) =>
                {
                    td.Close();
                    EndAll(ram);
                };

                var noButton = new TaskDialogCommandLink() { Text = "No, End only your instance" };
                noButton.Click += (s, e) =>
                {
                    td.Close();
                    System.Windows.Application.Current.Shutdown();
                };

                var cancelButton = new TaskDialogCommandLink() { Text = "Cancel" };
                cancelButton.Click += (s, e) =>
                {
                    td.Close();
                };

                td.Controls.Add(yesButton);
                td.Controls.Add(noButton);
                td.Controls.Add(cancelButton);
                td.Show();
            });
        }
    }
}