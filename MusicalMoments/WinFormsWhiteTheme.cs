using System.Drawing;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class WinFormsWhiteTheme
    {
        private static readonly Color WhiteSurface = Color.White;

        public static void ApplyToForm(Form form)
        {
            if (form == null) return;

            form.BackColor = WhiteSurface;
            ApplyToControl(form);

            if (form.MainMenuStrip != null)
            {
                ApplyToToolStrip(form.MainMenuStrip);
            }

            if (form.ContextMenuStrip != null)
            {
                ApplyToToolStrip(form.ContextMenuStrip);
            }
        }

        public static void ApplyToControl(Control control)
        {
            if (control == null) return;

            ApplySingleControl(control);

            if (control.ContextMenuStrip != null)
            {
                ApplyToToolStrip(control.ContextMenuStrip);
            }

            foreach (Control child in control.Controls)
            {
                ApplyToControl(child);
            }
        }

        public static void ApplyToToolStrip(ToolStrip strip)
        {
            if (strip == null) return;

            strip.BackColor = WhiteSurface;

            foreach (ToolStripItem item in strip.Items)
            {
                ApplyToToolStripItem(item);
            }
        }

        private static void ApplyToToolStripItem(ToolStripItem item)
        {
            if (item == null) return;

            item.BackColor = WhiteSurface;

            if (item is ToolStripDropDownItem dropDownItem)
            {
                if (dropDownItem.DropDown != null)
                {
                    dropDownItem.DropDown.BackColor = WhiteSurface;
                }

                foreach (ToolStripItem child in dropDownItem.DropDownItems)
                {
                    ApplyToToolStripItem(child);
                }
            }
        }

        private static void ApplySingleControl(Control control)
        {
            control.BackColor = WhiteSurface;

            switch (control)
            {
                case Button button:
                    button.UseVisualStyleBackColor = false;
                    break;
                case CheckBox checkBox:
                    checkBox.UseVisualStyleBackColor = false;
                    break;
                case RadioButton radioButton:
                    radioButton.UseVisualStyleBackColor = false;
                    break;
                case TabPage tabPage:
                    tabPage.BackColor = WhiteSurface;
                    break;
            }
        }
    }
}
