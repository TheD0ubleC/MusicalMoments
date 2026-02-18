using Microsoft.VisualBasic.Devices;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class AudioPlaybackService
    {
        public static event EventHandler<AudioPlaybackWarningEventArgs> PlaybackWarningRaised;
        public static event EventHandler PlaybackCompleted;

        public static WaveOutEvent CurrentOutputDevice { get; private set; }
        public static AudioFileReader CurrentAudioFile { get; private set; }

        private static readonly object playbackLock = new object();
        private static readonly object cacheLock = new object();
        private static readonly Dictionary<string, CachedPcmAudio> pcmCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly WaveFormat targetFormat = new WaveFormat(48000, 16, 2);
        private static readonly AudioPerformanceSettings settings = AudioPerformanceSettings.CreateDefault();

        private static PlaybackSession primarySession;
        private static PlaybackSession secondarySession;

        private static WaveOutEvent tipOutput;
        private static AudioFileReader tipAudio;

        private static WaveInEvent waveIn;
        private static WaveOutEvent waveOut;
        private static BufferedWaveProvider loopbackBuffer;

        private static AudioBufferRuntimeOptions recommendedOptions = BuildRecommendedOptions();
        private static DateTime optionsTimestampUtc = DateTime.MinValue;
        private static int adaptivePenalty;
        private static DateTime lastWarningAtUtc = DateTime.MinValue;

        private static DateTime ioProbeTimestampUtc = DateTime.MinValue;
        private static double ioProbeMbPerSecond;

        public static bool IsPlaybackActive
        {
            get
            {
                lock (playbackLock)
                {
                    return IsSessionPlaying(primarySession)
                        || IsSessionPlaying(secondarySession)
                        || tipOutput?.PlaybackState == PlaybackState.Playing;
                }
            }
        }

        public static AudioPerformanceSettings GetPerformanceSettings()
        {
            lock (playbackLock)
            {
                return settings.Clone();
            }
        }

        public static void ApplyPerformanceSettings(AudioPerformanceSettings next)
        {
            if (next == null)
            {
                return;
            }

            lock (playbackLock)
            {
                settings.Mode = next.Mode;
                settings.SimpleLevel = Math.Clamp(next.SimpleLevel, 1, 10);
                settings.ManualPlaybackLatencyMs = Math.Clamp(next.ManualPlaybackLatencyMs, 60, 1200);
                settings.ManualBufferSeconds = Math.Clamp(next.ManualBufferSeconds, 2, 120);
                settings.ManualBuffersCount = Math.Clamp(next.ManualBuffersCount, 2, 32);
                settings.UseAdvancedManual = next.UseAdvancedManual;
                settings.PreloadMode = next.PreloadMode;
                settings.PreloadCount = Math.Max(0, next.PreloadCount);
                if (settings.Mode != AudioPerformanceMode.Adaptive)
                {
                    adaptivePenalty = 0;
                }

                recommendedOptions = BuildRecommendedOptions();
                optionsTimestampUtc = DateTime.UtcNow;
            }

            ApplyCacheRetentionPolicy();
        }

        public static AudioBufferRuntimeOptions GetRecommendedRuntimeOptions()
        {
            lock (playbackLock)
            {
                if ((DateTime.UtcNow - optionsTimestampUtc).TotalSeconds > 30)
                {
                    recommendedOptions = BuildRecommendedOptions();
                    optionsTimestampUtc = DateTime.UtcNow;
                }

                return recommendedOptions;
            }
        }

        public static void PlayAudioToSpecificDevice(
            string audioPath,
            int deviceNumber,
            bool stopCurrent,
            float volume,
            bool relayPlayback,
            string relayAudioPath,
            int relayDeviceNumber,
            float relayVolume)
        {
            if (string.IsNullOrWhiteSpace(audioPath) || !File.Exists(audioPath))
            {
                Warn("播放失败", "音频文件不存在，请刷新音频列表后重试。");
                return;
            }

            try
            {
                byte[] primaryPcm = GetOrDecodePcm(audioPath, markPreloaded: false);
                byte[] secondaryPcm = null;
                if (relayPlayback)
                {
                    string relayPath = string.IsNullOrWhiteSpace(relayAudioPath) ? audioPath : relayAudioPath;
                    if (File.Exists(relayPath))
                    {
                        secondaryPcm = string.Equals(audioPath, relayPath, StringComparison.OrdinalIgnoreCase)
                            ? primaryPcm
                            : GetOrDecodePcm(relayPath, markPreloaded: false);
                    }
                }

                lock (playbackLock)
                {
                    AudioBufferRuntimeOptions runtime = ResolveRuntimeOptionsLocked();
                    if (stopCurrent)
                    {
                        DisposeMainSessionsLocked();
                    }

                    primarySession = CreateAndStartSession(
                        primaryPcm,
                        deviceNumber,
                        volume,
                        runtime,
                        isPrimary: true,
                        channelName: "VB 输出");

                    if (relayPlayback && secondaryPcm != null)
                    {
                        secondarySession = CreateAndStartSession(
                            secondaryPcm,
                            relayDeviceNumber,
                            relayVolume,
                            runtime,
                            isPrimary: false,
                            channelName: "物理输出");
                    }
                    else
                    {
                        DisposeSecondarySessionLocked();
                    }

                    CurrentOutputDevice = primarySession?.Output;
                    CurrentAudioFile = null;
                    adaptivePenalty = Math.Max(0, adaptivePenalty - 1);
                }
            }
            catch (Exception ex)
            {
                adaptivePenalty = Math.Clamp(adaptivePenalty + 1, 0, 12);
                Warn("播放失败", $"播放过程中出现错误：{ex.Message}");
            }

            ApplyCacheRetentionPolicy();
        }

        public static void PlayAudioEx(string audioPath, int deviceNumber, float volume)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(audioPath) || !File.Exists(audioPath))
                {
                    Warn("提示音播放失败", "提示音文件不存在。");
                    return;
                }

                AudioBufferRuntimeOptions runtime = GetRecommendedRuntimeOptions();
                StopTipPlayback();

                tipAudio = new AudioFileReader(audioPath);
                tipOutput = new WaveOutEvent
                {
                    DeviceNumber = deviceNumber,
                    DesiredLatency = runtime.PlaybackLatencyMs,
                    NumberOfBuffers = Math.Clamp(runtime.BuffersCount, 2, 8),
                    Volume = Math.Clamp(volume, 0f, 1f)
                };
                tipOutput.PlaybackStopped += (_, _) => StopTipPlayback();
                tipOutput.Init(tipAudio);
                tipOutput.Play();
            }
            catch (Exception ex)
            {
                Warn("提示音播放失败", ex.Message);
            }
        }

        public static void StopCurrentPlayback()
        {
            lock (playbackLock)
            {
                DisposeMainSessionsLocked();
            }

            StopTipPlayback();
            CurrentOutputDevice = null;
            CurrentAudioFile = null;
            RaisePlaybackCompleted();
            ApplyCacheRetentionPolicy();
        }

        public static void StartMicrophoneToSpeaker(int inputDeviceIndex, int outputDeviceIndex)
        {
            StopMicrophoneToSpeaker();

            AudioBufferRuntimeOptions runtime = GetRecommendedRuntimeOptions();
            waveIn = new WaveInEvent
            {
                DeviceNumber = inputDeviceIndex,
                WaveFormat = targetFormat,
                BufferMilliseconds = Math.Clamp(runtime.PlaybackLatencyMs, 60, 450)
            };

            waveOut = new WaveOutEvent
            {
                DeviceNumber = outputDeviceIndex,
                DesiredLatency = runtime.PlaybackLatencyMs,
                NumberOfBuffers = Math.Clamp(runtime.BuffersCount, 2, 8)
            };

            loopbackBuffer = new BufferedWaveProvider(targetFormat)
            {
                BufferDuration = TimeSpan.FromSeconds(Math.Clamp(runtime.BufferSeconds, 2, 30)),
                ReadFully = true,
                DiscardOnBufferOverflow = true
            };

            waveIn.DataAvailable += (_, e) =>
            {
                try
                {
                    loopbackBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
                }
                catch (Exception ex)
                {
                    Warn("麦克风转发异常", BuildReadableErrorMessage("麦克风缓冲区写入失败", ex.Message));
                }
            };

            waveOut.Init(loopbackBuffer);
            waveIn.StartRecording();
            waveOut.Play();
        }

        public static void StopMicrophoneToSpeaker()
        {
            try
            {
                waveIn?.StopRecording();
            }
            catch
            {
                // Ignore stop failures.
            }

            waveIn?.Dispose();
            waveIn = null;

            try
            {
                waveOut?.Stop();
            }
            catch
            {
                // Ignore stop failures.
            }

            waveOut?.Dispose();
            waveOut = null;
            loopbackBuffer = null;
        }

        public static string DisplayAudioProperties(string audioPath)
        {
            try
            {
                using AudioFileReader audio = new AudioFileReader(audioPath);
                return $"格式:{Path.GetExtension(audioPath).TrimStart('.').ToUpperInvariant()} | 采样率:{audio.WaveFormat.SampleRate} Hz | 位深:{audio.WaveFormat.BitsPerSample} bit | 声道:{audio.WaveFormat.Channels}";
            }
            catch
            {
                return "无法读取音频属性。";
            }
        }

        public static async Task<AudioPreloadSummary> RebuildPreloadCacheAsync(
            IReadOnlyList<AudioInfo> allAudioItems,
            string selectedAudioPath,
            CancellationToken cancellationToken)
        {
            AudioPerformanceSettings snapshot = GetPerformanceSettings();
            int requested = Math.Max(0, snapshot.PreloadCount);
            bool preloadEnabled = snapshot.PreloadMode == AudioPreloadMode.Auto
                || snapshot.PreloadMode == AudioPreloadMode.SemiAuto
                || snapshot.PreloadMode == AudioPreloadMode.AlwaysPreload;

            if (!preloadEnabled || allAudioItems == null || allAudioItems.Count == 0)
            {
                lock (cacheLock)
                {
                    foreach (CachedPcmAudio cache in pcmCache.Values)
                    {
                        cache.IsPreloaded = false;
                    }
                }

                ApplyCacheRetentionPolicy();
                return BuildPreloadSummary(0, 0);
            }

            List<string> candidates = BuildPreloadCandidates(allAudioItems, selectedAudioPath);
            int targetCount = ResolvePreloadTargetCount(snapshot, requested, candidates);
            int summaryRequested = snapshot.PreloadMode == AudioPreloadMode.Auto ? targetCount : requested;
            lock (cacheLock)
            {
                foreach (CachedPcmAudio cache in pcmCache.Values)
                {
                    cache.IsPreloaded = false;
                }
            }

            int loaded = 0;
            for (int index = 0; index < targetCount; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string path = candidates[index];
                try
                {
                    await Task.Run(() => GetOrDecodePcm(path, markPreloaded: true), cancellationToken);
                    loaded++;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Warn("预加载提示", $"预加载失败：{Path.GetFileName(path)}，{ex.Message}");
                }
            }

            ApplyCacheRetentionPolicy();
            return BuildPreloadSummary(summaryRequested, loaded);
        }

        public static async Task PrimeHotCacheAsync(
            IEnumerable<string> candidatePaths,
            int maxCount,
            CancellationToken cancellationToken)
        {
            if (candidatePaths == null || maxCount <= 0)
            {
                return;
            }

            HashSet<string> dedup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> targets = new List<string>();
            foreach (string path in candidatePaths)
            {
                if (targets.Count >= maxCount)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    continue;
                }

                string fullPath = Path.GetFullPath(path);
                if (dedup.Add(fullPath))
                {
                    targets.Add(fullPath);
                }
            }

            foreach (string path in targets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await Task.Run(() => GetOrDecodePcm(path, markPreloaded: true), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    // Ignore hot-prime failures to keep key dispatch responsive.
                }
            }

            ApplyCacheRetentionPolicy();
        }

        public static AudioMemorySnapshot GetMemorySnapshot()
        {
            long available = 0;
            try
            {
                available = (long)Math.Min(long.MaxValue, new ComputerInfo().AvailablePhysicalMemory);
            }
            catch
            {
                // Ignore memory-read failures.
            }

            long preloaded = 0;
            lock (cacheLock)
            {
                preloaded = pcmCache.Values.Where(item => item.IsPreloaded).Sum(item => item.PcmData.LongLength);
            }

            long workingSet = 0;
            long privateWorkingSet = 0;
            try
            {
                Process current = Process.GetCurrentProcess();
                current.Refresh();
                workingSet = current.WorkingSet64;
                privateWorkingSet = current.PrivateMemorySize64;
            }
            catch
            {
                // Ignore process-read failures.
            }

            return new AudioMemorySnapshot(workingSet, privateWorkingSet, preloaded, available);
        }

        private static bool IsSessionPlaying(PlaybackSession session)
        {
            return session != null
                && session.Output != null
                && session.Output.PlaybackState == PlaybackState.Playing;
        }

        private static PlaybackSession CreateAndStartSession(
            byte[] pcm,
            int deviceNumber,
            float volume,
            AudioBufferRuntimeOptions runtime,
            bool isPrimary,
            string channelName)
        {
            if (pcm == null || pcm.Length <= 0)
            {
                throw new InvalidOperationException("PCM 数据为空，无法播放。");
            }

            PlaybackSession oldSession = isPrimary ? primarySession : secondarySession;
            oldSession?.Dispose();
            if (isPrimary)
            {
                primarySession = null;
            }
            else
            {
                secondarySession = null;
            }

            MemoryStream pcmStream = new MemoryStream(pcm, writable: false);
            RawSourceWaveStream sourceWave = new RawSourceWaveStream(pcmStream, targetFormat);
            WaveOutEvent output = new WaveOutEvent
            {
                DeviceNumber = deviceNumber,
                DesiredLatency = runtime.PlaybackLatencyMs,
                NumberOfBuffers = Math.Clamp(runtime.BuffersCount, 2, 12),
                Volume = Math.Clamp(volume, 0f, 1f)
            };

            PlaybackSession session = new PlaybackSession(output, sourceWave, pcmStream, channelName);
            output.PlaybackStopped += (_, e) => HandleSessionStopped(session, isPrimary, e);

            try
            {
                output.Init(sourceWave);
                output.Play();
            }
            catch (Exception ex)
            {
                session.Dispose();
                adaptivePenalty = Math.Clamp(adaptivePenalty + 1, 0, 12);
                throw new InvalidOperationException($"{channelName}设备启动失败：{ex.Message}", ex);
            }

            return session;
        }

        private static void HandleSessionStopped(PlaybackSession session, bool isPrimary, StoppedEventArgs args)
        {
            bool shouldRaise = false;
            lock (playbackLock)
            {
                if (isPrimary)
                {
                    if (ReferenceEquals(primarySession, session))
                    {
                        primarySession = null;
                    }
                }
                else
                {
                    if (ReferenceEquals(secondarySession, session))
                    {
                        secondarySession = null;
                    }
                }

                if (primarySession == null && secondarySession == null)
                {
                    CurrentOutputDevice = null;
                    CurrentAudioFile = null;
                    shouldRaise = true;
                }
            }

            session.Dispose();

            if (args?.Exception != null)
            {
                adaptivePenalty = Math.Clamp(adaptivePenalty + 1, 0, 12);
                Warn("播放异常", BuildReadableErrorMessage($"{session.ChannelName}播放中断", args.Exception.Message));
            }
            else
            {
                adaptivePenalty = Math.Max(0, adaptivePenalty - 1);
            }

            if (shouldRaise)
            {
                RaisePlaybackCompleted();
                ApplyCacheRetentionPolicy();
            }
        }

        private static void DisposeMainSessionsLocked()
        {
            DisposePrimarySessionLocked();
            DisposeSecondarySessionLocked();
        }

        private static void DisposePrimarySessionLocked()
        {
            PlaybackSession session = primarySession;
            primarySession = null;
            session?.Dispose();
        }

        private static void DisposeSecondarySessionLocked()
        {
            PlaybackSession session = secondarySession;
            secondarySession = null;
            session?.Dispose();
        }

        private static void StopTipPlayback()
        {
            try
            {
                tipOutput?.Stop();
            }
            catch
            {
                // Ignore stop failures.
            }

            tipOutput?.Dispose();
            tipAudio?.Dispose();
            tipOutput = null;
            tipAudio = null;
        }

        private static List<string> BuildPreloadCandidates(IReadOnlyList<AudioInfo> allAudioItems, string selectedAudioPath)
        {
            HashSet<string> dedup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> result = new List<string>();

            void Add(string path)
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    return;
                }

                string fullPath = Path.GetFullPath(path);
                if (dedup.Add(fullPath))
                {
                    result.Add(fullPath);
                }
            }

            Add(selectedAudioPath);
            foreach (AudioInfo item in allAudioItems.Where(item => item != null && item.Key != Keys.None))
            {
                Add(item.FilePath);
            }

            foreach (AudioInfo item in allAudioItems.Where(item => item != null && item.Key == Keys.None))
            {
                Add(item.FilePath);
            }

            return result;
        }

        private static AudioPreloadSummary BuildPreloadSummary(int requestedCount, int preloadedCount)
        {
            AudioMemorySnapshot snapshot = GetMemorySnapshot();
            bool lowMemory = snapshot.AvailableSystemMemoryBytes > 0
                && (snapshot.AvailableSystemMemoryBytes < 1_200_000_000L
                    || snapshot.PreloadedBytes > snapshot.AvailableSystemMemoryBytes * 0.45);

            return new AudioPreloadSummary
            {
                RequestedCount = requestedCount,
                PreloadedCount = preloadedCount,
                PreloadedBytes = snapshot.PreloadedBytes,
                LowMemoryRisk = lowMemory,
                Warning = lowMemory
                    ? "当前系统可用内存偏低，建议减少预加载数量或切换到“播放后释放”。"
                    : string.Empty
            };
        }

        private static int ResolvePreloadTargetCount(AudioPerformanceSettings snapshot, int requested, IReadOnlyList<string> candidates)
        {
            if (snapshot == null || candidates == null || candidates.Count == 0)
            {
                return 0;
            }

            return snapshot.PreloadMode switch
            {
                AudioPreloadMode.Auto => CalculateAutoPreloadCount(candidates, int.MaxValue),
                AudioPreloadMode.SemiAuto => CalculateAutoPreloadCount(candidates, requested),
                AudioPreloadMode.AlwaysPreload => Math.Clamp(requested, 0, candidates.Count),
                _ => 0
            };
        }

        private static int CalculateAutoPreloadCount(IReadOnlyList<string> candidates, int maxAllowed)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return 0;
            }

            AudioMemorySnapshot memory = GetMemorySnapshot();
            long estimatedPerItem = EstimateAverageDecodedBytes(candidates);
            if (estimatedPerItem <= 0)
            {
                estimatedPerItem = 8 * 1024 * 1024;
            }

            long reserveBytes = Math.Max(900L * 1024 * 1024, memory.AvailableSystemMemoryBytes / 3);
            long usableBytes = memory.AvailableSystemMemoryBytes > 0
                ? Math.Max(0, memory.AvailableSystemMemoryBytes - reserveBytes)
                : 0;

            int byMemory = usableBytes > 0
                ? (int)Math.Clamp(usableBytes / estimatedPerItem, 0, candidates.Count)
                : Math.Min(candidates.Count, 6);
            int byCpu = Environment.ProcessorCount switch
            {
                <= 4 => 10,
                <= 8 => 18,
                _ => 28
            };

            int candidate = Math.Min(candidates.Count, Math.Min(byMemory, byCpu));
            if (candidate <= 0 && candidates.Count > 0 && memory.AvailableSystemMemoryBytes > 2L * 1024 * 1024 * 1024)
            {
                candidate = 1;
            }

            if (maxAllowed < int.MaxValue)
            {
                candidate = Math.Min(candidate, Math.Max(0, maxAllowed));
            }

            return Math.Clamp(candidate, 0, candidates.Count);
        }

        private static long EstimateAverageDecodedBytes(IReadOnlyList<string> candidates)
        {
            lock (cacheLock)
            {
                if (pcmCache.Count > 0)
                {
                    long fromCache = (long)pcmCache.Values
                        .Take(16)
                        .Select(value => value.PcmData?.LongLength ?? 0L)
                        .Where(length => length > 0)
                        .DefaultIfEmpty(0L)
                        .Average();
                    if (fromCache > 0)
                    {
                        return fromCache;
                    }
                }
            }

            int sample = Math.Min(10, candidates.Count);
            long total = 0;
            int count = 0;
            for (int i = 0; i < sample; i++)
            {
                string path = candidates[i];
                try
                {
                    long length = new FileInfo(path).Length;
                    long estimate = Math.Clamp(length * 6, 2_500_000L, 32_000_000L);
                    total += estimate;
                    count++;
                }
                catch
                {
                    // Ignore file metadata errors and keep sampling.
                }
            }

            if (count <= 0)
            {
                return 8 * 1024 * 1024;
            }

            return Math.Max(2_500_000L, total / count);
        }

        private static AudioBufferRuntimeOptions ResolveRuntimeOptionsLocked()
        {
            if ((DateTime.UtcNow - optionsTimestampUtc).TotalSeconds > 30)
            {
                recommendedOptions = BuildRecommendedOptions();
                optionsTimestampUtc = DateTime.UtcNow;
            }

            int simpleLevel = Math.Clamp(settings.SimpleLevel, 1, 10);
            return settings.Mode switch
            {
                AudioPerformanceMode.Adaptive => new AudioBufferRuntimeOptions(
                    recommendedOptions.PlaybackLatencyMs + adaptivePenalty * 35,
                    recommendedOptions.BufferSeconds + adaptivePenalty,
                    recommendedOptions.BuffersCount + adaptivePenalty / 2),
                AudioPerformanceMode.ManualSimple => new AudioBufferRuntimeOptions(
                    120 + simpleLevel * 24,
                    4 + simpleLevel,
                    3 + (simpleLevel - 1) / 3),
                AudioPerformanceMode.ManualAdvanced => new AudioBufferRuntimeOptions(
                    settings.ManualPlaybackLatencyMs,
                    settings.ManualBufferSeconds,
                    settings.ManualBuffersCount),
                _ => recommendedOptions
            };
        }

        private static AudioBufferRuntimeOptions BuildRecommendedOptions()
        {
            int cpuCores = Math.Max(1, Environment.ProcessorCount);

            double availableMemoryGb = 0;
            double totalMemoryGb = 0;
            try
            {
                ComputerInfo info = new ComputerInfo();
                availableMemoryGb = info.AvailablePhysicalMemory / 1024d / 1024d / 1024d;
                totalMemoryGb = info.TotalPhysicalMemory / 1024d / 1024d / 1024d;
            }
            catch
            {
                // Ignore memory probe failures.
            }

            double ioMbPerSecond = GetDiskProbeResultMb();

            int latency = 160;
            int seconds = 8;
            int buffers = 4;

            if (cpuCores <= 4)
            {
                latency += 70;
                buffers += 1;
            }
            else if (cpuCores <= 8)
            {
                latency += 20;
            }

            if (availableMemoryGb > 0 && availableMemoryGb < 6)
            {
                latency += 40;
                seconds += 3;
                buffers += 1;
            }
            else if (availableMemoryGb > 0 && availableMemoryGb < 10)
            {
                seconds += 1;
            }

            if (totalMemoryGb > 0 && totalMemoryGb <= 8)
            {
                latency += 20;
            }

            if (ioMbPerSecond > 0 && ioMbPerSecond < 180)
            {
                latency += 50;
                seconds += 2;
            }

            return new AudioBufferRuntimeOptions(latency, seconds, buffers);
        }

        private static double GetDiskProbeResultMb()
        {
            if ((DateTime.UtcNow - ioProbeTimestampUtc).TotalMinutes < 5)
            {
                return ioProbeMbPerSecond;
            }

            ioProbeTimestampUtc = DateTime.UtcNow;
            ioProbeMbPerSecond = EstimateDiskThroughputMb();
            return ioProbeMbPerSecond;
        }

        private static double EstimateDiskThroughputMb()
        {
            try
            {
                string tempPath = Path.Combine(AppContext.BaseDirectory, ".__mm_io_probe.tmp");
                byte[] payload = new byte[4 * 1024 * 1024];
                new Random(17).NextBytes(payload);

                Stopwatch sw = Stopwatch.StartNew();
                File.WriteAllBytes(tempPath, payload);
                _ = File.ReadAllBytes(tempPath);
                sw.Stop();

                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                return (payload.Length * 2d / 1024d / 1024d) / Math.Max(0.001, sw.Elapsed.TotalSeconds);
            }
            catch
            {
                return 0;
            }
        }

        private static byte[] GetOrDecodePcm(string audioPath, bool markPreloaded)
        {
            string fullPath = Path.GetFullPath(audioPath);
            DateTime writeTimeUtc = File.GetLastWriteTimeUtc(fullPath);

            lock (cacheLock)
            {
                if (pcmCache.TryGetValue(fullPath, out CachedPcmAudio cached)
                    && cached.LastWriteTimeUtc == writeTimeUtc)
                {
                    cached.IsPreloaded = cached.IsPreloaded || markPreloaded;
                    cached.LastAccessUtc = DateTime.UtcNow;
                    return cached.PcmData;
                }
            }

            byte[] pcm = DecodeToPcm(fullPath);
            lock (cacheLock)
            {
                pcmCache[fullPath] = new CachedPcmAudio
                {
                    LastWriteTimeUtc = writeTimeUtc,
                    PcmData = pcm,
                    IsPreloaded = markPreloaded,
                    LastAccessUtc = DateTime.UtcNow
                };
            }

            return pcm;
        }

        private static byte[] DecodeToPcm(string fullPath)
        {
            using AudioFileReader reader = new AudioFileReader(fullPath);
            ISampleProvider provider = reader;

            if (provider.WaveFormat.SampleRate != targetFormat.SampleRate)
            {
                provider = new WdlResamplingSampleProvider(provider, targetFormat.SampleRate);
            }

            if (provider.WaveFormat.Channels != targetFormat.Channels)
            {
                provider = EnsureStereo(provider);
            }

            SampleToWaveProvider16 pcmProvider = new SampleToWaveProvider16(provider);
            using MemoryStream memory = new MemoryStream();

            byte[] chunk = new byte[targetFormat.AverageBytesPerSecond / 2];
            int read;
            while ((read = pcmProvider.Read(chunk, 0, chunk.Length)) > 0)
            {
                memory.Write(chunk, 0, read);
            }

            return memory.ToArray();
        }

        private static ISampleProvider EnsureStereo(ISampleProvider source)
        {
            int channels = source.WaveFormat.Channels;
            if (channels == 2)
            {
                return source;
            }

            if (channels == 1)
            {
                return new MonoToStereoSampleProvider(source);
            }

            MultiplexingSampleProvider multiplexer = new MultiplexingSampleProvider(new[] { source }, 2);
            multiplexer.ConnectInputToOutput(0, 0);
            multiplexer.ConnectInputToOutput(Math.Min(1, channels - 1), 1);
            return multiplexer;
        }

        private static void ApplyCacheRetentionPolicy()
        {
            AudioPreloadMode mode;
            lock (playbackLock)
            {
                mode = settings.PreloadMode;
            }

            lock (cacheLock)
            {
                if (mode == AudioPreloadMode.KeepAfterPlayback)
                {
                    return;
                }

                List<string> removeKeys = new List<string>();
                foreach ((string key, CachedPcmAudio value) in pcmCache)
                {
                    bool keepPreloaded =
                        (mode == AudioPreloadMode.AlwaysPreload
                        || mode == AudioPreloadMode.Auto
                        || mode == AudioPreloadMode.SemiAuto)
                        && value.IsPreloaded;
                    if (!keepPreloaded)
                    {
                        removeKeys.Add(key);
                    }
                }

                foreach (string key in removeKeys)
                {
                    pcmCache.Remove(key);
                }
            }
        }

        private static void RaisePlaybackCompleted()
        {
            try
            {
                PlaybackCompleted?.Invoke(null, EventArgs.Empty);
            }
            catch
            {
                // Ignore event failures.
            }
        }

        private static void Warn(string title, string message)
        {
            if ((DateTime.UtcNow - lastWarningAtUtc).TotalMilliseconds < 250)
            {
                return;
            }

            lastWarningAtUtc = DateTime.UtcNow;
            try
            {
                PlaybackWarningRaised?.Invoke(null, new AudioPlaybackWarningEventArgs(title, message));
            }
            catch
            {
                // Ignore warning dispatch failures.
            }
        }

        private static string BuildReadableErrorMessage(string prefix, string detail)
        {
            string cleanDetail = string.IsNullOrWhiteSpace(detail) ? "未知错误。" : detail.Trim();
            bool isBufferFull =
                cleanDetail.Contains("buffer full", StringComparison.OrdinalIgnoreCase)
                || (cleanDetail.Contains("buffer", StringComparison.OrdinalIgnoreCase)
                    && cleanDetail.Contains("full", StringComparison.OrdinalIgnoreCase));

            if (isBufferFull)
            {
                return $"{prefix}：缓冲区已满。建议在“优化”页提高延迟/缓冲秒数，或切换到“自动推荐/自适应”模式。";
            }

            return $"{prefix}：{cleanDetail}";
        }

        private sealed class PlaybackSession : IDisposable
        {
            public PlaybackSession(WaveOutEvent output, RawSourceWaveStream sourceWave, MemoryStream rawStream, string channelName)
            {
                Output = output;
                SourceWave = sourceWave;
                RawStream = rawStream;
                ChannelName = channelName;
            }

            public WaveOutEvent Output { get; }
            public RawSourceWaveStream SourceWave { get; }
            public MemoryStream RawStream { get; }
            public string ChannelName { get; }

            public void Dispose()
            {
                try
                {
                    Output?.Stop();
                }
                catch
                {
                    // Ignore stop failures.
                }

                Output?.Dispose();
                SourceWave?.Dispose();
                RawStream?.Dispose();
            }
        }

        private sealed class CachedPcmAudio
        {
            public DateTime LastWriteTimeUtc { get; init; }
            public byte[] PcmData { get; init; } = Array.Empty<byte>();
            public bool IsPreloaded { get; set; }
            public DateTime LastAccessUtc { get; set; }
        }
    }
}
