using HibernateSmart.Core;
using HibernateSmart.Infrastructure.SharedMemory;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;

namespace HibernateSmart.UI.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly RegistrySettingsAccessor _settings;
        private readonly BackgroundHost _host;

        public SettingsWindow(RegistrySettingsAccessor settings, BackgroundHost host)
        {
            InitializeComponent();

            _settings = settings;
            _host = host;

            if (!_host.IsServer)
            {
                Close();
            }

            var s = _settings.Get();
            EnableLoggingCheck.IsChecked = s.EnableLogging;
            IdleThresholdBox.Text = s.IdleThresholdSeconds.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int threshold;
            if (!int.TryParse(IdleThresholdBox.Text.Trim(), out threshold))
            {
                new TaskDialog
                {
                    Caption = "Validation",
                    InstructionText = "Idle threshold must be a valid integer value.", 
                    Icon = TaskDialogStandardIcon.Warning,
                    StandardButtons = TaskDialogStandardButtons.Ok
                }.Show();

                return;
            }

            if (threshold < 60 || threshold > 86400)
            {
                new TaskDialog
                {
                    Caption = "Validation",
                    InstructionText = "Idle threshold must be between 60 seconds (1 minute) and 86,400 seconds (24 hours).",
                    Icon = TaskDialogStandardIcon.Warning,
                    StandardButtons = TaskDialogStandardButtons.Ok,
                }.Show();
                return;
            }

            var s = _settings.Get();
            s.EnableLogging = EnableLoggingCheck.IsChecked == true;
            s.IdleThresholdSeconds = threshold;

            _settings.Set(s);
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

