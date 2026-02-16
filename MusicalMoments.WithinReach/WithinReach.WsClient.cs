using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MusicalMoments.WithinReach
{
    internal sealed class PluginWsClient : IDisposable
    {
        private static readonly JsonSerializerOptions RequestOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions ResponseOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly Uri serverUri;
        private readonly SemaphoreSlim requestLock = new SemaphoreSlim(1, 1);
        private ClientWebSocket socket;

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

                string requestJson = JsonSerializer.Serialize(new WsRequest
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Action = action,
                    Data = data
                }, RequestOptions);

                byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
                await socket.SendAsync(new ArraySegment<byte>(requestBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                string responseJson = await ReceiveTextAsync(socket);
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    return default;
                }

                WsEnvelope envelope = JsonSerializer.Deserialize<WsEnvelope>(responseJson, ResponseOptions);
                if (envelope == null || !envelope.Ok)
                {
                    return default;
                }

                if (envelope.Data.ValueKind == JsonValueKind.Null || envelope.Data.ValueKind == JsonValueKind.Undefined)
                {
                    return default;
                }

                return envelope.Data.Deserialize<T>(ResponseOptions);
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

        private sealed class WsRequest
        {
            public string Id { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public object Data { get; set; }
        }

        private sealed class WsEnvelope
        {
            public bool Ok { get; set; }
            public string Message { get; set; } = string.Empty;
            public JsonElement Data { get; set; }
        }
    }
}
