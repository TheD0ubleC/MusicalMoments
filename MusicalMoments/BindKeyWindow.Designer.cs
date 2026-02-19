namespace MusicalMoments
{
    partial class BindKeyWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BindKeyWindow));
            BindKey = new TextBox();
            Tip = new Label();
            removeKey = new Button();
            SuspendLayout();
            // 
            // BindKey
            // 
            BindKey.BorderStyle = BorderStyle.FixedSingle;
            BindKey.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            BindKey.ForeColor = Color.FromArgb(90, 90, 90);
            BindKey.ImeMode = ImeMode.Disable;
            BindKey.Location = new Point(12, 38);
            BindKey.Name = "BindKey";
            BindKey.RightToLeft = RightToLeft.No;
            BindKey.Size = new Size(192, 28);
            BindKey.TabIndex = 9;
            BindKey.Text = "Key";
            BindKey.TextAlign = HorizontalAlignment.Center;
            BindKey.KeyDown += BindKey_KeyDown;
            BindKey.KeyPress += BindKey_KeyPress;
            BindKey.Leave += BindKey_Leave;
            // 
            // Tip
            // 
            Tip.AutoSize = true;
            Tip.Font = new Font("Microsoft JhengHei UI", 15F, FontStyle.Regular, GraphicsUnit.Point);
            Tip.ForeColor = Color.FromArgb(90, 90, 90);
            Tip.Location = new Point(12, 9);
            Tip.Name = "Tip";
            Tip.Size = new Size(192, 25);
            Tip.TabIndex = 10;
            Tip.Text = "请按下欲绑定的按键";
            // 
            // removeKey
            // 
            removeKey.Font = new Font("Microsoft JhengHei UI", 10.25F, FontStyle.Regular, GraphicsUnit.Point);
            removeKey.ForeColor = Color.FromArgb(90, 90, 90);
            removeKey.Location = new Point(12, 72);
            removeKey.Name = "removeKey";
            removeKey.Size = new Size(192, 31);
            removeKey.TabIndex = 11;
            removeKey.Text = "点我消除已绑定的键";
            removeKey.UseVisualStyleBackColor = false;
            removeKey.Click += removeKey_Click;
            // 
            // BindKeyWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(217, 114);
            Controls.Add(removeKey);
            Controls.Add(Tip);
            Controls.Add(BindKey);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "BindKeyWindow";
            Text = "绑定按键";
            Load += BindKeyWindow_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox BindKey;
        private Label Tip;
        private Button removeKey;
    }
}
