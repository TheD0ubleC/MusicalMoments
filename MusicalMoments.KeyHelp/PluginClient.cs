using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MusicalMoments.KeyHelp
{
    internal sealed class PluginWsClient : IDisposable
    {
        private readonly Uri serverUri;
        private ClientWebSocket socket;
        private readonly SemaphoreSlim requestLock = new SemaphoreSlim(1, 1);

        public bool HasAddress => serverUri != null;

        public PluginWsClient(string address)
        {
            string normalizedAddress = string.IsNullOrWhiteSpace(address)
                ? string.Empty
                : address.Trim().Trim('"');
            if (!string.IsNullOrWhiteSpace(normalizedAddress)
                && Uri.TryCreate(normalizedAddress, UriKind.Absolute, out Uri uri)
                && (uri.Scheme.Equals("ws", StringComparison.OrdinalIgnoreCase)
                    || uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase)))
            {
                serverUri = uri;
            }
        }

        public async Task<PluginStateSnapshot> GetStateAsync()
        {
            if (serverUri == null)
            {
                PluginLog.Write("GetStateAsync: serverUri 为空。");
                return null;
            }

            await requestLock.WaitAsync();
            try
            {
                if (!await EnsureConnectedAsync())
                {
                    return null;
                }

                string requestId = Guid.NewGuid().ToString("N");
                string requestJson = JsonSerializer.Serialize(new WsRequest
                {
                    Id = requestId,
                    Action = "get_state"
                }, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
                await socket.SendAsync(new ArraySegment<byte>(requestBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                string responseJson = await ReceiveTextAsync(socket);
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    PluginLog.Write("GetStateAsync: 收到空响应。");
                    return null;
                }

                WsEnvelope envelope = JsonSerializer.Deserialize<WsEnvelope>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (envelope == null || !envelope.Ok || envelope.Data.ValueKind == JsonValueKind.Undefined)
                {
                    string status = envelope == null ? "envelope=null" : $"ok={envelope.Ok}, message={envelope.Message}";
                    PluginLog.Write($"GetStateAsync: 响应无效，{status}。原始响应={responseJson}");
                    return null;
                }

                return envelope.Data.Deserialize<PluginStateSnapshot>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                PluginLog.Write("GetStateAsync: 请求失败，已关闭 socket。");
                CloseSocket();
                return null;
            }
            finally
            {
                requestLock.Release();
            }
        }

        private async Task<bool> EnsureConnectedAsync()
        {
            if (socket != null && socket.State == WebSocketState.Open)
            {
                return true;
            }

            CloseSocket();
            socket = new ClientWebSocket();
            try
            {
                await socket.ConnectAsync(serverUri, CancellationToken.None);
                if (socket.State != WebSocketState.Open)
                {
                    PluginLog.Write($"EnsureConnectedAsync: 连接后状态异常: {socket.State}");
                    CloseSocket();
                    return false;
                }

                _ = await ReceiveTextAsync(socket);
                PluginLog.Write($"EnsureConnectedAsync: 连接成功 {serverUri}");
                return true;
            }
            catch
            {
                PluginLog.Write($"EnsureConnectedAsync: 连接失败 {serverUri}");
                CloseSocket();
                return false;
            }
        }

        private static async Task<string> ReceiveTextAsync(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[4096];
            using MemoryStream memoryStream = new MemoryStream();
            while (true)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }

                memoryStream.Write(buffer, 0, result.Count);
                if (result.EndOfMessage)
                {
                    break;
                }
            }

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        private void CloseSocket()
        {
            if (socket == null)
            {
                return;
            }

            try
            {
                if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                {
                    socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
            }
            catch
            {
                // Ignore close errors.
            }
            finally
            {
                socket.Dispose();
                socket = null;
            }
        }

        public void Dispose()
        {
            CloseSocket();
            requestLock.Dispose();
        }

        private sealed class WsRequest
        {
            public string Id { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
        }

        private sealed class WsEnvelope
        {
            public bool Ok { get; set; }
            public string Message { get; set; } = string.Empty;
            public JsonElement Data { get; set; }
        }
    }

    internal sealed class PluginStateSnapshot
    {
        public string SdkVersion { get; set; } = string.Empty;
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
        public DateTime Timestamp { get; set; }
    }

    internal static class PluginLog
    {
        private static readonly object SyncRoot = new object();

        public static void Write(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugin.log");
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
                lock (SyncRoot)
                {
                    File.AppendAllText(logPath, line, Encoding.UTF8);
                }
            }
            catch
            {
                // Ignore logging failures.
            }
        }
    }
}
