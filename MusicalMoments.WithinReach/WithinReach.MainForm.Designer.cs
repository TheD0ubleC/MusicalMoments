namespace MusicalMoments.WithinReach
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            headerTitleLabel = new Label();
            subtitleLabel = new Label();
            statusGroup = new GroupBox();
            playbackBehaviorLabel = new Label();
            webStatusLabel = new Label();
            mmStatusLabel = new Label();
            serverGroup = new GroupBox();
            refreshButton = new Button();
            copyAddressButton = new Button();
            openWebButton = new Button();
            toggleWebButton = new Button();
            portInput = new NumericUpDown();
            portLabel = new Label();
            modeGroup = new GroupBox();
            modeTipLabel = new Label();
            modeDirectPlayRadio = new RadioButton();
            modeSetSelectedRadio = new RadioButton();
            addressGroup = new GroupBox();
            addressTextBox = new TextBox();
            addressLabel = new Label();
            statusTimer = new System.Windows.Forms.Timer(components);
            statusGroup.SuspendLayout();
            serverGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)portInput).BeginInit();
            modeGroup.SuspendLayout();
            addressGroup.SuspendLayout();
            SuspendLayout();
            // 
            // headerTitleLabel
            // 
            headerTitleLabel.AutoSize = true;
            headerTitleLabel.Font = new Font("Microsoft YaHei UI", 17.25F, FontStyle.Bold, GraphicsUnit.Point);
            headerTitleLabel.Location = new Point(16, 14);
            headerTitleLabel.Name = "headerTitleLabel";
            headerTitleLabel.Size = new Size(250, 30);
            headerTitleLabel.TabIndex = 0;
            headerTitleLabel.Text = "WithinReach 控制中心";
            // 
            // subtitleLabel
            // 
            subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            subtitleLabel.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            subtitleLabel.Location = new Point(19, 46);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Size = new Size(947, 21);
            subtitleLabel.TabIndex = 1;
            subtitleLabel.Text = "在电脑或手机浏览器中远程控制 MM 音频播放，并实时查看播放状态。";
            // 
            // statusGroup
            // 
            statusGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            statusGroup.Controls.Add(playbackBehaviorLabel);
            statusGroup.Controls.Add(webStatusLabel);
            statusGroup.Controls.Add(mmStatusLabel);
            statusGroup.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            statusGroup.Location = new Point(16, 74);
            statusGroup.Name = "statusGroup";
            statusGroup.Size = new Size(950, 112);
            statusGroup.TabIndex = 3;
            statusGroup.TabStop = false;
            statusGroup.Text = "状态总览";
            // 
            // playbackBehaviorLabel
            // 
            playbackBehaviorLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            playbackBehaviorLabel.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            playbackBehaviorLabel.Location = new Point(16, 78);
            playbackBehaviorLabel.Name = "playbackBehaviorLabel";
            playbackBehaviorLabel.Size = new Size(918, 22);
            playbackBehaviorLabel.TabIndex = 2;
            playbackBehaviorLabel.Text = "播放逻辑：读取中";
            // 
            // webStatusLabel
            // 
            webStatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            webStatusLabel.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            webStatusLabel.Location = new Point(16, 52);
            webStatusLabel.Name = "webStatusLabel";
            webStatusLabel.Size = new Size(918, 22);
            webStatusLabel.TabIndex = 1;
            webStatusLabel.Text = "Web 服务状态：未启动";
            // 
            // mmStatusLabel
            // 
            mmStatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mmStatusLabel.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            mmStatusLabel.Location = new Point(16, 26);
            mmStatusLabel.Name = "mmStatusLabel";
            mmStatusLabel.Size = new Size(918, 22);
            mmStatusLabel.TabIndex = 0;
            mmStatusLabel.Text = "MM 连接状态：初始化中";
            // 
            // serverGroup
            // 
            serverGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            serverGroup.Controls.Add(refreshButton);
            serverGroup.Controls.Add(copyAddressButton);
            serverGroup.Controls.Add(openWebButton);
            serverGroup.Controls.Add(toggleWebButton);
            serverGroup.Controls.Add(portInput);
            serverGroup.Controls.Add(portLabel);
            serverGroup.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            serverGroup.Location = new Point(16, 196);
            serverGroup.Name = "serverGroup";
            serverGroup.Size = new Size(950, 76);
            serverGroup.TabIndex = 4;
            serverGroup.TabStop = false;
            serverGroup.Text = "Web 服务";
            // 
            // refreshButton
            // 
            refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refreshButton.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            refreshButton.Location = new Point(840, 28);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(94, 32);
            refreshButton.TabIndex = 5;
            refreshButton.Text = "刷新状态";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += refreshButton_Click;
            // 
            // copyAddressButton
            // 
            copyAddressButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            copyAddressButton.Enabled = false;
            copyAddressButton.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            copyAddressButton.Location = new Point(734, 28);
            copyAddressButton.Name = "copyAddressButton";
            copyAddressButton.Size = new Size(98, 32);
            copyAddressButton.TabIndex = 4;
            copyAddressButton.Text = "复制地址";
            copyAddressButton.UseVisualStyleBackColor = true;
            copyAddressButton.Click += CopyAddressButton_Click;
            // 
            // openWebButton
            // 
            openWebButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            openWebButton.Enabled = false;
            openWebButton.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            openWebButton.Location = new Point(628, 28);
            openWebButton.Name = "openWebButton";
            openWebButton.Size = new Size(98, 32);
            openWebButton.TabIndex = 3;
            openWebButton.Text = "打开控制页";
            openWebButton.UseVisualStyleBackColor = true;
            openWebButton.Click += OpenWebButton_Click;
            // 
            // toggleWebButton
            // 
            toggleWebButton.Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            toggleWebButton.Location = new Point(236, 28);
            toggleWebButton.Name = "toggleWebButton";
            toggleWebButton.Size = new Size(136, 32);
            toggleWebButton.TabIndex = 2;
            toggleWebButton.Text = "启动 Web 服务";
            toggleWebButton.UseVisualStyleBackColor = true;
            toggleWebButton.Click += ToggleWebButton_Click;
            // 
            // portInput
            // 
            portInput.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
            portInput.Location = new Point(91, 32);
            portInput.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            portInput.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
            portInput.Name = "portInput";
            portInput.Size = new Size(132, 23);
            portInput.TabIndex = 1;
            portInput.Value = new decimal(new int[] { 18080, 0, 0, 0 });
            // 
            // portLabel
            // 
            portLabel.AutoSize = true;
            portLabel.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            portLabel.Location = new Point(16, 33);
            portLabel.Name = "portLabel";
            portLabel.Size = new Size(65, 20);
            portLabel.TabIndex = 0;
            portLabel.Text = "端口号：";
            // 
            // modeGroup
            // 
            modeGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            modeGroup.Controls.Add(modeTipLabel);
            modeGroup.Controls.Add(modeDirectPlayRadio);
            modeGroup.Controls.Add(modeSetSelectedRadio);
            modeGroup.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            modeGroup.Location = new Point(16, 282);
            modeGroup.Name = "modeGroup";
            modeGroup.Size = new Size(950, 86);
            modeGroup.TabIndex = 5;
            modeGroup.TabStop = false;
            modeGroup.Text = "Web 点击音频行为";
            // 
            // modeTipLabel
            // 
            modeTipLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            modeTipLabel.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            modeTipLabel.Location = new Point(16, 58);
            modeTipLabel.Name = "modeTipLabel";
            modeTipLabel.Size = new Size(918, 20);
            modeTipLabel.TabIndex = 2;
            modeTipLabel.Text = "设为播放项：Web 按钮显示“使用此音频”；直接播放：Web 按钮显示“播放此音频”。";
            // 
            // modeDirectPlayRadio
            // 
            modeDirectPlayRadio.AutoSize = true;
            modeDirectPlayRadio.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            modeDirectPlayRadio.Location = new Point(232, 30);
            modeDirectPlayRadio.Name = "modeDirectPlayRadio";
            modeDirectPlayRadio.Size = new Size(139, 24);
            modeDirectPlayRadio.TabIndex = 1;
            modeDirectPlayRadio.Text = "按下音频后直接播放";
            modeDirectPlayRadio.UseVisualStyleBackColor = true;
            modeDirectPlayRadio.CheckedChanged += ModeRadio_CheckedChanged;
            // 
            // modeSetSelectedRadio
            // 
            modeSetSelectedRadio.AutoSize = true;
            modeSetSelectedRadio.Checked = true;
            modeSetSelectedRadio.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            modeSetSelectedRadio.Location = new Point(16, 30);
            modeSetSelectedRadio.Name = "modeSetSelectedRadio";
            modeSetSelectedRadio.Size = new Size(139, 24);
            modeSetSelectedRadio.TabIndex = 0;
            modeSetSelectedRadio.TabStop = true;
            modeSetSelectedRadio.Text = "按下音频后设为播放项";
            modeSetSelectedRadio.UseVisualStyleBackColor = true;
            modeSetSelectedRadio.CheckedChanged += ModeRadio_CheckedChanged;
            // 
            // addressGroup
            // 
            addressGroup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            addressGroup.Controls.Add(addressTextBox);
            addressGroup.Controls.Add(addressLabel);
            addressGroup.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            addressGroup.Location = new Point(16, 378);
            addressGroup.Name = "addressGroup";
            addressGroup.Size = new Size(950, 250);
            addressGroup.TabIndex = 6;
            addressGroup.TabStop = false;
            addressGroup.Text = "访问地址与说明";
            // 
            // addressTextBox
            // 
            addressTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            addressTextBox.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
            addressTextBox.Location = new Point(16, 54);
            addressTextBox.Multiline = true;
            addressTextBox.Name = "addressTextBox";
            addressTextBox.ReadOnly = true;
            addressTextBox.ScrollBars = ScrollBars.Vertical;
            addressTextBox.Size = new Size(918, 180);
            addressTextBox.TabIndex = 1;
            addressTextBox.Text = "Web 服务未启动。";
            // 
            // addressLabel
            // 
            addressLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            addressLabel.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            addressLabel.Location = new Point(16, 26);
            addressLabel.Name = "addressLabel";
            addressLabel.Size = new Size(918, 24);
            addressLabel.TabIndex = 0;
            addressLabel.Text = "默认监听 0.0.0.0，局域网设备可直接访问以下地址。";
            // 
            // statusTimer
            // 
            statusTimer.Interval = 900;
            statusTimer.Tick += statusTimer_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(982, 643);
            Controls.Add(addressGroup);
            Controls.Add(modeGroup);
            Controls.Add(serverGroup);
            Controls.Add(statusGroup);
            Controls.Add(subtitleLabel);
            Controls.Add(headerTitleLabel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(800, 500);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MusicalMoments.WithinReach";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            statusGroup.ResumeLayout(false);
            serverGroup.ResumeLayout(false);
            serverGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)portInput).EndInit();
            modeGroup.ResumeLayout(false);
            modeGroup.PerformLayout();
            addressGroup.ResumeLayout(false);
            addressGroup.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label headerTitleLabel;
        private Label subtitleLabel;
        private GroupBox statusGroup;
        private Label playbackBehaviorLabel;
        private Label webStatusLabel;
        private Label mmStatusLabel;
        private GroupBox serverGroup;
        private Button refreshButton;
        private Button copyAddressButton;
        private Button openWebButton;
        private Button toggleWebButton;
        private NumericUpDown portInput;
        private Label portLabel;
        private GroupBox modeGroup;
        private Label modeTipLabel;
        private RadioButton modeDirectPlayRadio;
        private RadioButton modeSetSelectedRadio;
        private GroupBox addressGroup;
        private TextBox addressTextBox;
        private Label addressLabel;
        private System.Windows.Forms.Timer statusTimer;
    }
}
