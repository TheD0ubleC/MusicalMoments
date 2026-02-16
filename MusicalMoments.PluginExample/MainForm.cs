using System.Collections.Generic;
using System.Windows.Forms;

namespace MusicalMoments.PluginExample
{
    internal sealed partial class MainForm : Form
    {
        private readonly PluginWsClient wsClient;
        private readonly List<AudioItem> audioItems = new List<AudioItem>();
        private bool refreshing;

        public MainForm(string serverAddress)
        {
            wsClient = new PluginWsClient(serverAddress);
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            if (!wsClient.HasAddress)
            {
                statusLabel.Text = "状态：未收到插件服务地址参数。";
                return;
            }

            await RefreshStateAsync();
            await RefreshAudioListAsync();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            refreshTimer.Stop();
            wsClient.Dispose();
        }

        private async void refreshTimer_Tick(object sender, EventArgs e)
        {
            await RefreshStateAsync();
        }

        private void realtimeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            refreshTimer.Enabled = realtimeCheckBox.Checked;
        }

        private async void refreshStateButton_Click(object sender, EventArgs e)
        {
            await RefreshStateAsync();
        }

        private async void refreshAudioButton_Click(object sender, EventArgs e)
        {
            await RefreshAudioListAsync();
        }

        private async void playSelectedButton_Click(object sender, EventArgs e)
        {
            await PlaySelectedAudioAsync();
        }

        private async void stopButton_Click(object sender, EventArgs e)
        {
            await wsClient.RequestAsync<object>("stop_audio");
        }

        private async void audioListBox_DoubleClick(object sender, EventArgs e)
        {
            await PlaySelectedAudioAsync();
        }

        private async Task RefreshStateAsync()
        {
            if (refreshing)
            {
                return;
            }

            refreshing = true;
            try
            {
                PluginStateSnapshot state = await wsClient.RequestAsync<PluginStateSnapshot>("get_state");
                if (state == null)
                {
                    statusLabel.Text = "状态：连接失败，等待重试...";
                    return;
                }

                statusLabel.Text = "状态：已连接";
                stateTextBox.Text =
                    $"SdkVersion: {state.SdkVersion}{Environment.NewLine}" +
                    $"AppVersion: {state.AppVersion}{Environment.NewLine}" +
                    $"RunningDirectory: {state.RunningDirectory}{Environment.NewLine}" +
                    $"IsPlaying: {state.IsPlaying}{Environment.NewLine}" +
                    $"PlayAudioEnabled: {state.PlayAudioEnabled}{Environment.NewLine}" +
                    $"PlayAudioKey: {state.PlayAudioKey}{Environment.NewLine}" +
                    $"ToggleStreamKey: {state.ToggleStreamKey}{Environment.NewLine}" +
                    $"VBVolume: {state.VBVolume}{Environment.NewLine}" +
                    $"Volume: {state.Volume}{Environment.NewLine}" +
                    $"TipsVolume: {state.TipsVolume}{Environment.NewLine}" +
                    $"IsVBInstalled: {state.IsVBInstalled}{Environment.NewLine}" +
                    $"IsAdministrator: {state.IsAdministrator}{Environment.NewLine}" +
                    $"SelectedAudioPath: {state.SelectedAudioPath}{Environment.NewLine}" +
                    $"Timestamp: {state.Timestamp:yyyy-MM-dd HH:mm:ss}";
            }
            finally
            {
                refreshing = false;
            }
        }

        private async Task RefreshAudioListAsync()
        {
            AudioItem[] items = await wsClient.RequestAsync<AudioItem[]>("get_audio_list");
            audioItems.Clear();
            audioItems.AddRange(items ?? Array.Empty<AudioItem>());

            audioListBox.BeginUpdate();
            audioListBox.Items.Clear();
            foreach (AudioItem item in audioItems)
            {
                string keyText = string.IsNullOrWhiteSpace(item.Key) ? "未绑定" : item.Key;
                audioListBox.Items.Add($"{item.Name} [{keyText}]");
            }

            audioListBox.EndUpdate();
        }

        private async Task PlaySelectedAudioAsync()
        {
            int selectedIndex = audioListBox.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= audioItems.Count)
            {
                return;
            }

            await wsClient.RequestAsync<object>("play_audio", new { path = audioItems[selectedIndex].FilePath });
        }
    }
}
