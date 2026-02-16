using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MusicalMoments
{
    public partial class MainWindow
    {
        
                private void open_help_window_Click(object sender, EventArgs e)
                {
                    const string tutorialUrl = "https://example.com/mm-guide";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tutorialUrl,
                        UseShellExecute = true
                    });
                }
        
                private void open_help_button2_Click(object sender, EventArgs e)
                {
                    mainTabControl.SelectedTab = tabPage3;
                    if (playbackBehaviorGroup != null)
                    {
                        playbackBehaviorGroup.Focus();
                    }
                }
        
                private void check_update_Click(object sender, EventArgs e)
                {
                    var ori_text = check_update.Text;
                    check_update.Text = "检查中...";
                    CheckNewVer(true);
                    check_update.Text = ori_text;
                }
        
                private void open_help_button1_Click(object sender, EventArgs e)
                {
                    Form help_window = new HelpWindow();
                    help_window.Show();
                }
    }
}
