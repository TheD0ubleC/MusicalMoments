using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicalMoments
{
    public partial class MainWindow
    {
        private readonly ConcurrentQueue<Keys> hotkeyQueue = new ConcurrentQueue<Keys>();
        private readonly AutoResetEvent hotkeyQueueSignal = new AutoResetEvent(false);
        private readonly object hotkeyAudioIndexLock = new object();
        private Dictionary<Keys, string> hotkeyAudioPathIndex = new Dictionary<Keys, string>();
        private CancellationTokenSource hotkeyWorkerCts;
        private Thread hotkeyWorkerThread;
        private CancellationTokenSource hotAudioPrimeCts;
        private volatile bool switchStreamTipsCache = true;

        private void StartHotkeyDispatcher()
        {
            if (hotkeyWorkerThread != null)
            {
                return;
            }

            RuntimeSchedulingService.ApplyHighPerformanceProfile();

            hotkeyWorkerCts = new CancellationTokenSource();
            hotkeyWorkerThread = new Thread(() => HotkeyWorkerLoop(hotkeyWorkerCts.Token))
            {
                IsBackground = true,
                Name = "MM-HotkeyWorker",
                Priority = ThreadPriority.Highest
            };
            hotkeyWorkerThread.Start();
            UpdateSwitchStreamTipsCache();
            RebuildHotkeyAudioIndex();
        }

        private void StopHotkeyDispatcher()
        {
            CancellationTokenSource cts = hotkeyWorkerCts;
            hotkeyWorkerCts = null;

            if (cts != null)
            {
                cts.Cancel();
                hotkeyQueueSignal.Set();
            }

            if (hotkeyWorkerThread != null)
            {
                try
                {
                    hotkeyWorkerThread.Join(800);
                }
                catch
                {
                    // Ignore shutdown wait failures.
                }

                hotkeyWorkerThread = null;
            }

            while (hotkeyQueue.TryDequeue(out _))
            {
                // Drain pending input commands.
            }

            CancellationTokenSource oldPrimeCts = Interlocked.Exchange(ref hotAudioPrimeCts, null);
            if (oldPrimeCts != null)
            {
                oldPrimeCts.Cancel();
                oldPrimeCts.Dispose();
            }

            RuntimeSchedulingService.RestoreDefaults();
        }

        private void EnqueueGlobalHotkey(Keys keyCode)
        {
            if (hotkeyWorkerThread == null)
            {
                HandleGlobalHotkeyOnWorker(keyCode);
                return;
            }

            hotkeyQueue.Enqueue(keyCode);
            hotkeyQueueSignal.Set();
        }

        private void HotkeyWorkerLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                hotkeyQueueSignal.WaitOne(20);
                while (!cancellationToken.IsCancellationRequested && hotkeyQueue.TryDequeue(out Keys keyCode))
                {
                    HandleGlobalHotkeyOnWorker(keyCode);
                }
            }
        }

        private void HandleGlobalHotkeyOnWorker(Keys keyCode)
        {
            if (keyCode == Keys.Escape)
            {
                return;
            }

            if (keyCode == playAudioKey)
            {
                RequestPlayAudio(selectedAudioPath, countAsPlayed: true);
                return;
            }

            if (keyCode == toggleStreamKey)
            {
                TogglePlaybackModeCore(switchStreamTipsCache);
                return;
            }

            string audioPath = null;
            lock (hotkeyAudioIndexLock)
            {
                hotkeyAudioPathIndex.TryGetValue(keyCode, out audioPath);
            }

            if (!string.IsNullOrWhiteSpace(audioPath))
            {
                RequestPlayAudio(audioPath, countAsPlayed: true);
            }
        }

        internal void RebuildHotkeyAudioIndex()
        {
            Dictionary<Keys, string> next = new Dictionary<Keys, string>();
            foreach (AudioInfo item in audioInfo)
            {
                if (item == null
                    || item.Key == Keys.None
                    || string.IsNullOrWhiteSpace(item.FilePath)
                    || !File.Exists(item.FilePath))
                {
                    continue;
                }

                if (!next.ContainsKey(item.Key))
                {
                    next[item.Key] = Path.GetFullPath(item.FilePath);
                }
            }

            lock (hotkeyAudioIndexLock)
            {
                hotkeyAudioPathIndex = next;
            }

            QueueHotAudioPrime();
        }

        internal void QueueHotAudioPrime()
        {
            CancellationTokenSource nextCts = new CancellationTokenSource();
            CancellationTokenSource oldCts = Interlocked.Exchange(ref hotAudioPrimeCts, nextCts);
            if (oldCts != null)
            {
                oldCts.Cancel();
                oldCts.Dispose();
            }

            _ = PrimeHotAudioCacheAsync(nextCts.Token);
        }

        private async Task PrimeHotAudioCacheAsync(CancellationToken cancellationToken)
        {
            List<string> candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(selectedAudioPath) && File.Exists(selectedAudioPath))
            {
                candidates.Add(Path.GetFullPath(selectedAudioPath));
            }

            lock (hotkeyAudioIndexLock)
            {
                candidates.AddRange(hotkeyAudioPathIndex.Values);
            }

            if (candidates.Count == 0)
            {
                return;
            }

            await AudioPlaybackService.PrimeHotCacheAsync(candidates, 36, cancellationToken);
        }

        private void UpdateSwitchStreamTipsCache()
        {
            switchStreamTipsCache = switchStreamTips?.Checked ?? false;
        }
    }
}
