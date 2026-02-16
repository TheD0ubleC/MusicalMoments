using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class AudioPlaybackService
    {
        public static WaveOutEvent CurrentOutputDevice { get; private set; }
        public static AudioFileReader CurrentAudioFile { get; private set; }
        public static bool IsPlaybackActive
        {
            get
            {
                lock (playbackLock)
                {
                    bool bufferedPlaying =
                        (primaryBuffer?.BufferedBytes ?? 0) > 0
                        || (secondaryBuffer?.BufferedBytes ?? 0) > 0;
                    bool directPlaying = currentOutputDeviceEx?.PlaybackState == PlaybackState.Playing;
                    return bufferedPlaying || directPlaying;
                }
            }
        }

        private static readonly object playbackLock = new object();
        private static readonly Dictionary<string, CachedPcmAudio> pcmCache = new Dictionary<string, CachedPcmAudio>(StringComparer.OrdinalIgnoreCase);

        // 统一到稳定的输出格式，尽可能降低不同采样率/声道导致的卡顿。
        private static readonly WaveFormat targetPlaybackFormat = new WaveFormat(48000, 16, 2);
        private const int PlaybackLatencyMs = 120;
        private const int DefaultBufferSeconds = 20;

        private static WaveOutEvent primaryOutput;
        private static BufferedWaveProvider primaryBuffer;
        private static int primaryDeviceId = int.MinValue;

        private static WaveOutEvent secondaryOutput;
        private static BufferedWaveProvider secondaryBuffer;
        private static int secondaryDeviceId = int.MinValue;

        private static WaveInEvent waveIn;
        private static WaveOutEvent waveOut;
        private static BufferedWaveProvider bufferedWaveProvider;

        private static WaveOutEvent currentOutputDeviceEx;
        private static AudioFileReader currentAudioFileEx;

        public static void PlayAudioToSpecificDevice(
            string audioFilePath,
            int deviceNumber,
            bool stopCurrent,
            float volume,
            bool relayPlayback,
            string relayAudioFilePath,
            int relayDeviceNumber,
            float relayVolume)
        {
            try
            {
                lock (playbackLock)
                {
                    EnsurePrimaryPipeline(deviceNumber, volume);

                    if (relayPlayback)
                    {
                        EnsureSecondaryPipeline(relayDeviceNumber, relayVolume);
                    }
                    else
                    {
                        DisposeSecondaryPipeline();
                    }

                    if (stopCurrent)
                    {
                        primaryBuffer.ClearBuffer();
                        secondaryBuffer?.ClearBuffer();
                    }

                    byte[] primaryPcm = GetOrDecodePcm(audioFilePath);
                    EnsureBufferCapacity(primaryBuffer, primaryPcm.Length);
                    primaryBuffer.AddSamples(primaryPcm, 0, primaryPcm.Length);

                    if (relayPlayback && secondaryBuffer != null)
                    {
                        string secondaryPath = string.IsNullOrWhiteSpace(relayAudioFilePath) ? audioFilePath : relayAudioFilePath;
                        byte[] secondaryPcm = string.Equals(secondaryPath, audioFilePath, StringComparison.OrdinalIgnoreCase)
                            ? primaryPcm
                            : GetOrDecodePcm(secondaryPath);

                        EnsureBufferCapacity(secondaryBuffer, secondaryPcm.Length);
                        secondaryBuffer.AddSamples(secondaryPcm, 0, secondaryPcm.Length);
                    }

                    CurrentOutputDevice = primaryOutput;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放音频时出错: {ex.Message}", "错误");
            }
        }

        public static void PlayAudioEx(string audioFilePath, int deviceNumber, float volume)
        {
            try
            {
                if (currentOutputDeviceEx != null)
                {
                    currentOutputDeviceEx.Stop();
                    currentOutputDeviceEx.Dispose();
                    currentAudioFileEx?.Dispose();
                }

                AudioFileReader audioFile = new AudioFileReader(audioFilePath);
                WaveOutEvent outputDevice = new WaveOutEvent { DeviceNumber = deviceNumber, DesiredLatency = PlaybackLatencyMs };
                outputDevice.Volume = Math.Clamp(volume, 0, 1);
                outputDevice.PlaybackStopped += (_, _) =>
                {
                    outputDevice.Dispose();
                    audioFile.Dispose();
                };

                outputDevice.Init(audioFile);
                outputDevice.Play();
                currentOutputDeviceEx = outputDevice;
                currentAudioFileEx = audioFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放音频时出错: {ex.Message}", "错误");
            }
        }

        public static void StopCurrentPlayback()
        {
            lock (playbackLock)
            {
                primaryBuffer?.ClearBuffer();
                secondaryBuffer?.ClearBuffer();
            }

            if (currentOutputDeviceEx != null)
            {
                currentOutputDeviceEx.Stop();
                currentOutputDeviceEx.Dispose();
                currentOutputDeviceEx = null;
            }

            currentAudioFileEx?.Dispose();
            currentAudioFileEx = null;
            CurrentOutputDevice = null;
            CurrentAudioFile = null;
        }

        public static void StartMicrophoneToSpeaker(int inputDeviceIndex, int outputDeviceIndex)
        {
            StopMicrophoneToSpeaker();

            waveIn = new WaveInEvent
            {
                DeviceNumber = inputDeviceIndex,
                WaveFormat = new WaveFormat(44100, 16, 2)
            };

            waveOut = new WaveOutEvent
            {
                DeviceNumber = outputDeviceIndex
            };

            bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                DiscardOnBufferOverflow = true
            };

            waveOut.Init(bufferedWaveProvider);
            waveIn.DataAvailable += (_, e) =>
            {
                bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            };

            waveIn.StartRecording();
            waveOut.Play();
        }

        public static void StopMicrophoneToSpeaker()
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveOut?.Stop();
            waveOut?.Dispose();
            bufferedWaveProvider = null;
        }

        public static string DisplayAudioProperties(string audioFilePath)
        {
            try
            {
                using AudioFileReader audioFile = new AudioFileReader(audioFilePath);
                string frequency = audioFile.WaveFormat.SampleRate.ToString();
                string bits = (audioFile.WaveFormat.BitsPerSample / 8).ToString();
                string channels = audioFile.WaveFormat.Channels.ToString();
                string format = Path.GetExtension(audioFilePath).TrimStart('.');
                return $"格式: {format.ToUpper()}, 频率: {frequency} Hz, 位数: {bits} bit, 声道: {channels}";
            }
            catch
            {
                return "无法读取音频属性";
            }
        }

        private static void EnsurePrimaryPipeline(int deviceNumber, float volume)
        {
            if (primaryOutput != null && primaryDeviceId == deviceNumber)
            {
                primaryOutput.Volume = Math.Clamp(volume, 0, 1);
                return;
            }

            DisposePrimaryPipeline();

            primaryDeviceId = deviceNumber;
            primaryBuffer = CreatePlaybackBuffer();
            primaryOutput = new WaveOutEvent
            {
                DeviceNumber = deviceNumber,
                DesiredLatency = PlaybackLatencyMs,
                NumberOfBuffers = 3
            };
            primaryOutput.Init(primaryBuffer);
            primaryOutput.Volume = Math.Clamp(volume, 0, 1);
            primaryOutput.Play();
            CurrentOutputDevice = primaryOutput;
        }

        private static void EnsureSecondaryPipeline(int deviceNumber, float volume)
        {
            if (secondaryOutput != null && secondaryDeviceId == deviceNumber)
            {
                secondaryOutput.Volume = Math.Clamp(volume, 0, 1);
                return;
            }

            DisposeSecondaryPipeline();

            secondaryDeviceId = deviceNumber;
            secondaryBuffer = CreatePlaybackBuffer();
            secondaryOutput = new WaveOutEvent
            {
                DeviceNumber = deviceNumber,
                DesiredLatency = PlaybackLatencyMs,
                NumberOfBuffers = 3
            };
            secondaryOutput.Init(secondaryBuffer);
            secondaryOutput.Volume = Math.Clamp(volume, 0, 1);
            secondaryOutput.Play();
        }

        private static BufferedWaveProvider CreatePlaybackBuffer()
        {
            BufferedWaveProvider buffer = new BufferedWaveProvider(targetPlaybackFormat)
            {
                DiscardOnBufferOverflow = false,
                ReadFully = true
            };
            buffer.BufferDuration = TimeSpan.FromSeconds(DefaultBufferSeconds);
            return buffer;
        }

        private static void EnsureBufferCapacity(BufferedWaveProvider buffer, int requiredBytes)
        {
            if (buffer == null || requiredBytes <= 0)
            {
                return;
            }

            if (requiredBytes <= buffer.BufferLength)
            {
                return;
            }

            int minBytes = targetPlaybackFormat.AverageBytesPerSecond;
            int newLength = Math.Max(requiredBytes + minBytes, buffer.BufferLength * 2);
            buffer.BufferLength = newLength;
        }

        private static byte[] GetOrDecodePcm(string audioFilePath)
        {
            string fullPath = Path.GetFullPath(audioFilePath);
            DateTime lastWriteUtc = File.GetLastWriteTimeUtc(fullPath);

            if (pcmCache.TryGetValue(fullPath, out CachedPcmAudio cached) && cached.LastWriteTimeUtc == lastWriteUtc)
            {
                return cached.PcmData;
            }

            byte[] pcmData = DecodeFileToTargetPcm(fullPath);
            pcmCache[fullPath] = new CachedPcmAudio(lastWriteUtc, pcmData);
            return pcmData;
        }

        private static byte[] DecodeFileToTargetPcm(string fullPath)
        {
            using AudioFileReader reader = new AudioFileReader(fullPath);

            ISampleProvider sampleProvider = reader;
            sampleProvider = EnsureStereo(sampleProvider);

            if (sampleProvider.WaveFormat.SampleRate != targetPlaybackFormat.SampleRate)
            {
                sampleProvider = new WdlResamplingSampleProvider(sampleProvider, targetPlaybackFormat.SampleRate);
            }

            if (sampleProvider.WaveFormat.Channels != targetPlaybackFormat.Channels)
            {
                sampleProvider = EnsureStereo(sampleProvider);
            }

            SampleToWaveProvider16 pcmProvider = new SampleToWaveProvider16(sampleProvider);
            using MemoryStream memoryStream = new MemoryStream();
            byte[] readBuffer = new byte[targetPlaybackFormat.AverageBytesPerSecond / 2];

            int bytesRead;
            while ((bytesRead = pcmProvider.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                memoryStream.Write(readBuffer, 0, bytesRead);
            }

            return memoryStream.ToArray();
        }

        private static ISampleProvider EnsureStereo(ISampleProvider source)
        {
            if (source.WaveFormat.Channels == 2)
            {
                return source;
            }

            if (source.WaveFormat.Channels == 1)
            {
                return new MonoToStereoSampleProvider(source);
            }

            MultiplexingSampleProvider multiplexer = new MultiplexingSampleProvider(new[] { source }, 2);
            multiplexer.ConnectInputToOutput(0, 0);
            multiplexer.ConnectInputToOutput(1, 1);
            return multiplexer;
        }

        private static void DisposePrimaryPipeline()
        {
            if (primaryOutput != null)
            {
                primaryOutput.Stop();
                primaryOutput.Dispose();
                primaryOutput = null;
            }

            primaryBuffer = null;
            primaryDeviceId = int.MinValue;
        }

        private static void DisposeSecondaryPipeline()
        {
            if (secondaryOutput != null)
            {
                secondaryOutput.Stop();
                secondaryOutput.Dispose();
                secondaryOutput = null;
            }

            secondaryBuffer = null;
            secondaryDeviceId = int.MinValue;
        }

        private sealed class CachedPcmAudio
        {
            public DateTime LastWriteTimeUtc { get; }
            public byte[] PcmData { get; }

            public CachedPcmAudio(DateTime lastWriteTimeUtc, byte[] pcmData)
            {
                LastWriteTimeUtc = lastWriteTimeUtc;
                PcmData = pcmData;
            }
        }
    }
}
