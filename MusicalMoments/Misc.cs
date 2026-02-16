using NAudio.Wave;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal class Misc
    {
        public static WaveOutEvent currentOutputDevice => AudioPlaybackService.CurrentOutputDevice;
        public static AudioFileReader currentAudioFile => AudioPlaybackService.CurrentAudioFile;

        public static bool checkVB() => SystemAudioService.CheckVirtualCableInstalled();

        public static string[] GetInputAudioDeviceNames() => SystemAudioService.GetInputAudioDeviceNames();

        public static string[] GetOutputAudioDeviceNames() => SystemAudioService.GetOutputAudioDeviceNames();

        public static int GetOutputDeviceID(string deviceName) => SystemAudioService.GetOutputDeviceId(deviceName);

        public static int GetInputDeviceID(string deviceName) => SystemAudioService.GetInputDeviceId(deviceName);

        public static void PlayAudioToSpecificDevice(string audioFilePath, int deviceNumber, bool stopCurrent, float volume, bool rplay, string raudioFilePath, int rdeviceNumber, float rvolume)
            => AudioPlaybackService.PlayAudioToSpecificDevice(audioFilePath, deviceNumber, stopCurrent, volume, rplay, raudioFilePath, rdeviceNumber, rvolume);

        public static void PlayAudioex(string audioFilePath, int deviceNumber, float volume) => AudioPlaybackService.PlayAudioEx(audioFilePath, deviceNumber, volume);

        public static void Delay(int time) => UiEffectsService.Delay(time);

        public static Task AddAudioFilesToListView(string directoryPath, ListView listView) => AudioLibraryService.AddAudioFilesToListView(directoryPath, listView);

        public static void WriteKeyJsonInfo(string jsonFilePath, string valueToWrite) => AudioLibraryService.WriteKeyJsonInfo(jsonFilePath, valueToWrite);

        public static void AddPluginFilesToListView(string rootDirectoryPath, ListView listView) => PluginCatalogService.AddPluginFilesToListView(rootDirectoryPath, listView);

        public static string GetKeyDisplay(KeyEventArgs keyEventArgs = null, KeyPressEventArgs keyPressEventArgs = null) => KeyBindingService.GetKeyDisplay(keyEventArgs, keyPressEventArgs);

        public static void StartMicrophoneToSpeaker(int inputDeviceIndex, int outputDeviceIndex) => AudioPlaybackService.StartMicrophoneToSpeaker(inputDeviceIndex, outputDeviceIndex);

        public static void StopMicrophoneToSpeaker() => AudioPlaybackService.StopMicrophoneToSpeaker();

        public static void StopCurrentPlayback() => AudioPlaybackService.StopCurrentPlayback();

        public static int NCMConvert(string filepath, string outputpath = "default") => AudioLibraryService.NcmConvert(filepath, outputpath);

        public static Task GetDownloadJsonFromFile(string filePath, ListView listView, ListBox downloadLinkListBox)
            => DiscoveryService.GetDownloadJsonFromFile(filePath, listView, downloadLinkListBox);

        public static Task<int?> GetTotalFromJsonFile(string filePath) => DiscoveryService.GetTotalFromJsonFile(filePath);

        public static Task DownloadJsonFile(string url, string filePath) => DiscoveryService.DownloadJsonFile(url, filePath);

        public static Task<bool> VerifyFileHashAsync(string filePath, string hashUrl) => DiscoveryService.VerifyFileHashAsync(filePath, hashUrl);

        public static bool IsAdministrator() => SystemAudioService.IsAdministrator();

        public static void ButtonStabilization(int time, Button button) => UiEffectsService.ButtonStabilization(time, button);

        public static Task FadeIn(int durationMilliseconds, Form form) => UiEffectsService.FadeIn(durationMilliseconds, form);

        public static Task FadeOut(int durationMilliseconds, Form form) => UiEffectsService.FadeOut(durationMilliseconds, form);

        public void ApplyResourcesToControls(Control.ControlCollection controls, string baseName, Assembly assembly)
            => LocalizationService.ApplyResourcesToControls(controls, baseName, assembly);

        public static void BuildLocalizationBaseFiles(Control.ControlCollection controls, string filePath)
            => LocalizationService.BuildLocalizationBaseFiles(controls, filePath);

        public static Task<string> GetLatestVersionAsync() => UpdateService.GetLatestVersionAsync();

        public static Task<string> GetLatestVersionTipsAsync() => UpdateService.GetLatestVersionTipsAsync();

        public static string DisplayAudioProperties(string audioFilePath) => AudioPlaybackService.DisplayAudioProperties(audioFilePath);

        public static int CalculateDaysBetweenDates(string dateStr1, string dateStr2)
            => DateTimeService.CalculateDaysBetweenDates(dateStr1, dateStr2);
    }
}
