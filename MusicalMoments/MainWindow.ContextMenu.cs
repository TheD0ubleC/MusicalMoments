using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace MusicalMoments
{
    public partial class MainWindow
    {

        private void PlaySelectedMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selectedItem = audioListView.SelectedItems[0];
            string filePath = selectedItem.Tag as string;
            try
            {
                PlayAudioex(filePath, Misc.GetOutputDeviceID(comboBox_AudioEquipmentOutput.SelectedItem.ToString()), volume);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放音频时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            playedCount = playedCount + 1;
        }

        private async void DeleteSelectedMenuItem_Click(object sender, EventArgs e)
        {
            if (audioListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = audioListView.SelectedItems[0];
                string filePath = selectedItem.Tag as string;
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    DialogResult dialogResult = MessageBox.Show("确定要删除这个文件吗？", "删除文件", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        try
                        {
                            File.Delete(filePath); // 删除文件
                            await reLoadList();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"删除文件时出错: {ex.Message}");
                        }
                    }
                }
            }
        }

        private async void RenameSelectedMenuItem_Click(object sender, EventArgs e)
        {
            if (audioListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = audioListView.SelectedItems[0];
                string originalFilePath = selectedItem.Tag as string;
                if (originalFilePath == null) return;
                string directoryPath = Path.GetDirectoryName(originalFilePath);
                string originalFileName = Path.GetFileName(originalFilePath);
                string currentName = selectedItem.Text;
                string extension = Path.GetExtension(originalFilePath);
                string input = Interaction.InputBox("请输入新的名称：", "重命名", currentName, -1, -1);
                if (string.IsNullOrEmpty(input) || input.Equals(currentName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                string newFileName = input + extension;
                string newFilePath = Path.Combine(directoryPath, newFileName);
                // 检查新命名的文件是否已存在
                if (File.Exists(newFilePath))
                {
                    MessageBox.Show("命名重复", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    try
                    {
                        File.Move(originalFilePath, newFilePath);
                        await reLoadList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"重命名文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SetAsPlaybackItemMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selectedItem = audioListView.SelectedItems[0];
            string filePath = selectedItem.Tag as string;
            if (TrySetSelectedAudio(filePath))
            {
                SelectedAudioLabel.Text = $"已选择:{selectedItem.Text}";
            }
        }

        private async void OpenFileLocationMenuItem_Click(object sender, EventArgs e)
        {
            if (audioListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = audioListView.SelectedItems[0];
                string filePath = selectedItem.Tag as string;
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    string argument = "/select, \"" + filePath + "\"";
                    Process.Start("explorer.exe", argument);
                }
                else
                {
                    await reLoadList();
                }
            }
        }

        private void StopPlaybackMenuItem_Click(object sender, EventArgs e)
        {
            StopPlayback();
        }

        private void BindKeyMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selectedItem = audioListView.SelectedItems[0];
            Keys key = Keys.None;
            string filePath = selectedItem.Tag as string;
            AudioInfo existingAudioInfo = audioInfo.FirstOrDefault(item => string.Equals(item.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            if (existingAudioInfo != null)
            {
                key = KeyBindingService.NormalizeBinding(existingAudioInfo.Key);
            }
            else if (selectedItem.SubItems[3].Text != "未绑定"
                     && KeyBindingService.TryParseBinding(selectedItem.SubItems[3].Text, out Keys parsedKey))
            {
                key = parsedKey;
            }

            BindKeyWindow bindKeyWindow = new BindKeyWindow(key);
            bindKeyWindow.ShowDialog();
            nowKey = KeyBindingService.NormalizeBinding(bindKeyWindow.Key);

            Misc.WriteKeyJsonInfo(Path.ChangeExtension(selectedItem.Tag.ToString(), ".json"), nowKey.ToString());
            selectedItem.SubItems[3].Text = nowKey == Keys.None ? "未绑定" : KeyBindingService.GetDisplayTextForKey(nowKey);
            if (existingAudioInfo != null)
            {
                existingAudioInfo.Key = nowKey;
            }
            else
            {
                audioInfo.Add(new AudioInfo
                {
                    Name = selectedItem.SubItems[0].Text,
                    FilePath = filePath,
                    Key = nowKey
                });
            }

            RebuildHotkeyAudioIndex();
            if (CheckDuplicateKeys()) { MessageBox.Show($"已检测到相同按键 请勿作死将两个或多个音频绑定在同个按键上 该操作可能会导致MM崩溃 此提示会在Bind Key时与软件启动时检测并发出", "温馨提示"); }
        }

        public static bool CheckDuplicateKeys()
        {
            for (int i = 0; i < audioInfo.Count; i++)
            {
                var parentItem = audioInfo[i];
                Keys parentKey = KeyBindingService.NormalizeBinding(parentItem.Key);
                if (parentKey == Keys.None)
                    continue;

                for (int j = i + 1; j < audioInfo.Count; j++)
                {
                    var childItem = audioInfo[j];
                    Keys childKey = KeyBindingService.NormalizeBinding(childItem.Key);
                    if (childKey == Keys.None)
                        continue;

                    if (parentKey == childKey)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
