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
        private Keys pendingGameBinding = Keys.None;
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
                if (!KeyBindingService.TryParseBinding(state.PlayAudioKey, out Keys parsedPlayAudioKey))
                {
                    mmPlayAudioKey = Keys.None;
                }
                else
                {
                    mmPlayAudioKey = KeyBindingService.NormalizeBinding(parsedPlayAudioKey);
                }

                mmKeyLabel.Text = $"MM 播放按键：{(mmPlayAudioKey == Keys.None ? "未绑定" : KeyBindingService.GetDisplayTextForKey(mmPlayAudioKey))}";
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

            if (KeyBindingService.NormalizeBinding(e.Key) != mmPlayAudioKey || !latestState.PlayAudioEnabled)
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
            Keys normalized = KeyBindingService.NormalizeBinding(key);
            if (normalized == Keys.None)
            {
                return false;
            }

            Keys modifiers = normalized & Keys.Modifiers;
            Keys primary = KeyBindingService.NormalizePrimaryKey(normalized & Keys.KeyCode);
            if (!PressModifierKeys(modifiers))
            {
                return false;
            }

            if (TrySendPrimaryDown(primary))
            {
                return true;
            }

            ReleaseModifierKeys(modifiers);
            return false;
        }

        private static void ReleaseBoundInput(Keys key)
        {
            Keys normalized = KeyBindingService.NormalizeBinding(key);
            if (normalized == Keys.None)
            {
                return;
            }

            Keys modifiers = normalized & Keys.Modifiers;
            Keys primary = KeyBindingService.NormalizePrimaryKey(normalized & Keys.KeyCode);
            SendPrimaryUp(primary);
            ReleaseModifierKeys(modifiers);
        }

        private static bool TapBoundInput(Keys key)
        {
            if (!TryPressBoundInput(key))
            {
                return false;
            }

            ReleaseBoundInput(key);
            return true;
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
            if (!KeyBindingService.TryBuildBindingFromKeyEvent(e, out Keys key, out string displayText))
            {
                return;
            }

            bindKeyTextBox.Text = displayText;
            e.SuppressKeyPress = true;
            if (KeyBindingService.IsModifierOnlyBinding(key))
            {
                pendingGameBinding = key;
                return;
            }

            CommitGameBinding(key, displayText);
        }

        private void bindKeyTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!KeyBindingService.ShouldCommitPendingBindingOnKeyUp(e, pendingGameBinding))
            {
                return;
            }

            CommitGameBinding(pendingGameBinding);
            e.SuppressKeyPress = true;
        }

        private void bindKeyTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void bindKeyTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (!KeyBindingService.TryGetSupportedMouseBinding(e.Button, Control.ModifierKeys, out Keys key, out string displayText))
            {
                return;
            }

            CommitGameBinding(key, displayText);
        }

        private void CommitGameBinding(Keys key, string displayText = null)
        {
            pendingGameBinding = Keys.None;
            gameKey = KeyBindingService.NormalizeBinding(key);
            bindKeyTextBox.Text = displayText ?? KeyBindingService.GetDisplayTextForKey(gameKey);
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
                if (settings == null || !KeyBindingService.TryParseBinding(settings.GameKey, out Keys key))
                {
                    bindKeyTextBox.Text = "None";
                    inputMode = HoldMode;
                    holdModeRadio.Checked = true;
                    return;
                }

                gameKey = KeyBindingService.NormalizeBinding(key);
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
                    GameKey = KeyBindingService.NormalizeBinding(gameKey).ToString(),
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

        private static bool PressModifierKeys(Keys modifiers)
        {
            if ((modifiers & Keys.Control) == Keys.Control && !SendKeyboardInput(Keys.ControlKey, keyUp: false))
            {
                return false;
            }

            if ((modifiers & Keys.Alt) == Keys.Alt && !SendKeyboardInput(Keys.Menu, keyUp: false))
            {
                if ((modifiers & Keys.Control) == Keys.Control)
                {
                    SendKeyboardInput(Keys.ControlKey, keyUp: true);
                }

                return false;
            }

            if ((modifiers & Keys.Shift) == Keys.Shift && !SendKeyboardInput(Keys.ShiftKey, keyUp: false))
            {
                if ((modifiers & Keys.Alt) == Keys.Alt)
                {
                    SendKeyboardInput(Keys.Menu, keyUp: true);
                }

                if ((modifiers & Keys.Control) == Keys.Control)
                {
                    SendKeyboardInput(Keys.ControlKey, keyUp: true);
                }

                return false;
            }

            return true;
        }

        private static void ReleaseModifierKeys(Keys modifiers)
        {
            if ((modifiers & Keys.Shift) == Keys.Shift)
            {
                SendKeyboardInput(Keys.ShiftKey, keyUp: true);
            }

            if ((modifiers & Keys.Alt) == Keys.Alt)
            {
                SendKeyboardInput(Keys.Menu, keyUp: true);
            }

            if ((modifiers & Keys.Control) == Keys.Control)
            {
                SendKeyboardInput(Keys.ControlKey, keyUp: true);
            }
        }

        private static bool TrySendPrimaryDown(Keys primary)
        {
            return primary switch
            {
                Keys.MButton => SendMouseInput(MouseEventFMiddleDown),
                Keys.XButton1 => SendMouseInput(MouseEventFXDown, XButton1),
                Keys.XButton2 => SendMouseInput(MouseEventFXDown, XButton2),
                _ => SendKeyboardInput(primary, keyUp: false)
            };
        }

        private static void SendPrimaryUp(Keys primary)
        {
            switch (primary)
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
                    SendKeyboardInput(primary, keyUp: true);
                    break;
            }
        }

        private static bool SendKeyboardInput(Keys key, bool keyUp)
        {
            Keys normalized = KeyBindingService.NormalizePrimaryKey(key & Keys.KeyCode);
            if (normalized == Keys.None)
            {
                return false;
            }

            uint flags = keyUp ? KeyEventFKeyUp : 0;
            if (RequiresExtendedKeyFlag(normalized))
            {
                flags |= KeyEventFExtendedKey;
            }

            keybd_event((byte)normalized, 0, flags, UIntPtr.Zero);
            return true;
        }

        private static bool RequiresExtendedKeyFlag(Keys key)
        {
            return key == Keys.Menu
                || key == Keys.ControlKey
                || key == Keys.Insert
                || key == Keys.Delete
                || key == Keys.Home
                || key == Keys.End
                || key == Keys.PageUp
                || key == Keys.PageDown
                || key == Keys.Right
                || key == Keys.Left
                || key == Keys.Up
                || key == Keys.Down
                || key == Keys.NumLock
                || key == Keys.Cancel
                || key == Keys.PrintScreen
                || key == Keys.Divide;
        }
    }
}
