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
            sideLists = new CustomUI.BufferedListView();
            sideListsImage = new ImageList(components);
            tabPage2 = new TabPage();
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
            mainTabControl = new CustomUI.CoverTabControl();
            tabPage3 = new TabPage();
            volume_Group = new GroupBox();
            volume_Label3 = new Label();
            TipsVolumeTrackBar = new TrackBar();
            volume_Label2 = new Label();
            VolumeTrackBar = new TrackBar();
            volume_Label1 = new Label();
            VBVolumeTrackBar = new TrackBar();
            group_Misc = new GroupBox();
            Languagelabel = new Label();
            languageComboBox = new ComboBox();
            switchStreamTips = new CheckBox();
            audioEquipmentPlay = new CheckBox();
            group_Key = new GroupBox();
            label_Key2 = new Label();
            PlayAudio = new TextBox();
            label_Key1 = new Label();
            ToggleStream = new TextBox();
            group_AudioEquipment = new GroupBox();
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
            audioTips = new Button();
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
            播放ToolStripMenuItem = new ToolStripMenuItem();
            停止播放ToolStripMenuItem = new ToolStripMenuItem();
            删除ToolStripMenuItem = new ToolStripMenuItem();
            重命名ToolStripMenuItem = new ToolStripMenuItem();
            设为播放项ToolStripMenuItem = new ToolStripMenuItem();
            打开文件所在位置ToolStripMenuItem = new ToolStripMenuItem();
            绑定按键ToolStripMenuItem = new ToolStripMenuItem();
            upData = new OpenFileDialog();
            tabPage2.SuspendLayout();
            tabPage1.SuspendLayout();
            tips_Group1.SuspendLayout();
            mainTabControl.SuspendLayout();
            tabPage3.SuspendLayout();
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
            sideLists.Font = new Font("Microsoft JhengHei UI", 13F, FontStyle.Regular, GraphicsUnit.Point);
            sideLists.ForeColor = Color.FromArgb(90, 90, 90);
            sideLists.Items.AddRange(new ListViewItem[] { listViewItem10, listViewItem11, listViewItem12, listViewItem13, listViewItem14, listViewItem15, listViewItem16, listViewItem17, listViewItem18 });
            sideLists.LargeImageList = sideListsImage;
            sideLists.Location = new Point(3, 16);
            sideLists.Name = "sideLists";
            sideLists.OwnerDraw = true;
            sideLists.Scrollable = false;
            sideLists.Size = new Size(78, 363);
            sideLists.SmallImageList = sideListsImage;
            sideLists.TabIndex = 0;
            sideLists.UseCompatibleStateImageBehavior = false;
            sideLists.View = View.List;
            sideLists.DrawItem += sideLists_DrawItem;
            sideLists.SelectedIndexChanged += sideLists_SelectedIndexChanged;
            // 
            // sideListsImage
            // 
            sideListsImage.ColorDepth = ColorDepth.Depth32Bit;
            sideListsImage.ImageStream = (ImageListStreamer)resources.GetObject("sideListsImage.ImageStream");
            sideListsImage.TransparentColor = Color.Transparent;
            sideListsImage.Images.SetKeyName(0, "主页.png");
            sideListsImage.Images.SetKeyName(1, "音频.png");
            sideListsImage.Images.SetKeyName(2, "设置.png");
            sideListsImage.Images.SetKeyName(3, "赞助.png");
            sideListsImage.Images.SetKeyName(4, "关于.png");
            sideListsImage.Images.SetKeyName(5, "转换.png");
            sideListsImage.Images.SetKeyName(6, "发现.png");
            sideListsImage.Images.SetKeyName(7, "插件.png");
            sideListsImage.Images.SetKeyName(8, "反馈.png");
            // 
            // tabPage2
            // 
            tabPage2.BackColor = SystemColors.Control;
            tabPage2.Controls.Add(mToAudioData1);
            tabPage2.Controls.Add(SelectedAudioLabel);
            tabPage2.Controls.Add(reLoadAudioListsView);
            tabPage2.Controls.Add(audioListView);
            tabPage2.Location = new Point(0, 22);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(565, 341);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "音频";
            // 
            // mToAudioData1
            // 
            mToAudioData1.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            mToAudioData1.Location = new Point(399, 303);
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
            SelectedAudioLabel.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            SelectedAudioLabel.Location = new Point(6, 303);
            SelectedAudioLabel.Name = "SelectedAudioLabel";
            SelectedAudioLabel.RightToLeft = RightToLeft.No;
            SelectedAudioLabel.Size = new Size(50, 17);
            SelectedAudioLabel.TabIndex = 2;
            SelectedAudioLabel.Text = "已选择:";
            // 
            // reLoadAudioListsView
            // 
            reLoadAudioListsView.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            reLoadAudioListsView.Location = new Point(317, 303);
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
            audioListView.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            audioListView.ForeColor = SystemColors.WindowFrame;
            audioListView.Location = new Point(6, 6);
            audioListView.Name = "audioListView";
            audioListView.RightToLeft = RightToLeft.No;
            audioListView.Size = new Size(553, 291);
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
            tabPage1.Size = new Size(565, 341);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "主页";
            // 
            // tips_Group1
            // 
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
            tips_Group1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point);
            tips_Group1.ForeColor = Color.FromArgb(90, 90, 90);
            tips_Group1.Location = new Point(0, -10);
            tips_Group1.Name = "tips_Group1";
            tips_Group1.RightToLeft = RightToLeft.No;
            tips_Group1.Size = new Size(565, 334);
            tips_Group1.TabIndex = 11;
            tips_Group1.TabStop = false;
            // 
            // to_audio_page
            // 
            to_audio_page.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            to_audio_page.Location = new Point(458, 246);
            to_audio_page.Name = "to_audio_page";
            to_audio_page.Size = new Size(97, 25);
            to_audio_page.TabIndex = 18;
            to_audio_page.Text = "前往尝试";
            to_audio_page.UseVisualStyleBackColor = true;
            // 
            // audio_page_tips
            // 
            audio_page_tips.BorderStyle = BorderStyle.FixedSingle;
            audio_page_tips.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            audio_page_tips.Location = new Point(6, 242);
            audio_page_tips.Name = "audio_page_tips";
            audio_page_tips.Size = new Size(553, 34);
            audio_page_tips.TabIndex = 19;
            audio_page_tips.Text = "在音频页对音频右键可绑定按键或执行其他功能";
            // 
            // toC
            // 
            toC.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            toC.Location = new Point(458, 170);
            toC.Name = "toC";
            toC.Size = new Size(97, 25);
            toC.TabIndex = 17;
            toC.Text = "前往转换";
            toC.UseVisualStyleBackColor = true;
            toC.Click += toC_Click;
            // 
            // tips_Label5
            // 
            tips_Label5.BorderStyle = BorderStyle.FixedSingle;
            tips_Label5.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            tips_Label5.Location = new Point(6, 166);
            tips_Label5.Name = "tips_Label5";
            tips_Label5.Size = new Size(553, 34);
            tips_Label5.TabIndex = 16;
            tips_Label5.Text = "4.出现电音请先转换音频为指定格式";
            // 
            // mToAudioData
            // 
            mToAudioData.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            mToAudioData.Location = new Point(458, 132);
            mToAudioData.Name = "mToAudioData";
            mToAudioData.Size = new Size(97, 25);
            mToAudioData.TabIndex = 15;
            mToAudioData.Text = "点我打开";
            mToAudioData.UseVisualStyleBackColor = true;
            mToAudioData.Click += mToAudioData_Click;
            // 
            // retestVB
            // 
            retestVB.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            retestVB.Location = new Point(458, 208);
            retestVB.Name = "retestVB";
            retestVB.Size = new Size(97, 25);
            retestVB.TabIndex = 0;
            retestVB.Text = "重新检测";
            retestVB.UseVisualStyleBackColor = true;
            retestVB.Click += retestVB_Click;
            // 
            // label_VBStatus
            // 
            label_VBStatus.BorderStyle = BorderStyle.FixedSingle;
            label_VBStatus.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label_VBStatus.Location = new Point(6, 204);
            label_VBStatus.Name = "label_VBStatus";
            label_VBStatus.Size = new Size(553, 34);
            label_VBStatus.TabIndex = 1;
            label_VBStatus.Text = "VB声卡状态";
            // 
            // toSettings
            // 
            toSettings.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            toSettings.Location = new Point(458, 94);
            toSettings.Name = "toSettings";
            toSettings.Size = new Size(97, 25);
            toSettings.TabIndex = 14;
            toSettings.Text = "前往绑定";
            toSettings.UseVisualStyleBackColor = true;
            toSettings.Click += toSettings_Click;
            // 
            // tips_Label4
            // 
            tips_Label4.BorderStyle = BorderStyle.FixedSingle;
            tips_Label4.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            tips_Label4.Location = new Point(6, 128);
            tips_Label4.Name = "tips_Label4";
            tips_Label4.Size = new Size(553, 34);
            tips_Label4.TabIndex = 13;
            tips_Label4.Text = "3.音频放置位置位于根目录的[AudioData]";
            // 
            // tips_Label3
            // 
            tips_Label3.BorderStyle = BorderStyle.FixedSingle;
            tips_Label3.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            tips_Label3.Location = new Point(6, 90);
            tips_Label3.Name = "tips_Label3";
            tips_Label3.Size = new Size(553, 34);
            tips_Label3.TabIndex = 12;
            tips_Label3.Text = "2.安装后需在设置中绑定对应设备";
            // 
            // toVB
            // 
            toVB.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            toVB.Location = new Point(458, 55);
            toVB.Name = "toVB";
            toVB.Size = new Size(97, 25);
            toVB.TabIndex = 11;
            toVB.Text = "点我下载";
            toVB.UseVisualStyleBackColor = true;
            toVB.Click += toVB_Click;
            // 
            // tips_Label2
            // 
            tips_Label2.BorderStyle = BorderStyle.FixedSingle;
            tips_Label2.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            tips_Label2.Location = new Point(6, 51);
            tips_Label2.Name = "tips_Label2";
            tips_Label2.Size = new Size(553, 34);
            tips_Label2.TabIndex = 10;
            tips_Label2.Text = "1.首次使用前需安装VB声卡";
            // 
            // tips_Label1
            // 
            tips_Label1.Font = new Font("Microsoft JhengHei UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point);
            tips_Label1.Location = new Point(6, 13);
            tips_Label1.Name = "tips_Label1";
            tips_Label1.Size = new Size(553, 34);
            tips_Label1.TabIndex = 9;
            tips_Label1.Text = "首次使用须知";
            tips_Label1.TextAlign = ContentAlignment.TopCenter;
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
            mainTabControl.Font = new Font("Microsoft JhengHei UI", 10.25F, FontStyle.Regular, GraphicsUnit.Point);
            mainTabControl.ItemSize = new Size(62, 22);
            mainTabControl.Location = new Point(6, 22);
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(565, 363);
            mainTabControl.SizeMode = TabSizeMode.Fixed;
            mainTabControl.TabIndex = 1;
            mainTabControl.TabStop = false;
            // 
            // tabPage3
            // 
            tabPage3.BackColor = SystemColors.Control;
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
            // volume_Group
            // 
            volume_Group.Controls.Add(volume_Label3);
            volume_Group.Controls.Add(TipsVolumeTrackBar);
            volume_Group.Controls.Add(volume_Label2);
            volume_Group.Controls.Add(VolumeTrackBar);
            volume_Group.Controls.Add(volume_Label1);
            volume_Group.Controls.Add(VBVolumeTrackBar);
            volume_Group.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
            volume_Group.ForeColor = Color.FromArgb(90, 90, 90);
            volume_Group.Location = new Point(6, 227);
            volume_Group.Name = "volume_Group";
            volume_Group.RightToLeft = RightToLeft.No;
            volume_Group.Size = new Size(553, 71);
            volume_Group.TabIndex = 12;
            volume_Group.TabStop = false;
            volume_Group.Text = "音量";
            // 
            // volume_Label3
            // 
            volume_Label3.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
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
            volume_Label2.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
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
            volume_Label1.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
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
            group_Misc.Controls.Add(Languagelabel);
            group_Misc.Controls.Add(languageComboBox);
            group_Misc.Controls.Add(switchStreamTips);
            group_Misc.Controls.Add(audioEquipmentPlay);
            group_Misc.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
            group_Misc.ForeColor = Color.FromArgb(90, 90, 90);
            group_Misc.Location = new Point(6, 304);
            group_Misc.Name = "group_Misc";
            group_Misc.RightToLeft = RightToLeft.No;
            group_Misc.Size = new Size(553, 46);
            group_Misc.TabIndex = 11;
            group_Misc.TabStop = false;
            group_Misc.Text = "其他设置";
            // 
            // Languagelabel
            // 
            Languagelabel.AutoSize = true;
            Languagelabel.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
            Languagelabel.Location = new Point(312, 19);
            Languagelabel.Name = "Languagelabel";
            Languagelabel.Size = new Size(40, 18);
            Languagelabel.TabIndex = 12;
            Languagelabel.Text = "语言:";
            // 
            // languageComboBox
            // 
            languageComboBox.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            languageComboBox.ForeColor = Color.FromArgb(90, 90, 90);
            languageComboBox.FormattingEnabled = true;
            languageComboBox.Items.AddRange(new object[] { "简体中文", "English", "日本語" });
            languageComboBox.Location = new Point(363, 15);
            languageComboBox.Name = "languageComboBox";
            languageComboBox.Size = new Size(184, 25);
            languageComboBox.TabIndex = 2;
            languageComboBox.Text = "简体中文";
            languageComboBox.SelectedIndexChanged += languageComboBox_SelectedIndexChanged;
            // 
            // switchStreamTips
            // 
            switchStreamTips.AutoSize = true;
            switchStreamTips.Checked = true;
            switchStreamTips.CheckState = CheckState.Checked;
            switchStreamTips.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
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
            audioEquipmentPlay.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
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
            group_Key.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
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
            label_Key2.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label_Key2.Location = new Point(267, 23);
            label_Key2.Name = "label_Key2";
            label_Key2.Size = new Size(124, 24);
            label_Key2.TabIndex = 11;
            label_Key2.Text = "播放音频按键";
            // 
            // PlayAudio
            // 
            PlayAudio.BorderStyle = BorderStyle.FixedSingle;
            PlayAudio.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
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
            // 
            // label_Key1
            // 
            label_Key1.AutoSize = true;
            label_Key1.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label_Key1.Location = new Point(8, 24);
            label_Key1.Name = "label_Key1";
            label_Key1.Size = new Size(105, 24);
            label_Key1.TabIndex = 9;
            label_Key1.Text = "切换源按键";
            // 
            // ToggleStream
            // 
            ToggleStream.BorderStyle = BorderStyle.FixedSingle;
            ToggleStream.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
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
            // 
            // group_AudioEquipment
            // 
            group_AudioEquipment.Controls.Add(comboBox_AudioEquipmentOutput);
            group_AudioEquipment.Controls.Add(label_AudioEquipment4);
            group_AudioEquipment.Controls.Add(label_AudioEquipment1);
            group_AudioEquipment.Controls.Add(comboBox_AudioEquipmentInput);
            group_AudioEquipment.Controls.Add(label_AudioEquipment2);
            group_AudioEquipment.Controls.Add(comboBox_VBAudioEquipmentOutput);
            group_AudioEquipment.Controls.Add(label_AudioEquipment3);
            group_AudioEquipment.Controls.Add(comboBox_VBAudioEquipmentInput);
            group_AudioEquipment.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
            group_AudioEquipment.ForeColor = Color.FromArgb(90, 90, 90);
            group_AudioEquipment.Location = new Point(6, 6);
            group_AudioEquipment.Name = "group_AudioEquipment";
            group_AudioEquipment.RightToLeft = RightToLeft.No;
            group_AudioEquipment.Size = new Size(553, 151);
            group_AudioEquipment.TabIndex = 6;
            group_AudioEquipment.TabStop = false;
            group_AudioEquipment.Text = "音频设备";
            // 
            // comboBox_AudioEquipmentOutput
            // 
            comboBox_AudioEquipmentOutput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_AudioEquipmentOutput.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
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
            label_AudioEquipment4.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label_AudioEquipment4.Location = new Point(8, 117);
            label_AudioEquipment4.Name = "label_AudioEquipment4";
            label_AudioEquipment4.Size = new Size(105, 24);
            label_AudioEquipment4.TabIndex = 6;
            label_AudioEquipment4.Text = "物理扬声器";
            // 
            // label_AudioEquipment1
            // 
            label_AudioEquipment1.AutoSize = true;
            label_AudioEquipment1.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label_AudioEquipment1.Location = new Point(8, 24);
            label_AudioEquipment1.Name = "label_AudioEquipment1";
            label_AudioEquipment1.Size = new Size(111, 24);
            label_AudioEquipment1.TabIndex = 0;
            label_AudioEquipment1.Text = "VB声卡输入";
            // 
            // comboBox_AudioEquipmentInput
            // 
            comboBox_AudioEquipmentInput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_AudioEquipmentInput.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
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
            label_AudioEquipment2.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label_AudioEquipment2.Location = new Point(6, 55);
            label_AudioEquipment2.Name = "label_AudioEquipment2";
            label_AudioEquipment2.Size = new Size(111, 24);
            label_AudioEquipment2.TabIndex = 1;
            label_AudioEquipment2.Text = "VB声卡输出";
            // 
            // comboBox_VBAudioEquipmentOutput
            // 
            comboBox_VBAudioEquipmentOutput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_VBAudioEquipmentOutput.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
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
            label_AudioEquipment3.Font = new Font("Microsoft JhengHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label_AudioEquipment3.Location = new Point(8, 86);
            label_AudioEquipment3.Name = "label_AudioEquipment3";
            label_AudioEquipment3.Size = new Size(105, 24);
            label_AudioEquipment3.TabIndex = 2;
            label_AudioEquipment3.Text = "物理麦克风";
            // 
            // comboBox_VBAudioEquipmentInput
            // 
            comboBox_VBAudioEquipmentInput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_VBAudioEquipmentInput.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
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
            aifadian.Font = new Font("Microsoft JhengHei UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
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
            imageAliPay.Image = Properties.Resources.支付宝;
            imageAliPay.Location = new Point(289, 6);
            imageAliPay.Name = "imageAliPay";
            imageAliPay.Size = new Size(270, 270);
            imageAliPay.SizeMode = PictureBoxSizeMode.StretchImage;
            imageAliPay.TabIndex = 4;
            imageAliPay.TabStop = false;
            // 
            // imageWeChat
            // 
            imageWeChat.Image = Properties.Resources.微信;
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
            thankTip.Font = new Font("Microsoft JhengHei UI", 24F, FontStyle.Bold, GraphicsUnit.Point);
            thankTip.Location = new Point(179, 305);
            thankTip.Name = "thankTip";
            thankTip.Size = new Size(210, 41);
            thankTip.TabIndex = 5;
            thankTip.Text = "感谢您的支持";
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
            info_Group.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            info_Group.ForeColor = Color.FromArgb(90, 90, 90);
            info_Group.Location = new Point(5, 172);
            info_Group.Name = "info_Group";
            info_Group.RightToLeft = RightToLeft.No;
            info_Group.Size = new Size(557, 190);
            info_Group.TabIndex = 4;
            info_Group.TabStop = false;
            info_Group.Text = "引用许可证";
            // 
            // info_ListBox
            // 
            info_ListBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            info_ListBox.BackColor = SystemColors.Control;
            info_ListBox.Font = new Font("Microsoft JhengHei UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            info_ListBox.ForeColor = Color.FromArgb(90, 90, 90);
            info_ListBox.FormattingEnabled = true;
            info_ListBox.ItemHeight = 14;
            info_ListBox.Items.AddRange(new object[] { "NAudio", "Newtonsoft.Json", "System.Management", "taglib-sharp-netstandard2.0", "MouseKeyHook", "MediaToolkit", "HtmlAgilityPack" });
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
            info_Label5.Font = new Font("Microsoft JhengHei UI Light", 12F, FontStyle.Regular, GraphicsUnit.Point);
            info_Label5.ForeColor = Color.FromArgb(90, 90, 90);
            info_Label5.Location = new Point(6, 16);
            info_Label5.Name = "info_Label5";
            info_Label5.RightToLeft = RightToLeft.No;
            info_Label5.Size = new Size(377, 157);
            info_Label5.TabIndex = 3;
            info_Label5.Text = "MM使用了NAudio音频处理库。\r\nNAudio遵循Microsoft Public License (Ms-PL)。\r\n版权所有 (c) [NAudio] \r\n完整的许可证文本可在以下链接找到:\r\nhttps://opensource.org/licenses/MS-PL\r\n特此向NAudio及其贡献者表示感谢。";
            info_Label5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // info_Label2
            // 
            info_Label2.AutoSize = true;
            info_Label2.Font = new Font("Microsoft JhengHei UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            info_Label2.Location = new Point(167, 40);
            info_Label2.Name = "info_Label2";
            info_Label2.Size = new Size(336, 26);
            info_Label2.TabIndex = 2;
            info_Label2.Text = "版本号:1.4.0-Release+20250129";
            // 
            // info_Label1
            // 
            info_Label1.AutoSize = true;
            info_Label1.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
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
            info_Label3.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
            info_Label3.Location = new Point(172, 72);
            info_Label3.Name = "info_Label3";
            info_Label3.RightToLeft = RightToLeft.No;
            info_Label3.Size = new Size(323, 162);
            info_Label3.TabIndex = 5;
            info_Label3.Text = "- 主版本号（Major Version）：1\r\n- 次版本号（Minor Version）：4\r\n- 修订号（Patch Version）：0\r\n- 预发布版本号（Pre-release Version）：Release\r\n- 构建号（Build Number）：20250129\r\n\r\n\r\n\r\n\r\n";
            // 
            // tabPage6
            // 
            tabPage6.AllowDrop = true;
            tabPage6.BackColor = SystemColors.Control;
            tabPage6.Controls.Add(audioTips);
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
            // audioTips
            // 
            audioTips.Location = new Point(6, 312);
            audioTips.Name = "audioTips";
            audioTips.Size = new Size(152, 32);
            audioTips.TabIndex = 6;
            audioTips.Text = "关于电音";
            audioTips.UseVisualStyleBackColor = true;
            audioTips.Click += audioTips_Click;
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
            conversion_Label5.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            conversion_Label5.Location = new Point(6, 62);
            conversion_Label5.Name = "conversion_Label5";
            conversion_Label5.Size = new Size(531, 31);
            conversion_Label5.TabIndex = 4;
            conversion_Label5.Text = "转换一个音频以查看属性";
            // 
            // conversion_Label4
            // 
            conversion_Label4.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
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
            convert_Button.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
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
            name_TextBox.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            comboBoxOutputFormat.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            dataPath_TextBox.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            dataPath_TextBox.ForeColor = Color.FromArgb(90, 90, 90);
            dataPath_TextBox.Location = new Point(6, 24);
            dataPath_TextBox.Name = "dataPath_TextBox";
            dataPath_TextBox.Size = new Size(460, 28);
            dataPath_TextBox.TabIndex = 1;
            // 
            // upData_button
            // 
            upData_button.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
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
            to_mmdownloader.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
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
            AudioListView_fd.Font = new Font("Microsoft JhengHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            AudioListView_fd.ForeColor = SystemColors.WindowFrame;
            AudioListView_fd.HideSelection = true;
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
            numberLabel.Font = new Font("Microsoft JhengHei UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
            numberLabel.Location = new Point(436, 184);
            numberLabel.Name = "numberLabel";
            numberLabel.RightToLeft = RightToLeft.No;
            numberLabel.Size = new Size(51, 14);
            numberLabel.TabIndex = 5;
            numberLabel.Text = "显示数量";
            // 
            // DownloadSelected
            // 
            DownloadSelected.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
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
            SearchBarTextBox.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
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
            DownloadLinkListBox.Font = new Font("Microsoft JhengHei UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            DownloadLinkListBox.ForeColor = Color.FromArgb(90, 90, 90);
            DownloadLinkListBox.FormattingEnabled = true;
            DownloadLinkListBox.ItemHeight = 14;
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
            LoadList.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
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
            PluginServerAddress.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            pluginListView.Font = new Font("Microsoft JhengHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
            pluginListView.ForeColor = SystemColors.WindowFrame;
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
            FeedbackContent.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            Contact.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point);
            Contact.ForeColor = Color.FromArgb(90, 90, 90);
            Contact.Location = new Point(59, 233);
            Contact.Name = "Contact";
            Contact.RightToLeft = RightToLeft.No;
            Contact.Size = new Size(500, 26);
            Contact.TabIndex = 7;
            // 
            // FeedbackTips3
            // 
            FeedbackTips3.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
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
            FeedbackTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            mainGroupBox.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            mainGroupBox.ForeColor = Color.FromArgb(90, 90, 90);
            mainGroupBox.Location = new Point(105, 4);
            mainGroupBox.Name = "mainGroupBox";
            mainGroupBox.RightToLeft = RightToLeft.Yes;
            mainGroupBox.Size = new Size(577, 391);
            mainGroupBox.TabIndex = 2;
            mainGroupBox.TabStop = false;
            mainGroupBox.Text = "主页";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(sideLists);
            groupBox1.Location = new Point(12, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(87, 389);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            // 
            // mainContextMenuStrip
            // 
            mainContextMenuStrip.Items.AddRange(new ToolStripItem[] { 播放ToolStripMenuItem, 停止播放ToolStripMenuItem, 删除ToolStripMenuItem, 重命名ToolStripMenuItem, 设为播放项ToolStripMenuItem, 打开文件所在位置ToolStripMenuItem, 绑定按键ToolStripMenuItem });
            mainContextMenuStrip.Name = "mainContextMenuStrip";
            mainContextMenuStrip.Size = new Size(173, 158);
            // 
            // 播放ToolStripMenuItem
            // 
            播放ToolStripMenuItem.Name = "播放ToolStripMenuItem";
            播放ToolStripMenuItem.Size = new Size(172, 22);
            播放ToolStripMenuItem.Text = "播放选择项";
            播放ToolStripMenuItem.Click += 播放ToolStripMenuItem_Click;
            // 
            // 停止播放ToolStripMenuItem
            // 
            停止播放ToolStripMenuItem.Name = "停止播放ToolStripMenuItem";
            停止播放ToolStripMenuItem.Size = new Size(172, 22);
            停止播放ToolStripMenuItem.Text = "停止播放";
            停止播放ToolStripMenuItem.Click += 停止播放ToolStripMenuItem_Click;
            // 
            // 删除ToolStripMenuItem
            // 
            删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
            删除ToolStripMenuItem.Size = new Size(172, 22);
            删除ToolStripMenuItem.Text = "删除选择项";
            删除ToolStripMenuItem.Click += 删除ToolStripMenuItem_Click;
            // 
            // 重命名ToolStripMenuItem
            // 
            重命名ToolStripMenuItem.Name = "重命名ToolStripMenuItem";
            重命名ToolStripMenuItem.Size = new Size(172, 22);
            重命名ToolStripMenuItem.Text = "重命名选择项";
            重命名ToolStripMenuItem.Click += 重命名ToolStripMenuItem_Click;
            // 
            // 设为播放项ToolStripMenuItem
            // 
            设为播放项ToolStripMenuItem.Name = "设为播放项ToolStripMenuItem";
            设为播放项ToolStripMenuItem.Size = new Size(172, 22);
            设为播放项ToolStripMenuItem.Text = "设为播放项";
            设为播放项ToolStripMenuItem.Click += 设为播放项ToolStripMenuItem_Click;
            // 
            // 打开文件所在位置ToolStripMenuItem
            // 
            打开文件所在位置ToolStripMenuItem.Name = "打开文件所在位置ToolStripMenuItem";
            打开文件所在位置ToolStripMenuItem.Size = new Size(172, 22);
            打开文件所在位置ToolStripMenuItem.Text = "打开文件所在位置";
            打开文件所在位置ToolStripMenuItem.Click += 打开文件所在位置ToolStripMenuItem_Click;
            // 
            // 绑定按键ToolStripMenuItem
            // 
            绑定按键ToolStripMenuItem.Name = "绑定按键ToolStripMenuItem";
            绑定按键ToolStripMenuItem.Size = new Size(172, 22);
            绑定按键ToolStripMenuItem.Text = "绑定按键";
            绑定按键ToolStripMenuItem.Click += 绑定按键ToolStripMenuItem_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(692, 407);
            Controls.Add(groupBox1);
            Controls.Add(mainGroupBox);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainWindow";
            FormClosing += MainWindow_FormClosing;
            Load += MainWindow_Load;
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage1.ResumeLayout(false);
            tips_Group1.ResumeLayout(false);
            mainTabControl.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
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
        private ContextMenuStrip mainContextMenuStrip;
        private ToolStripMenuItem 播放ToolStripMenuItem;
        private ToolStripMenuItem 删除ToolStripMenuItem;
        private ToolStripMenuItem 重命名ToolStripMenuItem;
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
        private ToolStripMenuItem 设为播放项ToolStripMenuItem;
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
        private ToolStripMenuItem 打开文件所在位置ToolStripMenuItem;
        private CheckBox switchStreamTips;
        private Label Languagelabel;
        private ComboBox languageComboBox;
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
        private ToolStripMenuItem 停止播放ToolStripMenuItem;
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
        private Button audioTips;
        private ColumnHeader AudioBindKey;
        private ToolStripMenuItem 绑定按键ToolStripMenuItem;
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
    }
}