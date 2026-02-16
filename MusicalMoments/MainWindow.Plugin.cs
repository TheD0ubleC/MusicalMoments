using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MusicalMoments
{
    public partial class MainWindow
    {
        private readonly Dictionary<string, Process> loadedPluginProcesses = new Dictionary<string, Process>(StringComparer.OrdinalIgnoreCase);

        private void TogglePluginServer_Click(object sender, EventArgs e)
        {
            if (!pluginServer)
            {
                if (!Misc.IsAdministrator())
                {
                    MessageBox.Show("插件服务要求 MM 以管理员身份运行。\r\n请以管理员身份重启 MM 后再开启插件服务。", "需要管理员权限");
                    return;
                }

                PluginSDK.PluginServer.StartServer();
                pluginServer = true;
                TogglePluginServer.Text = "关闭插件服务";
                PluginStatus.Text = "插件状态:已开启(WS)";
                LoadPlugin.Enabled = true;
                pluginListView.Enabled = true;
                PluginServerAddress.Text = PluginSDK.PluginServer.GetServerAddress();
                RefreshPluginListUi();
            }
            else
            {
                PluginSDK.PluginServer.StopServer();
                pluginServer = false;
                TogglePluginServer.Text = "开启插件服务";
                PluginStatus.Text = "插件状态:未开启";
                LoadPlugin.Enabled = false;
                pluginListView.Enabled = false;
                PluginServerAddress.Text = string.Empty;
                UpdatePluginLoadStateIndicators();
            }
        }

        private void PluginServerAddress_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PluginServerAddress.Text))
            {
                Clipboard.SetText(PluginServerAddress.Text);
            }
        }

        private void mToPluginData_Click(object sender, EventArgs e)
        {
            string folderPath = Path.Combine(runningDirectory, "Plugin");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }

        private void LoadPlugin_Click(object sender, EventArgs e)
        {
            if (!pluginServer || !PluginSDK.PluginServer.IsRunning)
            {
                MessageBox.Show("请先开启插件服务。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (pluginListView.SelectedItems.Count <= 0)
            {
                MessageBox.Show("请选择要加载的插件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListViewItem selectedItem = pluginListView.SelectedItems[0];
            string pluginFilePath = NormalizePluginPath(selectedItem.Tag as string);
            if (string.IsNullOrWhiteSpace(pluginFilePath) || !File.Exists(pluginFilePath))
            {
                MessageBox.Show("插件可执行文件不存在，请刷新插件列表后重试。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RefreshPluginListUi();
                return;
            }

            if (IsPluginRunning(pluginFilePath))
            {
                MessageBox.Show("该插件已加载，无需重复加载。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdatePluginLoadStateIndicators();
                return;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pluginFilePath,
                    WorkingDirectory = Path.GetDirectoryName(pluginFilePath) ?? runningDirectory,
                    UseShellExecute = false
                };
                string wsAddress = PluginSDK.PluginServer.GetServerAddress();
                startInfo.ArgumentList.Add(wsAddress);
                startInfo.EnvironmentVariables["MM_PLUGIN_WS_ADDRESS"] = wsAddress;

                Process process = Process.Start(startInfo);
                if (process == null)
                {
                    MessageBox.Show("插件启动失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                process.EnableRaisingEvents = true;
                string trackedPath = pluginFilePath;
                process.Exited += (_, _) =>
                {
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            if (loadedPluginProcesses.TryGetValue(trackedPath, out Process currentProcess) && ReferenceEquals(currentProcess, process))
                            {
                                loadedPluginProcesses.Remove(trackedPath);
                                currentProcess.Dispose();
                            }

                            UpdatePluginLoadStateIndicators();
                        }));
                    }
                    catch
                    {
                        // Ignore UI disposal/exit race conditions.
                    }
                };

                loadedPluginProcesses[pluginFilePath] = process;
                UpdatePluginLoadStateIndicators();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"请确认该插件是否可执行。\r\n错误详情:\r\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void reLoadPluginListsView_Click(object sender, EventArgs e)
        {
            RefreshPluginListUi();
        }

        private void RefreshPluginListUi()
        {
            string pluginDirectory = Path.Combine(runningDirectory, "Plugin");
            Misc.AddPluginFilesToListView(pluginDirectory, pluginListView);
            UpdatePluginLoadStateIndicators();
        }

        private void UpdatePluginLoadStateIndicators()
        {
            CleanupExitedPlugins();
            foreach (ListViewItem item in pluginListView.Items)
            {
                if (item.SubItems.Count < 3)
                {
                    continue;
                }

                string pluginPath = NormalizePluginPath(item.Tag as string);
                bool loaded = IsPluginRunning(pluginPath);
                string rawVersion = item.SubItems[2].Text ?? string.Empty;
                string cleanVersion = RemoveLoadStateSuffix(rawVersion);
                string statusSuffix = loaded ? "已加载" : "未加载";
                item.SubItems[2].Text = $"{cleanVersion} | {statusSuffix}";
                item.ForeColor = loaded ? Color.FromArgb(34, 139, 34) : SystemColors.WindowFrame;
            }
        }

        private void CleanupExitedPlugins()
        {
            foreach (var pair in loadedPluginProcesses.ToList())
            {
                Process process = pair.Value;
                if (process == null || process.HasExited)
                {
                    loadedPluginProcesses.Remove(pair.Key);
                    try
                    {
                        process?.Dispose();
                    }
                    catch
                    {
                        // Ignore disposal failures.
                    }
                }
            }
        }

        private bool IsPluginRunning(string pluginFilePath)
        {
            if (string.IsNullOrWhiteSpace(pluginFilePath))
            {
                return false;
            }

            if (!loadedPluginProcesses.TryGetValue(pluginFilePath, out Process process))
            {
                return false;
            }

            return process != null && !process.HasExited;
        }

        private static string RemoveLoadStateSuffix(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            int splitIndex = text.IndexOf(" | ", StringComparison.Ordinal);
            return splitIndex >= 0 ? text.Substring(0, splitIndex) : text;
        }

        private static string NormalizePluginPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            try
            {
                return Path.GetFullPath(path);
            }
            catch
            {
                return path;
            }
        }
    }
}
