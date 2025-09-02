using HibernateSmart.Core;
using HibernateSmart.Infrastructure.SharedMemory;
using HibernateSmart.Models;
using HibernateSmart.Utils;
using Microsoft.Win32;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HibernateSmart
{
    public static class SettingsViewModel
    {
        private static RegistrySettingsAccessor _accessor;
        private static HibernateSmartSettings _settings;
        private static BackgroundHost _host;
        private static readonly object _lock = new object();
        private static RegistryKey _watchedKey;
        private static bool _initialized;

        public static void Init(BackgroundHost host, RegistrySettingsAccessor accessor)
        {
            if (_initialized) return;
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            _host = host ?? throw new ArgumentNullException(nameof(host));
            LoadSettings();
            StartWatching(host.GlobalCancellationToken);
            _initialized = true;
        }

        public static bool EnableLogging
        {
            get
            {
                EnsureInit();
                lock (_lock) return _settings.EnableLogging;
            }
        }

        public static int IdleThresholdSeconds
        {
            get
            {
                EnsureInit();
                lock (_lock) return _settings.IdleThresholdSeconds;
            }
        }

        private static void LoadSettings()
        {
            lock (_lock)
            {
                _settings = _accessor.Get();
            }
        }

        public static void StartWatching(CancellationToken token)
        {
            _watchedKey = Registry.LocalMachine.OpenSubKey(RegistrySettingsAccessor.BaseKey, writable: false);

            if (_watchedKey != null)
            {
                Task.Run(async () =>
                {
                    using (var changeEvent = new AutoResetEvent(false))
                    {
                        while (!token.IsCancellationRequested)
                        {
                            try
                            {
                                var handle = _watchedKey.Handle;

                                int res = RegistryNotify.RegNotifyChangeKeyValue(
                                    handle,
                                    true,
                                    RegistryNotify.REG_NOTIFY_CHANGE_LAST_SET,
                                    changeEvent.SafeWaitHandle.DangerousGetHandle(),
                                    true);

                                if (res != 0)
                                {
                                    Logger.Error("RegNotifyChangeKeyValue failed to register.", mode: _host.IsServer ? AppMode.Server : AppMode.Clinet);
                                    break;
                                }

                                int triggered = WaitHandle.WaitAny(new[] { changeEvent, token.WaitHandle });
                                // if (triggered == 258) continue;
                                if (triggered == 1) break;

                                LoadSettings();
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"Error watching registry key: {ex.Message}", mode: _host.IsServer ? AppMode.Server : AppMode.Clinet);
                            }
                            await Task.Delay(200, token);
                        }
                    }
                }, token);
            }
        }

        private static void EnsureInit()
        {
            if (!_initialized)
                throw new InvalidOperationException("SettingsViewModel.Init must be called before use.");
        }
    }
}