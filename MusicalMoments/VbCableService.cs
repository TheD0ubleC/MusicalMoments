using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using NAudio.CoreAudioApi;

namespace MusicalMoments
{
    internal static class VbCableService
    {
        private const string VbDownloadUrl = "https://download.vb-audio.com/Download_CABLE/VBCABLE_Driver_Pack43.zip";
        private const string PackageZipFileName = "VB.zip";
        private const string PackageFolderName = "VB";
        private const string InstallerNameX64 = "VBCABLE_Setup_x64.exe";
        private const string InstallerNameX86 = "VBCABLE_Setup.exe";
        private const string DefaultDeviceSnapshotFileName = "vb_default_devices_snapshot.json";
        private static readonly JsonSerializerOptions SnapshotJsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        internal sealed class VbHealthReport
        {
            public bool IsInstalled { get; init; }
            public WindowsAudioEndpointService.EndpointFormatInfo RenderEndpointFormat { get; init; }
            public WindowsAudioEndpointService.EndpointFormatInfo CaptureEndpointFormat { get; init; }
            public string DetailMessage { get; init; } = string.Empty;

            public bool IsRenderFormatHealthy =>
                RenderEndpointFormat != null
                && RenderEndpointFormat.SampleRate == 48000
                && RenderEndpointFormat.BitsPerSample == 16
                && RenderEndpointFormat.Channels == 2;

            public bool IsCaptureFormatHealthy =>
                CaptureEndpointFormat != null
                && CaptureEndpointFormat.SampleRate == 48000
                && CaptureEndpointFormat.BitsPerSample == 16
                && CaptureEndpointFormat.Channels == 2;

            public bool IsFormatHealthy => IsRenderFormatHealthy && IsCaptureFormatHealthy;

            public bool IsHealthy => IsInstalled && IsFormatHealthy;
        }

        internal sealed class VbOperationResult
        {
            public bool Succeeded { get; init; }
            public string Message { get; init; } = string.Empty;
        }

        private sealed class VbPackagePaths
        {
            public string PackageDirectory { get; init; } = string.Empty;
            public string InstallerPath { get; init; } = string.Empty;
        }

        public static bool IsInstalled()
        {
            return AreVbEndpointsPresent();
        }

        public static VbHealthReport BuildHealthReport()
        {
            using MMDevice renderEndpoint = WindowsAudioEndpointService.FindVbRenderEndpoint();
            using MMDevice captureEndpoint = WindowsAudioEndpointService.FindVbCaptureEndpoint();

            WindowsAudioEndpointService.EndpointFormatInfo renderFormat = renderEndpoint == null
                ? null
                : WindowsAudioEndpointService.GetEndpointFormat(renderEndpoint);
            WindowsAudioEndpointService.EndpointFormatInfo captureFormat = captureEndpoint == null
                ? null
                : WindowsAudioEndpointService.GetEndpointFormat(captureEndpoint);

            bool installed = renderEndpoint != null || captureEndpoint != null;
            bool registryResidual = !installed && SystemAudioService.CheckVirtualCableInstalled();
            List<string> details = new List<string>
            {
                installed ? "VB-CABLE 检测结果：已安装。" : "VB-CABLE 检测结果：未安装。",
                renderFormat == null
                    ? "VB 扬声器（CABLE Input）：未找到。"
                    : $"VB 扬声器（CABLE Input）：{renderFormat.DisplayText}",
                captureFormat == null
                    ? "VB 麦克风（CABLE Output）：未找到。"
                    : $"VB 麦克风（CABLE Output）：{captureFormat.DisplayText}"
            };

            if (renderFormat != null || captureFormat != null)
            {
                bool renderOk = renderFormat != null
                                && renderFormat.SampleRate == 48000
                                && renderFormat.BitsPerSample == 16
                                && renderFormat.Channels == 2;
                bool captureOk = captureFormat != null
                                 && captureFormat.SampleRate == 48000
                                 && captureFormat.BitsPerSample == 16
                                 && captureFormat.Channels == 2;

                details.Add(renderOk
                    ? "VB 扬声器格式检查：通过（要求 16bit / 48k / 2声道）。"
                    : "VB 扬声器格式检查：未通过（要求 16bit / 48k / 2声道）。");
                details.Add(captureOk
                    ? "VB 麦克风格式检查：通过（要求 16bit / 48k / 2声道）。"
                    : "VB 麦克风格式检查：未通过（要求 16bit / 48k / 2声道）。");
            }

            if (registryResidual)
            {
                details.Add("检测到 VB 卸载后注册表残留，但未检测到设备端点。通常可忽略，重启后会刷新。");
            }

            return new VbHealthReport
            {
                IsInstalled = installed,
                RenderEndpointFormat = renderFormat,
                CaptureEndpointFormat = captureFormat,
                DetailMessage = string.Join(Environment.NewLine, details)
            };
        }

        public static async Task<VbOperationResult> InstallAsync(
            string appDirectory,
            bool restoreSystemDefaults,
            IProgress<string> progress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                progress?.Report("正在准备安装包...");

                WindowsAudioEndpointService.DefaultDeviceSnapshot defaultSnapshot = WindowsAudioEndpointService.CaptureCurrentDefaults();
                string snapshotPath = GetSnapshotFilePath(appDirectory);
                SaveDefaultSnapshot(snapshotPath, defaultSnapshot, progress);
                progress?.Report(
                    $"已记录系统默认设备：扬声器[{DescribeSnapshotEndpoint(defaultSnapshot.RenderProfile, defaultSnapshot.RenderDeviceName, defaultSnapshot.RenderDeviceId)}]，" +
                    $"麦克风[{DescribeSnapshotEndpoint(defaultSnapshot.CaptureProfile, defaultSnapshot.CaptureDeviceName, defaultSnapshot.CaptureDeviceId)}]");

                VbPackagePaths package = await EnsurePackageReadyAsync(appDirectory, progress, cancellationToken);
                progress?.Report("正在执行静默安装...");
                int exitCode = await RunElevatedProcessAsync(package.InstallerPath, "-i -h", package.PackageDirectory, cancellationToken);
                progress?.Report($"安装程序已退出（ExitCode={exitCode}），正在检测安装结果...");

                await Task.Delay(1000, cancellationToken);
                bool endpointDetected = AreVbEndpointsPresent();
                bool registryDetected = SystemAudioService.CheckVirtualCableInstalled();
                if (!endpointDetected && !registryDetected)
                {
                    return new VbOperationResult
                    {
                        Succeeded = false,
                        Message = "未检测到 VB-CABLE，安装可能未成功。请确认系统驱动安装策略。"
                    };
                }

                bool endpointReady = await WaitForAudioEndpointRefreshAsync(requireVbEndpoints: true, progress, cancellationToken);
                if (!endpointReady && !AreVbEndpointsPresent())
                {
                    return new VbOperationResult
                    {
                        Succeeded = true,
                        Message = "VB-CABLE 安装命令已执行，但系统设备列表尚未刷新出端点。请等待几秒或重启后再检查。"
                    };
                }

                if (restoreSystemDefaults && defaultSnapshot != null)
                {
                    progress?.Report("正在恢复系统默认设备...");
                    var restoreResult = await TryRestoreDefaultsWithRetryAsync(defaultSnapshot, progress, cancellationToken);
                    if (!restoreResult.Restored)
                    {
                        return new VbOperationResult
                        {
                            Succeeded = true,
                            Message =
                                "VB-CABLE 安装成功，但恢复系统默认设备失败。" + Environment.NewLine +
                                $"失败原因：{restoreResult.ErrorMessage}" + Environment.NewLine +
                                $"安装前记录：扬声器[{DescribeSnapshotEndpoint(defaultSnapshot.RenderProfile, defaultSnapshot.RenderDeviceName, defaultSnapshot.RenderDeviceId)}]，" +
                                $"麦克风[{DescribeSnapshotEndpoint(defaultSnapshot.CaptureProfile, defaultSnapshot.CaptureDeviceName, defaultSnapshot.CaptureDeviceId)}]" + Environment.NewLine +
                                "请在系统声音设置中手动恢复。"
                        };
                    }

                    progress?.Report("系统默认设备已恢复为安装前状态。");
                }

                return new VbOperationResult
                {
                    Succeeded = true,
                    Message = "VB-CABLE 已静默安装完成。"
                };
            }
            catch (OperationCanceledException)
            {
                return new VbOperationResult
                {
                    Succeeded = false,
                    Message = "操作已取消。"
                };
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                return new VbOperationResult
                {
                    Succeeded = false,
                    Message = "你取消了管理员授权，安装未执行。"
                };
            }
            catch (Exception ex)
            {
                return new VbOperationResult
                {
                    Succeeded = false,
                    Message = $"安装失败：{ex.Message}"
                };
            }
        }

        public static async Task<VbOperationResult> UninstallAsync(
            string appDirectory,
            IProgress<string> progress,
            CancellationToken cancellationToken = default)
        {
            if (!AreVbEndpointsPresent())
            {
                return new VbOperationResult
                {
                    Succeeded = true,
                    Message = "未检测到 VB-CABLE，跳过卸载。"
                };
            }

            try
            {
                string snapshotPath = GetSnapshotFilePath(appDirectory);
                WindowsAudioEndpointService.DefaultDeviceSnapshot savedSnapshot = TryLoadDefaultSnapshot(snapshotPath, progress);
                VbPackagePaths package = await EnsurePackageReadyAsync(appDirectory, progress, cancellationToken);
                progress?.Report("正在执行静默卸载...");
                await RunElevatedProcessAsync(package.InstallerPath, "-u -h", package.PackageDirectory, cancellationToken);

                await Task.Delay(1000, cancellationToken);
                if (!AreVbEndpointsPresent())
                {
                    await RestoreSnapshotAfterUninstallAsync(savedSnapshot, progress, cancellationToken);
                    return new VbOperationResult
                    {
                        Succeeded = true,
                        Message = BuildUninstallSuccessMessage()
                    };
                }

                if (TryGetUninstallCommand(out string uninstallCommand))
                {
                    progress?.Report("安装器卸载未完成，尝试系统卸载命令...");
                    ParseCommand(uninstallCommand, out string executable, out string arguments);
                    await RunElevatedProcessAsync(executable, arguments, appDirectory, cancellationToken);
                    await Task.Delay(800, cancellationToken);
                    if (!AreVbEndpointsPresent())
                    {
                        await RestoreSnapshotAfterUninstallAsync(savedSnapshot, progress, cancellationToken);
                        return new VbOperationResult
                        {
                            Succeeded = true,
                            Message = BuildUninstallSuccessMessage()
                        };
                    }
                }

                return new VbOperationResult
                {
                    Succeeded = false,
                    Message = "卸载命令已执行，但系统仍检测到 VB-CABLE。请重启后再检查。"
                };
            }
            catch (OperationCanceledException)
            {
                return new VbOperationResult
                {
                    Succeeded = false,
                    Message = "操作已取消。"
                };
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                return new VbOperationResult
                {
                    Succeeded = false,
                    Message = "你取消了管理员授权，卸载未执行。"
                };
            }
            catch (Exception ex)
            {
                return new VbOperationResult
                {
                    Succeeded = false,
                    Message = $"卸载失败：{ex.Message}"
                };
            }
        }

        public static VbOperationResult TryRepairFormat()
        {
            using MMDevice renderEndpoint = WindowsAudioEndpointService.FindVbRenderEndpoint();
            using MMDevice captureEndpoint = WindowsAudioEndpointService.FindVbCaptureEndpoint();
            if (renderEndpoint == null || captureEndpoint == null)
            {
                return new VbOperationResult
                {
                    Succeeded = false,
                    Message = "未找到完整的 VB 设备（CABLE Input / CABLE Output）。请先安装 VB-CABLE。"
                };
            }

            bool renderFixed = WindowsAudioEndpointService.TrySetDeviceFormatTo16Bit48kStereo(renderEndpoint.ID, out string renderError);
            bool captureFixed = WindowsAudioEndpointService.TrySetDeviceFormatTo16Bit48kStereo(captureEndpoint.ID, out string captureError);
            if (renderFixed && captureFixed)
            {
                VbHealthReport report = BuildHealthReport();
                return new VbOperationResult
                {
                    Succeeded = report.IsFormatHealthy,
                    Message = report.IsFormatHealthy
                        ? "修复完成：VB 扬声器和 VB 麦克风均为 16bit/48k/2声道。"
                        : "修复命令已执行，但检测仍未达标，请到系统声音高级设置手动核对。"
                };
            }

            List<string> errors = new List<string>();
            if (!renderFixed)
            {
                errors.Add($"VB 扬声器修复失败：{renderError}");
            }

            if (!captureFixed)
            {
                errors.Add($"VB 麦克风修复失败：{captureError}");
            }

            return new VbOperationResult
            {
                Succeeded = false,
                Message = string.Join(Environment.NewLine, errors)
            };
        }

        private static async Task<(bool Restored, string ErrorMessage)> TryRestoreDefaultsWithRetryAsync(
            WindowsAudioEndpointService.DefaultDeviceSnapshot snapshot,
            IProgress<string> progress,
            CancellationToken cancellationToken)
        {
            const int maxAttempts = 18;
            const int delayMs = 1000;
            string lastError = "未知错误。";
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                bool restored = WindowsAudioEndpointService.TryRestoreDefaults(snapshot, out string errorMessage);
                if (restored)
                {
                    return (true, string.Empty);
                }

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    lastError = errorMessage;
                }

                progress?.Report($"正在恢复原默认设备（第 {attempt}/{maxAttempts} 次）...");
                await Task.Delay(delayMs, cancellationToken);
            }

            return (false, lastError);
        }

        private static async Task<bool> WaitForAudioEndpointRefreshAsync(bool requireVbEndpoints, IProgress<string> progress, CancellationToken cancellationToken)
        {
            const int maxAttempts = 12;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using MMDevice defaultRender = WindowsAudioEndpointService.FindDefaultDevice(DataFlow.Render);
                using MMDevice defaultCapture = WindowsAudioEndpointService.FindDefaultDevice(DataFlow.Capture);
                using MMDevice vbRender = WindowsAudioEndpointService.FindVbRenderEndpoint();
                using MMDevice vbCapture = WindowsAudioEndpointService.FindVbCaptureEndpoint();
                bool ready = requireVbEndpoints
                    ? defaultRender != null && defaultCapture != null && vbRender != null && vbCapture != null
                    : defaultRender != null && defaultCapture != null;
                if (ready)
                {
                    return true;
                }

                progress?.Report("正在等待系统刷新音频设备...");
                await Task.Delay(1000, cancellationToken);
            }

            return false;
        }

        private static string GetSnapshotFilePath(string appDirectory)
        {
            return Path.Combine(appDirectory, DefaultDeviceSnapshotFileName);
        }

        private static void SaveDefaultSnapshot(
            string snapshotFilePath,
            WindowsAudioEndpointService.DefaultDeviceSnapshot snapshot,
            IProgress<string> progress)
        {
            try
            {
                if (snapshot == null)
                {
                    return;
                }

                string json = JsonSerializer.Serialize(snapshot, SnapshotJsonOptions);
                File.WriteAllText(snapshotFilePath, json);
            }
            catch (Exception ex)
            {
                progress?.Report($"记录安装前默认设备快照失败：{ex.Message}");
            }
        }

        private static WindowsAudioEndpointService.DefaultDeviceSnapshot TryLoadDefaultSnapshot(
            string snapshotFilePath,
            IProgress<string> progress)
        {
            try
            {
                if (!File.Exists(snapshotFilePath))
                {
                    progress?.Report("未找到安装前默认设备快照，将跳过卸载后自动恢复。");
                    return null;
                }

                string json = File.ReadAllText(snapshotFilePath);
                var snapshot = JsonSerializer.Deserialize<WindowsAudioEndpointService.DefaultDeviceSnapshot>(json, SnapshotJsonOptions);
                return snapshot;
            }
            catch (Exception ex)
            {
                progress?.Report($"读取默认设备快照失败：{ex.Message}");
                return null;
            }
        }

        private static async Task RestoreSnapshotAfterUninstallAsync(
            WindowsAudioEndpointService.DefaultDeviceSnapshot savedSnapshot,
            IProgress<string> progress,
            CancellationToken cancellationToken)
        {
            if (savedSnapshot == null)
            {
                return;
            }

            await WaitForAudioEndpointRefreshAsync(requireVbEndpoints: false, progress, cancellationToken);
            progress?.Report("正在恢复安装前默认设备...");
            var restoreResult = await TryRestoreDefaultsWithRetryAsync(savedSnapshot, progress, cancellationToken);
            if (!restoreResult.Restored)
            {
                progress?.Report($"卸载后恢复安装前默认设备失败：{restoreResult.ErrorMessage}");
            }
            else
            {
                progress?.Report("卸载后已恢复安装前默认设备。");
            }
        }

        private static string DescribeSnapshotEndpoint(
            WindowsAudioEndpointService.EndpointMatchProfile profile,
            string fallbackFriendlyName,
            string endpointId)
        {
            string displayName = FirstNonEmpty(profile?.FriendlyName, fallbackFriendlyName, "未知设备名");
            string displayLabel = FirstNonEmpty(profile?.DeviceLabel, "未知类型");
            string displayManufacturer = FirstNonEmpty(profile?.Manufacturer, "未知厂商");
            string displayFormat = profile != null && profile.SampleRate > 0 && profile.BitsPerSample > 0
                ? $"{profile.BitsPerSample}位/{profile.SampleRate}Hz/{Math.Max(1, profile.Channels)}声道"
                : "未知格式";
            string displayDriverVersion = FirstNonEmpty(profile?.DriverVersion, "未知版本");
            string displayDriverDate = FirstNonEmpty(profile?.DriverDate, "未知日期");
            string displayId = string.IsNullOrWhiteSpace(endpointId) ? "未记录ID" : endpointId;

            return $"{displayLabel} {displayName}; 厂商={displayManufacturer}; 格式={displayFormat}; 驱动={displayDriverVersion}; 日期={displayDriverDate}; ID={displayId}";
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static async Task<VbPackagePaths> EnsurePackageReadyAsync(
            string appDirectory,
            IProgress<string> progress,
            CancellationToken cancellationToken)
        {
            string zipPath = Path.Combine(appDirectory, PackageZipFileName);
            string packageDirectory = Path.Combine(appDirectory, PackageFolderName);
            if (!TryResolveInstallerPath(packageDirectory, out string installerPath))
            {
                if (!File.Exists(zipPath))
                {
                    progress?.Report("正在下载 VB-CABLE 安装包...");
                    using HttpClient httpClient = new HttpClient();
                    using HttpResponseMessage response = await httpClient.GetAsync(VbDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    await using Stream sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    await using FileStream targetStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await sourceStream.CopyToAsync(targetStream, cancellationToken);
                }

                progress?.Report("正在解压安装包...");
                Directory.CreateDirectory(packageDirectory);
                ZipFile.ExtractToDirectory(zipPath, packageDirectory, true);
                if (!TryResolveInstallerPath(packageDirectory, out installerPath))
                {
                    throw new FileNotFoundException("未找到 VBCABLE_Setup 安装程序。");
                }
            }

            return new VbPackagePaths
            {
                PackageDirectory = packageDirectory,
                InstallerPath = installerPath
            };
        }

        private static bool TryResolveInstallerPath(string packageDirectory, out string installerPath)
        {
            installerPath = string.Empty;
            if (!Directory.Exists(packageDirectory))
            {
                return false;
            }

            string x64InstallerPath = Path.Combine(packageDirectory, InstallerNameX64);
            if (File.Exists(x64InstallerPath))
            {
                installerPath = x64InstallerPath;
                return true;
            }

            string x86InstallerPath = Path.Combine(packageDirectory, InstallerNameX86);
            if (File.Exists(x86InstallerPath))
            {
                installerPath = x86InstallerPath;
                return true;
            }

            string fallbackInstaller = Directory
                .GetFiles(packageDirectory, "VBCABLE_Setup*.exe", SearchOption.TopDirectoryOnly)
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(fallbackInstaller))
            {
                installerPath = fallbackInstaller;
                return true;
            }

            return false;
        }

        private static bool TryGetUninstallCommand(out string uninstallCommand)
        {
            uninstallCommand = string.Empty;
            foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                using RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                if (TryReadUninstallString(localMachine, out uninstallCommand))
                {
                    return true;
                }

                using RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view);
                if (TryReadUninstallString(currentUser, out uninstallCommand))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryReadUninstallString(RegistryKey baseKey, out string uninstallCommand)
        {
            uninstallCommand = string.Empty;
            using RegistryKey uninstallRoot = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (uninstallRoot == null)
            {
                return false;
            }

            foreach (string subKeyName in uninstallRoot.GetSubKeyNames())
            {
                using RegistryKey subKey = uninstallRoot.OpenSubKey(subKeyName);
                string displayName = subKey?.GetValue("DisplayName") as string ?? string.Empty;
                if (!displayName.Contains("CABLE", StringComparison.OrdinalIgnoreCase)
                    && !displayName.Contains("VB-Audio", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string quietUninstall = subKey.GetValue("QuietUninstallString") as string;
                if (!string.IsNullOrWhiteSpace(quietUninstall))
                {
                    uninstallCommand = Environment.ExpandEnvironmentVariables(quietUninstall);
                    return true;
                }

                string uninstall = subKey.GetValue("UninstallString") as string;
                if (!string.IsNullOrWhiteSpace(uninstall))
                {
                    uninstallCommand = Environment.ExpandEnvironmentVariables(uninstall);
                    return true;
                }
            }

            return false;
        }

        private static void ParseCommand(string command, out string executable, out string arguments)
        {
            executable = command?.Trim() ?? string.Empty;
            arguments = string.Empty;
            if (string.IsNullOrWhiteSpace(executable))
            {
                return;
            }

            if (executable.StartsWith("\"", StringComparison.Ordinal))
            {
                int endQuoteIndex = executable.IndexOf('"', 1);
                if (endQuoteIndex > 1)
                {
                    arguments = executable.Substring(endQuoteIndex + 1).Trim();
                    executable = executable.Substring(1, endQuoteIndex - 1);
                    return;
                }
            }

            int firstSpaceIndex = executable.IndexOf(' ');
            if (firstSpaceIndex > 0)
            {
                arguments = executable.Substring(firstSpaceIndex + 1).Trim();
                executable = executable.Substring(0, firstSpaceIndex);
            }
        }

        private static async Task<int> RunElevatedProcessAsync(
            string filePath,
            string arguments,
            string workingDirectory,
            CancellationToken cancellationToken)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = arguments ?? string.Empty,
                WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory) ? AppDomain.CurrentDomain.BaseDirectory : workingDirectory,
                UseShellExecute = true,
                Verb = "runas"
            };

            using Process process = Process.Start(startInfo);
            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode;
        }

        private static bool AreVbEndpointsPresent()
        {
            using MMDevice renderEndpoint = WindowsAudioEndpointService.FindVbRenderEndpoint();
            using MMDevice captureEndpoint = WindowsAudioEndpointService.FindVbCaptureEndpoint();
            return renderEndpoint != null || captureEndpoint != null;
        }

        private static string BuildUninstallSuccessMessage()
        {
            if (SystemAudioService.CheckVirtualCableInstalled())
            {
                return "VB-CABLE 已卸载完成。当前仅检测到注册表残留，通常可忽略，重启后会刷新。";
            }

            return "VB-CABLE 已卸载完成。";
        }
    }
}
