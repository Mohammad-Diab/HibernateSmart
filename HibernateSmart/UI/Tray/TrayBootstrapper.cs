using HibernateSmart.Core;
using HibernateSmart.Infrastructure.SharedMemory;
using System;
using System.Windows.Forms;
using System.Drawing;

namespace HibernateSmart.UI.Tray
{
    public sealed class TrayBootstrapper : IDisposable
    {
        private readonly BackgroundHost _host;
        private readonly RegistrySettingsAccessor _settings;
        private readonly SharedMemoryAccessor _ram;
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _menu;

        public TrayBootstrapper(BackgroundHost host, RegistrySettingsAccessor settings, SharedMemoryAccessor ram)
        {
            _host = host;
            _settings = settings;
            _ram = ram;
        }

        public void Initialize()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = LoadIcon(),
                Visible = true,
                Text = $"HibernateSmart {(_host.IsServer ? "(Server)" : "(Client)")}"
            };

            _menu = new ContextMenuStrip();

            FillMenu();

            _trayIcon.ContextMenuStrip = _menu;

            _host.RoleChanged += isServer =>
            {
                FillMenu();
                
                _trayIcon.Text = $"HibernateSmart {(isServer ? "(Server)" : "(Client)")}";
            };
        }

        private void FillMenu()
        {
            if (_menu == null || _host == null) return;

            _menu.Items.Clear();

            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += (sender, e) => TrayController.OpenLogWindow(_host);
            _menu.Items.Add(openItem);

            if (_host.IsServer)
            {
                var settingsItem = new ToolStripMenuItem("Settings");
                settingsItem.Click += (sender, e) => TrayController.OpenSettingsWindow(_settings, _host);
                _menu.Items.Add(settingsItem);
            }

            _menu.Items.Add(new ToolStripSeparator());

            var _statusItem = new ToolStripMenuItem(_host.IsServer ? "Runnig Mode: Server" : "Runnig Mode: Client") { Enabled = false };
            _menu.Items.Add(_statusItem);

            if (_host.IsServer)
            {
                var enabled = _ram.ReadBoolean(SharedMemoryLayout.Off_SettingsFlags, SharedMemoryLayout.Enabled);
                string enableDisableText = enabled ? "Disable" : "Enable";
                var _enableDisableItem = new ToolStripMenuItem(enableDisableText);
                _enableDisableItem.Click += (sender, e) =>
                {
                    TrayController.ToggleEnabled(_ram, _enableDisableItem);
                };
                _menu.Items.Add(_enableDisableItem);
            }

            _menu.Items.Add(new ToolStripSeparator());

            // It is pointless to close one instance of application they should be closed all
            var exitItem = new ToolStripMenuItem("Close");
            exitItem.Click += (sender, e) => TrayController.ExitApplication(_ram);
            _menu.Items.Add(exitItem);

            // if (_host.IsServer)
            // {
            var endAll = new ToolStripMenuItem("End All");
            endAll.Click += (sender, e) =>
            {
                TrayController.EndAll(_ram);
            };
            _menu.Items.Add(endAll);
            // }
        }

        private Icon LoadIcon()
        {
            try
            {
                return Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? SystemIcons.Application;
            }
            catch
            {
                return SystemIcons.Application;
            }
        }

        public void Dispose()
        {
            try { if (_trayIcon != null) _trayIcon.Visible = false; } catch { }
            _trayIcon?.Dispose();
            _menu?.Dispose();
        }
    }
}