using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;

namespace MusicalMoments
{
    internal static class RuntimeSchedulingService
    {
        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint uPeriod);

        private static int highPerformanceRefCount;
        private static bool timerResolutionRaised;

        public static void ApplyHighPerformanceProfile()
        {
            if (Interlocked.Increment(ref highPerformanceRefCount) != 1)
            {
                return;
            }

            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                if (currentProcess.PriorityClass < ProcessPriorityClass.AboveNormal)
                {
                    currentProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
                }
            }
            catch
            {
                // Ignore process priority failures.
            }

            try
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }
            catch
            {
                // Ignore GC mode failures.
            }

            try
            {
                ThreadPool.GetMinThreads(out int minWorker, out int minIo);
                int targetWorker = Math.Max(minWorker, Math.Max(8, Environment.ProcessorCount * 2));
                int targetIo = Math.Max(minIo, 8);
                ThreadPool.SetMinThreads(targetWorker, targetIo);
            }
            catch
            {
                // Ignore threadpool tuning failures.
            }

            try
            {
                if (timeBeginPeriod(1) == 0)
                {
                    timerResolutionRaised = true;
                }
            }
            catch
            {
                // Ignore timer resolution failures.
            }
        }

        public static void RestoreDefaults()
        {
            if (Interlocked.Decrement(ref highPerformanceRefCount) > 0)
            {
                return;
            }

            if (highPerformanceRefCount < 0)
            {
                highPerformanceRefCount = 0;
            }

            try
            {
                GCSettings.LatencyMode = GCLatencyMode.Interactive;
            }
            catch
            {
                // Ignore GC mode failures.
            }

            if (timerResolutionRaised)
            {
                try
                {
                    timeEndPeriod(1);
                }
                catch
                {
                    // Ignore timer restore failures.
                }

                timerResolutionRaised = false;
            }
        }
    }
}
