using HibernateSmart.Infrastructure;
using HibernateSmart.Infrastructure.SharedMemory;
using HibernateSmart.Services;
using HibernateSmart.Utils;
using HibernateSmart.Utils.Time;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HibernateSmart.Core
{
    public class BackgroundHost
    {
        private readonly SharedMemoryAccessor _ram;
        private readonly string _username;
        private readonly Guid _myGuid;
        private readonly TimeProvider _time;

        private readonly ServerRoleManager _roleManager;
        private readonly ActivityMonitor _activityMonitor;
        private readonly IdleDecisionEngine _decisionEngine;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _clientTask;
        private Task _electionTask;
        private Task _serverTask;
        private Task _shutdownTask;

        public bool IsServer => _roleManager.IsServer;
        public CancellationToken GlobalCancellationToken => _cts.Token;

        public event Action<bool> RoleChanged;

        public BackgroundHost(SharedMemoryAccessor ram, Guid myGuid, string username, TimeProvider time)
        {
            _ram = ram;
            _username = username;
            _myGuid = myGuid;
            _time = time;

            _roleManager = new ServerRoleManager(_ram, _myGuid, _time, SharedMemoryLayout.ServerDeadMs);
            _activityMonitor = new ActivityMonitor(_ram, _username, _time);
            _decisionEngine = new IdleDecisionEngine(_ram, SharedMemoryLayout.ClientStaleMs);

            _roleManager.BecameServer += () =>
            {
                Logger.Info("This is now the Server", mode: AppMode.Initializing);
                RoleChanged?.Invoke(true);
            };

            _roleManager.LostServer += () =>
            {
                Logger.Info("This is no longer the Server", mode: AppMode.Initializing);
                RoleChanged?.Invoke(false);
            };
        }

        public void Start()
        {
            Logger.Info("Starting...", mode: _roleManager.IsServer ? AppMode.Server : AppMode.Clinet);

            _clientTask = Task.Run(() => ClientLoopAsync(_cts.Token));
            _electionTask = Task.Run(() => ElectionLoopAsync(_cts.Token));
            _serverTask = Task.Run(() => ServerLoopAsync(_cts.Token));
            _shutdownTask = Task.Run(() => ShutdownLoopAsync(_cts.Token));
        }


        public async Task StopAsync()
        {
            _cts?.Cancel();
            _roleManager.RelinquishServer();

            var allTasks = Task.WhenAll(_electionTask, _clientTask, _serverTask);
            var timeout = Task.Delay(SharedMemoryLayout.KillTimeout);
            await Task.WhenAny(allTasks, timeout);
        }

        private async Task ShutdownLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var endAll = _ram.ReadBoolean(SharedMemoryLayout.Off_SettingsFlags, SharedMemoryLayout.EndAll);
                if (endAll)
                {
                    Logger.Warn("End All signal received. Shutting down...");
                    await StopAsync();
                    Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
                    break;
                }
                await Task.Delay(SharedMemoryLayout.ClientUpdateMs, token);
            }
        }

        private async Task ClientLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _activityMonitor.UpdateUserActivity();
                }
                catch
                {
                    Logger.Info("Failed to update last input info.");
                }

                await Task.Delay(SharedMemoryLayout.ClientUpdateMs, token);
            }
        }


        private async Task ElectionLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await _roleManager.MonitorOnce(token);
                await Task.Delay(SharedMemoryLayout.ElectionSleepMs, token);
            }
        }

        private async Task ServerLoopAsync(CancellationToken token)
        {
            bool hibernateTriggered = false;
            ulong lastHibernateCheck = 0;
            if (_roleManager.IsServer)
                Logger.Info($"Idle threshold: {SettingsViewModel.IdleThresholdSeconds} seconds", mode: AppMode.Server);
            while (!token.IsCancellationRequested)
            {
                if (!_roleManager.IsServer)
                {
                    await Task.Delay(SharedMemoryLayout.ServerLoopSleepWhenNotServerMs, token);
                    continue;
                }

                try
                {
                    var now = _time.NowMs;
                    _ram.WriteUInt64(SharedMemoryLayout.Off_LastHeartbeatMs, now);
                    _ram.WriteByte(SharedMemoryLayout.Off_ServerLock, 1);
                    bool isEnabled = _ram.ReadBoolean(SharedMemoryLayout.Off_SettingsFlags, SharedMemoryLayout.Enabled);
                    Logger.Info($"Updating Server heartbeat.", mode: AppMode.Server);
                    CleanupStaleEntries(now);

                    if (isEnabled)
                    {
                        int sessionCount = UserSessions.GetActiveSessionCount();
                        var idleStats = _decisionEngine.GetMinActiveIdleSeconds(now);
                        if (idleStats.ActiveUserCount < sessionCount)
                        {
                            Logger.Warn("Not all users have a running instance of HibernateSmart. Skipping hibernation", mode: AppMode.Server);
                            continue;
                        }
                        Logger.Info($"Minimum active idle time across users: {idleStats.MinIdleSeconds} seconds.", mode: AppMode.Server);

                        if (hibernateTriggered)
                        {
                            Logger.Warn("Hibernation was recently triggered, waiting for user activity to reset.", mode: AppMode.Server);
                            if (idleStats.MinIdleSeconds < SettingsViewModel.IdleThresholdSeconds)
                            {
                                hibernateTriggered = false;
                                Logger.Warn("User activity detected — hibernate timer reset.", mode: AppMode.Server);
                            }
                        }
                        else if (_decisionEngine.ShouldHibernate(idleStats.MinIdleSeconds))
                        {
                            if (now - lastHibernateCheck > 5000)
                            {
                                string sleepBlocker = SleepBlocker.GetSleepBlockersSummary();
                                if (string.IsNullOrEmpty(sleepBlocker))
                                {
                                    Logger.Info("Nothing is blocking sleep.", mode: AppMode.Server);
                                    Logger.Info("Idle threshold exceeded and no blockers. Triggering hibernation.", mode: AppMode.Server);
                                    hibernateTriggered = true;
                                    PowerManager.Hibernate();
                                    lastHibernateCheck = now;
                                }
                                else
                                {
                                    Logger.Warn($"System Sleep Blockers: {sleepBlocker}", mode: AppMode.Server);
                                }
                            }
                            else
                            {
                                Logger.Warn($"Hibernate check skipped to avoid rapid re-triggering.", mode: AppMode.Server);
                            }
                        }
                    }
                    else
                    {
                        Logger.Warn("Hibernation is disabled in settings.", mode: AppMode.Server);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Server loop error: {ex.Message}", mode: AppMode.Server);
                }

                await Task.Delay(SharedMemoryLayout.ServerHeartbeatMs, token);
            }
        }

        private void CleanupStaleEntries(ulong now)
        {
            List<string> cleanedUsers = new List<string>();
            for (int i = 0; i < SharedMemoryLayout.MaxUsers; i++)
            {
                int baseOff = SharedMemoryLayout.Off_EntryBase + (i * SharedMemoryLayout.EntrySize);
                byte len = _ram.ReadByte(baseOff + SharedMemoryLayout.Off_UsernameLen);
                if (len == 0) continue;
                ulong last = _ram.ReadUInt64(baseOff + SharedMemoryLayout.Off_LastUpdateMs);
                if (last == 0 || (now - last) >= SharedMemoryLayout.ClientStaleMs)
                {
                    string user = _ram.ReadString(baseOff + SharedMemoryLayout.Off_UsernameBytes, len);
                    _ram.WriteByte(baseOff + SharedMemoryLayout.Off_UsernameLen, 0);
                    cleanedUsers.Add(user);
                }
            }
            if (cleanedUsers.Count > 0) Logger.Warn($"Cleaning up stale entries \"{string.Join("\", \"", cleanedUsers)}\"", mode: AppMode.Server);
        }
    }
}
