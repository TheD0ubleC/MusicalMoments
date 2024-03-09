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
namespace MusicalMoments
{
    public partial class MainWindow : Form
    {
        public static string runningDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private Keys toggleStreamKey;
        private Keys playAudioKey;
        private string selectedAudioPath;
        private int closeCount = 0;
        private int playedCount = 0;
        private int changedCount = 0;
        private string firstStart = System.DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒");
        bool playAudio = true;
        private IKeyboardMouseEvents m_GlobalHook;
        private static bool isPlaying = false;
        float VBvolume = 1f;
        float volume = 1f;
        float tipsvolume = 1f;
        public MainWindow()
        {
            InitializeComponent();
            Subscribe();
        }
        public void Subscribe()
        {
            // 订阅全局键盘事件
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyDown += GlobalHookKeyDown;
        }
        private void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {
            // 检查是否按下了播放音频的按键
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
                            if (!isPlaying)
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
        private void MainWindow_Load(object sender, EventArgs e)
        {
            /*
            用于构建本地化基文件  
            BuildLocalizationBaseFiles(this.Controls, $"{runningDirectory}Resources.zh-CN.resx");
            */
            mainTabControl.ItemSize = new System.Drawing.Size(0, 1);
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioData"));//创建存放音频的文件夹
            Misc.AddAudioFilesToListView(runningDirectory + @"\AudioData\", audioListView);

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
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userSettings.json");
            if (File.Exists(configPath))
            {
                mainTabControl.SelectedIndex = 1;
                mainGroupBox.Text = "音频";
                try
                {
                    string json = File.ReadAllText(configPath);
                    var settings = JsonConvert.DeserializeObject<UserSettings>(json);
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
                comboBox_AudioEquipmentInput.SelectedIndex = 0;
                comboBox_AudioEquipmentOutput.SelectedIndex = 0;
                comboBox_VBAudioEquipmentInput.SelectedIndex = 0;
                comboBox_VBAudioEquipmentOutput.SelectedIndex = 0;
                VBVolumeTrackBar_Scroll(null, null);
                VolumeTrackBar_Scroll(null, null);
                TipsVolumeTrackBar_Scroll(null, null);
                firstStart = System.DateTime.Now.ToString("yyyy年MM月dd HH时mm分ss秒");
            }
        }
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
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
                CloseCount = closeCount + 1,
                PlayedCount = playedCount,
                ChangedCount = changedCount,
                FirstStart = firstStart,
            };
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(Path.Combine(runningDirectory, "userSettings.json"), json);
        }
        private void sideLists_SelectedIndexChanged(object sender, EventArgs e)
        {
            reLoadList();
            foreach (int index in sideLists.SelectedIndices)
            {
                mainTabControl.SelectTab(index);
            }
            foreach (ListViewItem item in sideLists.SelectedItems)
            {
                mainGroupBox.Text = $"{item.Text}";
            }
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
            PlayAudioex(filePath, Misc.GetOutputDeviceID(comboBox_AudioEquipmentOutput.SelectedItem.ToString()), volume);
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
                reLoadList();
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
        public void reLoadList()
        {
            audioListView.Items.Clear();
            Misc.AddAudioFilesToListView(runningDirectory + @"\AudioData\", audioListView);
        }
        private void reLoadAudioListsView_Click(object sender, EventArgs e)
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
        private void 打开文件所在位置ToolStripMenuItem_Click(object sender, EventArgs e)
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
                string extension = Path.GetExtension(dataPath_TextBox.Text);
                if (extension == ".ncm")
                {
                    if (Misc.NCMConvert(dataPath_TextBox.Text, runningDirectory + "AudioData\\" + name_TextBox.Text + "." + "mp3") == 0)
                    {
                        MessageBox.Show("转换成功 已存储至运行目录下的AudioData文件夹", "提示");
                    }
                }
                else
                { MessageBox.Show("目前版本不支持转换该格式", "提示"); }

            }
        }
        private void audioListView_DragDrop(object sender, DragEventArgs e)
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
        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (languageComboBox.SelectedItem.ToString())
            {
                case "简体中文":
                    break;
                case "English":
                    MessageBox.Show("The current version does not support this language, but support for this language will be added in the future.", "Tips");
                    //ApplyResourcesToControls(this.Controls, $"{runningDirectory}ResourceFiles\\Localization\\en\\en.resx", Assembly.GetExecutingAssembly());
                    break;
                case "日本語":
                    MessageBox.Show("現在のバージョンではこの言語はサポートされていませんが、将来的にはこの言語のサポートが追加される予定です。", "チップ");
                    //ApplyResourcesToControls(this.Controls, $"{runningDirectory}ResourceFiles\\Localization\\ja\\ja.resx", Assembly.GetExecutingAssembly());
                    break;
            }
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

        private void 停止播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopPlayback();
        }
    }
}