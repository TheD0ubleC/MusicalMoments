namespace MusicalMoments.KeyHelp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            bindKeyLabel = new Label();
            bindKeyTextBox = new TextBox();
            tipLabel = new Label();
            mmKeyLabel = new Label();
            statusLabel = new Label();
            modeGroupBox = new GroupBox();
            holdModeRadio = new RadioButton();
            toggleModeRadio = new RadioButton();
            pollTimer = new System.Windows.Forms.Timer(components);
            modeGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // bindKeyLabel
            // 
            bindKeyLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            bindKeyLabel.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            bindKeyLabel.Location = new Point(20, 24);
            bindKeyLabel.Name = "bindKeyLabel";
            bindKeyLabel.Size = new Size(760, 24);
            bindKeyLabel.TabIndex = 0;
            bindKeyLabel.Text = "游戏按键（按下 ESC 清除）：";
            // 
            // bindKeyTextBox
            // 
            bindKeyTextBox.BorderStyle = BorderStyle.FixedSingle;
            bindKeyTextBox.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            bindKeyTextBox.ForeColor = Color.FromArgb(90, 90, 90);
            bindKeyTextBox.ImeMode = ImeMode.Disable;
            bindKeyTextBox.Location = new Point(20, 56);
            bindKeyTextBox.Name = "bindKeyTextBox";
            bindKeyTextBox.RightToLeft = RightToLeft.No;
            bindKeyTextBox.Size = new Size(260, 29);
            bindKeyTextBox.TabIndex = 1;
            bindKeyTextBox.Text = "Key";
            bindKeyTextBox.TextAlign = HorizontalAlignment.Center;
            bindKeyTextBox.KeyDown += bindKeyTextBox_KeyDown;
            bindKeyTextBox.KeyPress += bindKeyTextBox_KeyPress;
            bindKeyTextBox.MouseDown += bindKeyTextBox_MouseDown;
            // 
            // tipLabel
            // 
            tipLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tipLabel.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            tipLabel.Location = new Point(20, 98);
            tipLabel.Name = "tipLabel";
            tipLabel.Size = new Size(760, 22);
            tipLabel.TabIndex = 2;
            tipLabel.Text = "规则：支持键盘、中键、侧键。左键/右键/滚轮不支持，按 ESC 可清除绑定。";
            // 
            // mmKeyLabel
            // 
            mmKeyLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mmKeyLabel.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            mmKeyLabel.Location = new Point(20, 130);
            mmKeyLabel.Name = "mmKeyLabel";
            mmKeyLabel.Size = new Size(760, 22);
            mmKeyLabel.TabIndex = 3;
            mmKeyLabel.Text = "MM 播放按键：未获取";
            // 
            // statusLabel
            // 
            statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            statusLabel.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            statusLabel.Location = new Point(20, 460);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(760, 22);
            statusLabel.TabIndex = 4;
            statusLabel.Text = "状态：初始化中";
            // 
            // modeGroupBox
            // 
            modeGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            modeGroupBox.Controls.Add(toggleModeRadio);
            modeGroupBox.Controls.Add(holdModeRadio);
            modeGroupBox.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            modeGroupBox.Location = new Point(20, 168);
            modeGroupBox.Name = "modeGroupBox";
            modeGroupBox.Size = new Size(760, 74);
            modeGroupBox.TabIndex = 5;
            modeGroupBox.TabStop = false;
            modeGroupBox.Text = "触发模式";
            // 
            // holdModeRadio
            // 
            holdModeRadio.AutoSize = true;
            holdModeRadio.Checked = true;
            holdModeRadio.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            holdModeRadio.Location = new Point(16, 33);
            holdModeRadio.Name = "holdModeRadio";
            holdModeRadio.Size = new Size(82, 22);
            holdModeRadio.TabIndex = 0;
            holdModeRadio.TabStop = true;
            holdModeRadio.Text = "按住按键";
            holdModeRadio.UseVisualStyleBackColor = true;
            holdModeRadio.CheckedChanged += holdModeRadio_CheckedChanged;
            // 
            // toggleModeRadio
            // 
            toggleModeRadio.AutoSize = true;
            toggleModeRadio.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            toggleModeRadio.Location = new Point(132, 33);
            toggleModeRadio.Name = "toggleModeRadio";
            toggleModeRadio.Size = new Size(82, 22);
            toggleModeRadio.TabIndex = 1;
            toggleModeRadio.Text = "切换按键";
            toggleModeRadio.UseVisualStyleBackColor = true;
            toggleModeRadio.CheckedChanged += toggleModeRadio_CheckedChanged;
            // 
            // pollTimer
            // 
            pollTimer.Interval = 120;
            pollTimer.Tick += pollTimer_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 500);
            Controls.Add(modeGroupBox);
            Controls.Add(statusLabel);
            Controls.Add(mmKeyLabel);
            Controls.Add(tipLabel);
            Controls.Add(bindKeyTextBox);
            Controls.Add(bindKeyLabel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimumSize = new Size(800, 500);
            MinimizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MusicalMoments.KeyHelp";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            modeGroupBox.ResumeLayout(false);
            modeGroupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label bindKeyLabel;
        private TextBox bindKeyTextBox;
        private Label tipLabel;
        private Label mmKeyLabel;
        private Label statusLabel;
        private GroupBox modeGroupBox;
        private RadioButton holdModeRadio;
        private RadioButton toggleModeRadio;
        private System.Windows.Forms.Timer pollTimer;
    }
}
