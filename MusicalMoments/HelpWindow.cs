using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MusicalMoments
{
    public partial class HelpWindow : Form
    {
        public HelpWindow()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = Size;

            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            r_help.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            this.Load += HelpWindow_Load;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // 打开系统设置中的声音设置  
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ms-settings:sound",
                UseShellExecute = true
            });
        }
        private async void HelpWindow_Load(object sender, EventArgs e)
        {
            LoadJsonData();
        }
        private List<Control> dynamicControls = new List<Control>();

        private async void LoadJsonData(bool removeAddedControls = false)
        {
            string url = "http://scmd.cc/data.json";

            try
            {
                if (removeAddedControls)
                {
                    foreach (var control in dynamicControls)
                    {
                        this.Controls.Remove(control);
                        control.Dispose();
                    }
                    dynamicControls.Clear();
                }

                using HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                string json = await client.GetStringAsync(url);

                var components = JsonSerializer.Deserialize<List<Component>>(json);
                if (components != null)
                {
                    foreach (var component in components)
                    {
                        Control control = CreateControlFromJson(component);
                        if (control != null)
                        {
                            this.Controls.Add(control);
                            dynamicControls.Add(control);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                MessageBox.Show($"加载组件时出错: {ex.Message}");
            }
        }

        private Control CreateControlFromJson(Component component)
        {

            Control control = component.Type switch
            {
                "Button" => new Button(),
                "Label" => new Label(),
                "TextBox" => new TextBox(),
                "CheckBox" => new CheckBox(),
                _ => null
            };

            if (control == null) return null;

            // 2. 提前取出 ClickScript，避免被当成普通属性处理
            string clickScript = null;
            if (component.RawProperties != null && component.RawProperties.TryGetValue("ClickScript", out var val))
            {
                clickScript = val.GetString();
                component.RawProperties.Remove("ClickScript");
            }


            // 3. 通过反射设置控件的常规属性
            foreach (var prop in component.RawProperties)
            {
                try
                {
                    var property = control.GetType().GetProperty(prop.Key);
                    if (property != null && property.CanWrite)
                    {
                        object value = ConvertToPropertyValue(property.PropertyType, prop.Value);
                        property.SetValue(control, value);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"属性 {prop.Key} 赋值失败: {ex.Message}");
                }
            }

            // 4. 如果存在 ClickScript 且是 Button，绑定事件
            if (!string.IsNullOrEmpty(clickScript) && control is Button btn)
            {
                btn.Click += (s, e) => MiniDsl.Execute(clickScript);

            }

            return control;
        }



        private object ConvertToPropertyValue(Type targetType, JsonElement json)
        {
            try
            {
                if (targetType == typeof(string))
                    return json.GetString();
                if (targetType == typeof(int))
                    return json.GetInt32();
                if (targetType == typeof(bool))
                    return json.GetBoolean();
                if (targetType == typeof(Point))
                {
                    var arr = json.EnumerateArray().ToArray();
                    return new Point(arr[0].GetInt32(), arr[1].GetInt32());
                }
                if (targetType == typeof(Size))
                {
                    var arr = json.EnumerateArray().ToArray();
                    return new Size(arr[0].GetInt32(), arr[1].GetInt32());
                }
                if (targetType == typeof(Color))
                    return ColorTranslator.FromHtml(json.GetString());

                // 支持更多类型请在此扩展
            }
            catch
            {
                // 忽略转换失败的字段
            }

            return null;
        }
        private string EscapePowerShell(string script)
        {
            return script.Replace("\"", "`\"");  // 转义双引号
        }

        public static void ExecutePowerShell(string script)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "powershell";
                    process.StartInfo.Arguments = $"-Command \"{script}\"";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[动态执行错误]: {ex.Message}");
            }
        }



        public class Component
        {
            public string Type { get; set; }

            [JsonExtensionData]
            public Dictionary<string, JsonElement> RawProperties { get; set; }


        }

        private void r_help_Click(object sender, EventArgs e)
        {
            LoadJsonData(true);
        }
    }
}
