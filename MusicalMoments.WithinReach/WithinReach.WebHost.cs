using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace MusicalMoments.WithinReach
{
    internal sealed class WithinReachWebHost : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly PluginWsClient wsClient;
        private readonly Func<WebClickMode> modeGetter;
        private readonly Action<WebClickMode> modeSetter;
        private readonly object lockObject = new object();

        private HttpListener listener;
        private CancellationTokenSource cancellationTokenSource;
        private Task listenTask;

        public WithinReachWebHost(PluginWsClient wsClient, Func<WebClickMode> modeGetter, Action<WebClickMode> modeSetter)
        {
            this.wsClient = wsClient;
            this.modeGetter = modeGetter;
            this.modeSetter = modeSetter;
        }

        public bool IsRunning => listener != null && listener.IsListening;
        public int Port { get; private set; }
        public string BindAddress => Port > 0 ? $"http://0.0.0.0:{Port}/" : string.Empty;
        public string BrowserAddress => Port > 0 ? $"http://127.0.0.1:{Port}/" : string.Empty;

        public void Start(int preferredPort)
        {
            lock (lockObject)
            {
                if (IsRunning)
                {
                    return;
                }

                int port = ResolvePort(preferredPort);
                HttpListener nextListener = new HttpListener();
                nextListener.Prefixes.Add($"http://+:{port}/");
                nextListener.Start();

                listener = nextListener;
                Port = port;
                cancellationTokenSource = new CancellationTokenSource();
                listenTask = Task.Run(() => ListenLoopAsync(cancellationTokenSource.Token));
            }
        }

        public void Stop()
        {
            lock (lockObject)
            {
                if (!IsRunning)
                {
                    return;
                }

                try { cancellationTokenSource?.Cancel(); } catch { }
                try { listener?.Stop(); listener?.Close(); } catch { }
                try { listenTask?.Wait(1500); } catch { }

                listener = null;
                listenTask = null;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                Port = 0;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private async Task ListenLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await listener.GetContextAsync();
                }
                catch
                {
                    break;
                }

                _ = Task.Run(() => HandleContextAsync(context, cancellationToken), cancellationToken);
            }
        }

        private async Task HandleContextAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            try
            {
                AddHeaders(context.Response);
                string path = context.Request.Url?.AbsolutePath ?? "/";
                if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleApiAsync(context, path, cancellationToken);
                }
                else
                {
                    await HandleStaticAsync(context, path, cancellationToken);
                }
            }
            catch (HttpMethodException ex)
            {
                await WriteJsonAsync(context.Response, new ApiEnvelope(false, ex.Message, null), cancellationToken, HttpStatusCode.MethodNotAllowed);
            }
            catch (Exception ex)
            {
                await WriteJsonAsync(context.Response, new ApiEnvelope(false, ex.Message, null), cancellationToken, HttpStatusCode.InternalServerError);
            }
        }

        private async Task HandleApiAsync(HttpListenerContext context, string path, CancellationToken cancellationToken)
        {
            if (context.Request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                context.Response.Close();
                return;
            }

            string normalizedPath = path.TrimEnd('/').ToLowerInvariant();
            switch (normalizedPath)
            {
                case "/api/state":
                    EnsureMethod(context.Request, "GET");
                    await WriteJsonAsync(context.Response, new ApiEnvelope(true, "ok", await BuildStateDataAsync()), cancellationToken);
                    break;
                case "/api/audio":
                    EnsureMethod(context.Request, "GET");
                    AudioItem[] items = await wsClient.RequestAsync<AudioItem[]>("get_audio_list") ?? Array.Empty<AudioItem>();
                    await WriteJsonAsync(context.Response, new ApiEnvelope(true, "ok", items), cancellationToken);
                    break;
                case "/api/click":
                    EnsureMethod(context.Request, "POST");
                    ClickRequest click = await ReadJsonAsync<ClickRequest>(context.Request);
                    if (click == null || string.IsNullOrWhiteSpace(click.Path))
                    {
                        await WriteJsonAsync(context.Response, new ApiEnvelope(false, "缺少 path", null), cancellationToken, HttpStatusCode.BadRequest);
                        break;
                    }

                    if (modeGetter() == WebClickMode.DirectPlay)
                    {
                        await wsClient.RequestAsync<object>("play_audio", new { path = click.Path });
                    }
                    else
                    {
                        await wsClient.RequestAsync<object>("set_selected_audio", new { path = click.Path });
                    }

                    await WriteJsonAsync(context.Response, new ApiEnvelope(true, "ok", await BuildStateDataAsync()), cancellationToken);
                    break;
                case "/api/stop":
                    EnsureMethod(context.Request, "POST");
                    await wsClient.RequestAsync<object>("stop_audio");
                    await WriteJsonAsync(context.Response, new ApiEnvelope(true, "ok", await BuildStateDataAsync()), cancellationToken);
                    break;
                case "/api/mode":
                    if (context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    {
                        await WriteJsonAsync(context.Response, new ApiEnvelope(true, "ok", BuildModeData()), cancellationToken);
                        break;
                    }

                    EnsureMethod(context.Request, "POST");
                    ModeRequest modeRequest = await ReadJsonAsync<ModeRequest>(context.Request);
                    if (modeRequest != null && TryParseMode(modeRequest.Mode, out WebClickMode parsedMode))
                    {
                        modeSetter(parsedMode);
                    }
                    await WriteJsonAsync(context.Response, new ApiEnvelope(true, "ok", BuildModeData()), cancellationToken);
                    break;
                default:
                    await WriteJsonAsync(context.Response, new ApiEnvelope(false, "接口不存在", null), cancellationToken, HttpStatusCode.NotFound);
                    break;
            }
        }

        private async Task HandleStaticAsync(HttpListenerContext context, string path, CancellationToken cancellationToken)
        {
            if (!TryResolveStaticRoot(out string webRoot, out string[] searchedRoots))
            {
                string message = "未找到静态资源目录 www。已搜索路径：\r\n" + string.Join("\r\n", searchedRoots);
                await WritePlainTextAsync(context.Response, message, cancellationToken, HttpStatusCode.NotFound);
                return;
            }

            string relativePath = path == "/"
                ? "index.html"
                : path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

            string fullPath = Path.GetFullPath(Path.Combine(webRoot, relativePath));
            if (!fullPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase))
            {
                await WriteJsonAsync(context.Response, new ApiEnvelope(false, "拒绝访问", null), cancellationToken, HttpStatusCode.Forbidden);
                return;
            }

            if (!File.Exists(fullPath))
            {
                await WriteJsonAsync(context.Response, new ApiEnvelope(false, "文件不存在", null), cancellationToken, HttpStatusCode.NotFound);
                return;
            }

            byte[] payload = await File.ReadAllBytesAsync(fullPath, cancellationToken);
            context.Response.ContentType = GetContentType(Path.GetExtension(fullPath));
            context.Response.ContentLength64 = payload.Length;
            await context.Response.OutputStream.WriteAsync(payload, 0, payload.Length, cancellationToken);
            context.Response.Close();
        }

        private static bool TryResolveStaticRoot(out string webRoot, out string[] searchedRoots)
        {
            string baseDir = AppContext.BaseDirectory;
            string currentDir = Environment.CurrentDirectory;
            string processDir = Path.GetDirectoryName(Environment.ProcessPath ?? string.Empty) ?? string.Empty;

            string[] candidates = new[]
            {
                Path.Combine(baseDir, "www"),
                Path.Combine(currentDir, "www"),
                Path.Combine(processDir, "www"),
                Path.Combine(baseDir, "Plugin", "MusicalMoments.WithinReach", "www"),
                Path.Combine(currentDir, "Plugin", "MusicalMoments.WithinReach", "www"),
                Path.Combine(processDir, "Plugin", "MusicalMoments.WithinReach", "www")
            }
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path =>
            {
                try
                {
                    return Path.GetFullPath(path);
                }
                catch
                {
                    return path;
                }
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

            searchedRoots = candidates;
            foreach (string candidate in candidates)
            {
                if (Directory.Exists(candidate))
                {
                    webRoot = candidate;
                    return true;
                }
            }

            webRoot = string.Empty;
            return false;
        }

        private static void EnsureMethod(HttpListenerRequest request, string expectedMethod)
        {
            if (!request.HttpMethod.Equals(expectedMethod, StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpMethodException($"仅支持 {expectedMethod}");
            }
        }

        private object BuildModeData()
        {
            WebClickMode mode = modeGetter();
            return new
            {
                mode = mode == WebClickMode.DirectPlay ? "direct_play" : "set_selected",
                modeText = mode == WebClickMode.DirectPlay ? "按下音频后直接播放" : "按下音频后设为播放项"
            };
        }

        private async Task<object> BuildStateDataAsync()
        {
            PluginStateSnapshot state = await wsClient.RequestAsync<PluginStateSnapshot>("get_state");
            PlaybackBehaviorSnapshot behavior = await wsClient.RequestAsync<PlaybackBehaviorSnapshot>("get_playback_behavior");
            WebClickMode mode = modeGetter();
            return new
            {
                connected = state != null,
                mode = mode == WebClickMode.DirectPlay ? "direct_play" : "set_selected",
                isPlaying = state?.IsPlaying ?? false,
                selectedAudioPath = state?.SelectedAudioPath ?? string.Empty,
                currentPlaybackAudioPath = state?.CurrentPlaybackAudioPath ?? string.Empty,
                sameAudioBehavior = behavior?.SameAudioBehavior ?? state?.SameAudioBehavior ?? string.Empty,
                differentAudioBehavior = behavior?.DifferentAudioBehavior ?? state?.DifferentAudioBehavior ?? string.Empty,
                timestamp = DateTime.Now
            };
        }

        private static void AddHeaders(HttpListenerResponse response)
        {
            response.Headers["Cache-Control"] = "no-store";
            response.Headers["Access-Control-Allow-Origin"] = "*";
            response.Headers["Access-Control-Allow-Methods"] = "GET,POST,OPTIONS";
            response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
        }

        private static async Task<T> ReadJsonAsync<T>(HttpListenerRequest request) where T : class
        {
            if (request?.InputStream == null)
            {
                return null;
            }

            using StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
            string body = await reader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(body, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private static async Task WriteJsonAsync(HttpListenerResponse response, ApiEnvelope envelope, CancellationToken cancellationToken, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions);
            response.StatusCode = (int)statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = payload.Length;
            await response.OutputStream.WriteAsync(payload, 0, payload.Length, cancellationToken);
            response.Close();
        }

        private static async Task WritePlainTextAsync(HttpListenerResponse response, string text, CancellationToken cancellationToken, HttpStatusCode statusCode)
        {
            byte[] payload = Encoding.UTF8.GetBytes(text ?? string.Empty);
            response.StatusCode = (int)statusCode;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = payload.Length;
            await response.OutputStream.WriteAsync(payload, 0, payload.Length, cancellationToken);
            response.Close();
        }

        private static int ResolvePort(int preferredPort)
        {
            if (preferredPort > 0 && IsPortAvailable(preferredPort))
            {
                return preferredPort;
            }

            TcpListener temporaryListener = new TcpListener(IPAddress.Loopback, 0);
            temporaryListener.Start();
            int availablePort = ((IPEndPoint)temporaryListener.LocalEndpoint).Port;
            temporaryListener.Stop();
            return availablePort;
        }

        private static bool IsPortAvailable(int port)
        {
            try
            {
                TcpListener temporaryListener = new TcpListener(IPAddress.Loopback, port);
                temporaryListener.Start();
                temporaryListener.Stop();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "application/javascript; charset=utf-8",
                ".json" => "application/json; charset=utf-8",
                ".svg" => "image/svg+xml",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        private static bool TryParseMode(string value, out WebClickMode mode)
        {
            mode = WebClickMode.SetSelected;
            string normalizedValue = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalizedValue)
            {
                case "set_selected":
                case "setselected":
                case "selected":
                    mode = WebClickMode.SetSelected;
                    return true;
                case "direct_play":
                case "directplay":
                case "play":
                    mode = WebClickMode.DirectPlay;
                    return true;
                default:
                    return false;
            }
        }

        private sealed class ClickRequest
        {
            public string Path { get; set; } = string.Empty;
        }

        private sealed class ModeRequest
        {
            public string Mode { get; set; } = string.Empty;
        }

        private sealed class ApiEnvelope
        {
            public ApiEnvelope(bool ok, string message, object data)
            {
                Ok = ok;
                Message = message;
                Data = data;
            }

            public bool Ok { get; }
            public string Message { get; }
            public object Data { get; }
        }

        private sealed class HttpMethodException : Exception
        {
            public HttpMethodException(string message) : base(message)
            {
            }
        }
    }
}
