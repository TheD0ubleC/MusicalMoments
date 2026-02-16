using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace MusicalMoments
{
    internal static class WindowsAudioEndpointService
    {
        private const int SpeakerFrontLeftRightMask = 0x3;
        private const int SpeakerFrontCenterMask = 0x4;
        private const string DevicePropertyKeyPrefix = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\MMDevices\\Audio\\";
        private const string PropDeviceLabel = "{a45c254e-df1c-4efd-8020-67d146a850e0},2";
        private const string PropManufacturer = "{a45c254e-df1c-4efd-8020-67d146a850e0},13";
        private const string PropFriendlyName = "{a45c254e-df1c-4efd-8020-67d146a850e0},14";
        private const string PropEndpointDeviceName = "{b3f8fa53-0004-438e-9003-51a46e139bfc},6";
        private const string PropParentDeviceId = "{b3f8fa53-0004-438e-9003-51a46e139bfc},2";
        private const string PropDriverInfo = "{83da6326-97a6-4088-9453-a1923f573b29},3";
        private const string PropDriverDateRaw = "{83da6326-97a6-4088-9453-a1923f573b29},100";
        private const string PropDriverInf = "{a8b865dd-2e3d-4094-ad97-e593a70c75d6},5";
        private const string PropHardwareId = "{9dad2fed-3c19-4cde-b3c9-1bd56be25698},0";

        internal sealed class DefaultDeviceSnapshot
        {
            public string RenderDeviceId { get; init; } = string.Empty;
            public string CaptureDeviceId { get; init; } = string.Empty;
            public string RenderDeviceName { get; init; } = string.Empty;
            public string CaptureDeviceName { get; init; } = string.Empty;
            public EndpointMatchProfile RenderProfile { get; init; } = new EndpointMatchProfile();
            public EndpointMatchProfile CaptureProfile { get; init; } = new EndpointMatchProfile();
        }

        internal sealed class EndpointFormatInfo
        {
            public string DeviceId { get; init; } = string.Empty;
            public string FriendlyName { get; init; } = string.Empty;
            public DataFlow DataFlow { get; init; }
            public int SampleRate { get; init; }
            public int BitsPerSample { get; init; }
            public int Channels { get; init; }
            public string DisplayText => $"{SampleRate}Hz / {BitsPerSample}bit / {Channels}声道";
        }

        internal sealed class EndpointMatchProfile
        {
            public string EndpointId { get; init; } = string.Empty;
            public string FriendlyName { get; init; } = string.Empty;
            public string DeviceLabel { get; init; } = string.Empty;
            public string Manufacturer { get; init; } = string.Empty;
            public string ParentDeviceId { get; init; } = string.Empty;
            public string HardwareId { get; init; } = string.Empty;
            public string DriverVersion { get; init; } = string.Empty;
            public string DriverDate { get; init; } = string.Empty;
            public string DriverInf { get; init; } = string.Empty;
            public int SampleRate { get; init; }
            public int BitsPerSample { get; init; }
            public int Channels { get; init; }

            public bool HasIdentity =>
                !string.IsNullOrWhiteSpace(FriendlyName)
                || !string.IsNullOrWhiteSpace(DeviceLabel)
                || !string.IsNullOrWhiteSpace(Manufacturer)
                || !string.IsNullOrWhiteSpace(ParentDeviceId)
                || !string.IsNullOrWhiteSpace(HardwareId);
        }

        public static DefaultDeviceSnapshot CaptureCurrentDefaults()
        {
            try
            {
                using MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                using MMDevice renderDefault = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                using MMDevice captureDefault = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
                EndpointMatchProfile renderProfile = BuildEndpointMatchProfile(DataFlow.Render, renderDefault);
                EndpointMatchProfile captureProfile = BuildEndpointMatchProfile(DataFlow.Capture, captureDefault);

                return new DefaultDeviceSnapshot
                {
                    RenderDeviceId = renderProfile.EndpointId,
                    CaptureDeviceId = captureProfile.EndpointId,
                    RenderDeviceName = renderProfile.FriendlyName,
                    CaptureDeviceName = captureProfile.FriendlyName,
                    RenderProfile = renderProfile,
                    CaptureProfile = captureProfile
                };
            }
            catch
            {
                return new DefaultDeviceSnapshot();
            }
        }

        public static bool TryRestoreDefaults(DefaultDeviceSnapshot snapshot, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (snapshot == null)
            {
                return true;
            }

            List<string> errors = new List<string>();

            if (!TryRestoreOne(
                    DataFlow.Render,
                    snapshot.RenderDeviceId,
                    snapshot.RenderDeviceName,
                    snapshot.RenderProfile,
                    out string renderError))
            {
                errors.Add($"恢复默认扬声器失败: {renderError}");
            }

            if (!TryRestoreOne(
                    DataFlow.Capture,
                    snapshot.CaptureDeviceId,
                    snapshot.CaptureDeviceName,
                    snapshot.CaptureProfile,
                    out string captureError))
            {
                errors.Add($"恢复默认麦克风失败: {captureError}");
            }

            errorMessage = string.Join(Environment.NewLine, errors);
            return errors.Count == 0;
        }

        private static bool TryRestoreOne(
            DataFlow flow,
            string endpointId,
            string endpointName,
            EndpointMatchProfile snapshotProfile,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (TryRestoreById(flow, endpointId, out string idError))
            {
                return true;
            }

            if (TryRestoreByProfile(flow, snapshotProfile, out string profileError))
            {
                return true;
            }

            string fallbackSetError = string.Empty;
            if (TryFindEndpointIdByFriendlyName(flow, endpointName, out string fallbackId)
                && TrySetDefaultEndpoint(fallbackId, out fallbackSetError))
            {
                if (IsDefaultEndpoint(flow, fallbackId))
                {
                    return true;
                }

                errorMessage = "已执行恢复命令，但系统默认设备尚未切换完成。";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(fallbackSetError))
            {
                errorMessage = fallbackSetError;
                return false;
            }

            if (!string.IsNullOrWhiteSpace(idError))
            {
                errorMessage = idError;
                return false;
            }

            if (!string.IsNullOrWhiteSpace(profileError))
            {
                errorMessage = profileError;
                return false;
            }

            errorMessage = "未找到安装前记录的默认设备。";
            return false;
        }

        private static bool TryRestoreById(DataFlow flow, string endpointId, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(endpointId))
            {
                errorMessage = "安装前未记录到目标设备 ID。";
                return false;
            }

            if (!ContainsEndpointId(flow, endpointId, DeviceState.Active))
            {
                if (ContainsEndpointId(flow, endpointId, DeviceState.Active | DeviceState.Disabled | DeviceState.NotPresent | DeviceState.Unplugged))
                {
                    errorMessage = "安装前记录的设备当前不可用（可能被禁用或断开）。";
                }
                else
                {
                    errorMessage = "安装前记录的设备在当前系统中不存在。";
                }

                return false;
            }

            if (!TrySetDefaultEndpoint(endpointId, out string setError))
            {
                errorMessage = string.IsNullOrWhiteSpace(setError)
                    ? "恢复默认设备失败。"
                    : setError;
                return false;
            }

            if (!IsDefaultEndpoint(flow, endpointId))
            {
                errorMessage = "已执行恢复命令，但系统默认设备未切换到目标设备。";
                return false;
            }

            return true;
        }

        private static bool TryRestoreByProfile(DataFlow flow, EndpointMatchProfile snapshotProfile, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (snapshotProfile == null || !snapshotProfile.HasIdentity)
            {
                errorMessage = "安装前未记录到可用于匹配的设备特征。";
                return false;
            }

            if (!TryFindBestMatchEndpointId(flow, snapshotProfile, out string matchedEndpointId, out string matchDetail))
            {
                errorMessage = $"未找到足够相似的设备。{matchDetail}";
                return false;
            }

            if (!TrySetDefaultEndpoint(matchedEndpointId, out string setError))
            {
                errorMessage = string.IsNullOrWhiteSpace(setError)
                    ? "按设备特征匹配后设置默认设备失败。"
                    : setError;
                return false;
            }

            if (!IsDefaultEndpoint(flow, matchedEndpointId))
            {
                errorMessage = $"已匹配到设备但系统默认设备未切换成功。{matchDetail}";
                return false;
            }

            return true;
        }

        public static bool TrySetDefaultEndpoint(string endpointId, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(endpointId))
            {
                errorMessage = "端点 ID 为空。";
                return false;
            }

            try
            {
                IPolicyConfig policyConfig = (IPolicyConfig)new PolicyConfigClient();
                Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(endpointId, ERole.Console));
                Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(endpointId, ERole.Multimedia));
                Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(endpointId, ERole.Communications));
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public static bool TryFindEndpointIdByFriendlyName(DataFlow flow, string endpointName, out string endpointId)
        {
            endpointId = string.Empty;
            if (string.IsNullOrWhiteSpace(endpointName))
            {
                return false;
            }

            try
            {
                using MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                var endpoints = enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active);
                foreach (MMDevice endpoint in endpoints)
                {
                    if (!TryGetFriendlyName(endpoint, out string name))
                    {
                        continue;
                    }

                    if (name.Equals(endpointName, StringComparison.OrdinalIgnoreCase)
                        || name.Contains(endpointName, StringComparison.OrdinalIgnoreCase)
                        || endpointName.Contains(name, StringComparison.OrdinalIgnoreCase))
                    {
                        endpointId = endpoint.ID;
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore endpoint enumeration failures and return false.
            }

            return false;
        }

        public static bool IsDefaultEndpoint(DataFlow flow, string endpointId)
        {
            if (string.IsNullOrWhiteSpace(endpointId))
            {
                return false;
            }

            try
            {
                using MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                using MMDevice currentDefault = enumerator.GetDefaultAudioEndpoint(flow, Role.Multimedia);
                return string.Equals(currentDefault?.ID, endpointId, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryFindBestMatchEndpointId(
            DataFlow flow,
            EndpointMatchProfile targetProfile,
            out string endpointId,
            out string detailMessage)
        {
            endpointId = string.Empty;
            detailMessage = string.Empty;

            EndpointMatchProfile bestCandidate = null;
            int bestScore = int.MinValue;
            int secondScore = int.MinValue;

            try
            {
                using MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                var endpoints = enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active);
                foreach (MMDevice endpoint in endpoints)
                {
                    EndpointMatchProfile candidate = BuildEndpointMatchProfile(flow, endpoint);
                    int score = ComputeMatchScore(targetProfile, candidate);
                    if (score > bestScore)
                    {
                        secondScore = bestScore;
                        bestScore = score;
                        bestCandidate = candidate;
                    }
                    else if (score > secondScore)
                    {
                        secondScore = score;
                    }
                }
            }
            catch (Exception ex)
            {
                detailMessage = $"枚举设备失败: {ex.Message}";
                return false;
            }

            if (bestCandidate == null)
            {
                detailMessage = "未找到可用音频设备。";
                return false;
            }

            const int minimumScore = 36;
            if (bestScore < minimumScore)
            {
                detailMessage = $"最佳匹配分过低({bestScore})，候选设备[{bestCandidate.FriendlyName}]。";
                return false;
            }

            endpointId = bestCandidate.EndpointId;
            detailMessage = $"匹配设备[{bestCandidate.FriendlyName}]，分数 {bestScore}（次优 {secondScore}）。";
            return true;
        }

        private static int ComputeMatchScore(EndpointMatchProfile target, EndpointMatchProfile candidate)
        {
            if (target == null || candidate == null)
            {
                return int.MinValue;
            }

            int score = 0;

            score += ScoreText(target.FriendlyName, candidate.FriendlyName, 42, 22, 8);
            score += ScoreText(target.DeviceLabel, candidate.DeviceLabel, 28, 14, 4);
            score += ScoreText(target.Manufacturer, candidate.Manufacturer, 28, 16, 6);
            score += ScoreText(target.ParentDeviceId, candidate.ParentDeviceId, 44, 22, 8);
            score += ScoreText(target.HardwareId, candidate.HardwareId, 40, 18, 8);
            score += ScoreText(target.DriverVersion, candidate.DriverVersion, 26, 14, 0);
            score += ScoreText(target.DriverInf, candidate.DriverInf, 20, 10, 0);
            score += ScoreText(target.DriverDate, candidate.DriverDate, 14, 8, 0);

            if (target.SampleRate > 0 && candidate.SampleRate > 0)
            {
                score += target.SampleRate == candidate.SampleRate ? 8 : -3;
            }

            if (target.BitsPerSample > 0 && candidate.BitsPerSample > 0)
            {
                score += target.BitsPerSample == candidate.BitsPerSample ? 8 : -3;
            }

            if (target.Channels > 0 && candidate.Channels > 0)
            {
                score += target.Channels == candidate.Channels ? 4 : -1;
            }

            bool targetIsVirtualCable = IsLikelyVirtualCable(target);
            bool candidateIsVirtualCable = IsLikelyVirtualCable(candidate);
            if (targetIsVirtualCable != candidateIsVirtualCable)
            {
                score -= 50;
            }

            return score;
        }

        private static int ScoreText(string target, string candidate, int exactScore, int containScore, int prefixScore)
        {
            if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(candidate))
            {
                return 0;
            }

            string a = NormalizeText(target);
            string b = NormalizeText(candidate);
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
            {
                return 0;
            }

            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
            {
                return exactScore;
            }

            if (a.Contains(b, StringComparison.OrdinalIgnoreCase) || b.Contains(a, StringComparison.OrdinalIgnoreCase))
            {
                return containScore;
            }

            if (a.StartsWith(b, StringComparison.OrdinalIgnoreCase) || b.StartsWith(a, StringComparison.OrdinalIgnoreCase))
            {
                return prefixScore;
            }

            return 0;
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string normalized = text.Trim().ToLowerInvariant();
            normalized = normalized.Replace("(", string.Empty)
                                   .Replace(")", string.Empty)
                                   .Replace("{", string.Empty)
                                   .Replace("}", string.Empty)
                                   .Replace("[", string.Empty)
                                   .Replace("]", string.Empty)
                                   .Replace(" ", string.Empty);
            return normalized;
        }

        private static bool IsLikelyVirtualCable(EndpointMatchProfile profile)
        {
            string merged =
                $"{profile.FriendlyName}|{profile.DeviceLabel}|{profile.Manufacturer}|{profile.ParentDeviceId}|{profile.HardwareId}";
            return merged.Contains("VB-Audio", StringComparison.OrdinalIgnoreCase)
                   || merged.Contains("CABLE", StringComparison.OrdinalIgnoreCase)
                   || merged.Contains("VBCABLE", StringComparison.OrdinalIgnoreCase);
        }

        private static EndpointMatchProfile BuildEndpointMatchProfile(DataFlow flow, MMDevice endpoint)
        {
            if (endpoint == null)
            {
                return new EndpointMatchProfile();
            }

            EndpointRegistryMetadata metadata = ReadEndpointRegistryMetadata(flow, endpoint.ID);
            int sampleRate = 0;
            int bits = 0;
            int channels = 0;
            if (!TryGetDeviceFormatFromPolicy(endpoint.ID, out sampleRate, out bits, out channels))
            {
                try
                {
                    WaveFormat mixFormat = endpoint.AudioClient?.MixFormat;
                    sampleRate = mixFormat?.SampleRate ?? 0;
                    bits = mixFormat?.BitsPerSample ?? 0;
                    channels = mixFormat?.Channels ?? 0;
                }
                catch
                {
                    // Ignore mix format read failures.
                }
            }

            string friendlyName = GetSafeFriendlyName(endpoint);
            if (string.IsNullOrWhiteSpace(friendlyName))
            {
                friendlyName = metadata.RegistryFriendlyName;
            }

            string manufacturer = metadata.Manufacturer;
            if (string.IsNullOrWhiteSpace(manufacturer))
            {
                manufacturer = metadata.EndpointDeviceName;
            }

            return new EndpointMatchProfile
            {
                EndpointId = endpoint.ID ?? string.Empty,
                FriendlyName = friendlyName,
                DeviceLabel = metadata.DeviceLabel,
                Manufacturer = manufacturer,
                ParentDeviceId = metadata.ParentDeviceId,
                HardwareId = metadata.HardwareId,
                DriverVersion = metadata.DriverVersion,
                DriverDate = metadata.DriverDate,
                DriverInf = metadata.DriverInf,
                SampleRate = sampleRate,
                BitsPerSample = bits,
                Channels = channels
            };
        }

        private sealed class EndpointRegistryMetadata
        {
            public string DeviceLabel { get; init; } = string.Empty;
            public string Manufacturer { get; init; } = string.Empty;
            public string RegistryFriendlyName { get; init; } = string.Empty;
            public string EndpointDeviceName { get; init; } = string.Empty;
            public string ParentDeviceId { get; init; } = string.Empty;
            public string HardwareId { get; init; } = string.Empty;
            public string DriverVersion { get; init; } = string.Empty;
            public string DriverDate { get; init; } = string.Empty;
            public string DriverInf { get; init; } = string.Empty;
        }

        private static EndpointRegistryMetadata ReadEndpointRegistryMetadata(DataFlow flow, string endpointId)
        {
            if (!TryExtractEndpointGuid(endpointId, out string endpointGuid))
            {
                return new EndpointRegistryMetadata();
            }

            string flowPart = flow == DataFlow.Render ? "Render" : "Capture";
            string propertiesPath = $"{DevicePropertyKeyPrefix}{flowPart}\\{endpointGuid}\\Properties";

            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                using RegistryKey propertiesKey = baseKey.OpenSubKey(propertiesPath, false);
                if (propertiesKey == null)
                {
                    return new EndpointRegistryMetadata();
                }

                object rawDriverDate = propertiesKey.GetValue(PropDriverDateRaw, null);
                string driverInfo = ReadRegistryValueAsString(propertiesKey.GetValue(PropDriverInfo, null));
                string driverVersion = ParseDriverVersion(driverInfo);
                string driverDate = ParseDriverDate(rawDriverDate);

                return new EndpointRegistryMetadata
                {
                    DeviceLabel = ReadRegistryValueAsString(propertiesKey.GetValue(PropDeviceLabel, null)),
                    Manufacturer = ReadRegistryValueAsString(propertiesKey.GetValue(PropManufacturer, null)),
                    RegistryFriendlyName = ReadRegistryValueAsString(propertiesKey.GetValue(PropFriendlyName, null)),
                    EndpointDeviceName = ReadRegistryValueAsString(propertiesKey.GetValue(PropEndpointDeviceName, null)),
                    ParentDeviceId = ReadRegistryValueAsString(propertiesKey.GetValue(PropParentDeviceId, null)),
                    HardwareId = ReadRegistryValueAsString(propertiesKey.GetValue(PropHardwareId, null)),
                    DriverVersion = driverVersion,
                    DriverDate = driverDate,
                    DriverInf = ReadRegistryValueAsString(propertiesKey.GetValue(PropDriverInf, null))
                };
            }
            catch
            {
                return new EndpointRegistryMetadata();
            }
        }

        private static bool TryExtractEndpointGuid(string endpointId, out string endpointGuidWithBraces)
        {
            endpointGuidWithBraces = string.Empty;
            if (string.IsNullOrWhiteSpace(endpointId))
            {
                return false;
            }

            Match match = Regex.Match(endpointId, @"\{[0-9a-fA-F\-]{36}\}$");
            if (!match.Success)
            {
                return false;
            }

            endpointGuidWithBraces = match.Value;
            return true;
        }

        private static string ReadRegistryValueAsString(object value)
        {
            switch (value)
            {
                case null:
                    return string.Empty;
                case string text:
                    return text.Trim();
                case string[] multi:
                    return string.Join(";", multi.Where(item => !string.IsNullOrWhiteSpace(item))).Trim();
                case byte[] bytes:
                    return DecodeRegistryBinary(bytes);
                default:
                    return Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
            }
        }

        private static string DecodeRegistryBinary(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                if (bytes.Length % 2 == 0)
                {
                    string unicodeText = Encoding.Unicode.GetString(bytes).TrimEnd('\0').Trim();
                    if (!string.IsNullOrWhiteSpace(unicodeText) && unicodeText.Any(ch => !char.IsControl(ch)))
                    {
                        return unicodeText;
                    }
                }
            }
            catch
            {
                // Ignore unicode decode failures.
            }

            return BitConverter.ToString(bytes);
        }

        private static string ParseDriverVersion(string driverInfo)
        {
            if (string.IsNullOrWhiteSpace(driverInfo))
            {
                return string.Empty;
            }

            Match versionMatch = Regex.Match(driverInfo, @"\d+\.\d+\.\d+\.\d+");
            return versionMatch.Success ? versionMatch.Value : string.Empty;
        }

        private static string ParseDriverDate(object rawValue)
        {
            if (rawValue == null)
            {
                return string.Empty;
            }

            try
            {
                long fileTime = rawValue switch
                {
                    byte[] bytes when bytes.Length >= 8 => BitConverter.ToInt64(bytes, 0),
                    long longValue => longValue,
                    int intValue => intValue,
                    _ => 0L
                };

                if (fileTime <= 0)
                {
                    return string.Empty;
                }

                DateTime date = DateTime.FromFileTimeUtc(fileTime).ToLocalTime();
                if (date.Year < 1990 || date.Year > 2100)
                {
                    return string.Empty;
                }

                return date.ToString("yyyy/M/d", CultureInfo.InvariantCulture);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static EndpointFormatInfo GetEndpointFormat(MMDevice endpoint)
        {
            int sampleRate;
            int bits;
            int channels;
            if (!TryGetDeviceFormatFromPolicy(endpoint.ID, out sampleRate, out bits, out channels))
            {
                WaveFormat format = endpoint.AudioClient.MixFormat;
                sampleRate = format.SampleRate;
                bits = format.BitsPerSample;
                channels = format.Channels;
            }

            return new EndpointFormatInfo
            {
                DeviceId = endpoint.ID,
                FriendlyName = GetSafeFriendlyName(endpoint),
                DataFlow = endpoint.DataFlow,
                SampleRate = sampleRate,
                BitsPerSample = bits,
                Channels = channels
            };
        }

        public static MMDevice FindDefaultDevice(DataFlow flow)
        {
            try
            {
                using MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                return enumerator.GetDefaultAudioEndpoint(flow, Role.Multimedia);
            }
            catch
            {
                return null;
            }
        }

        public static MMDevice FindVbRenderEndpoint()
        {
            return FindByName(DataFlow.Render, "CABLE Input", "VB-Audio Virtual Cable");
        }

        public static MMDevice FindVbCaptureEndpoint()
        {
            return FindByName(DataFlow.Capture, "CABLE Output", "VB-Audio Virtual Cable");
        }

        public static MMDevice FindByName(DataFlow flow, params string[] keywords)
        {
            try
            {
                using MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                var endpoints = enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active);
                foreach (MMDevice endpoint in endpoints)
                {
                    string name = GetSafeFriendlyName(endpoint);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        EndpointRegistryMetadata metadata = ReadEndpointRegistryMetadata(flow, endpoint.ID);
                        name = string.Join(" ",
                            new[] { metadata.RegistryFriendlyName, metadata.EndpointDeviceName, metadata.DeviceLabel }
                                .Where(item => !string.IsNullOrWhiteSpace(item)));
                    }

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    if (keywords.Any(keyword => !string.IsNullOrWhiteSpace(keyword)
                                                && name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    {
                        return endpoint;
                    }
                }
            }
            catch
            {
                // Ignore endpoint enumeration failures and return null.
            }

            return null;
        }

        public static string GetFriendlyNameSafe(MMDevice endpoint)
        {
            return TryGetFriendlyName(endpoint, out string name) ? name : string.Empty;
        }

        private static bool TryGetFriendlyName(MMDevice endpoint, out string friendlyName)
        {
            friendlyName = string.Empty;
            if (endpoint == null)
            {
                return false;
            }

            try
            {
                friendlyName = endpoint.FriendlyName ?? string.Empty;
                return !string.IsNullOrWhiteSpace(friendlyName);
            }
            catch
            {
                // Some broken/virtual endpoints throw COM exceptions when reading FriendlyName.
                return false;
            }
        }

        private static string GetSafeFriendlyName(MMDevice endpoint)
        {
            return TryGetFriendlyName(endpoint, out string name) ? name : string.Empty;
        }

        private static bool TryGetDeviceFormatFromPolicy(string endpointId, out int sampleRate, out int bitsPerSample, out int channels)
        {
            sampleRate = 0;
            bitsPerSample = 0;
            channels = 0;

            if (string.IsNullOrWhiteSpace(endpointId))
            {
                return false;
            }

            IntPtr formatPointer = IntPtr.Zero;
            try
            {
                IPolicyConfig policyConfig = (IPolicyConfig)new PolicyConfigClient();
                int hr = policyConfig.GetDeviceFormat(endpointId, false, out formatPointer);
                Marshal.ThrowExceptionForHR(hr);
                if (formatPointer == IntPtr.Zero)
                {
                    return false;
                }

                WaveFormatExNative format = Marshal.PtrToStructure<WaveFormatExNative>(formatPointer);
                sampleRate = (int)format.SamplesPerSec;
                bitsPerSample = format.BitsPerSample;
                channels = format.Channels;

                if (format.FormatTag == 0xFFFE && format.CbSize >= 22)
                {
                    WaveFormatExtensibleNative extensible = Marshal.PtrToStructure<WaveFormatExtensibleNative>(formatPointer);
                    if (extensible.Samples > 0 && extensible.Samples <= bitsPerSample)
                    {
                        bitsPerSample = extensible.Samples;
                    }
                }

                return sampleRate > 0 && bitsPerSample > 0 && channels > 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (formatPointer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(formatPointer);
                }
            }
        }

        private static bool ContainsEndpointId(DataFlow flow, string endpointId, DeviceState stateMask)
        {
            if (string.IsNullOrWhiteSpace(endpointId))
            {
                return false;
            }

            try
            {
                using MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                var endpoints = enumerator.EnumerateAudioEndPoints(flow, stateMask);
                foreach (MMDevice endpoint in endpoints)
                {
                    if (string.Equals(endpoint.ID, endpointId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore endpoint enumeration failures and treat as not found.
            }

            return false;
        }

        public static bool TrySetDeviceFormatTo16Bit48kStereo(string endpointId, out string errorMessage)
        {
            return TrySetDeviceFormat(endpointId, 48000, 16, 2, out errorMessage);
        }

        public static bool TrySetDeviceFormatTo32Bit48kStereo(string endpointId, out string errorMessage)
        {
            if (TrySetDeviceFormat(endpointId, 48000, 32, 2, AudioSubtypes.IeeeFloat, out errorMessage))
            {
                return true;
            }

            string floatError = errorMessage;
            if (TrySetDeviceFormat(endpointId, 48000, 32, 2, AudioSubtypes.Pcm, out errorMessage))
            {
                return true;
            }

            string pcmError = errorMessage;
            errorMessage = $"设备不支持 48k/32bit/2声道（已尝试 Float 与 PCM）。Float错误：{floatError}；PCM错误：{pcmError}";
            return false;
        }

        public static bool TrySetDeviceFormatTo16Bit48kMono(string endpointId, out string errorMessage)
        {
            return TrySetDeviceFormat(endpointId, 48000, 16, 1, out errorMessage);
        }

        public static bool TrySetDeviceFormat(string endpointId, int sampleRate, int bitsPerSample, int channels, out string errorMessage)
        {
            return TrySetDeviceFormat(endpointId, sampleRate, bitsPerSample, channels, AudioSubtypes.Pcm, out errorMessage);
        }

        private static bool TrySetDeviceFormat(
            string endpointId,
            int sampleRate,
            int bitsPerSample,
            int channels,
            Guid subFormat,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(endpointId))
            {
                errorMessage = "端点 ID 为空。";
                return false;
            }

            IntPtr endpointFormatPtr = IntPtr.Zero;
            IntPtr mixFormatPtr = IntPtr.Zero;
            try
            {
                IPolicyConfig policyConfig = (IPolicyConfig)new PolicyConfigClient();
                WaveFormatExtensibleNative format = BuildWaveFormat(sampleRate, bitsPerSample, channels, subFormat);
                endpointFormatPtr = Marshal.AllocHGlobal(Marshal.SizeOf<WaveFormatExtensibleNative>());
                mixFormatPtr = Marshal.AllocHGlobal(Marshal.SizeOf<WaveFormatExtensibleNative>());
                Marshal.StructureToPtr(format, endpointFormatPtr, false);
                Marshal.StructureToPtr(format, mixFormatPtr, false);

                int hr = policyConfig.SetDeviceFormat(endpointId, endpointFormatPtr, mixFormatPtr);
                Marshal.ThrowExceptionForHR(hr);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = BuildDeviceFormatErrorMessage(ex, sampleRate, bitsPerSample, channels, subFormat);
                return false;
            }
            finally
            {
                if (endpointFormatPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(endpointFormatPtr);
                }

                if (mixFormatPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(mixFormatPtr);
                }
            }
        }

        private static string BuildDeviceFormatErrorMessage(Exception ex, int sampleRate, int bitsPerSample, int channels, Guid subFormat)
        {
            uint hResult = unchecked((uint)ex.HResult);
            string typeName = subFormat == AudioSubtypes.IeeeFloat ? "Float" : "PCM";
            if (hResult == 0x88890008)
            {
                return $"设备不支持目标格式：{sampleRate}Hz/{bitsPerSample}bit/{channels}声道({typeName})。";
            }

            if (hResult == 0x80070005)
            {
                return "权限不足，无法写入设备格式。请以管理员身份运行。";
            }

            return ex.Message;
        }

        private static WaveFormatExtensibleNative BuildWaveFormat(int sampleRate, int bitsPerSample, int channels, Guid subFormat)
        {
            ushort channelCount = (ushort)Math.Max(1, channels);
            ushort blockAlign = (ushort)(channelCount * (bitsPerSample / 8));
            uint avgBytesPerSec = (uint)(sampleRate * blockAlign);
            int channelMask = channelCount switch
            {
                1 => SpeakerFrontCenterMask,
                2 => SpeakerFrontLeftRightMask,
                _ => 0
            };

            return new WaveFormatExtensibleNative
            {
                Format = new WaveFormatExNative
                {
                    FormatTag = 0xFFFE,
                    Channels = channelCount,
                    SamplesPerSec = (uint)sampleRate,
                    AvgBytesPerSec = avgBytesPerSec,
                    BlockAlign = blockAlign,
                    BitsPerSample = (ushort)bitsPerSample,
                    CbSize = 22
                },
                Samples = (ushort)bitsPerSample,
                ChannelMask = channelMask,
                SubFormat = subFormat
            };
        }

        private static class AudioSubtypes
        {
            public static readonly Guid Pcm = new Guid("00000001-0000-0010-8000-00AA00389B71");
            public static readonly Guid IeeeFloat = new Guid("00000003-0000-0010-8000-00AA00389B71");
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct WaveFormatExNative
        {
            public ushort FormatTag;
            public ushort Channels;
            public uint SamplesPerSec;
            public uint AvgBytesPerSec;
            public ushort BlockAlign;
            public ushort BitsPerSample;
            public ushort CbSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct WaveFormatExtensibleNative
        {
            public WaveFormatExNative Format;
            public ushort Samples;
            public int ChannelMask;
            public Guid SubFormat;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PropertyKey
        {
            public Guid fmtid;
            public int pid;
        }

        private enum ERole
        {
            Console = 0,
            Multimedia = 1,
            Communications = 2
        }

        [ComImport]
        [Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
        private class PolicyConfigClient
        {
        }

        [ComImport]
        [Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPolicyConfig
        {
            [PreserveSig]
            int GetMixFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceId, out IntPtr formatPointer);

            [PreserveSig]
            int GetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.Bool)] bool defaultFormat, out IntPtr formatPointer);

            [PreserveSig]
            int ResetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

            [PreserveSig]
            int SetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceId, IntPtr endpointFormatPointer, IntPtr mixFormatPointer);

            [PreserveSig]
            int GetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.Bool)] bool defaultPeriod, out long defaultPeriodPointer, out long minimumPeriodPointer);

            [PreserveSig]
            int SetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string deviceId, ref long processingPeriodPointer);

            [PreserveSig]
            int GetShareMode([MarshalAs(UnmanagedType.LPWStr)] string deviceId, IntPtr modePointer);

            [PreserveSig]
            int SetShareMode([MarshalAs(UnmanagedType.LPWStr)] string deviceId, IntPtr modePointer);

            [PreserveSig]
            int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string deviceId, ref PropertyKey propertyKey, IntPtr propertyValuePointer);

            [PreserveSig]
            int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string deviceId, ref PropertyKey propertyKey, IntPtr propertyValuePointer);

            [PreserveSig]
            int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string deviceId, ERole role);

            [PreserveSig]
            int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.Bool)] bool isVisible);
        }
    }
}
