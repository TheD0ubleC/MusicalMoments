using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicalMoments
{
    public partial class MainWindow
    {
        private bool optimizationUiSyncing;
        private bool optimizationPreloadTextSyncing;
        private CancellationTokenSource optimizationPreloadRefreshCts;
        private DateTime optimizationLastLowMemoryWarningUtc = DateTime.MinValue;
        private System.Windows.Forms.Timer optimizationRealtimeTimer;
        private bool optimizationRealtimeTickPending;

        private void InitializeOptimizationPageUx()
        {
            optimizationUiSyncing = true;
            try
            {
                if (optimizationModeComboBox.Items.Count == 0)
                {
                    optimizationModeComboBox.Items.AddRange(new object[]
                    {
                        "自动推荐（根据硬件）",
                        "自适应（自动动态调整）",
                        "简单调节（滑块）",
                        "高级调节（手动参数）"
                    });
                }

                if (optimizationPreloadModeComboBox.Items.Count == 0)
                {
                    optimizationPreloadModeComboBox.Items.AddRange(new object[]
                    {
                        "自动（推荐）",
                        "半自动（按数量上限）",
                        "始终预加载音频（按数量）",
                        "不预加载（播放后释放）",
                        "不预加载，但播放后不释放"
                    });
                }

                optimizationSimpleTrackBar.Minimum = 1;
                optimizationSimpleTrackBar.Maximum = 10;
                optimizationSimpleTrackBar.TickStyle = TickStyle.None;

                optimizationPreloadTrackBar.Minimum = 0;
                optimizationPreloadTrackBar.TickStyle = TickStyle.None;

                optimizationModeComboBox.SelectedIndex = 0;
                optimizationPreloadModeComboBox.SelectedIndex = 0;
                optimizationPreloadCountTextBox.Text = "0";
                optimizationRealtimeCheckBox.Checked = true;
            }
            finally
            {
                optimizationUiSyncing = false;
            }

            if (optimizationRealtimeTimer == null)
            {
                optimizationRealtimeTimer = new System.Windows.Forms.Timer
                {
                    Interval = 1000
                };
                optimizationRealtimeTimer.Tick += optimizationRealtimeTimer_Tick;
            }

            RefreshOptimizationPreloadRangeFromAudioCount();
            UpdateOptimizationSimpleValueLabel();
            ApplyOptimizationControlState();
            UpdateOptimizationCurrentLabel();
            QueueOptimizationPreloadRefresh(rebuildCache: true);
            StartOptimizationRealtimeUpdates();
        }

        private void ApplyPerformanceSettingsFromUser(UserSettings settings)
        {
            AudioPerformanceSettings next = AudioPerformanceSettings.CreateDefault();

            if (settings != null)
            {
                if (!string.IsNullOrWhiteSpace(settings.AudioPerformanceMode)
                    && Enum.TryParse(settings.AudioPerformanceMode, true, out AudioPerformanceMode mode))
                {
                    next.Mode = mode;
                }

                next.SimpleLevel = Math.Clamp(settings.AudioPerformanceSimpleLevel <= 0 ? 6 : settings.AudioPerformanceSimpleLevel, 1, 10);
                next.ManualPlaybackLatencyMs = Math.Clamp(settings.AudioManualPlaybackLatencyMs <= 0 ? 220 : settings.AudioManualPlaybackLatencyMs, 60, 1200);
                next.ManualBufferSeconds = Math.Clamp(settings.AudioManualBufferSeconds <= 0 ? 10 : settings.AudioManualBufferSeconds, 2, 120);
                next.ManualBuffersCount = Math.Clamp(settings.AudioManualBuffersCount <= 0 ? 4 : settings.AudioManualBuffersCount, 2, 32);
                next.UseAdvancedManual = settings.AudioManualAdvanced;

                if (!string.IsNullOrWhiteSpace(settings.AudioPreloadMode)
                    && Enum.TryParse(settings.AudioPreloadMode, true, out AudioPreloadMode preloadMode))
                {
                    next.PreloadMode = preloadMode;
                }

                int maxPreload = GetOptimizationAudioCount();
                next.PreloadCount = Math.Clamp(Math.Max(0, settings.AudioPreloadCount), 0, maxPreload);
            }

            AudioPlaybackService.ApplyPerformanceSettings(next);
            BindPerformanceSettingsToUi(next);
            if (optimizationRealtimeCheckBox != null)
            {
                optimizationUiSyncing = true;
                try
                {
                    optimizationRealtimeCheckBox.Checked = settings?.OptimizationRealtimeUpdate ?? true;
                }
                finally
                {
                    optimizationUiSyncing = false;
                }
            }

            QueueOptimizationPreloadRefresh(rebuildCache: true);
        }

        private void FillPerformanceSettingsToUser(UserSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            AudioPerformanceSettings snapshot = CollectPerformanceSettingsFromUi();
            settings.AudioPerformanceMode = snapshot.Mode.ToString();
            settings.AudioPerformanceSimpleLevel = snapshot.SimpleLevel;
            settings.AudioManualPlaybackLatencyMs = snapshot.ManualPlaybackLatencyMs;
            settings.AudioManualBufferSeconds = snapshot.ManualBufferSeconds;
            settings.AudioManualBuffersCount = snapshot.ManualBuffersCount;
            settings.AudioManualAdvanced = snapshot.UseAdvancedManual;
            settings.AudioPreloadMode = snapshot.PreloadMode.ToString();
            settings.AudioPreloadCount = snapshot.PreloadCount;
            settings.OptimizationRealtimeUpdate = optimizationRealtimeCheckBox?.Checked ?? true;
        }

        private AudioPerformanceSettings CollectPerformanceSettingsFromUi()
        {
            AudioPerformanceMode mode = GetPerformanceModeFromIndex(optimizationModeComboBox.SelectedIndex);
            AudioPreloadMode preloadMode = GetPreloadModeFromIndex(optimizationPreloadModeComboBox.SelectedIndex);

            int preloadCount = 0;
            if (preloadMode == AudioPreloadMode.AlwaysPreload || preloadMode == AudioPreloadMode.SemiAuto)
            {
                preloadCount = ParsePreloadCountFromTextBox();
            }

            return new AudioPerformanceSettings
            {
                Mode = mode,
                SimpleLevel = Math.Clamp(optimizationSimpleTrackBar.Value, 1, 10),
                ManualPlaybackLatencyMs = (int)optimizationLatencyNumeric.Value,
                ManualBufferSeconds = (int)optimizationBufferSecondsNumeric.Value,
                ManualBuffersCount = (int)optimizationBuffersCountNumeric.Value,
                UseAdvancedManual = mode == AudioPerformanceMode.ManualAdvanced,
                PreloadMode = preloadMode,
                PreloadCount = Math.Clamp(preloadCount, 0, GetOptimizationAudioCount())
            };
        }

        private void BindPerformanceSettingsToUi(AudioPerformanceSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            int maxPreload = GetOptimizationAudioCount();
            int preloadCount = Math.Clamp(settings.PreloadCount, 0, maxPreload);

            optimizationUiSyncing = true;
            optimizationPreloadTextSyncing = true;
            try
            {
                optimizationModeComboBox.SelectedIndex = GetPerformanceModeIndex(settings.Mode);
                optimizationSimpleTrackBar.Value = Math.Clamp(settings.SimpleLevel, optimizationSimpleTrackBar.Minimum, optimizationSimpleTrackBar.Maximum);

                optimizationLatencyNumeric.Value = Math.Clamp(settings.ManualPlaybackLatencyMs, (int)optimizationLatencyNumeric.Minimum, (int)optimizationLatencyNumeric.Maximum);
                optimizationBufferSecondsNumeric.Value = Math.Clamp(settings.ManualBufferSeconds, (int)optimizationBufferSecondsNumeric.Minimum, (int)optimizationBufferSecondsNumeric.Maximum);
                optimizationBuffersCountNumeric.Value = Math.Clamp(settings.ManualBuffersCount, (int)optimizationBuffersCountNumeric.Minimum, (int)optimizationBuffersCountNumeric.Maximum);

                optimizationPreloadModeComboBox.SelectedIndex = GetPreloadModeIndex(settings.PreloadMode);
                optimizationPreloadTrackBar.Maximum = maxPreload;
                optimizationPreloadTrackBar.Value = Math.Clamp(preloadCount, optimizationPreloadTrackBar.Minimum, optimizationPreloadTrackBar.Maximum);
                optimizationPreloadCountTextBox.Text = optimizationPreloadTrackBar.Value.ToString(CultureInfo.InvariantCulture);
            }
            finally
            {
                optimizationPreloadTextSyncing = false;
                optimizationUiSyncing = false;
            }

            RefreshOptimizationPreloadRangeFromAudioCount();
            UpdateOptimizationSimpleValueLabel();
            ApplyOptimizationControlState();
            UpdateOptimizationCurrentLabel();
        }

        private void optimizationModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (optimizationUiSyncing)
            {
                return;
            }

            ApplyOptimizationControlState();
            UpdateOptimizationCurrentLabel(usePendingSettings: true);
        }

        private void optimizationSimpleTrackBar_Scroll(object sender, EventArgs e)
        {
            UpdateOptimizationSimpleValueLabel();
            if (!optimizationUiSyncing)
            {
                UpdateOptimizationCurrentLabel(usePendingSettings: true);
            }
        }

        private void optimizationApplyButton_Click(object sender, EventArgs e)
        {
            ApplyOptimizationSettings(persist: true, rebuildPreloadCache: true);
        }

        private void optimizationPreloadModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (optimizationUiSyncing)
            {
                return;
            }

            ApplyOptimizationControlState();
            AudioPreloadMode mode = GetPreloadModeFromIndex(optimizationPreloadModeComboBox.SelectedIndex);
            if (mode != AudioPreloadMode.AlwaysPreload && mode != AudioPreloadMode.SemiAuto)
            {
                optimizationPreloadTextSyncing = true;
                try
                {
                    optimizationPreloadTrackBar.Value = 0;
                    optimizationPreloadCountTextBox.Text = "0";
                }
                finally
                {
                    optimizationPreloadTextSyncing = false;
                }
            }

            ApplyOptimizationSettings(persist: false, rebuildPreloadCache: true);
        }

        private void optimizationPreloadTrackBar_Scroll(object sender, EventArgs e)
        {
            if (optimizationPreloadTextSyncing)
            {
                return;
            }

            optimizationPreloadTextSyncing = true;
            try
            {
                optimizationPreloadCountTextBox.Text = optimizationPreloadTrackBar.Value.ToString(CultureInfo.InvariantCulture);
            }
            finally
            {
                optimizationPreloadTextSyncing = false;
            }

            ApplyOptimizationSettings(persist: false, rebuildPreloadCache: true);
        }

        private void optimizationPreloadCountTextBox_TextChanged(object sender, EventArgs e)
        {
            if (optimizationPreloadTextSyncing)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(optimizationPreloadCountTextBox.Text))
            {
                return;
            }

            if (!int.TryParse(optimizationPreloadCountTextBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return;
            }

            int clamped = Math.Clamp(value, 0, optimizationPreloadTrackBar.Maximum);
            optimizationPreloadTextSyncing = true;
            try
            {
                if (optimizationPreloadTrackBar.Value != clamped)
                {
                    optimizationPreloadTrackBar.Value = clamped;
                }

                if (clamped != value)
                {
                    optimizationPreloadCountTextBox.Text = clamped.ToString(CultureInfo.InvariantCulture);
                    optimizationPreloadCountTextBox.SelectionStart = optimizationPreloadCountTextBox.Text.Length;
                }
            }
            finally
            {
                optimizationPreloadTextSyncing = false;
            }

            ApplyOptimizationSettings(persist: false, rebuildPreloadCache: true);
        }

        private void optimizationRealtimeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (optimizationRealtimeCheckBox.Checked)
            {
                StartOptimizationRealtimeUpdates();
                QueueOptimizationPreloadRefresh(rebuildCache: false);
            }
            else
            {
                StopOptimizationRealtimeUpdates();
            }

            if (!optimizationUiSyncing)
            {
                SaveUserData();
            }
        }

        private void optimizationRealtimeTimer_Tick(object sender, EventArgs e)
        {
            if (optimizationRealtimeTickPending)
            {
                return;
            }

            optimizationRealtimeTickPending = true;
            try
            {
                QueueOptimizationPreloadRefresh(rebuildCache: false);
            }
            finally
            {
                optimizationRealtimeTickPending = false;
            }
        }

        private void optimizationPreloadApplyButton_Click(object sender, EventArgs e)
        {
            ApplyOptimizationSettings(persist: true, rebuildPreloadCache: true);
        }

        private void ApplyOptimizationSettings(bool persist, bool rebuildPreloadCache)
        {
            AudioPerformanceSettings settings = CollectPerformanceSettingsFromUi();
            AudioPlaybackService.ApplyPerformanceSettings(settings);
            UpdateOptimizationCurrentLabel();

            if (persist)
            {
                SaveUserData();
            }

            QueueOptimizationPreloadRefresh(rebuildPreloadCache);
        }

        private void QueueOptimizationPreloadRefresh(bool rebuildCache)
        {
            if (IsDisposed)
            {
                return;
            }

            CancellationTokenSource nextCts = new CancellationTokenSource();
            CancellationTokenSource oldCts = Interlocked.Exchange(ref optimizationPreloadRefreshCts, nextCts);
            if (oldCts != null)
            {
                oldCts.Cancel();
                oldCts.Dispose();
            }

            _ = RefreshOptimizationPreloadSummaryAsync(rebuildCache, nextCts.Token);
        }

        private async Task RefreshOptimizationPreloadSummaryAsync(bool rebuildCache, CancellationToken cancellationToken)
        {
            try
            {
                RefreshOptimizationPreloadRangeFromAudioCount();
                List<AudioInfo> audioSnapshot = BuildOptimizationAudioSnapshot();

                AudioPreloadSummary preloadSummary;
                if (rebuildCache)
                {
                    preloadSummary = await AudioPlaybackService.RebuildPreloadCacheAsync(audioSnapshot, selectedAudioPath, cancellationToken);
                }
                else
                {
                    AudioMemorySnapshot current = AudioPlaybackService.GetMemorySnapshot();
                    preloadSummary = new AudioPreloadSummary
                    {
                        RequestedCount = ParsePreloadCountFromTextBox(),
                        PreloadedCount = 0,
                        PreloadedBytes = current.PreloadedBytes,
                        LowMemoryRisk = false,
                        Warning = string.Empty
                    };
                }

                cancellationToken.ThrowIfCancellationRequested();
                AudioMemorySnapshot memorySnapshot = AudioPlaybackService.GetMemorySnapshot();
                double processMb = ToMb(memorySnapshot.PrivateWorkingSetBytes > 0
                    ? memorySnapshot.PrivateWorkingSetBytes
                    : memorySnapshot.ProcessWorkingSetBytes);
                double workingSetMb = ToMb(memorySnapshot.ProcessWorkingSetBytes);
                double preloadMb = ToMb(memorySnapshot.PreloadedBytes);
                double availableMb = ToMb(memorySnapshot.AvailableSystemMemoryBytes);

                optimizationMemoryLabel.Text =
                    $"MM 内存占用：{processMb:0.00} MB | 工作集：{workingSetMb:0.00} MB | 预加载缓存估算：{preloadMb:0.00} MB（已计入MM占用） | 系统剩余内存：{availableMb:0.00} MB";

                if (preloadSummary.LowMemoryRisk)
                {
                    optimizationMemoryLabel.ForeColor = Color.FromArgb(178, 34, 34);
                    if ((DateTime.UtcNow - optimizationLastLowMemoryWarningUtc).TotalSeconds > 15)
                    {
                        optimizationLastLowMemoryWarningUtc = DateTime.UtcNow;
                        MessageBox.Show(
                            string.IsNullOrWhiteSpace(preloadSummary.Warning)
                                ? "系统可用内存偏低，建议降低预加载数量。"
                                : preloadSummary.Warning,
                            "预加载提示",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    optimizationMemoryLabel.ForeColor = Color.FromArgb(30, 30, 30);
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore canceled refreshes.
            }
            catch (Exception ex)
            {
                optimizationMemoryLabel.ForeColor = Color.FromArgb(178, 34, 34);
                optimizationMemoryLabel.Text = $"读取预加载信息失败：{ex.Message}";
            }
        }

        private void RefreshOptimizationPreloadRangeFromAudioCount()
        {
            if (optimizationPreloadTrackBar == null)
            {
                return;
            }

            int max = GetOptimizationAudioCount();
            optimizationPreloadTrackBar.Maximum = max;
            if (optimizationPreloadTrackBar.Value > max)
            {
                optimizationPreloadTrackBar.Value = max;
            }

            if (!int.TryParse(optimizationPreloadCountTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int input))
            {
                input = optimizationPreloadTrackBar.Value;
            }

            int clamped = Math.Clamp(input, 0, max);
            optimizationPreloadTextSyncing = true;
            try
            {
                optimizationPreloadCountTextBox.Text = clamped.ToString(CultureInfo.InvariantCulture);
            }
            finally
            {
                optimizationPreloadTextSyncing = false;
            }
        }

        private void ApplyOptimizationControlState()
        {
            AudioPerformanceMode mode = GetPerformanceModeFromIndex(optimizationModeComboBox.SelectedIndex);
            bool simpleMode = mode == AudioPerformanceMode.ManualSimple;
            bool advancedMode = mode == AudioPerformanceMode.ManualAdvanced;

            optimizationSimpleTrackBar.Enabled = simpleMode;
            optimizationSimpleValueLabel.Enabled = simpleMode;
            optimizationLatencyNumeric.Enabled = advancedMode;
            optimizationBufferSecondsNumeric.Enabled = advancedMode;
            optimizationBuffersCountNumeric.Enabled = advancedMode;

            AudioPreloadMode preloadMode = GetPreloadModeFromIndex(optimizationPreloadModeComboBox.SelectedIndex);
            bool enablePreloadCount = preloadMode == AudioPreloadMode.AlwaysPreload
                || preloadMode == AudioPreloadMode.SemiAuto;
            optimizationPreloadTrackBar.Enabled = enablePreloadCount;
            optimizationPreloadCountTextBox.Enabled = enablePreloadCount;
        }

        private void UpdateOptimizationSimpleValueLabel()
        {
            optimizationSimpleValueLabel.Text = $"当前: {optimizationSimpleTrackBar.Value}";
        }

        private void UpdateOptimizationCurrentLabel(bool usePendingSettings = false)
        {
            AudioPerformanceSettings snapshot = usePendingSettings
                ? CollectPerformanceSettingsFromUi()
                : AudioPlaybackService.GetPerformanceSettings();

            AudioBufferRuntimeOptions runtime = ResolvePreviewRuntime(snapshot);
            string modeText = snapshot.Mode switch
            {
                AudioPerformanceMode.AutoRecommended => "自动推荐",
                AudioPerformanceMode.Adaptive => "自适应",
                AudioPerformanceMode.ManualSimple => "简单调节",
                AudioPerformanceMode.ManualAdvanced => "高级调节",
                _ => "未知"
            };

            string prefix = usePendingSettings ? "待应用参数" : "当前参数";
            optimizationCurrentLabel.Text =
                $"{prefix}：模式 {modeText} | 延迟 {runtime.PlaybackLatencyMs} ms | 缓冲 {runtime.BufferSeconds} s | 缓冲块 {runtime.BuffersCount}";
        }

        private static AudioBufferRuntimeOptions ResolvePreviewRuntime(AudioPerformanceSettings snapshot)
        {
            if (snapshot == null)
            {
                return AudioPlaybackService.GetRecommendedRuntimeOptions();
            }

            int simpleLevel = Math.Clamp(snapshot.SimpleLevel, 1, 10);
            AudioBufferRuntimeOptions recommended = AudioPlaybackService.GetRecommendedRuntimeOptions();

            return snapshot.Mode switch
            {
                AudioPerformanceMode.ManualSimple => new AudioBufferRuntimeOptions(
                    120 + simpleLevel * 24,
                    4 + simpleLevel,
                    3 + (simpleLevel - 1) / 3),
                AudioPerformanceMode.ManualAdvanced => new AudioBufferRuntimeOptions(
                    snapshot.ManualPlaybackLatencyMs,
                    snapshot.ManualBufferSeconds,
                    snapshot.ManualBuffersCount),
                _ => recommended
            };
        }

        private int ParsePreloadCountFromTextBox()
        {
            if (!int.TryParse(optimizationPreloadCountTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                value = optimizationPreloadTrackBar.Value;
            }

            return Math.Clamp(value, 0, GetOptimizationAudioCount());
        }

        private int GetOptimizationAudioCount()
        {
            int count = allAudioLibraryItems?.Count ?? 0;
            if (count <= 0)
            {
                count = audioInfo?.Count ?? 0;
            }

            return Math.Max(0, count);
        }

        private List<AudioInfo> BuildOptimizationAudioSnapshot()
        {
            IEnumerable<AudioInfo> source = (allAudioLibraryItems != null && allAudioLibraryItems.Count > 0)
                ? allAudioLibraryItems
                : audioInfo;

            return source
                .Where(item => item != null)
                .Select(item => new AudioInfo
                {
                    Name = item.Name,
                    Track = item.Track,
                    FileType = item.FileType,
                    FilePath = item.FilePath,
                    Key = item.Key
                })
                .ToList();
        }

        private static double ToMb(long bytes)
        {
            return bytes / 1024d / 1024d;
        }

        private static AudioPerformanceMode GetPerformanceModeFromIndex(int index)
        {
            return index switch
            {
                1 => AudioPerformanceMode.Adaptive,
                2 => AudioPerformanceMode.ManualSimple,
                3 => AudioPerformanceMode.ManualAdvanced,
                _ => AudioPerformanceMode.AutoRecommended
            };
        }

        private static int GetPerformanceModeIndex(AudioPerformanceMode mode)
        {
            return mode switch
            {
                AudioPerformanceMode.Adaptive => 1,
                AudioPerformanceMode.ManualSimple => 2,
                AudioPerformanceMode.ManualAdvanced => 3,
                _ => 0
            };
        }

        private static AudioPreloadMode GetPreloadModeFromIndex(int index)
        {
            return index switch
            {
                0 => AudioPreloadMode.Auto,
                1 => AudioPreloadMode.SemiAuto,
                2 => AudioPreloadMode.AlwaysPreload,
                4 => AudioPreloadMode.KeepAfterPlayback,
                _ => AudioPreloadMode.NoneReleaseAfterPlayback
            };
        }

        private static int GetPreloadModeIndex(AudioPreloadMode mode)
        {
            return mode switch
            {
                AudioPreloadMode.Auto => 0,
                AudioPreloadMode.SemiAuto => 1,
                AudioPreloadMode.AlwaysPreload => 2,
                AudioPreloadMode.KeepAfterPlayback => 4,
                _ => 3
            };
        }

        private void StartOptimizationRealtimeUpdates()
        {
            if (optimizationRealtimeTimer == null || !optimizationRealtimeCheckBox.Checked)
            {
                return;
            }

            optimizationRealtimeTimer.Start();
        }

        internal void StopOptimizationRealtimeUpdates()
        {
            optimizationRealtimeTimer?.Stop();
        }
    }
}
