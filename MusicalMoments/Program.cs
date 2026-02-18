using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class Program
    {
        static bool isAppRunning = false;
        private static int unhandledDialogShown;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            Mutex mutex = new Mutex(true, System.Diagnostics.Process.GetCurrentProcess().ProcessName, out isAppRunning);
            if (!isAppRunning)
            {
                Environment.Exit(1);
            }
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            ApplicationConfiguration.Initialize();
            Application.Run(new MainWindow());


        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException("AppDomain", ex);
            }
        }
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogException("UI", e.Exception);
        }
        private static void LogException(string source, Exception ex)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                string logContent =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}{Environment.NewLine}" +
                    $"{ex}{Environment.NewLine}{new string('-', 50)}{Environment.NewLine}";
                File.AppendAllText(logPath, logContent, Encoding.UTF8);
            }
            catch
            {
                // Ignore logging failures and still try to notify the user.
            }

            if (Interlocked.Exchange(ref unhandledDialogShown, 1) != 0)
            {
                return;
            }

            try
            {
                MessageBox.Show(
                    $"程序发生异常：{ex.Message}\r\n详细信息已写入运行目录 error.log",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                // Ignore message box failures in crash paths.
            }
        }
    }
}

