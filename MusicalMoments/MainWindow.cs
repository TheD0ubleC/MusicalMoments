using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using NAudio.Wave;
using Gma.System.MouseKeyHook;
using System.Reflection;
using System.Text;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Security.Policy;
using NAudio.Gui;
using TagLib.Mpeg;

using File = System.IO.File;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.VisualBasic.Devices;
using System.IO;
using System.Net.Mail;
using Newtonsoft.Json.Linq;

namespace MusicalMoments
{
    public partial class MainWindow : Form
    {
        public static string nowVer = "v1.4.1-release-x64";
        public static string runningDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static Keys toggleStreamKey;
        public static Keys playAudioKey;
        public static string selectedAudioPath;
        public static string selectedPluginPath;
        public static int closeCount = 0;
        public static int playedCount = 0;
        public static int changedCount = 0;
        public static string firstStart = System.DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒");
        public static bool playAudio = true;
        public static IKeyboardMouseEvents m_GlobalHook;
        public static bool isPlaying = false;
        public static float VBvolume = 1f;
        public static float volume = 1f;
        public static float tipsvolume = 1f;

        public static int VBInputComboSelect = 0;
        public static int VBOutputComboSelect = 0;
        public static int InputComboSelect = 0;
        public static int OutputComboSelect = 0;
        public static bool AudioEquipmentPlayCheck = true;

        public static List<AudioInfo> audioInfo = new List<AudioInfo>();
        public MainWindow()
        {
            InitializeComponent();
            Subscribe();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }
        public void Subscribe()
        {
            // 订阅全局键盘事件
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyDown += GlobalHookKeyDown;
        }

        private async void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == playAudioKey)
            {
                if (playAudio)
                {
                    if (File.Exists(selectedAudioPath))
                    {
                        if (isPlaying)
                        {
                            Misc.PlayAudioToSpecificDevice(selectedAudioPath, Misc.GetOutputDeviceID(comboBox_VBAudioEquipmentOutput.SelectedItem.ToString()), true, VBvolume, audioEquipmentPlay.Checked, selectedAudioPath, Misc.GetOutputDeviceID(comboBox_AudioEquipmentOutput.SelectedItem.ToString()), volume);
                            isPlaying = true;
                        }
                        else
                        {
                            Misc.PlayAudioToSpecificDevice(selectedAudioPath, Misc.GetOutputDeviceID(comboBox_VBAudioEquipmentOutput.SelectedItem.ToString()), true, VBvolume, audioEquipmentPlay.Checked, selectedAudioPath, Misc.GetOutputDeviceID(comboBox_AudioEquipmentOutput.SelectedItem.ToString()), volume);
                            isPlaying = false;
                        }
                    }
                }
                playedCount = playedCount + 1;
            }
            // 检查是否按下了切换流的按键
            if (e.KeyCode == toggleStreamKey)
            {
                if (switchStreamTips.Checked)
                {
                    playAudio = !playAudio;
                    string audioFilePath = playAudio
                        ? Path.Combine(runningDirectory, @"ResourceFiles\切换为音频.wav")
                        : Path.Combine(runningDirectory, @"ResourceFiles\切换为麦克风.wav");
                    PlayAudioex(audioFilePath, Misc.GetOutputDeviceID(comboBox_AudioEquipmentOutput.SelectedItem.ToString()), tipsvolume);
                    if (!playAudio)
                    {
                        if (File.Exists(selectedAudioPath))
                        {
                            if (isPlaying)
                            {
                                Misc.PlayAudioToSpecificDevice(selectedAudioPath, Misc.GetOutputDeviceID(comboBox_VBAudioEquipmentOutput.SelectedItem.ToString()), true, VBvolume, audioEquipmentPlay.Checked, selectedAudioPath, Misc.GetOutputDeviceID(comboBox_AudioEquipmentOutput.SelectedItem.ToString()), volume);
                            }
                        }
                        Misc.StartMicrophoneToSpeaker(Misc.GetInputDeviceID(comboBox_AudioEquipmentInput.SelectedItem.ToString()), Misc.GetOutputDeviceID(comboBox_VBAudioEquipmentOutput.SelectedItem.ToString()));
                    }
                    else
                    {
                        Misc.StopMicrophoneToSpeaker();
                    }
                }
                changedCount = changedCount + 1;
            }

            foreach (var audio in audioInfo)
            {
                if (e.KeyCode == audio.Key)
                {
                    if (playAudio)
                    {
                        if (File.Exists(audio.FilePath))
                        {
                            if (isPlaying)
                            {
                                Misc.PlayAudioToSpecificDevice(audio.FilePath, Misc.GetOutputDeviceID(comboBox_VBAudioEquipmentOutput.SelectedItem.ToString()), true, VBvolume, audioEquipmentPlay.Checked, audio.FilePath, Misc.GetOutputDeviceID(comboBox_AudioEquipmentOutput.SelectedItem.ToString()), volume);
                                isPlaying = true;
                            }
                            else
                            {
                                Misc.PlayAudioToSpecificDevice(audio.FilePath, Misc.GetOutputDeviceID(comboBox_VBAudioEquipmentOutput.SelectedItem.ToString()), true, VBvolume, audioEquipmentPlay.Checked, audio.FilePath, Misc.GetOutputDeviceID(comboBox_AudioEquipmentOutput.SelectedItem.ToString()), volume);
                                isPlaying = false;
                            }
                        }
                    }

                }
            }
        }
        private void sideLists_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawBackground();
            // 绘制图片
            if (e.Item.ImageIndex >= 0)
            {
                e.Graphics.DrawImage(sideLists.SmallImageList.Images[e.Item.ImageIndex], e.Bounds.Left, e.Bounds.Top);
            }
            // 设置文本颜色为灰色
            Brush textBrush = new SolidBrush(Color.FromArgb(85, 85, 85));
            // 绘制文本
            e.Graphics.DrawString(e.Item.Text, e.Item.Font, textBrush, e.Bounds.Left + 40, e.Bounds.Top);
        }
        private async void MainWindow_Load(object sender, EventArgs e)
        {
            Visible = false;
            foreach (TabPage tabPage in mainTabControl.TabPages)
            {
                mainTabControl.SelectTab(tabPage);
                //我有强迫症 所以防止切换选项卡的时候有卡顿的加载就在启动前先都切一遍:D
            }
            /*
            用于构建本地化基文件(感觉用不上这东西 别说海外用户了 能有几个国内用户用我就心满意足了TT)
            BuildLocalizationBaseFiles(this.Controls, $"{runningDirectory}Resources.zh-CN.resx");
            */
            Misc.FadeIn(200, this);

            mainTabControl.ItemSize = new System.Drawing.Size(0, 1);
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioData"));//创建存放音频的文件夹
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugin"));//创建存放插件的文件夹
            await Misc.AddAudioFilesToListView(runningDirectory + @"\AudioData\", audioListView);
            Misc.AddPluginFilesToListView(runningDirectory + @"\Plugin\", pluginListView);
            if (!Misc.IsAdministrator()) { Text += " [当前非管理员运行,可能会出现按下按键无反应]"; }
            else { Text += " MusicalMoments"; }


            label_VBStatus.Text = Misc.checkVB() ? "VB声卡已安装" : "VB声卡未安装";
            comboBoxOutputFormat.SelectedIndex = 0;

            foreach (string device in Misc.GetInputAudioDeviceNames())
            {
                comboBox_VBAudioEquipmentInput.Items.Add(device);
                comboBox_AudioEquipmentInput.Items.Add(device);
            }
            foreach (string device in Misc.GetOutputAudioDeviceNames())
            {
                comboBox_VBAudioEquipmentOutput.Items.Add(device);
                comboBox_AudioEquipmentOutput.Items.Add(device);
            }
            comboBox_AudioEquipmentInput.SelectedIndex = 0;
            comboBox_AudioEquipmentOutput.SelectedIndex = 0;
            comboBox_VBAudioEquipmentInput.SelectedIndex = 0;
            comboBox_VBAudioEquipmentOutput.SelectedIndex = 0;
            VBVolumeTrackBar_Scroll(null, null);
            VolumeTrackBar_Scroll(null, null);
            TipsVolumeTrackBar_Scroll(null, null);
            numberLabel.Text = "";


            LoadUserData();
            if (CheckDuplicateKeys()) { MessageBox.Show($"已检测到相同按键 请勿作死将两个或多个音频绑定在同个按键上 该操作可能会导致MM崩溃 此提示会在绑定按键时与软件启动时检测并发出", "温馨提示"); }
            Misc.APIStartup();
            //最后再版本验证 以防UI错误
            CheckNewVer();

        }

        private void LoadUserData(bool changeToAudioPage = true)
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userSettings.json");
            if (File.Exists(configPath))
            {
                if (changeToAudioPage)
                {
                    mainTabControl.SelectedIndex = 1;
                    mainGroupBox.Text = "音频";
                }

                try
                {
                    string json = File.ReadAllText(configPath);
                    var settings = JsonConvert.DeserializeObject<UserSettings>(json);
                    VBInputComboSelect = comboBox_VBAudioEquipmentInput.SelectedIndex;
                    VBOutputComboSelect = comboBox_AudioEquipmentInput.SelectedIndex;
                    InputComboSelect = comboBox_VBAudioEquipmentOutput.SelectedIndex;
                    OutputComboSelect = comboBox_AudioEquipmentOutput.SelectedIndex;
                    AudioEquipmentPlayCheck = audioEquipmentPlay.Checked;

                    comboBox_VBAudioEquipmentInput.SelectedIndex = settings.VBAudioEquipmentInputIndex >= comboBox_VBAudioEquipmentInput.Items.Count ? 0 : settings.VBAudioEquipmentInputIndex;
                    comboBox_AudioEquipmentInput.SelectedIndex = settings.AudioEquipmentInputIndex >= comboBox_AudioEquipmentInput.Items.Count ? 0 : settings.AudioEquipmentInputIndex;
                    comboBox_VBAudioEquipmentOutput.SelectedIndex = settings.VBAudioEquipmentOutputIndex >= comboBox_VBAudioEquipmentOutput.Items.Count ? 0 : settings.VBAudioEquipmentOutputIndex;
                    comboBox_AudioEquipmentOutput.SelectedIndex = settings.AudioEquipmentOutputIndex >= comboBox_AudioEquipmentOutput.Items.Count ? 0 : settings.AudioEquipmentOutputIndex;
                    if (!string.IsNullOrEmpty(settings.ToggleStreamKey))
                    {
                        Keys key;
                        if (Enum.TryParse(settings.ToggleStreamKey, out key))
                        {
                            toggleStreamKey = key;
                            ToggleStream.Text = key.ToString();
                        }
                    }
                    if (!string.IsNullOrEmpty(settings.PlayAudioKey))
                    {
                        Keys key;
                        if (Enum.TryParse(settings.PlayAudioKey, out key))
                        {
                            playAudioKey = key;
                            PlayAudio.Text = key.ToString();
                        }
                    }
                    if (settings.CloseCount == 35)
                    {
                        MessageBox.Show("看来你已经启动过了35次软件呢！究竟是因为太喜欢放音频还是因为我的代码BUG太多一直闪退捏？！？(ps:算是小彩蛋吧？看到了可以发我哟 毕竟我也想知道是谁这么喜欢放音频 或者是帮我找BUG?后面还有彩蛋哦 多打开几次吧~)", "你好呀");
                    }
                    else if (settings.CloseCount == 50)
                    {
                        MessageBox.Show("看来你已经启动过了50次软件呢！这次我敢确定绝对不是我代码BUG多！！因为BUG多一直闪退就代表我的软件不好用！之后用户就会换软件了！！！(ps:当启动次数有80次后会有一个大彩蛋呢~)", "你好呀");
                    }
                    else if (settings.CloseCount == 80)
                    {
                        MessageBox.Show("这么快就80次了吗？！？那就告诉你大彩蛋的位置吧~首先呢要去关于页面 然后连续戳20次LOGO图片就有了 那就这样 拜啦 后面也不会有彩蛋了~", "你好呀");
                    }
                    VBVolumeTrackBar.Value = (int)(settings.VBVolume * 100);
                    VolumeTrackBar.Value = (int)(settings.Volume * 100);
                    TipsVolumeTrackBar.Value = (int)(settings.TipsVolume * 100);
                    VBVolumeTrackBar_Scroll(null, null);
                    VolumeTrackBar_Scroll(null, null);
                    TipsVolumeTrackBar_Scroll(null, null);
                    audioEquipmentPlay.Checked = settings.AudioEquipmentPlay;
                    switchStreamTips.Checked = settings.SwitchStreamTips;
                    closeCount = settings.CloseCount;
                    playedCount = settings.PlayedCount;
                    changedCount = settings.ChangedCount;
                    firstStart = settings.FirstStart;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取配置时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                mainTabControl.SelectedIndex = 0;
                comboBox_AudioEquipmentInput.SelectedIndex = 0;
                comboBox_AudioEquipmentOutput.SelectedIndex = 0;
                comboBox_VBAudioEquipmentInput.SelectedIndex = 0;
                comboBox_VBAudioEquipmentOutput.SelectedIndex = 0;
                VBVolumeTrackBar_Scroll(null, null);
                VolumeTrackBar_Scroll(null, null);
                TipsVolumeTrackBar_Scroll(null, null);
                firstStart = System.DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒");
            }
        }
        private async void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveUserData(true);

            e.Cancel = true;
            Misc.FadeOut(200, this);
            Misc.APIShutdown();
        }

        private void SaveUserData(bool addCloseCount = false)
        {
            var settings = new UserSettings
            {
                VBAudioEquipmentInputIndex = comboBox_VBAudioEquipmentInput.SelectedIndex,
                AudioEquipmentInputIndex = comboBox_AudioEquipmentInput.SelectedIndex,
                VBAudioEquipmentOutputIndex = comboBox_VBAudioEquipmentOutput.SelectedIndex,
                AudioEquipmentOutputIndex = comboBox_AudioEquipmentOutput.SelectedIndex,
                ToggleStreamKey = toggleStreamKey.ToString(),
                PlayAudioKey = playAudioKey.ToString(),
                AudioEquipmentPlay = audioEquipmentPlay.Checked,
                SwitchStreamTips = switchStreamTips.Checked,
                VBVolume = VBVolumeTrackBar.Value / 100f,
                Volume = VolumeTrackBar.Value / 100f,
                TipsVolume = TipsVolumeTrackBar.Value / 100f,

                CloseCount = closeCount + (addCloseCount ? 1 : 0),
                PlayedCount = playedCount,
                ChangedCount = changedCount,
                FirstStart = firstStart,
            };
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(Path.Combine(runningDirectory, "userSettings.json"), json);
        }




        private async void CheckNewVer(bool noUpdateShowMessage = false)
        {
            var newVerTips = await Misc.GetLatestVersionTipsAsync();
            var latestVer = await Misc.GetLatestVersionAsync();
            if (latestVer != nowVer)
            {
                DialogResult dialogResult = MessageBox.Show($"Musical Moments存在新版本，请尽快更新。当前版本为{nowVer} 最新版本为{latestVer}。\r\n按下是则自动下载最新版本压缩包 按下否则跳转至最新版本页面 按下取消则关闭\r\n\r\n以下是新版本简介:\r\n{newVerTips}", "新版本推送", MessageBoxButtons.YesNoCancel);

                if (dialogResult == DialogResult.Yes)
                {
                    string[] parts = latestVer.Split('-');
                    string version = parts[0].TrimStart('v'); // 获取版本号部分
                    string downloadUrl = $"https://kkgithub.com/TheD0ubleC/MusicalMoments/releases/download/{latestVer}/MM.Release-{version}.zip";
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFile(downloadUrl, $"{runningDirectory}MM.Release-{version}.zip");
                            MessageBox.Show($"下载成功 已存放至运行目录 即将开始更新 详情路径:{runningDirectory}MM.Release-{version}.zip", "提示");
                            Process currentProcess = Process.GetCurrentProcess();
                            int pid = currentProcess.Id;
                            StartApplication(Path.Combine(runningDirectory, "Updater.exe"), $"{pid} {runningDirectory}MM.Release-{version}.zip {runningDirectory}");
                        }
                    }
                    catch (Exception ex)
                    { MessageBox.Show($"下载失败，请至github页面自行下载或在群文件下载 错误详情:{ex.ToString()}", "错误"); }
                }
                else if (dialogResult == DialogResult.No)
                {
                    Process.Start(new ProcessStartInfo("https://github.com/TheD0ubleC/MusicalMoments/releases/tag/" + latestVer) { UseShellExecute = true });
                }
            }
            if (noUpdateShowMessage)
            { MessageBox.Show($"当前已是最新版本！", "提示"); }
        }
        static void StartApplication(string applicationPath, string arguments)
        {
            try
            {
                if (File.Exists(applicationPath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = applicationPath,
                        Arguments = arguments
                    };
                    Process.Start(startInfo);
                    Console.WriteLine($"Started application: {applicationPath} with arguments: {arguments}");
                }
                else
                {
                    Console.WriteLine($"Application not found: {applicationPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting application: {ex.Message}");
            }
        }
        private async void sideLists_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveUserData();
            comboBox_VBAudioEquipmentInput.Items.Clear();
            comboBox_AudioEquipmentInput.Items.Clear();
            foreach (string device in Misc.GetInputAudioDeviceNames())
            {
                comboBox_VBAudioEquipmentInput.Items.Add(device);
                comboBox_AudioEquipmentInput.Items.Add(device);
            }
            comboBox_VBAudioEquipmentOutput.Items.Clear();
            comboBox_AudioEquipmentOutput.Items.Clear();
            foreach (string device in Misc.GetOutputAudioDeviceNames())
            {
                comboBox_VBAudioEquipmentOutput.Items.Add(device);
                comboBox_AudioEquipmentOutput.Items.Add(device);
            }
            Misc.AddPluginFilesToListView(runningDirectory + @"\Plugin\", pluginListView);
            foreach (int index in sideLists.SelectedIndices)
            {
                mainTabControl.SelectTab(index);
            }
            foreach (ListViewItem item in sideLists.SelectedItems)
            {
                mainGroupBox.Text = $"{item.Text}";
            }
            AudioListView_fd.Items.Clear();
            if (mainTabControl.SelectedIndex == 1) { reLoadList(); }
            LoadUserData(false);
        }
        private void retestVB_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(Misc.checkVB() ? "vb已安装" : "vb未安装");
            label_VBStatus.Text = Misc.checkVB() ? "VB声卡已安装" : "VB声卡未安装";
        }
        private void audioListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTestInfo = audioListView.HitTest(e.Location);
                if (hitTestInfo.Item != null)
                {
                    // 显示菜单在鼠标点击的位置
                    mainContextMenuStrip.Show(audioListView, e.Location);
                }
            }
        }
        private void 播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selectedItem = audioListView.SelectedItems[0];
            string filePath = selectedItem.Tag as string;
            try
            {
                PlayAudioex(filePath, Misc.GetOutputDeviceID(comboBox_AudioEquipmentOutput.SelectedItem.ToString()), volume);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放音频时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            playedCount = playedCount + 1;
        }
        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (audioListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = audioListView.SelectedItems[0];
                string filePath = selectedItem.Tag as string;
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    DialogResult dialogResult = MessageBox.Show("确定要删除这个文件吗？", "删除文件", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        try
                        {
                            File.Delete(filePath); // 删除文件
                            audioListView.Items.Remove(selectedItem); // 如果文件删除成功，从ListView中移除项
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"删除文件时出错: {ex.Message}");
                        }
                    }
                }
            }
        }
        private void 重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (audioListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = audioListView.SelectedItems[0];
                string originalFilePath = selectedItem.Tag as string;
                if (originalFilePath == null) return;
                string directoryPath = Path.GetDirectoryName(originalFilePath);
                string originalFileName = Path.GetFileName(originalFilePath);
                string currentName = selectedItem.Text;
                string extension = Path.GetExtension(originalFilePath);
                string input = Interaction.InputBox("请输入新的名称：", "重命名", currentName, -1, -1);
                if (string.IsNullOrEmpty(input) || input.Equals(currentName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                string newFileName = input + extension;
                string newFilePath = Path.Combine(directoryPath, newFileName);
                // 检查新命名的文件是否已存在
                if (File.Exists(newFilePath))
                {
                    MessageBox.Show("命名重复", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    try
                    {
                        File.Move(originalFilePath, newFilePath);
                        selectedItem.Text = input;
                        selectedItem.Tag = newFilePath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"重命名文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void ToggleStream_KeyDown(object sender, KeyEventArgs e)
        {
            string displayText = Misc.GetKeyDisplay(keyEventArgs: e);
            if (!string.IsNullOrEmpty(displayText))
            {
                ToggleStream.Text = displayText;
                toggleStreamKey = e.KeyCode;
                ToggleStream.Enabled = false;
                ToggleStream.Enabled = true;
                e.SuppressKeyPress = true;
            }
        }
        private void ToggleStream_KeyPress(object sender, KeyPressEventArgs e)
        {
            string displayText = e.KeyChar.ToString().ToUpper();
            ToggleStream.Text = displayText;
            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z')
            {
                toggleStreamKey = (Keys)e.KeyChar;
            }
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                toggleStreamKey = (Keys)(e.KeyChar - 32);
            }
            e.Handled = true;
        }
        private void PlayAudio_KeyDown(object sender, KeyEventArgs e)
        {
            string displayText = Misc.GetKeyDisplay(keyEventArgs: e);
            if (!string.IsNullOrEmpty(displayText))
            {
                PlayAudio.Text = displayText;
                playAudioKey = e.KeyCode;
                PlayAudio.Enabled = false;
                PlayAudio.Enabled = true;
                e.SuppressKeyPress = true;
            }
        }
        private void PlayAudio_KeyPress(object sender, KeyPressEventArgs e)
        {
            string displayText = e.KeyChar.ToString().ToUpper();
            PlayAudio.Text = displayText;
            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z')
            {
                playAudioKey = (Keys)e.KeyChar;
            }
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                playAudioKey = (Keys)(e.KeyChar - 32);
            }
            e.Handled = true;

        }
        private void info_ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (info_ListBox.Text)
            {
                case "NAudio":
                    info_Label5.Text =
                        "MM使用了NAudio音频处理库。\r\n" +
                        "NAudio遵循Microsoft Public License (Ms-PL)。\r\n" +
                        "版权所有 (c) [NAudio] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "特此向NAudio及其贡献者表示感谢。";
                    break;
                case "Newtonsoft.Json":
                    info_Label5.Text =
                        "MM使用了Newtonsoft.Json库。\r\n" +
                        "Newtonsoft.Json遵循MIT License。\r\n" +
                        "版权所有 (c) [Newtonsoft.Json] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/MIT\r\n" +
                        "特此向Newtonsoft.Json及其贡献者表示感谢。";
                    break;
                case "System.Management":
                    info_Label5.Text =
                        "MM使用了System.Management库。\r\n" +
                        "NAudio遵循MIT License。\r\n" +
                        "版权所有 (c) [.NET Foundation 和贡献者] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "特此向.NET社区及其贡献者表示感谢。";
                    break;
                case "taglib-sharp-netstandard2.0":
                    info_Label5.Text =
                        "MM使用了taglib-sharp-netstandard2.0库。\r\n" +
                        "taglib-sharp-netstandard2.0遵循LGPL-2.1 License\r\n" +
                        "版权所有 (c) [taglib-sharp] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/LGPL-2.1\r\n" +
                        "特此向taglib-sharp及其贡献者表示感谢。";
                    break;
                case "MouseKeyHook":
                    info_Label5.Text =
                        "MM使用了MouseKeyHook库。\r\n" +
                        "MouseKeyHook遵循MIT License\r\n" +
                        "版权所有 (c) [George Mamaladze] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "特此向George Mamaladze及其贡献者表示感谢。";
                    break;
                case "MediaToolkit":
                    info_Label5.Text =
                        "MM使用了MediaToolkit库。\r\n" +
                        "MediaToolkit暂无使用中的许可证\r\n" +
                        "版权所有 (c) [Aydin] \r\n" +
                        "特此向Aydin表示感谢。";
                    break;
                case "HtmlAgilityPack":
                    info_Label5.Text =
                        "HtmlAgilityPack。\r\n" +
                        "HtmlAgilityPack遵循 MIT License\r\n" +
                        "版权所有 (c) [zzzprojects] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "特此向zzzprojects及其贡献者表示感谢。";
                    break;
            }
        }
        private static WaveOutEvent currentOutputDevice = null;
        private static AudioFileReader currentAudioFile = null;
        public void PlayAudioex(string audioFilePath, int deviceNumber, float volume)
        {
            try
            {
                if (currentOutputDevice != null)
                {
                    currentOutputDevice.Stop();
                    currentOutputDevice.Dispose();
                    currentAudioFile?.Dispose();
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
                currentOutputDevice = outputDevice;
                currentAudioFile = audioFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放音频时出错: {ex.Message}", "错误");
            }
        }
        public void StopPlayback()
        {
            try
            {
                if (currentOutputDevice != null)
                {
                    currentOutputDevice.Stop(); // 停止播放
                    currentOutputDevice.Dispose(); // 释放音频输出设备资源
                    currentOutputDevice = null; // 清除引用
                }
                if (currentAudioFile != null)
                {
                    currentAudioFile.Dispose(); // 释放音频文件资源
                    currentAudioFile = null; // 清除引用
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"停止播放时出错: {ex.Message}", "错误");
            }
        }

        private void 设为播放项ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selectedItem = audioListView.SelectedItems[0];
            string filePath = selectedItem.Tag as string;
            selectedAudioPath = filePath;
            SelectedAudioLabel.Text = $"已选择:{selectedItem.Text}";
        }
        public async Task reLoadList()
        {
            audioInfo.Clear();
            audioListView.Items.Clear();
            await Misc.AddAudioFilesToListView(runningDirectory + @"\AudioData\", audioListView);
            foreach (ListViewItem item in audioListView.Items)
            {
                string filePath = item.Tag as string;
                Keys key = Keys.None;
                if (item.SubItems[3].Text != "未绑定")
                { key = (Keys)Enum.Parse(typeof(Keys), item.SubItems[3].Text); }
                audioInfo.Add(new AudioInfo { Name = item.SubItems[0].Text, FilePath = filePath, Key = key });
            }

        }
        private async void reLoadAudioListsView_Click(object sender, EventArgs e)
        {
            reLoadList();
        }
        private void aifadian_Click(object sender, EventArgs e)
        {
            string url = "https://afdian.net/a/MusicalMoments";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }
        private void toVB_Click(object sender, EventArgs e)
        {
            toVB.Text = "下载中";
            using (WebClient webClient = new WebClient())
            {
                string url = "https://download.vb-audio.com/Download_CABLE/VBCABLE_Driver_Pack43.zip";
                string filePath = Path.Combine(runningDirectory, "VB.zip");
                try
                {
                    webClient.DownloadFile(url, filePath);
                    DialogResult result = MessageBox.Show($"下载完成，已保存在程序的运行目录，是否打开文件？", "打开文件", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("下载文件时发生错误：" + ex.Message);
                }
                toVB.Text = "点我下载";
            }
        }
        private void toSettings_Click(object sender, EventArgs e)
        {
            mainTabControl.SelectedIndex = 2;
        }
        private void mToAudioData_Click(object sender, EventArgs e)
        {
            string folderPath = Path.Combine(runningDirectory, "AudioData");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            Process.Start(new ProcessStartInfo()
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
        private async void 打开文件所在位置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (audioListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = audioListView.SelectedItems[0];
                string filePath = selectedItem.Tag as string;
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    string argument = "/select, \"" + filePath + "\"";
                    Process.Start("explorer.exe", argument);
                }
                else
                {
                    reLoadList();
                }
            }
        }
        private void VBVolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            volume_Label1.Text = $"声卡麦({VBVolumeTrackBar.Value}%):";
            VBvolume = VBVolumeTrackBar.Value / 100f;
        }
        private void VolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            volume_Label2.Text = $"物理麦({VolumeTrackBar.Value}%):";
            volume = VolumeTrackBar.Value / 100f;
        }
        private void TipsVolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            volume_Label3.Text = $"提示音({TipsVolumeTrackBar.Value}%):";
            tipsvolume = TipsVolumeTrackBar.Value / 100f;
        }
        private static int logoClickCount = 0;
        private void LogoImage_Click(object sender, EventArgs e)
        {
            logoClickCount++;
            if (logoClickCount >= 10)
            {
                MessageBox.Show($"嗨~我是CC，这个软件的开发者。我们第一次见面是在<{firstStart}> 现在是<{System.DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒")}> 这期间你已经播放了<{playedCount}>次音频 还切换了<{changedCount}>次音频流！！(ps:如果数据不太对可能是因为你不小心把运行目录的json删除了吧？)", "恭喜你发现了彩蛋！");
            }
        }


        private void upData_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "音视频文件|*.mp3;*.wav;*.ogg;*.acc;*.ncm;*.qmc3;*.mp4;*.avi|全部文件|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFile = openFileDialog.FileName;
                dataPath_TextBox.Text = selectedFile;
                name_TextBox.Text = Path.GetFileNameWithoutExtension(selectedFile);

                // 显示音频属性
                conversion_Label4.Text = "源信息:" + Misc.DisplayAudioProperties(selectedFile);
            }
        }

        private void tabPage6_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string selectedFile = files[0];
                dataPath_TextBox.Text = selectedFile;
                name_TextBox.Text = Path.GetFileNameWithoutExtension(selectedFile);

                // 显示音频属性
                conversion_Label4.Text = "源信息:" + Misc.DisplayAudioProperties(selectedFile);
            }
        }

        private void tabPage6_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }
        private void convert_Button_Click(object sender, EventArgs e)
        {
            if (!File.Exists(dataPath_TextBox.Text))
            {
                MessageBox.Show("选择的文件不存在", "错误");
            }
            else
            {
                string extension = Path.GetExtension(dataPath_TextBox.Text).ToLower();
                if (extension == ".ncm")
                {
                    if (Misc.NCMConvert(dataPath_TextBox.Text, runningDirectory + "AudioData\\" + name_TextBox.Text + ".mp3") == 0)
                    {
                        conversion_Label5.Text = "转换后:" + Misc.DisplayAudioProperties(runningDirectory + "AudioData\\" + name_TextBox.Text + ".mp3");
                        MessageBox.Show("转换成功 已存储至运行目录下的AudioData文件夹", "提示");
                    }
                    else
                    {
                        MessageBox.Show("转换失败", "错误");
                    }
                }
                else if (extension == ".flac" || extension == ".ogg" || extension == ".mp3" || extension == ".wav")
                {
                    string targetFormat = comboBoxOutputFormat.SelectedItem.ToString().ToLower();
                    if (AudioConverter.ConvertTo(dataPath_TextBox.Text, runningDirectory + "AudioData\\" + name_TextBox.Text + "." + targetFormat, targetFormat))
                    {
                        conversion_Label5.Text = "转换后:" + Misc.DisplayAudioProperties(runningDirectory + "AudioData\\" + name_TextBox.Text + "." + targetFormat);
                        MessageBox.Show("转换成功 已存储至运行目录下的AudioData文件夹", "提示");
                    }
                    else
                    {
                        MessageBox.Show("转换失败", "错误");
                    }
                }
                else
                {
                    MessageBox.Show("不支持的文件格式", "错误");
                }
            }
        }
        private async void audioListView_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string audioDataPath = Path.Combine(runningDirectory, "AudioData");
            HashSet<string> supportedExtensions = new HashSet<string> { ".mp3", ".wav" };
            Directory.CreateDirectory(audioDataPath);
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (supportedExtensions.Contains(extension))
                {
                    string destFile = Path.Combine(audioDataPath, Path.GetFileName(file));
                    if (File.Exists(destFile))
                    {
                        var result = MessageBox.Show($"文件 {Path.GetFileName(file)} 已存在。是否覆盖？", "文件冲突", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result != DialogResult.Yes)
                        {
                            continue;
                        }
                    }
                    File.Copy(file, destFile, true);
                }
            }
            reLoadList();
        }
        private void audioListView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void mToAudioData1_Click(object sender, EventArgs e)
        {
            string folderPath = Path.Combine(runningDirectory, "AudioData");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            Process.Start(new ProcessStartInfo()
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }

        private void 停止播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopPlayback();
        }

        public class AudioItem
        {
            public string Name { get; set; }
            public string DownloadLink { get; set; }

            public AudioItem(string name, string downloadLink)
            {
                Name = name;
                DownloadLink = downloadLink;
            }
        }

        // 定义一个全局变量来存储原始音频列表和对应链接
        List<ListViewItem> OriginalAudioItems = new List<ListViewItem>();
        private async void LoadList_Click(object sender, EventArgs e)
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download.json");
            string jsonUrl = "https://www.scmd.cc/api/all-audio";
            string hashUrl = "https://www.scmd.cc/api/all-audio-hash";

            try
            {
                bool useLocalJson = false;

                if (File.Exists(jsonFilePath))
                {
                    bool isHashMatching = await Misc.VerifyFileHashAsync(jsonFilePath, hashUrl);
                    if (isHashMatching)
                    {
                        Console.WriteLine("本地 JSON 文件哈希匹配，直接加载数据！");
                        useLocalJson = true;
                    }
                    else
                    {
                        Console.WriteLine("本地 JSON 文件已过期，重新下载！");
                    }
                }
                else
                {
                    Console.WriteLine("本地 JSON 文件不存在，开始下载！");
                }

                if (!useLocalJson)
                {
                    await Misc.DownloadJsonFile(jsonUrl, jsonFilePath);
                }

                // 清空 ListView 和 ListBox
                AudioListView_fd.Items.Clear();
                DownloadLinkListBox.Items.Clear();

                // 加载 JSON 数据到 ListView 和 ListBox
                await Misc.GetDownloadJsonFromFile(jsonFilePath, AudioListView_fd, DownloadLinkListBox);

                // 获取 total 并更新 numberLabel
                int? total = await Misc.GetTotalFromJsonFile(jsonFilePath);
                numberLabel.Text = total.HasValue ? $"{total.Value} 个项目" : $"{AudioListView_fd.Items.Count} 个项目";
                OriginalAudioItems.Clear();
                foreach (ListViewItem item in AudioListView_fd.Items)
                {
                    OriginalAudioItems.Add((ListViewItem)item.Clone());
                }

                if (!Debugger.IsAttached) { Misc.ButtonStabilization(5, LoadList); }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载列表时发生错误: {ex.Message}");
            }
        }





        private void SearchBarTextBox_Enter(object sender, EventArgs e)
        {
            if (SearchBarTextBox.Text == "搜索")
            {
                SearchBarTextBox.Text = "";
            }
        }

        private void SearchBarTextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBarTextBox.Text))
            {
                SearchBarTextBox.Text = "搜索";
            }
        }

        private void SearchBarTextBox_TextChanged(object sender, EventArgs e)
        {
            string keyword = SearchBarTextBox.Text.Trim().ToLower();

            // 如果备份列表为空，就直接返回
            if (OriginalAudioItems == null || OriginalAudioItems.Count == 0)
                return;

            AudioListView_fd.BeginUpdate();
            AudioListView_fd.Items.Clear();

            IEnumerable<ListViewItem> filteredItems;

            if (string.IsNullOrWhiteSpace(keyword) || keyword == "搜索")
            {
                filteredItems = OriginalAudioItems;
            }
            else
            {
                filteredItems = OriginalAudioItems.Where(item =>
                    (item.SubItems.Count > 0 && item.SubItems[0].Text.ToLower().Contains(keyword)) || // 名称列
                    (item.SubItems.Count > 1 && item.SubItems[1].Text.ToLower().Contains(keyword)) || // 下载次数列
                    (item.SubItems.Count > 2 && item.SubItems[2].Text.ToLower().Contains(keyword))    // 上传者列
                );
            }

            foreach (var item in filteredItems)
            {
                AudioListView_fd.Items.Add((ListViewItem)item.Clone());
            }

            AudioListView_fd.EndUpdate();

            numberLabel.Text = $"{AudioListView_fd.Items.Count} 个项目";
        }


        private async void DownloadSelected_Click(object sender, EventArgs e)
        {
            if (AudioListView_fd.SelectedIndices.Count == 0)
            {
                MessageBox.Show("请选择要下载的音频。", "提示");
                return;
            }

            int selectedIndex = AudioListView_fd.SelectedIndices[0];

            // 确保 `ListBox` 也有对应的链接
            if (selectedIndex >= DownloadLinkListBox.Items.Count)
            {
                MessageBox.Show("未找到对应的下载链接。", "错误");
                return;
            }

            // 获取下载链接
            string downloadLink = DownloadLinkListBox.Items[selectedIndex].ToString();

            // 获取原始 URL 文件名
            string rawFileName = Path.GetFileName(new Uri(downloadLink).AbsolutePath);

            // ✅ 解码 URL 编码的文件名
            string decodedFileName = Uri.UnescapeDataString(rawFileName);

            // 指定下载的保存路径
            string saveDirectory = Path.Combine(Application.StartupPath, "AudioData");
            string savePath = Path.Combine(saveDirectory, decodedFileName);

            try
            {
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                using (HttpClient client = new HttpClient())
                {
                    byte[] fileBytes = await client.GetByteArrayAsync(downloadLink);
                    await File.WriteAllBytesAsync(savePath, fileBytes);
                }

                MessageBox.Show($"已成功下载到音频文件夹：{decodedFileName}\n保存路径：{savePath}", "下载完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"下载失败：{ex.Message}", "错误");
            }
        }





        bool pluginServer = false;
        private void TogglePluginServer_Click(object sender, EventArgs e)
        {
            if (!pluginServer)
            {
                PluginSDK.PluginServer.StartServer();
                pluginServer = true;
                TogglePluginServer.Text = "关闭插件服务";
                PluginStatus.Text = "插件状态:已开启";
                LoadPlugin.Enabled = true;
                pluginListView.Enabled = true;
                PluginServerAddress.Text = $"{PluginSDK.PluginServer.GetServerAddress()}";
            }
            else
            {
                PluginSDK.PluginServer.StopServer();
                pluginServer = false;
                TogglePluginServer.Text = "开启插件服务";
                PluginStatus.Text = "插件状态:未开启";
                LoadPlugin.Enabled = false;
                pluginListView.Enabled = false;
                PluginServerAddress.Text = "";
            }
        }

        private void PluginServerAddress_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(PluginServerAddress.Text);
        }

        private void mToPluginData_Click(object sender, EventArgs e)
        {
            string folderPath = Path.Combine(runningDirectory, "Plugin");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            Process.Start(new ProcessStartInfo()
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
        private void LoadPlugin_Click(object sender, EventArgs e)
        {
            // 检查是否有选中的项
            if (pluginListView.SelectedItems.Count > 0)
            {
                // 获取选中项
                ListViewItem selectedItem = pluginListView.SelectedItems[0];

                // 从选中项的 Tag 属性中获取插件文件的完整路径
                string pluginFilePath = selectedItem.Tag as string;

                // 如果插件文件路径不为空
                if (!string.IsNullOrEmpty(pluginFilePath))
                {
                    // 设置启动信息
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = pluginFilePath;
                    startInfo.Arguments = PluginServerAddress.Text;

                    // 启动选中的插件应用程序
                    try
                    { Process.Start(startInfo); }
                    catch (Exception ex) { MessageBox.Show($"请确认该插件是否为可执行文件 错误详情:\r\n{ex}", "错误"); }
                }
            }
            else
            {
                // 如果没有选中的项，给出提示或者清空显示的内容
                MessageBox.Show("请选择要加载的插件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void reLoadPluginListsView_Click(object sender, EventArgs e)
        {
            Misc.AddPluginFilesToListView(runningDirectory + @"\Plugin\", pluginListView);
        }

        private void toC_Click(object sender, EventArgs e)
        {
            mainTabControl.SelectedIndex = 5;
        }

        private void audioTips_Click(object sender, EventArgs e)
        {
            MessageBox.Show("如果您的音频格式非192000hz频率那么在播放时可能会出现电音 这是因为VB声卡通道是192000hz频率 \r\n(我都检查多少遍代码了也没有什么缓冲溢出 这是VB的原因关我什么事 别一直吵吵 还是那句话 别人能用为什么你不能用 有时候该换个方位想想究竟是软件的问题还是文件的问题 我在三台机子上都试过了并未出现电音 且委托朋友帮我测试也都没有问题 人不行别怪路不平 代码都开源的 有问题你找出来我能不改吗?关键是你也找不出问题 就什么事都赖我身上 爱用不用没强迫你用 不行就去用SoundPad 没人拦着你)", "提示");
        }
        public static Keys nowKey = Keys.None;
        private async void 绑定按键ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selectedItem = audioListView.SelectedItems[0];
            Keys key = Keys.None;
            if (selectedItem.SubItems[3].Text != "未绑定")
            {
                key = (Keys)Enum.Parse(typeof(Keys), selectedItem.SubItems[3].Text);
            }
            BindKeyWindow bindKeyWindow = new BindKeyWindow(key);
            bindKeyWindow.ShowDialog();
            nowKey = bindKeyWindow.Key;

            Misc.WriteKeyJsonInfo(Path.ChangeExtension(selectedItem.Tag.ToString(), ".json"), nowKey.ToString());
            if (CheckDuplicateKeys()) { MessageBox.Show($"已检测到相同按键 请勿作死将两个或多个音频绑定在同个按键上 该操作可能会导致MM崩溃 此提示会在绑定按键时与软件启动时检测并发出", "温馨提示"); }

        }

        public static bool CheckDuplicateKeys()
        {
            for (int i = 0; i < audioInfo.Count; i++)
            {
                var parentItem = audioInfo[i];
                if (parentItem.Key == Keys.None)
                    continue;

                for (int j = i + 1; j < audioInfo.Count; j++)
                {
                    var childItem = audioInfo[j];
                    if (childItem.Key == Keys.None)
                        continue;

                    if (parentItem.Key == childItem.Key)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        private void FeedbackTipsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请留下您的问题与您的联系方式 如电子邮箱、QQ等 收到反馈后会在72小时内回复您\r\n但请注意 切勿滥用", "提示");
        }

        private void FeedbackButton_Click(object sender, EventArgs e)
        {
            string host = "smtphz.qiye.163.com";
            int port = 25;
            string from = "feedback@scmd.cc";
            string to = "feedback@scmd.cc";
            MailMessage message = new MailMessage(from, to);
            string level = "普通";
            if (FeedbackAverage.Checked) { level = "普通"; }
            if (FeedbackUrgent.Checked) { level = "紧急"; }
            if (FeedbackDisaster.Checked) { level = "灾难"; }

            message.Subject = $"{level} - {FeedbackTitle.Text}";
            message.IsBodyHtml = true;
            //我有强迫症 看不惯难看的默认样式 然后特地为这个写了个很好看很好看的样式(★w★）
            string htmlBody = $@"
    <html>
        <head>
            <style>
                body {{
                    font-family: 'Arial', sans-serif;
                    margin: 0;
                    padding: 0;
                    background-color: #f7f7f7;
                }}
                .container {{
                    max-width: 600px;
                    margin: 20px auto;
                    background: white;
                    border-radius: 8px;
                    box-shadow: 0 4px 8px rgba(0,0,0,0.1);
                    background-image: linear-gradient(to bottom right, #FFD3A5, #FD6585);
                    overflow: hidden;
                }}
                .header {{
                    background: #FFF;
                    padding: 20px;
                    text-align: center;
                    border-top-left-radius: 8px;
                    border-top-right-radius: 8px;
                    border-bottom: 1px solid #eee;
                }}
                .body {{
                    padding: 20px;
                    background: #FFF;
                    color: #333;
                }}
                .footer {{
                    background: #FFF;
                    padding: 20px;
                    text-align: center;
                    border-bottom-left-radius: 8px;
                    border-bottom-right-radius: 8px;
                    border-top: 1px solid #eee;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1 style='color: #FD6585;'>{level} - {FeedbackTitle.Text}</h1>
                </div>
                <div class='body'>
                    <p>{FeedbackContent.Text.Replace("\n", "<br>")}</p>
                </div>
                <div class='footer'>
                    <p>联系方式:</strong> {Contact.Text}</p>
                </div>
            </div>
        </body>
    </html>";

            message.Body = htmlBody;
            if (string.IsNullOrWhiteSpace(FeedbackTitle.Text) || string.IsNullOrWhiteSpace(FeedbackContent.Text))
            {
                MessageBox.Show("标题和内容不能为空，请填写后再提交。", "错误");
                return;
            }

            if (!IsValidContent(FeedbackContent.Text))
            {
                MessageBox.Show("请输入有意义的内容，避免乱码或无意义字符。", "错误");
                return;
            }

            SmtpClient client = new SmtpClient(host, port);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("feedback@scmd.cc", "SCMDfb2023");
            try
            {
                client.Send(message);
                MessageBox.Show("反馈发送成功！", "谢谢你");
            }
            catch (Exception ex)
            {
                MessageBox.Show("邮件发送失败：" + ex.Message, "抱歉");
            }
        }
        private bool IsValidContent(string content)
        {
            int chineseCount = content.Count(c => c >= 0x4E00 && c <= 0x9FA5);
            if (chineseCount < content.Length * 0.3)
                return false;
            if (content.Length < 10)
                return false;
            const int maxRepetitions = 3;
            char lastChar = '\0';
            int currentRepetition = 1;

            foreach (char c in content)
            {
                if (c == lastChar)
                {
                    currentRepetition++;
                    if (currentRepetition > maxRepetitions)
                        return true;
                }
                else
                {
                    lastChar = c;
                    currentRepetition = 1;
                }
            }
            return true;
        }

        private void audioEquipmentPlay_CheckedChanged(object sender, EventArgs e)
        {
            VBInputComboSelect = comboBox_VBAudioEquipmentInput.SelectedIndex;
            VBOutputComboSelect = comboBox_AudioEquipmentInput.SelectedIndex;
            InputComboSelect = comboBox_VBAudioEquipmentOutput.SelectedIndex;
            OutputComboSelect = comboBox_AudioEquipmentOutput.SelectedIndex;
            AudioEquipmentPlayCheck = audioEquipmentPlay.Checked;
        }

        private void comboBox_VBAudioEquipmentInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            VBInputComboSelect = comboBox_VBAudioEquipmentInput.SelectedIndex;
            VBOutputComboSelect = comboBox_AudioEquipmentInput.SelectedIndex;
            InputComboSelect = comboBox_VBAudioEquipmentOutput.SelectedIndex;
            OutputComboSelect = comboBox_AudioEquipmentOutput.SelectedIndex;
            AudioEquipmentPlayCheck = audioEquipmentPlay.Checked;
        }

        private void comboBox_VBAudioEquipmentOutput_SelectedIndexChanged(object sender, EventArgs e)
        {
            VBInputComboSelect = comboBox_VBAudioEquipmentInput.SelectedIndex;
            VBOutputComboSelect = comboBox_AudioEquipmentInput.SelectedIndex;
            InputComboSelect = comboBox_VBAudioEquipmentOutput.SelectedIndex;
            OutputComboSelect = comboBox_AudioEquipmentOutput.SelectedIndex;
            AudioEquipmentPlayCheck = audioEquipmentPlay.Checked;
        }

        private void comboBox_AudioEquipmentInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            VBInputComboSelect = comboBox_VBAudioEquipmentInput.SelectedIndex;
            VBOutputComboSelect = comboBox_AudioEquipmentInput.SelectedIndex;
            InputComboSelect = comboBox_VBAudioEquipmentOutput.SelectedIndex;
            OutputComboSelect = comboBox_AudioEquipmentOutput.SelectedIndex;
            AudioEquipmentPlayCheck = audioEquipmentPlay.Checked;
        }

        private void comboBox_AudioEquipmentOutput_SelectedIndexChanged(object sender, EventArgs e)
        {
            VBInputComboSelect = comboBox_VBAudioEquipmentInput.SelectedIndex;
            VBOutputComboSelect = comboBox_AudioEquipmentInput.SelectedIndex;
            InputComboSelect = comboBox_VBAudioEquipmentOutput.SelectedIndex;
            OutputComboSelect = comboBox_AudioEquipmentOutput.SelectedIndex;
            AudioEquipmentPlayCheck = audioEquipmentPlay.Checked;
        }

        private void open_help_window_Click(object sender, EventArgs e)
        {
            Form help_window = new HelpWindow();
            help_window.Show();
        }

        private void open_help_button2_Click(object sender, EventArgs e)
        {
            Form help_window = new HelpWindow();
            help_window.Show();
        }

        private void check_update_Click(object sender, EventArgs e)
        {
            var ori_text = check_update.Text;
            check_update.Text = "检查中...";
            CheckNewVer(true);
            check_update.Text = ori_text;
        }

        private void open_help_button1_Click(object sender, EventArgs e)
        {
            Form help_window = new HelpWindow();
            help_window.Show();
        }
    }
}