using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MusicalMoments
{
    internal readonly struct ClientUsageSnapshot
    {
        public ulong PlayedCount { get; }
        public ulong CloseCount { get; }
        public ulong StreamChangedCount { get; }

        public ClientUsageSnapshot(ulong playedCount, ulong closeCount, ulong streamChangedCount)
        {
            PlayedCount = playedCount;
            CloseCount = closeCount;
            StreamChangedCount = streamChangedCount;
        }
    }

    internal sealed class ClientUsageReporter : IDisposable
    {
        private const string DefaultWebBaseUrl = "https://www.scmd.cc";
        private const string BaseUrlEnvKey = "MM_WEB_BASE_URL";
        private const string BaseUrlFileName = "mm-web-base-url.txt";
        private const string ClientIDFileName = "usage-client.id";

        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(45);
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(4);

        private readonly HttpClient httpClient;
        private readonly Uri endpoint;
        private readonly string clientId;
        private readonly string sessionId;
        private readonly string appVersion;
        private readonly Func<ClientUsageSnapshot> snapshotFactory;

        private CancellationTokenSource heartbeatCts;
        private Task heartbeatTask = Task.CompletedTask;
        private int started;
        private int disposed;

        private ClientUsageReporter(
            Uri endpoint,
            string clientId,
            string sessionId,
            string appVersion,
            Func<ClientUsageSnapshot> snapshotFactory)
        {
            this.endpoint = endpoint;
            this.clientId = clientId;
            this.sessionId = sessionId;
            this.appVersion = appVersion;
            this.snapshotFactory = snapshotFactory;
            httpClient = new HttpClient
            {
                Timeout = RequestTimeout
            };
        }

        public static ClientUsageReporter TryCreate(
            string runningDirectory,
            string appVersion,
            Func<ClientUsageSnapshot> snapshotFactory)
        {
            if (snapshotFactory == null)
            {
                return null;
            }

            if (!TryResolveEndpoint(runningDirectory, out Uri endpoint))
            {
                return null;
            }

            string normalizedDirectory = string.IsNullOrWhiteSpace(runningDirectory)
                ? AppDomain.CurrentDomain.BaseDirectory
                : runningDirectory;
            string clientId = LoadOrCreateClientID(normalizedDirectory);
            string sessionId = Guid.NewGuid().ToString("N");
            string normalizedVersion = string.IsNullOrWhiteSpace(appVersion)
                ? "unknown"
                : appVersion.Trim();
            if (normalizedVersion.Length > 64)
            {
                normalizedVersion = normalizedVersion[..64];
            }

            return new ClientUsageReporter(
                endpoint,
                clientId,
                sessionId,
                normalizedVersion,
                snapshotFactory);
        }

        public void Start()
        {
            if (Volatile.Read(ref disposed) == 1)
            {
                return;
            }

            if (Interlocked.Exchange(ref started, 1) == 1)
            {
                return;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            heartbeatCts = cts;
            heartbeatTask = RunHeartbeatLoopAsync(cts.Token);
            _ = SendAsync("startup", CancellationToken.None);
        }

        public Task ReportHeartbeatAsync()
        {
            if (Volatile.Read(ref started) == 0 || Volatile.Read(ref disposed) == 1)
            {
                return Task.CompletedTask;
            }

            return SendAsync("heartbeat", CancellationToken.None);
        }

        public void StopAndDispose(bool sendShutdown)
        {
            if (Interlocked.Exchange(ref disposed, 1) == 1)
            {
                return;
            }

            CancellationTokenSource cts = Interlocked.Exchange(ref heartbeatCts, null);
            if (cts != null)
            {
                try
                {
                    cts.Cancel();
                }
                catch
                {
                    // Ignore cancellation failures.
                }
                finally
                {
                    cts.Dispose();
                }
            }

            try
            {
                heartbeatTask.GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore heartbeat loop shutdown errors.
            }

            bool wasStarted = Interlocked.Exchange(ref started, 0) == 1;
            if (sendShutdown && wasStarted)
            {
                try
                {
                    using CancellationTokenSource timeout = new CancellationTokenSource(RequestTimeout);
                    SendAsync("shutdown", timeout.Token).GetAwaiter().GetResult();
                }
                catch
                {
                    // Ignore best-effort shutdown report failures.
                }
            }

            httpClient.Dispose();
        }

        public void Dispose()
        {
            StopAndDispose(sendShutdown: false);
        }

        private async Task RunHeartbeatLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(HeartbeatInterval, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Retry on next interval.
                    continue;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await SendAsync("heartbeat", cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SendAsync(string eventType, CancellationToken cancellationToken)
        {
            ClientUsageSnapshot snapshot;
            try
            {
                snapshot = snapshotFactory();
            }
            catch
            {
                return;
            }

            string payload = JsonConvert.SerializeObject(new
            {
                clientId,
                sessionId,
                @event = eventType,
                appVersion,
                playedCount = snapshot.PlayedCount,
                closeCount = snapshot.CloseCount,
                streamChangedCount = snapshot.StreamChangedCount
            });

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            try
            {
                using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                _ = response.IsSuccessStatusCode;
            }
            catch
            {
                // Ignore network/timeout errors to avoid blocking main app flow.
            }
        }

        private static bool TryResolveEndpoint(string runningDirectory, out Uri endpoint)
        {
            endpoint = null;

            string baseUrl = Environment.GetEnvironmentVariable(BaseUrlEnvKey)?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                string directory = string.IsNullOrWhiteSpace(runningDirectory)
                    ? AppDomain.CurrentDomain.BaseDirectory
                    : runningDirectory;
                string filePath = Path.Combine(directory, BaseUrlFileName);
                if (File.Exists(filePath))
                {
                    try
                    {
                        baseUrl = (File.ReadAllText(filePath) ?? string.Empty).Trim();
                    }
                    catch
                    {
                        baseUrl = string.Empty;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = DefaultWebBaseUrl;
            }

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri baseUri))
            {
                return false;
            }

            if (!string.Equals(baseUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            endpoint = new Uri(baseUri, "/api/client/usage");
            return true;
        }

        private static string LoadOrCreateClientID(string runningDirectory)
        {
            string path = Path.Combine(runningDirectory, ClientIDFileName);
            try
            {
                if (File.Exists(path))
                {
                    string current = (File.ReadAllText(path) ?? string.Empty).Trim();
                    if (IsValidClientID(current))
                    {
                        return current;
                    }
                }
            }
            catch
            {
                // Ignore read errors and regenerate an ID.
            }

            string generated = Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(path, generated);
            }
            catch
            {
                // Ignore persistence errors; keep in-memory ID.
            }

            return generated;
        }

        private static bool IsValidClientID(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 72)
            {
                return false;
            }

            foreach (char c in value)
            {
                if ((c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || (c >= '0' && c <= '9')
                    || c == '-'
                    || c == '_')
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
