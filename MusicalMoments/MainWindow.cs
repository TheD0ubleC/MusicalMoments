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

namespace MusicalMoments
{
    public partial class MainWindow : Form
    {
        public static string nowVer = "v1.3.0-release-x64";
        public static string runningDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static Keys toggleStreamKey;
        public static Keys playAudioKey;
        public static string selectedAudioPath;
        public static string selectedPluginPath;
        public static int closeCount = 0;
        public static int playedCount = 0;
        public static int changedCount = 0;
        public static string firstStart = System.DateTime.Now.ToString("yyyy��MM��dd�� HHʱmm��ss��");
        public static bool playAudio = true;
        public static IKeyboardMouseEvents m_GlobalHook;
        public static bool isPlaying = false;
        public static float VBvolume = 1f;
        public static float volume = 1f;
        public static float tipsvolume = 1f;
        public MainWindow()
        {
            InitializeComponent();
            Subscribe();
        }
        public void Subscribe()
        {
            // ����ȫ�ּ����¼�
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyDown += GlobalHookKeyDown;
        }
        private void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {
            // ����Ƿ����˲�����Ƶ�İ���
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
            // ����Ƿ������л����İ���
            if (e.KeyCode == toggleStreamKey)
            {
                if (switchStreamTips.Checked)
                {
                    playAudio = !playAudio;
                    string audioFilePath = playAudio
                        ? Path.Combine(runningDirectory, @"ResourceFiles\�л�Ϊ��Ƶ.wav")
                        : Path.Combine(runningDirectory, @"ResourceFiles\�л�Ϊ��˷�.wav");
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
        }
        private void sideLists_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawBackground();
            // ����ͼƬ
            if (e.Item.ImageIndex >= 0)
            {
                e.Graphics.DrawImage(sideLists.SmallImageList.Images[e.Item.ImageIndex], e.Bounds.Left, e.Bounds.Top);
            }
            // �����ı���ɫΪ��ɫ
            Brush textBrush = new SolidBrush(Color.FromArgb(85, 85, 85));
            // �����ı�
            e.Graphics.DrawString(e.Item.Text, e.Item.Font, textBrush, e.Bounds.Left + 40, e.Bounds.Top);
        }
        private async void MainWindow_Load(object sender, EventArgs e)
        {
            Visible = false;
            foreach (TabPage tabPage in mainTabControl.TabPages)
            {
                mainTabControl.SelectTab(tabPage);
                //����ǿ��֢ ���Է�ֹ�л�ѡ���ʱ���п��ٵļ��ؾ�������ǰ�ȶ���һ��:D
            }
            /*
            ���ڹ������ػ����ļ�(�о��ò����ⶫ�� ��˵�����û��� ���м��������û����Ҿ�����������TT)
            BuildLocalizationBaseFiles(this.Controls, $"{runningDirectory}Resources.zh-CN.resx");
            */
            Misc.FadeIn(200, this);

            mainTabControl.ItemSize = new System.Drawing.Size(0, 1);
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioData"));//���������Ƶ���ļ���
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugin"));//������Ų�����ļ���
            Misc.AddAudioFilesToListView(runningDirectory + @"\AudioData\", audioListView);
            Misc.AddPluginFilesToListView(runningDirectory + @"\Plugin\", pluginListView);
            if (!Misc.IsAdministrator()) { Text += " [��ǰ�ǹ���Ա����,���ܻ���ְ��°����޷�Ӧ]"; }


            label_VBStatus.Text = Misc.checkVB() ? "VB�����Ѱ�װ" : "VB����δ��װ";
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



            LoadUserData();

            //����ٰ汾��֤ �Է�UI����
            CheckNewVer();

        }

        private void LoadUserData()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userSettings.json");
            if (File.Exists(configPath))
            {
                mainTabControl.SelectedIndex = 1;
                mainGroupBox.Text = "��Ƶ";
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
                        MessageBox.Show("�������Ѿ���������35������أ���������Ϊ̫ϲ������Ƶ������Ϊ�ҵĴ���BUG̫��һֱ�����󣿣���(ps:����С�ʵ��ɣ������˿��Է���Ӵ �Ͼ���Ҳ��֪����˭��ôϲ������Ƶ �����ǰ�����BUG?���滹�вʵ�Ŷ ��򿪼��ΰ�~)", "���ѽ");
                    }
                    else if (settings.CloseCount == 50)
                    {
                        MessageBox.Show("�������Ѿ���������50������أ�����Ҹ�ȷ�����Բ����Ҵ���BUG�࣡����ΪBUG��һֱ���˾ʹ����ҵ���������ã�֮���û��ͻỻ����ˣ�����(ps:������������80�κ����һ����ʵ���~)", "���ѽ");
                    }
                    else if (settings.CloseCount == 80)
                    {
                        MessageBox.Show("��ô���80�����𣿣����Ǿ͸������ʵ���λ�ð�~������Ҫȥ����ҳ�� Ȼ��������20��LOGOͼƬ������ �Ǿ����� ���� ����Ҳ�����вʵ���~", "���ѽ");
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
                    MessageBox.Show($"��ȡ����ʱ����: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                firstStart = System.DateTime.Now.ToString("yyyy��MM��dd�� HHʱmm��ss��");
            }
        }
        private async void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
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

            e.Cancel = true;
            Misc.FadeOut(200, this);
        }


        private async void CheckNewVer()
        {
            var newVerTips = await Misc.GetLatestVersionTipsAsync();
            var latestVer = await Misc.GetLatestVersionAsync();
            if (latestVer != nowVer)
            {
                DialogResult dialogResult = MessageBox.Show($"Musical Moments�����°汾���뾡����¡���ǰ�汾Ϊ{nowVer} ���°汾Ϊ{latestVer}��\r\n���������Զ��������°汾ѹ���� ���·�����ת�����°汾ҳ�� ����ȡ����ر�\r\n\r\n�������°汾���:\r\n{newVerTips}", "�°汾����", MessageBoxButtons.YesNoCancel);

                if (dialogResult == DialogResult.Yes)
                {
                    string[] parts = latestVer.Split('-');
                    string version = parts[0].TrimStart('v'); // ��ȡ�汾�Ų���
                    string downloadUrl = $"https://kkgithub.com/TheD0ubleC/MusicalMoments/releases/download/{latestVer}/MM.Release-{version}.zip";
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFile(downloadUrl, $"{runningDirectory}MM.Release-{version}.zip");
                            MessageBox.Show($"���سɹ� �Ѵ��������Ŀ¼ ����·��:{runningDirectory}MM.Release-{version}.zip", "��ʾ");
                        }
                    }
                    catch (Exception ex)
                    { MessageBox.Show($"����ʧ�ܣ�����githubҳ���������ػ���Ⱥ�ļ����� ��������:{ex.ToString()}", "����"); }
                }
                else if (dialogResult == DialogResult.No)
                {
                    Process.Start(new ProcessStartInfo("https://github.com/TheD0ubleC/MusicalMoments/releases/tag/" + latestVer) { UseShellExecute = true });
                }
            }

        }
        private void sideLists_SelectedIndexChanged(object sender, EventArgs e)
        {
            reLoadList();
            Misc.AddPluginFilesToListView(runningDirectory + @"\Plugin\", pluginListView);
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
            //MessageBox.Show(Misc.checkVB() ? "vb�Ѱ�װ" : "vbδ��װ");
            label_VBStatus.Text = Misc.checkVB() ? "VB�����Ѱ�װ" : "VB����δ��װ";
        }
        private void audioListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTestInfo = audioListView.HitTest(e.Location);
                if (hitTestInfo.Item != null)
                {
                    // ��ʾ�˵����������λ��
                    mainContextMenuStrip.Show(audioListView, e.Location);
                }
            }
        }
        private void ����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selectedItem = audioListView.SelectedItems[0];
            string filePath = selectedItem.Tag as string;
            PlayAudioex(filePath, Misc.GetOutputDeviceID(comboBox_AudioEquipmentOutput.SelectedItem.ToString()), volume);
            playedCount = playedCount + 1;
        }
        private void ɾ��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (audioListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = audioListView.SelectedItems[0];
                string filePath = selectedItem.Tag as string;
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    DialogResult dialogResult = MessageBox.Show("ȷ��Ҫɾ������ļ���", "ɾ���ļ�", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        try
                        {
                            File.Delete(filePath); // ɾ���ļ�
                            audioListView.Items.Remove(selectedItem); // ����ļ�ɾ���ɹ�����ListView���Ƴ���
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"ɾ���ļ�ʱ����: {ex.Message}");
                        }
                    }
                }
            }
        }
        private void ������ToolStripMenuItem_Click(object sender, EventArgs e)
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
                string input = Interaction.InputBox("�������µ����ƣ�", "������", currentName, -1, -1);
                if (string.IsNullOrEmpty(input) || input.Equals(currentName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                string newFileName = input + extension;
                string newFilePath = Path.Combine(directoryPath, newFileName);
                // ������������ļ��Ƿ��Ѵ���
                if (File.Exists(newFilePath))
                {
                    MessageBox.Show("�����ظ�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show($"�������ļ�ʱ����: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        "MMʹ����NAudio��Ƶ����⡣\r\n" +
                        "NAudio��ѭMicrosoft Public License (Ms-PL)��\r\n" +
                        "��Ȩ���� (c) [NAudio] \r\n" +
                        "���������֤�ı��������������ҵ�:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "�ش���NAudio���乱���߱�ʾ��л��";
                    break;
                case "Newtonsoft.Json":
                    info_Label5.Text =
                        "MMʹ����Newtonsoft.Json�⡣\r\n" +
                        "Newtonsoft.Json��ѭMIT License��\r\n" +
                        "��Ȩ���� (c) [Newtonsoft.Json] \r\n" +
                        "���������֤�ı��������������ҵ�:\r\n" +
                        "https://opensource.org/licenses/MIT\r\n" +
                        "�ش���Newtonsoft.Json���乱���߱�ʾ��л��";
                    break;
                case "System.Management":
                    info_Label5.Text =
                        "MMʹ����System.Management�⡣\r\n" +
                        "NAudio��ѭMIT License��\r\n" +
                        "��Ȩ���� (c) [.NET Foundation �͹�����] \r\n" +
                        "���������֤�ı��������������ҵ�:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "�ش���.NET�������乱���߱�ʾ��л��";
                    break;
                case "taglib-sharp-netstandard2.0":
                    info_Label5.Text =
                        "MMʹ����taglib-sharp-netstandard2.0�⡣\r\n" +
                        "taglib-sharp-netstandard2.0��ѭLGPL-2.1 License\r\n" +
                        "��Ȩ���� (c) [taglib-sharp] \r\n" +
                        "���������֤�ı��������������ҵ�:\r\n" +
                        "https://opensource.org/licenses/LGPL-2.1\r\n" +
                        "�ش���taglib-sharp���乱���߱�ʾ��л��";
                    break;
                case "MouseKeyHook":
                    info_Label5.Text =
                        "MMʹ����MouseKeyHook�⡣\r\n" +
                        "MouseKeyHook��ѭMIT License\r\n" +
                        "��Ȩ���� (c) [George Mamaladze] \r\n" +
                        "���������֤�ı��������������ҵ�:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "�ش���George Mamaladze���乱���߱�ʾ��л��";
                    break;
                case "MediaToolkit":
                    info_Label5.Text =
                        "MMʹ����MediaToolkit�⡣\r\n" +
                        "MediaToolkit����ʹ���е����֤\r\n" +
                        "��Ȩ���� (c) [Aydin] \r\n" +
                        "�ش���Aydin��ʾ��л��";
                    break;
                case "HtmlAgilityPack":
                    info_Label5.Text =
                        "HtmlAgilityPack��\r\n" +
                        "HtmlAgilityPack��ѭ MIT License\r\n" +
                        "��Ȩ���� (c) [zzzprojects] \r\n" +
                        "���������֤�ı��������������ҵ�:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "�ش���zzzprojects���乱���߱�ʾ��л��";
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
                // Ӧ����������
                outputDevice.Volume = volume; // ȷ�� volume ֵ�� 0 �� 1 ֮��
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
                MessageBox.Show($"������Ƶʱ����: {ex.Message}", "����");
                reLoadList();
            }
        }
        public void StopPlayback()
        {
            try
            {
                if (currentOutputDevice != null)
                {
                    currentOutputDevice.Stop(); // ֹͣ����
                    currentOutputDevice.Dispose(); // �ͷ���Ƶ����豸��Դ
                    currentOutputDevice = null; // �������
                }
                if (currentAudioFile != null)
                {
                    currentAudioFile.Dispose(); // �ͷ���Ƶ�ļ���Դ
                    currentAudioFile = null; // �������
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ֹͣ����ʱ����: {ex.Message}", "����");
            }
        }

        private void ��Ϊ������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selectedItem = audioListView.SelectedItems[0];
            string filePath = selectedItem.Tag as string;
            selectedAudioPath = filePath;
            SelectedAudioLabel.Text = $"��ѡ��:{selectedItem.Text}";
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
            toVB.Text = "������";
            using (WebClient webClient = new WebClient())
            {
                string url = "https://download.vb-audio.com/Download_CABLE/VBCABLE_Driver_Pack43.zip";
                string filePath = Path.Combine(runningDirectory, "VB.zip");
                try
                {
                    webClient.DownloadFile(url, filePath);
                    DialogResult result = MessageBox.Show($"������ɣ��ѱ����ڳ��������Ŀ¼���Ƿ���ļ���", "���ļ�", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("�����ļ�ʱ��������" + ex.Message);
                }
                toVB.Text = "��������";
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
        private void ���ļ�����λ��ToolStripMenuItem_Click(object sender, EventArgs e)
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
            volume_Label1.Text = $"������({VBVolumeTrackBar.Value}%):";
            VBvolume = VBVolumeTrackBar.Value / 100f;
        }
        private void VolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            volume_Label2.Text = $"������({VolumeTrackBar.Value}%):";
            volume = VolumeTrackBar.Value / 100f;
        }
        private void TipsVolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            volume_Label3.Text = $"��ʾ��({TipsVolumeTrackBar.Value}%):";
            tipsvolume = TipsVolumeTrackBar.Value / 100f;
        }
        private static int logoClickCount = 0;
        private void LogoImage_Click(object sender, EventArgs e)
        {
            logoClickCount++;
            if (logoClickCount >= 10)
            {
                MessageBox.Show($"��~����CC���������Ŀ����ߡ����ǵ�һ�μ�������<{firstStart}> ������<{System.DateTime.Now.ToString("yyyy��MM��dd��HHʱmm��ss��")}> ���ڼ����Ѿ�������<{playedCount}>����Ƶ ���л���<{changedCount}>����Ƶ������(ps:������ݲ�̫�Կ�������Ϊ�㲻С�İ�����Ŀ¼��jsonɾ���˰ɣ�)", "��ϲ�㷢���˲ʵ���");
            }
        }
        private void upData_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "����Ƶ�ļ�|*.mp3;*.wav;*.ogg;*.acc;*.ncm;*.qmc3;*.mp4;*.avi|ȫ���ļ�|*.*";
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
                MessageBox.Show("ѡ����ļ�������", "����");
            }
            else
            {
                string extension = Path.GetExtension(dataPath_TextBox.Text).ToLower();
                if (extension == ".ncm")
                {
                    if (Misc.NCMConvert(dataPath_TextBox.Text, runningDirectory + "AudioData\\" + name_TextBox.Text + ".mp3") == 0)
                    {
                        MessageBox.Show("ת���ɹ� �Ѵ洢������Ŀ¼�µ�AudioData�ļ���", "��ʾ");
                    }
                    else
                    {
                        MessageBox.Show("ת��ʧ��", "����");
                    }
                }
                else if (extension == ".flac" || extension == ".ogg" || extension == ".mp3" || extension == ".wav")
                {
                    string targetFormat = comboBoxOutputFormat.SelectedItem.ToString().ToLower();
                    if (AudioConverter.ConvertTo(dataPath_TextBox.Text, runningDirectory + "AudioData\\" + name_TextBox.Text + "." + targetFormat, targetFormat))
                    {
                        MessageBox.Show("ת���ɹ� �Ѵ洢������Ŀ¼�µ�AudioData�ļ���", "��ʾ");
                    }
                    else
                    {
                        MessageBox.Show("ת��ʧ��", "����");
                    }
                }
                else
                {
                    MessageBox.Show("��֧�ֵ��ļ���ʽ", "����");
                }
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
                        var result = MessageBox.Show($"�ļ� {Path.GetFileName(file)} �Ѵ��ڡ��Ƿ񸲸ǣ�", "�ļ���ͻ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                case "��������":
                    break;
                case "English":
                    MessageBox.Show("The current version does not support this language, but support for this language will be added in the future.", "Tips");
                    //ApplyResourcesToControls(this.Controls, $"{runningDirectory}ResourceFiles\\Localization\\en\\en.resx", Assembly.GetExecutingAssembly());
                    break;
                case "�ձ��Z":
                    MessageBox.Show("�F�ڤΥЩ`�����ǤϤ������Z�ϥ��ݩ`�Ȥ���Ƥ��ޤ��󤬡������ĤˤϤ������Z�Υ��ݩ`�Ȥ�׷�Ӥ�����趨�Ǥ���", "���å�");
                    //ApplyResourcesToControls(this.Controls, $"{runningDirectory}ResourceFiles\\Localization\\ja\\ja.resx", Assembly.GetExecutingAssembly());
                    break;
            }
        }


        private void ֹͣ����ToolStripMenuItem_Click(object sender, EventArgs e)
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

        // ����һ��ȫ�ֱ������洢ԭʼ��Ƶ�б�Ͷ�Ӧ����
        List<AudioItem> OriginalAudioList = new List<AudioItem>();
        private void LoadList_Click(object sender, EventArgs e)
        {
            AudioListBox.Items.Clear();
            // ���ԭʼ��Ƶ�б�
            OriginalAudioList.Clear();

            // ��ȡ���ؿ�Ƭ�����ص��б����
            Misc.GetDownloadCards("https://slam.scmd.cc/", AudioListBox, DownloadLinkListBox);

            // �����ؿ�Ƭ��Ϣ��ӵ�ԭʼ��Ƶ�б�
            for (int i = 0; i < AudioListBox.Items.Count; i++)
            {
                OriginalAudioList.Add(new AudioItem(AudioListBox.Items[i].ToString(), DownloadLinkListBox.Items[i].ToString()));
            }
            numberLabel.Text = $"{AudioListBox.Items.Count} ����Ŀ";
            if (!Debugger.IsAttached) { Misc.ButtonStabilization(5, LoadList); }

        }

        private void SearchBarTextBox_Enter(object sender, EventArgs e)
        {
            if (SearchBarTextBox.Text == "����")
            {
                SearchBarTextBox.Text = "";
            }
        }

        private void SearchBarTextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBarTextBox.Text))
            {
                SearchBarTextBox.Text = "����";
            }
        }

        private void SearchBarTextBox_TextChanged(object sender, EventArgs e)
        {
            // ����б��
            AudioListBox.Items.Clear();

            // ��ȡ�����ؼ���
            string keyword = SearchBarTextBox.Text.ToLower();

            // ����ؼ���Ϊ�գ�����ʾ������
            if (string.IsNullOrWhiteSpace(keyword) || keyword == "����")
            {
                foreach (var item in OriginalAudioList)
                {
                    AudioListBox.Items.Add(item.Name);
                }
            }
            else
            {
                // ����ԭʼ�б��е����������ʾ���������ؼ��ʵ���
                foreach (var item in OriginalAudioList)
                {
                    if (item.Name.ToLower().Contains(keyword))
                    {
                        AudioListBox.Items.Add(item.Name);
                    }
                }
            }
            numberLabel.Text = $"{AudioListBox.Items.Count} ����Ŀ";
        }

        private void DownloadSelected_Click(object sender, EventArgs e)
        {
            // ����Ƿ�ѡ������
            if (AudioListBox.SelectedIndex == -1)
            {
                MessageBox.Show("��ѡ��Ҫ���ص���Ƶ��", "��ʾ");
                return;
            }

            // ��ȡѡ���������
            string selectedName = AudioListBox.SelectedItem.ToString();

            // ����ѡ�������������
            string downloadLink = OriginalAudioList.Find(x => x.Name == selectedName).DownloadLink;

            // ����ҵ��������ӣ���������ز���
            if (!string.IsNullOrEmpty(downloadLink))
            {
                // ʹ�� WebClient ��������
                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        // ��ȡ�ļ�������������������ȡ��
                        string fileName = Path.GetFileName(downloadLink);

                        // ָ�����صı���·��
                        string savePath = Path.Combine(Application.StartupPath, "AudioData", fileName);

                        // �����ļ�
                        webClient.DownloadFile(downloadLink, savePath);

                        MessageBox.Show($"�ѳɹ����أ�{fileName}", "��ʾ");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"����ʧ�ܣ�{ex.Message}", "����");
                    }
                }
            }
            else
            {
                MessageBox.Show("δ�ҵ�ѡ������������ӡ�", "����");
            }
        }

        bool pluginServer = false;
        private void TogglePluginServer_Click(object sender, EventArgs e)
        {
            if (!pluginServer)
            {
                PluginSDK.PluginServer.StartServer();
                pluginServer = true;
                TogglePluginServer.Text = "�رղ������";
                PluginStatus.Text = "���״̬:�ѿ���";
                LoadPlugin.Enabled = true;
                pluginListView.Enabled = true;
                PluginServerAddress.Text = PluginSDK.PluginServer.GetServerAddress();
            }
            else
            {
                PluginSDK.PluginServer.StopServer();
                pluginServer = false;
                TogglePluginServer.Text = "�����������";
                PluginStatus.Text = "���״̬:δ����";
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
            // ����Ƿ���ѡ�е���
            if (pluginListView.SelectedItems.Count > 0)
            {
                // ��ȡѡ����
                ListViewItem selectedItem = pluginListView.SelectedItems[0];

                // ��ѡ����� Tag �����л�ȡ����ļ�������·��
                string pluginFilePath = selectedItem.Tag as string;

                // �������ļ�·����Ϊ��
                if (!string.IsNullOrEmpty(pluginFilePath))
                {
                    // ����������Ϣ
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = pluginFilePath;
                    startInfo.Arguments = PluginServerAddress.Text;

                    // ����ѡ�еĲ��Ӧ�ó���
                    try
                    { Process.Start(startInfo); }
                    catch(Exception ex) { MessageBox.Show($"��ȷ�ϸò���Ƿ�Ϊ��ִ���ļ� ��������:\r\n{ex}","����"); }
                }
            }
            else
            {
                // ���û��ѡ�е��������ʾ���������ʾ������
                MessageBox.Show("��ѡ��Ҫ���صĲ����", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void reLoadPluginListsView_Click(object sender, EventArgs e)
        {
            Misc.AddPluginFilesToListView(runningDirectory + @"\Plugin\", pluginListView);
        }
    }
}