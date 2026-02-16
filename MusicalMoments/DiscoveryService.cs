using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class DiscoveryService
    {
        public static async Task GetDownloadJsonFromFile(string filePath, ListView listView, ListBox downloadLinkListBox)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"文件不存在: {filePath}");
                    return;
                }

                string jsonResponse = await File.ReadAllTextAsync(filePath);
                JObject json = JObject.Parse(jsonResponse);
                if (json["files"] == null || json["files"]!.Type != JTokenType.Array)
                {
                    MessageBox.Show("本地 JSON 异常！将尝试重新获取");
                    await DownloadJsonFile("https://www.scmd.cc/api/all-audio", filePath);
                    return;
                }

                listView.Items.Clear();
                downloadLinkListBox.Items.Clear();

                foreach (JToken file in json["files"]!)
                {
                    string link = file["download-link"]?.ToString();
                    if (string.IsNullOrEmpty(link))
                    {
                        continue;
                    }

                    int markerIndex = link.IndexOf("/DATA/", StringComparison.OrdinalIgnoreCase);
                    if (markerIndex < 0)
                    {
                        Console.WriteLine($"链接中未找到 '/DATA/'：{link}");
                        continue;
                    }

                    string name = link.Substring(markerIndex + 6);
                    string uploader = file["uploader"]?.ToString() ?? "未知";
                    string downloadCount = file["download-count"]?.ToString() ?? "0";

                    ListViewItem item = new ListViewItem(name);
                    item.SubItems.Add(downloadCount);
                    item.SubItems.Add(uploader);
                    listView.Items.Add(item);
                    downloadLinkListBox.Items.Add(link);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取 JSON 文件时出错: {ex.Message}");
            }
        }

        public static async Task<int?> GetTotalFromJsonFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                string jsonResponse = await File.ReadAllTextAsync(filePath);
                JObject json = JObject.Parse(jsonResponse);
                if (json["total"] == null || json["total"]!.Type != JTokenType.Integer)
                {
                    MessageBox.Show("JSON 数据中未找到 'total' 字段或格式错误！");
                    return null;
                }

                return json["total"]!.Value<int>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取 JSON 文件时出错: {ex.Message}");
                return null;
            }
        }

        public static async Task DownloadJsonFile(string url, string filePath)
        {
            try
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();
                await File.WriteAllTextAsync(filePath, jsonResponse);
                Console.WriteLine($"JSON 数据已保存至: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下载 JSON 时出错: {ex.Message}");
                MessageBox.Show($"下载 JSON 时出错: {ex.Message}");
            }
        }

        public static async Task<bool> VerifyFileHashAsync(string filePath, string hashUrl)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"文件未找到: {filePath}");
                    return false;
                }

                string localHash;
                using (MD5 md5 = MD5.Create())
                await using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = await md5.ComputeHashAsync(stream);
                    localHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }

                using HttpClient client = new HttpClient();
                string response = await client.GetStringAsync(hashUrl);
                JObject json = JObject.Parse(response);
                string cloudHash = json["hash"]?.ToString();
                if (string.IsNullOrEmpty(cloudHash))
                {
                    Console.WriteLine("未能从云端获取哈希值。");
                    return false;
                }

                return string.Equals(localHash, cloudHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"验证哈希值时出错: {ex.Message}");
                return false;
            }
        }
    }
}
