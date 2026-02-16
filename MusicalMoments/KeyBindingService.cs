using System;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class KeyBindingService
    {
        public static string GetKeyDisplay(KeyEventArgs keyEventArgs = null, KeyPressEventArgs keyPressEventArgs = null)
        {
            if (keyEventArgs != null)
            {
                Keys keyCode = keyEventArgs.KeyCode;
                switch (keyCode)
                {
                    case Keys.Space:
                        return "SPACE";
                    case Keys.Back:
                        return "BACKSPACE";
                    case Keys.Delete:
                        return "DELETE";
                    case Keys.Home:
                        return "HOME";
                    case Keys.End:
                        return "END";
                    case Keys.PageUp:
                        return "PAGE UP";
                    case Keys.PageDown:
                        return "PAGE DOWN";
                    case Keys.Escape:
                        return "None";
                    case Keys.F1:
                    case Keys.F2:
                    case Keys.F3:
                    case Keys.F4:
                    case Keys.F5:
                    case Keys.F6:
                    case Keys.F7:
                    case Keys.F8:
                    case Keys.F9:
                    case Keys.F10:
                    case Keys.F11:
                    case Keys.F12:
                        return keyCode.ToString();
                    case Keys.D0:
                    case Keys.D1:
                    case Keys.D2:
                    case Keys.D3:
                    case Keys.D4:
                    case Keys.D5:
                    case Keys.D6:
                    case Keys.D7:
                    case Keys.D8:
                    case Keys.D9:
                        return keyCode.ToString().Substring(1);
                    case Keys.Oemcomma:
                        return ",";
                    case Keys.OemPeriod:
                        return ".";
                    case Keys.OemSemicolon:
                        return ";";
                    case Keys.OemQuotes:
                        return "'";
                    case Keys.OemOpenBrackets:
                        return "[";
                    case Keys.OemCloseBrackets:
                        return "]";
                    case Keys.OemPipe:
                        return "\\";
                    case Keys.OemMinus:
                        return "-";
                    case Keys.Oemplus:
                        return "=";
                    case Keys.Oemtilde:
                        return "`";
                    case Keys.OemQuestion:
                        return "/";
                    case Keys.OemBackslash:
                        return "\\";
                    case Keys.NumPad0:
                        return "NUMPAD 0";
                    case Keys.NumPad1:
                        return "NUMPAD 1";
                    case Keys.NumPad2:
                        return "NUMPAD 2";
                    case Keys.NumPad3:
                        return "NUMPAD 3";
                    case Keys.NumPad4:
                        return "NUMPAD 4";
                    case Keys.NumPad5:
                        return "NUMPAD 5";
                    case Keys.NumPad6:
                        return "NUMPAD 6";
                    case Keys.NumPad7:
                        return "NUMPAD 7";
                    case Keys.NumPad8:
                        return "NUMPAD 8";
                    case Keys.NumPad9:
                        return "NUMPAD 9";
                    case Keys.Multiply:
                        return "NUMPAD *";
                    case Keys.Add:
                        return "NUMPAD +";
                    case Keys.Subtract:
                        return "NUMPAD -";
                    case Keys.Decimal:
                        return "NUMPAD .";
                    case Keys.Divide:
                        return "NUMPAD /";
                    case Keys.NumLock:
                        return "NUM LOCK";
                    case Keys.Scroll:
                        return "SCROLL LOCK";
                    case Keys.Pause:
                        return "PAUSE/BREAK";
                    case Keys.Insert:
                        return "INSERT";
                    case Keys.PrintScreen:
                        return "PRINT SCREEN";
                    case Keys.CapsLock:
                        return "CAPS LOCK";
                    case Keys.LWin:
                    case Keys.RWin:
                        return "WINDOWS";
                    case Keys.Apps:
                        return "APPLICATION";
                    case Keys.Tab:
                        return "TAB";
                    case Keys.Enter:
                        return "ENTER";
                    case Keys.ShiftKey:
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        return "SHIFT";
                    case Keys.ControlKey:
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        return "CONTROL";
                    case Keys.Alt:
                    case Keys.Menu:
                        return "ALT";
                    case Keys.MButton:
                        return "MButton";
                    case Keys.XButton1:
                        return "XButton1";
                    case Keys.XButton2:
                        return "XButton2";
                }
            }

            if (keyPressEventArgs != null)
            {
                return keyPressEventArgs.KeyChar.ToString().ToUpperInvariant();
            }

            return string.Empty;
        }

        public static string GetDisplayTextForKey(Keys key)
        {
            if (key == Keys.None)
            {
                return "None";
            }

            string text = GetKeyDisplay(new KeyEventArgs(key), null);
            return string.IsNullOrWhiteSpace(text) ? key.ToString() : text;
        }

        public static bool TryGetSupportedMouseBinding(MouseButtons mouseButton, out Keys key, out string displayText)
        {
            switch (mouseButton)
            {
                case MouseButtons.Middle:
                    key = Keys.MButton;
                    displayText = "MButton";
                    return true;
                case MouseButtons.XButton1:
                    key = Keys.XButton1;
                    displayText = "XButton1";
                    return true;
                case MouseButtons.XButton2:
                    key = Keys.XButton2;
                    displayText = "XButton2";
                    return true;
                default:
                    key = Keys.None;
                    displayText = string.Empty;
                    return false;
            }
        }
    }
}
