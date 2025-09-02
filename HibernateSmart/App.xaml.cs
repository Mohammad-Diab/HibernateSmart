using HibernateSmart.Core;
using HibernateSmart.Infrastructure;
using HibernateSmart.Infrastructure.SharedMemory;
using HibernateSmart.Utils;
using HibernateSmart.Utils.Time;
using System;
using System.Threading;
using System.Windows;

namespace HibernateSmart
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        const string _mutexName = "HibernateSmart";
        private UI.Tray.TrayBootstrapper _tray;
        private BackgroundHost _host;
        private SharedMemoryAccessor _ram;
        private RegistrySettingsAccessor _settings;
        private TimeProvider _time;
        private Guid _myGuid;

        private const string MapName = @"Global\HibernateSmart_MMF_V1";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!PermissionsChecker.IsAdministrator())
            {
                MessageBox.Show("Application must run as Administrator.", "Administrator Privileges Required", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            bool isOS64 = Environment.Is64BitOperatingSystem;
            bool isProcess64 = Environment.Is64BitProcess;

            if (isOS64 && !isProcess64)
            {
                MessageBox.Show(
                    "You are running the 32-bit version on a 64-bit system.\n" +
                    "Please install and run the 64-bit version for full functionality.",
                    "Architecture Mismatch",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown();
            }

            var username = UserSessions.GetUsername();
            _ = new Mutex(true, $@"Global\{_mutexName}_{username}", out bool createdNew);

            if (!createdNew)
            {
                MessageBox.Show("You can run only one instance of HibernateSmart per user.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

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

            _myGuid = Guid.NewGuid();

            Logger.Info("Log started", mode: AppMode.Initializing);

            Logger.Info("Initializing...", mode: AppMode.Initializing);

            Logger.Info(_host.IsServer ? "Current role: Server" : "Current role: Client", mode: AppMode.Initializing);

            _host.Start();

            _tray = new UI.Tray.TrayBootstrapper(_host, _settings, _ram);
            _tray.Initialize();
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
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
            try { await _host?.StopAsync(); } catch { }
            try { _ram?.Dispose(); } catch { }
        }
    }
}
