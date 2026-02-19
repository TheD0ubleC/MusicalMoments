using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class KeyBindingService
    {
        public static string GetKeyDisplay(KeyEventArgs keyEventArgs = null, KeyPressEventArgs keyPressEventArgs = null)
        {
            if (keyEventArgs != null)
            {
                if (TryBuildBindingFromKeyEvent(keyEventArgs, out _, out string displayText))
                {
                    return displayText;
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
            Keys normalized = NormalizeBinding(key);
            if (normalized == Keys.None)
            {
                return "None";
            }

            Keys keyCode = NormalizePrimaryKey(normalized & Keys.KeyCode);
            Keys modifiers = normalized & Keys.Modifiers;

            List<string> parts = new List<string>(4);
            if ((modifiers & Keys.Control) == Keys.Control)
            {
                parts.Add("Ctrl");
            }

            if ((modifiers & Keys.Alt) == Keys.Alt)
            {
                parts.Add("Alt");
            }

            if ((modifiers & Keys.Shift) == Keys.Shift)
            {
                parts.Add("Shift");
            }

            string keyName = GetSimpleKeyName(keyCode);
            if (!string.IsNullOrWhiteSpace(keyName))
            {
                parts.Add(keyName);
            }

            return parts.Count == 0 ? "None" : string.Join("+", parts);
        }

        public static bool TryBuildBindingFromKeyEvent(KeyEventArgs keyEventArgs, out Keys key, out string displayText)
        {
            key = Keys.None;
            displayText = string.Empty;
            if (keyEventArgs == null)
            {
                return false;
            }

            Keys keyCode = NormalizePrimaryKey(keyEventArgs.KeyCode);
            bool ctrl = (keyEventArgs.Modifiers & Keys.Control) == Keys.Control;
            bool alt = (keyEventArgs.Modifiers & Keys.Alt) == Keys.Alt;
            bool shift = (keyEventArgs.Modifiers & Keys.Shift) == Keys.Shift;

            if (keyCode == Keys.Escape)
            {
                key = Keys.None;
                displayText = "None";
                return true;
            }

            key = BuildBinding(keyCode, ctrl, alt, shift);
            if (key == Keys.None)
            {
                return false;
            }

            displayText = GetDisplayTextForKey(key);
            return !string.IsNullOrWhiteSpace(displayText);
        }

        public static Keys NormalizeBinding(Keys key)
        {
            if (key == Keys.None)
            {
                return Keys.None;
            }

            Keys keyCode = NormalizePrimaryKey(key & Keys.KeyCode);
            Keys modifiers = key & Keys.Modifiers;

            if (keyCode == Keys.LButton || keyCode == Keys.RButton)
            {
                return Keys.None;
            }

            if (keyCode == Keys.ControlKey)
            {
                modifiers &= ~Keys.Control;
            }
            else if (keyCode == Keys.Menu)
            {
                modifiers &= ~Keys.Alt;
            }
            else if (keyCode == Keys.ShiftKey)
            {
                modifiers &= ~Keys.Shift;
            }

            return keyCode | modifiers;
        }

        public static Keys BuildBinding(Keys primaryKey, bool ctrl, bool alt, bool shift)
        {
            Keys keyCode = NormalizePrimaryKey(primaryKey & Keys.KeyCode);
            if (keyCode == Keys.None || keyCode == Keys.LButton || keyCode == Keys.RButton)
            {
                return Keys.None;
            }

            Keys modifiers = Keys.None;
            if (ctrl)
            {
                modifiers |= Keys.Control;
            }

            if (alt)
            {
                modifiers |= Keys.Alt;
            }

            if (shift)
            {
                modifiers |= Keys.Shift;
            }

            return NormalizeBinding(keyCode | modifiers);
        }

        public static bool TryParseBinding(string text, out Keys key)
        {
            key = Keys.None;
            if (string.IsNullOrWhiteSpace(text) || string.Equals(text.Trim(), "None", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (Enum.TryParse(text, true, out Keys parsed))
            {
                key = NormalizeBinding(parsed);
                return true;
            }

            string[] parts = text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                return false;
            }

            bool ctrl = false;
            bool alt = false;
            bool shift = false;
            Keys primary = Keys.None;

            foreach (string rawPart in parts)
            {
                string part = rawPart.Trim();
                if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase)
                    || part.Equals("Control", StringComparison.OrdinalIgnoreCase))
                {
                    ctrl = true;
                    continue;
                }

                if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                {
                    alt = true;
                    continue;
                }

                if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                {
                    shift = true;
                    continue;
                }

                if (TryParseSimpleKeyName(part, out Keys parsedPrimary))
                {
                    primary = parsedPrimary;
                    continue;
                }

                return false;
            }

            if (primary == Keys.None)
            {
                if (ctrl)
                {
                    primary = Keys.ControlKey;
                }
                else if (alt)
                {
                    primary = Keys.Menu;
                }
                else if (shift)
                {
                    primary = Keys.ShiftKey;
                }
            }

            key = BuildBinding(primary, ctrl, alt, shift);
            return key != Keys.None;
        }

        public static bool TryGetSupportedMouseBinding(MouseButtons mouseButton, out Keys key, out string displayText)
            => TryGetSupportedMouseBinding(mouseButton, Keys.None, out key, out displayText);

        public static bool TryGetSupportedMouseBinding(MouseButtons mouseButton, Keys modifiers, out Keys key, out string displayText)
        {
            Keys primary = mouseButton switch
            {
                MouseButtons.Middle => Keys.MButton,
                MouseButtons.XButton1 => Keys.XButton1,
                MouseButtons.XButton2 => Keys.XButton2,
                _ => Keys.None
            };

            if (primary == Keys.None)
            {
                key = Keys.None;
                displayText = string.Empty;
                return false;
            }

            bool ctrl = (modifiers & Keys.Control) == Keys.Control;
            bool alt = (modifiers & Keys.Alt) == Keys.Alt;
            bool shift = (modifiers & Keys.Shift) == Keys.Shift;

            key = BuildBinding(primary, ctrl, alt, shift);
            displayText = GetDisplayTextForKey(key);
            return key != Keys.None;
        }

        public static Keys NormalizePrimaryKey(Keys keyCode)
        {
            return keyCode switch
            {
                Keys.LControlKey => Keys.ControlKey,
                Keys.RControlKey => Keys.ControlKey,
                Keys.Control => Keys.ControlKey,
                Keys.LShiftKey => Keys.ShiftKey,
                Keys.RShiftKey => Keys.ShiftKey,
                Keys.Shift => Keys.ShiftKey,
                Keys.LMenu => Keys.Menu,
                Keys.RMenu => Keys.Menu,
                Keys.Alt => Keys.Menu,
                _ => keyCode
            };
        }

        public static bool IsModifierPrimaryKey(Keys keyCode)
        {
            keyCode = NormalizePrimaryKey(keyCode);
            return keyCode == Keys.ControlKey || keyCode == Keys.Menu || keyCode == Keys.ShiftKey;
        }

        public static bool IsEscapeWithoutModifier(Keys key)
        {
            Keys normalized = NormalizeBinding(key);
            return (normalized & Keys.KeyCode) == Keys.Escape;
        }

        private static bool TryParseSimpleKeyName(string text, out Keys key)
        {
            key = Keys.None;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (text.Length == 1)
            {
                char c = text[0];
                if (c >= 'A' && c <= 'Z')
                {
                    key = (Keys)c;
                    return true;
                }

                if (c >= 'a' && c <= 'z')
                {
                    key = (Keys)char.ToUpperInvariant(c);
                    return true;
                }

                if (c >= '0' && c <= '9')
                {
                    key = Keys.D0 + (c - '0');
                    return true;
                }
            }

            switch (text.ToUpperInvariant())
            {
                case "SPACE":
                    key = Keys.Space;
                    return true;
                case "BACKSPACE":
                    key = Keys.Back;
                    return true;
                case "DELETE":
                    key = Keys.Delete;
                    return true;
                case "HOME":
                    key = Keys.Home;
                    return true;
                case "END":
                    key = Keys.End;
                    return true;
                case "PAGEUP":
                case "PAGE UP":
                    key = Keys.PageUp;
                    return true;
                case "PAGEDOWN":
                case "PAGE DOWN":
                    key = Keys.PageDown;
                    return true;
                case "TAB":
                    key = Keys.Tab;
                    return true;
                case "ENTER":
                    key = Keys.Enter;
                    return true;
                case "MOUSEMIDDLE":
                    key = Keys.MButton;
                    return true;
                case "MOUSEX1":
                    key = Keys.XButton1;
                    return true;
                case "MOUSEX2":
                    key = Keys.XButton2;
                    return true;
            }

            if (Enum.TryParse(text, true, out Keys parsed))
            {
                key = NormalizePrimaryKey(parsed & Keys.KeyCode);
                return key != Keys.None;
            }

            return false;
        }

        private static string GetSimpleKeyName(Keys keyCode)
        {
            if (keyCode == Keys.None)
            {
                return string.Empty;
            }

            if (keyCode >= Keys.A && keyCode <= Keys.Z)
            {
                return keyCode.ToString().ToUpperInvariant();
            }

            if (keyCode >= Keys.D0 && keyCode <= Keys.D9)
            {
                return ((char)('0' + (keyCode - Keys.D0))).ToString();
            }

            if (keyCode >= Keys.F1 && keyCode <= Keys.F24)
            {
                return keyCode.ToString().ToUpperInvariant();
            }

            if (SimpleKeyNames.TryGetValue(keyCode, out string name))
            {
                return name;
            }

            return keyCode.ToString().ToUpperInvariant();
        }

        private static readonly Dictionary<Keys, string> SimpleKeyNames = new Dictionary<Keys, string>
        {
            [Keys.Space] = "SPACE",
            [Keys.Back] = "BACKSPACE",
            [Keys.Delete] = "DELETE",
            [Keys.Home] = "HOME",
            [Keys.End] = "END",
            [Keys.PageUp] = "PAGE UP",
            [Keys.PageDown] = "PAGE DOWN",
            [Keys.Oemcomma] = ",",
            [Keys.OemPeriod] = ".",
            [Keys.OemSemicolon] = ";",
            [Keys.OemQuotes] = "'",
            [Keys.OemOpenBrackets] = "[",
            [Keys.OemCloseBrackets] = "]",
            [Keys.OemPipe] = "\\",
            [Keys.OemMinus] = "-",
            [Keys.Oemplus] = "=",
            [Keys.Oemtilde] = "`",
            [Keys.OemQuestion] = "/",
            [Keys.OemBackslash] = "\\",
            [Keys.NumPad0] = "NUMPAD 0",
            [Keys.NumPad1] = "NUMPAD 1",
            [Keys.NumPad2] = "NUMPAD 2",
            [Keys.NumPad3] = "NUMPAD 3",
            [Keys.NumPad4] = "NUMPAD 4",
            [Keys.NumPad5] = "NUMPAD 5",
            [Keys.NumPad6] = "NUMPAD 6",
            [Keys.NumPad7] = "NUMPAD 7",
            [Keys.NumPad8] = "NUMPAD 8",
            [Keys.NumPad9] = "NUMPAD 9",
            [Keys.Multiply] = "NUMPAD *",
            [Keys.Add] = "NUMPAD +",
            [Keys.Subtract] = "NUMPAD -",
            [Keys.Decimal] = "NUMPAD .",
            [Keys.Divide] = "NUMPAD /",
            [Keys.NumLock] = "NUM LOCK",
            [Keys.Scroll] = "SCROLL LOCK",
            [Keys.Pause] = "PAUSE/BREAK",
            [Keys.Insert] = "INSERT",
            [Keys.PrintScreen] = "PRINT SCREEN",
            [Keys.CapsLock] = "CAPS LOCK",
            [Keys.Apps] = "APPLICATION",
            [Keys.ControlKey] = "CTRL",
            [Keys.Menu] = "ALT",
            [Keys.ShiftKey] = "SHIFT",
            [Keys.MButton] = "MOUSEMIDDLE",
            [Keys.XButton1] = "MOUSEX1",
            [Keys.XButton2] = "MOUSEX2"
        };
    }
}
