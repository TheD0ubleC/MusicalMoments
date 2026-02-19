using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class AudioLibraryService
    {
        public static async Task AddAudioFilesToListView(string directoryPath, ListView listView)
        {
            if (listView == null)
            {
                return;
            }

            listView.BeginUpdate();
            listView.Items.Clear();
            foreach (AudioInfo audio in LoadAudioInfos(directoryPath))
            {
                string keyText = audio.Key == Keys.None ? "未绑定" : KeyBindingService.GetDisplayTextForKey(audio.Key);
                ListViewItem item = new ListViewItem(new[] { audio.Name, audio.Track, audio.FileType, keyText })
                {
                    Tag = audio.FilePath
                };
                listView.Items.Add(item);
            }

            listView.EndUpdate();
            await Task.CompletedTask;
        }

        public static List<AudioInfo> LoadAudioInfos(string directoryPath)
        {
            List<AudioInfo> result = new List<AudioInfo>();
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                return result;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            FileInfo[] audioFiles = dirInfo
                .GetFiles("*.*", SearchOption.TopDirectoryOnly)
                .Where(file =>
                    file.Extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase) ||
                    file.Extension.Equals(".wav", StringComparison.OrdinalIgnoreCase) ||
                    file.Extension.Equals(".flac", StringComparison.OrdinalIgnoreCase) ||
                    file.Extension.Equals(".falc", StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (FileInfo audioFile in audioFiles)
            {
                string track = "未知";
                try
                {
                    using TagLib.File fileTag = TagLib.File.Create(audioFile.FullName);
                    if (!string.IsNullOrWhiteSpace(fileTag.Tag.Title))
                    {
                        track = fileTag.Tag.Title;
                    }
                }
                catch
                {
                    // Ignore metadata read failures and use fallback text.
                }

                result.Add(new AudioInfo
                {
                    Name = Path.GetFileNameWithoutExtension(audioFile.Name),
                    Track = track,
                    FileType = audioFile.Extension.TrimStart('.').ToUpperInvariant(),
                    FilePath = audioFile.FullName,
                    Key = ReadKeyValue(Path.ChangeExtension(audioFile.FullName, ".json"))
                });
            }

            return result;
        }

        public static void WriteKeyJsonInfo(string jsonFilePath, string valueToWrite)
        {
            try
            {
                if (File.Exists(jsonFilePath))
                {
                    string jsonContent = File.ReadAllText(jsonFilePath);
                    dynamic jsonObject = JsonConvert.DeserializeObject(jsonContent);
                    if (jsonObject == null)
                    {
                        jsonObject = new JObject();
                    }

                    jsonObject["Key"] = valueToWrite;
                    File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(jsonObject));
                }
                else
                {
                    JObject jsonObject = new JObject
                    {
                        ["Key"] = valueToWrite
                    };
                    File.WriteAllText(jsonFilePath, jsonObject.ToString());
                }
            }
            catch
            {
                // Keep original behavior: ignore and continue.
            }
        }

        public static int NcmConvert(string filePath, string outputPath = "default")
        {
            NeteaseCrypt neteaseCrypt = new NeteaseCrypt(filePath);
            int result = neteaseCrypt.Dump();
            neteaseCrypt.FixMetadata();
            if (outputPath != "default")
            {
                string fileSuffix = File.Exists(filePath.Replace("ncm", "flac", StringComparison.OrdinalIgnoreCase)) ? "flac" : "mp3";
                string sourceFileName = filePath.Replace("ncm", fileSuffix, StringComparison.OrdinalIgnoreCase);
                try
                {
                    File.Copy(
                        sourceFileName,
                        AppDomain.CurrentDomain.BaseDirectory + "AudioData\\" + Path.GetFileNameWithoutExtension(outputPath + ".mp3"),
                        true);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"错误：{ex.Message}");
                }
            }

            return result;
        }

        private static string ReadKeyJsonInfo(string jsonFilePath)
        {
            string info = "未绑定";
            try
            {
                if (File.Exists(jsonFilePath))
                {
                    string jsonContent = File.ReadAllText(jsonFilePath);
                    dynamic jsonObject = JsonConvert.DeserializeObject(jsonContent);
                    if (jsonObject != null && jsonObject["Key"] != null)
                    {
                        string key = jsonObject["Key"].ToString();
                        info = string.IsNullOrEmpty(key) || key == "None" ? "未绑定" : key;
                    }
                    else
                    {
                        JObject emptyValue = new JObject { ["Key"] = string.Empty };
                        File.WriteAllText(jsonFilePath, emptyValue.ToString());
                    }
                }
                else
                {
                    File.WriteAllText(jsonFilePath, "{\"Key\":\"\"}");
                }
            }
            catch
            {
                // Keep original behavior: ignore and continue.
            }

            return info;
        }

        private static Keys ReadKeyValue(string jsonFilePath)
        {
            try
            {
                string keyText = ReadKeyJsonInfo(jsonFilePath);
                if (string.IsNullOrWhiteSpace(keyText)
                    || keyText == "未绑定"
                    || keyText == "None")
                {
                    return Keys.None;
                }

                return KeyBindingService.TryParseBinding(keyText, out Keys keyValue)
                    ? KeyBindingService.NormalizeBinding(keyValue)
                    : Keys.None;
            }
            catch
            {
                return Keys.None;
            }
        }
    }
}
