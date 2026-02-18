using System;

namespace MusicalMoments
{
    internal enum AudioPerformanceMode
    {
        AutoRecommended = 0,
        Adaptive = 1,
        ManualSimple = 2,
        ManualAdvanced = 3
    }

    internal enum AudioPreloadMode
    {
        NoneReleaseAfterPlayback = 0,
        AlwaysPreload = 1,
        KeepAfterPlayback = 2,
        Auto = 3,
        SemiAuto = 4
    }

    internal readonly struct AudioBufferRuntimeOptions
    {
        public AudioBufferRuntimeOptions(int playbackLatencyMs, int bufferSeconds, int buffersCount)
        {
            PlaybackLatencyMs = Math.Clamp(playbackLatencyMs, 60, 1200);
            BufferSeconds = Math.Clamp(bufferSeconds, 2, 120);
            BuffersCount = Math.Clamp(buffersCount, 2, 32);
        }

        public int PlaybackLatencyMs { get; }
        public int BufferSeconds { get; }
        public int BuffersCount { get; }
    }

    internal sealed class AudioPerformanceSettings
    {
        public AudioPerformanceMode Mode { get; set; } = AudioPerformanceMode.AutoRecommended;
        public int SimpleLevel { get; set; } = 6;
        public int ManualPlaybackLatencyMs { get; set; } = 220;
        public int ManualBufferSeconds { get; set; } = 10;
        public int ManualBuffersCount { get; set; } = 4;
        public bool UseAdvancedManual { get; set; }
        public AudioPreloadMode PreloadMode { get; set; } = AudioPreloadMode.Auto;
        public int PreloadCount { get; set; }

        public static AudioPerformanceSettings CreateDefault()
        {
            return new AudioPerformanceSettings();
        }

        public AudioPerformanceSettings Clone()
        {
            return new AudioPerformanceSettings
            {
                Mode = Mode,
                SimpleLevel = SimpleLevel,
                ManualPlaybackLatencyMs = ManualPlaybackLatencyMs,
                ManualBufferSeconds = ManualBufferSeconds,
                ManualBuffersCount = ManualBuffersCount,
                UseAdvancedManual = UseAdvancedManual,
                PreloadMode = PreloadMode,
                PreloadCount = PreloadCount
            };
        }
    }

    internal readonly struct AudioMemorySnapshot
    {
        public AudioMemorySnapshot(
            long processWorkingSetBytes,
            long privateWorkingSetBytes,
            long preloadedBytes,
            long availableSystemMemoryBytes)
        {
            ProcessWorkingSetBytes = Math.Max(0, processWorkingSetBytes);
            PrivateWorkingSetBytes = Math.Max(0, privateWorkingSetBytes);
            PreloadedBytes = Math.Max(0, preloadedBytes);
            AvailableSystemMemoryBytes = Math.Max(0, availableSystemMemoryBytes);
        }

        public long ProcessWorkingSetBytes { get; }
        public long PrivateWorkingSetBytes { get; }
        public long PreloadedBytes { get; }
        public long AvailableSystemMemoryBytes { get; }
    }

    internal sealed class AudioPreloadSummary
    {
        public int RequestedCount { get; init; }
        public int PreloadedCount { get; init; }
        public long PreloadedBytes { get; init; }
        public bool LowMemoryRisk { get; init; }
        public string Warning { get; init; } = string.Empty;
    }

    internal sealed class AudioPlaybackWarningEventArgs : EventArgs
    {
        public AudioPlaybackWarningEventArgs(string title, string message)
        {
            Title = string.IsNullOrWhiteSpace(title) ? "提示" : title;
            Message = string.IsNullOrWhiteSpace(message) ? "发生未知问题。" : message;
        }

        public string Title { get; }
        public string Message { get; }
    }
}
