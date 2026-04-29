using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using NAudio.Wave;
using System.Reflection;
using System.Text;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Security.Policy;
using NAudio.Gui;
using TagLib.Mpeg;

using File = System.IO.File;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.VisualBasic.Devices;
using System.IO;
using System.IO.Compression;
using System.Net.Mail;
using Newtonsoft.Json.Linq;

namespace MusicalMoments
{
    public partial class MainWindow : Form
    {
        public static MainWindow CurrentInstance { get; private set; }
        public static string nowVer = VersionService.CurrentVersionTag;
        public static string runningDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static Keys toggleStreamKey;
        public static Keys playAudioKey;
        public static string selectedAudioPath;
        public static string selectedPluginPath;
        public static int closeCount = 0;
        public static int playedCount = 0;
        public static int changedCount = 0;
        public static string firstStart = System.DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒");
        public static bool playAudio = true;
        private GlobalInputHook globalInputHook;
        private Keys pendingToggleStreamBinding = Keys.None;
        private Keys pendingPlayAudioBinding = Keys.None;
        public static bool isPlaying = false;
        public static float VBvolume = 1f;
        public static float volume = 1f;
        public static float tipsvolume = 1f;

        public static int VBInputComboSelect = 0;
        public static int VBOutputComboSelect = 0;
        public static int InputComboSelect = 0;
        public static int OutputComboSelect = 0;
        public static bool AudioEquipmentPlayCheck = true;

        public static List<AudioInfo> audioInfo = new List<AudioInfo>();
        private static readonly HttpClient UpdateDownloadClient = CreateUpdateDownloadClient();
        private static readonly string[] UpdateDownloadHosts = { "https://github.scmd.cc", "https://github.com" };
        private int updateCheckInProgress;
        private bool synchronizingNavigationSelection;
        private int lastNavigationIndex = -1;
        private int visualNavigationIndex;
        public MainWindow()
        {
            InitializeComponent();
            WinFormsWhiteTheme.ApplyToForm(this);
            WinFormsWhiteTheme.ApplyToToolStrip(mainContextMenuStrip);
            CurrentInstance = this;
            InitializeCloseBehaviorUx();
            InitializeTrayFeatures();
            InitializeResponsiveLayout();
            InitializeAudioPageUx();
            InitializeOptimizationPageUx();
            SyncSideNavigationSelection(force: true);
            switchStreamTips.CheckedChanged += (_, _) => UpdateSwitchStreamTipsCache();
            UpdateSwitchStreamTipsCache();
            Subscribe();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }
        public void Subscribe()
        {
            if (globalInputHook != null)
            {
                return;
            }

            globalInputHook = new GlobalInputHook();
            globalInputHook.HotkeyPressed += GlobalHotkeyPressed;
            globalInputHook.Start();
            StartHotkeyDispatcher();
        }

        public void Unsubscribe()
        {
            if (globalInputHook == null)
            {
                return;
            }

            globalInputHook.HotkeyPressed -= GlobalHotkeyPressed;
            globalInputHook.Dispose();
            globalInputHook = null;
            StopHotkeyDispatcher();
        }

        private void GlobalHotkeyPressed(object sender, GlobalHotkeyEventArgs e)
        {
            EnqueueGlobalHotkey(e.Key);
        }

        internal void TogglePlaybackMode(bool playSwitchTip)
        {
            TogglePlaybackModeCore(playSwitchTip);
        }
        private void sideLists_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            int selectedIndex = sideLists.SelectedIndices.Count > 0
                ? sideLists.SelectedIndices[0]
                : Math.Clamp(visualNavigationIndex, 0, sideLists.Items.Count - 1);
            bool isSelected = e.Item.Index == selectedIndex;
            Color backgroundColor = isSelected ? Color.FromArgb(236, 244, 255) : sideLists.BackColor;
            Color textColor = isSelected ? Color.FromArgb(28, 99, 178) : Color.FromArgb(85, 85, 85);

            using (SolidBrush backgroundBrush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            }

            if (e.Item.ImageIndex >= 0
                && sideLists.SmallImageList != null
                && e.Item.ImageIndex < sideLists.SmallImageList.Images.Count)
            {
                int iconSize = 18;
                int iconX = e.Bounds.Left + 6;
                int iconY = e.Bounds.Top + Math.Max(0, (e.Bounds.Height - iconSize) / 2);
                Rectangle iconBounds = new Rectangle(iconX, iconY, iconSize, iconSize);
                e.Graphics.DrawImage(sideLists.SmallImageList.Images[e.Item.ImageIndex], iconBounds);
            }

            Rectangle textBounds = new Rectangle(
                e.Bounds.Left + 30,
                e.Bounds.Top,
                Math.Max(8, e.Bounds.Width - 34),
                e.Bounds.Height);
            TextRenderer.DrawText(
                e.Graphics,
                e.Item.Text,
                e.Item.Font,
                textBounds,
                textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            if (e.Item.Index < sideLists.Items.Count - 1)
            {
                using (Pen splitPen = new Pen(Color.FromArgb(220, 220, 220)))
                {
                    int y = e.Bounds.Bottom - 1;
                    e.Graphics.DrawLine(splitPen, e.Bounds.Left + 6, y, e.Bounds.Right - 6, y);
                }
            }
        }
        private async void MainWindow_Load(object sender, EventArgs e)
        {
            Visible = false;
            foreach (TabPage tabPage in mainTabControl.TabPages)
            {
                mainTabControl.SelectTab(tabPage);
                // 我有强迫症 所以防止切换选项卡的时候有卡顿的加载就在启动前先都切一遍:D
            }

            if (mainTabControl.TabPages.Count > 0)
            {
                mainTabControl.SelectedIndex = 0;
                visualNavigationIndex = 0;
            }

            /*
            用于构建本地化基文件(感觉用不上这东西 别说海外用户了 能有几个国内用户用我就心满意足了TT)
            BuildLocalizationBaseFiles(this.Controls, $"{runningDirectory}Resources.zh-CN.resx");
            */
            await Misc.FadeIn(200, this);

            mainTabControl.ItemSize = new System.Drawing.Size(0, 1);
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioData"));//创建存放音频的文件夹
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugin"));//创建存放插件的文件夹
            await reLoadList();
            RefreshPluginListUi();
            if (!Misc.IsAdministrator()) { Text += " [当前非管理员运行,可能会出现按下按键无反应]"; }
            else { Text += " MusicalMoments"; }


            label_VBStatus.Text = Misc.checkVB() ? "VB声卡状态：已安装" : "VB声卡状态：未安装";
            comboBoxOutputFormat.SelectedIndex = 0;

            RefreshAudioDeviceCombosPreserveSelection();
            VBVolumeTrackBar_Scroll(null, null);
            VolumeTrackBar_Scroll(null, null);
            TipsVolumeTrackBar_Scroll(null, null);
            numberLabel.Text = "";


            LoadUserData();
            StartClientUsageReporter();
            UpdateAboutVersionInfo();
            SyncSideNavigationSelection(force: true);
            if (CheckDuplicateKeys()) { MessageBox.Show($"已检测到相同按键 请勿作死将两个或多个音频绑定在同个按键上 该操作可能会导致MM崩溃 此提示会在Bind Key时与软件启动时检测并发出", "温馨提示"); }
            await RefreshVbHealthUiAsync(showDialog: false);
            if (!Misc.IsAdministrator())
            {
                toVB.Text = "安装VB(需管理员)";
                mToAudioData.Text = "卸载VB(需管理员)";
                help_tip.Text = "状态：当前非管理员运行，无法执行一键安装/卸载";
                help_tip.ForeColor = Color.FromArgb(178, 34, 34);
            }
            //   最后再版本验证 以防UI错误
            _ = CheckNewVer();

        }






        private void UpdateAboutVersionInfo()
        {
            string versionTag = VersionService.CurrentVersionTag;
            string versionNumber = VersionService.CurrentVersionNumber;
            string[] versionParts = versionNumber.Split('.', StringSplitOptions.RemoveEmptyEntries);
            string major = versionParts.Length > 0 ? versionParts[0] : "0";
            string minor = versionParts.Length > 1 ? versionParts[1] : "0";
            string patch = versionParts.Length > 2 ? versionParts[2] : "0";

            string releaseChannel = "release";
            string[] tagParts = versionTag.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (tagParts.Length >= 2)
            {
                releaseChannel = tagParts[1];
            }

            string buildTimestampText = GetBuildTimestampText();

            info_Label2.Text = $"版本号:{versionTag}";
            info_Label3.Text =
                $"- 主版本号（Major Version）：{major}\r\n" +
                $"- 次版本号（Minor Version）：{minor}\r\n" +
                $"- 修订号（Patch Version）：{patch}\r\n" +
                $"- 预发布版本号（Pre-release Version）：{releaseChannel}\r\n" +
                $"- 构建时间（Build Time）：{buildTimestampText}";
        }

        private static string GetBuildTimestampText()
        {
            try
            {
                string processPath = Environment.ProcessPath ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(processPath) && File.Exists(processPath))
                {
                    return File.GetLastWriteTime(processPath).ToString("yyyy/MM/dd HH:mm:ss");
                }

                string assemblyPath = typeof(MainWindow).Assembly.Location;
                if (!string.IsNullOrWhiteSpace(assemblyPath) && File.Exists(assemblyPath))
                {
                    return File.GetLastWriteTime(assemblyPath).ToString("yyyy/MM/dd HH:mm:ss");
                }
            }
            catch
            {
                // Ignore version metadata read failures.
            }

            return "未知";
        }

        private async Task CheckNewVer(bool noUpdateShowMessage = false)
        {
            if (Interlocked.Exchange(ref updateCheckInProgress, 1) == 1)
            {
                return;
            }

            try
            {
                ReleaseInfo latestRelease = await UpdateService.GetLatestReleaseAsync();
                string latestVer = latestRelease.TagName?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(latestVer))
                {
                    if (noUpdateShowMessage)
                    {
                        MessageBox.Show("检查更新失败，请稍后重试。", "提示");
                    }

                    return;
                }

                if (string.Equals(latestVer, nowVer, StringComparison.OrdinalIgnoreCase))
                {
                    if (noUpdateShowMessage)
                    {
                        MessageBox.Show("当前已是最新版本！", "提示");
                    }

                    return;
                }

                string newVerTips = string.IsNullOrWhiteSpace(latestRelease.Body) ? "暂无更新说明。" : latestRelease.Body;
                DialogResult dialogResult = MessageBox.Show(
                    $"Musical Moments存在新版本，请尽快更新。当前版本为{nowVer} 最新版本为{latestVer}。\r\n按下是则自动下载最新版本压缩包 按下否则跳转至最新版本页面 按下取消则关闭\r\n\r\n以下是新版本简介:\r\n{newVerTips}",
                    "新版本推送",
                    MessageBoxButtons.YesNoCancel);

                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        await StartAutoUpdateAsync(latestRelease);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"自动更新失败，请稍后重试或手动下载。\r\n错误详情:{ex}", "错误");
                    }
                }
                else if (dialogResult == DialogResult.No)
                {
                    string releasePageUrl = ResolveReleasePageUrl(latestRelease, latestVer);
                    Process.Start(new ProcessStartInfo(releasePageUrl) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                if (noUpdateShowMessage)
                {
                    MessageBox.Show($"检查更新失败：{ex.Message}", "错误");
                }
            }
            finally
            {
                Interlocked.Exchange(ref updateCheckInProgress, 0);
            }
        }

        private async Task StartAutoUpdateAsync(ReleaseInfo release)
        {
            string latestVer = release.TagName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(latestVer))
            {
                throw new InvalidOperationException("未获取到有效版本号。");
            }

            string updaterPath = Path.Combine(runningDirectory, "MusicalMoments.Updater.exe");
            if (!File.Exists(updaterPath))
            {
                throw new FileNotFoundException("未找到更新器，请重新下载完整版本。", updaterPath);
            }

            string version = VersionService.ExtractVersionNumber(latestVer);
            string packageFileName = $"MM.Release-{version}.zip";
            string tempUpdateDirectory = Path.Combine(Path.GetTempPath(), "MusicalMoments", "Update");
            Directory.CreateDirectory(tempUpdateDirectory);
            string zipPath = Path.Combine(tempUpdateDirectory, packageFileName);

            List<string> candidateUrls = BuildUpdatePackageUrls(release, version);
            if (candidateUrls.Count == 0)
            {
                throw new InvalidOperationException("未找到可用的更新包下载地址。");
            }

            Exception? lastError = null;
            bool isDownloaded = false;
            foreach (string downloadUrl in candidateUrls)
            {
                try
                {
                    await DownloadUpdatePackageAsync(downloadUrl, zipPath);
                    ValidateDownloadedPackage(zipPath);
                    isDownloaded = true;
                    break;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }

            if (!isDownloaded)
            {
                throw new InvalidOperationException("自动下载更新包失败。", lastError);
            }

            Process currentProcess = Process.GetCurrentProcess();
            bool updaterStarted = StartApplication(updaterPath, currentProcess.Id.ToString(), zipPath, runningDirectory);
            if (!updaterStarted)
            {
                throw new InvalidOperationException("启动更新器失败。");
            }

            MessageBox.Show($"更新包下载并校验成功，程序将重启并自动完成更新。\r\n更新包路径:{zipPath}", "提示");
            Close();
        }

        private static List<string> BuildUpdatePackageUrls(ReleaseInfo release, string version)
        {
            string latestVer = release.TagName?.Trim() ?? string.Empty;
            string expectedPackageName = $"MM.Release-{version}.zip";
            List<(string Url, int Score)> rankedCandidates = new List<(string Url, int Score)>();

            foreach (ReleaseAssetInfo asset in release.Assets)
            {
                string assetUrl = asset.DownloadUrl?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(assetUrl))
                {
                    continue;
                }

                string assetName = asset.Name?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(assetName))
                {
                    assetName = Path.GetFileName(assetUrl);
                }

                if (!assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int score = 0;
                if (assetName.Equals(expectedPackageName, StringComparison.OrdinalIgnoreCase))
                {
                    score += 1000;
                }
                if (assetName.IndexOf(version, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score += 100;
                }
                if (assetName.IndexOf("release", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score += 30;
                }
                if (assetName.IndexOf("x64", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score += 10;
                }

                rankedCandidates.Add((assetUrl, score));
            }

            rankedCandidates.Sort((left, right) => right.Score.CompareTo(left.Score));

            List<string> candidateUrls = new List<string>();
            foreach ((string url, _) in rankedCandidates)
            {
                AddMirroredUrlCandidates(candidateUrls, url);
            }

            if (!string.IsNullOrWhiteSpace(latestVer))
            {
                foreach (string host in UpdateDownloadHosts)
                {
                    AddMirroredUrlCandidates(candidateUrls, $"{host}/TheD0ubleC/MusicalMoments/releases/download/{latestVer}/{expectedPackageName}");
                }
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> deduplicatedUrls = new List<string>();
            foreach (string candidateUrl in candidateUrls)
            {
                if (string.IsNullOrWhiteSpace(candidateUrl) || !seen.Add(candidateUrl))
                {
                    continue;
                }

                deduplicatedUrls.Add(candidateUrl);
            }

            return deduplicatedUrls;
        }

        private static void AddMirroredUrlCandidates(List<string> targetUrls, string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return;
            }

            targetUrls.Add(rawUrl);

            const string githubHost = "://github.com/";
            const string mirrorHost = "://github.scmd.cc/";
            if (rawUrl.Contains(githubHost, StringComparison.OrdinalIgnoreCase))
            {
                targetUrls.Add(rawUrl.Replace(githubHost, mirrorHost, StringComparison.OrdinalIgnoreCase));
            }
            else if (rawUrl.Contains(mirrorHost, StringComparison.OrdinalIgnoreCase))
            {
                targetUrls.Add(rawUrl.Replace(mirrorHost, githubHost, StringComparison.OrdinalIgnoreCase));
            }
        }

        private static string ResolveReleasePageUrl(ReleaseInfo release, string latestVer)
        {
            List<string> candidateUrls = new List<string>();
            AddMirroredUrlCandidates(candidateUrls, release.HtmlUrl ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(latestVer))
            {
                foreach (string host in UpdateDownloadHosts)
                {
                    candidateUrls.Add($"{host}/TheD0ubleC/MusicalMoments/releases/tag/{latestVer}");
                }
            }

            foreach (string url in candidateUrls)
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    return url;
                }
            }

            return "https://github.com/TheD0ubleC/MusicalMoments/releases";
        }

        private static async Task DownloadUpdatePackageAsync(string downloadUrl, string zipPath)
        {
            using HttpResponseMessage response = await UpdateDownloadClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using Stream packageStream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await packageStream.CopyToAsync(fileStream);
        }

        private static void ValidateDownloadedPackage(string zipPath)
        {
            using ZipArchive zip = ZipFile.OpenRead(zipPath);
            if (zip.Entries.Count == 0)
            {
                throw new InvalidDataException("更新包为空。");
            }

            bool hasMainExecutable = false;
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                if (string.Equals(Path.GetFileName(entry.FullName), "MusicalMoments.exe", StringComparison.OrdinalIgnoreCase))
                {
                    hasMainExecutable = true;
                    break;
                }
            }

            if (!hasMainExecutable)
            {
                throw new InvalidDataException("更新包缺少主程序文件。");
            }
        }

        private static HttpClient CreateUpdateDownloadClient()
        {
            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MusicalMoments-Updater/2.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/octet-stream");
            return client;
        }

        private static bool StartApplication(string applicationPath, params string[] arguments)
        {
            try
            {
                if (File.Exists(applicationPath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = applicationPath,
                        WorkingDirectory = Path.GetDirectoryName(applicationPath) ?? runningDirectory,
                        UseShellExecute = false
                    };
                    foreach (string argument in arguments)
                    {
                        startInfo.ArgumentList.Add(argument);
                    }
                    Process.Start(startInfo);
                    Console.WriteLine($"Started application: {applicationPath} with arguments: {string.Join(" ", arguments)}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Application not found: {applicationPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting application: {ex.Message}");
            }

            return false;
        }
        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mainTabControl.SelectedIndex >= 0)
            {
                visualNavigationIndex = mainTabControl.SelectedIndex;
            }

            SyncSideNavigationSelection(force: false);
            if (mainTabControl.SelectedIndex >= 0 && mainTabControl.SelectedIndex < mainTabControl.TabPages.Count)
            {
                mainGroupBox.Text = mainTabControl.TabPages[mainTabControl.SelectedIndex].Text;
            }
        }

        private void SyncSideNavigationSelection(bool force)
        {
            if (sideLists == null || sideLists.Items.Count == 0)
            {
                return;
            }

            int targetIndex = Math.Clamp(mainTabControl.SelectedIndex, 0, sideLists.Items.Count - 1);
            visualNavigationIndex = targetIndex;
            if (!force
                && sideLists.SelectedIndices.Count == 1
                && sideLists.SelectedIndices[0] == targetIndex)
            {
                return;
            }

            synchronizingNavigationSelection = true;
            try
            {
                sideLists.BeginUpdate();
                foreach (ListViewItem item in sideLists.Items)
                {
                    item.Selected = false;
                }

                sideLists.Items[targetIndex].Selected = true;
                sideLists.Items[targetIndex].Focused = true;
                lastNavigationIndex = targetIndex;
            }
            finally
            {
                sideLists.EndUpdate();
                synchronizingNavigationSelection = false;
                sideLists.RedrawItems(0, sideLists.Items.Count - 1, false);
            }
        }

        private async void sideLists_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (synchronizingNavigationSelection)
            {
                return;
            }

            if (sideLists.SelectedIndices.Count <= 0)
            {
                // ListView may raise a transient "no selection" event while switching rows.
                // Ignore this state to avoid rolling back user navigation.
                if (sideLists.Items.Count > 0)
                {
                    sideLists.RedrawItems(0, sideLists.Items.Count - 1, false);
                }
                return;
            }

            int selectedIndex = sideLists.SelectedIndices[0];
            if (selectedIndex < 0 || selectedIndex >= mainTabControl.TabPages.Count)
            {
                return;
            }

            if (selectedIndex == lastNavigationIndex && mainTabControl.SelectedIndex == selectedIndex)
            {
                return;
            }

            lastNavigationIndex = selectedIndex;
            visualNavigationIndex = selectedIndex;
            if (mainTabControl.SelectedIndex != selectedIndex)
            {
                mainTabControl.SelectedIndex = selectedIndex;
            }

            if (sideLists.SelectedItems.Count > 0)
            {
                mainGroupBox.Text = sideLists.SelectedItems[0].Text;
            }

            if (selectedIndex == 1)
            {
                await reLoadList();
            }
            else if (selectedIndex == 2)
            {
                RefreshAudioDeviceCombosPreserveSelection();
            }
            else if (selectedIndex == 8)
            {
                RefreshPluginListUi();
            }
            else if (selectedIndex == 7)
            {
                AudioListView_fd.Items.Clear();
            }

            sideLists.RedrawItems(0, sideLists.Items.Count - 1, false);
        }

        private void sideLists_MouseDown(object sender, MouseEventArgs e)
        {
            if (synchronizingNavigationSelection)
            {
                return;
            }

            ListViewHitTestInfo hitInfo = sideLists.HitTest(e.Location);
            if (hitInfo?.Item == null)
            {
                return;
            }

            if (!hitInfo.Item.Selected)
            {
                hitInfo.Item.Selected = true;
                hitInfo.Item.Focused = true;
            }
        }
        private async void retestVB_Click(object sender, EventArgs e)
        {
            if (vbOperationBusy)
            {
                MessageBox.Show("当前已有任务在执行，请稍候。", "提示");
                return;
            }

            try
            {
                SetVbOperationBusy(true, "正在检查 VB 状态...");
                await RefreshVbHealthUiAsync(showDialog: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"检查 VB 状态时出错：{ex.Message}", "错误");
            }
            finally
            {
                SetVbOperationBusy(false, null);
            }
        }
        private void audioListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTestInfo = audioListView.HitTest(e.Location);
                if (hitTestInfo.Item != null)
                {
                    // 显示菜单在鼠标点击的位置
                    mainContextMenuStrip.Show(audioListView, e.Location);
                }
            }
        }

        private void ApplyBindingCaptureToTextBox(System.Windows.Forms.TextBox textBox, Keys key, string displayText, ref Keys targetKey, ref Keys pendingBinding)
        {
            pendingBinding = Keys.None;
            targetKey = KeyBindingService.NormalizeBinding(key);
            textBox.Text = displayText;
            textBox.Enabled = false;
            textBox.Enabled = true;
        }

        private void HandleBindingTextBoxKeyDown(System.Windows.Forms.TextBox textBox, KeyEventArgs e, ref Keys targetKey, ref Keys pendingBinding)
        {
            if (!KeyBindingService.TryBuildBindingFromKeyEvent(e, out Keys key, out string displayText))
            {
                return;
            }

            textBox.Text = displayText;
            e.SuppressKeyPress = true;
            if (KeyBindingService.IsModifierOnlyBinding(key))
            {
                pendingBinding = key;
                return;
            }

            ApplyBindingCaptureToTextBox(textBox, key, displayText, ref targetKey, ref pendingBinding);
        }

        private void HandleBindingTextBoxKeyUp(System.Windows.Forms.TextBox textBox, KeyEventArgs e, ref Keys targetKey, ref Keys pendingBinding)
        {
            if (!KeyBindingService.ShouldCommitPendingBindingOnKeyUp(e, pendingBinding))
            {
                return;
            }

            Keys key = pendingBinding;
            ApplyBindingCaptureToTextBox(textBox, key, KeyBindingService.GetDisplayTextForKey(key), ref targetKey, ref pendingBinding);
            e.SuppressKeyPress = true;
        }

        private void ToggleStream_KeyDown(object sender, KeyEventArgs e)
        {
            HandleBindingTextBoxKeyDown(ToggleStream, e, ref toggleStreamKey, ref pendingToggleStreamBinding);
        }
        private void ToggleStream_KeyUp(object sender, KeyEventArgs e)
        {
            HandleBindingTextBoxKeyUp(ToggleStream, e, ref toggleStreamKey, ref pendingToggleStreamBinding);
        }
        private void ToggleStream_KeyPress(object sender, KeyPressEventArgs e)
        {
            // KeyDown handles binding capture, keep KeyPress from injecting text.
            e.Handled = true;
        }
        private void ToggleStream_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                && sender is Control control
                && !control.Focused)
            {
                return;
            }

            if (!KeyBindingService.TryGetSupportedMouseBinding(e.Button, Control.ModifierKeys, out Keys key, out string displayText))
            {
                return;
            }

            ApplyBindingCaptureToTextBox(ToggleStream, key, displayText, ref toggleStreamKey, ref pendingToggleStreamBinding);
        }
        private void PlayAudio_KeyDown(object sender, KeyEventArgs e)
        {
            HandleBindingTextBoxKeyDown(PlayAudio, e, ref playAudioKey, ref pendingPlayAudioBinding);
        }
        private void PlayAudio_KeyUp(object sender, KeyEventArgs e)
        {
            HandleBindingTextBoxKeyUp(PlayAudio, e, ref playAudioKey, ref pendingPlayAudioBinding);
        }
        private void PlayAudio_KeyPress(object sender, KeyPressEventArgs e)
        {
            // KeyDown handles binding capture, keep KeyPress from injecting text.
            e.Handled = true;

        }
        private void PlayAudio_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                && sender is Control control
                && !control.Focused)
            {
                return;
            }

            if (!KeyBindingService.TryGetSupportedMouseBinding(e.Button, Control.ModifierKeys, out Keys key, out string displayText))
            {
                return;
            }

            ApplyBindingCaptureToTextBox(PlayAudio, key, displayText, ref playAudioKey, ref pendingPlayAudioBinding);
        }
        private void info_ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (info_ListBox.Text)
            {
                case "NAudio":
                    info_Label5.Text =
                        "MM使用了NAudio音频处理库。\r\n" +
                        "NAudio遵循Microsoft Public License (Ms-PL)。\r\n" +
                        "版权所有 (c) [NAudio] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "特此向NAudio及其贡献者表示感谢。";
                    break;
                case "Newtonsoft.Json":
                    info_Label5.Text =
                        "MM使用了Newtonsoft.Json库。\r\n" +
                        "Newtonsoft.Json遵循MIT License。\r\n" +
                        "版权所有 (c) [Newtonsoft.Json] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/MIT\r\n" +
                        "特此向Newtonsoft.Json及其贡献者表示感谢。";
                    break;
                case "System.Management":
                    info_Label5.Text =
                        "MM使用了System.Management库。\r\n" +
                        "NAudio遵循MIT License。\r\n" +
                        "版权所有 (c) [.NET Foundation 和贡献者] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "特此向.NET社区及其贡献者表示感谢。";
                    break;
                case "taglib-sharp-netstandard2.0":
                    info_Label5.Text =
                        "MM使用了taglib-sharp-netstandard2.0库。\r\n" +
                        "taglib-sharp-netstandard2.0遵循LGPL-2.1 License\r\n" +
                        "版权所有 (c) [taglib-sharp] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/LGPL-2.1\r\n" +
                        "特此向taglib-sharp及其贡献者表示感谢。";
                    break;
                case "MediaToolkit":
                    info_Label5.Text =
                        "MM使用了MediaToolkit库。\r\n" +
                        "MediaToolkit暂无使用中的许可证\r\n" +
                        "版权所有 (c) [Aydin] \r\n" +
                        "特此向Aydin表示感谢。";
                    break;
                case "HtmlAgilityPack":
                    info_Label5.Text =
                        "HtmlAgilityPack。\r\n" +
                        "HtmlAgilityPack遵循 MIT License\r\n" +
                        "版权所有 (c) [zzzprojects] \r\n" +
                        "完整的许可证文本可在以下链接找到:\r\n" +
                        "https://opensource.org/licenses/MS-PL\r\n" +
                        "特此向zzzprojects及其贡献者表示感谢。";
                    break;
            }
        }
        private static WaveOutEvent currentOutputDevice = null;
        private static AudioFileReader currentAudioFile = null;
        public void PlayAudioex(string audioFilePath, int deviceNumber, float volume)
        {
            try
            {
                if (currentOutputDevice != null)
                {
                    currentOutputDevice.Stop();
                    currentOutputDevice.Dispose();
                    currentAudioFile?.Dispose();
                }
                var audioFile = new AudioFileReader(audioFilePath);
                var outputDevice = new WaveOutEvent { DeviceNumber = deviceNumber };
                // 应用音量设置
                outputDevice.Volume = volume; // 确保 volume 值在 0 到 1 之间
                outputDevice.PlaybackStopped += (sender, e) =>
                {
                    outputDevice.Dispose();
                    audioFile.Dispose();
                };
                outputDevice.Init(audioFile);
                outputDevice.Play();
                currentOutputDevice = outputDevice;
                currentAudioFile = audioFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放音频时出错: {ex.Message}", "错误");
            }
        }
        public void StopPlayback()
        {
            try
            {
                if (currentOutputDevice != null)
                {
                    currentOutputDevice.Stop(); // Stop Playback
                    currentOutputDevice.Dispose(); // 释放音频输出设备资源
                    currentOutputDevice = null; // 清除引用
                }
                if (currentAudioFile != null)
                {
                    currentAudioFile.Dispose(); // 释放音频文件资源
                    currentAudioFile = null; // 清除引用
                }

                MarkPlaybackStopped();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Stop Playback时出错: {ex.Message}", "错误");
            }
        }

        public async Task reLoadList()
        {
            await ReloadAudioLibraryAsync();
        }
        private async void reLoadAudioListsView_Click(object sender, EventArgs e)
        {
            await reLoadList();
        }
        private void aifadian_Click(object sender, EventArgs e)
        {
            string url = "https://afdian.net/a/MusicalMoments";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }
        private async void toVB_Click(object sender, EventArgs e)
        {
            if (vbOperationBusy)
            {
                MessageBox.Show("当前已有任务在执行，请稍候。", "提示");
                return;
            }
            if (!Misc.IsAdministrator())
            {
                MessageBox.Show("当前不是管理员运行，无法执行静默安装。\r\n请右键以管理员身份启动 MM 后重试。", "需要管理员权限");
                return;
            }

            DialogResult confirmResult = MessageBox.Show(
                "将执行：下载并静默安装 VB-CABLE。\r\n安装过程会在后台执行，请耐心等待。\r\n是否继续？",
                "确认安装",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirmResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SetVbOperationBusy(true, "正在下载并安装 VB-CABLE...");
                var progress = new Progress<string>(text => help_tip.Text = text);
                VbCableService.VbOperationResult result = await VbCableService.InstallAsync(
                    runningDirectory,
                    RestoreDefaultsAfterInstallEnabled,
                    progress);
                await RefreshVbHealthUiAsync(showDialog: false);
                MessageBox.Show(result.Message, result.Succeeded ? "安装完成" : "安装失败");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"安装 VB-CABLE 时出错：{ex.Message}", "错误");
            }
            finally
            {
                SetVbOperationBusy(false, null);
            }
        }
        private void toSettings_Click(object sender, EventArgs e)
        {
            mainTabControl.SelectedIndex = 2;
        }
        private async void mToAudioData_Click(object sender, EventArgs e)
        {
            if (vbOperationBusy)
            {
                MessageBox.Show("当前已有任务在执行，请稍候。", "提示");
                return;
            }
            if (!Misc.IsAdministrator())
            {
                MessageBox.Show("当前不是管理员运行，无法执行静默卸载。\r\n请右键以管理员身份启动 MM 后重试。", "需要管理员权限");
                return;
            }

            DialogResult confirmResult = MessageBox.Show(
                "将执行：静默卸载 VB-CABLE。\r\n是否继续？",
                "确认卸载",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirmResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SetVbOperationBusy(true, "正在卸载 VB-CABLE...");
                var progress = new Progress<string>(text => help_tip.Text = text);
                VbCableService.VbOperationResult result = await VbCableService.UninstallAsync(runningDirectory, progress);
                await RefreshVbHealthUiAsync(showDialog: false);
                MessageBox.Show(result.Message, result.Succeeded ? "卸载完成" : "卸载提示");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"卸载 VB-CABLE 时出错：{ex.Message}", "错误");
            }
            finally
            {
                SetVbOperationBusy(false, null);
            }
        }
        private void VBVolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            volume_Label1.Text = $"声卡麦({VBVolumeTrackBar.Value}%):";
            VBvolume = VBVolumeTrackBar.Value / 100f;
        }
        private void VolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            volume_Label2.Text = $"物理麦({VolumeTrackBar.Value}%):";
            volume = VolumeTrackBar.Value / 100f;
        }
        private void TipsVolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            volume_Label3.Text = $"提示音({TipsVolumeTrackBar.Value}%):";
            tipsvolume = TipsVolumeTrackBar.Value / 100f;
        }
        private static int logoClickCount = 0;
        private void LogoImage_Click(object sender, EventArgs e)
        {
            logoClickCount++;
            if (logoClickCount >= 10)
            {
                MessageBox.Show($"嗨~我是TheD0ubleC，这个软件的开发者。我们第一次见面是在<{firstStart}> 现在是<{System.DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒")}> 这期间你已经播放了<{playedCount}>次音频 还切换了<{changedCount}>次音频流！！(ps:如果数据不太对可能是因为你不小心把运行目录的json删除了吧？)", "恭喜你发现了彩蛋！");
            }
        }


        private void upData_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "音视频文件|*.mp3;*.wav;*.ogg;*.acc;*.ncm;*.qmc3;*.mp4;*.avi|全部文件|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFile = openFileDialog.FileName;
                dataPath_TextBox.Text = selectedFile;
                name_TextBox.Text = Path.GetFileNameWithoutExtension(selectedFile);

                conversion_Label4.Text = "源信息:" + Misc.DisplayAudioProperties(selectedFile);
            }
        }

        private void tabPage6_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string selectedFile = files[0];
                dataPath_TextBox.Text = selectedFile;
                name_TextBox.Text = Path.GetFileNameWithoutExtension(selectedFile);

                conversion_Label4.Text = "源信息:" + Misc.DisplayAudioProperties(selectedFile);
            }
        }

        private void tabPage6_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }
        private void convert_Button_Click(object sender, EventArgs e)
        {
            if (!File.Exists(dataPath_TextBox.Text))
            {
                MessageBox.Show("选择的文件不存在", "错误");
            }
            else
            {
                string extension = Path.GetExtension(dataPath_TextBox.Text).ToLower();
                if (extension == ".ncm")
                {
                    if (Misc.NCMConvert(dataPath_TextBox.Text, runningDirectory + "AudioData\\" + name_TextBox.Text + ".mp3") == 0)
                    {
                        conversion_Label5.Text = "转换后:" + Misc.DisplayAudioProperties(runningDirectory + "AudioData\\" + name_TextBox.Text + ".mp3");
                        MessageBox.Show("转换成功 已存储至运行目录下的AudioData文件夹", "提示");
                    }
                    else
                    {
                        MessageBox.Show("转换失败", "错误");
                    }
                }
                else if (extension == ".flac" || extension == ".ogg" || extension == ".mp3" || extension == ".wav")
                {
                    string targetFormat = comboBoxOutputFormat.SelectedItem.ToString().ToLower();
                    if (AudioConverter.ConvertTo(dataPath_TextBox.Text, runningDirectory + "AudioData\\" + name_TextBox.Text + "." + targetFormat, targetFormat))
                    {
                        conversion_Label5.Text = "转换后:" + Misc.DisplayAudioProperties(runningDirectory + "AudioData\\" + name_TextBox.Text + "." + targetFormat);
                        MessageBox.Show("转换成功 已存储至运行目录下的AudioData文件夹", "提示");
                    }
                    else
                    {
                        MessageBox.Show("转换失败", "错误");
                    }
                }
                else
                {
                    MessageBox.Show("不支持的文件格式", "错误");
                }
            }
        }
        private async void audioListView_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string audioDataPath = Path.Combine(runningDirectory, "AudioData");
            HashSet<string> supportedExtensions = new HashSet<string> { ".mp3", ".wav" };
            Directory.CreateDirectory(audioDataPath);
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (supportedExtensions.Contains(extension))
                {
                    string destFile = Path.Combine(audioDataPath, Path.GetFileName(file));
                    if (File.Exists(destFile))
                    {
                        var result = MessageBox.Show($"文件 {Path.GetFileName(file)} 已存在。是否覆盖？", "文件冲突", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result != DialogResult.Yes)
                        {
                            continue;
                        }
                    }
                    File.Copy(file, destFile, true);
                }
            }
            await reLoadList();
        }
        private void audioListView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void mToAudioData1_Click(object sender, EventArgs e)
        {
            string folderPath = Path.Combine(runningDirectory, "AudioData");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            Process.Start(new ProcessStartInfo()
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }


        public class AudioItem
        {
            public string Name { get; set; }
            public string DownloadLink { get; set; }

            public AudioItem(string name, string downloadLink)
            {
                Name = name;
                DownloadLink = downloadLink;
            }
        }

        List<ListViewItem> OriginalAudioItems = new List<ListViewItem>();
        private async void LoadList_Click(object sender, EventArgs e)
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download.json");
            string jsonUrl = "https://audio.scmd.cc/api/all-audio";
            string hashUrl = "https://audio.scmd.cc/api/all-audio-hash";

            try
            {
                bool useLocalJson = false;

                if (File.Exists(jsonFilePath))
                {
                    bool isHashMatching = await Misc.VerifyFileHashAsync(jsonFilePath, hashUrl);
                    if (isHashMatching)
                    {
                        Console.WriteLine("本地 JSON 文件哈希匹配，直接加载数据！");
                        useLocalJson = true;
                    }
                    else
                    {
                        Console.WriteLine("本地 JSON 文件已过期，重新下载！");
                    }
                }
                else
                {
                    Console.WriteLine("本地 JSON 文件不存在，开始下载！");
                }

                if (!useLocalJson)
                {
                    await Misc.DownloadJsonFile(jsonUrl, jsonFilePath);
                }


                AudioListView_fd.Items.Clear();
                DownloadLinkListBox.Items.Clear();


                await Misc.GetDownloadJsonFromFile(jsonFilePath, AudioListView_fd, DownloadLinkListBox);


                int? total = await Misc.GetTotalFromJsonFile(jsonFilePath);
                numberLabel.Text = total.HasValue ? $"{total.Value} 个项目" : $"{AudioListView_fd.Items.Count} 个项目";
                OriginalAudioItems.Clear();
                foreach (ListViewItem item in AudioListView_fd.Items)
                {
                    OriginalAudioItems.Add((ListViewItem)item.Clone());
                }

                if (!Debugger.IsAttached) { Misc.ButtonStabilization(5, LoadList); }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载列表时发生错误: {ex.Message}");
            }
        }





        private void SearchBarTextBox_Enter(object sender, EventArgs e)
        {
            if (SearchBarTextBox.Text == "搜索")
            {
                SearchBarTextBox.Text = "";
            }
        }

        private void SearchBarTextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBarTextBox.Text))
            {
                SearchBarTextBox.Text = "搜索";
            }
        }

        private void SearchBarTextBox_TextChanged(object sender, EventArgs e)
        {
            string keyword = SearchBarTextBox.Text.Trim().ToLower();

            if (OriginalAudioItems == null || OriginalAudioItems.Count == 0)
                return;

            AudioListView_fd.BeginUpdate();
            AudioListView_fd.Items.Clear();

            IEnumerable<ListViewItem> filteredItems;

            if (string.IsNullOrWhiteSpace(keyword) || keyword == "搜索")
            {
                filteredItems = OriginalAudioItems;
            }
            else
            {
                filteredItems = OriginalAudioItems.Where(item =>
                    (item.SubItems.Count > 0 && item.SubItems[0].Text.ToLower().Contains(keyword)) || // 名称列
                    (item.SubItems.Count > 1 && item.SubItems[1].Text.ToLower().Contains(keyword)) || // 下载次数列
                    (item.SubItems.Count > 2 && item.SubItems[2].Text.ToLower().Contains(keyword))    // 上传者列
                );
            }

            foreach (var item in filteredItems)
            {
                AudioListView_fd.Items.Add((ListViewItem)item.Clone());
            }

            AudioListView_fd.EndUpdate();

            numberLabel.Text = $"{AudioListView_fd.Items.Count} 个项目";
        }


        private async void DownloadSelected_Click(object sender, EventArgs e)
        {
            if (AudioListView_fd.SelectedIndices.Count == 0)
            {
                MessageBox.Show("请选择要下载的音频。", "提示");
                return;
            }

            int selectedIndex = AudioListView_fd.SelectedIndices[0];
            if (selectedIndex >= DownloadLinkListBox.Items.Count)
            {
                MessageBox.Show("未找到对应的下载链接。", "错误");
                return;
            }

            string downloadLink = DownloadLinkListBox.Items[selectedIndex].ToString();

            string rawFileName = Path.GetFileName(new Uri(downloadLink).AbsolutePath);

            string decodedFileName = Uri.UnescapeDataString(rawFileName);
            string saveDirectory = Path.Combine(Application.StartupPath, "AudioData");
            string savePath = Path.Combine(saveDirectory, decodedFileName);

            try
            {
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                using (HttpClient client = new HttpClient())
                {
                    byte[] fileBytes = await client.GetByteArrayAsync(downloadLink);
                    await File.WriteAllBytesAsync(savePath, fileBytes);
                }

                MessageBox.Show($"已成功下载到音频文件夹：{decodedFileName}\n保存路径：{savePath}", "下载完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"下载失败：{ex.Message}", "错误");
            }
        }





        bool pluginServer = false;




        private void toC_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:sound",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开系统声音设置失败：{ex.Message}", "错误");
            }
        }

        public static Keys nowKey = Keys.None;













    }
}


