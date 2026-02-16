using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MusicalMoments
{
    public class PluginSDK
    {
        public class PluginInfo
        {
            public string PluginDescription { get; set; } = string.Empty;
            public string PluginVersion { get; set; } = "1.0.0";
            public string Protocol { get; set; } = "ws";
        }

        public sealed class PluginStateSnapshot
        {
            public string SdkVersion { get; set; } = "ws-2.1.0";
            public string AppVersion { get; set; } = string.Empty;
            public string RunningDirectory { get; set; } = string.Empty;
            public bool IsPlaying { get; set; }
            public bool PlayAudioEnabled { get; set; }
            public string PlayAudioKey { get; set; } = string.Empty;
            public string ToggleStreamKey { get; set; } = string.Empty;
            public float VBVolume { get; set; }
            public float Volume { get; set; }
            public float TipsVolume { get; set; }
            public bool IsVBInstalled { get; set; }
            public bool IsAdministrator { get; set; }
            public string SelectedAudioPath { get; set; } = string.Empty;
            public string CurrentPlaybackAudioPath { get; set; } = string.Empty;
            public string SameAudioBehavior { get; set; } = string.Empty;
            public string DifferentAudioBehavior { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }

        public static class PluginServer
        {
            private sealed class PluginRequest
            {
                public string Id { get; set; } = string.Empty;
                public string Action { get; set; } = string.Empty;
                public JsonElement Data { get; set; }
                public JsonElement Params { get; set; }
            }

            private sealed class PluginResponse
            {
                public string Id { get; set; } = string.Empty;
                public string Action { get; set; } = string.Empty;
                public bool Ok { get; set; }
                public string Message { get; set; } = string.Empty;
                public object Data { get; set; }
                public DateTime Timestamp { get; set; } = DateTime.Now;
            }

            private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            private static readonly ConcurrentDictionary<Guid, WebSocket> ClientSockets = new ConcurrentDictionary<Guid, WebSocket>();
            private static HttpListener listener;
            private static CancellationTokenSource cancellationTokenSource;
            private static Task listenerTask;
            private static int port;

            public static bool IsRunning => listener != null && listener.IsListening;

            public static void StartServer()
            {
                if (IsRunning)
                {
                    return;
                }

                port = GetAvailablePort();
                listener = new HttpListener();
                listener.Prefixes.Add($"http://127.0.0.1:{port}/ws/");
                listener.Start();

                cancellationTokenSource = new CancellationTokenSource();
                listenerTask = Task.Run(() => ListenAsync(cancellationTokenSource.Token));
            }

            public static void StopServer()
            {
                if (!IsRunning)
                {
                    return;
                }

                try { cancellationTokenSource.Cancel(); } catch { }
                try { listener.Stop(); listener.Close(); } catch { }
                try { listenerTask?.Wait(1500); } catch { }

                foreach (var item in ClientSockets.ToArray())
                {
                    if (ClientSockets.TryRemove(item.Key, out WebSocket socket))
                    {
                        CloseSocketSafely(socket);
                    }
                }

                listener = null;
                listenerTask = null;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }

            public static string GetServerAddress()
            {
                return port <= 0 ? string.Empty : $"ws://127.0.0.1:{port}/ws/";
            }

            private static async Task ListenAsync(CancellationToken cancellationToken)
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

            private static async Task HandleContextAsync(HttpListenerContext context, CancellationToken cancellationToken)
            {
                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    byte[] bytes = Encoding.UTF8.GetBytes("This endpoint requires WebSocket.");
                    context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    context.Response.Close();
                    return;
                }

                WebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                WebSocket socket = wsContext.WebSocket;
                Guid id = Guid.NewGuid();
                ClientSockets[id] = socket;

                try
                {
                    await SendAsync(socket, new PluginResponse
                    {
                        Action = "hello",
                        Ok = true,
                        Message = "Connected",
                        Data = BuildStateSnapshot()
                    }, cancellationToken);

                    while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
                    {
                        string payload = await ReceiveTextAsync(socket, cancellationToken);
                        if (payload == null)
                        {
                            break;
                        }

                        PluginRequest request = ParseRequest(payload);
                        PluginResponse response = ProcessRequest(request);
                        await SendAsync(socket, response, cancellationToken);
                    }
                }
                catch
                {
                    // Ignore per-client failures.
                }
                finally
                {
                    if (ClientSockets.TryRemove(id, out WebSocket removed))
                    {
                        CloseSocketSafely(removed);
                    }
                }
            }

            private static PluginRequest ParseRequest(string payload)
            {
                if (string.IsNullOrWhiteSpace(payload))
                {
                    return new PluginRequest();
                }

                try
                {
                    return JsonSerializer.Deserialize<PluginRequest>(payload, JsonOptions) ?? new PluginRequest();
                }
                catch
                {
                    return new PluginRequest();
                }
            }

            private static PluginResponse ProcessRequest(PluginRequest request)
            {
                string action = request?.Action?.Trim() ?? string.Empty;
                JsonElement data = request.Data.ValueKind == JsonValueKind.Object ? request.Data : request.Params;
                PluginResponse response = new PluginResponse
                {
                    Id = request?.Id ?? string.Empty,
                    Action = action,
                    Ok = true
                };

                switch (action.ToLowerInvariant())
                {
                    case "ping":
                        response.Message = "pong";
                        break;
                    case "sdk_version":
                        response.Data = new { version = "ws-2.1.0" };
                        break;
                    case "get_capabilities":
                        response.Data = new { actions = new[] { "get_state", "get_audio_list", "get_audio_page", "set_selected_audio", "play_selected_audio", "play_audio", "stop_audio", "toggle_play_audio", "get_devices", "get_playback_behavior", "set_playback_behavior", "reload_audio_library" } };
                        break;
                    case "get_state":
                        response.Data = BuildStateSnapshot();
                        break;
                    case "list_audio":
                    case "get_audio_list":
                        response.Data = BuildAudioItems(MainWindow.audioInfo.OrderBy(item => item.Name));
                        break;
                    case "get_audio_page":
                        {
                            int page = Math.Max(1, ReadInt(data, "page", 1));
                            int pageSize = Math.Clamp(ReadInt(data, "pageSize", 30), 1, 200);
                            string keyword = ReadString(data, "keyword");
                            var filtered = MainWindow.audioInfo.Where(item =>
                                string.IsNullOrWhiteSpace(keyword)
                                || item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                || item.Track.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                || item.FileType.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                || item.Key.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase)).OrderBy(item => item.Name).ToList();
                            int total = filtered.Count;
                            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
                            page = Math.Clamp(page, 1, totalPages);
                            response.Data = new
                            {
                                page,
                                pageSize,
                                total,
                                totalPages,
                                items = BuildAudioItems(filtered.Skip((page - 1) * pageSize).Take(pageSize))
                            };
                            break;
                        }
                    case "reload_audio_library":
                        response.Data = new { queued = QueueReloadAudioLibrary() };
                        break;
                    case "set_selected_audio":
                        {
                            if (!TryResolveAudioPath(data, out string selectedPath))
                            {
                                response.Ok = false;
                                response.Message = "未找到目标音频。";
                                break;
                            }

                            response.Ok = MainWindow.TrySetSelectedAudio(selectedPath);
                            response.Data = BuildSelectedAudioData();
                            break;
                        }
                    case "play_selected_audio":
                        response.Ok = MainWindow.RequestPlayAudio(MainWindow.selectedAudioPath, countAsPlayed: true);
                        response.Data = BuildSelectedAudioData();
                        break;
                    case "play_audio":
                        {
                            if (!TryResolveAudioPath(data, out string playPath))
                            {
                                response.Ok = false;
                                response.Message = "未找到目标音频。";
                                break;
                            }

                            MainWindow.TrySetSelectedAudio(playPath);
                            response.Ok = MainWindow.RequestPlayAudio(playPath, countAsPlayed: true);
                            response.Data = BuildSelectedAudioData();
                            break;
                        }
                    case "stop_audio":
                        MainWindow.TryStopPlayback();
                        response.Data = new { stopped = true };
                        break;
                    case "toggle_play_audio":
                        {
                            bool? enabled = ReadBool(data, "enabled");
                            MainWindow.playAudio = enabled ?? !MainWindow.playAudio;
                            if (!MainWindow.playAudio)
                            {
                                MainWindow.TryStopPlayback();
                            }

                            response.Data = new { playAudioEnabled = MainWindow.playAudio };
                            break;
                        }
                    case "get_devices":
                        response.Data = new
                        {
                            inputDevices = Misc.GetInputAudioDeviceNames(),
                            outputDevices = Misc.GetOutputAudioDeviceNames(),
                            selected = new
                            {
                                vbInput = MainWindow.VBInputComboSelect,
                                physicalInput = MainWindow.VBOutputComboSelect,
                                vbOutput = MainWindow.InputComboSelect,
                                physicalOutput = MainWindow.OutputComboSelect
                            }
                        };
                        break;
                    case "get_playback_behavior":
                        response.Data = BuildPlaybackBehaviorData();
                        break;
                    case "set_playback_behavior":
                        {
                            if (Enum.TryParse(ReadString(data, "sameAudioBehavior"), true, out SameAudioPressBehavior same))
                            {
                                MainWindow.sameAudioPressBehavior = same;
                            }

                            if (Enum.TryParse(ReadString(data, "differentAudioBehavior"), true, out DifferentAudioInterruptBehavior diff))
                            {
                                MainWindow.differentAudioInterruptBehavior = diff;
                            }

                            response.Data = BuildPlaybackBehaviorData();
                            break;
                        }
                    default:
                        response.Ok = false;
                        response.Message = $"Unsupported action: {action}";
                        break;
                }

                return response;
            }

            private static PluginStateSnapshot BuildStateSnapshot()
            {
                return new PluginStateSnapshot
                {
                    AppVersion = MainWindow.nowVer,
                    RunningDirectory = MainWindow.runningDirectory,
                    IsPlaying = AudioPlaybackService.IsPlaybackActive,
                    PlayAudioEnabled = MainWindow.playAudio,
                    PlayAudioKey = MainWindow.playAudioKey.ToString(),
                    ToggleStreamKey = MainWindow.toggleStreamKey.ToString(),
                    VBVolume = MainWindow.VBvolume * 100f,
                    Volume = MainWindow.volume * 100f,
                    TipsVolume = MainWindow.tipsvolume * 100f,
                    IsVBInstalled = SystemAudioService.CheckVirtualCableInstalled(),
                    IsAdministrator = Misc.IsAdministrator(),
                    SelectedAudioPath = MainWindow.selectedAudioPath ?? string.Empty,
                    CurrentPlaybackAudioPath = MainWindow.CurrentPlaybackAudioPath ?? string.Empty,
                    SameAudioBehavior = MainWindow.sameAudioPressBehavior.ToString(),
                    DifferentAudioBehavior = MainWindow.differentAudioInterruptBehavior.ToString(),
                    Timestamp = DateTime.Now
                };
            }

            private static object BuildAudioItems(IEnumerable<AudioInfo> source)
            {
                return source.Select((item, index) => new
                {
                    index,
                    item.Name,
                    item.Track,
                    item.FileType,
                    item.FilePath,
                    key = item.Key.ToString(),
                    exists = File.Exists(item.FilePath)
                }).ToArray();
            }

            private static bool QueueReloadAudioLibrary()
            {
                MainWindow window = MainWindow.CurrentInstance;
                if (window == null || window.IsDisposed)
                {
                    return false;
                }

                try
                {
                    window.BeginInvoke(new Action(async () => await window.reLoadList()));
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            private static bool TryResolveAudioPath(JsonElement data, out string path)
            {
                path = string.Empty;
                string inputPath = ReadString(data, "path");
                if (!string.IsNullOrWhiteSpace(inputPath))
                {
                    string fullPath = Path.GetFullPath(inputPath);
                    if (File.Exists(fullPath))
                    {
                        path = fullPath;
                        return true;
                    }
                }

                int index = ReadInt(data, "index", -1);
                if (index >= 0)
                {
                    var list = MainWindow.audioInfo.OrderBy(item => item.Name).ToList();
                    if (index < list.Count)
                    {
                        path = list[index].FilePath;
                        return File.Exists(path);
                    }
                }

                string name = ReadString(data, "name");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    AudioInfo matched = MainWindow.audioInfo.FirstOrDefault(item =>
                        string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)
                        || item.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
                    if (matched != null && File.Exists(matched.FilePath))
                    {
                        path = matched.FilePath;
                        return true;
                    }
                }

                return false;
            }

            private static object BuildSelectedAudioData()
            {
                string path = MainWindow.selectedAudioPath ?? string.Empty;
                return new
                {
                    path,
                    name = string.IsNullOrWhiteSpace(path) ? string.Empty : Path.GetFileNameWithoutExtension(path),
                    exists = !string.IsNullOrWhiteSpace(path) && File.Exists(path)
                };
            }

            private static object BuildPlaybackBehaviorData()
            {
                return new
                {
                    sameAudioBehavior = MainWindow.sameAudioPressBehavior.ToString(),
                    sameAudioBehaviorText = PlaybackBehaviorText.ToDisplayText(MainWindow.sameAudioPressBehavior),
                    differentAudioBehavior = MainWindow.differentAudioInterruptBehavior.ToString(),
                    differentAudioBehaviorText = PlaybackBehaviorText.ToDisplayText(MainWindow.differentAudioInterruptBehavior)
                };
            }

            private static string ReadString(JsonElement data, string name)
            {
                if (data.ValueKind == JsonValueKind.Object && TryGetProperty(data, name, out JsonElement value))
                {
                    return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
                }

                return string.Empty;
            }

            private static int ReadInt(JsonElement data, string name, int defaultValue)
            {
                if (data.ValueKind == JsonValueKind.Object && TryGetProperty(data, name, out JsonElement value))
                {
                    if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number))
                    {
                        return number;
                    }

                    if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out int parsed))
                    {
                        return parsed;
                    }
                }

                return defaultValue;
            }

            private static bool? ReadBool(JsonElement data, string name)
            {
                if (data.ValueKind == JsonValueKind.Object && TryGetProperty(data, name, out JsonElement value))
                {
                    if (value.ValueKind == JsonValueKind.True)
                    {
                        return true;
                    }

                    if (value.ValueKind == JsonValueKind.False)
                    {
                        return false;
                    }

                    if (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out bool parsed))
                    {
                        return parsed;
                    }
                }

                return null;
            }

            private static bool TryGetProperty(JsonElement obj, string name, out JsonElement value)
            {
                value = default;
                if (obj.TryGetProperty(name, out value))
                {
                    return true;
                }

                foreach (JsonProperty property in obj.EnumerateObject())
                {
                    if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }

                return false;
            }

            private static async Task<string> ReceiveTextAsync(WebSocket socket, CancellationToken cancellationToken)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
                try
                {
                    using MemoryStream stream = new MemoryStream();
                    while (true)
                    {
                        WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            return null;
                        }

                        if (result.Count > 0)
                        {
                            stream.Write(buffer, 0, result.Count);
                        }

                        if (result.EndOfMessage)
                        {
                            break;
                        }
                    }

                    return Encoding.UTF8.GetString(stream.ToArray());
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            private static async Task SendAsync(WebSocket socket, PluginResponse response, CancellationToken cancellationToken)
            {
                byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response, JsonOptions));
                await socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, cancellationToken);
            }

            private static int GetAvailablePort()
            {
                TcpListener tempListener = new TcpListener(IPAddress.Loopback, 0);
                tempListener.Start();
                int availablePort = ((IPEndPoint)tempListener.LocalEndpoint).Port;
                tempListener.Stop();
                return availablePort;
            }

            private static void CloseSocketSafely(WebSocket socket)
            {
                if (socket == null)
                {
                    return;
                }

                try
                {
                    if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                    {
                        socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server stopping", CancellationToken.None).GetAwaiter().GetResult();
                    }
                }
                catch
                {
                    // Ignore close failures.
                }
                finally
                {
                    socket.Dispose();
                }
            }
        }
    }
}
