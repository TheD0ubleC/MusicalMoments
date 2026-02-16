using System.Reflection;

namespace MusicalMoments
{
    internal static class VersionService
    {
        private const string FallbackVersionTag = "v0.0.0-release-x64";

        public static string CurrentVersionTag { get; } = ResolveVersionTag();

        public static string CurrentVersionNumber => ExtractVersionNumber(CurrentVersionTag);

        public static string ExtractVersionNumber(string versionTag)
        {
            if (string.IsNullOrWhiteSpace(versionTag))
            {
                return "0.0.0";
            }

            string[] parts = versionTag.Split('-', StringSplitOptions.RemoveEmptyEntries);
            string firstPart = parts.Length > 0 ? parts[0] : versionTag;
            string normalized = firstPart.Trim().TrimStart('v', 'V');
            return string.IsNullOrWhiteSpace(normalized) ? "0.0.0" : normalized;
        }

        private static string ResolveVersionTag()
        {
            Assembly assembly = typeof(VersionService).Assembly;
            AssemblyInformationalVersionAttribute informationalVersionAttribute =
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (informationalVersionAttribute != null
                && !string.IsNullOrWhiteSpace(informationalVersionAttribute.InformationalVersion))
            {
                string normalized = informationalVersionAttribute.InformationalVersion.Trim();
                int metadataSeparator = normalized.IndexOf('+');
                if (metadataSeparator >= 0)
                {
                    normalized = normalized.Substring(0, metadataSeparator);
                }

                return normalized;
            }

            string assemblyVersion = assembly.GetName().Version?.ToString() ?? "0.0.0";
            if (string.IsNullOrWhiteSpace(assemblyVersion) || assemblyVersion == "0.0.0.0")
            {
                return FallbackVersionTag;
            }

            return $"v{assemblyVersion}-release-x64";
        }
    }
}
