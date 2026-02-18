using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using MusicalMoments;

namespace MusicalMoments.KeyHelp
{
    internal sealed partial class MainForm : Form
    {
        private const string SettingsFileName = "settings.json";
        private const string HoldMode = "hold";
        private const string ToggleMode = "toggle";

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const uint KeyEventFExtendedKey = 0x0001;
        private const uint KeyEventFKeyUp = 0x0002;
        private const uint InputMouse = 0;
        private const uint MouseEventFMiddleDown = 0x0020;
        private const uint MouseEventFMiddleUp = 0x0040;
        private const uint MouseEventFXDown = 0x0080;
        private const uint MouseEventFXUp = 0x0100;
        private const uint XButton1 = 0x0001;
        private const uint XButton2 = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        private readonly PluginWsClient wsClient;
        private readonly GlobalInputHook globalInputHook;

        private Keys gameKey = Keys.None;
        private Keys mmPlayAudioKey = Keys.None;
        private PluginStateSnapshot latestState;

        private bool pollInProgress;
        private bool holdTaskRunning;
        private string inputMode = HoldMode;

        public MainForm(string serverAddress)
        {
            wsClient = new PluginWsClient(serverAddress);
            globalInputHook = new GlobalInputHook();
            globalInputHook.HotkeyPressed += GlobalInputHook_HotkeyPressed;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadSettings();

            if (!wsClient.HasAddress)
            {
                statusLabel.Text = "状态：未收到插件服务地址参数，无法连接 MM。";
                return;
            }

            pollTimer.Start();
            globalInputHook.Start();
            statusLabel.Text = "状态：正在连接 MM 插件服务...";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            pollTimer.Stop();
            globalInputHook.HotkeyPressed -= GlobalInputHook_HotkeyPressed;
            globalInputHook.Dispose();
            wsClient.Dispose();
        }

        private async void pollTimer_Tick(object sender, EventArgs e)
        {
            await PollStateAsync();
        }

        private async Task PollStateAsync()
        {
            if (pollInProgress)
            {
                return;
            }

            pollInProgress = true;
            try
            {
                PluginStateSnapshot state = await wsClient.GetStateAsync();
                if (state == null)
                {
                    statusLabel.Text = "状态：连接失败，等待重试...";
                    return;
                }

                latestState = state;
                if (!Enum.TryParse(state.PlayAudioKey, out mmPlayAudioKey))
                {
                    mmPlayAudioKey = Keys.None;
                }

                mmKeyLabel.Text = $"MM 播放按键：{state.PlayAudioKey}";
                statusLabel.Text = "状态：已连接";
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"状态：连接异常 - {ex.Message}";
            }
            finally
            {
                pollInProgress = false;
            }
        }

        private async void GlobalInputHook_HotkeyPressed(object sender, GlobalHotkeyEventArgs e)
        {
            if (holdTaskRunning)
            {
                return;
            }

            if (gameKey == Keys.None || mmPlayAudioKey == Keys.None || latestState == null)
            {
                return;
            }

            if (e.Key != mmPlayAudioKey || !latestState.PlayAudioEnabled)
            {
                return;
            }

            holdTaskRunning = true;
            try
            {
                await Task.Delay(80);
                if (string.Equals(inputMode, ToggleMode, StringComparison.OrdinalIgnoreCase))
                {
                    await ToggleKeyWhilePlayingAsync();
                }
                else
                {
                    await HoldKeyWhilePlayingAsync();
                }
            }
            finally
            {
                holdTaskRunning = false;
            }
        }

        private async Task HoldKeyWhilePlayingAsync()
        {
            if (!TryPressBoundInput(gameKey))
            {
                statusLabel.Text = "状态：当前绑定暂不支持模拟。";
                return;
            }

            try
            {
                DateTime timeoutAt = DateTime.UtcNow.AddMinutes(5);
                while (DateTime.UtcNow < timeoutAt)
                {
                    PluginStateSnapshot state = await wsClient.GetStateAsync();
                    if (state == null || !state.IsPlaying)
                    {
                        break;
                    }

                    await Task.Delay(10);
                }
            }
            finally
            {
                ReleaseBoundInput(gameKey);
            }
        }

        private async Task ToggleKeyWhilePlayingAsync()
        {
            if (!TapBoundInput(gameKey))
            {
                statusLabel.Text = "状态：当前绑定暂不支持模拟。";
                return;
            }

            try
            {
                DateTime timeoutAt = DateTime.UtcNow.AddMinutes(5);
                while (DateTime.UtcNow < timeoutAt)
                {
                    PluginStateSnapshot state = await wsClient.GetStateAsync();
                    if (state == null || !state.IsPlaying)
                    {
                        break;
                    }

                    await Task.Delay(10);
                }
            }
            finally
            {
                TapBoundInput(gameKey);
            }
        }

        private static bool TryPressBoundInput(Keys key)
        {
            if (key == Keys.None)
            {
                return false;
            }

            switch (key)
            {
                case Keys.MButton:
                    return SendMouseInput(MouseEventFMiddleDown);
                case Keys.XButton1:
                    return SendMouseInput(MouseEventFXDown, XButton1);
                case Keys.XButton2:
                    return SendMouseInput(MouseEventFXDown, XButton2);
                default:
                    keybd_event((byte)key, 0, KeyEventFExtendedKey, UIntPtr.Zero);
                    return true;
            }
        }

        private static void ReleaseBoundInput(Keys key)
        {
            if (key == Keys.None)
            {
                return;
            }

            switch (key)
            {
                case Keys.MButton:
                    SendMouseInput(MouseEventFMiddleUp);
                    break;
                case Keys.XButton1:
                    SendMouseInput(MouseEventFXUp, XButton1);
                    break;
                case Keys.XButton2:
                    SendMouseInput(MouseEventFXUp, XButton2);
                    break;
                default:
                    keybd_event((byte)key, 0, KeyEventFExtendedKey | KeyEventFKeyUp, UIntPtr.Zero);
                    break;
            }
        }

        private static bool TapBoundInput(Keys key)
        {
            if (key == Keys.None)
            {
                return false;
            }

            switch (key)
            {
                case Keys.MButton:
                    return SendMouseInput(MouseEventFMiddleDown) && SendMouseInput(MouseEventFMiddleUp);
                case Keys.XButton1:
                    return SendMouseInput(MouseEventFXDown, XButton1) && SendMouseInput(MouseEventFXUp, XButton1);
                case Keys.XButton2:
                    return SendMouseInput(MouseEventFXDown, XButton2) && SendMouseInput(MouseEventFXUp, XButton2);
                default:
                    keybd_event((byte)key, 0, KeyEventFExtendedKey, UIntPtr.Zero);
                    keybd_event((byte)key, 0, KeyEventFExtendedKey | KeyEventFKeyUp, UIntPtr.Zero);
                    return true;
            }
        }

        private static bool SendMouseInput(uint flags, uint mouseData = 0)
        {
            INPUT[] inputs =
            {
                new INPUT
                {
                    type = InputMouse,
                    U = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dwFlags = flags,
                            mouseData = mouseData
                        }
                    }
                }
            };

            return SendInput(1, inputs, Marshal.SizeOf<INPUT>()) == 1;
        }

        private void bindKeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            string displayText = KeyBindingService.GetKeyDisplay(keyEventArgs: e);
            if (string.IsNullOrWhiteSpace(displayText))
            {
                return;
            }

            gameKey = displayText == "None" ? Keys.None : e.KeyCode;
            bindKeyTextBox.Text = displayText;
            SaveSettings();
            e.SuppressKeyPress = true;
        }

        private void bindKeyTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char keyChar = e.KeyChar;
            if (keyChar >= 'a' && keyChar <= 'z')
            {
                keyChar = (char)(keyChar - 32);
            }

            gameKey = (Keys)keyChar;
            bindKeyTextBox.Text = keyChar.ToString();
            SaveSettings();
            e.Handled = true;
        }

        private void bindKeyTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (!KeyBindingService.TryGetSupportedMouseBinding(e.Button, out Keys key, out string displayText))
            {
                return;
            }

            gameKey = key;
            bindKeyTextBox.Text = displayText;
            SaveSettings();
        }

        private void holdModeRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (!holdModeRadio.Checked)
            {
                return;
            }

            inputMode = HoldMode;
            SaveSettings();
        }

        private void toggleModeRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (!toggleModeRadio.Checked)
            {
                return;
            }

            inputMode = ToggleMode;
            SaveSettings();
        }

        private void LoadSettings()
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
                if (!File.Exists(settingsPath))
                {
                    bindKeyTextBox.Text = "None";
                    inputMode = HoldMode;
                    holdModeRadio.Checked = true;
                    return;
                }

                string json = File.ReadAllText(settingsPath);
                KeyHelpSettings settings = JsonSerializer.Deserialize<KeyHelpSettings>(json);
                if (settings == null || string.IsNullOrWhiteSpace(settings.GameKey) || !Enum.TryParse(settings.GameKey, out Keys key))
                {
                    bindKeyTextBox.Text = "None";
                    inputMode = HoldMode;
                    holdModeRadio.Checked = true;
                    return;
                }

                gameKey = key;
                bindKeyTextBox.Text = KeyBindingService.GetDisplayTextForKey(key);
                inputMode = string.Equals(settings.InputMode, ToggleMode, StringComparison.OrdinalIgnoreCase) ? ToggleMode : HoldMode;
                holdModeRadio.Checked = string.Equals(inputMode, HoldMode, StringComparison.OrdinalIgnoreCase);
                toggleModeRadio.Checked = string.Equals(inputMode, ToggleMode, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                bindKeyTextBox.Text = "None";
                inputMode = HoldMode;
                holdModeRadio.Checked = true;
            }
        }

        private void SaveSettings()
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
                var settings = new KeyHelpSettings
                {
                    GameKey = gameKey.ToString(),
                    InputMode = inputMode
                };
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);
            }
            catch
            {
                // Ignore save failures.
            }
        }

        private sealed class KeyHelpSettings
        {
            public string GameKey { get; set; } = Keys.None.ToString();
            public string InputMode { get; set; } = HoldMode;
        }
    }
}
