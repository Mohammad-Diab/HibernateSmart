using HibernateSmart.Core;
using HibernateSmart.Infrastructure;
using HibernateSmart.Infrastructure.SharedMemory;
using HibernateSmart.Utils;
using HibernateSmart.Utils.Time;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;


namespace HibernateSmart
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string MutexName = "HibernateSmart";
        private const string MapName = @"Global\HibernateSmart_MMF_V1";

        private UI.Tray.TrayBootstrapper _tray;
        private BackgroundHost _host;
        private SharedMemoryAccessor _ram;
        private RegistrySettingsAccessor _settings;
        private TimeProvider _time;
        private Guid _myGuid;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var username = UserSessions.GetUsername();
            if (!EnsureAdministrator()
                || !EnsureArchitectureMatch()
                || !EnsureSingleInstance(username))
            {
                Shutdown();
                return;
            }
            InitializeServices(username);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            var appMode = AppMode.Initializing;
            try
            {
                if (_host != null)
                    appMode = _host.IsServer ? AppMode.Server : AppMode.Clinet;
                Logger.Info("Shuting Down...", mode: appMode);
            }
            catch { }
            try { _tray?.Dispose(); } catch { }
            try { _ram?.Dispose(); } catch { }
            try { _host?.StopAsync().GetAwaiter().GetResult(); } catch { }
        }

        private bool EnsureAdministrator()
        {
            if (PermissionsChecker.IsAdministrator()) return true;

            var td = new TaskDialog
            {
                Caption = "Administrator Privileges Required",
                InstructionText = "Application must run as Administrator.",
                Text = "Do you want to restart the application with elevated privileges?",
                Icon = TaskDialogStandardIcon.Shield,
                StandardButtons = TaskDialogStandardButtons.None
            };

            var restartButton = new TaskDialogCommandLink("restart", "Restart as Administrator")
            {
                UseElevationIcon = true
            };
            restartButton.Click += (s, ev) =>
            {
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(new ProcessStartInfo
                {
                    FileName = exeName,
                    UseShellExecute = true,
                    Verb = "runas"
                });
                td.Close();
            };

            var exitButton = new TaskDialogCommandLink("exit", "Exit Application");
            exitButton.Click += (s, ev) => td.Close();

            td.Controls.Add(restartButton);
            td.Controls.Add(exitButton);
            td.Show();

            return false;
        }

        private bool EnsureArchitectureMatch()
        {
            var osArch = RuntimeInformation.OSArchitecture;
            var procArch = RuntimeInformation.ProcessArchitecture;

            if (osArch == procArch) return true;

            string message =
                $"Please download and install the {osArch.ToString().ToLower()} version for your system " +
                $"from <a href=\"https://github.com/Mohammad-Diab/HibernateSmart/releases\">HibernateSmart Releases</a>";

            new TaskDialog
            {
                Caption = "Architecture Mismatch",
                InstructionText = $"You are running the {procArch.ToString().ToLower()} version on a {osArch.ToString().ToLower()} system.",
                Text = message,
                Icon = TaskDialogStandardIcon.Error,
                HyperlinksEnabled = true,
                StandardButtons = TaskDialogStandardButtons.Ok
            }.Show();

            Shutdown();
            return false;
        }

        private bool EnsureSingleInstance(string username)
        {
            _ = new Mutex(true, $@"Global\{MutexName}_{username}", out bool createdNew);

            if (createdNew) return true;

            new TaskDialog
            {
                Caption = "Already Running",
                InstructionText = "You can run only one instance of HibernateSmart per user.",
                Icon = TaskDialogStandardIcon.Information,
                StandardButtons = TaskDialogStandardButtons.Ok
            }.Show();

            Shutdown();
            return false;
        }

        private void InitializeServices(string username)
        {
            _myGuid = Guid.NewGuid();

            _time = new TimeProvider();
            _ram = new SharedMemoryAccessor(MapName, SharedMemoryLayout.TotalSize);
            _ram.CreateOrOpenWithEveryone();

            _settings = new RegistrySettingsAccessor();

            _host = new BackgroundHost(
                ram: _ram,
                myGuid: _myGuid,
                username: username,
                time: _time);

            SettingsViewModel.Init(_host, _settings);

            Logger.Info("Log started", mode: AppMode.Initializing);
            Logger.Info("Initializing...", mode: AppMode.Initializing);
            Logger.Info(_host.IsServer ? "Current role: Server" : "Current role: Client", mode: AppMode.Initializing);

            _host.Start();

            _tray = new UI.Tray.TrayBootstrapper(_host, _settings, _ram);
            _tray.Initialize();
        }
    }
}
