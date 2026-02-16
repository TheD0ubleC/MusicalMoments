using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;

namespace MusicalMoments
{
    public partial class MainWindow
    {
        private bool vbOperationBusy;

        private bool RestoreDefaultsAfterInstallEnabled => restoreDefaultsAfterInstallCheckBox?.Checked ?? true;

        private async void to_audio_page_Click(object sender, EventArgs e)
        {
            if (vbOperationBusy)
            {
                MessageBox.Show("当前已有任务在执行，请稍候。", "提示");
                return;
            }

            try
            {
                SetVbOperationBusy(true, "正在修复 VB 格式...");
                VbCableService.VbOperationResult result = await Task.Run(VbCableService.TryRepairFormat);
                await RefreshVbHealthUiAsync(showDialog: false);
                MessageBox.Show(result.Message, result.Succeeded ? "修复完成" : "修复失败");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行修复时出错：{ex.Message}", "错误");
            }
            finally
            {
                SetVbOperationBusy(false, null);
            }
        }

        private async void autoSelectDevicesButton_Click(object sender, EventArgs e)
        {
            if (vbOperationBusy)
            {
                MessageBox.Show("当前已有任务在执行，请稍候。", "提示");
                return;
            }

            try
            {
                SetVbOperationBusy(true, "正在自动识别设备...");
                int selectedCount = 0;
                selectedCount += TrySelectSystemDefaultDevice(comboBox_AudioEquipmentOutput, DataFlow.Render) ? 1 : 0;
                selectedCount += TrySelectSystemDefaultDevice(comboBox_AudioEquipmentInput, DataFlow.Capture) ? 1 : 0;
                selectedCount += TrySelectByKeywords(comboBox_VBAudioEquipmentOutput, new[] { "CABLE Input", "VB-Audio Virtual Cable" }) ? 1 : 0;
                selectedCount += TrySelectByKeywords(comboBox_VBAudioEquipmentInput, new[] { "CABLE Output", "VB-Audio Virtual Cable" }) ? 1 : 0;
                SaveUserData();
                MessageBox.Show(
                    $"自动识别完成，已设置 {selectedCount}/4 项设备。\r\n请检查设备是否正确：\r\n- 物理扬声器/麦克风应为你的真实设备\r\n- VB 输入/输出应为 CABLE 设备",
                    "提示");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"自动识别设备时出错：{ex.Message}", "错误");
            }
            finally
            {
                SetVbOperationBusy(false, null);
            }
        }

        private bool TrySelectSystemDefaultDevice(ComboBox comboBox, DataFlow flow)
        {
            using MMDevice defaultDevice = WindowsAudioEndpointService.FindDefaultDevice(flow);
            string defaultName = WindowsAudioEndpointService.GetFriendlyNameSafe(defaultDevice);
            if (string.IsNullOrWhiteSpace(defaultName))
            {
                return false;
            }

            List<string> keywords = new List<string> { defaultName };
            int leftBracket = defaultName.IndexOf('(');
            int rightBracket = defaultName.LastIndexOf(')');
            if (leftBracket >= 0 && rightBracket > leftBracket)
            {
                string bracketName = defaultName.Substring(leftBracket + 1, rightBracket - leftBracket - 1).Trim();
                if (!string.IsNullOrWhiteSpace(bracketName))
                {
                    keywords.Add(bracketName);
                }
            }

            string normalized = defaultName
                .Replace("扬声器", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("麦克风", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("耳机", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Trim();
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                keywords.Add(normalized);
            }

            return TrySelectByKeywords(comboBox, keywords);
        }

        private static bool TrySelectByKeywords(ComboBox comboBox, IEnumerable<string> keywords)
        {
            if (comboBox == null || comboBox.Items.Count == 0)
            {
                return false;
            }

            List<string> validKeywords = keywords
                .Where(keyword => !string.IsNullOrWhiteSpace(keyword))
                .ToList();
            if (validKeywords.Count == 0)
            {
                return false;
            }

            for (int index = 0; index < comboBox.Items.Count; index++)
            {
                string itemText = comboBox.Items[index]?.ToString() ?? string.Empty;
                if (validKeywords.Any(keyword =>
                        itemText.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                        || keyword.Contains(itemText, StringComparison.OrdinalIgnoreCase)
                        || NormalizeDeviceText(itemText).Contains(NormalizeDeviceText(keyword), StringComparison.OrdinalIgnoreCase)
                        || NormalizeDeviceText(keyword).Contains(NormalizeDeviceText(itemText), StringComparison.OrdinalIgnoreCase)))
                {
                    comboBox.SelectedIndex = index;
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeDeviceText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text
                .Replace("扬声器", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("麦克风", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("耳机", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace(" ", string.Empty)
                .Trim();
        }

        private async Task RefreshVbHealthUiAsync(bool showDialog)
        {
            VbCableService.VbHealthReport report = await Task.Run(VbCableService.BuildHealthReport);
            label_VBStatus.Text = report.IsInstalled ? "VB 状态：已安装" : "VB 状态：未安装";
            if (report.IsHealthy)
            {
                help_tip.ForeColor = System.Drawing.Color.FromArgb(34, 139, 34);
                help_tip.Text = "状态：VB 正常（输入输出均为 16位/48k/2声道）";
            }
            else
            {
                help_tip.ForeColor = System.Drawing.Color.FromArgb(178, 34, 34);
                help_tip.Text = report.IsInstalled
                    ? "状态：VB 已安装，但格式未达标（请点“一键修复”）"
                    : "状态：VB 未安装（请先执行步骤1安装）";
            }

            if (showDialog)
            {
                MessageBox.Show(report.DetailMessage, "VB 检查结果");
            }
        }

        private void SetVbOperationBusy(bool busy, string statusText)
        {
            vbOperationBusy = busy;
            UseWaitCursor = busy;
            Cursor.Current = busy ? Cursors.WaitCursor : Cursors.Default;

            if (!string.IsNullOrWhiteSpace(statusText))
            {
                help_tip.Text = statusText;
            }

            foreach (Control control in new Control[]
                     {
                         toVB, retestVB, to_audio_page, mToAudioData, toSettings, toC, open_help_window,
                         check_update, autoSelectDevicesButton, sideLists, mainTabControl
                     })
            {
                if (control != null)
                {
                    control.Enabled = !busy;
                }
            }
        }
    }
}
