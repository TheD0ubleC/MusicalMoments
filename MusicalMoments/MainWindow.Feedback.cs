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
        
                private void FeedbackTipsButton_Click(object sender, EventArgs e)
                {
                    MessageBox.Show("请留下您的问题与您的联系方式 如电子邮箱、QQ等 收到反馈后会在72小时内回复您\r\n但请注意 切勿滥用", "提示");
                }
        
                private void FeedbackButton_Click(object sender, EventArgs e)
                {
                    string host = "smtphz.qiye.163.com";
                    int port = 25;
                    string from = "feedback@scmd.cc";
                    string to = "feedback@scmd.cc";
                    MailMessage message = new MailMessage(from, to);
                    string level = "普通";
                    if (FeedbackAverage.Checked) { level = "普通"; }
                    if (FeedbackUrgent.Checked) { level = "紧急"; }
                    if (FeedbackDisaster.Checked) { level = "灾难"; }
        
                    message.Subject = $"{level} - {FeedbackTitle.Text}";
                    message.IsBodyHtml = true;
                    //我有强迫症 看不惯难看的默认样式 然后特地为这个写了个很好看很好看的样式(★w★）
                    string htmlBody = $@"
            <html>
                <head>
                    <style>
                        body {{
                            font-family: 'Arial', sans-serif;
                            margin: 0;
                            padding: 0;
                            background-color: #f7f7f7;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background: white;
                            border-radius: 8px;
                            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
                            background-image: linear-gradient(to bottom right, #FFD3A5, #FD6585);
                            overflow: hidden;
                        }}
                        .header {{
                            background: #FFF;
                            padding: 20px;
                            text-align: center;
                            border-top-left-radius: 8px;
                            border-top-right-radius: 8px;
                            border-bottom: 1px solid #eee;
                        }}
                        .body {{
                            padding: 20px;
                            background: #FFF;
                            color: #333;
                        }}
                        .footer {{
                            background: #FFF;
                            padding: 20px;
                            text-align: center;
                            border-bottom-left-radius: 8px;
                            border-bottom-right-radius: 8px;
                            border-top: 1px solid #eee;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1 style='color: #FD6585;'>{level} - {FeedbackTitle.Text}</h1>
                        </div>
                        <div class='body'>
                            <p>{FeedbackContent.Text.Replace("\n", "<br>")}</p>
                        </div>
                        <div class='footer'>
                            <p>联系方式:</strong> {Contact.Text}</p>
                        </div>
                    </div>
                </body>
            </html>";
        
                    message.Body = htmlBody;
                    if (string.IsNullOrWhiteSpace(FeedbackTitle.Text) || string.IsNullOrWhiteSpace(FeedbackContent.Text))
                    {
                        MessageBox.Show("标题和内容不能为空，请填写后再提交。", "错误");
                        return;
                    }
        
                    if (!IsValidContent(FeedbackContent.Text))
                    {
                        MessageBox.Show("请输入有意义的内容，避免乱码或无意义字符。", "错误");
                        return;
                    }
        
                    SmtpClient client = new SmtpClient(host, port);
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("feedback@scmd.cc", "SCMDfb2023");
                    try
                    {
                        client.Send(message);
                        MessageBox.Show("反馈发送成功！", "谢谢你");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("邮件发送失败：" + ex.Message, "抱歉");
                    }
                }
        
                private bool IsValidContent(string content)
                {
                    int chineseCount = content.Count(c => c >= 0x4E00 && c <= 0x9FA5);
                    if (chineseCount < content.Length * 0.3)
                        return false;
                    if (content.Length < 10)
                        return false;
                    const int maxRepetitions = 3;
                    char lastChar = '\0';
                    int currentRepetition = 1;
        
                    foreach (char c in content)
                    {
                        if (c == lastChar)
                        {
                            currentRepetition++;
                            if (currentRepetition > maxRepetitions)
                                return true;
                        }
                        else
                        {
                            lastChar = c;
                            currentRepetition = 1;
                        }
                    }
                    return true;
                }
    }
}
