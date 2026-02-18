using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicalMoments
{
    public partial class MainWindow
    {
        private NotifyIcon trayNotifyIcon;
        private ContextMenuStrip trayContextMenu;
        private ToolStripMenuItem trayMemoryMenuItem;
        private ToolStripMenuItem trayModeMenuItem;
        private ToolStripMenuItem trayPluginMenuItem;
        private ToolStripMenuItem trayShowMenuItem;
        private ToolStripMenuItem trayExitMenuItem;
        private System.Windows.Forms.Timer trayStatusTimer;
        private bool trayTipShown;
        private bool trayHidden;
        private bool trayTransitionBusy;

        private enum CloseDecisionAction
        {
            Cancel = 0,
            MinimizeToTray = 1,
            ExitDirectly = 2
        }

        private readonly struct CloseDecision
        {
            public CloseDecision(CloseDecisionAction action, bool rememberChoice)
            {
                Action = action;
                RememberChoice = rememberChoice;
            }

            public CloseDecisionAction Action { get; }
            public bool RememberChoice { get; }
        }

        private void InitializeTrayFeatures()
        {
            if (trayNotifyIcon != null)
            {
                return;
            }

            trayContextMenu = new ContextMenuStrip();
            trayContextMenu.Opening += (_, _) => UpdateTrayStatusUi();

            trayMemoryMenuItem = new ToolStripMenuItem("\u5185\u5B58\u5360\u7528\uFF1A\u8BFB\u53D6\u4E2D...")
            {
                Enabled = false
            };
            trayModeMenuItem = new ToolStripMenuItem("\u5F53\u524D\u6A21\u5F0F\uFF1A\u97F3\u9891\u6A21\u5F0F (\u70B9\u51FB\u5207\u6362)");
            trayModeMenuItem.Click += (_, _) =>
            {
                TogglePlaybackMode(playSwitchTip: switchStreamTips?.Checked ?? false);
                UpdateTrayStatusUi();
            };
            trayPluginMenuItem = new ToolStripMenuItem("\u63D2\u4EF6\u670D\u52A1\uFF1A\u672A\u5F00\u542F")
            {
                Enabled = false
            };
            trayShowMenuItem = new ToolStripMenuItem("\u663E\u793A\u4E3B\u7A97\u53E3");
            trayShowMenuItem.Click += (_, _) => RestoreFromTray();
            trayExitMenuItem = new ToolStripMenuItem("\u9000\u51FA");
            trayExitMenuItem.Click += (_, _) =>
            {
                allowExitWithoutPrompt = true;
                trayNotifyIcon.Visible = false;
                Close();
            };

            trayContextMenu.Items.Add(trayMemoryMenuItem);
            trayContextMenu.Items.Add(trayModeMenuItem);
            trayContextMenu.Items.Add(trayPluginMenuItem);
            trayContextMenu.Items.Add(new ToolStripSeparator());
            trayContextMenu.Items.Add(trayShowMenuItem);
            trayContextMenu.Items.Add(trayExitMenuItem);

            trayNotifyIcon = new NotifyIcon(components)
            {
                Text = "MusicalMoments",
                Icon = Icon ?? SystemIcons.Application,
                Visible = false,
                ContextMenuStrip = trayContextMenu
            };
            trayNotifyIcon.DoubleClick += (_, _) => RestoreFromTray();

            trayStatusTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            trayStatusTimer.Tick += (_, _) => UpdateTrayStatusUi();
        }

        private async void HideToTray()
        {
            if (IsDisposed || trayTransitionBusy || trayHidden)
            {
                return;
            }

            trayTransitionBusy = true;
            try
            {
                InitializeTrayFeatures();
                UpdateTrayStatusUi();
                trayNotifyIcon.Visible = true;
                trayStatusTimer?.Start();

                ShowInTaskbar = true;
                await UiEffectsService.FadeHideWithoutDispose(140, this);
                ShowInTaskbar = false;
                trayHidden = true;

                if (!trayTipShown)
                {
                    trayTipShown = true;
                    trayNotifyIcon.ShowBalloonTip(
                        1200,
                        "MusicalMoments",
                        "\u7A0B\u5E8F\u5DF2\u6700\u5C0F\u5316\u5230\u6258\u76D8\uFF0C\u53CC\u51FB\u6258\u76D8\u56FE\u6807\u53EF\u6062\u590D\u3002",
                        ToolTipIcon.Info);
                }
            }
            finally
            {
                trayTransitionBusy = false;
            }
        }

        private async void RestoreFromTray()
        {
            if (IsDisposed || trayTransitionBusy || !trayHidden)
            {
                return;
            }

            trayTransitionBusy = true;
            try
            {
                ShowInTaskbar = true;
                WindowState = FormWindowState.Normal;
                await UiEffectsService.FadeShowWithoutFlash(140, this);
                Activate();
                trayHidden = false;

                if (trayNotifyIcon != null)
                {
                    trayNotifyIcon.Visible = false;
                }

                trayStatusTimer?.Stop();
                ApplyResponsiveLayoutNow();
                SyncSideNavigationSelection(force: true);
            }
            finally
            {
                trayTransitionBusy = false;
            }
        }

        private void UpdateTrayStatusUi()
        {
            if (trayContextMenu == null || trayNotifyIcon == null)
            {
                return;
            }

            AudioMemorySnapshot memory = AudioPlaybackService.GetMemorySnapshot();
            double processMb = (memory.PrivateWorkingSetBytes > 0
                    ? memory.PrivateWorkingSetBytes
                    : memory.ProcessWorkingSetBytes)
                / 1024d / 1024d;

            trayMemoryMenuItem.Text = $"\u5185\u5B58\u5360\u7528\uFF1A{processMb:0.00} MB";
            trayModeMenuItem.Text =
                $"\u5F53\u524D\u6A21\u5F0F\uFF1A{(playAudio ? "\u97F3\u9891\u6A21\u5F0F" : "\u9EA6\u514B\u98CE\u6A21\u5F0F")} (\u70B9\u51FB\u5207\u6362)";

            bool pluginOn = pluginServer && PluginSDK.PluginServer.IsRunning;
            trayPluginMenuItem.Text = $"\u63D2\u4EF6\u670D\u52A1\uFF1A{(pluginOn ? "\u5DF2\u5F00\u542F" : "\u672A\u5F00\u542F")}";
        }

        private void DisposeTrayFeatures()
        {
            trayTransitionBusy = false;
            trayHidden = false;

            if (trayStatusTimer != null)
            {
                trayStatusTimer.Stop();
                trayStatusTimer.Dispose();
                trayStatusTimer = null;
            }

            if (trayNotifyIcon != null)
            {
                trayNotifyIcon.Visible = false;
                trayNotifyIcon.Dispose();
                trayNotifyIcon = null;
            }

            if (trayContextMenu != null)
            {
                trayContextMenu.Dispose();
                trayContextMenu = null;
            }

            trayMemoryMenuItem = null;
            trayModeMenuItem = null;
            trayPluginMenuItem = null;
            trayShowMenuItem = null;
            trayExitMenuItem = null;
        }

        private CloseDecision ShowCloseDecisionDialog()
        {
            using Form dialog = new Form
            {
                Text = "\u5173\u95ED MusicalMoments",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(410, 175),
                Font = Font
            };

            Label titleLabel = new Label
            {
                Left = 14,
                Top = 12,
                Width = 382,
                Height = 46,
                Text = "\u4F60\u5E0C\u671B\u672C\u6B21\u70B9\u51FB\u5173\u95ED\u6309\u94AE\u65F6\u5982\u4F55\u5904\u7406\uFF1F\r\n\u53EF\u9009\u62E9\u9690\u85CF\u5230\u6258\u76D8\u6216\u76F4\u63A5\u9000\u51FA\u7A0B\u5E8F\u3002",
                RightToLeft = RightToLeft.No
            };

            CheckBox rememberCheckBox = new CheckBox
            {
                Left = 14,
                Top = 66,
                Width = 382,
                Text = "\u8BB0\u4F4F\u672C\u6B21\u9009\u62E9\uFF0C\u4EE5\u540E\u4E0D\u518D\u8BE2\u95EE",
                RightToLeft = RightToLeft.No
            };

            Button toTrayButton = new Button
            {
                Left = 14,
                Top = 110,
                Width = 118,
                Height = 30,
                Text = "\u9690\u85CF\u5230\u6258\u76D8",
                DialogResult = DialogResult.Yes
            };

            Button exitButton = new Button
            {
                Left = 138,
                Top = 110,
                Width = 118,
                Height = 30,
                Text = "\u76F4\u63A5\u5173\u95ED",
                DialogResult = DialogResult.No
            };

            Button cancelButton = new Button
            {
                Left = 278,
                Top = 110,
                Width = 118,
                Height = 30,
                Text = "\u53D6\u6D88",
                DialogResult = DialogResult.Cancel
            };

            dialog.Controls.Add(titleLabel);
            dialog.Controls.Add(rememberCheckBox);
            dialog.Controls.Add(toTrayButton);
            dialog.Controls.Add(exitButton);
            dialog.Controls.Add(cancelButton);
            dialog.AcceptButton = toTrayButton;
            dialog.CancelButton = cancelButton;

            DialogResult result = dialog.ShowDialog(this);
            return result switch
            {
                DialogResult.Yes => new CloseDecision(CloseDecisionAction.MinimizeToTray, rememberCheckBox.Checked),
                DialogResult.No => new CloseDecision(CloseDecisionAction.ExitDirectly, rememberCheckBox.Checked),
                _ => new CloseDecision(CloseDecisionAction.Cancel, false)
            };
        }
    }
}
