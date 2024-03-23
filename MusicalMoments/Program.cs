namespace MusicalMoments
{
    internal static class Program
    {
        static bool isAppRunning = false;
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
            LogException((Exception)e.ExceptionObject);
        }
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogException(e.Exception);
        }
        private static void LogException(Exception ex)
        {
        }
    }
}