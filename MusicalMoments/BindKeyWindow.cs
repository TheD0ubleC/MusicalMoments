using System;
using System.Windows.Forms;

namespace MusicalMoments
{
    public partial class BindKeyWindow : Form
    {
        private readonly Keys originalKey;
        private Keys nowKey;
        private Keys pendingKeyboardBinding;
        private GlobalInputHook globalHook;
        private bool bindingCommitted;

        public Keys Key => nowKey;

        public BindKeyWindow(Keys key)
        {
            InitializeComponent();
            WinFormsWhiteTheme.ApplyToForm(this);
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
            if (!KeyBindingService.TryBuildBindingFromKeyEvent(e, out Keys keyValue, out string displayText))
            {
                return;
            }

            BindKey.Text = displayText;
            e.SuppressKeyPress = true;
            if (KeyBindingService.IsModifierOnlyBinding(keyValue))
            {
                pendingKeyboardBinding = keyValue;
                return;
            }

            pendingKeyboardBinding = Keys.None;
            CommitBinding(keyValue, displayText);
        }

        private void BindKey_KeyUp(object sender, KeyEventArgs e)
        {
            if (bindingCommitted || !KeyBindingService.ShouldCommitPendingBindingOnKeyUp(e, pendingKeyboardBinding))
            {
                return;
            }

            Keys keyValue = pendingKeyboardBinding;
            pendingKeyboardBinding = Keys.None;
            CommitBinding(keyValue, KeyBindingService.GetDisplayTextForKey(keyValue));
            e.SuppressKeyPress = true;
        }

        private void BindKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            // KeyDown handles binding capture, keep KeyPress from injecting text.
            e.Handled = true;
        }

        private void BindKey_Leave(object sender, EventArgs e)
        {
            // Keep window alive for global capture.
        }

        private void removeKey_Click(object sender, EventArgs e)
        {
            pendingKeyboardBinding = Keys.None;
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

            if (KeyBindingService.IsEscapeWithoutModifier(e.Key))
            {
                CommitBinding(Keys.None, "None");
                return;
            }

            string displayText = KeyBindingService.GetDisplayTextForKey(e.Key);
            if (string.IsNullOrWhiteSpace(displayText))
            {
                return;
            }

            BindKey.Text = displayText;
            if (KeyBindingService.IsModifierOnlyBinding(e.Key))
            {
                pendingKeyboardBinding = e.Key;
                return;
            }

            pendingKeyboardBinding = Keys.None;
            CommitBinding(e.Key, displayText);
        }

        private void CommitBinding(Keys key, string displayText)
        {
            if (bindingCommitted)
            {
                return;
            }

            bindingCommitted = true;
            pendingKeyboardBinding = Keys.None;
            nowKey = KeyBindingService.NormalizeBinding(key);
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
