using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicalMoments
{
    internal sealed class ReleaseAssetInfo
    {
        public ReleaseAssetInfo(string name, string downloadUrl)
        {
            Name = name;
            DownloadUrl = downloadUrl;
        }

        public string Name { get; }
        public string DownloadUrl { get; }
    }

    internal sealed class ReleaseInfo
    {
        public string TagName { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public IReadOnlyList<ReleaseAssetInfo> Assets { get; set; } = Array.Empty<ReleaseAssetInfo>();
    }

    internal static class UpdateService
    {
        private const string RepoOwner = "TheD0ubleC";
        private const string RepoName = "MusicalMoments";
        private static readonly string[] LatestReleaseApiUrls =
        {
            $"https://api.github.scmd.cc/repos/{RepoOwner}/{RepoName}/releases/latest",
            $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest"
        };

        private static readonly HttpClient Client = CreateHttpClient();

        public static async Task<ReleaseInfo> GetLatestReleaseAsync()
        {
            foreach (string apiUrl in LatestReleaseApiUrls)
            {
                ReleaseInfo? release = await TryGetLatestReleaseFromEndpointAsync(apiUrl);
                if (release != null && !string.IsNullOrWhiteSpace(release.TagName))
                {
                    return release;
                }
            }

            return new ReleaseInfo
            {
                TagName = MainWindow.nowVer,
                HtmlUrl = $"https://github.scmd.cc/{RepoOwner}/{RepoName}/releases"
            };
        }

        public static async Task<string> GetLatestVersionAsync()
        {
            ReleaseInfo release = await GetLatestReleaseAsync();
            return string.IsNullOrWhiteSpace(release.TagName) ? MainWindow.nowVer : release.TagName;
        }


        public static async Task<string> GetLatestVersionTipsAsync()
        {
            ReleaseInfo release = await GetLatestReleaseAsync();
            return release.Body ?? string.Empty;
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MusicalMoments-Updater/2.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            return client;
        }

        private static async Task<ReleaseInfo?> TryGetLatestReleaseFromEndpointAsync(string apiUrl)
        {
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    using HttpResponseMessage response = await Client.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        if ((int)response.StatusCode >= 500 && attempt < 2)
                        {
                            await Task.Delay(250 * attempt);
                            continue;
                        }

                        return null;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    ReleaseInfo release = ParseLatestRelease(json);
                    if (!string.IsNullOrWhiteSpace(release.TagName))
                    {
                        return release;
                    }

                    return null;
                }
                catch
                {
                    if (attempt >= 2)
                    {
                        return null;
                    }

                    await Task.Delay(250 * attempt);
                }
            }

            return null;
        }

        private static ReleaseInfo ParseLatestRelease(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            ReleaseInfo release = new ReleaseInfo
            {
                TagName = GetStringProperty(root, "tag_name"),
                Body = GetStringProperty(root, "body"),
                HtmlUrl = GetStringProperty(root, "html_url"),
                Assets = ParseAssets(root)
            };

            return release;
        }

        private static IReadOnlyList<ReleaseAssetInfo> ParseAssets(JsonElement root)
        {
            if (!root.TryGetProperty("assets", out JsonElement assetsElement) || assetsElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<ReleaseAssetInfo>();
            }

            List<ReleaseAssetInfo> assets = new List<ReleaseAssetInfo>();
            foreach (JsonElement assetElement in assetsElement.EnumerateArray())
            {
                string downloadUrl = GetStringProperty(assetElement, "browser_download_url");
                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    continue;
                }

                string name = GetStringProperty(assetElement, "name");
                assets.Add(new ReleaseAssetInfo(name, downloadUrl));
            }

            return assets;
        }

        private static string GetStringProperty(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement propertyElement))
            {
                return string.Empty;
            }

            return propertyElement.GetString() ?? string.Empty;
        }
    }
}
