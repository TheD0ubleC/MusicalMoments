using System;
using System.IO;

namespace MusicalMoments
{
    public partial class MainWindow
    {
        internal static SameAudioPressBehavior sameAudioPressBehavior = SameAudioPressBehavior.StopPlayback;
        internal static DifferentAudioInterruptBehavior differentAudioInterruptBehavior = DifferentAudioInterruptBehavior.StopAndPlayNew;

        private static readonly object playbackStateLock = new object();
        private static string currentPlaybackAudioPath = string.Empty;

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
                playedCount += 1;
            }

            return true;
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
            vbOutputDeviceId = -1;
            physicalOutputDeviceId = -1;

            string[] outputDeviceNames = Misc.GetOutputAudioDeviceNames();
            if (outputDeviceNames.Length <= 0)
            {
                return false;
            }

            int vbIndex = Math.Clamp(InputComboSelect, 0, outputDeviceNames.Length - 1);
            int physicalIndex = Math.Clamp(OutputComboSelect, 0, outputDeviceNames.Length - 1);

            string vbOutputName = outputDeviceNames[vbIndex];
            string physicalOutputName = outputDeviceNames[physicalIndex];

            vbOutputDeviceId = Misc.GetOutputDeviceID(vbOutputName);
            physicalOutputDeviceId = Misc.GetOutputDeviceID(physicalOutputName);

            if (vbOutputDeviceId < 0)
            {
                return false;
            }

            if (physicalOutputDeviceId < 0)
            {
                if (AudioEquipmentPlayCheck)
                {
                    return false;
                }

                physicalOutputDeviceId = vbOutputDeviceId;
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
