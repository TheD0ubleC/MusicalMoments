namespace MusicalMoments
{
    internal enum SameAudioPressBehavior
    {
        RestartFromBeginning = 0,
        StopPlayback = 1
    }

    internal enum DifferentAudioInterruptBehavior
    {
        StopOnly = 0,
        StopAndPlayNew = 1
    }

    internal static class PlaybackBehaviorText
    {
        public static string ToDisplayText(SameAudioPressBehavior behavior)
        {
            return behavior switch
            {
                SameAudioPressBehavior.StopPlayback => "再次按下时停止播放",
                _ => "再次按下时从头重播"
            };
        }

        public static string ToDisplayText(DifferentAudioInterruptBehavior behavior)
        {
            return behavior switch
            {
                DifferentAudioInterruptBehavior.StopOnly => "播放其他音频时仅停止当前",
                _ => "播放其他音频时停止并播放新的"
            };
        }
    }
}
