using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MusicalMoments
{
    public class PluginSDK
    {
        public class PluginInfo
        {
            public string PluginDescription { get; set; }
            public string PluginVersion { get; set; }
        }
        public class PluginServer
        {
            private static TcpListener listener;
            private static int port;
            private static CancellationTokenSource cancellationTokenSource;
            private static Task listenerTask;

            public static void StartServer()
            {
                if (listener == null || !listener.Server.IsBound)
                {
                    port = GetAvailablePort();
                    listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    cancellationTokenSource = new CancellationTokenSource();
                    listenerTask = Task.Run(() => Listen(cancellationTokenSource.Token));
                }
            }

            public static void StopServer()
            {
                if (listener != null && listener.Server.IsBound)
                {
                    cancellationTokenSource.Cancel();
                    listener.Stop();
                    listenerTask.Wait(); // 等待任务完成
                }
            }

            private static void Listen(CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        Thread clientThread = new Thread(() => HandleClient(client));
                        clientThread.Start();
                    }
                    catch (SocketException ex)
                    {
                        // 如果捕获到异常，则检查是否是由于取消操作而引起的
                        if (ex.SocketErrorCode != SocketError.Interrupted)
                        {
                            throw;
                        }
                    }
                }
            }

            private static void HandleClient(TcpClient client)
            {
                // 获取客户端请求数据
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("收到请求：\n" + request);

                // 解析请求
                string[] requestLines = request.Split('\n');
                string[] requestParts = requestLines[0].Split(' ');
                string method = requestParts[0];
                string endpoint = requestParts[1];

                // 参数请求
                string[] endpointParts = endpoint.Split('?');
                string endpointWithoutParams = endpointParts[0];
                string[] queryParams = null;
                if (endpointParts.Length > 1)
                {
                    queryParams = endpointParts[1].Split('&');
                }

                // 处理GET请求
                if (method == "GET")
                {
                    // 模拟一些API端点
                    if (endpoint == "/api/SDKVer")
                    {
                        // 返回一些示例数据
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"  + "V1.0.0";
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (endpoint == "/api/isPlaying") // 是否在播放中
                    {
                        // 返回 isPlaying 变量的值
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"  + (Misc.currentOutputDevice != null ? "true" : "false");
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (endpoint == "/api/playAudio") // 是否正在使用音频源
                    {
                        // 返回 playAudio 变量的值
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"  + (MainWindow.playAudio ? "true" : "false");
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (endpoint == "/api/nowVer") // 当前版本
                    {
                        // 返回 nowVer 变量的值
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"  + MainWindow.nowVer;
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (endpoint == "/api/runningDirectory") // 程序运行目录
                    {
                        // 返回 runningDirectory 变量的值
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"  + MainWindow.runningDirectory;
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (endpoint == "/api/playAudioKey") // 播放音频按键
                    {
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"  + MainWindow.playAudioKey.ToString();
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (endpoint == "/api/toggleStreamKey") // 切换源按键
                    {
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"  + MainWindow.toggleStreamKey.ToString();
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (endpoint == "/api/VBvolume") // 切换源按键
                    {
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"  + (MainWindow.VBvolume * 100f).ToString();
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (endpoint == "/api/volume") // 切换源按键
                    {
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"  + (MainWindow.volume * 100f).ToString();
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (endpoint == "/api/tipsvolume") // 切换源按键
                    {
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"  + (MainWindow.tipsvolume * 100f).ToString();
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (endpointWithoutParams == "/api/listViewInfo")
                    {
                        string result = "";

                        if (queryParams != null)
                        {
                            foreach (string param in queryParams)
                            {
                                string[] Parts = param.Split('=');
                                if (Parts[0] == "type" && Parts[1] == "json")
                                {
                                    result = JsonConvert.SerializeObject(MainWindow.audioInfo, Formatting.None);
                                    break;
                                }
                            }
                        }
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: application/json; charset=utf-8\r\n\r\n" + result;
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }



                }
                client.Close();
            }

            private static int GetAvailablePort()
            {
                TcpListener tempListener = new TcpListener(IPAddress.Any, 0);
                tempListener.Start();
                int port = ((IPEndPoint)tempListener.LocalEndpoint).Port;
                tempListener.Stop();
                return port;
            }

            public static string GetServerAddress()
            {
                return $"http://localhost:{port}/";
            }
        }
    }
}
