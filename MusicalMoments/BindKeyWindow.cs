using System;
using System.Windows.Forms;

namespace MusicalMoments
{
    public partial class BindKeyWindow : Form
    {
        private Keys originalKey;
        private Keys nowKey;
        public Keys Key
        {
            get { return nowKey; }
        }

        public BindKeyWindow(Keys key)
        {
            InitializeComponent();
            originalKey = key;
            BindKey.Text = originalKey.ToString();
        }

        private void BindKeyWindow_Load(object sender, EventArgs e)
        {

        }

        private void BindKey_KeyDown(object sender, KeyEventArgs e)
        {
            string displayText = Misc.GetKeyDisplay(keyEventArgs: e);
            if (!string.IsNullOrEmpty(displayText))
            {
                BindKey.Text = displayText;
                nowKey = e.KeyCode;
                e.SuppressKeyPress = true;
            }
            Misc.Delay(500);
            MainWindow.nowKey = nowKey;
            Close();
        }

        private void BindKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            string displayText = e.KeyChar.ToString().ToUpper();
            BindKey.Text = displayText;
            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z')
            {
                nowKey = (Keys)e.KeyChar;
            }
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                nowKey = (Keys)(e.KeyChar - 32);
            }
            e.Handled = true;
        }

        private void BindKey_Leave(object sender, EventArgs e)
        {
            //Close();
        }

        private void removeKey_Click(object sender, EventArgs e)
        {
            MainWindow.nowKey = Keys.None;
            Close();
        }
    }
}
