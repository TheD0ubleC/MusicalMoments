using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class PluginCatalogService
    {
        public static void AddPluginFilesToListView(string rootDirectoryPath, ListView listView)
        {
            listView.Items.Clear();
            if (string.IsNullOrWhiteSpace(rootDirectoryPath) || !Directory.Exists(rootDirectoryPath))
            {
                return;
            }

            string[] subDirectories = Directory.GetDirectories(rootDirectoryPath);

            foreach (string subDirectory in subDirectories)
            {
                string directoryName = new DirectoryInfo(subDirectory).Name;
                string exeFilePath = Path.Combine(subDirectory, directoryName + ".exe");
                if (!File.Exists(exeFilePath))
                {
                    exeFilePath = Directory.GetFiles(subDirectory, "*.exe", SearchOption.TopDirectoryOnly)
                        .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                        .FirstOrDefault() ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(exeFilePath) || !File.Exists(exeFilePath))
                {
                    continue;
                }

                string jsonFilePath = Path.Combine(subDirectory, directoryName + ".json");
                if (!File.Exists(jsonFilePath))
                {
                    jsonFilePath = Path.ChangeExtension(exeFilePath, ".json");
                }
                string description = "无描述";
                string version = "无版本";

                if (File.Exists(exeFilePath) && File.Exists(jsonFilePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(jsonFilePath);
                        JObject jsonObject = JObject.Parse(jsonContent);
                        string protocol = jsonObject["Protocol"]?.ToString() ?? string.Empty;
                        if (!protocol.Equals("ws", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        PluginSDK.PluginInfo pluginInfo = jsonObject.ToObject<PluginSDK.PluginInfo>();
                        if (pluginInfo != null)
                        {
                            description = pluginInfo.PluginDescription;
                            version = pluginInfo.PluginVersion;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                ListViewItem item = new ListViewItem(new[] { directoryName, description, version })
                {
                    Tag = exeFilePath
                };
                listView.Items.Add(item);
            }
        }
    }
}
