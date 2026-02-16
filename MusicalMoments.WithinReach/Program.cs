using System.Windows.Forms;

namespace MusicalMoments.WithinReach
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            string wsAddress = ResolveServerAddress(args);
            Application.Run(new MainForm(wsAddress));
        }

        private static string ResolveServerAddress(string[] args)
        {
            string fromArgs = args != null && args.Length > 0 ? args[0] : string.Empty;
            string cleanedArgs = NormalizeAddress(fromArgs);
            if (!string.IsNullOrWhiteSpace(cleanedArgs))
            {
                return cleanedArgs;
            }

            string fromEnvironment = Environment.GetEnvironmentVariable("MM_PLUGIN_WS_ADDRESS") ?? string.Empty;
            return NormalizeAddress(fromEnvironment);
        }

        private static string NormalizeAddress(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().Trim('"');
        }
    }
}
