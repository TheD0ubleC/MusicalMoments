using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MusicalMoments.PluginExample
{
    internal sealed class PluginWsClient : IDisposable
    {
        private readonly Uri serverUri;
        private readonly SemaphoreSlim requestLock = new SemaphoreSlim(1, 1);
        private ClientWebSocket socket;

        public bool HasAddress => serverUri != null;

        public PluginWsClient(string address)
        {
            if (!string.IsNullOrWhiteSpace(address)
                && Uri.TryCreate(address.Trim().Trim('"'), UriKind.Absolute, out Uri uri)
                && (uri.Scheme.Equals("ws", StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase)))
            {
                serverUri = uri;
            }
        }

        public async Task<T> RequestAsync<T>(string action, object data = null)
        {
            if (serverUri == null)
            {
                return default;
            }

            await requestLock.WaitAsync();
            try
            {
                if (!await EnsureConnectedAsync())
                {
                    return default;
                }

                string requestJson = JsonSerializer.Serialize(new
                {
                    id = Guid.NewGuid().ToString("N"),
                    action,
                    data
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
                await socket.SendAsync(new ArraySegment<byte>(requestBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                string responseJson = await ReceiveTextAsync(socket);
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    return default;
                }

                WsEnvelope envelope = JsonSerializer.Deserialize<WsEnvelope>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (envelope == null || !envelope.Ok || envelope.Data.ValueKind == JsonValueKind.Undefined || envelope.Data.ValueKind == JsonValueKind.Null)
                {
                    return default;
                }

                return envelope.Data.Deserialize<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                CloseSocket();
                return default;
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
                    CloseSocket();
                    return false;
                }

                _ = await ReceiveTextAsync(socket);
                return true;
            }
            catch
            {
                CloseSocket();
                return false;
            }
        }

        private static async Task<string> ReceiveTextAsync(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[4096];
            using MemoryStream stream = new MemoryStream();
            while (true)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }

                stream.Write(buffer, 0, result.Count);
                if (result.EndOfMessage)
                {
                    break;
                }
            }

            return Encoding.UTF8.GetString(stream.ToArray());
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
                    socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None).GetAwaiter().GetResult();
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

    internal sealed class AudioItem
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
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
