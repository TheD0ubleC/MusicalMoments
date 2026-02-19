namespace MusicalMoments
{
    partial class HelpWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            button1 = new Button();
            r_help = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft JhengHei UI", 12F);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(541, 20);
            label1.TabIndex = 0;
            label1.Text = "安装VB后系统声音消失解决方法 - 前往设置将音频设备还原成你的物理设备\r\n";
            // 
            // button1
            // 
            button1.Font = new Font("Microsoft JhengHei UI", 9F);
            button1.Location = new Point(559, 5);
            button1.Name = "button1";
            button1.Size = new Size(75, 28);
            button1.TabIndex = 1;
            button1.Text = "前往设置";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // r_help
            // 
            r_help.Location = new Point(597, 336);
            r_help.Name = "r_help";
            r_help.Size = new Size(42, 41);
            r_help.TabIndex = 2;
            r_help.Text = "刷新";
            r_help.UseVisualStyleBackColor = false;
            r_help.Click += r_help_Click;
            // 
            // HelpWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(651, 389);
            Controls.Add(r_help);
            Controls.Add(button1);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "HelpWindow";
            Text = "帮助";
            Load += HelpWindow_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button button1;
        private Button r_help;
    }
}
