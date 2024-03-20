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
            ListViewItem listViewItem1 = new ListViewItem("主页", 0);
            ListViewItem listViewItem2 = new ListViewItem("音频", 1);
            ListViewItem listViewItem3 = new ListViewItem("设置", 2);
            ListViewItem listViewItem4 = new ListViewItem("赞助", 3);
            ListViewItem listViewItem5 = new ListViewItem("关于", 4);
            ListViewItem listViewItem6 = new ListViewItem("转换", 5);
            ListViewItem listViewItem7 = new ListViewItem("发现", 6);
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
            tabPage1 = new TabPage();
            tips_Group1 = new GroupBox();
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
            info_Label3 = new Label();
            info_Group = new GroupBox();
            info_ListBox = new ListBox();
            info_Label5 = new Label();
            info_Label2 = new Label();
            info_Label1 = new Label();
            LogoImage = new PictureBox();
            tabPage6 = new TabPage();
            conversion_Group3 = new GroupBox();
            label2 = new Label();
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
            numberLabel = new Label();
            DownloadSelected = new Button();
            SearchBarTextBox = new TextBox();
            AudioListBox = new ListBox();
            DownloadLinkListBox = new ListBox();
            LoadList = new Button();
            mainGroupBox = new GroupBox();
            groupBox1 = new GroupBox();
            mainContextMenuStrip = new ContextMenuStrip(components);
            播放ToolStripMenuItem = new ToolStripMenuItem();
            停止播放ToolStripMenuItem = new ToolStripMenuItem();
            删除ToolStripMenuItem = new ToolStripMenuItem();
            重命名ToolStripMenuItem = new ToolStripMenuItem();
            设为播放项ToolStripMenuItem = new ToolStripMenuItem();
            打开文件所在位置ToolStripMenuItem = new ToolStripMenuItem();
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
            conversion_Group3.SuspendLayout();
            conversion_Group2.SuspendLayout();
            conversion_Group1.SuspendLayout();
            tabPage7.SuspendLayout();
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
            sideLists.Font = new Font("NLXJT", 24F, FontStyle.Bold, GraphicsUnit.Point);
            sideLists.ForeColor = Color.FromArgb(90, 90, 90);
            sideLists.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2, listViewItem3, listViewItem4, listViewItem5, listViewItem6, listViewItem7 });
            sideLists.LargeImageList = sideListsImage;
            sideLists.Location = new Point(3, 16);
            sideLists.Name = "sideLists";
            sideLists.OwnerDraw = true;
            sideLists.Scrollable = false;
            sideLists.Size = new Size(111, 363);
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
            sideListsImage.Images.SetKeyName(0, "首页.png");
            sideListsImage.Images.SetKeyName(1, "声音.png");
            sideListsImage.Images.SetKeyName(2, "设置.png");
            sideListsImage.Images.SetKeyName(3, "赞.png");
            sideListsImage.Images.SetKeyName(4, "一般提示.png");
            sideListsImage.Images.SetKeyName(5, "转换.png");
            sideListsImage.Images.SetKeyName(6, "发现.png");
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
            SelectedAudioLabel.Font = new Font("NLXJT", 10F, FontStyle.Bold, GraphicsUnit.Point);
            SelectedAudioLabel.Location = new Point(6, 303);
            SelectedAudioLabel.Name = "SelectedAudioLabel";
            SelectedAudioLabel.RightToLeft = RightToLeft.No;
            SelectedAudioLabel.Size = new Size(58, 19);
            SelectedAudioLabel.TabIndex = 2;
            SelectedAudioLabel.Text = "已选择:";
            // 
            // reLoadAudioListsView
            // 
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
            audioListView.Columns.AddRange(new ColumnHeader[] { AudioName, AudioTrack, AudioType });
            audioListView.Font = new Font("NLXJT", 12F, FontStyle.Regular, GraphicsUnit.Point);
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
            AudioName.Width = 300;
            // 
            // AudioTrack
            // 
            AudioTrack.Text = "曲目";
            AudioTrack.Width = 120;
            // 
            // AudioType
            // 
            AudioType.Text = "音频类型";
            AudioType.Width = 100;
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
            tips_Group1.Controls.Add(mToAudioData);
            tips_Group1.Controls.Add(retestVB);
            tips_Group1.Controls.Add(label_VBStatus);
            tips_Group1.Controls.Add(toSettings);
            tips_Group1.Controls.Add(tips_Label4);
            tips_Group1.Controls.Add(tips_Label3);
            tips_Group1.Controls.Add(toVB);
            tips_Group1.Controls.Add(tips_Label2);
            tips_Group1.Controls.Add(tips_Label1);
            tips_Group1.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
            tips_Group1.ForeColor = Color.FromArgb(90, 90, 90);
            tips_Group1.Location = new Point(0, -10);
            tips_Group1.Name = "tips_Group1";
            tips_Group1.RightToLeft = RightToLeft.No;
            tips_Group1.Size = new Size(565, 207);
            tips_Group1.TabIndex = 11;
            tips_Group1.TabStop = false;
            // 
            // mToAudioData
            // 
            mToAudioData.Font = new Font("NLXJT", 13F, FontStyle.Bold, GraphicsUnit.Point);
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
            retestVB.Font = new Font("NLXJT", 13F, FontStyle.Bold, GraphicsUnit.Point);
            retestVB.Location = new Point(458, 170);
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
            label_VBStatus.Font = new Font("NLXJT", 18F, FontStyle.Bold, GraphicsUnit.Point);
            label_VBStatus.Location = new Point(6, 166);
            label_VBStatus.Name = "label_VBStatus";
            label_VBStatus.Size = new Size(553, 34);
            label_VBStatus.TabIndex = 1;
            label_VBStatus.Text = "VB声卡状态";
            // 
            // toSettings
            // 
            toSettings.Font = new Font("NLXJT", 13F, FontStyle.Bold, GraphicsUnit.Point);
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
            tips_Label4.Font = new Font("NLXJT", 18F, FontStyle.Bold, GraphicsUnit.Point);
            tips_Label4.Location = new Point(6, 128);
            tips_Label4.Name = "tips_Label4";
            tips_Label4.Size = new Size(553, 34);
            tips_Label4.TabIndex = 13;
            tips_Label4.Text = "3.音频放置位置位于根目录的[AudioData]";
            // 
            // tips_Label3
            // 
            tips_Label3.BorderStyle = BorderStyle.FixedSingle;
            tips_Label3.Font = new Font("NLXJT", 18F, FontStyle.Bold, GraphicsUnit.Point);
            tips_Label3.Location = new Point(6, 90);
            tips_Label3.Name = "tips_Label3";
            tips_Label3.Size = new Size(553, 34);
            tips_Label3.TabIndex = 12;
            tips_Label3.Text = "2.安装后需在设置中绑定对应设备";
            // 
            // toVB
            // 
            toVB.Font = new Font("NLXJT", 13F, FontStyle.Bold, GraphicsUnit.Point);
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
            tips_Label2.Font = new Font("NLXJT", 18F, FontStyle.Bold, GraphicsUnit.Point);
            tips_Label2.Location = new Point(6, 51);
            tips_Label2.Name = "tips_Label2";
            tips_Label2.Size = new Size(553, 34);
            tips_Label2.TabIndex = 10;
            tips_Label2.Text = "1.首次使用前需安装VB声卡";
            // 
            // tips_Label1
            // 
            tips_Label1.Font = new Font("NLXJT", 24F, FontStyle.Bold, GraphicsUnit.Point);
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
            volume_Group.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            volume_Label3.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            volume_Label2.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            volume_Label1.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            group_Misc.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            Languagelabel.Font = new Font("NLXJT", 14F, FontStyle.Bold, GraphicsUnit.Point);
            Languagelabel.Location = new Point(367, 17);
            Languagelabel.Name = "Languagelabel";
            Languagelabel.Size = new Size(57, 25);
            Languagelabel.TabIndex = 12;
            Languagelabel.Text = "语言:";
            // 
            // languageComboBox
            // 
            languageComboBox.Font = new Font("NLXJT", 10F, FontStyle.Bold, GraphicsUnit.Point);
            languageComboBox.ForeColor = Color.FromArgb(90, 90, 90);
            languageComboBox.FormattingEnabled = true;
            languageComboBox.Items.AddRange(new object[] { "简体中文", "English", "日本語" });
            languageComboBox.Location = new Point(429, 15);
            languageComboBox.Name = "languageComboBox";
            languageComboBox.Size = new Size(121, 25);
            languageComboBox.TabIndex = 2;
            languageComboBox.Text = "简体中文";
            languageComboBox.SelectedIndexChanged += languageComboBox_SelectedIndexChanged;
            // 
            // switchStreamTips
            // 
            switchStreamTips.AutoSize = true;
            switchStreamTips.Checked = true;
            switchStreamTips.CheckState = CheckState.Checked;
            switchStreamTips.Location = new Point(196, 18);
            switchStreamTips.Name = "switchStreamTips";
            switchStreamTips.Size = new Size(165, 25);
            switchStreamTips.TabIndex = 1;
            switchStreamTips.Text = "切换源时播放提示";
            switchStreamTips.UseVisualStyleBackColor = true;
            // 
            // audioEquipmentPlay
            // 
            audioEquipmentPlay.AutoSize = true;
            audioEquipmentPlay.Checked = true;
            audioEquipmentPlay.CheckState = CheckState.Checked;
            audioEquipmentPlay.Location = new Point(8, 18);
            audioEquipmentPlay.Name = "audioEquipmentPlay";
            audioEquipmentPlay.Size = new Size(182, 25);
            audioEquipmentPlay.TabIndex = 0;
            audioEquipmentPlay.Text = "物理扬声器同步播放";
            audioEquipmentPlay.UseVisualStyleBackColor = true;
            // 
            // group_Key
            // 
            group_Key.Controls.Add(label_Key2);
            group_Key.Controls.Add(PlayAudio);
            group_Key.Controls.Add(label_Key1);
            group_Key.Controls.Add(ToggleStream);
            group_Key.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            label_Key2.Font = new Font("NLXJT", 14F, FontStyle.Bold, GraphicsUnit.Point);
            label_Key2.Location = new Point(267, 23);
            label_Key2.Name = "label_Key2";
            label_Key2.Size = new Size(132, 25);
            label_Key2.TabIndex = 11;
            label_Key2.Text = "播放音频按键";
            // 
            // PlayAudio
            // 
            PlayAudio.BorderStyle = BorderStyle.FixedSingle;
            PlayAudio.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
            PlayAudio.ForeColor = Color.FromArgb(90, 90, 90);
            PlayAudio.ImeMode = ImeMode.Disable;
            PlayAudio.Location = new Point(405, 20);
            PlayAudio.Name = "PlayAudio";
            PlayAudio.RightToLeft = RightToLeft.No;
            PlayAudio.Size = new Size(135, 28);
            PlayAudio.TabIndex = 10;
            PlayAudio.Text = "Key";
            PlayAudio.TextAlign = HorizontalAlignment.Center;
            PlayAudio.KeyDown += PlayAudio_KeyDown;
            PlayAudio.KeyPress += PlayAudio_KeyPress;
            // 
            // label_Key1
            // 
            label_Key1.AutoSize = true;
            label_Key1.Font = new Font("NLXJT", 14F, FontStyle.Bold, GraphicsUnit.Point);
            label_Key1.Location = new Point(8, 24);
            label_Key1.Name = "label_Key1";
            label_Key1.Size = new Size(112, 25);
            label_Key1.TabIndex = 9;
            label_Key1.Text = "切换源按键";
            // 
            // ToggleStream
            // 
            ToggleStream.BorderStyle = BorderStyle.FixedSingle;
            ToggleStream.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
            ToggleStream.ForeColor = Color.FromArgb(90, 90, 90);
            ToggleStream.ImeMode = ImeMode.Disable;
            ToggleStream.Location = new Point(126, 21);
            ToggleStream.Name = "ToggleStream";
            ToggleStream.RightToLeft = RightToLeft.No;
            ToggleStream.Size = new Size(135, 28);
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
            group_AudioEquipment.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
            comboBox_AudioEquipmentOutput.Font = new Font("NLXJT", 10F, FontStyle.Regular, GraphicsUnit.Point);
            comboBox_AudioEquipmentOutput.ForeColor = Color.FromArgb(90, 90, 90);
            comboBox_AudioEquipmentOutput.FormattingEnabled = true;
            comboBox_AudioEquipmentOutput.Location = new Point(126, 117);
            comboBox_AudioEquipmentOutput.Name = "comboBox_AudioEquipmentOutput";
            comboBox_AudioEquipmentOutput.Size = new Size(421, 25);
            comboBox_AudioEquipmentOutput.TabIndex = 7;
            // 
            // label_AudioEquipment4
            // 
            label_AudioEquipment4.AutoSize = true;
            label_AudioEquipment4.Font = new Font("NLXJT", 14F, FontStyle.Bold, GraphicsUnit.Point);
            label_AudioEquipment4.Location = new Point(8, 117);
            label_AudioEquipment4.Name = "label_AudioEquipment4";
            label_AudioEquipment4.Size = new Size(112, 25);
            label_AudioEquipment4.TabIndex = 6;
            label_AudioEquipment4.Text = "物理扬声器";
            // 
            // label_AudioEquipment1
            // 
            label_AudioEquipment1.AutoSize = true;
            label_AudioEquipment1.Font = new Font("NLXJT", 14F, FontStyle.Bold, GraphicsUnit.Point);
            label_AudioEquipment1.Location = new Point(8, 24);
            label_AudioEquipment1.Name = "label_AudioEquipment1";
            label_AudioEquipment1.Size = new Size(114, 25);
            label_AudioEquipment1.TabIndex = 0;
            label_AudioEquipment1.Text = "VB声卡输入";
            // 
            // comboBox_AudioEquipmentInput
            // 
            comboBox_AudioEquipmentInput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_AudioEquipmentInput.Font = new Font("NLXJT", 10F, FontStyle.Regular, GraphicsUnit.Point);
            comboBox_AudioEquipmentInput.ForeColor = Color.FromArgb(90, 90, 90);
            comboBox_AudioEquipmentInput.FormattingEnabled = true;
            comboBox_AudioEquipmentInput.Location = new Point(126, 86);
            comboBox_AudioEquipmentInput.Name = "comboBox_AudioEquipmentInput";
            comboBox_AudioEquipmentInput.Size = new Size(421, 25);
            comboBox_AudioEquipmentInput.TabIndex = 5;
            // 
            // label_AudioEquipment2
            // 
            label_AudioEquipment2.AutoSize = true;
            label_AudioEquipment2.Font = new Font("NLXJT", 14F, FontStyle.Bold, GraphicsUnit.Point);
            label_AudioEquipment2.Location = new Point(6, 55);
            label_AudioEquipment2.Name = "label_AudioEquipment2";
            label_AudioEquipment2.Size = new Size(114, 25);
            label_AudioEquipment2.TabIndex = 1;
            label_AudioEquipment2.Text = "VB声卡输出";
            // 
            // comboBox_VBAudioEquipmentOutput
            // 
            comboBox_VBAudioEquipmentOutput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_VBAudioEquipmentOutput.Font = new Font("NLXJT", 10F, FontStyle.Regular, GraphicsUnit.Point);
            comboBox_VBAudioEquipmentOutput.ForeColor = Color.FromArgb(90, 90, 90);
            comboBox_VBAudioEquipmentOutput.FormattingEnabled = true;
            comboBox_VBAudioEquipmentOutput.Location = new Point(126, 55);
            comboBox_VBAudioEquipmentOutput.Name = "comboBox_VBAudioEquipmentOutput";
            comboBox_VBAudioEquipmentOutput.Size = new Size(421, 25);
            comboBox_VBAudioEquipmentOutput.TabIndex = 4;
            // 
            // label_AudioEquipment3
            // 
            label_AudioEquipment3.AutoSize = true;
            label_AudioEquipment3.Font = new Font("NLXJT", 14F, FontStyle.Bold, GraphicsUnit.Point);
            label_AudioEquipment3.Location = new Point(8, 86);
            label_AudioEquipment3.Name = "label_AudioEquipment3";
            label_AudioEquipment3.Size = new Size(112, 25);
            label_AudioEquipment3.TabIndex = 2;
            label_AudioEquipment3.Text = "物理麦克风";
            // 
            // comboBox_VBAudioEquipmentInput
            // 
            comboBox_VBAudioEquipmentInput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_VBAudioEquipmentInput.Font = new Font("NLXJT", 10F, FontStyle.Regular, GraphicsUnit.Point);
            comboBox_VBAudioEquipmentInput.ForeColor = Color.FromArgb(90, 90, 90);
            comboBox_VBAudioEquipmentInput.FormattingEnabled = true;
            comboBox_VBAudioEquipmentInput.Location = new Point(126, 24);
            comboBox_VBAudioEquipmentInput.Name = "comboBox_VBAudioEquipmentInput";
            comboBox_VBAudioEquipmentInput.Size = new Size(421, 25);
            comboBox_VBAudioEquipmentInput.TabIndex = 3;
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
            aifadian.Font = new Font("NLXJT", 18F, FontStyle.Bold, GraphicsUnit.Point);
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
            thankTip.Font = new Font("NLXJT", 24F, FontStyle.Bold, GraphicsUnit.Point);
            thankTip.Location = new Point(179, 305);
            thankTip.Name = "thankTip";
            thankTip.Size = new Size(216, 42);
            thankTip.TabIndex = 5;
            thankTip.Text = "感谢您的支持";
            // 
            // tabPage5
            // 
            tabPage5.BackColor = SystemColors.Control;
            tabPage5.Controls.Add(info_Label3);
            tabPage5.Controls.Add(info_Group);
            tabPage5.Controls.Add(info_Label2);
            tabPage5.Controls.Add(info_Label1);
            tabPage5.Controls.Add(LogoImage);
            tabPage5.Location = new Point(0, 22);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(565, 341);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "关于";
            // 
            // info_Label3
            // 
            info_Label3.AutoSize = true;
            info_Label3.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
            info_Label3.Location = new Point(172, 72);
            info_Label3.Name = "info_Label3";
            info_Label3.RightToLeft = RightToLeft.No;
            info_Label3.Size = new Size(364, 105);
            info_Label3.TabIndex = 5;
            info_Label3.Text = "- 主版本号（Major Version）：1\r\n- 次版本号（Minor Version）：2\r\n- 修订号（Patch Version）：0\r\n- 预发布版本号（Pre-release Version）：Release\r\n- 构建号（Build Number）：20240320";
            // 
            // info_Group
            // 
            info_Group.Controls.Add(info_ListBox);
            info_Group.Controls.Add(info_Label5);
            info_Group.Font = new Font("NLXJT", 10F, FontStyle.Bold, GraphicsUnit.Point);
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
            info_ListBox.ForeColor = Color.FromArgb(90, 90, 90);
            info_ListBox.FormattingEnabled = true;
            info_ListBox.ItemHeight = 17;
            info_ListBox.Items.AddRange(new object[] { "NAudio", "Newtonsoft.Json", "System.Management", "taglib-sharp-netstandard2.0", "MouseKeyHook", "MediaToolkit", "HtmlAgilityPack" });
            info_ListBox.Location = new Point(389, 16);
            info_ListBox.Name = "info_ListBox";
            info_ListBox.RightToLeft = RightToLeft.No;
            info_ListBox.Size = new Size(162, 157);
            info_ListBox.TabIndex = 5;
            info_ListBox.SelectedIndexChanged += info_ListBox_SelectedIndexChanged;
            // 
            // info_Label5
            // 
            info_Label5.BorderStyle = BorderStyle.FixedSingle;
            info_Label5.Font = new Font("NLXJT", 12.5F, FontStyle.Bold, GraphicsUnit.Point);
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
            info_Label2.Font = new Font("NLXJT", 20F, FontStyle.Bold, GraphicsUnit.Point);
            info_Label2.Location = new Point(167, 40);
            info_Label2.Name = "info_Label2";
            info_Label2.Size = new Size(337, 35);
            info_Label2.TabIndex = 2;
            info_Label2.Text = "版本号:1.2.0-Release+20240320";
            // 
            // info_Label1
            // 
            info_Label1.AutoSize = true;
            info_Label1.Font = new Font("NLXJT", 23F, FontStyle.Bold, GraphicsUnit.Point);
            info_Label1.ForeColor = Color.FromArgb(128, 128, 255);
            info_Label1.Location = new Point(167, 6);
            info_Label1.Name = "info_Label1";
            info_Label1.Size = new Size(392, 40);
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
            // tabPage6
            // 
            tabPage6.AllowDrop = true;
            tabPage6.BackColor = SystemColors.Control;
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
            // conversion_Group3
            // 
            conversion_Group3.Controls.Add(label2);
            conversion_Group3.ForeColor = Color.FromArgb(90, 90, 90);
            conversion_Group3.Location = new Point(6, 150);
            conversion_Group3.Name = "conversion_Group3";
            conversion_Group3.RightToLeft = RightToLeft.No;
            conversion_Group3.Size = new Size(553, 61);
            conversion_Group3.TabIndex = 4;
            conversion_Group3.TabStop = false;
            conversion_Group3.Text = "提示";
            // 
            // label2
            // 
            label2.Location = new Point(6, 27);
            label2.Name = "label2";
            label2.Size = new Size(541, 31);
            label2.TabIndex = 3;
            label2.Text = "支持被转化的格式:<ncm><mp3><wav><ogg><flac>";
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
            convert_Button.Font = new Font("NLXJT", 13F, FontStyle.Bold, GraphicsUnit.Point);
            convert_Button.Location = new Point(472, 29);
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
            conversion_Label2.Location = new Point(286, 31);
            conversion_Label2.Name = "conversion_Label2";
            conversion_Label2.Size = new Size(64, 29);
            conversion_Label2.TabIndex = 5;
            conversion_Label2.Text = "格式:";
            // 
            // name_TextBox
            // 
            name_TextBox.BorderStyle = BorderStyle.FixedSingle;
            name_TextBox.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
            name_TextBox.ForeColor = Color.FromArgb(90, 90, 90);
            name_TextBox.Location = new Point(66, 29);
            name_TextBox.Name = "name_TextBox";
            name_TextBox.Size = new Size(214, 28);
            name_TextBox.TabIndex = 4;
            // 
            // conversion_Label1
            // 
            conversion_Label1.AutoSize = true;
            conversion_Label1.Location = new Point(6, 31);
            conversion_Label1.Name = "conversion_Label1";
            conversion_Label1.Size = new Size(64, 29);
            conversion_Label1.TabIndex = 3;
            conversion_Label1.Text = "名称:";
            // 
            // comboBoxOutputFormat
            // 
            comboBoxOutputFormat.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
            comboBoxOutputFormat.ForeColor = Color.FromArgb(90, 90, 90);
            comboBoxOutputFormat.FormattingEnabled = true;
            comboBoxOutputFormat.Items.AddRange(new object[] { "mp3", "wav" });
            comboBoxOutputFormat.Location = new Point(356, 28);
            comboBoxOutputFormat.Name = "comboBoxOutputFormat";
            comboBoxOutputFormat.Size = new Size(110, 29);
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
            dataPath_TextBox.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
            dataPath_TextBox.ForeColor = Color.FromArgb(90, 90, 90);
            dataPath_TextBox.Location = new Point(6, 28);
            dataPath_TextBox.Name = "dataPath_TextBox";
            dataPath_TextBox.Size = new Size(460, 28);
            dataPath_TextBox.TabIndex = 1;
            // 
            // upData_button
            // 
            upData_button.Font = new Font("NLXJT", 13F, FontStyle.Bold, GraphicsUnit.Point);
            upData_button.Location = new Point(472, 28);
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
            tabPage7.Controls.Add(numberLabel);
            tabPage7.Controls.Add(DownloadSelected);
            tabPage7.Controls.Add(SearchBarTextBox);
            tabPage7.Controls.Add(AudioListBox);
            tabPage7.Controls.Add(DownloadLinkListBox);
            tabPage7.Controls.Add(LoadList);
            tabPage7.Location = new Point(0, 22);
            tabPage7.Name = "tabPage7";
            tabPage7.Padding = new Padding(3);
            tabPage7.Size = new Size(565, 341);
            tabPage7.TabIndex = 6;
            tabPage7.Text = "发现";
            // 
            // numberLabel
            // 
            numberLabel.AutoSize = true;
            numberLabel.Font = new Font("NLXJT", 10F, FontStyle.Bold, GraphicsUnit.Point);
            numberLabel.Location = new Point(436, 70);
            numberLabel.Name = "numberLabel";
            numberLabel.RightToLeft = RightToLeft.No;
            numberLabel.Size = new Size(0, 19);
            numberLabel.TabIndex = 5;
            // 
            // DownloadSelected
            // 
            DownloadSelected.Font = new Font("NLXJT", 14F, FontStyle.Bold, GraphicsUnit.Point);
            DownloadSelected.Location = new Point(436, 39);
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
            SearchBarTextBox.Font = new Font("NLXJT", 12F, FontStyle.Bold, GraphicsUnit.Point);
            SearchBarTextBox.ForeColor = Color.FromArgb(90, 90, 90);
            SearchBarTextBox.Location = new Point(6, 6);
            SearchBarTextBox.Name = "SearchBarTextBox";
            SearchBarTextBox.RightToLeft = RightToLeft.No;
            SearchBarTextBox.Size = new Size(424, 28);
            SearchBarTextBox.TabIndex = 4;
            SearchBarTextBox.Text = "搜索";
            SearchBarTextBox.TextAlign = HorizontalAlignment.Center;
            SearchBarTextBox.TextChanged += SearchBarTextBox_TextChanged;
            SearchBarTextBox.Enter += SearchBarTextBox_Enter;
            SearchBarTextBox.Leave += SearchBarTextBox_Leave;
            // 
            // AudioListBox
            // 
            AudioListBox.Font = new Font("NLXJT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            AudioListBox.ForeColor = Color.FromArgb(90, 90, 90);
            AudioListBox.FormattingEnabled = true;
            AudioListBox.ItemHeight = 21;
            AudioListBox.Location = new Point(6, 39);
            AudioListBox.Name = "AudioListBox";
            AudioListBox.RightToLeft = RightToLeft.No;
            AudioListBox.ScrollAlwaysVisible = true;
            AudioListBox.Size = new Size(424, 319);
            AudioListBox.TabIndex = 0;
            // 
            // DownloadLinkListBox
            // 
            DownloadLinkListBox.Font = new Font("NLXJT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            DownloadLinkListBox.ForeColor = Color.FromArgb(90, 90, 90);
            DownloadLinkListBox.FormattingEnabled = true;
            DownloadLinkListBox.ItemHeight = 21;
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
            LoadList.Font = new Font("NLXJT", 14F, FontStyle.Bold, GraphicsUnit.Point);
            LoadList.Location = new Point(436, 6);
            LoadList.Name = "LoadList";
            LoadList.Size = new Size(123, 28);
            LoadList.TabIndex = 1;
            LoadList.Text = "加载/刷新";
            LoadList.UseVisualStyleBackColor = true;
            LoadList.Click += LoadList_Click;
            // 
            // mainGroupBox
            // 
            mainGroupBox.Controls.Add(mainTabControl);
            mainGroupBox.Font = new Font("NLXJT", 16F, FontStyle.Bold, GraphicsUnit.Point);
            mainGroupBox.ForeColor = Color.FromArgb(90, 90, 90);
            mainGroupBox.Location = new Point(135, 4);
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
            groupBox1.Location = new Point(12, 10);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(117, 385);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            // 
            // mainContextMenuStrip
            // 
            mainContextMenuStrip.Items.AddRange(new ToolStripItem[] { 播放ToolStripMenuItem, 停止播放ToolStripMenuItem, 删除ToolStripMenuItem, 重命名ToolStripMenuItem, 设为播放项ToolStripMenuItem, 打开文件所在位置ToolStripMenuItem });
            mainContextMenuStrip.Name = "mainContextMenuStrip";
            mainContextMenuStrip.Size = new Size(173, 136);
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
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(724, 407);
            Controls.Add(groupBox1);
            Controls.Add(mainGroupBox);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainWindow";
            Text = "Musical Moments - 音乐时刻";
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
            conversion_Group3.ResumeLayout(false);
            conversion_Group2.ResumeLayout(false);
            conversion_Group2.PerformLayout();
            conversion_Group1.ResumeLayout(false);
            conversion_Group1.PerformLayout();
            tabPage7.ResumeLayout(false);
            tabPage7.PerformLayout();
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
        private ComboBox comboBox_AudioEquipmentInput;
        private ComboBox comboBox_VBAudioEquipmentOutput;
        private ComboBox comboBox_VBAudioEquipmentInput;
        private GroupBox group_AudioEquipment;
        private ComboBox comboBox_AudioEquipmentOutput;
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
        private CheckBox audioEquipmentPlay;
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
        private Label label2;
        private Button mToAudioData1;
        private ToolStripMenuItem 停止播放ToolStripMenuItem;
        private TabPage tabPage7;
        private ListBox AudioListBox;
        private Button LoadList;
        private ListBox DownloadLinkListBox;
        private Button DownloadSelected;
        private TextBox SearchBarTextBox;
        private Label numberLabel;
    }
}