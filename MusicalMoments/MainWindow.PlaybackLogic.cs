using System;
using System.IO;
using System.Threading;

namespace MusicalMoments
{
    public partial class MainWindow
    {
        internal static SameAudioPressBehavior sameAudioPressBehavior = SameAudioPressBehavior.StopPlayback;
        internal static DifferentAudioInterruptBehavior differentAudioInterruptBehavior = DifferentAudioInterruptBehavior.StopAndPlayNew;

        private static readonly object playbackStateLock = new object();
        private static readonly object playbackModeLock = new object();
        private static readonly object playbackDeviceCacheLock = new object();
        private static string currentPlaybackAudioPath = string.Empty;
        private static int cachedVbOutputDeviceId = -1;
        private static int cachedPhysicalOutputDeviceId = -1;
        private static int cachedMicrophoneInputDeviceId = -1;
        private static int cachedTipOutputDeviceId = -1;

        internal static string CurrentPlaybackAudioPath
        {
            get
            {
                lock (playbackStateLock)
                {
                    return currentPlaybackAudioPath;
                }
            }
        }

        internal static bool RequestPlayAudio(string audioPath, bool countAsPlayed)
        {
            if (!playAudio)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(audioPath) || !File.Exists(audioPath))
            {
                return false;
            }

            string fullPath = Path.GetFullPath(audioPath);
            bool isPlaybackActive = AudioPlaybackService.IsPlaybackActive;
            bool isSameAudio = false;
            lock (playbackStateLock)
            {
                isSameAudio = isPlaybackActive
                    && !string.IsNullOrWhiteSpace(currentPlaybackAudioPath)
                    && string.Equals(currentPlaybackAudioPath, fullPath, StringComparison.OrdinalIgnoreCase);
            }

            if (isPlaybackActive)
            {
                if (isSameAudio)
                {
                    if (sameAudioPressBehavior == SameAudioPressBehavior.StopPlayback)
                    {
                        Misc.StopCurrentPlayback();
                        MarkPlaybackStopped();
                        return true;
                    }
                }
                else
                {
                    Misc.StopCurrentPlayback();
                    MarkPlaybackStopped();
                    if (differentAudioInterruptBehavior == DifferentAudioInterruptBehavior.StopOnly)
                    {
                        return true;
                    }
                }
            }

            if (!TryResolvePlaybackOutputDeviceIds(out int vbOutputDeviceId, out int physicalOutputDeviceId))
            {
                return false;
            }

            Misc.PlayAudioToSpecificDevice(
                fullPath,
                vbOutputDeviceId,
                true,
                VBvolume,
                AudioEquipmentPlayCheck,
                fullPath,
                physicalOutputDeviceId,
                volume);

            lock (playbackStateLock)
            {
                currentPlaybackAudioPath = fullPath;
            }

            isPlaying = true;
            if (countAsPlayed)
            {
                Interlocked.Increment(ref playedCount);
            }

            return true;
        }

        internal static void TogglePlaybackModeCore(bool playSwitchTip)
        {
            lock (playbackModeLock)
            {
                playAudio = !playAudio;

                if (playSwitchTip && TryResolveTipOutputDeviceId(out int tipOutputDeviceId))
                {
                    string tipPath = playAudio
                        ? Path.Combine(runningDirectory, @"ResourceFiles\switch_to_audio.wav")
                        : Path.Combine(runningDirectory, @"ResourceFiles\switch_to_microphone.wav");
                    Misc.PlayAudioex(tipPath, tipOutputDeviceId, tipsvolume);
                }

                if (!playAudio)
                {
                    Misc.StopCurrentPlayback();
                    MarkPlaybackStopped();
                    if (TryResolveMicrophoneRouteDeviceIds(out int micInputDeviceId, out int vbOutputDeviceId))
                    {
                        Misc.StartMicrophoneToSpeaker(micInputDeviceId, vbOutputDeviceId);
                    }
                }
                else
                {
                    Misc.StopMicrophoneToSpeaker();
                }
            }

            Interlocked.Increment(ref changedCount);
        }

        internal static void UpdatePlaybackDeviceCache(
            int vbOutputDeviceId,
            int physicalOutputDeviceId,
            int microphoneInputDeviceId,
            int tipOutputDeviceId)
        {
            lock (playbackDeviceCacheLock)
            {
                cachedVbOutputDeviceId = vbOutputDeviceId;
                cachedPhysicalOutputDeviceId = physicalOutputDeviceId;
                cachedMicrophoneInputDeviceId = microphoneInputDeviceId;
                cachedTipOutputDeviceId = tipOutputDeviceId;
            }
        }

        internal static void MarkPlaybackStopped()
        {
            lock (playbackStateLock)
            {
                currentPlaybackAudioPath = string.Empty;
            }

            isPlaying = false;
        }

        private static bool TryResolvePlaybackOutputDeviceIds(out int vbOutputDeviceId, out int physicalOutputDeviceId)
        {
            lock (playbackDeviceCacheLock)
            {
                vbOutputDeviceId = cachedVbOutputDeviceId;
                physicalOutputDeviceId = cachedPhysicalOutputDeviceId;
            }

            if (vbOutputDeviceId < 0)
            {
                string[] outputDeviceNames = Misc.GetOutputAudioDeviceNames();
                if (outputDeviceNames.Length <= 0)
                {
                    return false;
                }

                int vbIndex = Math.Clamp(InputComboSelect, 0, outputDeviceNames.Length - 1);
                vbOutputDeviceId = Misc.GetOutputDeviceID(outputDeviceNames[vbIndex]);

                if (vbOutputDeviceId >= 0)
                {
                    lock (playbackDeviceCacheLock)
                    {
                        cachedVbOutputDeviceId = vbOutputDeviceId;
                    }
                }
            }

            if (vbOutputDeviceId < 0)
            {
                return false;
            }

            if (physicalOutputDeviceId < 0)
            {
                string[] outputDeviceNames = Misc.GetOutputAudioDeviceNames();
                if (outputDeviceNames.Length > 0)
                {
                    int physicalIndex = Math.Clamp(OutputComboSelect, 0, outputDeviceNames.Length - 1);
                    physicalOutputDeviceId = Misc.GetOutputDeviceID(outputDeviceNames[physicalIndex]);
                }

                if (AudioEquipmentPlayCheck)
                {
                    return false;
                }

                physicalOutputDeviceId = vbOutputDeviceId;
            }
            else if (!AudioEquipmentPlayCheck)
            {
                physicalOutputDeviceId = vbOutputDeviceId;
            }

            lock (playbackDeviceCacheLock)
            {
                cachedVbOutputDeviceId = vbOutputDeviceId;
                cachedPhysicalOutputDeviceId = physicalOutputDeviceId;
            }

            return true;
        }

        private static bool TryResolveMicrophoneRouteDeviceIds(out int microphoneInputDeviceId, out int vbOutputDeviceId)
        {
            lock (playbackDeviceCacheLock)
            {
                microphoneInputDeviceId = cachedMicrophoneInputDeviceId;
                vbOutputDeviceId = cachedVbOutputDeviceId;
            }

            if (microphoneInputDeviceId < 0)
            {
                string[] inputDeviceNames = Misc.GetInputAudioDeviceNames();
                if (inputDeviceNames.Length <= 0)
                {
                    return false;
                }

                int micIndex = Math.Clamp(VBOutputComboSelect, 0, inputDeviceNames.Length - 1);
                microphoneInputDeviceId = Misc.GetInputDeviceID(inputDeviceNames[micIndex]);
                if (microphoneInputDeviceId >= 0)
                {
                    lock (playbackDeviceCacheLock)
                    {
                        cachedMicrophoneInputDeviceId = microphoneInputDeviceId;
                    }
                }
            }

            if (vbOutputDeviceId < 0)
            {
                string[] outputDeviceNames = Misc.GetOutputAudioDeviceNames();
                if (outputDeviceNames.Length <= 0)
                {
                    return false;
                }

                int vbIndex = Math.Clamp(InputComboSelect, 0, outputDeviceNames.Length - 1);
                vbOutputDeviceId = Misc.GetOutputDeviceID(outputDeviceNames[vbIndex]);
                if (vbOutputDeviceId >= 0)
                {
                    lock (playbackDeviceCacheLock)
                    {
                        cachedVbOutputDeviceId = vbOutputDeviceId;
                    }
                }
            }

            return microphoneInputDeviceId >= 0 && vbOutputDeviceId >= 0;
        }

        private static bool TryResolveTipOutputDeviceId(out int tipOutputDeviceId)
        {
            lock (playbackDeviceCacheLock)
            {
                tipOutputDeviceId = cachedTipOutputDeviceId >= 0
                    ? cachedTipOutputDeviceId
                    : cachedPhysicalOutputDeviceId;
            }

            if (tipOutputDeviceId >= 0)
            {
                return true;
            }

            string[] outputDeviceNames = Misc.GetOutputAudioDeviceNames();
            if (outputDeviceNames.Length <= 0)
            {
                return false;
            }

            int tipIndex = Math.Clamp(OutputComboSelect, 0, outputDeviceNames.Length - 1);
            tipOutputDeviceId = Misc.GetOutputDeviceID(outputDeviceNames[tipIndex]);
            if (tipOutputDeviceId < 0)
            {
                return false;
            }

            lock (playbackDeviceCacheLock)
            {
                cachedTipOutputDeviceId = tipOutputDeviceId;
            }

            return true;
        }

        internal static bool TrySetSelectedAudio(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (!File.Exists(path))
            {
                return false;
            }

            selectedAudioPath = Path.GetFullPath(path);
            MainWindow window = CurrentInstance;
            if (window != null && !window.IsDisposed)
            {
                window.QueueHotAudioPrime();
                try
                {
                    window.BeginInvoke(new Action(() =>
                    {
                        if (!window.IsDisposed)
                        {
                            string displayName = Path.GetFileNameWithoutExtension(selectedAudioPath);
                            window.SelectedAudioLabel.Text = $"已选择:{displayName}";
                        }
                    }));
                }
                catch
                {
                    // Ignore UI update failures.
                }
            }

            return true;
        }

        internal static bool TryStopPlayback()
        {
            Misc.StopCurrentPlayback();
            MarkPlaybackStopped();
            return true;
        }
    }
}
