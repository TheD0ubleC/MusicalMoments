using System.Windows.Forms;

namespace MusicalMoments
{
    public class AudioInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public Keys Key { get; set; } = Keys.None;
    }
}
