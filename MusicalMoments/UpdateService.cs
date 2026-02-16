using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicalMoments
{
    internal static class UpdateService
    {
        private const string RepoOwner = "TheD0ubleC";
        private const string RepoName = "MusicalMoments";
        static readonly HttpClient client = new HttpClient();

        public static async Task<string> GetLatestVersionAsync()
        {
            string releasesUrl =
                $"https://api.github.scmd.cc/repos/{RepoOwner}/{RepoName}/releases/latest";

            try
            {
                if (!client.DefaultRequestHeaders.Contains("User-Agent"))
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "MusicalMoments-Updater/1.0"
                    );

                    client.DefaultRequestHeaders.Accept.ParseAdd(
                        "application/json"
                    );
                }

                var response = await client.GetAsync(releasesUrl);
                if (!response.IsSuccessStatusCode)
                    return MainWindow.nowVer;

                string json = await response.Content.ReadAsStringAsync();
                return ParseLatestVersion(json);
            }
            catch
            {
                return MainWindow.nowVer;
            }
        }


        public static async Task<string> GetLatestVersionTipsAsync()
        {
            HttpResponseMessage response = null;
            string latestVersion = string.Empty;
            string releasesUrl = $"https://api.github.scmd.cc/repos/{RepoOwner}/{RepoName}/releases/latest";

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "C# App");
            try
            {
                response = await client.GetAsync(releasesUrl);
            }
            catch
            {
                return MainWindow.nowVer;
            }

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                latestVersion = ParseLatestVersionTips(json);
            }

            return latestVersion;
        }

        private static string ParseLatestVersion(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("tag_name", out JsonElement tagElement))
            {
                return tagElement.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static string ParseLatestVersionTips(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("body", out JsonElement tagElement))
            {
                return tagElement.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
