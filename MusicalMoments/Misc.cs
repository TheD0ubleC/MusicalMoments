using HtmlAgilityPack;
using Microsoft.Win32;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Resources;
using System.Text.Json;
namespace MusicalMoments
{
    internal class Misc
    {
        private static WaveOutEvent currentOutputDevice = null;
        private static AudioFileReader currentAudioFile = null;
        public static bool checkVB()
        {
            bool isVBCableInstalled = false; // 初始化VB-Cable安装状态为假（未安装）
            // 打开注册表中的卸载信息部分
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key != null) // 确保key不为null
            {
                foreach (string subkeyName in key.GetSubKeyNames()) // 遍历所有子键
                {
                    RegistryKey subkey = key.OpenSubKey(subkeyName);
                    string displayName = subkey.GetValue("DisplayName") as string;
                    if (!string.IsNullOrEmpty(displayName) && displayName.Contains("CABLE")) // 检查显示名称是否包含"CABLE"
                    {
                        isVBCableInstalled = true; // 如果找到，更新安装状态为真（已安装）
                        break; // 退出循环
                    }
                }
            }
            return isVBCableInstalled;
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
        public static int GetOutputDeviceID(string deviceName)
        {
            for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
            {
                var capabilities = WaveOut.GetCapabilities(deviceId);
                if (capabilities.ProductName.Contains(deviceName, StringComparison.OrdinalIgnoreCase))
                {
                    return deviceId;
                }
            }
            return -1; // 没有找到匹配的设备
        }
        public static int GetInputDeviceID(string deviceName)
        {
            for (int deviceId = 0; deviceId < WaveIn.DeviceCount; deviceId++)
            {
                var capabilities = WaveIn.GetCapabilities(deviceId);
                if (capabilities.ProductName.Contains(deviceName, StringComparison.OrdinalIgnoreCase))
                {
                    return deviceId;
                }
            }
            return -1; // 没有找到匹配的设备
        }
        private static bool ignoreNextPlayAttempt = false;
        public static void PlayAudioToSpecificDevice(string audioFilePath, int deviceNumber, bool stopCurrent, float volume,bool rplay, string raudioFilePath, int rdeviceNumber, float rvolume)
        {
            try
            {
                if (stopCurrent && currentOutputDevice != null)
                {
                    currentOutputDevice.Stop();
                    currentOutputDevice.Dispose();
                    currentAudioFile.Dispose();
                    currentOutputDeviceEX.Stop();
                    currentOutputDeviceEX.Dispose();
                    currentAudioFileEX?.Dispose();
                    currentOutputDevice = null;
                    currentAudioFile = null;
                    return;
                }
                else if (currentOutputDevice == null)
                {
                    var audioFile = new AudioFileReader(audioFilePath);
                    var outputDevice = new WaveOutEvent { DeviceNumber = deviceNumber };
                    outputDevice.Volume = Math.Max(0, Math.Min(1, volume));
                    int totalMilliseconds = (int)audioFile.TotalTime.TotalMilliseconds;
                    if(rplay)
                    {
                        PlayAudioex(raudioFilePath,rdeviceNumber,rvolume);
                        ignoreNextPlayAttempt = false;
                    }
                    outputDevice.PlaybackStopped += (sender, e) =>
                    {
                        outputDevice.Dispose();
                        audioFile.Dispose();
                        ignoreNextPlayAttempt = false;
                        currentOutputDevice = null;
                        currentAudioFile = null;
                    };
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    ignoreNextPlayAttempt = false;
                    currentOutputDevice = outputDevice;
                    currentAudioFile = audioFile;
                }
                ignoreNextPlayAttempt = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放音频时出错: {ex.Message}");
            }
            ignoreNextPlayAttempt = false;
        }
        public static void PlayAudioex(string audioFilePath, int deviceNumber, float volume)
        {
            try
            {
                if (currentOutputDeviceEX != null)
                {
                    currentOutputDeviceEX.Stop();
                    currentOutputDeviceEX.Dispose();
                    currentAudioFileEX?.Dispose();
                }
                var audioFile = new AudioFileReader(audioFilePath);
                var outputDevice = new WaveOutEvent { DeviceNumber = deviceNumber };
                // 应用音量设置
                outputDevice.Volume = volume; // 确保 volume 值在 0 到 1 之间
                outputDevice.PlaybackStopped += (sender, e) =>
                {
                    outputDevice.Dispose();
                    audioFile.Dispose();
                };
                outputDevice.Init(audioFile);
                outputDevice.Play();
                currentOutputDeviceEX = outputDevice;
                currentAudioFileEX = audioFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放音频时出错: {ex.Message}", "错误");
            }
        }
        public static void Delay(int time)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < time)
            {
                Application.DoEvents();
            }
        }
        public static void AddAudioFilesToListView(string directoryPath, ListView listView)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            FileInfo[] files = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                                       .Where(f => f.Extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase) ||
                                                   f.Extension.Equals(".wav", StringComparison.OrdinalIgnoreCase)||
                                                   f.Extension.Equals(".falc", StringComparison.OrdinalIgnoreCase))
                                       .ToArray();
            foreach (FileInfo file in files)
            {
                var fileTag = TagLib.File.Create(file.FullName);
                // 尝试读取音频文件的标题作为曲目信息
                string track = string.IsNullOrWhiteSpace(fileTag.Tag.Title) ? "无" : fileTag.Tag.Title;
                string fileName = Path.GetFileNameWithoutExtension(file.Name); // 获取不带扩展名的文件名
                string fileType = file.Extension.TrimStart('.').ToUpper(); // 获取文件类型
                ListViewItem item = new ListViewItem(new[] { fileName, track, fileType });
                item.Tag = file.FullName; // 将文件的完整路径存储在Tag属性中
                listView.Items.Add(item);
            }
        }
        public static string GetKeyDisplay(KeyEventArgs keyEventArgs = null, KeyPressEventArgs keyPressEventArgs = null)
        {
            // 从 KeyEventArgs 提取 keyCode
            if (keyEventArgs != null)
            {
                Keys keyCode = keyEventArgs.KeyCode;
                switch (keyCode)
                {
                    case Keys.Space:
                        return "SPACE";
                    case Keys.Back:
                        return "BACKSPACE";
                    case Keys.Delete:
                        return "DELETE";
                    case Keys.Home:
                        return "HOME";
                    case Keys.End:
                        return "END";
                    case Keys.PageUp:
                        return "PAGE UP";
                    case Keys.PageDown:
                        return "PAGE DOWN";
                    case Keys.Escape:
                        return "ESCAPE";
                    // 处理 F1-F12
                    case Keys.F1:
                    case Keys.F2:
                    case Keys.F3:
                    case Keys.F4:
                    case Keys.F5:
                    case Keys.F6:
                    case Keys.F7:
                    case Keys.F8:
                    case Keys.F9:
                    case Keys.F10:
                    case Keys.F11:
                    case Keys.F12:
                        return keyCode.ToString();
                }
            }
            if (keyPressEventArgs != null)
            {
                char keyChar = keyPressEventArgs.KeyChar;
                return keyChar.ToString().ToUpper();
            }
            return string.Empty;
        }
        private static WaveInEvent waveIn;
        private static WaveOutEvent waveOut;
        private static BufferedWaveProvider bufferedWaveProvider;
        private static WaveOutEvent currentOutputDeviceEX;
        private static AudioFileReader currentAudioFileEX;
        public static void StartMicrophoneToSpeaker(int inputDeviceIndex, int outputDeviceIndex)
        {
            StopMicrophoneToSpeaker(); // 停止之前的流
            waveIn = new WaveInEvent
            {
                DeviceNumber = inputDeviceIndex,
                WaveFormat = new WaveFormat(44100, 16, 1) // 设置为44100Hz, 16位, 单声道
            };
            waveOut = new WaveOutEvent
            {
                DeviceNumber = outputDeviceIndex
            };
            bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                DiscardOnBufferOverflow = true
            };
            waveOut.Init(bufferedWaveProvider);
            waveIn.DataAvailable += (sender, e) =>
            {
                bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            };
            waveIn.StartRecording();
            waveOut.Play();
        }
        public static void StopMicrophoneToSpeaker()
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveOut?.Stop();
            waveOut?.Dispose();
            bufferedWaveProvider = null;
        }
        public static int NCMConvert(string filepath, string outputpath = "default")
        {
            NeteaseCrypt neteaseCrypt = new NeteaseCrypt(filepath);
            int result = neteaseCrypt.Dump();
            neteaseCrypt.FixMetadata();
            if (outputpath != "default")
            {
                string fileSuffix;
                if (File.Exists(filepath.Replace("ncm", "flac")))
                {
                    fileSuffix = "flac";
                }
                else
                {
                    fileSuffix = "mp3";
                }
                string sourceFileName = filepath.Replace("ncm", fileSuffix);
                try
                {
                    File.Copy(sourceFileName, AppDomain.CurrentDomain.BaseDirectory +"AudioData\\"+ Path.GetFileNameWithoutExtension(outputpath+ ".mp3"), true);
                }
                catch (IOException e)
                {
                    MessageBox.Show($"错误：{e.Message}");
                }
            }
            return result;
        }

        public static void GetDownloadCards(string url, ListBox nameListBox, ListBox linkListBox)
        {
            string htmlContent;
            WebClient client = new WebClient();
            try
            {
                htmlContent = client.DownloadString(url);
            }
            catch (Exception e)
            {
                MessageBox.Show($"加载错误，请检查您的网络是否可以正常链接至<slam.scmd.cc>。在重试之前请确保您已关闭VPN或网络代理工具\r\n具体错误信息:{e.ToString()}","错误");
                return;
            }
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlContent);
            HtmlNodeCollection cardNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'download-card')]");
            if (cardNodes != null)
            {
                foreach (HtmlNode cardNode in cardNodes)
                {
                    string title = GetInnerText(cardNode.InnerHtml, "<h2>", "</h2>").Trim();
                    string downloadUrl = GetAttribute(cardNode.InnerHtml, "href=\"(.*?)\"", "href");
                    nameListBox.Items.Add(title);
                    linkListBox.Items.Add(url + downloadUrl);
                }
            }
        }


        private static string GetInnerText(string input, string startTag, string endTag)
        {
            int startIndex = input.IndexOf(startTag) + startTag.Length;
            int endIndex = input.IndexOf(endTag, startIndex);
            return input.Substring(startIndex, endIndex - startIndex);
        }

        private static string GetAttribute(string input, string pattern, string attributeName)
        {
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);
            return match.Groups[1].Value;
        }
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public static void ButtonStabilization(int time,Button button)
        {
            var ori = button.Text;
            button.Enabled = false;
            for (int i = time; i != 0; i--) { button.Text = i.ToString(); Misc.Delay(1000); }
            button.Enabled = true;
            button.Text = ori;
        }
        public static async Task FadeIn(int durationMilliseconds, Form form)
        {
            form.Visible = false;
            form.Opacity = 0;
            form.Show();

            for (double opacity = 0; opacity <= 1; opacity += 0.05)
            {
                form.Opacity = opacity;
                await Task.Delay(durationMilliseconds / 20);
            }

            form.Opacity = 1;
        }

        public static async Task FadeOut(int durationMilliseconds, Form form)
        {
            for (double opacity = 1; opacity >= 0; opacity -= 0.05)
            {
                form.Opacity = opacity;
                await Task.Delay(durationMilliseconds / 20);
            }
            form.Visible = false;
            form.Dispose();
        }
        public void ApplyResourcesToControls(Control.ControlCollection controls, string baseName, Assembly assembly)
        {
            ResourceManager rm = new ResourceManager(baseName, assembly);
            foreach (Control control in controls)
            {
                if (control.HasChildren)
                {
                    ApplyResourcesToControls(control.Controls, baseName, assembly);
                }
                string key = $"{control.Name}ResxText";
                string resourceValue = rm.GetString(key);
                if (!string.IsNullOrEmpty(resourceValue))
                {
                    control.Text = resourceValue;
                }
            }
        }
        public static void BuildLocalizationBaseFiles(Control.ControlCollection controls, string filePath)
        {
            //该函数用于生成标准本地化文件 基文件为简体中文
            StringBuilder resxContent = new StringBuilder();
            // 添加.resx文件必要的头部
            resxContent.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            resxContent.AppendLine("<root>");
            resxContent.AppendLine("  <!-- 简化的头部信息 -->");
            resxContent.AppendLine("  <resheader name=\"resmimetype\">");
            resxContent.AppendLine("    <value>text/microsoft-resx</value>");
            resxContent.AppendLine("  </resheader>");
            resxContent.AppendLine("  <resheader name=\"version\">");
            resxContent.AppendLine("    <value>2.0</value>");
            resxContent.AppendLine("  </resheader>");
            resxContent.AppendLine("  <resheader name=\"reader\">");
            resxContent.AppendLine("    <value>System.Resources.ResXResourceReader, System.Windows.Forms, ...</value>");
            resxContent.AppendLine("  </resheader>");
            resxContent.AppendLine("  <resheader name=\"writer\">");
            resxContent.AppendLine("    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, ...</value>");
            resxContent.AppendLine("  </resheader>");
            // 为控件生成<data>元素
            resxContent.Append(BuildControlToXMLDataValue(controls));
            // 添加文件尾部
            resxContent.AppendLine("</root>");
            // 保存到文件
            File.WriteAllText(filePath, resxContent.ToString());
        }
        private static string BuildControlToXMLDataValue(Control.ControlCollection controls)
        {
            StringBuilder resxEntries = new StringBuilder();
            foreach (Control control in controls)
            {
                if (control.HasChildren)
                {
                    resxEntries.Append(BuildControlToXMLDataValue(control.Controls));
                }
                if (!string.IsNullOrEmpty(control.Name) && !string.IsNullOrEmpty(control.Text))
                {
                    string escapedControlText = control.Text
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;");
                    resxEntries.AppendLine($"  <data name=\"{control.Name}ResxText\" xml:space=\"preserve\">");
                    resxEntries.AppendLine($"    <value>{escapedControlText}</value>");
                    resxEntries.AppendLine($"  </data>");
                }
            }
            return resxEntries.ToString();
        }


        private const string RepoOwner = "TheD0ubleC"; // GitHub用户名
        private const string RepoName = "MusicalMoments"; // GitHub仓库名

        public static async Task<string> GetLatestVersionAsync()
        {
            HttpResponseMessage response = null;
            string latestVersion = string.Empty;

            string releasesUrl = $"https://api.kkgithub.com/repos/{RepoOwner}/{RepoName}/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# App");
                try { response = await client.GetAsync(releasesUrl);}catch (Exception ex) { return MainWindow.nowVer; }

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    latestVersion = ParseLatestVersion(json);
                }
            }

            return latestVersion;
        }
        public static async Task<string> GetLatestVersionTipsAsync()
        {
            HttpResponseMessage response = null;
            string latestVersion = string.Empty;

            string releasesUrl = $"https://api.kkgithub.com/repos/{RepoOwner}/{RepoName}/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# App");
                try { response = await client.GetAsync(releasesUrl); } catch (Exception ex) { return MainWindow.nowVer; }

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    latestVersion = ParseLatestVersionTips(json);
                }
            }

            return latestVersion;
        }

        private static string ParseLatestVersion(string json)
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("tag_name", out JsonElement tagElement))
                {
                    return tagElement.GetString();
                }
            }

            return string.Empty;
        }

        private static string ParseLatestVersionTips(string json)
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("body", out JsonElement tagElement))
                {
                    return tagElement.GetString();
                }
            }

            return string.Empty;
        }


    }
}
