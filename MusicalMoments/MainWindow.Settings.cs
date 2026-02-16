using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MusicalMoments
{
    public partial class MainWindow
    {
        private bool refreshingAudioDeviceCombos;

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

                sameAudioPressBehavior = SameAudioPressBehavior.StopPlayback;
                differentAudioInterruptBehavior = DifferentAudioInterruptBehavior.StopAndPlayNew;
                UpdatePlaybackBehaviorToolTip();
                firstStart = DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒");
            }
        }

        private async void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveUserData(true);
            Unsubscribe();

            e.Cancel = true;
            await Misc.FadeOut(200, this);
        }

        private void SaveUserData(bool addCloseCount = false)
        {
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
                RestoreDefaultsAfterInstall = restoreDefaultsAfterInstallCheckBox?.Checked ?? true,
                VBVolume = VBVolumeTrackBar.Value / 100f,
                Volume = VolumeTrackBar.Value / 100f,
                TipsVolume = TipsVolumeTrackBar.Value / 100f,
                CloseCount = closeCount + (addCloseCount ? 1 : 0),
                PlayedCount = playedCount,
                ChangedCount = changedCount,
                FirstStart = firstStart
            };

            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(Path.Combine(runningDirectory, "userSettings.json"), json);
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
