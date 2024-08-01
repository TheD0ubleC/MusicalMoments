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
using Newtonsoft.Json;
using System.Globalization;
namespace MusicalMoments
{
    public class AudioInfo
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public Keys Key { get; set; }
    }



    internal class Misc
    {
        public static WaveOutEvent currentOutputDevice = null;
        public static AudioFileReader currentAudioFile = null;
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
                    var outputDevice = new WaveOutEvent { DeviceNumber = deviceNumber, DesiredLatency = 50 };
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
                var outputDevice = new WaveOutEvent { DeviceNumber = deviceNumber, DesiredLatency = 50 };
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
        public static async Task AddAudioFilesToListView(string directoryPath, ListView listView)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            FileInfo[] audioFiles = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                                            .Where(f => f.Extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase) ||
                                                        f.Extension.Equals(".wav", StringComparison.OrdinalIgnoreCase) ||
                                                        f.Extension.Equals(".falc", StringComparison.OrdinalIgnoreCase))
                                            .ToArray();
            foreach (FileInfo audioFile in audioFiles)
            {
                // 获取音频文件信息
                var fileTag = TagLib.File.Create(audioFile.FullName);
                string fileName = Path.GetFileNameWithoutExtension(audioFile.Name);
                string track = string.IsNullOrWhiteSpace(fileTag.Tag.Title) ? "无" : fileTag.Tag.Title;
                string fileType = audioFile.Extension.TrimStart('.').ToUpper();

                // 获取 JSON 文件路径并读取信息
                string jsonFilePath = Path.ChangeExtension(audioFile.FullName, ".json");
                string jsonInfo = ReadKeyJsonInfo(jsonFilePath);

                // 将文件信息添加到 ListView 中
                ListViewItem item = new ListViewItem(new[] { fileName, track, fileType, jsonInfo });
                item.Tag = audioFile.FullName;
                listView.Items.Add(item);
            }
        }


        private static string ReadKeyJsonInfo(string jsonFilePath)
        {
            string info = "未绑定";
            try
            {
                // 检查 JSON 文件是否存在
                if (File.Exists(jsonFilePath))
                {
                    // 读取 JSON 文件内容并尝试解析
                    string jsonContent = File.ReadAllText(jsonFilePath);
                    dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent);

                    // 如果成功解析，则尝试读取 "Key" 值
                    if (jsonObject != null && jsonObject["Key"] != null)
                    {
                        string key = jsonObject["Key"].ToString();

                        // 如果 "Key" 值为空字符串，则将 info 设置为 "未绑定"
                        if (string.IsNullOrEmpty(key) || key == "None")
                        {
                            info = "未绑定";
                        }
                        else
                        {
                            info = key;
                        }
                    }
                    else
                    {
                        // 如果没有 "Key" 值，则添加一个空的 "Key" 值并返回 "未绑定"
                        jsonObject = new Newtonsoft.Json.Linq.JObject();
                        jsonObject["Key"] = "";
                        File.WriteAllText(jsonFilePath, jsonObject.ToString());
                    }
                }
                else
                {
                    // 如果 JSON 文件不存在，则创建一个并添加一个空的 "Key" 值并返回 "未绑定"
                    File.WriteAllText(jsonFilePath, "{\"Key\":\"\"}");
                }
            }
            catch{}
            return info;
        }
        public static void WriteKeyJsonInfo(string jsonFilePath, string valueToWrite)
        {
            try
            {
                // 检查 JSON 文件是否存在
                if (File.Exists(jsonFilePath))
                {
                    // 读取 JSON 文件内容
                    string jsonContent = File.ReadAllText(jsonFilePath);
                    dynamic jsonObject = JsonConvert.DeserializeObject(jsonContent);

                    // 更新 "Key" 值
                    jsonObject["Key"] = valueToWrite;

                    // 将更新后的 JSON 重新写入文件
                    File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(jsonObject));
                }
                else
                {
                    // 创建一个新的 JSON 对象并设置 "Key" 值
                    var jsonObject = new JObject();
                    jsonObject["Key"] = valueToWrite;

                    // 将 JSON 对象写入文件
                    File.WriteAllText(jsonFilePath, jsonObject.ToString());
                }
            }
            catch {}
        }




        public static void AddPluginFilesToListView(string rootDirectoryPath, ListView listView)
        {
            // 清空列表视图中的项
            listView.Items.Clear();

            // 获取根目录下的所有文件夹
            string[] subDirectories = Directory.GetDirectories(rootDirectoryPath);
            foreach (string subDirectory in subDirectories)
            {
                string directoryName = new DirectoryInfo(subDirectory).Name;
                string exeFilePath = Path.Combine(subDirectory, directoryName + ".exe");
                string jsonFilePath = Path.Combine(subDirectory, directoryName + ".json");
                if (File.Exists(exeFilePath) && File.Exists(jsonFilePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(jsonFilePath);
                        PluginSDK.PluginInfo pluginInfo = JsonConvert.DeserializeObject<PluginSDK.PluginInfo>(jsonContent);
                        if (pluginInfo != null)
                        {
                            ListViewItem item = new ListViewItem(new[] { directoryName, pluginInfo.PluginDescription, pluginInfo.PluginVersion });
                            item.Tag = exeFilePath;
                            listView.Items.Add(item);
                        }
                        else
                        {
                            ListViewItem item = new ListViewItem(new[] { directoryName, "无描述", "无描述" });
                            item.Tag = exeFilePath;
                            listView.Items.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        ListViewItem item = new ListViewItem(new[] { directoryName, "无描述", "无描述" });
                        item.Tag = exeFilePath;
                        listView.Items.Add(item);
                    }
                }
                else
                {
                    ListViewItem item = new ListViewItem(new[] { directoryName, "无描述", "无描述" });
                    item.Tag = exeFilePath;
                    listView.Items.Add(item);
                }
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
                        return "None";
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
                    // 处理数字键
                    case Keys.D0:
                    case Keys.D1:
                    case Keys.D2:
                    case Keys.D3:
                    case Keys.D4:
                    case Keys.D5:
                    case Keys.D6:
                    case Keys.D7:
                    case Keys.D8:
                    case Keys.D9:
                        return keyCode.ToString().Substring(1);
                    // 特殊的符号？
                    case Keys.Oemcomma:
                        return ",";
                    case Keys.OemPeriod:
                        return ".";
                    case Keys.OemSemicolon:
                        return ";";
                    case Keys.OemQuotes:
                        return "'";
                    case Keys.OemOpenBrackets:
                        return "[";
                    case Keys.OemCloseBrackets:
                        return "]";
                    case Keys.OemPipe:
                        return "\\";
                    case Keys.OemMinus:
                        return "-";
                    case Keys.Oemplus:
                        return "=";
                    case Keys.Oemtilde:
                        return "`";
                    case Keys.OemQuestion:
                        return "/";
                    case Keys.OemBackslash:
                        return "\\";
                    // 添加小键盘按键
                    case Keys.NumPad0:
                        return "NUMPAD 0";
                    case Keys.NumPad1:
                        return "NUMPAD 1";
                    case Keys.NumPad2:
                        return "NUMPAD 2";
                    case Keys.NumPad3:
                        return "NUMPAD 3";
                    case Keys.NumPad4:
                        return "NUMPAD 4";
                    case Keys.NumPad5:
                        return "NUMPAD 5";
                    case Keys.NumPad6:
                        return "NUMPAD 6";
                    case Keys.NumPad7:
                        return "NUMPAD 7";
                    case Keys.NumPad8:
                        return "NUMPAD 8";
                    case Keys.NumPad9:
                        return "NUMPAD 9";
                    case Keys.Multiply:
                        return "NUMPAD *";
                    case Keys.Add:
                        return "NUMPAD +";
                    case Keys.Subtract:
                        return "NUMPAD -";
                    case Keys.Decimal:
                        return "NUMPAD .";
                    case Keys.Divide:
                        return "NUMPAD /";
                    case Keys.NumLock:
                        return "NUM LOCK";
                    case Keys.Scroll:
                        return "SCROLL LOCK";
                    // 其他不常用按键
                    case Keys.Pause:
                        return "PAUSE/BREAK";
                    case Keys.Insert:
                        return "INSERT";
                    case Keys.PrintScreen:
                        return "PRINT SCREEN";
                    case Keys.CapsLock:
                        return "CAPS LOCK";
                    case Keys.LWin:
                    case Keys.RWin:
                        return "WINDOWS";
                    case Keys.Apps:
                        return "APPLICATION";
                    case Keys.Tab:
                        return "TAB";
                    case Keys.Enter:
                        return "ENTER";
                    case Keys.ShiftKey:
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        return "SHIFT";
                    case Keys.ControlKey:
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        return "CONTROL";
                    case Keys.Alt:
                    case Keys.Menu:
                        return "ALT";
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
                WaveFormat = new WaveFormat(192000, 16, 2) // 设置为192000Hz, 16位, 2声道
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

        public static string DisplayAudioProperties(string audioFilePath)
        {
            try
            {
                using (var audioFile = new AudioFileReader(audioFilePath))
                {
                    string frequency = audioFile.WaveFormat.SampleRate.ToString();
                    string bits = (audioFile.WaveFormat.BitsPerSample / 8).ToString(); // 位数
                    string channels = audioFile.WaveFormat.Channels.ToString(); // 声道
                    string format = Path.GetExtension(audioFilePath).TrimStart('.'); // 格式

                    // 将音频属性显示在标签上
                    return $"格式: {format.ToUpper()}, 频率: {frequency} Hz, 位数: {bits} bit, 声道: {channels}";
                }
            }
            catch
            {
                return "无法读取音频属性";
            }
        }

        private static readonly HttpClient client = new HttpClient();

        public static async Task APIStartup()
        {
            try
            {
                // 生成一个唯一标识符（GUID）
                string clientId = Guid.NewGuid().ToString();

                // 添加唯一标识符到请求头
                client.DefaultRequestHeaders.Add("X-Client-ID", clientId);

                string url = "https://api.scmd.cc/MM.php?action=startup";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // 确保响应成功

                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody); // 输出响应内容
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HTTP 请求错误: {e.Message}");
            }
        }

        public static async Task APIShutdown()
        {
            try
            {
                string url = "https://api.scmd.cc/MM.php?action=shutdown";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // 确保响应成功

                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody); // 输出响应内容
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HTTP 请求错误: {e.Message}");
            }
        }

        public static int CalculateDaysBetweenDates(string dateStr1, string dateStr2)
        {
            DateTime date1, date2;
            string[] formats = {
            "yyyy年MM月dd日HH时mm分ss秒",
            "yyyy年MM月dd日HH时mm分",
            "yyyy年MM月dd日HH时",
            "yyyy年MM月dd日",
            "yyyy年MM月dd",
            "yyyy年MM月",
            "yyyy年MM",
            "yyyy年"
        };
            dateStr1 = dateStr1.Replace(" ", "");
            dateStr2 = dateStr2.Replace(" ", "");
            if (!DateTime.TryParseExact(dateStr1, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date1))
            {
                throw new ArgumentException("Invalid date format for the first date.");
            }
            if (!DateTime.TryParseExact(dateStr2, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date2))
            {
                throw new ArgumentException("Invalid date format for the second date.");
            }
            return Math.Abs((date2 - date1).Days);
        }
    }
}
