namespace MusicalMoments.PluginExample
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
            statusLabel = new Label();
            refreshStateButton = new Button();
            refreshAudioButton = new Button();
            playSelectedButton = new Button();
            stopButton = new Button();
            realtimeCheckBox = new CheckBox();
            stateTextBox = new TextBox();
            audioListBox = new ListBox();
            refreshTimer = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // statusLabel
            // 
            statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            statusLabel.Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            statusLabel.Location = new Point(12, 10);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(776, 24);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "状态：初始化中";
            // 
            // refreshStateButton
            // 
            refreshStateButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            refreshStateButton.Location = new Point(12, 38);
            refreshStateButton.Name = "refreshStateButton";
            refreshStateButton.Size = new Size(96, 28);
            refreshStateButton.TabIndex = 1;
            refreshStateButton.Text = "刷新状态";
            refreshStateButton.UseVisualStyleBackColor = true;
            refreshStateButton.Click += refreshStateButton_Click;
            // 
            // refreshAudioButton
            // 
            refreshAudioButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            refreshAudioButton.Location = new Point(114, 38);
            refreshAudioButton.Name = "refreshAudioButton";
            refreshAudioButton.Size = new Size(96, 28);
            refreshAudioButton.TabIndex = 2;
            refreshAudioButton.Text = "刷新音频";
            refreshAudioButton.UseVisualStyleBackColor = true;
            refreshAudioButton.Click += refreshAudioButton_Click;
            // 
            // playSelectedButton
            // 
            playSelectedButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            playSelectedButton.Location = new Point(216, 38);
            playSelectedButton.Name = "playSelectedButton";
            playSelectedButton.Size = new Size(112, 28);
            playSelectedButton.TabIndex = 3;
            playSelectedButton.Text = "播放选中音频";
            playSelectedButton.UseVisualStyleBackColor = true;
            playSelectedButton.Click += playSelectedButton_Click;
            // 
            // stopButton
            // 
            stopButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            stopButton.Location = new Point(334, 38);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(96, 28);
            stopButton.TabIndex = 4;
            stopButton.Text = "停止播放";
            stopButton.UseVisualStyleBackColor = true;
            stopButton.Click += stopButton_Click;
            // 
            // realtimeCheckBox
            // 
            realtimeCheckBox.AutoSize = true;
            realtimeCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            realtimeCheckBox.Location = new Point(438, 43);
            realtimeCheckBox.Name = "realtimeCheckBox";
            realtimeCheckBox.Size = new Size(99, 21);
            realtimeCheckBox.TabIndex = 5;
            realtimeCheckBox.Text = "实时刷新状态";
            realtimeCheckBox.UseVisualStyleBackColor = true;
            realtimeCheckBox.CheckedChanged += realtimeCheckBox_CheckedChanged;
            // 
            // stateTextBox
            // 
            stateTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            stateTextBox.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
            stateTextBox.Location = new Point(12, 74);
            stateTextBox.Multiline = true;
            stateTextBox.Name = "stateTextBox";
            stateTextBox.ReadOnly = true;
            stateTextBox.ScrollBars = ScrollBars.Vertical;
            stateTextBox.Size = new Size(500, 414);
            stateTextBox.TabIndex = 6;
            // 
            // audioListBox
            // 
            audioListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            audioListBox.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            audioListBox.FormattingEnabled = true;
            audioListBox.ItemHeight = 17;
            audioListBox.Location = new Point(520, 74);
            audioListBox.Name = "audioListBox";
            audioListBox.Size = new Size(268, 412);
            audioListBox.TabIndex = 7;
            audioListBox.DoubleClick += audioListBox_DoubleClick;
            // 
            // refreshTimer
            // 
            refreshTimer.Interval = 400;
            refreshTimer.Tick += refreshTimer_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 500);
            Controls.Add(audioListBox);
            Controls.Add(stateTextBox);
            Controls.Add(realtimeCheckBox);
            Controls.Add(stopButton);
            Controls.Add(playSelectedButton);
            Controls.Add(refreshAudioButton);
            Controls.Add(refreshStateButton);
            Controls.Add(statusLabel);
            MinimumSize = new Size(800, 500);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MusicalMoments.PluginExample";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        private Label statusLabel;
        private Button refreshStateButton;
        private Button refreshAudioButton;
        private Button playSelectedButton;
        private Button stopButton;
        private CheckBox realtimeCheckBox;
        private TextBox stateTextBox;
        private ListBox audioListBox;
        private System.Windows.Forms.Timer refreshTimer;
    }
}
