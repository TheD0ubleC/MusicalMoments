using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MusicalMoments
{
    public partial class MainWindow
    {
        private bool refreshingAudioDeviceCombos;
        private bool allowExitWithoutPrompt;
        private bool closingWithFade;
        private ClientUsageReporter usageReporter;

        private void InitializeCloseBehaviorUx()
        {
            if (closeBehaviorComboBox == null)
            {
                return;
            }

            if (closeBehaviorComboBox.Items.Count == 0)
            {
                closeBehaviorComboBox.Items.Add("隐藏到托盘");
                closeBehaviorComboBox.Items.Add("直接关闭");
                closeBehaviorComboBox.Items.Add("每次都询问");
            }

            if (closeBehaviorComboBox.SelectedIndex < 0)
            {
                closeBehaviorComboBox.SelectedIndex = 1;
            }
        }

        private WindowCloseBehavior GetSelectedCloseBehavior()
        {
            return closeBehaviorComboBox?.SelectedIndex switch
            {
                0 => WindowCloseBehavior.MinimizeToTray,
                2 => WindowCloseBehavior.AskEveryTime,
                _ => WindowCloseBehavior.ExitDirectly
            };
        }

        private void SetSelectedCloseBehavior(WindowCloseBehavior behavior)
        {
            if (closeBehaviorComboBox == null)
            {
                return;
            }

            closeBehaviorComboBox.SelectedIndex = behavior switch
            {
                WindowCloseBehavior.MinimizeToTray => 0,
                WindowCloseBehavior.AskEveryTime => 2,
                _ => 1
            };
        }

        private static WindowCloseBehavior ParseCloseBehavior(string value)
        {
            if (!string.IsNullOrWhiteSpace(value)
                && Enum.TryParse(value, true, out WindowCloseBehavior behavior))
            {
                return behavior;
            }

            return WindowCloseBehavior.ExitDirectly;
        }

        private static int ClampComboIndex(int index, int itemCount)
        {
            if (itemCount <= 0)
            {
                return -1;
            }

            if (index < 0)
            {
                return 0;
            }

            return Math.Min(index, itemCount - 1);
        }

        private int GetSafeSelectedIndex(ComboBox comboBox)
        {
            if (comboBox == null)
            {
                return -1;
            }

            return ClampComboIndex(comboBox.SelectedIndex, comboBox.Items.Count);
        }

        private static int FindComboIndexByName(ComboBox comboBox, string preferredName)
        {
            if (comboBox == null || comboBox.Items.Count == 0 || string.IsNullOrWhiteSpace(preferredName))
            {
                return -1;
            }

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                string text = comboBox.Items[i]?.ToString() ?? string.Empty;
                if (string.Equals(text, preferredName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                string text = comboBox.Items[i]?.ToString() ?? string.Empty;
                if (text.Contains(preferredName, StringComparison.OrdinalIgnoreCase)
                    || preferredName.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void FillComboAndSelect(ComboBox comboBox, string[] devices, string preferredName, int fallbackIndex)
        {
            comboBox.BeginUpdate();
            comboBox.Items.Clear();
            foreach (string device in devices)
            {
                comboBox.Items.Add(device);
            }

            if (comboBox.Items.Count > 0)
            {
                int targetIndex = FindComboIndexByName(comboBox, preferredName);
                if (targetIndex < 0)
                {
                    targetIndex = ClampComboIndex(fallbackIndex, comboBox.Items.Count);
                }

                comboBox.SelectedIndex = targetIndex < 0 ? 0 : targetIndex;
            }
            else
            {
                comboBox.SelectedIndex = -1;
            }

            comboBox.EndUpdate();
        }

        private void SyncAudioDeviceSelectionCache()
        {
            VBInputComboSelect = GetSafeSelectedIndex(comboBox_VBAudioEquipmentInput);
            VBOutputComboSelect = GetSafeSelectedIndex(comboBox_AudioEquipmentInput);
            InputComboSelect = GetSafeSelectedIndex(comboBox_VBAudioEquipmentOutput);
            OutputComboSelect = GetSafeSelectedIndex(comboBox_AudioEquipmentOutput);
            AudioEquipmentPlayCheck = audioEquipmentPlay.Checked;

            string vbOutputName = comboBox_VBAudioEquipmentOutput.SelectedItem?.ToString() ?? string.Empty;
            string physicalOutputName = comboBox_AudioEquipmentOutput.SelectedItem?.ToString() ?? string.Empty;
            string microphoneInputName = comboBox_AudioEquipmentInput.SelectedItem?.ToString() ?? string.Empty;

            int vbOutputDeviceId = string.IsNullOrWhiteSpace(vbOutputName) ? -1 : Misc.GetOutputDeviceID(vbOutputName);
            int physicalOutputDeviceId = string.IsNullOrWhiteSpace(physicalOutputName) ? -1 : Misc.GetOutputDeviceID(physicalOutputName);
            int microphoneInputDeviceId = string.IsNullOrWhiteSpace(microphoneInputName) ? -1 : Misc.GetInputDeviceID(microphoneInputName);
            int tipOutputDeviceId = physicalOutputDeviceId >= 0 ? physicalOutputDeviceId : vbOutputDeviceId;

            UpdatePlaybackDeviceCache(vbOutputDeviceId, physicalOutputDeviceId, microphoneInputDeviceId, tipOutputDeviceId);
            UpdateSwitchStreamTipsCache();
        }

        internal void RefreshAudioDeviceCombosPreserveSelection()
        {
            string[] inputDevices = Misc.GetInputAudioDeviceNames();
            string[] outputDevices = Misc.GetOutputAudioDeviceNames();

            string currentVbInput = comboBox_VBAudioEquipmentInput.SelectedItem?.ToString() ?? string.Empty;
            string currentPhysicalInput = comboBox_AudioEquipmentInput.SelectedItem?.ToString() ?? string.Empty;
            string currentVbOutput = comboBox_VBAudioEquipmentOutput.SelectedItem?.ToString() ?? string.Empty;
            string currentPhysicalOutput = comboBox_AudioEquipmentOutput.SelectedItem?.ToString() ?? string.Empty;

            refreshingAudioDeviceCombos = true;
            try
            {
                FillComboAndSelect(comboBox_VBAudioEquipmentInput, inputDevices, currentVbInput, VBInputComboSelect);
                FillComboAndSelect(comboBox_AudioEquipmentInput, inputDevices, currentPhysicalInput, VBOutputComboSelect);
                FillComboAndSelect(comboBox_VBAudioEquipmentOutput, outputDevices, currentVbOutput, InputComboSelect);
                FillComboAndSelect(comboBox_AudioEquipmentOutput, outputDevices, currentPhysicalOutput, OutputComboSelect);
            }
            finally
            {
                refreshingAudioDeviceCombos = false;
            }

            SyncAudioDeviceSelectionCache();
        }

        private void ApplyAudioDeviceSelection(UserSettings settings)
        {
            refreshingAudioDeviceCombos = true;
            try
            {
                comboBox_VBAudioEquipmentInput.SelectedIndex = ClampComboIndex(settings.VBAudioEquipmentInputIndex, comboBox_VBAudioEquipmentInput.Items.Count);
                comboBox_AudioEquipmentInput.SelectedIndex = ClampComboIndex(settings.AudioEquipmentInputIndex, comboBox_AudioEquipmentInput.Items.Count);
                comboBox_VBAudioEquipmentOutput.SelectedIndex = ClampComboIndex(settings.VBAudioEquipmentOutputIndex, comboBox_VBAudioEquipmentOutput.Items.Count);
                comboBox_AudioEquipmentOutput.SelectedIndex = ClampComboIndex(settings.AudioEquipmentOutputIndex, comboBox_AudioEquipmentOutput.Items.Count);
            }
            finally
            {
                refreshingAudioDeviceCombos = false;
            }

            SyncAudioDeviceSelectionCache();
        }

        private void LoadUserData(bool changeToAudioPage = true)
        {
            InitializeCloseBehaviorUx();

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
                    UserSettings settings = JsonConvert.DeserializeObject<UserSettings>(json);
                    if (settings == null)
                    {
                        throw new InvalidDataException("配置文件格式无效。");
                    }

                    ApplyAudioDeviceSelection(settings);

                    if (!string.IsNullOrEmpty(settings.ToggleStreamKey) && Enum.TryParse(settings.ToggleStreamKey, out Keys toggleKey))
                    {
                        toggleStreamKey = toggleKey;
                        ToggleStream.Text = toggleKey.ToString();
                    }

                    if (!string.IsNullOrEmpty(settings.PlayAudioKey) && Enum.TryParse(settings.PlayAudioKey, out Keys playKey))
                    {
                        playAudioKey = playKey;
                        PlayAudio.Text = playKey.ToString();
                    }

                    VBVolumeTrackBar.Value = (int)(settings.VBVolume * 100);
                    VolumeTrackBar.Value = (int)(settings.Volume * 100);
                    TipsVolumeTrackBar.Value = (int)(settings.TipsVolume * 100);
                    VBVolumeTrackBar_Scroll(null, null);
                    VolumeTrackBar_Scroll(null, null);
                    TipsVolumeTrackBar_Scroll(null, null);

                    audioEquipmentPlay.Checked = settings.AudioEquipmentPlay;
                    switchStreamTips.Checked = settings.SwitchStreamTips;

                    if (!string.IsNullOrWhiteSpace(settings.SameAudioPressBehavior)
                        && Enum.TryParse(settings.SameAudioPressBehavior, out SameAudioPressBehavior sameBehavior))
                    {
                        sameAudioPressBehavior = sameBehavior;
                    }
                    else
                    {
                        sameAudioPressBehavior = SameAudioPressBehavior.StopPlayback;
                    }

                    if (!string.IsNullOrWhiteSpace(settings.DifferentAudioInterruptBehavior)
                        && Enum.TryParse(settings.DifferentAudioInterruptBehavior, out DifferentAudioInterruptBehavior differentBehavior))
                    {
                        differentAudioInterruptBehavior = differentBehavior;
                    }
                    else
                    {
                        differentAudioInterruptBehavior = DifferentAudioInterruptBehavior.StopAndPlayNew;
                    }

                    UpdatePlaybackBehaviorToolTip();

                    if (restoreDefaultsAfterInstallCheckBox != null)
                    {
                        restoreDefaultsAfterInstallCheckBox.Checked = settings.RestoreDefaultsAfterInstall ?? true;
                    }

                    SetSelectedCloseBehavior(ParseCloseBehavior(settings.CloseActionOnExit));
                    ApplyPerformanceSettingsFromUser(settings);

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
                refreshingAudioDeviceCombos = true;
                comboBox_AudioEquipmentInput.SelectedIndex = ClampComboIndex(0, comboBox_AudioEquipmentInput.Items.Count);
                comboBox_AudioEquipmentOutput.SelectedIndex = ClampComboIndex(0, comboBox_AudioEquipmentOutput.Items.Count);
                comboBox_VBAudioEquipmentInput.SelectedIndex = ClampComboIndex(0, comboBox_VBAudioEquipmentInput.Items.Count);
                comboBox_VBAudioEquipmentOutput.SelectedIndex = ClampComboIndex(0, comboBox_VBAudioEquipmentOutput.Items.Count);
                refreshingAudioDeviceCombos = false;
                SyncAudioDeviceSelectionCache();

                VBVolumeTrackBar_Scroll(null, null);
                VolumeTrackBar_Scroll(null, null);
                TipsVolumeTrackBar_Scroll(null, null);

                if (restoreDefaultsAfterInstallCheckBox != null)
                {
                    restoreDefaultsAfterInstallCheckBox.Checked = true;
                }

                SetSelectedCloseBehavior(WindowCloseBehavior.ExitDirectly);
                sameAudioPressBehavior = SameAudioPressBehavior.StopPlayback;
                differentAudioInterruptBehavior = DifferentAudioInterruptBehavior.StopAndPlayNew;
                UpdatePlaybackBehaviorToolTip();
                ApplyPerformanceSettingsFromUser(null);
                firstStart = DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒");
            }
        }


        private ClientUsageSnapshot BuildClientUsageSnapshot()
        {
            int played = Interlocked.Add(ref playedCount, 0);
            int closed = Interlocked.Add(ref closeCount, 0);
            int changed = Interlocked.Add(ref changedCount, 0);

            return new ClientUsageSnapshot(
                playedCount: (ulong)Math.Max(played, 0),
                closeCount: (ulong)Math.Max(closed, 0),
                streamChangedCount: (ulong)Math.Max(changed, 0));
        }

        private void StartClientUsageReporter()
        {
            if (usageReporter != null)
            {
                return;
            }

            usageReporter = ClientUsageReporter.TryCreate(
                runningDirectory,
                VersionService.CurrentVersionTag,
                BuildClientUsageSnapshot);
            usageReporter?.Start();
        }

        private void ReportClientUsageHeartbeat()
        {
            ClientUsageReporter reporter = usageReporter;
            if (reporter == null)
            {
                return;
            }

            _ = reporter.ReportHeartbeatAsync();
        }

        private void StopClientUsageReporter()
        {
            ClientUsageReporter reporter = usageReporter;
            usageReporter = null;
            reporter?.StopAndDispose(sendShutdown: true);
        }
        private async void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown
                || e.CloseReason == CloseReason.TaskManagerClosing
                || allowExitWithoutPrompt)
            {
                SaveUserData(true);
                CleanupBeforeExit();
                return;
            }

            if (closingWithFade)
            {
                e.Cancel = true;
                return;
            }

            WindowCloseBehavior behavior = GetSelectedCloseBehavior();
            if (behavior == WindowCloseBehavior.AskEveryTime)
            {
                CloseDecision decision = ShowCloseDecisionDialog();
                if (decision.Action == CloseDecisionAction.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                behavior = decision.Action == CloseDecisionAction.MinimizeToTray
                    ? WindowCloseBehavior.MinimizeToTray
                    : WindowCloseBehavior.ExitDirectly;

                if (decision.RememberChoice)
                {
                    SetSelectedCloseBehavior(behavior);
                }
            }

            if (behavior == WindowCloseBehavior.MinimizeToTray)
            {
                SaveUserData(true);
                e.Cancel = true;
                BeginInvoke(new Action(HideToTray));
                return;
            }

            e.Cancel = true;
            closingWithFade = true;
            try
            {
                Enabled = false;
                await UiEffectsService.FadeHideWithoutDispose(160, this);
                allowExitWithoutPrompt = true;
                Close();
            }
            finally
            {
                if (!IsDisposed && !allowExitWithoutPrompt)
                {
                    Enabled = true;
                    Show();
                    Opacity = 1;
                }

                closingWithFade = false;
            }
        }

        private void CleanupBeforeExit()
        {
            StopClientUsageReporter();
            StopOptimizationRealtimeUpdates();
            DisposeTrayFeatures();
            Unsubscribe();
        }

        private void SaveUserData(bool addCloseCount = false)
        {
            int closeValue = addCloseCount
                ? Interlocked.Increment(ref closeCount)
                : Interlocked.Add(ref closeCount, 0);
            int playedValue = Interlocked.Add(ref playedCount, 0);
            int changedValue = Interlocked.Add(ref changedCount, 0);

            UserSettings settings = new UserSettings
            {
                VBAudioEquipmentInputIndex = GetSafeSelectedIndex(comboBox_VBAudioEquipmentInput),
                AudioEquipmentInputIndex = GetSafeSelectedIndex(comboBox_AudioEquipmentInput),
                VBAudioEquipmentOutputIndex = GetSafeSelectedIndex(comboBox_VBAudioEquipmentOutput),
                AudioEquipmentOutputIndex = GetSafeSelectedIndex(comboBox_AudioEquipmentOutput),
                ToggleStreamKey = toggleStreamKey.ToString(),
                PlayAudioKey = playAudioKey.ToString(),
                AudioEquipmentPlay = audioEquipmentPlay.Checked,
                SwitchStreamTips = switchStreamTips.Checked,
                SameAudioPressBehavior = sameAudioPressBehavior.ToString(),
                DifferentAudioInterruptBehavior = differentAudioInterruptBehavior.ToString(),
                CloseActionOnExit = GetSelectedCloseBehavior().ToString(),
                RestoreDefaultsAfterInstall = restoreDefaultsAfterInstallCheckBox?.Checked ?? true,
                VBVolume = VBVolumeTrackBar.Value / 100f,
                Volume = VolumeTrackBar.Value / 100f,
                TipsVolume = TipsVolumeTrackBar.Value / 100f,
                CloseCount = closeValue,
                PlayedCount = playedValue,
                ChangedCount = changedValue,
                FirstStart = firstStart
            };

            FillPerformanceSettingsToUser(settings);

            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(Path.Combine(runningDirectory, "userSettings.json"), json);

            if (addCloseCount)
            {
                ReportClientUsageHeartbeat();
            }
        }

        private void audioEquipmentPlay_CheckedChanged(object sender, EventArgs e)
        {
            if (refreshingAudioDeviceCombos)
            {
                return;
            }

            SyncAudioDeviceSelectionCache();
        }

        private void comboBox_VBAudioEquipmentInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (refreshingAudioDeviceCombos)
            {
                return;
            }

            SyncAudioDeviceSelectionCache();
        }

        private void comboBox_VBAudioEquipmentOutput_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (refreshingAudioDeviceCombos)
            {
                return;
            }

            SyncAudioDeviceSelectionCache();
        }

        private void comboBox_AudioEquipmentInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (refreshingAudioDeviceCombos)
            {
                return;
            }

            SyncAudioDeviceSelectionCache();
        }

        private void comboBox_AudioEquipmentOutput_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (refreshingAudioDeviceCombos)
            {
                return;
            }

            SyncAudioDeviceSelectionCache();
        }
    }
}


