using System.Linq;
using System.Windows.Forms;

namespace MusicalMoments.PluginExample
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            string serverAddress = ResolveServerAddress(args);
            PluginLog.Write($"启动，参数地址: {args?.FirstOrDefault() ?? "<empty>"}，解析地址: {serverAddress}");
            Application.Run(new MainForm(serverAddress));
        }

        private static string ResolveServerAddress(string[] args)
        {
            string fromArgs = args != null && args.Length > 0 ? args[0] : string.Empty;
            string cleanedArgs = CleanAddress(fromArgs);
            if (!string.IsNullOrWhiteSpace(cleanedArgs))
            {
                return cleanedArgs;
            }

            string fromEnvironment = Environment.GetEnvironmentVariable("MM_PLUGIN_WS_ADDRESS") ?? string.Empty;
            return CleanAddress(fromEnvironment);
        }

        private static string CleanAddress(string raw)
        {
            return string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.Trim().Trim('"');
        }
    }
}
