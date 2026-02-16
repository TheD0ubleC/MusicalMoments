namespace MusicalMoments
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ListViewItem listViewItem10 = new ListViewItem("主页", 0);
            ListViewItem listViewItem11 = new ListViewItem("音频", 1);
            ListViewItem listViewItem12 = new ListViewItem("设置", 2);
            ListViewItem listViewItem13 = new ListViewItem("赞助", 3);
            ListViewItem listViewItem14 = new ListViewItem("关于", 4);
            ListViewItem listViewItem15 = new ListViewItem("转换", 5);
            ListViewItem listViewItem16 = new ListViewItem("发现", 6);
            ListViewItem listViewItem17 = new ListViewItem("插件", 7);
            ListViewItem listViewItem18 = new ListViewItem("反馈", 8);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            sideLists = new MusicalMoments.CustomUI.BufferedListView();
            sideListsImage = new ImageList(components);
            tabPage2 = new TabPage();
            audioNextPageButton = new Button();
            audioPrevPageButton = new Button();
            audioPageStatusLabel = new Label();
            audioSearchTextBox = new TextBox();
            audioSearchLabel = new Label();
            mToAudioData1 = new Button();
            SelectedAudioLabel = new Label();
            reLoadAudioListsView = new Button();
            audioListView = new ListView();
            AudioName = new ColumnHeader();
            AudioTrack = new ColumnHeader();
            AudioType = new ColumnHeader();
            AudioBindKey = new ColumnHeader();
            tabPage1 = new TabPage();
            tips_Group1 = new GroupBox();
            open_help_window = new Button();
            help_tip = new Label();
            to_audio_page = new Button();
            audio_page_tips = new Label();
            toC = new Button();
            tips_Label5 = new Label();
            mToAudioData = new Button();
            retestVB = new Button();
            label_VBStatus = new Label();
            toSettings = new Button();
            tips_Label4 = new Label();
            tips_Label3 = new Label();
            toVB = new Button();
            tips_Label2 = new Label();
            tips_Label1 = new Label();
            restoreDefaultsAfterInstallCheckBox = new CheckBox();
            mainTabControl = new MusicalMoments.CustomUI.CoverTabControl();
            tabPage3 = new TabPage();
            playbackBehaviorGroup = new GroupBox();
            differentAudioBehaviorComboBox = new ComboBox();
            differentAudioBehaviorLabel = new Label();
            sameAudioBehaviorComboBox = new ComboBox();
            sameAudioBehaviorLabel = new Label();
            volume_Group = new GroupBox();
            volume_Label3 = new Label();
            TipsVolumeTrackBar = new TrackBar();
            volume_Label2 = new Label();
            VolumeTrackBar = new TrackBar();
            volume_Label1 = new Label();
            VBVolumeTrackBar = new TrackBar();
            group_Misc = new GroupBox();
            check_update = new Button();
            open_help_button2 = new Button();
            switchStreamTips = new CheckBox();
            audioEquipmentPlay = new CheckBox();
            group_Key = new GroupBox();
            label_Key2 = new Label();
            PlayAudio = new TextBox();
            label_Key1 = new Label();
            ToggleStream = new TextBox();
            group_AudioEquipment = new GroupBox();
            autoSelectDevicesButton = new Button();
            comboBox_AudioEquipmentOutput = new ComboBox();
            label_AudioEquipment4 = new Label();
            label_AudioEquipment1 = new Label();
            comboBox_AudioEquipmentInput = new ComboBox();
            label_AudioEquipment2 = new Label();
            comboBox_VBAudioEquipmentOutput = new ComboBox();
            label_AudioEquipment3 = new Label();
            comboBox_VBAudioEquipmentInput = new ComboBox();
            tabPage4 = new TabPage();
            aifadian = new Button();
            imageAliPay = new PictureBox();
            imageWeChat = new PictureBox();
            thankTip = new Label();
            tabPage5 = new TabPage();
            info_Group = new GroupBox();
            info_ListBox = new ListBox();
            info_Label5 = new Label();
            info_Label2 = new Label();
            info_Label1 = new Label();
            LogoImage = new PictureBox();
            info_Label3 = new Label();
            tabPage6 = new TabPage();
            open_help_button1 = new Button();
            conversion_Group4 = new GroupBox();
            conversion_Label5 = new Label();
            conversion_Label4 = new Label();
            conversion_Group3 = new GroupBox();
            conversion_Label3 = new Label();
            conversion_Group2 = new GroupBox();
            convert_Button = new Button();
            conversion_Label2 = new Label();
            name_TextBox = new TextBox();
            conversion_Label1 = new Label();
            comboBoxOutputFormat = new ComboBox();
            conversion_Group1 = new GroupBox();
            dataPath_TextBox = new TextBox();
            upData_button = new Button();
            tabPage7 = new TabPage();
            to_mmdownloader = new Button();
            AudioListView_fd = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            numberLabel = new Label();
            DownloadSelected = new Button();
            SearchBarTextBox = new TextBox();
            DownloadLinkListBox = new ListBox();
            LoadList = new Button();
            tabPage9 = new TabPage();
            LoadPlugin = new Button();
            reLoadPluginListsView = new Button();
            PluginServerAddress = new Label();
            mToPluginData = new Button();
            PluginStatus = new Label();
            TogglePluginServer = new Button();
            pluginListView = new ListView();
            PluginName = new ColumnHeader();
            PluginAuthor = new ColumnHeader();
            PluginVer = new ColumnHeader();
            tabPage10 = new TabPage();
            FeedbackTipsButton = new Button();
            FeedbackContent = new TextBox();
            FeedbackTips4 = new Label();
            FeedbackButton = new Button();
            FeedbackDisaster = new RadioButton();
            FeedbackAverage = new RadioButton();
            FeedbackUrgent = new RadioButton();
            Contact = new TextBox();
            FeedbackTips3 = new Label();
            FeedbackTips2 = new Label();
            FeedbackTitle = new TextBox();
            FeedbackTips1 = new Label();
            mainGroupBox = new GroupBox();
            groupBox1 = new GroupBox();
            mainContextMenuStrip = new ContextMenuStrip(components);
            playSelectedMenuItem = new ToolStripMenuItem();
            stopPlaybackMenuItem = new ToolStripMenuItem();
            deleteSelectedMenuItem = new ToolStripMenuItem();
            renameSelectedMenuItem = new ToolStripMenuItem();
            setAsPlaybackItemMenuItem = new ToolStripMenuItem();
            openFileLocationMenuItem = new ToolStripMenuItem();
            bindKeyMenuItem = new ToolStripMenuItem();
            upData = new OpenFileDialog();
            tabPage2.SuspendLayout();
            tabPage1.SuspendLayout();
            tips_Group1.SuspendLayout();
            mainTabControl.SuspendLayout();
            tabPage3.SuspendLayout();
            playbackBehaviorGroup.SuspendLayout();
            volume_Group.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TipsVolumeTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)VolumeTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)VBVolumeTrackBar).BeginInit();
            group_Misc.SuspendLayout();
            group_Key.SuspendLayout();
            group_AudioEquipment.SuspendLayout();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)imageAliPay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)imageWeChat).BeginInit();
            tabPage5.SuspendLayout();
            info_Group.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LogoImage).BeginInit();
            tabPage6.SuspendLayout();
            conversion_Group4.SuspendLayout();
            conversion_Group3.SuspendLayout();
            conversion_Group2.SuspendLayout();
            conversion_Group1.SuspendLayout();
            tabPage7.SuspendLayout();
            tabPage9.SuspendLayout();
            tabPage10.SuspendLayout();
            mainGroupBox.SuspendLayout();
            groupBox1.SuspendLayout();
            mainContextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // sideLists
            // 
            sideLists.BackColor = SystemColors.Control;
            sideLists.BorderStyle = BorderStyle.None;
            sideLists.Cursor = Cursors.Hand;
            sideLists.Font = new Font("Microsoft JhengHei UI", 13F);
            sideLists.ForeColor = Color.FromArgb(90, 90, 90);
            sideLists.Items.AddRange(new ListViewItem[] { listViewItem10, listViewItem11, listViewItem12, listViewItem13, listViewItem14, listViewItem15, listViewItem16, listViewItem17, listViewItem18 });
            sideLists.LargeImageList = sideListsImage;
            sideLists.HideSelection = false;
            sideLists.HotTracking = false;
            sideLists.HoverSelection = false;
            sideLists.Location = new Point(3, 16);
            sideLists.MultiSelect = false;
            sideLists.Name = "sideLists";
            sideLists.OwnerDraw = true;
            sideLists.Scrollable = false;
            sideLists.Size = new Size(86, 363);
            sideLists.SmallImageList = sideListsImage;
            sideLists.TabIndex = 0;
            sideLists.TabStop = false;
            sideLists.TileSize = new Size(84, 40);
            sideLists.UseCompatibleStateImageBehavior = false;
            sideLists.View = View.Tile;
            sideLists.DrawItem += sideLists_DrawItem;
            sideLists.MouseDown += sideLists_MouseDown;
            sideLists.SelectedIndexChanged += sideLists_SelectedIndexChanged;
            // 
            // sideListsImage
            // 
            sideListsImage.ColorDepth = ColorDepth.Depth32Bit;
            sideListsImage.ImageStream = (ImageListStreamer)resources.GetObject("sideListsImage.ImageStream");
            sideListsImage.TransparentColor = Color.Transparent;
            sideListsImage.Images.SetKeyName(0, "main_page.png");
            sideListsImage.Images.SetKeyName(1, "audio.png");
            sideListsImage.Images.SetKeyName(2, "settings.png");
            sideListsImage.Images.SetKeyName(3, "donate.png");
            sideListsImage.Images.SetKeyName(4, "about.png");
            sideListsImage.Images.SetKeyName(5, "convert.png");
            sideListsImage.Images.SetKeyName(6, "discover.png");
            sideListsImage.Images.SetKeyName(7, "plugin.png");
            sideListsImage.Images.SetKeyName(8, "feedback.png");
            // 
            // tabPage2
            // 
            tabPage2.BackColor = SystemColors.Control;
            tabPage2.Controls.Add(audioNextPageButton);
            tabPage2.Controls.Add(audioPrevPageButton);
            tabPage2.Controls.Add(audioPageStatusLabel);
            tabPage2.Controls.Add(audioSearchTextBox);
            tabPage2.Controls.Add(audioSearchLabel);
            tabPage2.Controls.Add(mToAudioData1);
            tabPage2.Controls.Add(SelectedAudioLabel);
            tabPage2.Controls.Add(reLoadAudioListsView);
            tabPage2.Controls.Add(audioListView);
            tabPage2.Location = new Point(0, 22);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(671, 434);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "音频";
            // 
            // audioNextPageButton
            // 
            audioNextPageButton.Font = new Font("Microsoft JhengHei UI", 9.25F, FontStyle.Bold);
            audioNextPageButton.Location = new Point(339, 301);
            audioNextPageButton.Name = "audioNextPageButton";
            audioNextPageButton.Size = new Size(78, 32);
            audioNextPageButton.TabIndex = 8;
            audioNextPageButton.Text = "下一页";
            audioNextPageButton.UseVisualStyleBackColor = true;
            audioNextPageButton.Click += audioNextPageButton_Click;
            // 
            // audioPrevPageButton
            // 
            audioPrevPageButton.Font = new Font("Microsoft JhengHei UI", 9.25F, FontStyle.Bold);
            audioPrevPageButton.Location = new Point(255, 301);
            audioPrevPageButton.Name = "audioPrevPageButton";
            audioPrevPageButton.Size = new Size(78, 32);
            audioPrevPageButton.TabIndex = 7;
            audioPrevPageButton.Text = "上一页";
            audioPrevPageButton.UseVisualStyleBackColor = true;
            audioPrevPageButton.Click += audioPrevPageButton_Click;
            // 
            // audioPageStatusLabel
            // 
            audioPageStatusLabel.Font = new Font("Microsoft JhengHei UI", 9.25F, FontStyle.Bold);
            audioPageStatusLabel.Location = new Point(112, 303);
            audioPageStatusLabel.Name = "audioPageStatusLabel";
            audioPageStatusLabel.Size = new Size(137, 24);
            audioPageStatusLabel.TabIndex = 6;
            audioPageStatusLabel.Text = "第 1 / 1 页，共 0 条";
            audioPageStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // audioSearchTextBox
            // 
            audioSearchTextBox.BorderStyle = BorderStyle.FixedSingle;
            audioSearchTextBox.Font = new Font("Microsoft JhengHei UI", 9.75F);
            audioSearchTextBox.ForeColor = Color.FromArgb(90, 90, 90);
            audioSearchTextBox.Location = new Point(61, 6);
            audioSearchTextBox.Name = "audioSearchTextBox";
            audioSearchTextBox.PlaceholderText = "按名称 / 曲目 / 类型 / 按键搜索";
            audioSearchTextBox.RightToLeft = RightToLeft.No;
            audioSearchTextBox.Size = new Size(604, 24);
            audioSearchTextBox.TabIndex = 5;
            audioSearchTextBox.TextChanged += audioSearchTextBox_TextChanged;
            // 
            // audioSearchLabel
            // 
            audioSearchLabel.AutoSize = true;
            audioSearchLabel.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold);
            audioSearchLabel.ForeColor = Color.FromArgb(90, 90, 90);
            audioSearchLabel.Location = new Point(6, 9);
            audioSearchLabel.Name = "audioSearchLabel";
            audioSearchLabel.RightToLeft = RightToLeft.No;
            audioSearchLabel.Size = new Size(47, 17);
            audioSearchLabel.TabIndex = 4;
            audioSearchLabel.Text = "搜索：";
            // 
            // mToAudioData1
            // 
            mToAudioData1.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold);
            mToAudioData1.Location = new Point(505, 301);
            mToAudioData1.Name = "mToAudioData1";
            mToAudioData1.Size = new Size(160, 32);
            mToAudioData1.TabIndex = 3;
            mToAudioData1.Text = "打开存放路径";
            mToAudioData1.UseVisualStyleBackColor = true;
            mToAudioData1.Click += mToAudioData1_Click;
            // 
            // SelectedAudioLabel
            // 
            SelectedAudioLabel.AutoSize = true;
            SelectedAudioLabel.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold);
            SelectedAudioLabel.Location = new Point(6, 303);
            SelectedAudioLabel.Name = "SelectedAudioLabel";
            SelectedAudioLabel.RightToLeft = RightToLeft.No;
            SelectedAudioLabel.Size = new Size(50, 17);
            SelectedAudioLabel.TabIndex = 2;
            SelectedAudioLabel.Text = "已选择:";
            // 
            // reLoadAudioListsView
            // 
            reLoadAudioListsView.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold);
            reLoadAudioListsView.Location = new Point(423, 301);
            reLoadAudioListsView.Name = "reLoadAudioListsView";
            reLoadAudioListsView.Size = new Size(75, 32);
            reLoadAudioListsView.TabIndex = 1;
            reLoadAudioListsView.Text = "刷新";
            reLoadAudioListsView.UseVisualStyleBackColor = true;
            reLoadAudioListsView.Click += reLoadAudioListsView_Click;
            // 
            // audioListView
            // 
            audioListView.AllowDrop = true;
            audioListView.Columns.AddRange(new ColumnHeader[] { AudioName, AudioTrack, AudioType, AudioBindKey });
            audioListView.Font = new Font("Microsoft JhengHei UI", 9.75F);
            audioListView.ForeColor = SystemColors.WindowFrame;
            audioListView.FullRowSelect = true;
            audioListView.Location = new Point(6, 36);
            audioListView.Name = "audioListView";
            audioListView.RightToLeft = RightToLeft.No;
            audioListView.Size = new Size(659, 261);
            audioListView.TabIndex = 0;
            audioListView.UseCompatibleStateImageBehavior = false;
            audioListView.View = View.Details;
            audioListView.DragDrop += audioListView_DragDrop;
            audioListView.DragEnter += audioListView_DragEnter;
            audioListView.MouseClick += audioListView_MouseClick;
            // 
            // AudioName
            // 
            AudioName.Text = "名称";
            AudioName.Width = 240;
            // 
            // AudioTrack
            // 
            AudioTrack.Text = "曲目";
            AudioTrack.Width = 120;
            // 
            // AudioType
            // 
            AudioType.Text = "类型";
            AudioType.Width = 80;
            // 
            // AudioBindKey
            // 
            AudioBindKey.Text = "按键";
            AudioBindKey.Width = 100;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = SystemColors.Control;
            tabPage1.Controls.Add(tips_Group1);
            tabPage1.Location = new Point(0, 22);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(671, 434);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "主页";
            // 
            // tips_Group1
            // 
            tips_Group1.Controls.Add(open_help_window);
            tips_Group1.Controls.Add(help_tip);
            tips_Group1.Controls.Add(to_audio_page);
            tips_Group1.Controls.Add(audio_page_tips);
            tips_Group1.Controls.Add(toC);
            tips_Group1.Controls.Add(tips_Label5);
            tips_Group1.Controls.Add(mToAudioData);
            tips_Group1.Controls.Add(retestVB);
            tips_Group1.Controls.Add(label_VBStatus);
            tips_Group1.Controls.Add(toSettings);
            tips_Group1.Controls.Add(tips_Label4);
            tips_Group1.Controls.Add(tips_Label3);
            tips_Group1.Controls.Add(toVB);
            tips_Group1.Controls.Add(tips_Label2);
            tips_Group1.Controls.Add(tips_Label1);
            tips_Group1.Controls.Add(restoreDefaultsAfterInstallCheckBox);
            tips_Group1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            tips_Group1.ForeColor = Color.FromArgb(90, 90, 90);
            tips_Group1.Location = new Point(0, -10);
            tips_Group1.Name = "tips_Group1";
            tips_Group1.RightToLeft = RightToLeft.No;
            tips_Group1.Size = new Size(665, 444);
            tips_Group1.TabIndex = 11;
            tips_Group1.TabStop = false;
            // 
            // open_help_window
            // 
            open_help_window.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            open_help_window.Location = new Point(562, 281);
            open_help_window.Name = "open_help_window";
            open_help_window.Size = new Size(97, 27);
            open_help_window.TabIndex = 20;
            open_help_window.Text = "图文教程";
            open_help_window.UseVisualStyleBackColor = true;
            open_help_window.Click += open_help_window_Click;
            // 
            // help_tip
            // 
            help_tip.BorderStyle = BorderStyle.FixedSingle;
            help_tip.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold);
            help_tip.ForeColor = Color.FromArgb(90, 90, 90);
            help_tip.Location = new Point(6, 280);
            help_tip.Name = "help_tip";
            help_tip.Size = new Size(550, 34);
            help_tip.TabIndex = 21;
            help_tip.Text = "状态：等待操作";
            // 
            // to_audio_page
            // 
            to_audio_page.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            to_audio_page.Location = new Point(562, 129);
            to_audio_page.Name = "to_audio_page";
            to_audio_page.Size = new Size(97, 27);
            to_audio_page.TabIndex = 18;
            to_audio_page.Text = "一键修复";
            to_audio_page.UseVisualStyleBackColor = true;
            to_audio_page.Click += to_audio_page_Click;
            // 
            // audio_page_tips
            // 
            audio_page_tips.BorderStyle = BorderStyle.FixedSingle;
            audio_page_tips.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold);
            audio_page_tips.Location = new Point(6, 242);
            audio_page_tips.Name = "audio_page_tips";
            audio_page_tips.Size = new Size(550, 34);
            audio_page_tips.TabIndex = 19;
            audio_page_tips.Text = "附加工具：可快速打开系统声音设置与图文教程";
            // 
            // toC
            // 
            toC.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            toC.Location = new Point(562, 243);
            toC.Name = "toC";
            toC.Size = new Size(97, 27);
            toC.TabIndex = 17;
            toC.Text = "声音设置";
            toC.UseVisualStyleBackColor = true;
            toC.Click += toC_Click;
            // 
            // tips_Label5
            // 
            tips_Label5.BorderStyle = BorderStyle.FixedSingle;
            tips_Label5.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold);
            tips_Label5.Location = new Point(6, 166);
            tips_Label5.Name = "tips_Label5";
            tips_Label5.Size = new Size(550, 34);
            tips_Label5.TabIndex = 10;
            tips_Label5.Text = "步骤4：前往【设置】点击【自动识别设备】并确认设备";
            // 
            // mToAudioData
            // 
            mToAudioData.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            mToAudioData.Location = new Point(562, 205);
            mToAudioData.Name = "mToAudioData";
            mToAudioData.Size = new Size(97, 27);
            mToAudioData.TabIndex = 15;
            mToAudioData.Text = "卸载VB";
            mToAudioData.UseVisualStyleBackColor = true;
            mToAudioData.Click += mToAudioData_Click;
            // 
            // retestVB
            // 
            retestVB.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            retestVB.Location = new Point(562, 91);
            retestVB.Name = "retestVB";
            retestVB.Size = new Size(97, 27);
            retestVB.TabIndex = 0;
            retestVB.Text = "检查状态";
            retestVB.UseVisualStyleBackColor = true;
            retestVB.Click += retestVB_Click;
            // 
            // label_VBStatus
            // 
            label_VBStatus.BorderStyle = BorderStyle.FixedSingle;
            label_VBStatus.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold);
            label_VBStatus.Location = new Point(6, 204);
            label_VBStatus.Name = "label_VBStatus";
            label_VBStatus.Size = new Size(550, 34);
            label_VBStatus.TabIndex = 1;
            label_VBStatus.Text = "VB状态：未检测";
            // 
            // toSettings
            // 
            toSettings.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            toSettings.Location = new Point(562, 164);
            toSettings.Name = "toSettings";
            toSettings.Size = new Size(97, 27);
            toSettings.TabIndex = 0;
            toSettings.Text = "前往设置";
            toSettings.UseVisualStyleBackColor = true;
            toSettings.Click += toSettings_Click;
            // 
            // tips_Label4
            // 
            tips_Label4.BorderStyle = BorderStyle.FixedSingle;
            tips_Label4.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold);
            tips_Label4.Location = new Point(6, 128);
            tips_Label4.Name = "tips_Label4";
            tips_Label4.Size = new Size(550, 34);
            tips_Label4.TabIndex = 13;
            tips_Label4.Text = "步骤3：如提示格式异常，请点右侧【一键修复】";
            // 
            // tips_Label3
            // 
            tips_Label3.BorderStyle = BorderStyle.FixedSingle;
            tips_Label3.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold);
            tips_Label3.Location = new Point(6, 90);
            tips_Label3.Name = "tips_Label3";
            tips_Label3.Size = new Size(550, 34);
            tips_Label3.TabIndex = 12;
            tips_Label3.Text = "步骤2：安装完成后点右侧【检查状态】确认 VB 正常";
            // 
            // toVB
            // 
            toVB.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            toVB.Location = new Point(562, 52);
            toVB.Name = "toVB";
            toVB.Size = new Size(97, 27);
            toVB.TabIndex = 11;
            toVB.Text = "安装VB";
            toVB.UseVisualStyleBackColor = true;
            toVB.Click += toVB_Click;
            // 
            // tips_Label2
            // 
            tips_Label2.BorderStyle = BorderStyle.FixedSingle;
            tips_Label2.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold);
            tips_Label2.Location = new Point(6, 51);
            tips_Label2.Name = "tips_Label2";
            tips_Label2.Size = new Size(550, 34);
            tips_Label2.TabIndex = 10;
            tips_Label2.Text = "步骤1：点击右侧【安装VB】（会自动下载并静默安装）";
            // 
            // tips_Label1
            // 
            tips_Label1.Font = new Font("Microsoft JhengHei UI", 20.25F, FontStyle.Bold);
            tips_Label1.Location = new Point(6, 13);
            tips_Label1.Name = "tips_Label1";
            tips_Label1.Size = new Size(553, 34);
            tips_Label1.TabIndex = 9;
            tips_Label1.Text = "新手引导（按步骤操作）";
            tips_Label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // restoreDefaultsAfterInstallCheckBox
            // 
            restoreDefaultsAfterInstallCheckBox.AutoSize = true;
            restoreDefaultsAfterInstallCheckBox.Checked = true;
            restoreDefaultsAfterInstallCheckBox.CheckState = CheckState.Checked;
            restoreDefaultsAfterInstallCheckBox.Font = new Font("Microsoft JhengHei UI", 9.25F, FontStyle.Bold);
            restoreDefaultsAfterInstallCheckBox.Location = new Point(6, 316);
            restoreDefaultsAfterInstallCheckBox.Name = "restoreDefaultsAfterInstallCheckBox";
            restoreDefaultsAfterInstallCheckBox.Size = new Size(313, 21);
            restoreDefaultsAfterInstallCheckBox.TabIndex = 22;
            restoreDefaultsAfterInstallCheckBox.Text = "安装后自动恢复系统默认扬声器与麦克风（推荐）";
            restoreDefaultsAfterInstallCheckBox.UseVisualStyleBackColor = true;
            // 
            // mainTabControl
            // 
            mainTabControl.Controls.Add(tabPage1);
            mainTabControl.Controls.Add(tabPage2);
            mainTabControl.Controls.Add(tabPage3);
            mainTabControl.Controls.Add(tabPage4);
            mainTabControl.Controls.Add(tabPage5);
            mainTabControl.Controls.Add(tabPage6);
            mainTabControl.Controls.Add(tabPage7);
            mainTabControl.Controls.Add(tabPage9);
            mainTabControl.Controls.Add(tabPage10);
            mainTabControl.Font = new Font("Microsoft JhengHei UI", 10.25F);
            mainTabControl.ItemSize = new Size(62, 22);
            mainTabControl.Location = new Point(6, 22);
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(671, 456);
            mainTabControl.SizeMode = TabSizeMode.Fixed;
            mainTabControl.TabIndex = 1;
            mainTabControl.TabStop = false;
            mainTabControl.SelectedIndexChanged += mainTabControl_SelectedIndexChanged;
            // 
            // tabPage3
            // 
            tabPage3.AutoScroll = true;
            tabPage3.BackColor = SystemColors.Control;
            tabPage3.Controls.Add(playbackBehaviorGroup);
            tabPage3.Controls.Add(volume_Group);
            tabPage3.Controls.Add(group_Misc);
            tabPage3.Controls.Add(group_Key);
            tabPage3.Controls.Add(group_AudioEquipment);
            tabPage3.Location = new Point(0, 22);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(565, 341);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "设置";
            // 
            // playbackBehaviorGroup
            // 
            playbackBehaviorGroup.Controls.Add(differentAudioBehaviorComboBox);
            playbackBehaviorGroup.Controls.Add(differentAudioBehaviorLabel);
            playbackBehaviorGroup.Controls.Add(sameAudioBehaviorComboBox);
            playbackBehaviorGroup.Controls.Add(sameAudioBehaviorLabel);
            playbackBehaviorGroup.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold);
            playbackBehaviorGroup.ForeColor = Color.FromArgb(90, 90, 90);
            playbackBehaviorGroup.Location = new Point(6, 227);
            playbackBehaviorGroup.Name = "playbackBehaviorGroup";
            playbackBehaviorGroup.RightToLeft = RightToLeft.No;
            playbackBehaviorGroup.Size = new Size(553, 70);
            playbackBehaviorGroup.TabIndex = 13;
            playbackBehaviorGroup.TabStop = false;
            playbackBehaviorGroup.Text = "播放逻辑";
            // 
            // differentAudioBehaviorComboBox
            // 
            differentAudioBehaviorComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            differentAudioBehaviorComboBox.Font = new Font("Microsoft JhengHei UI", 9.25F);
            differentAudioBehaviorComboBox.FormattingEnabled = true;
            differentAudioBehaviorComboBox.Items.AddRange(new object[] { "仅停止当前播放", "停止并播放新的音频" });
            differentAudioBehaviorComboBox.Location = new Point(438, 26);
            differentAudioBehaviorComboBox.Name = "differentAudioBehaviorComboBox";
            differentAudioBehaviorComboBox.Size = new Size(105, 23);
            differentAudioBehaviorComboBox.TabIndex = 3;
            differentAudioBehaviorComboBox.SelectedIndexChanged += DifferentAudioBehaviorComboBox_SelectedIndexChanged;
            // 
            // differentAudioBehaviorLabel
            // 
            differentAudioBehaviorLabel.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold);
            differentAudioBehaviorLabel.Location = new Point(320, 27);
            differentAudioBehaviorLabel.Name = "differentAudioBehaviorLabel";
            differentAudioBehaviorLabel.Size = new Size(110, 24);
            differentAudioBehaviorLabel.TabIndex = 2;
            differentAudioBehaviorLabel.Text = "不同音频按下时：";
            differentAudioBehaviorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // sameAudioBehaviorComboBox
            // 
            sameAudioBehaviorComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sameAudioBehaviorComboBox.Font = new Font("Microsoft JhengHei UI", 9.25F);
            sameAudioBehaviorComboBox.FormattingEnabled = true;
            sameAudioBehaviorComboBox.Items.AddRange(new object[] { "从头重新播放", "停止当前播放" });
            sameAudioBehaviorComboBox.Location = new Point(148, 26);
            sameAudioBehaviorComboBox.Name = "sameAudioBehaviorComboBox";
            sameAudioBehaviorComboBox.Size = new Size(160, 23);
            sameAudioBehaviorComboBox.TabIndex = 1;
            sameAudioBehaviorComboBox.SelectedIndexChanged += SameAudioBehaviorComboBox_SelectedIndexChanged;
            // 
            // sameAudioBehaviorLabel
            // 
            sameAudioBehaviorLabel.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold);
            sameAudioBehaviorLabel.Location = new Point(10, 27);
            sameAudioBehaviorLabel.Name = "sameAudioBehaviorLabel";
            sameAudioBehaviorLabel.Size = new Size(130, 24);
            sameAudioBehaviorLabel.TabIndex = 0;
            sameAudioBehaviorLabel.Text = "同一音频再次按下：";
            sameAudioBehaviorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // volume_Group
            // 
            volume_Group.Controls.Add(volume_Label3);
            volume_Group.Controls.Add(TipsVolumeTrackBar);
            volume_Group.Controls.Add(volume_Label2);
            volume_Group.Controls.Add(VolumeTrackBar);
            volume_Group.Controls.Add(volume_Label1);
            volume_Group.Controls.Add(VBVolumeTrackBar);
            volume_Group.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold);
            volume_Group.ForeColor = Color.FromArgb(90, 90, 90);
            volume_Group.Location = new Point(6, 303);
            volume_Group.Name = "volume_Group";
            volume_Group.RightToLeft = RightToLeft.No;
            volume_Group.Size = new Size(553, 71);
            volume_Group.TabIndex = 12;
            volume_Group.TabStop = false;
            volume_Group.Text = "音量";
            // 
            // volume_Label3
            // 
            volume_Label3.Font = new Font("Microsoft JhengHei UI", 10.5F);
            volume_Label3.Location = new Point(363, 20);
            volume_Label3.Name = "volume_Label3";
            volume_Label3.Size = new Size(61, 45);
            volume_Label3.TabIndex = 14;
            volume_Label3.Text = "提示音(100%):";
            volume_Label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TipsVolumeTrackBar
            // 
            TipsVolumeTrackBar.LargeChange = 1;
            TipsVolumeTrackBar.Location = new Point(416, 20);
            TipsVolumeTrackBar.Maximum = 100;
            TipsVolumeTrackBar.Name = "TipsVolumeTrackBar";
            TipsVolumeTrackBar.Size = new Size(124, 45);
            TipsVolumeTrackBar.TabIndex = 13;
            TipsVolumeTrackBar.TickStyle = TickStyle.Both;
            TipsVolumeTrackBar.Value = 100;
            TipsVolumeTrackBar.Scroll += TipsVolumeTrackBar_Scroll;
            // 
            // volume_Label2
            // 
            volume_Label2.Font = new Font("Microsoft JhengHei UI", 10.5F);
            volume_Label2.Location = new Point(186, 20);
            volume_Label2.Name = "volume_Label2";
            volume_Label2.Size = new Size(61, 45);
            volume_Label2.TabIndex = 12;
            volume_Label2.Text = "物理麦(100%):";
            volume_Label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // VolumeTrackBar
            // 
            VolumeTrackBar.LargeChange = 1;
            VolumeTrackBar.Location = new Point(233, 20);
            VolumeTrackBar.Maximum = 100;
            VolumeTrackBar.Name = "VolumeTrackBar";
            VolumeTrackBar.Size = new Size(124, 45);
            VolumeTrackBar.TabIndex = 11;
            VolumeTrackBar.TickStyle = TickStyle.Both;
            VolumeTrackBar.Value = 100;
            VolumeTrackBar.Scroll += VolumeTrackBar_Scroll;
            // 
            // volume_Label1
            // 
            volume_Label1.Font = new Font("Microsoft JhengHei UI", 10.5F);
            volume_Label1.Location = new Point(4, 20);
            volume_Label1.Name = "volume_Label1";
            volume_Label1.Size = new Size(63, 45);
            volume_Label1.TabIndex = 10;
            volume_Label1.Text = "声卡麦(100%):";
            volume_Label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // VBVolumeTrackBar
            // 
            VBVolumeTrackBar.LargeChange = 1;
            VBVolumeTrackBar.Location = new Point(56, 20);
            VBVolumeTrackBar.Maximum = 100;
            VBVolumeTrackBar.Name = "VBVolumeTrackBar";
            VBVolumeTrackBar.Size = new Size(124, 45);
            VBVolumeTrackBar.TabIndex = 0;
            VBVolumeTrackBar.TickStyle = TickStyle.Both;
            VBVolumeTrackBar.Value = 100;
            VBVolumeTrackBar.Scroll += VBVolumeTrackBar_Scroll;
            // 
            // group_Misc
            // 
            group_Misc.Controls.Add(check_update);
            group_Misc.Controls.Add(open_help_button2);
            group_Misc.Controls.Add(switchStreamTips);
            group_Misc.Controls.Add(audioEquipmentPlay);
            group_Misc.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold);
            group_Misc.ForeColor = Color.FromArgb(90, 90, 90);
            group_Misc.Location = new Point(6, 380);
            group_Misc.Name = "group_Misc";
            group_Misc.RightToLeft = RightToLeft.No;
            group_Misc.Size = new Size(553, 46);
            group_Misc.TabIndex = 11;
            group_Misc.TabStop = false;
            group_Misc.Text = "其他设置";
            // 
            // check_update
            // 
            check_update.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            check_update.Location = new Point(430, 18);
            check_update.Name = "check_update";
            check_update.Size = new Size(112, 22);
            check_update.TabIndex = 23;
            check_update.Text = "检查更新";
            check_update.UseVisualStyleBackColor = true;
            check_update.Click += check_update_Click;
            // 
            // open_help_button2
            // 
            open_help_button2.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            open_help_button2.Location = new Point(312, 18);
            open_help_button2.Name = "open_help_button2";
            open_help_button2.Size = new Size(112, 22);
            open_help_button2.TabIndex = 22;
            open_help_button2.Text = "播放逻辑设置";
            open_help_button2.UseVisualStyleBackColor = true;
            open_help_button2.Visible = false;
            open_help_button2.Click += open_help_button2_Click;
            // 
            // switchStreamTips
            // 
            switchStreamTips.AutoSize = true;
            switchStreamTips.Checked = true;
            switchStreamTips.CheckState = CheckState.Checked;
            switchStreamTips.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold);
            switchStreamTips.Location = new Point(167, 18);
            switchStreamTips.Name = "switchStreamTips";
            switchStreamTips.Size = new Size(139, 22);
            switchStreamTips.TabIndex = 1;
            switchStreamTips.Text = "切换源时播放提示";
            switchStreamTips.UseVisualStyleBackColor = true;
            // 
            // audioEquipmentPlay
            // 
            audioEquipmentPlay.AutoSize = true;
            audioEquipmentPlay.Checked = true;
            audioEquipmentPlay.CheckState = CheckState.Checked;
            audioEquipmentPlay.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold);
            audioEquipmentPlay.Location = new Point(8, 18);
            audioEquipmentPlay.Name = "audioEquipmentPlay";
            audioEquipmentPlay.Size = new Size(153, 22);
            audioEquipmentPlay.TabIndex = 0;
            audioEquipmentPlay.Text = "物理扬声器同步播放";
            audioEquipmentPlay.UseVisualStyleBackColor = true;
            audioEquipmentPlay.CheckedChanged += audioEquipmentPlay_CheckedChanged;
            // 
            // group_Key
            // 
            group_Key.Controls.Add(label_Key2);
            group_Key.Controls.Add(PlayAudio);
            group_Key.Controls.Add(label_Key1);
            group_Key.Controls.Add(ToggleStream);
            group_Key.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold);
            group_Key.ForeColor = Color.FromArgb(90, 90, 90);
            group_Key.Location = new Point(6, 163);
            group_Key.Name = "group_Key";
            group_Key.RightToLeft = RightToLeft.No;
            group_Key.Size = new Size(553, 58);
            group_Key.TabIndex = 10;
            group_Key.TabStop = false;
            group_Key.Text = "按键";
            // 
            // label_Key2
            // 
            label_Key2.AutoSize = true;
            label_Key2.Font = new Font("Microsoft JhengHei UI", 14.25F);
            label_Key2.Location = new Point(267, 23);
            label_Key2.Name = "label_Key2";
            label_Key2.Size = new Size(124, 24);
            label_Key2.TabIndex = 11;
            label_Key2.Text = "播放音频按键";
            // 
            // PlayAudio
            // 
            PlayAudio.BorderStyle = BorderStyle.FixedSingle;
            PlayAudio.Font = new Font("Microsoft JhengHei UI", 10.5F);
            PlayAudio.ForeColor = Color.FromArgb(90, 90, 90);
            PlayAudio.ImeMode = ImeMode.Disable;
            PlayAudio.Location = new Point(405, 22);
            PlayAudio.Name = "PlayAudio";
            PlayAudio.RightToLeft = RightToLeft.No;
            PlayAudio.Size = new Size(135, 25);
            PlayAudio.TabIndex = 10;
            PlayAudio.Text = "Key";
            PlayAudio.TextAlign = HorizontalAlignment.Center;
            PlayAudio.KeyDown += PlayAudio_KeyDown;
            PlayAudio.KeyPress += PlayAudio_KeyPress;
            PlayAudio.MouseDown += PlayAudio_MouseDown;
            // 
            // label_Key1
            // 
            label_Key1.AutoSize = true;
            label_Key1.Font = new Font("Microsoft JhengHei UI", 14.25F);
            label_Key1.Location = new Point(8, 24);
            label_Key1.Name = "label_Key1";
            label_Key1.Size = new Size(105, 24);
            label_Key1.TabIndex = 9;
            label_Key1.Text = "切换源按键";
            // 
            // ToggleStream
            // 
            ToggleStream.BorderStyle = BorderStyle.FixedSingle;
            ToggleStream.Font = new Font("Microsoft JhengHei UI", 10.5F);
            ToggleStream.ForeColor = Color.FromArgb(90, 90, 90);
            ToggleStream.ImeMode = ImeMode.Disable;
            ToggleStream.Location = new Point(126, 23);
            ToggleStream.Name = "ToggleStream";
            ToggleStream.RightToLeft = RightToLeft.No;
            ToggleStream.Size = new Size(135, 25);
            ToggleStream.TabIndex = 8;
            ToggleStream.Text = "Key";
            ToggleStream.TextAlign = HorizontalAlignment.Center;
            ToggleStream.KeyDown += ToggleStream_KeyDown;
            ToggleStream.KeyPress += ToggleStream_KeyPress;
            ToggleStream.MouseDown += ToggleStream_MouseDown;
            // 
            // group_AudioEquipment
            // 
            group_AudioEquipment.Controls.Add(autoSelectDevicesButton);
            group_AudioEquipment.Controls.Add(comboBox_AudioEquipmentOutput);
            group_AudioEquipment.Controls.Add(label_AudioEquipment4);
            group_AudioEquipment.Controls.Add(label_AudioEquipment1);
            group_AudioEquipment.Controls.Add(comboBox_AudioEquipmentInput);
            group_AudioEquipment.Controls.Add(label_AudioEquipment2);
            group_AudioEquipment.Controls.Add(comboBox_VBAudioEquipmentOutput);
            group_AudioEquipment.Controls.Add(label_AudioEquipment3);
            group_AudioEquipment.Controls.Add(comboBox_VBAudioEquipmentInput);
            group_AudioEquipment.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold);
            group_AudioEquipment.ForeColor = Color.FromArgb(90, 90, 90);
            group_AudioEquipment.Location = new Point(6, 6);
            group_AudioEquipment.Name = "group_AudioEquipment";
            group_AudioEquipment.RightToLeft = RightToLeft.No;
            group_AudioEquipment.Size = new Size(553, 151);
            group_AudioEquipment.TabIndex = 6;
            group_AudioEquipment.TabStop = false;
            group_AudioEquipment.Text = "音频设备";
            // 
            // autoSelectDevicesButton
            // 
            autoSelectDevicesButton.Font = new Font("Microsoft JhengHei UI", 8.25F, FontStyle.Bold);
            autoSelectDevicesButton.Location = new Point(432, 0);
            autoSelectDevicesButton.Name = "autoSelectDevicesButton";
            autoSelectDevicesButton.Size = new Size(115, 22);
            autoSelectDevicesButton.TabIndex = 8;
            autoSelectDevicesButton.Text = "自动识别设备";
            autoSelectDevicesButton.UseVisualStyleBackColor = true;
            autoSelectDevicesButton.Click += autoSelectDevicesButton_Click;
            // 
            // comboBox_AudioEquipmentOutput
            // 
            comboBox_AudioEquipmentOutput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_AudioEquipmentOutput.Font = new Font("Microsoft JhengHei UI", 9.75F);
            comboBox_AudioEquipmentOutput.ForeColor = Color.FromArgb(90, 90, 90);
            comboBox_AudioEquipmentOutput.FormattingEnabled = true;
            comboBox_AudioEquipmentOutput.Location = new Point(126, 117);
            comboBox_AudioEquipmentOutput.Name = "comboBox_AudioEquipmentOutput";
            comboBox_AudioEquipmentOutput.Size = new Size(421, 25);
            comboBox_AudioEquipmentOutput.TabIndex = 7;
            comboBox_AudioEquipmentOutput.SelectedIndexChanged += comboBox_AudioEquipmentOutput_SelectedIndexChanged;
            // 
            // label_AudioEquipment4
            // 
            label_AudioEquipment4.AutoSize = true;
            label_AudioEquipment4.Font = new Font("Microsoft JhengHei UI", 14.25F);
            label_AudioEquipment4.Location = new Point(8, 117);
            label_AudioEquipment4.Name = "label_AudioEquipment4";
            label_AudioEquipment4.Size = new Size(105, 24);
            label_AudioEquipment4.TabIndex = 6;
            label_AudioEquipment4.Text = "物理扬声器";
            // 
            // label_AudioEquipment1
            // 
            label_AudioEquipment1.AutoSize = true;
            label_AudioEquipment1.Font = new Font("Microsoft JhengHei UI", 14.25F);
            label_AudioEquipment1.Location = new Point(8, 24);
            label_AudioEquipment1.Name = "label_AudioEquipment1";
            label_AudioEquipment1.Size = new Size(111, 24);
            label_AudioEquipment1.TabIndex = 0;
            label_AudioEquipment1.Text = "VB声卡输入";
            // 
            // comboBox_AudioEquipmentInput
            // 
            comboBox_AudioEquipmentInput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_AudioEquipmentInput.Font = new Font("Microsoft JhengHei UI", 9.75F);
            comboBox_AudioEquipmentInput.ForeColor = Color.FromArgb(90, 90, 90);
            comboBox_AudioEquipmentInput.FormattingEnabled = true;
            comboBox_AudioEquipmentInput.Location = new Point(126, 86);
            comboBox_AudioEquipmentInput.Name = "comboBox_AudioEquipmentInput";
            comboBox_AudioEquipmentInput.Size = new Size(421, 25);
            comboBox_AudioEquipmentInput.TabIndex = 5;
            comboBox_AudioEquipmentInput.SelectedIndexChanged += comboBox_AudioEquipmentInput_SelectedIndexChanged;
            // 
            // label_AudioEquipment2
            // 
            label_AudioEquipment2.AutoSize = true;
            label_AudioEquipment2.Font = new Font("Microsoft JhengHei UI", 14.25F);
            label_AudioEquipment2.Location = new Point(6, 55);
            label_AudioEquipment2.Name = "label_AudioEquipment2";
            label_AudioEquipment2.Size = new Size(111, 24);
            label_AudioEquipment2.TabIndex = 1;
            label_AudioEquipment2.Text = "VB声卡输出";
            // 
            // comboBox_VBAudioEquipmentOutput
            // 
            comboBox_VBAudioEquipmentOutput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_VBAudioEquipmentOutput.Font = new Font("Microsoft JhengHei UI", 9.75F);
            comboBox_VBAudioEquipmentOutput.ForeColor = Color.FromArgb(90, 90, 90);
            comboBox_VBAudioEquipmentOutput.FormattingEnabled = true;
            comboBox_VBAudioEquipmentOutput.Location = new Point(126, 55);
            comboBox_VBAudioEquipmentOutput.Name = "comboBox_VBAudioEquipmentOutput";
            comboBox_VBAudioEquipmentOutput.Size = new Size(421, 25);
            comboBox_VBAudioEquipmentOutput.TabIndex = 4;
            comboBox_VBAudioEquipmentOutput.SelectedIndexChanged += comboBox_VBAudioEquipmentOutput_SelectedIndexChanged;
            // 
            // label_AudioEquipment3
            // 
            label_AudioEquipment3.AutoSize = true;
            label_AudioEquipment3.Font = new Font("Microsoft JhengHei UI", 14.25F);
            label_AudioEquipment3.Location = new Point(8, 86);
            label_AudioEquipment3.Name = "label_AudioEquipment3";
            label_AudioEquipment3.Size = new Size(105, 24);
            label_AudioEquipment3.TabIndex = 2;
            label_AudioEquipment3.Text = "物理麦克风";
            // 
            // comboBox_VBAudioEquipmentInput
            // 
            comboBox_VBAudioEquipmentInput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_VBAudioEquipmentInput.Font = new Font("Microsoft JhengHei UI", 9.75F);
            comboBox_VBAudioEquipmentInput.ForeColor = Color.FromArgb(90, 90, 90);
            comboBox_VBAudioEquipmentInput.FormattingEnabled = true;
            comboBox_VBAudioEquipmentInput.Location = new Point(126, 24);
            comboBox_VBAudioEquipmentInput.Name = "comboBox_VBAudioEquipmentInput";
            comboBox_VBAudioEquipmentInput.Size = new Size(421, 25);
            comboBox_VBAudioEquipmentInput.TabIndex = 3;
            comboBox_VBAudioEquipmentInput.SelectedIndexChanged += comboBox_VBAudioEquipmentInput_SelectedIndexChanged;
            // 
            // tabPage4
            // 
            tabPage4.BackColor = SystemColors.Control;
            tabPage4.Controls.Add(aifadian);
            tabPage4.Controls.Add(imageAliPay);
            tabPage4.Controls.Add(imageWeChat);
            tabPage4.Controls.Add(thankTip);
            tabPage4.Location = new Point(0, 22);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(565, 341);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "赞助";
            // 
            // aifadian
            // 
            aifadian.Font = new Font("Microsoft JhengHei UI", 15F, FontStyle.Bold);
            aifadian.Location = new Point(6, 282);
            aifadian.Name = "aifadian";
            aifadian.Size = new Size(99, 32);
            aifadian.TabIndex = 6;
            aifadian.Text = "爱发电";
            aifadian.UseVisualStyleBackColor = true;
            aifadian.Click += aifadian_Click;
            // 
            // imageAliPay
            // 
            imageAliPay.Image = Properties.Resources.Alipay;
            imageAliPay.Location = new Point(289, 6);
            imageAliPay.Name = "imageAliPay";
            imageAliPay.Size = new Size(270, 270);
            imageAliPay.SizeMode = PictureBoxSizeMode.StretchImage;
            imageAliPay.TabIndex = 4;
            imageAliPay.TabStop = false;
            // 
            // imageWeChat
            // 
            imageWeChat.Image = Properties.Resources.WeChat;
            imageWeChat.Location = new Point(6, 6);
            imageWeChat.Name = "imageWeChat";
            imageWeChat.Size = new Size(270, 270);
            imageWeChat.SizeMode = PictureBoxSizeMode.StretchImage;
            imageWeChat.TabIndex = 3;
            imageWeChat.TabStop = false;
            // 
            // thankTip
            // 
            thankTip.AutoSize = true;
            thankTip.Font = new Font("Microsoft JhengHei UI", 24F, FontStyle.Bold);
            thankTip.Location = new Point(179, 305);
            thankTip.Name = "thankTip";
            thankTip.Size = new Size(210, 41);
            thankTip.TabIndex = 5;
            thankTip.Text = "感谢您的支持，打赏请写下您的名字和留言\n未来将有感谢名单页面！";
            thankTip.RightToLeft = RightToLeft.No;
            thankTip.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tabPage5
            // 
            tabPage5.BackColor = SystemColors.Control;
            tabPage5.Controls.Add(info_Group);
            tabPage5.Controls.Add(info_Label2);
            tabPage5.Controls.Add(info_Label1);
            tabPage5.Controls.Add(LogoImage);
            tabPage5.Controls.Add(info_Label3);
            tabPage5.Location = new Point(0, 22);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(565, 341);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "关于";
            // 
            // info_Group
            // 
            info_Group.Controls.Add(info_ListBox);
            info_Group.Controls.Add(info_Label5);
            info_Group.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold);
            info_Group.ForeColor = Color.FromArgb(90, 90, 90);
            info_Group.Location = new Point(5, 172);
            info_Group.Name = "info_Group";
            info_Group.RightToLeft = RightToLeft.No;
            info_Group.Size = new Size(557, 170);
            info_Group.TabIndex = 4;
            info_Group.TabStop = false;
            info_Group.Text = "引用许可证";
            // 
            // info_ListBox
            // 
            info_ListBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            info_ListBox.BackColor = SystemColors.Control;
            info_ListBox.Font = new Font("Microsoft JhengHei UI", 8.25F);
            info_ListBox.ForeColor = Color.FromArgb(90, 90, 90);
            info_ListBox.FormattingEnabled = true;
            info_ListBox.Items.AddRange(new object[] { "NAudio", "Newtonsoft.Json", "System.Management", "taglib-sharp-netstandard2.0", "Win32Hooks", "MediaToolkit", "HtmlAgilityPack" });
            info_ListBox.Location = new Point(389, 16);
            info_ListBox.Name = "info_ListBox";
            info_ListBox.RightToLeft = RightToLeft.No;
            info_ListBox.Size = new Size(162, 158);
            info_ListBox.TabIndex = 5;
            info_ListBox.SelectedIndexChanged += info_ListBox_SelectedIndexChanged;
            // 
            // info_Label5
            // 
            info_Label5.BorderStyle = BorderStyle.FixedSingle;
            info_Label5.Font = new Font("Microsoft JhengHei UI Light", 12F);
            info_Label5.ForeColor = Color.FromArgb(90, 90, 90);
            info_Label5.Location = new Point(6, 16);
            info_Label5.Name = "info_Label5";
            info_Label5.RightToLeft = RightToLeft.No;
            info_Label5.Size = new Size(377, 150);
            info_Label5.TabIndex = 3;
            info_Label5.Text = "MM使用了NAudio音频处理库。\r\nNAudio遵循Microsoft Public License (Ms-PL)。\r\n版权所有 (c) [NAudio] \r\n完整的许可证文本可在以下链接找到:\r\nhttps://opensource.org/licenses/MS-PL\r\n特此向NAudio及其贡献者表示感谢。";
            info_Label5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // info_Label2
            // 
            info_Label2.AutoSize = true;
            info_Label2.Font = new Font("Microsoft JhengHei UI", 15.75F, FontStyle.Bold);
            info_Label2.Location = new Point(167, 40);
            info_Label2.Name = "info_Label2";
            info_Label2.Size = new Size(336, 26);
            info_Label2.TabIndex = 2;
            info_Label2.Text = "版本号:加载中...";
            // 
            // info_Label1
            // 
            info_Label1.AutoSize = true;
            info_Label1.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold | FontStyle.Italic);
            info_Label1.ForeColor = Color.FromArgb(128, 128, 255);
            info_Label1.Location = new Point(167, 6);
            info_Label1.Name = "info_Label1";
            info_Label1.Size = new Size(381, 35);
            info_Label1.TabIndex = 1;
            info_Label1.Text = "Musical Moments - 音乐时刻";
            // 
            // LogoImage
            // 
            LogoImage.BorderStyle = BorderStyle.FixedSingle;
            LogoImage.Image = Properties.Resources.MMLOGO;
            LogoImage.Location = new Point(6, 6);
            LogoImage.Name = "LogoImage";
            LogoImage.Size = new Size(160, 160);
            LogoImage.SizeMode = PictureBoxSizeMode.Zoom;
            LogoImage.TabIndex = 0;
            LogoImage.TabStop = false;
            LogoImage.WaitOnLoad = true;
            LogoImage.Click += LogoImage_Click;
            // 
            // info_Label3
            // 
            info_Label3.AutoSize = true;
            info_Label3.Font = new Font("Microsoft JhengHei UI", 10.5F);
            info_Label3.Location = new Point(172, 72);
            info_Label3.Name = "info_Label3";
            info_Label3.RightToLeft = RightToLeft.No;
            info_Label3.Size = new Size(323, 162);
            info_Label3.TabIndex = 5;
            info_Label3.Text = "版本详情加载中...";
            // 
            // tabPage6
            // 
            tabPage6.AllowDrop = true;
            tabPage6.BackColor = SystemColors.Control;
            tabPage6.Controls.Add(open_help_button1);
            tabPage6.Controls.Add(conversion_Group4);
            tabPage6.Controls.Add(conversion_Group3);
            tabPage6.Controls.Add(conversion_Group2);
            tabPage6.Controls.Add(conversion_Group1);
            tabPage6.Location = new Point(0, 22);
            tabPage6.Name = "tabPage6";
            tabPage6.Padding = new Padding(3);
            tabPage6.Size = new Size(565, 341);
            tabPage6.TabIndex = 5;
            tabPage6.Text = "转换";
            tabPage6.DragDrop += tabPage6_DragDrop;
            tabPage6.DragEnter += tabPage6_DragEnter;
            // 
            // open_help_button1
            // 
            open_help_button1.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            open_help_button1.Location = new Point(4, 310);
            open_help_button1.Name = "open_help_button1";
            open_help_button1.Size = new Size(159, 32);
            open_help_button1.TabIndex = 21;
            open_help_button1.Text = "打开帮助";
            open_help_button1.UseVisualStyleBackColor = true;
            open_help_button1.Click += open_help_button1_Click;
            // 
            // conversion_Group4
            // 
            conversion_Group4.Controls.Add(conversion_Label5);
            conversion_Group4.Controls.Add(conversion_Label4);
            conversion_Group4.ForeColor = Color.FromArgb(90, 90, 90);
            conversion_Group4.Location = new Point(6, 205);
            conversion_Group4.Name = "conversion_Group4";
            conversion_Group4.RightToLeft = RightToLeft.No;
            conversion_Group4.Size = new Size(553, 101);
            conversion_Group4.TabIndex = 5;
            conversion_Group4.TabStop = false;
            conversion_Group4.Text = "属性";
            // 
            // conversion_Label5
            // 
            conversion_Label5.Font = new Font("Microsoft JhengHei UI", 12F);
            conversion_Label5.Location = new Point(6, 62);
            conversion_Label5.Name = "conversion_Label5";
            conversion_Label5.Size = new Size(531, 31);
            conversion_Label5.TabIndex = 4;
            conversion_Label5.Text = "转换一个音频以查看属性";
            // 
            // conversion_Label4
            // 
            conversion_Label4.Font = new Font("Microsoft JhengHei UI", 12F);
            conversion_Label4.Location = new Point(6, 31);
            conversion_Label4.Name = "conversion_Label4";
            conversion_Label4.Size = new Size(531, 31);
            conversion_Label4.TabIndex = 3;
            conversion_Label4.Text = "上传一个音频以查看属性";
            // 
            // conversion_Group3
            // 
            conversion_Group3.Controls.Add(conversion_Label3);
            conversion_Group3.ForeColor = Color.FromArgb(90, 90, 90);
            conversion_Group3.Location = new Point(6, 150);
            conversion_Group3.Name = "conversion_Group3";
            conversion_Group3.RightToLeft = RightToLeft.No;
            conversion_Group3.Size = new Size(553, 49);
            conversion_Group3.TabIndex = 4;
            conversion_Group3.TabStop = false;
            conversion_Group3.Text = "提示";
            // 
            // conversion_Label3
            // 
            conversion_Label3.Location = new Point(6, 21);
            conversion_Label3.Name = "conversion_Label3";
            conversion_Label3.Size = new Size(541, 25);
            conversion_Label3.TabIndex = 3;
            conversion_Label3.Text = "支持被转化的格式:<ncm><mp3><wav><ogg><flac>";
            // 
            // conversion_Group2
            // 
            conversion_Group2.Controls.Add(convert_Button);
            conversion_Group2.Controls.Add(conversion_Label2);
            conversion_Group2.Controls.Add(name_TextBox);
            conversion_Group2.Controls.Add(conversion_Label1);
            conversion_Group2.Controls.Add(comboBoxOutputFormat);
            conversion_Group2.ForeColor = Color.FromArgb(90, 90, 90);
            conversion_Group2.Location = new Point(6, 78);
            conversion_Group2.Name = "conversion_Group2";
            conversion_Group2.RightToLeft = RightToLeft.No;
            conversion_Group2.Size = new Size(553, 66);
            conversion_Group2.TabIndex = 3;
            conversion_Group2.TabStop = false;
            conversion_Group2.Text = "第二步 选择转换格式并转换";
            // 
            // convert_Button
            // 
            convert_Button.Font = new Font("Microsoft JhengHei UI", 9.75F);
            convert_Button.Location = new Point(472, 26);
            convert_Button.Name = "convert_Button";
            convert_Button.Size = new Size(75, 28);
            convert_Button.TabIndex = 6;
            convert_Button.Text = "转换";
            convert_Button.UseVisualStyleBackColor = true;
            convert_Button.Click += convert_Button_Click;
            // 
            // conversion_Label2
            // 
            conversion_Label2.AutoSize = true;
            conversion_Label2.Location = new Point(311, 31);
            conversion_Label2.Name = "conversion_Label2";
            conversion_Label2.Size = new Size(39, 18);
            conversion_Label2.TabIndex = 5;
            conversion_Label2.Text = "格式:";
            // 
            // name_TextBox
            // 
            name_TextBox.BorderStyle = BorderStyle.FixedSingle;
            name_TextBox.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold);
            name_TextBox.ForeColor = Color.FromArgb(90, 90, 90);
            name_TextBox.Location = new Point(51, 26);
            name_TextBox.Name = "name_TextBox";
            name_TextBox.Size = new Size(254, 28);
            name_TextBox.TabIndex = 4;
            // 
            // conversion_Label1
            // 
            conversion_Label1.AutoSize = true;
            conversion_Label1.Location = new Point(6, 31);
            conversion_Label1.Name = "conversion_Label1";
            conversion_Label1.Size = new Size(39, 18);
            conversion_Label1.TabIndex = 3;
            conversion_Label1.Text = "名称:";
            // 
            // comboBoxOutputFormat
            // 
            comboBoxOutputFormat.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold);
            comboBoxOutputFormat.ForeColor = Color.FromArgb(90, 90, 90);
            comboBoxOutputFormat.FormattingEnabled = true;
            comboBoxOutputFormat.Items.AddRange(new object[] { "mp3", "wav" });
            comboBoxOutputFormat.Location = new Point(356, 26);
            comboBoxOutputFormat.Name = "comboBoxOutputFormat";
            comboBoxOutputFormat.Size = new Size(110, 28);
            comboBoxOutputFormat.TabIndex = 2;
            // 
            // conversion_Group1
            // 
            conversion_Group1.Controls.Add(dataPath_TextBox);
            conversion_Group1.Controls.Add(upData_button);
            conversion_Group1.ForeColor = Color.FromArgb(90, 90, 90);
            conversion_Group1.Location = new Point(6, 6);
            conversion_Group1.Name = "conversion_Group1";
            conversion_Group1.RightToLeft = RightToLeft.No;
            conversion_Group1.Size = new Size(553, 66);
            conversion_Group1.TabIndex = 1;
            conversion_Group1.TabStop = false;
            conversion_Group1.Text = "第一步 上传文件或拖拽至此";
            // 
            // dataPath_TextBox
            // 
            dataPath_TextBox.BorderStyle = BorderStyle.FixedSingle;
            dataPath_TextBox.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold);
            dataPath_TextBox.ForeColor = Color.FromArgb(90, 90, 90);
            dataPath_TextBox.Location = new Point(6, 24);
            dataPath_TextBox.Name = "dataPath_TextBox";
            dataPath_TextBox.Size = new Size(460, 28);
            dataPath_TextBox.TabIndex = 1;
            // 
            // upData_button
            // 
            upData_button.Font = new Font("Microsoft JhengHei UI", 9.75F);
            upData_button.Location = new Point(472, 24);
            upData_button.Name = "upData_button";
            upData_button.Size = new Size(75, 28);
            upData_button.TabIndex = 0;
            upData_button.Text = "上传";
            upData_button.UseVisualStyleBackColor = true;
            upData_button.Click += upData_button_Click;
            // 
            // tabPage7
            // 
            tabPage7.BackColor = SystemColors.Control;
            tabPage7.Controls.Add(to_mmdownloader);
            tabPage7.Controls.Add(AudioListView_fd);
            tabPage7.Controls.Add(numberLabel);
            tabPage7.Controls.Add(DownloadSelected);
            tabPage7.Controls.Add(SearchBarTextBox);
            tabPage7.Controls.Add(DownloadLinkListBox);
            tabPage7.Controls.Add(LoadList);
            tabPage7.Location = new Point(0, 22);
            tabPage7.Name = "tabPage7";
            tabPage7.Padding = new Padding(3);
            tabPage7.Size = new Size(565, 341);
            tabPage7.TabIndex = 6;
            tabPage7.Text = "发现";
            // 
            // to_mmdownloader
            // 
            to_mmdownloader.Font = new Font("Microsoft JhengHei UI", 10.5F);
            to_mmdownloader.Location = new Point(436, 71);
            to_mmdownloader.Name = "to_mmdownloader";
            to_mmdownloader.Size = new Size(123, 110);
            to_mmdownloader.TabIndex = 7;
            to_mmdownloader.Text = "前往网页版\r\n\r\n更丝滑更流畅\r\n上传并分享您的音频\r\n";
            to_mmdownloader.UseVisualStyleBackColor = true;
            // 
            // AudioListView_fd
            // 
            AudioListView_fd.AllowDrop = true;
            AudioListView_fd.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader5 });
            AudioListView_fd.Font = new Font("Microsoft JhengHei UI", 9.75F);
            AudioListView_fd.ForeColor = SystemColors.WindowFrame;
            AudioListView_fd.FullRowSelect = true;
            AudioListView_fd.Location = new Point(6, 37);
            AudioListView_fd.Name = "AudioListView_fd";
            AudioListView_fd.RightToLeft = RightToLeft.No;
            AudioListView_fd.Size = new Size(424, 301);
            AudioListView_fd.TabIndex = 6;
            AudioListView_fd.UseCompatibleStateImageBehavior = false;
            AudioListView_fd.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "名称";
            columnHeader1.Width = 265;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "下载次数";
            columnHeader2.Width = 80;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "上传者";
            columnHeader5.Width = 80;
            // 
            // numberLabel
            // 
            numberLabel.AutoSize = true;
            numberLabel.Font = new Font("Microsoft JhengHei UI", 8.25F, FontStyle.Bold);
            numberLabel.Location = new Point(436, 184);
            numberLabel.Name = "numberLabel";
            numberLabel.RightToLeft = RightToLeft.No;
            numberLabel.Size = new Size(51, 14);
            numberLabel.TabIndex = 5;
            numberLabel.Text = "显示数量";
            // 
            // DownloadSelected
            // 
            DownloadSelected.Font = new Font("Microsoft JhengHei UI", 10.5F);
            DownloadSelected.Location = new Point(436, 37);
            DownloadSelected.Name = "DownloadSelected";
            DownloadSelected.Size = new Size(123, 28);
            DownloadSelected.TabIndex = 3;
            DownloadSelected.Text = "下载选中";
            DownloadSelected.UseVisualStyleBackColor = true;
            DownloadSelected.Click += DownloadSelected_Click;
            // 
            // SearchBarTextBox
            // 
            SearchBarTextBox.BorderStyle = BorderStyle.FixedSingle;
            SearchBarTextBox.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold);
            SearchBarTextBox.ForeColor = Color.FromArgb(90, 90, 90);
            SearchBarTextBox.Location = new Point(6, 6);
            SearchBarTextBox.Name = "SearchBarTextBox";
            SearchBarTextBox.RightToLeft = RightToLeft.No;
            SearchBarTextBox.Size = new Size(424, 25);
            SearchBarTextBox.TabIndex = 4;
            SearchBarTextBox.Text = "搜索";
            SearchBarTextBox.TextAlign = HorizontalAlignment.Center;
            SearchBarTextBox.TextChanged += SearchBarTextBox_TextChanged;
            SearchBarTextBox.Enter += SearchBarTextBox_Enter;
            SearchBarTextBox.Leave += SearchBarTextBox_Leave;
            // 
            // DownloadLinkListBox
            // 
            DownloadLinkListBox.Font = new Font("Microsoft JhengHei UI", 8.25F);
            DownloadLinkListBox.ForeColor = Color.FromArgb(90, 90, 90);
            DownloadLinkListBox.FormattingEnabled = true;
            DownloadLinkListBox.Location = new Point(436, 247);
            DownloadLinkListBox.Name = "DownloadLinkListBox";
            DownloadLinkListBox.RightToLeft = RightToLeft.No;
            DownloadLinkListBox.ScrollAlwaysVisible = true;
            DownloadLinkListBox.Size = new Size(123, 88);
            DownloadLinkListBox.TabIndex = 2;
            DownloadLinkListBox.Visible = false;
            // 
            // LoadList
            // 
            LoadList.Font = new Font("Microsoft JhengHei UI", 10.5F);
            LoadList.Location = new Point(436, 6);
            LoadList.Name = "LoadList";
            LoadList.Size = new Size(123, 28);
            LoadList.TabIndex = 1;
            LoadList.Text = "加载/刷新";
            LoadList.UseVisualStyleBackColor = true;
            LoadList.Click += LoadList_Click;
            // 
            // tabPage9
            // 
            tabPage9.BackColor = SystemColors.Control;
            tabPage9.Controls.Add(LoadPlugin);
            tabPage9.Controls.Add(reLoadPluginListsView);
            tabPage9.Controls.Add(PluginServerAddress);
            tabPage9.Controls.Add(mToPluginData);
            tabPage9.Controls.Add(PluginStatus);
            tabPage9.Controls.Add(TogglePluginServer);
            tabPage9.Controls.Add(pluginListView);
            tabPage9.Location = new Point(0, 22);
            tabPage9.Name = "tabPage9";
            tabPage9.Padding = new Padding(3);
            tabPage9.Size = new Size(565, 341);
            tabPage9.TabIndex = 8;
            tabPage9.Text = "插件";
            // 
            // LoadPlugin
            // 
            LoadPlugin.Enabled = false;
            LoadPlugin.Location = new Point(285, 304);
            LoadPlugin.Name = "LoadPlugin";
            LoadPlugin.Size = new Size(75, 31);
            LoadPlugin.TabIndex = 8;
            LoadPlugin.Text = "加载";
            LoadPlugin.UseVisualStyleBackColor = true;
            LoadPlugin.Click += LoadPlugin_Click;
            // 
            // reLoadPluginListsView
            // 
            reLoadPluginListsView.Location = new Point(285, 267);
            reLoadPluginListsView.Name = "reLoadPluginListsView";
            reLoadPluginListsView.Size = new Size(75, 31);
            reLoadPluginListsView.TabIndex = 6;
            reLoadPluginListsView.Text = "刷新";
            reLoadPluginListsView.UseVisualStyleBackColor = true;
            reLoadPluginListsView.Click += reLoadPluginListsView_Click;
            // 
            // PluginServerAddress
            // 
            PluginServerAddress.AutoSize = true;
            PluginServerAddress.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold);
            PluginServerAddress.Location = new Point(6, 298);
            PluginServerAddress.Name = "PluginServerAddress";
            PluginServerAddress.Size = new Size(0, 20);
            PluginServerAddress.TabIndex = 5;
            PluginServerAddress.Click += PluginServerAddress_Click;
            // 
            // mToPluginData
            // 
            mToPluginData.Location = new Point(366, 304);
            mToPluginData.Name = "mToPluginData";
            mToPluginData.Size = new Size(193, 31);
            mToPluginData.TabIndex = 4;
            mToPluginData.Text = "打开插件文件夹";
            mToPluginData.UseVisualStyleBackColor = true;
            mToPluginData.Click += mToPluginData_Click;
            // 
            // PluginStatus
            // 
            PluginStatus.AutoSize = true;
            PluginStatus.Location = new Point(6, 269);
            PluginStatus.Name = "PluginStatus";
            PluginStatus.Size = new Size(109, 18);
            PluginStatus.TabIndex = 3;
            PluginStatus.Text = "插件状态:未启动";
            // 
            // TogglePluginServer
            // 
            TogglePluginServer.Location = new Point(366, 267);
            TogglePluginServer.Name = "TogglePluginServer";
            TogglePluginServer.Size = new Size(193, 31);
            TogglePluginServer.TabIndex = 2;
            TogglePluginServer.Text = "开启插件服务";
            TogglePluginServer.UseVisualStyleBackColor = true;
            TogglePluginServer.Click += TogglePluginServer_Click;
            // 
            // pluginListView
            // 
            pluginListView.AllowDrop = true;
            pluginListView.Columns.AddRange(new ColumnHeader[] { PluginName, PluginAuthor, PluginVer });
            pluginListView.Enabled = false;
            pluginListView.Font = new Font("Microsoft JhengHei UI", 10.5F);
            pluginListView.ForeColor = SystemColors.WindowFrame;
            pluginListView.FullRowSelect = true;
            pluginListView.Location = new Point(6, 6);
            pluginListView.MultiSelect = false;
            pluginListView.Name = "pluginListView";
            pluginListView.RightToLeft = RightToLeft.No;
            pluginListView.Size = new Size(553, 255);
            pluginListView.TabIndex = 1;
            pluginListView.UseCompatibleStateImageBehavior = false;
            pluginListView.View = View.Details;
            // 
            // PluginName
            // 
            PluginName.Text = "插件名称";
            PluginName.Width = 180;
            // 
            // PluginAuthor
            // 
            PluginAuthor.Text = "描述";
            PluginAuthor.Width = 260;
            // 
            // PluginVer
            // 
            PluginVer.Text = "版本";
            PluginVer.Width = 100;
            // 
            // tabPage10
            // 
            tabPage10.BackColor = SystemColors.Control;
            tabPage10.Controls.Add(FeedbackTipsButton);
            tabPage10.Controls.Add(FeedbackContent);
            tabPage10.Controls.Add(FeedbackTips4);
            tabPage10.Controls.Add(FeedbackButton);
            tabPage10.Controls.Add(FeedbackDisaster);
            tabPage10.Controls.Add(FeedbackAverage);
            tabPage10.Controls.Add(FeedbackUrgent);
            tabPage10.Controls.Add(Contact);
            tabPage10.Controls.Add(FeedbackTips3);
            tabPage10.Controls.Add(FeedbackTips2);
            tabPage10.Controls.Add(FeedbackTitle);
            tabPage10.Controls.Add(FeedbackTips1);
            tabPage10.Location = new Point(0, 22);
            tabPage10.Name = "tabPage10";
            tabPage10.Padding = new Padding(3);
            tabPage10.Size = new Size(565, 341);
            tabPage10.TabIndex = 9;
            tabPage10.Text = "反馈";
            // 
            // FeedbackTipsButton
            // 
            FeedbackTipsButton.Location = new Point(333, 272);
            FeedbackTipsButton.Name = "FeedbackTipsButton";
            FeedbackTipsButton.Size = new Size(110, 32);
            FeedbackTipsButton.TabIndex = 14;
            FeedbackTipsButton.Text = "必看";
            FeedbackTipsButton.UseVisualStyleBackColor = true;
            FeedbackTipsButton.Click += FeedbackTipsButton_Click;
            // 
            // FeedbackContent
            // 
            FeedbackContent.BorderStyle = BorderStyle.FixedSingle;
            FeedbackContent.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            FeedbackContent.ForeColor = Color.FromArgb(90, 90, 90);
            FeedbackContent.Location = new Point(59, 41);
            FeedbackContent.Multiline = true;
            FeedbackContent.Name = "FeedbackContent";
            FeedbackContent.RightToLeft = RightToLeft.No;
            FeedbackContent.Size = new Size(500, 181);
            FeedbackContent.TabIndex = 5;
            // 
            // FeedbackTips4
            // 
            FeedbackTips4.Location = new Point(9, 279);
            FeedbackTips4.Name = "FeedbackTips4";
            FeedbackTips4.RightToLeft = RightToLeft.No;
            FeedbackTips4.Size = new Size(67, 31);
            FeedbackTips4.TabIndex = 13;
            FeedbackTips4.Text = "等级:";
            // 
            // FeedbackButton
            // 
            FeedbackButton.Location = new Point(449, 272);
            FeedbackButton.Name = "FeedbackButton";
            FeedbackButton.Size = new Size(110, 32);
            FeedbackButton.TabIndex = 12;
            FeedbackButton.Text = "反馈";
            FeedbackButton.UseVisualStyleBackColor = true;
            FeedbackButton.Click += FeedbackButton_Click;
            // 
            // FeedbackDisaster
            // 
            FeedbackDisaster.AutoSize = true;
            FeedbackDisaster.Location = new Point(202, 278);
            FeedbackDisaster.Name = "FeedbackDisaster";
            FeedbackDisaster.RightToLeft = RightToLeft.No;
            FeedbackDisaster.Size = new Size(54, 22);
            FeedbackDisaster.TabIndex = 11;
            FeedbackDisaster.Text = "灾难";
            FeedbackDisaster.UseVisualStyleBackColor = true;
            // 
            // FeedbackAverage
            // 
            FeedbackAverage.AutoSize = true;
            FeedbackAverage.Checked = true;
            FeedbackAverage.Location = new Point(82, 278);
            FeedbackAverage.Name = "FeedbackAverage";
            FeedbackAverage.RightToLeft = RightToLeft.No;
            FeedbackAverage.Size = new Size(54, 22);
            FeedbackAverage.TabIndex = 10;
            FeedbackAverage.TabStop = true;
            FeedbackAverage.Text = "普通";
            FeedbackAverage.UseVisualStyleBackColor = true;
            // 
            // FeedbackUrgent
            // 
            FeedbackUrgent.AutoSize = true;
            FeedbackUrgent.Location = new Point(142, 278);
            FeedbackUrgent.Name = "FeedbackUrgent";
            FeedbackUrgent.RightToLeft = RightToLeft.No;
            FeedbackUrgent.Size = new Size(54, 22);
            FeedbackUrgent.TabIndex = 9;
            FeedbackUrgent.Text = "紧急";
            FeedbackUrgent.UseVisualStyleBackColor = true;
            // 
            // Contact
            // 
            Contact.BorderStyle = BorderStyle.FixedSingle;
            Contact.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            Contact.ForeColor = Color.FromArgb(90, 90, 90);
            Contact.Location = new Point(59, 233);
            Contact.Name = "Contact";
            Contact.RightToLeft = RightToLeft.No;
            Contact.Size = new Size(500, 26);
            Contact.TabIndex = 7;
            // 
            // FeedbackTips3
            // 
            FeedbackTips3.Font = new Font("Microsoft JhengHei UI", 12F);
            FeedbackTips3.Location = new Point(9, 228);
            FeedbackTips3.Name = "FeedbackTips3";
            FeedbackTips3.RightToLeft = RightToLeft.No;
            FeedbackTips3.Size = new Size(70, 45);
            FeedbackTips3.TabIndex = 8;
            FeedbackTips3.Text = "联系\r\n方式";
            // 
            // FeedbackTips2
            // 
            FeedbackTips2.Location = new Point(9, 43);
            FeedbackTips2.Name = "FeedbackTips2";
            FeedbackTips2.RightToLeft = RightToLeft.No;
            FeedbackTips2.Size = new Size(73, 31);
            FeedbackTips2.TabIndex = 6;
            FeedbackTips2.Text = "内容";
            // 
            // FeedbackTitle
            // 
            FeedbackTitle.BorderStyle = BorderStyle.FixedSingle;
            FeedbackTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            FeedbackTitle.ForeColor = Color.FromArgb(90, 90, 90);
            FeedbackTitle.Location = new Point(59, 7);
            FeedbackTitle.Name = "FeedbackTitle";
            FeedbackTitle.RightToLeft = RightToLeft.No;
            FeedbackTitle.Size = new Size(500, 26);
            FeedbackTitle.TabIndex = 2;
            // 
            // FeedbackTips1
            // 
            FeedbackTips1.Location = new Point(9, 12);
            FeedbackTips1.Name = "FeedbackTips1";
            FeedbackTips1.RightToLeft = RightToLeft.No;
            FeedbackTips1.Size = new Size(73, 31);
            FeedbackTips1.TabIndex = 4;
            FeedbackTips1.Text = "标题";
            // 
            // mainGroupBox
            // 
            mainGroupBox.Controls.Add(mainTabControl);
            mainGroupBox.Font = new Font("Microsoft JhengHei UI", 12F);
            mainGroupBox.ForeColor = Color.FromArgb(90, 90, 90);
            mainGroupBox.Location = new Point(105, 4);
            mainGroupBox.Name = "mainGroupBox";
            mainGroupBox.RightToLeft = RightToLeft.Yes;
            mainGroupBox.Size = new Size(683, 484);
            mainGroupBox.TabIndex = 2;
            mainGroupBox.TabStop = false;
            mainGroupBox.Text = "主页";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(sideLists);
            groupBox1.Location = new Point(12, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(95, 389);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            // 
            // mainContextMenuStrip
            // 
            mainContextMenuStrip.Items.AddRange(new ToolStripItem[] { playSelectedMenuItem, stopPlaybackMenuItem, deleteSelectedMenuItem, renameSelectedMenuItem, setAsPlaybackItemMenuItem, openFileLocationMenuItem, bindKeyMenuItem });
            mainContextMenuStrip.Name = "mainContextMenuStrip";
            mainContextMenuStrip.Size = new Size(173, 158);
            // 
            // playSelectedMenuItem
            // 
            playSelectedMenuItem.Name = "playSelectedMenuItem";
            playSelectedMenuItem.Size = new Size(172, 22);
            playSelectedMenuItem.Text = "播放选择项";
            playSelectedMenuItem.Click += PlaySelectedMenuItem_Click;
            // 
            // stopPlaybackMenuItem
            // 
            stopPlaybackMenuItem.Name = "stopPlaybackMenuItem";
            stopPlaybackMenuItem.Size = new Size(172, 22);
            stopPlaybackMenuItem.Text = "停止播放";
            stopPlaybackMenuItem.Click += StopPlaybackMenuItem_Click;
            // 
            // deleteSelectedMenuItem
            // 
            deleteSelectedMenuItem.Name = "deleteSelectedMenuItem";
            deleteSelectedMenuItem.Size = new Size(172, 22);
            deleteSelectedMenuItem.Text = "删除选择项";
            deleteSelectedMenuItem.Click += DeleteSelectedMenuItem_Click;
            // 
            // renameSelectedMenuItem
            // 
            renameSelectedMenuItem.Name = "renameSelectedMenuItem";
            renameSelectedMenuItem.Size = new Size(172, 22);
            renameSelectedMenuItem.Text = "重命名选择项";
            renameSelectedMenuItem.Click += RenameSelectedMenuItem_Click;
            // 
            // setAsPlaybackItemMenuItem
            // 
            setAsPlaybackItemMenuItem.Name = "setAsPlaybackItemMenuItem";
            setAsPlaybackItemMenuItem.Size = new Size(172, 22);
            setAsPlaybackItemMenuItem.Text = "设为播放项";
            setAsPlaybackItemMenuItem.Click += SetAsPlaybackItemMenuItem_Click;
            // 
            // openFileLocationMenuItem
            // 
            openFileLocationMenuItem.Name = "openFileLocationMenuItem";
            openFileLocationMenuItem.Size = new Size(172, 22);
            openFileLocationMenuItem.Text = "打开文件所在位置";
            openFileLocationMenuItem.Click += OpenFileLocationMenuItem_Click;
            // 
            // bindKeyMenuItem
            // 
            bindKeyMenuItem.Name = "bindKeyMenuItem";
            bindKeyMenuItem.Size = new Size(172, 22);
            bindKeyMenuItem.Text = "绑定按键";
            bindKeyMenuItem.Click += BindKeyMenuItem_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 500);
            Controls.Add(groupBox1);
            Controls.Add(mainGroupBox);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(800, 500);
            Name = "MainWindow";
            FormClosing += MainWindow_FormClosing;
            Load += MainWindow_Load;
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage1.ResumeLayout(false);
            tips_Group1.ResumeLayout(false);
            tips_Group1.PerformLayout();
            mainTabControl.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            playbackBehaviorGroup.ResumeLayout(false);
            volume_Group.ResumeLayout(false);
            volume_Group.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)TipsVolumeTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)VolumeTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)VBVolumeTrackBar).EndInit();
            group_Misc.ResumeLayout(false);
            group_Misc.PerformLayout();
            group_Key.ResumeLayout(false);
            group_Key.PerformLayout();
            group_AudioEquipment.ResumeLayout(false);
            group_AudioEquipment.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)imageAliPay).EndInit();
            ((System.ComponentModel.ISupportInitialize)imageWeChat).EndInit();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            info_Group.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)LogoImage).EndInit();
            tabPage6.ResumeLayout(false);
            conversion_Group4.ResumeLayout(false);
            conversion_Group3.ResumeLayout(false);
            conversion_Group2.ResumeLayout(false);
            conversion_Group2.PerformLayout();
            conversion_Group1.ResumeLayout(false);
            conversion_Group1.PerformLayout();
            tabPage7.ResumeLayout(false);
            tabPage7.PerformLayout();
            tabPage9.ResumeLayout(false);
            tabPage9.PerformLayout();
            tabPage10.ResumeLayout(false);
            tabPage10.PerformLayout();
            mainGroupBox.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            mainContextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion
        private ImageList sideListsImage;
        private TabPage tabPage2;
        private TabPage tabPage1;
        private CustomUI.CoverTabControl mainTabControl;
        private CustomUI.BufferedListView sideLists;
        private GroupBox mainGroupBox;
        private Button retestVB;
        private TabPage tabPage3;
        private GroupBox playbackBehaviorGroup;
        private Label sameAudioBehaviorLabel;
        private ComboBox sameAudioBehaviorComboBox;
        private Label differentAudioBehaviorLabel;
        private ComboBox differentAudioBehaviorComboBox;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private GroupBox groupBox1;
        private PictureBox imageAliPay;
        private PictureBox imageWeChat;
        private Label thankTip;
        private Label label_VBStatus;
        private Label label_AudioEquipment3;
        private Label label_AudioEquipment2;
        private Label label_AudioEquipment1;
        private GroupBox group_AudioEquipment;
        private Label label_AudioEquipment4;
        private ListView audioListView;
        private ColumnHeader AudioName;
        private ColumnHeader AudioTrack;
        private ColumnHeader AudioType;
        private Button reLoadAudioListsView;
        private Label audioSearchLabel;
        private TextBox audioSearchTextBox;
        private Label audioPageStatusLabel;
        private Button audioPrevPageButton;
        private Button audioNextPageButton;
        private ContextMenuStrip mainContextMenuStrip;
        private ToolStripMenuItem playSelectedMenuItem;
        private ToolStripMenuItem deleteSelectedMenuItem;
        private ToolStripMenuItem renameSelectedMenuItem;
        private TextBox ToggleStream;
        private GroupBox group_Key;
        private Label label_Key2;
        private TextBox PlayAudio;
        private Label label_Key1;
        private GroupBox group_Misc;
        private PictureBox LogoImage;
        private Label info_Label1;
        private GroupBox info_Group;
        private Label info_Label5;
        private Label info_Label2;
        private ListBox info_ListBox;
        private Label info_Label3;
        private ToolStripMenuItem setAsPlaybackItemMenuItem;
        private Label SelectedAudioLabel;
        private Button aifadian;
        private GroupBox tips_Group1;
        private Label tips_Label1;
        private Button toVB;
        private Label tips_Label2;
        private Button mToAudioData;
        private Button toSettings;
        private Label tips_Label4;
        private Label tips_Label3;
        private ToolStripMenuItem openFileLocationMenuItem;
        private CheckBox switchStreamTips;
        private GroupBox volume_Group;
        private TrackBar VBVolumeTrackBar;
        private Label volume_Label2;
        private TrackBar VolumeTrackBar;
        private Label volume_Label1;
        private Label volume_Label3;
        private TrackBar TipsVolumeTrackBar;
        private TabPage tabPage6;
        private OpenFileDialog upData;
        private Button upData_button;
        private GroupBox conversion_Group1;
        private TextBox dataPath_TextBox;
        private ComboBox comboBoxOutputFormat;
        private GroupBox conversion_Group2;
        private Button convert_Button;
        private Label conversion_Label2;
        private TextBox name_TextBox;
        private Label conversion_Label1;
        private GroupBox conversion_Group3;
        private Label conversion_Label3;
        private Button mToAudioData1;
        private ToolStripMenuItem stopPlaybackMenuItem;
        private TabPage tabPage7;
        private Button LoadList;
        private ListBox DownloadLinkListBox;
        private Button DownloadSelected;
        private TextBox SearchBarTextBox;
        private Label numberLabel;
        private TabPage tabPage9;
        private ListView pluginListView;
        private ColumnHeader PluginName;
        private ColumnHeader PluginAuthor;
        private ColumnHeader PluginVer;
        private Button TogglePluginServer;
        private Button mToPluginData;
        private Label PluginStatus;
        private Label PluginServerAddress;
        private Button reLoadPluginListsView;
        private Button LoadPlugin;
        private GroupBox conversion_Group4;
        private Label conversion_Label4;
        private Label conversion_Label5;
        private Button toC;
        private Label tips_Label5;
        private ColumnHeader AudioBindKey;
        private ToolStripMenuItem bindKeyMenuItem;
        private TabPage tabPage10;
        private Button FeedbackTipsButton;
        private TextBox FeedbackContent;
        private Label FeedbackTips4;
        private Button FeedbackButton;
        private RadioButton FeedbackDisaster;
        private RadioButton FeedbackAverage;
        private RadioButton FeedbackUrgent;
        private TextBox Contact;
        private Label FeedbackTips3;
        private Label FeedbackTips2;
        private TextBox FeedbackTitle;
        private Label FeedbackTips1;
        public ComboBox comboBox_AudioEquipmentInput;
        public ComboBox comboBox_VBAudioEquipmentOutput;
        public ComboBox comboBox_VBAudioEquipmentInput;
        public ComboBox comboBox_AudioEquipmentOutput;
        public CheckBox audioEquipmentPlay;
        private Button to_audio_page;
        private Label audio_page_tips;
        private ListView AudioListView_fd;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader5;
        private Button to_mmdownloader;
        private Button open_help_window;
        private Label help_tip;
        private Button open_help_button1;
        private Button open_help_button2;
        private Button check_update;
        private CheckBox restoreDefaultsAfterInstallCheckBox;
        private Button autoSelectDevicesButton;
    }
}



namespace MusicalMoments
{
    public partial class MainWindow
    {
        private static readonly Size LegacyMinimumWindowSize = new Size(800, 500);
        private static readonly Size ExpandedDefaultWindowSize = new Size(940, 560);

        private void InitializeResponsiveLayout()
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = LegacyMinimumWindowSize;
            if (Size.Width <= LegacyMinimumWindowSize.Width + 8 && Size.Height <= LegacyMinimumWindowSize.Height + 30)
            {
                Size = ExpandedDefaultWindowSize;
            }

            ConfigureMainContainerLayout();
            ConfigureHomeTabLayout();
            ConfigureAudioTabLayout();
            ConfigureSettingsTabLayout();
            ConfigureDonateTabLayout();
            ConfigureAboutTabLayout();
            ConfigureConversionTabLayout();
            ConfigureDiscoverTabLayout();
            ConfigurePluginTabLayout();
            ConfigureFeedbackTabLayout();

            BindResponsiveEvents();
            ApplyResponsiveLayoutNow();
        }

        private void ConfigureMainContainerLayout()
        {
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            sideLists.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            mainGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }

        private void ConfigureHomeTabLayout()
        {
            tips_Group1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            tips_Label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tips_Label2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tips_Label3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tips_Label4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tips_Label5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label_VBStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            audio_page_tips.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            help_tip.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            toVB.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            toSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            mToAudioData.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            toC.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            retestVB.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            to_audio_page.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            open_help_window.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }

        private void ConfigureAudioTabLayout()
        {
            audioListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            SelectedAudioLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            reLoadAudioListsView.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            mToAudioData1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }

        private void ConfigureSettingsTabLayout()
        {
            group_AudioEquipment.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            group_Key.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            if (playbackBehaviorGroup != null)
            {
                playbackBehaviorGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }
            volume_Group.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            group_Misc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            comboBox_VBAudioEquipmentInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBox_VBAudioEquipmentOutput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBox_AudioEquipmentInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBox_AudioEquipmentOutput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            label_Key1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            ToggleStream.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            label_Key2.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            PlayAudio.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            audioEquipmentPlay.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            switchStreamTips.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            open_help_button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            check_update.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            volume_Label1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            VBVolumeTrackBar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            volume_Label2.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            VolumeTrackBar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            volume_Label3.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            TipsVolumeTrackBar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        }

        private void ConfigureDonateTabLayout()
        {
            imageWeChat.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            imageAliPay.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            thankTip.Anchor = AnchorStyles.Top;
            aifadian.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        }

        private void ConfigureAboutTabLayout()
        {
            LogoImage.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            info_Label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            info_Label2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            info_Label3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            info_Group.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            info_Label5.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            info_ListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
        }

        private void ConfigureConversionTabLayout()
        {
            conversion_Group1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            conversion_Group2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            conversion_Group3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            conversion_Group4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            dataPath_TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            upData_button.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            name_TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            conversion_Label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBoxOutputFormat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            convert_Button.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            open_help_button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        }

        private void ConfigureDiscoverTabLayout()
        {
            SearchBarTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            AudioListView_fd.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            LoadList.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DownloadSelected.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            to_mmdownloader.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numberLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DownloadLinkListBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }

        private void ConfigurePluginTabLayout()
        {
            pluginListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            PluginStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            PluginServerAddress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            reLoadPluginListsView.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LoadPlugin.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TogglePluginServer.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            mToPluginData.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }

        private void ConfigureFeedbackTabLayout()
        {
            FeedbackTips1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            FeedbackTips2.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            FeedbackTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FeedbackContent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            FeedbackTips3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Contact.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            FeedbackUrgent.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            FeedbackAverage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            FeedbackDisaster.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            FeedbackTips4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            FeedbackTipsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            FeedbackButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }

        private void BindResponsiveEvents()
        {
            Resize += (_, _) => UpdateMainContainerLayout();
            tips_Group1.Resize += (_, _) => UpdateGuideLayout();
            audioListView.Resize += (_, _) => UpdateAudioListViewColumns();
            group_Key.Resize += (_, _) => UpdateKeyBindingLayout();
            group_AudioEquipment.Resize += (_, _) => UpdateAudioEquipmentActionLayout();
            volume_Group.Resize += (_, _) => UpdateVolumeGroupLayout();
            group_Misc.Resize += (_, _) => UpdateMiscGroupLayout();
            conversion_Group1.Resize += (_, _) => UpdateConversionImportLayout();
            conversion_Group2.Resize += (_, _) => UpdateConversionFormatLayout();
            AudioListView_fd.Resize += (_, _) => UpdateDiscoverListViewColumns();
            pluginListView.Resize += (_, _) => UpdatePluginListViewColumns();
            sideLists.Resize += (_, _) => UpdateSideNavigationLayout();
            tabPage4.Resize += (_, _) => UpdateDonateTabLayout();
            tabPage3.Resize += (_, _) => ReflowSettingsPanels();
        }

        private void ApplyResponsiveLayoutNow()
        {
            UpdateMainContainerLayout();
            UpdateGuideLayout();
            UpdateAudioListViewColumns();
            UpdateKeyBindingLayout();
            UpdateAudioEquipmentActionLayout();
            UpdateVolumeGroupLayout();
            UpdateMiscGroupLayout();
            UpdateConversionImportLayout();
            UpdateConversionFormatLayout();
            UpdateDiscoverListViewColumns();
            UpdatePluginListViewColumns();
            UpdateSideNavigationLayout();
            UpdateDonateTabLayout();
            ReflowSettingsPanels();
        }

        private void UpdateMainContainerLayout()
        {
            int totalWidth = ClientSize.Width;
            int totalHeight = ClientSize.Height;
            if (totalWidth <= 0 || totalHeight <= 0)
            {
                return;
            }

            int sideMargin = 12;
            int topMargin = 6;
            int bottomMargin = 12;
            int sideBarWidth = 95;
            int gap = 6;

            groupBox1.Location = new Point(sideMargin, topMargin);
            groupBox1.Size = new Size(sideBarWidth, Math.Max(300, totalHeight - topMargin - bottomMargin));

            mainGroupBox.Location = new Point(groupBox1.Right + gap, 4);
            mainGroupBox.Size = new Size(Math.Max(420, totalWidth - mainGroupBox.Left - sideMargin), Math.Max(300, totalHeight - 16));
            UpdateSideNavigationLayout();
        }

        private void UpdateSideNavigationLayout()
        {
            if (sideLists == null || sideLists.IsDisposed || sideLists.Items.Count == 0)
            {
                return;
            }

            int itemCount = sideLists.Items.Count;
            int availableHeight = Math.Max(1, sideLists.ClientSize.Height);
            int preferredRowHeight = availableHeight / itemCount;
            int rowHeight = Math.Clamp(preferredRowHeight, 36, 48);
            int rowWidth = Math.Max(48, sideLists.ClientSize.Width - 2);

            if (sideLists.TileSize.Width != rowWidth || sideLists.TileSize.Height != rowHeight)
            {
                sideLists.TileSize = new Size(rowWidth, rowHeight);
            }
        }

        private void UpdateGuideLayout()
        {
            int rightPadding = 10;
            int leftPadding = 6;
            int buttonWidth = 122;
            int rightButtonLeft = Math.Max(300, tips_Group1.ClientSize.Width - buttonWidth - rightPadding);
            int textWidth = Math.Max(200, rightButtonLeft - leftPadding - 6);

            tips_Label1.Width = textWidth;
            tips_Label2.Width = textWidth;
            tips_Label3.Width = textWidth;
            tips_Label4.Width = textWidth;
            tips_Label5.Width = textWidth;
            label_VBStatus.Width = textWidth;
            audio_page_tips.Width = textWidth;
            help_tip.Width = textWidth;

            toVB.Left = rightButtonLeft;
            toSettings.Left = rightButtonLeft;
            mToAudioData.Left = rightButtonLeft;
            toC.Left = rightButtonLeft;
            retestVB.Left = rightButtonLeft;
            to_audio_page.Left = rightButtonLeft;
            open_help_window.Left = rightButtonLeft;

            if (restoreDefaultsAfterInstallCheckBox != null)
            {
                restoreDefaultsAfterInstallCheckBox.Left = Math.Max(250, rightButtonLeft - restoreDefaultsAfterInstallCheckBox.Width - 8);
            }

            // Keep action buttons above guide labels so clicks are never blocked.
            toVB.BringToFront();
            toSettings.BringToFront();
            mToAudioData.BringToFront();
            toC.BringToFront();
            retestVB.BringToFront();
            to_audio_page.BringToFront();
            open_help_window.BringToFront();
        }

        private static int GetAvailableListWidth(ListView listView)
        {
            return Math.Max(220, listView.ClientSize.Width - 4);
        }

        private void UpdateAudioListViewColumns()
        {
            if (audioListView.Columns.Count != 4)
            {
                return;
            }

            int width = GetAvailableListWidth(audioListView);
            int nameWidth = (int)(width * 0.45);
            int trackWidth = (int)(width * 0.22);
            int typeWidth = (int)(width * 0.14);
            int keyWidth = Math.Max(80, width - nameWidth - trackWidth - typeWidth);

            AudioName.Width = nameWidth;
            AudioTrack.Width = trackWidth;
            AudioType.Width = typeWidth;
            AudioBindKey.Width = keyWidth;
        }

        private void UpdateKeyBindingLayout()
        {
            int leftMargin = 8;
            int gap = 8;

            ToggleStream.Left = 126;
            int rightSectionLeft = ToggleStream.Right + 20;
            label_Key2.Left = rightSectionLeft;
            PlayAudio.Left = label_Key2.Right + gap;

            int rightBoundary = group_Key.ClientSize.Width - 12;
            if (PlayAudio.Right > rightBoundary)
            {
                PlayAudio.Width = Math.Max(90, rightBoundary - PlayAudio.Left);
            }

            if (ToggleStream.Right >= label_Key2.Left - gap)
            {
                label_Key2.Left = ToggleStream.Right + gap;
                PlayAudio.Left = label_Key2.Right + gap;
            }

            label_Key1.Left = leftMargin;
        }

        private void UpdateAudioEquipmentActionLayout()
        {
            if (autoSelectDevicesButton == null)
            {
                return;
            }

            autoSelectDevicesButton.Left = Math.Max(8, group_AudioEquipment.ClientSize.Width - autoSelectDevicesButton.Width - 8);
            autoSelectDevicesButton.Top = 1;
        }

        private void UpdateVolumeGroupLayout()
        {
            int contentWidth = volume_Group.ClientSize.Width - 12;
            if (contentWidth <= 0)
            {
                return;
            }

            const int sectionGap = 8;
            const int labelWidth = 70;
            int sectionWidth = Math.Max(140, (contentWidth - sectionGap * 2) / 3);

            int x1 = 6;
            int x2 = x1 + sectionWidth + sectionGap;
            int x3 = x2 + sectionWidth + sectionGap;

            LayoutVolumeSection(volume_Label1, VBVolumeTrackBar, x1, sectionWidth, labelWidth);
            LayoutVolumeSection(volume_Label2, VolumeTrackBar, x2, sectionWidth, labelWidth);
            LayoutVolumeSection(volume_Label3, TipsVolumeTrackBar, x3, sectionWidth, labelWidth);
        }

        private static void LayoutVolumeSection(Label label, TrackBar trackBar, int sectionLeft, int sectionWidth, int labelWidth)
        {
            label.Left = sectionLeft;
            label.Width = labelWidth;

            trackBar.Left = label.Right + 4;
            trackBar.Width = Math.Max(72, sectionWidth - labelWidth - 4);
        }

        private void UpdateMiscGroupLayout()
        {
            int rightPadding = 8;
            int gap = 6;

            check_update.Left = group_Misc.ClientSize.Width - check_update.Width - rightPadding;
            open_help_button2.Left = check_update.Left - open_help_button2.Width - gap;
        }

        private void UpdateConversionImportLayout()
        {
            int rightPadding = 6;
            int gap = 6;
            upData_button.Left = conversion_Group1.ClientSize.Width - upData_button.Width - rightPadding;
            dataPath_TextBox.Width = Math.Max(140, upData_button.Left - dataPath_TextBox.Left - gap);
        }

        private void UpdateConversionFormatLayout()
        {
            int rightPadding = 6;
            int gap = 6;

            convert_Button.Left = conversion_Group2.ClientSize.Width - convert_Button.Width - rightPadding;
            comboBoxOutputFormat.Left = convert_Button.Left - comboBoxOutputFormat.Width - gap;
            conversion_Label2.Left = comboBoxOutputFormat.Left - conversion_Label2.Width - gap;
            name_TextBox.Width = Math.Max(120, conversion_Label2.Left - name_TextBox.Left - gap);
        }

        private void UpdateDiscoverListViewColumns()
        {
            if (AudioListView_fd.Columns.Count != 3)
            {
                return;
            }

            int width = GetAvailableListWidth(AudioListView_fd);
            int nameWidth = (int)(width * 0.62);
            int countWidth = (int)(width * 0.18);
            int authorWidth = Math.Max(80, width - nameWidth - countWidth);

            columnHeader1.Width = nameWidth;
            columnHeader2.Width = countWidth;
            columnHeader5.Width = authorWidth;
        }

        private void UpdatePluginListViewColumns()
        {
            if (pluginListView.Columns.Count != 3)
            {
                return;
            }

            int width = GetAvailableListWidth(pluginListView);
            int nameWidth = (int)(width * 0.32);
            int descriptionWidth = (int)(width * 0.48);
            int versionWidth = Math.Max(80, width - nameWidth - descriptionWidth);

            PluginName.Width = nameWidth;
            PluginAuthor.Width = descriptionWidth;
            PluginVer.Width = versionWidth;
        }

        private void UpdateDonateTabLayout()
        {
            int margin = 6;
            int gap = 12;
            int availableWidth = tabPage4.ClientSize.Width - margin * 2;
            int qrSize = Math.Min(270, Math.Max(140, (availableWidth - gap) / 2));
            int totalWidth = qrSize * 2 + gap;
            int startX = Math.Max(margin, (tabPage4.ClientSize.Width - totalWidth) / 2);

            imageWeChat.Location = new Point(startX, 6);
            imageWeChat.Size = new Size(qrSize, qrSize);
            imageAliPay.Location = new Point(imageWeChat.Right + gap, 6);
            imageAliPay.Size = new Size(qrSize, qrSize);

            thankTip.Left = Math.Max(6, (tabPage4.ClientSize.Width - thankTip.Width) / 2);
            thankTip.Top = imageWeChat.Bottom + 8;
        }
    }
}



namespace MusicalMoments
{
    public partial class MainWindow
    {
        private bool updatingPlaybackBehaviorUi;

        private void SameAudioBehaviorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (updatingPlaybackBehaviorUi)
            {
                return;
            }

            sameAudioPressBehavior = sameAudioBehaviorComboBox.SelectedIndex == 1
                ? SameAudioPressBehavior.StopPlayback
                : SameAudioPressBehavior.RestartFromBeginning;
        }

        private void DifferentAudioBehaviorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (updatingPlaybackBehaviorUi)
            {
                return;
            }

            differentAudioInterruptBehavior = differentAudioBehaviorComboBox.SelectedIndex == 0
                ? DifferentAudioInterruptBehavior.StopOnly
                : DifferentAudioInterruptBehavior.StopAndPlayNew;
        }

        private void UpdatePlaybackBehaviorToolTip()
        {
            if (sameAudioBehaviorComboBox == null || differentAudioBehaviorComboBox == null)
            {
                return;
            }

            updatingPlaybackBehaviorUi = true;
            try
            {
                sameAudioBehaviorComboBox.SelectedIndex = sameAudioPressBehavior == SameAudioPressBehavior.StopPlayback ? 1 : 0;
                differentAudioBehaviorComboBox.SelectedIndex = differentAudioInterruptBehavior == DifferentAudioInterruptBehavior.StopOnly ? 0 : 1;
            }
            finally
            {
                updatingPlaybackBehaviorUi = false;
            }
        }

        private void ReflowSettingsPanels()
        {
            if (tabPage3 == null
                || group_AudioEquipment == null
                || group_Key == null
                || playbackBehaviorGroup == null
                || volume_Group == null
                || group_Misc == null)
            {
                return;
            }

            const int margin = 6;
            const int gap = 6;

            int contentWidth = Math.Max(260, tabPage3.ClientSize.Width - margin * 2);

            group_AudioEquipment.Left = margin;
            group_AudioEquipment.Top = margin;
            group_AudioEquipment.Width = contentWidth;

            group_Key.Left = margin;
            group_Key.Top = group_AudioEquipment.Bottom + gap;
            group_Key.Width = contentWidth;

            playbackBehaviorGroup.Left = margin;
            playbackBehaviorGroup.Top = group_Key.Bottom + gap;
            playbackBehaviorGroup.Width = contentWidth;

            volume_Group.Left = margin;
            volume_Group.Top = playbackBehaviorGroup.Bottom + gap;
            volume_Group.Width = contentWidth;

            group_Misc.Left = margin;
            group_Misc.Top = volume_Group.Bottom + gap;
            group_Misc.Width = contentWidth;

            int minHeight = group_Misc.Bottom + margin;
            tabPage3.AutoScrollMinSize = new Size(0, minHeight);

            LayoutPlaybackBehaviorControls();
            UpdateVolumeGroupLayout();
            UpdateMiscGroupLayout();
            UpdateAudioEquipmentActionLayout();
        }

        private void LayoutPlaybackBehaviorControls()
        {
            if (playbackBehaviorGroup == null
                || sameAudioBehaviorLabel == null
                || sameAudioBehaviorComboBox == null
                || differentAudioBehaviorLabel == null
                || differentAudioBehaviorComboBox == null)
            {
                return;
            }

            const int left = 10;
            const int top = 27;
            const int gap = 8;

            int totalWidth = Math.Max(280, playbackBehaviorGroup.ClientSize.Width - left * 2);
            int halfWidth = totalWidth / 2;
            int comboMinWidth = 120;

            int sameLabelWidth = 130;
            int sameComboWidth = Math.Max(comboMinWidth, halfWidth - sameLabelWidth - gap);
            int sameSectionWidth = sameLabelWidth + gap + sameComboWidth;

            sameAudioBehaviorLabel.SetBounds(left, top, sameLabelWidth, 24);
            sameAudioBehaviorComboBox.SetBounds(sameAudioBehaviorLabel.Right + gap, top - 1, sameComboWidth, 26);

            int secondLeft = left + sameSectionWidth + 12;
            int secondAvailable = totalWidth - sameSectionWidth - 12;
            int diffLabelWidth = 110;
            int diffComboWidth = Math.Max(comboMinWidth, secondAvailable - diffLabelWidth - gap);
            if (secondLeft + diffLabelWidth + gap + diffComboWidth > playbackBehaviorGroup.ClientSize.Width - left)
            {
                diffComboWidth = Math.Max(comboMinWidth, playbackBehaviorGroup.ClientSize.Width - secondLeft - diffLabelWidth - gap - left);
            }

            differentAudioBehaviorLabel.SetBounds(secondLeft, top, diffLabelWidth, 24);
            differentAudioBehaviorComboBox.SetBounds(differentAudioBehaviorLabel.Right + gap, top - 1, diffComboWidth, 26);
        }
    }
}



namespace MusicalMoments
{
    public partial class MainWindow
    {
        private const int AudioPageSize = 30;

        private readonly List<AudioInfo> allAudioLibraryItems = new List<AudioInfo>();
        private readonly List<AudioInfo> filteredAudioLibraryItems = new List<AudioInfo>();
        private int currentAudioPage = 1;

        private void InitializeAudioPageUx()
        {
            tabPage2.Resize += (_, _) => UpdateAudioPageLayout();
            UpdateAudioPageLayout();
        }

        private void audioSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            ApplyAudioFilterAndPaging(resetToFirstPage: true);
        }

        private void audioPrevPageButton_Click(object sender, EventArgs e)
        {
            if (currentAudioPage > 1)
            {
                currentAudioPage -= 1;
                RenderCurrentAudioPage();
            }
        }

        private void audioNextPageButton_Click(object sender, EventArgs e)
        {
            int totalPages = GetAudioTotalPages();
            if (currentAudioPage < totalPages)
            {
                currentAudioPage += 1;
                RenderCurrentAudioPage();
            }
        }

        private void UpdateAudioPageLayout()
        {
            if (tabPage2 == null || tabPage2.IsDisposed)
            {
                return;
            }

            const int margin = 6;
            const int gap = 6;
            const int bottomButtonHeight = 32;
            const int searchHeight = 27;
            const int pagerWidth = 78;

            int contentWidth = tabPage2.ClientSize.Width;
            int contentHeight = tabPage2.ClientSize.Height;
            if (contentWidth <= 0 || contentHeight <= 0)
            {
                return;
            }

            audioSearchLabel.Left = margin;
            audioSearchLabel.Top = margin + 4;

            audioSearchTextBox.Left = audioSearchLabel.Right + gap;
            audioSearchTextBox.Top = margin;
            audioSearchTextBox.Width = Math.Max(180, contentWidth - audioSearchTextBox.Left - margin);
            audioSearchTextBox.Height = searchHeight;

            int bottomY = contentHeight - margin - bottomButtonHeight;

            mToAudioData1.Top = bottomY;
            mToAudioData1.Left = contentWidth - margin - mToAudioData1.Width;

            reLoadAudioListsView.Top = bottomY;
            reLoadAudioListsView.Left = mToAudioData1.Left - gap - reLoadAudioListsView.Width;

            audioNextPageButton.Top = bottomY;
            audioNextPageButton.Width = pagerWidth;
            audioNextPageButton.Height = bottomButtonHeight;
            audioNextPageButton.Left = reLoadAudioListsView.Left - gap - audioNextPageButton.Width;

            audioPrevPageButton.Top = bottomY;
            audioPrevPageButton.Width = pagerWidth;
            audioPrevPageButton.Height = bottomButtonHeight;
            audioPrevPageButton.Left = audioNextPageButton.Left - gap - audioPrevPageButton.Width;

            audioPageStatusLabel.Left = margin;
            audioPageStatusLabel.Top = bottomY + 2;
            audioPageStatusLabel.Width = Math.Max(180, audioPrevPageButton.Left - margin - gap);
            audioPageStatusLabel.Height = bottomButtonHeight - 4;
            audioPageStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            SelectedAudioLabel.Left = margin;
            SelectedAudioLabel.Top = bottomY - 20;
            SelectedAudioLabel.Width = Math.Max(180, contentWidth - margin * 2);

            audioListView.Left = margin;
            audioListView.Top = audioSearchTextBox.Bottom + gap;
            audioListView.Width = Math.Max(220, contentWidth - margin * 2);
            audioListView.Height = Math.Max(80, SelectedAudioLabel.Top - gap - audioListView.Top);

            UpdateAudioListViewColumns();
        }

        private async Task ReloadAudioLibraryAsync()
        {
            string audioDirectory = Path.Combine(runningDirectory, "AudioData");
            Directory.CreateDirectory(audioDirectory);

            List<AudioInfo> loaded = await Task.Run(() => AudioLibraryService.LoadAudioInfos(audioDirectory));
            allAudioLibraryItems.Clear();
            allAudioLibraryItems.AddRange(loaded);

            audioInfo.Clear();
            audioInfo.AddRange(loaded.Select(item => new AudioInfo
            {
                Name = item.Name,
                Track = item.Track,
                FileType = item.FileType,
                FilePath = item.FilePath,
                Key = item.Key
            }));

            ApplyAudioFilterAndPaging(resetToFirstPage: false);
        }

        private void ApplyAudioFilterAndPaging(bool resetToFirstPage)
        {
            if (audioSearchTextBox == null)
            {
                return;
            }

            string keyword = (audioSearchTextBox.Text ?? string.Empty).Trim();
            filteredAudioLibraryItems.Clear();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                filteredAudioLibraryItems.AddRange(allAudioLibraryItems);
            }
            else
            {
                filteredAudioLibraryItems.AddRange(allAudioLibraryItems.Where(item =>
                    ContainsIgnoreCase(item.Name, keyword)
                    || ContainsIgnoreCase(item.Track, keyword)
                    || ContainsIgnoreCase(item.FileType, keyword)
                    || ContainsIgnoreCase(item.Key.ToString(), keyword)
                    || ContainsIgnoreCase(Path.GetFileName(item.FilePath), keyword)));
            }

            if (resetToFirstPage)
            {
                currentAudioPage = 1;
            }

            int totalPages = GetAudioTotalPages();
            currentAudioPage = Math.Clamp(currentAudioPage, 1, totalPages);
            RenderCurrentAudioPage();
        }

        private void RenderCurrentAudioPage()
        {
            if (audioListView == null)
            {
                return;
            }

            int totalPages = GetAudioTotalPages();
            int skip = (currentAudioPage - 1) * AudioPageSize;
            List<AudioInfo> pageItems = filteredAudioLibraryItems
                .Skip(skip)
                .Take(AudioPageSize)
                .ToList();

            audioListView.BeginUpdate();
            audioListView.Items.Clear();
            foreach (AudioInfo item in pageItems)
            {
                string keyText = item.Key == Keys.None ? "未绑定" : item.Key.ToString();
                ListViewItem row = new ListViewItem(new[] { item.Name, item.Track, item.FileType, keyText })
                {
                    Tag = item.FilePath
                };
                audioListView.Items.Add(row);
            }

            audioListView.EndUpdate();

            audioPageStatusLabel.Text = $"第 {currentAudioPage} / {totalPages} 页，共 {filteredAudioLibraryItems.Count} 条";
            audioPrevPageButton.Enabled = currentAudioPage > 1;
            audioNextPageButton.Enabled = currentAudioPage < totalPages;

            if (!string.IsNullOrWhiteSpace(selectedAudioPath) && File.Exists(selectedAudioPath))
            {
                SelectedAudioLabel.Text = $"已选择:{Path.GetFileNameWithoutExtension(selectedAudioPath)}";
            }
            else
            {
                SelectedAudioLabel.Text = "已选择:";
            }
        }

        private int GetAudioTotalPages()
        {
            int count = filteredAudioLibraryItems.Count;
            return Math.Max(1, (int)Math.Ceiling(count / (double)AudioPageSize));
        }

        private static bool ContainsIgnoreCase(string source, string keyword)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(keyword))
            {
                return false;
            }

            return source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}


