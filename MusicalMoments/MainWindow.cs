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
        private string firstStart = System.DateTime.Now.ToString("yyyy��MM��dd�� HHʱmm��ss��");
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
        private void MainWindow_Load(object sender, EventArgs e)
        {
            /*
            ���ڹ������ػ����ļ�  
            BuildLocalizationBaseFiles(this.Controls, $"{runningDirectory}Resources.zh-CN.resx");
            */
            mainTabControl.ItemSize = new System.Drawing.Size(0, 1);
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioData"));//���������Ƶ���ļ���
            Misc.AddAudioFilesToListView(runningDirectory + @"\AudioData\", audioListView);

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
                comboBox_AudioEquipmentInput.SelectedIndex = 0;
                comboBox_AudioEquipmentOutput.SelectedIndex = 0;
                comboBox_VBAudioEquipmentInput.SelectedIndex = 0;
                comboBox_VBAudioEquipmentOutput.SelectedIndex = 0;
                VBVolumeTrackBar_Scroll(null, null);
                VolumeTrackBar_Scroll(null, null);
                TipsVolumeTrackBar_Scroll(null, null);
                firstStart = System.DateTime.Now.ToString("yyyy��MM��dd HHʱmm��ss��");
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
                string extension = Path.GetExtension(dataPath_TextBox.Text);
                if (extension == ".ncm")
                {
                    if (Misc.NCMConvert(dataPath_TextBox.Text, runningDirectory + "AudioData\\" + name_TextBox.Text + "." + "mp3") == 0)
                    {
                        MessageBox.Show("ת���ɹ� �Ѵ洢������Ŀ¼�µ�AudioData�ļ���", "��ʾ");
                    }
                }
                else
                { MessageBox.Show("Ŀǰ�汾��֧��ת���ø�ʽ", "��ʾ"); }

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
            //�ú����������ɱ�׼���ػ��ļ� ���ļ�Ϊ��������
            StringBuilder resxContent = new StringBuilder();
            // ���.resx�ļ���Ҫ��ͷ��
            resxContent.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            resxContent.AppendLine("<root>");
            resxContent.AppendLine("  <!-- �򻯵�ͷ����Ϣ -->");
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
            // Ϊ�ؼ�����<data>Ԫ��
            resxContent.Append(BuildControlToXMLDataValue(controls));
            // ����ļ�β��
            resxContent.AppendLine("</root>");
            // ���浽�ļ�
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

        private void ֹͣ����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopPlayback();
        }
    }
}