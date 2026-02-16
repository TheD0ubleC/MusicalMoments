using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows.Forms;

namespace MusicalMoments.WithinReach
{
    internal sealed partial class MainForm : Form
    {
        private const string SettingsFileName = "withinreach.settings.json";

        private readonly PluginWsClient wsClient;
        private readonly WithinReachWebHost webHost;
        private readonly object modeLock = new object();

        private WebClickMode currentMode = WebClickMode.SetSelected;
        private bool refreshingStatus;
        private bool updatingModeSelection;

        public MainForm(string serverAddress)
        {
            wsClient = new PluginWsClient(serverAddress);
            webHost = new WithinReachWebHost(wsClient, GetCurrentMode, SetCurrentMode);
            InitializeComponent();
            ApplyMode(LoadPersistedMode(), persist: false);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            UpdateWebUi();
            statusTimer.Start();
            await RefreshStatusAsync();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            statusTimer.Stop();
            webHost.Dispose();
            wsClient.Dispose();
        }

        private async void statusTimer_Tick(object sender, EventArgs e)
        {
            await RefreshStatusAsync();
        }

        private async void ToggleWebButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (webHost.IsRunning)
                {
                    webHost.Stop();
                }
                else
                {
                    webHost.Start((int)portInput.Value);
                }

                UpdateWebUi();
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"切换 Web 服务失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenWebButton_Click(object sender, EventArgs e)
        {
            if (!webHost.IsRunning || string.IsNullOrWhiteSpace(webHost.BrowserAddress))
            {
                return;
            }

            Process.Start(new ProcessStartInfo { FileName = webHost.BrowserAddress, UseShellExecute = true });
        }

        private void CopyAddressButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(addressTextBox.Text))
            {
                Clipboard.SetText(addressTextBox.Text);
            }
        }

        private async void refreshButton_Click(object sender, EventArgs e)
        {
            await RefreshStatusAsync();
        }

        private void ModeRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (updatingModeSelection)
            {
                return;
            }

            WebClickMode mode = modeSetSelectedRadio.Checked ? WebClickMode.SetSelected : WebClickMode.DirectPlay;
            SetCurrentMode(mode);
        }

        private async Task RefreshStatusAsync()
        {
            if (refreshingStatus)
            {
                return;
            }

            refreshingStatus = true;
            try
            {
                PluginStateSnapshot state = await wsClient.RequestAsync<PluginStateSnapshot>("get_state");
                PlaybackBehaviorSnapshot behavior = await wsClient.RequestAsync<PlaybackBehaviorSnapshot>("get_playback_behavior");
                if (state == null)
                {
                    mmStatusLabel.Text = "MM 连接状态：连接失败，等待重试";
                    mmStatusLabel.ForeColor = System.Drawing.Color.FromArgb(178, 34, 34);
                    playbackBehaviorLabel.Text = "播放逻辑：读取失败";
                }
                else
                {
                    mmStatusLabel.Text = $"MM 连接状态：已连接，播放={state.IsPlaying}，启用播放键={state.PlayAudioEnabled}";
                    mmStatusLabel.ForeColor = System.Drawing.Color.FromArgb(34, 139, 34);
                    string sameText = behavior?.SameAudioBehaviorText ?? state.SameAudioBehavior;
                    string diffText = behavior?.DifferentAudioBehaviorText ?? state.DifferentAudioBehavior;
                    playbackBehaviorLabel.Text = $"播放逻辑：同音频[{sameText}]，不同音频[{diffText}]";
                }

                if (webHost.IsRunning)
                {
                    webStatusLabel.Text = $"Web 服务状态：运行中（0.0.0.0:{webHost.Port}）";
                    webStatusLabel.ForeColor = System.Drawing.Color.FromArgb(34, 139, 34);
                }
                else
                {
                    webStatusLabel.Text = "Web 服务状态：未启动";
                    webStatusLabel.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90);
                }
            }
            finally
            {
                refreshingStatus = false;
            }
        }

        private WebClickMode GetCurrentMode()
        {
            lock (modeLock)
            {
                return currentMode;
            }
        }

        private void SetCurrentMode(WebClickMode mode)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetCurrentMode(mode)));
                return;
            }

            ApplyMode(mode, persist: true);
        }

        private void ApplyMode(WebClickMode mode, bool persist)
        {
            lock (modeLock)
            {
                currentMode = mode;
            }

            updatingModeSelection = true;
            try
            {
                modeSetSelectedRadio.Checked = mode == WebClickMode.SetSelected;
                modeDirectPlayRadio.Checked = mode == WebClickMode.DirectPlay;
                UpdateModeTipText();
            }
            finally
            {
                updatingModeSelection = false;
            }

            if (persist)
            {
                SavePersistedMode(mode);
            }
        }

        private void UpdateWebUi()
        {
            bool running = webHost.IsRunning;
            toggleWebButton.Text = running ? "关闭 Web 服务" : "启动 Web 服务";
            openWebButton.Enabled = running;
            copyAddressButton.Enabled = running;
            portInput.Enabled = !running;

            if (!running)
            {
                addressTextBox.Text = "Web 服务未启动。";
                return;
            }

            List<string> lines = new List<string>
            {
                $"本机浏览器地址: {webHost.BrowserAddress}",
                $"监听地址: {webHost.BindAddress}"
            };
            foreach (string ip in GetLocalIpv4Addresses())
            {
                lines.Add($"局域网访问地址: http://{ip}:{webHost.Port}/");
            }

            addressTextBox.Text = string.Join(Environment.NewLine, lines);
        }

        private void UpdateModeTipText()
        {
            modeTipLabel.Text = currentMode == WebClickMode.DirectPlay
                ? "当前模式：Web 按钮显示“播放此音频”，点击后立刻播放。"
                : "当前模式：Web 按钮显示“使用此音频”，点击后仅设为播放项。";
        }

        private WebClickMode LoadPersistedMode()
        {
            string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
            if (!File.Exists(settingsPath))
            {
                return WebClickMode.SetSelected;
            }

            try
            {
                string json = File.ReadAllText(settingsPath);
                PersistedSettings settings = JsonSerializer.Deserialize<PersistedSettings>(json);
                if (settings != null && TryParseMode(settings.Mode, out WebClickMode mode))
                {
                    return mode;
                }
            }
            catch
            {
                // Ignore invalid settings file and use default mode.
            }

            return WebClickMode.SetSelected;
        }

        private void SavePersistedMode(WebClickMode mode)
        {
            string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
            try
            {
                PersistedSettings settings = new PersistedSettings
                {
                    Mode = mode == WebClickMode.DirectPlay ? "direct_play" : "set_selected"
                };

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(settingsPath, json);
            }
            catch
            {
                // Ignore IO failures.
            }
        }

        private static bool TryParseMode(string value, out WebClickMode mode)
        {
            mode = WebClickMode.SetSelected;
            string normalizedValue = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalizedValue)
            {
                case "set_selected":
                case "setselected":
                case "selected":
                    mode = WebClickMode.SetSelected;
                    return true;
                case "direct_play":
                case "directplay":
                case "play":
                    mode = WebClickMode.DirectPlay;
                    return true;
                default:
                    return false;
            }
        }

        private static IEnumerable<string> GetLocalIpv4Addresses()
        {
            try
            {
                return Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .Where(address => address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address))
                    .Select(address => address.ToString())
                    .Distinct()
                    .OrderBy(item => item)
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private sealed class PersistedSettings
        {
            public string Mode { get; set; } = "set_selected";
        }
    }
}
