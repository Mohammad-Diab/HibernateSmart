using HibernateSmart.Models;
using Microsoft.Win32;

namespace HibernateSmart.Infrastructure.SharedMemory
{
    public class RegistrySettingsAccessor
    {
        // This will be in HKEY_LOCAL_MACHINE\SOFTWARE\HibernateSmart
        public static readonly string BaseKey = @"SOFTWARE\HibernateSmart";

        public HibernateSmartSettings Get()
        {
            var s = new HibernateSmartSettings
            {
                IdleThresholdSeconds = ReadInt("IdleThresholdSeconds", 3600),
                EnableLogging = ReadBool("EnableLogging", true),
            };
            return s;
        }

        public void Set(HibernateSmartSettings s)
        {
            WriteInt("IdleThresholdSeconds", s.IdleThresholdSeconds);
            WriteBool("EnableLogging", s.EnableLogging);
        }

        private int ReadInt(string name, int defaultValue)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(BaseKey))
            {
                return (int)key.GetValue(name, defaultValue);
            }
        }

        private bool ReadBool(string name, bool defaultValue)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(BaseKey))
            {
                return ((int)key.GetValue(name, defaultValue ? 1 : 0)) != 0;
            }
        }

        private void WriteInt(string name, int value)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(BaseKey))
            {
                key.SetValue(name, value, RegistryValueKind.DWord);
            }
        }

        private void WriteBool(string name, bool value)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(BaseKey))
            {
                key.SetValue(name, value ? 1 : 0, RegistryValueKind.DWord);
            }
        }
    }
}


