using System;
using System.Windows.Forms;

namespace MusicalMoments
{
    public partial class BindKeyWindow : Form
    {
        private readonly Keys originalKey;
        private Keys nowKey;
        private GlobalInputHook globalHook;
        private bool bindingCommitted;

        public Keys Key => nowKey;

        public BindKeyWindow(Keys key)
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = Size;

            Tip.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            BindKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            removeKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            originalKey = key;
            nowKey = originalKey;
            BindKey.Text = KeyBindingService.GetDisplayTextForKey(originalKey);
        }

        private void BindKeyWindow_Load(object sender, EventArgs e)
        {
            InitializeGlobalHook();
            Activate();
            BringToFront();
            BindKey.Focus();
        }

        private void BindKey_KeyDown(object sender, KeyEventArgs e)
        {
            string displayText = KeyBindingService.GetKeyDisplay(keyEventArgs: e);
            if (string.IsNullOrWhiteSpace(displayText))
            {
                return;
            }

            Keys keyValue = displayText == "None" ? Keys.None : e.KeyCode;
            CommitBinding(keyValue, displayText);
            e.SuppressKeyPress = true;
        }

        private void BindKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            string displayText = KeyBindingService.GetKeyDisplay(keyPressEventArgs: e);
            if (string.IsNullOrWhiteSpace(displayText))
            {
                return;
            }

            Keys keyValue = (Keys)char.ToUpperInvariant(e.KeyChar);
            CommitBinding(keyValue, displayText);
            e.Handled = true;
        }

        private void BindKey_Leave(object sender, EventArgs e)
        {
            // Keep window alive for global capture.
        }

        private void removeKey_Click(object sender, EventArgs e)
        {
            CommitBinding(Keys.None, "None");
        }

        private void InitializeGlobalHook()
        {
            DisposeGlobalHook();
            globalHook = new GlobalInputHook();
            globalHook.HotkeyPressed += GlobalHook_HotkeyPressed;
            globalHook.Start();
        }

        private void GlobalHook_HotkeyPressed(object sender, GlobalHotkeyEventArgs e)
        {
            if (bindingCommitted)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => GlobalHook_HotkeyPressed(sender, e)));
                return;
            }

            if (e.Key == Keys.Escape)
            {
                CommitBinding(Keys.None, "None");
                return;
            }

            string displayText = KeyBindingService.GetDisplayTextForKey(e.Key);
            if (string.IsNullOrWhiteSpace(displayText))
            {
                return;
            }

            CommitBinding(e.Key, displayText);
        }

        private void CommitBinding(Keys key, string displayText)
        {
            if (bindingCommitted)
            {
                return;
            }

            bindingCommitted = true;
            nowKey = key;
            BindKey.Text = displayText;
            MainWindow.nowKey = nowKey;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void DisposeGlobalHook()
        {
            if (globalHook == null)
            {
                return;
            }

            globalHook.HotkeyPressed -= GlobalHook_HotkeyPressed;
            globalHook.Dispose();
            globalHook = null;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            DisposeGlobalHook();
            base.OnFormClosed(e);
        }
    }
}
