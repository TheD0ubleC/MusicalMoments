namespace MusicalMoments.WithinReach
{
    internal sealed class PluginStateSnapshot
    {
        public bool IsPlaying { get; set; }
        public bool PlayAudioEnabled { get; set; }
        public string SelectedAudioPath { get; set; } = string.Empty;
        public string CurrentPlaybackAudioPath { get; set; } = string.Empty;
        public string SameAudioBehavior { get; set; } = string.Empty;
        public string DifferentAudioBehavior { get; set; } = string.Empty;
    }

    internal sealed class PlaybackBehaviorSnapshot
    {
        public string SameAudioBehavior { get; set; } = string.Empty;
        public string SameAudioBehaviorText { get; set; } = string.Empty;
        public string DifferentAudioBehavior { get; set; } = string.Empty;
        public string DifferentAudioBehaviorText { get; set; } = string.Empty;
    }

    internal sealed class AudioItem
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }
}
