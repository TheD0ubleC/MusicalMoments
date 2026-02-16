using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Security.Principal;

namespace MusicalMoments
{
    internal static class SystemAudioService
    {
        public static bool CheckVirtualCableInstalled()
        {
            RegistryKey uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (uninstallKey == null)
            {
                return false;
            }

            foreach (string subkeyName in uninstallKey.GetSubKeyNames())
            {
                RegistryKey subkey = uninstallKey.OpenSubKey(subkeyName);
                string displayName = subkey?.GetValue("DisplayName") as string;
                if (!string.IsNullOrEmpty(displayName) && displayName.Contains("CABLE", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string[] GetInputAudioDeviceNames()
        {
            int waveInDevices = WaveIn.DeviceCount;
            string[] inputDeviceNames = new string[waveInDevices];
            for (int i = 0; i < waveInDevices; i++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(i);
                inputDeviceNames[i] = deviceInfo.ProductName;
            }

            return inputDeviceNames;
        }

        public static string[] GetOutputAudioDeviceNames()
        {
            int waveOutDevices = WaveOut.DeviceCount;
            string[] outputDeviceNames = new string[waveOutDevices];
            for (int i = 0; i < waveOutDevices; i++)
            {
                WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(i);
                outputDeviceNames[i] = deviceInfo.ProductName;
            }

            return outputDeviceNames;
        }

        public static int GetOutputDeviceId(string deviceName)
        {
            for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
            {
                WaveOutCapabilities capabilities = WaveOut.GetCapabilities(deviceId);
                if (capabilities.ProductName.Contains(deviceName, StringComparison.OrdinalIgnoreCase))
                {
                    return deviceId;
                }
            }

            return -1;
        }

        public static int GetInputDeviceId(string deviceName)
        {
            for (int deviceId = 0; deviceId < WaveIn.DeviceCount; deviceId++)
            {
                WaveInCapabilities capabilities = WaveIn.GetCapabilities(deviceId);
                if (capabilities.ProductName.Contains(deviceName, StringComparison.OrdinalIgnoreCase))
                {
                    return deviceId;
                }
            }

            return -1;
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
