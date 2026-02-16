using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal sealed class GlobalInputHook : IDisposable
    {
        private const int WhKeyboardLl = 13;
        private const int WhMouseLl = 14;

        private const int WmKeyDown = 0x0100;
        private const int WmKeyUp = 0x0101;
        private const int WmSysKeyDown = 0x0104;
        private const int WmSysKeyUp = 0x0105;
        private const int WmMButtonDown = 0x0207;
        private const int WmMButtonUp = 0x0208;
        private const int WmXButtonDown = 0x020B;
        private const int WmXButtonUp = 0x020C;

        private const int XButton1 = 0x0001;
        private const int XButton2 = 0x0002;

        private IntPtr keyboardHookId = IntPtr.Zero;
        private IntPtr mouseHookId = IntPtr.Zero;
        private readonly LowLevelKeyboardProc keyboardProc;
        private readonly LowLevelMouseProc mouseProc;
        private readonly HashSet<Keys> pressedHotkeys = new HashSet<Keys>();
        private readonly object pressedHotkeysLock = new object();
        private bool disposed;

        public event EventHandler<GlobalHotkeyEventArgs> HotkeyPressed;

        public GlobalInputHook()
        {
            keyboardProc = KeyboardHookCallback;
            mouseProc = MouseHookCallback;
        }

        public void Start()
        {
            if (keyboardHookId == IntPtr.Zero)
            {
                keyboardHookId = SetKeyboardHook(keyboardProc);
            }

            if (mouseHookId == IntPtr.Zero)
            {
                mouseHookId = SetMouseHook(mouseProc);
            }
        }

        public void Stop()
        {
            if (keyboardHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(keyboardHookId);
                keyboardHookId = IntPtr.Zero;
            }

            if (mouseHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(mouseHookId);
                mouseHookId = IntPtr.Zero;
            }

            lock (pressedHotkeysLock)
            {
                pressedHotkeys.Clear();
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int message = wParam.ToInt32();
                if (message == WmKeyDown || message == WmSysKeyDown)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    Keys key = (Keys)vkCode;
                    if (MarkPressed(key))
                    {
                        RaiseHotkeyPressed(key, isMouse: false);
                    }
                }
                else if (message == WmKeyUp || message == WmSysKeyUp)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    MarkReleased((Keys)vkCode);
                }
            }

            return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int message = wParam.ToInt32();
                if (message == WmMButtonDown)
                {
                    if (MarkPressed(Keys.MButton))
                    {
                        RaiseHotkeyPressed(Keys.MButton, isMouse: true);
                    }
                }
                else if (message == WmMButtonUp)
                {
                    MarkReleased(Keys.MButton);
                }
                else if (message == WmXButtonDown)
                {
                    MsllHookStruct mouseInfo = Marshal.PtrToStructure<MsllHookStruct>(lParam);
                    int xButton = (int)((mouseInfo.MouseData >> 16) & 0xFFFF);
                    if (xButton == XButton1)
                    {
                        if (MarkPressed(Keys.XButton1))
                        {
                            RaiseHotkeyPressed(Keys.XButton1, isMouse: true);
                        }
                    }
                    else if (xButton == XButton2)
                    {
                        if (MarkPressed(Keys.XButton2))
                        {
                            RaiseHotkeyPressed(Keys.XButton2, isMouse: true);
                        }
                    }
                }
                else if (message == WmXButtonUp)
                {
                    MsllHookStruct mouseInfo = Marshal.PtrToStructure<MsllHookStruct>(lParam);
                    int xButton = (int)((mouseInfo.MouseData >> 16) & 0xFFFF);
                    if (xButton == XButton1)
                    {
                        MarkReleased(Keys.XButton1);
                    }
                    else if (xButton == XButton2)
                    {
                        MarkReleased(Keys.XButton2);
                    }
                }
            }

            return CallNextHookEx(mouseHookId, nCode, wParam, lParam);
        }

        private void RaiseHotkeyPressed(Keys key, bool isMouse)
        {
            HotkeyPressed?.Invoke(this, new GlobalHotkeyEventArgs(key, isMouse));
        }

        private bool MarkPressed(Keys key)
        {
            lock (pressedHotkeysLock)
            {
                return pressedHotkeys.Add(key);
            }
        }

        private void MarkReleased(Keys key)
        {
            lock (pressedHotkeysLock)
            {
                pressedHotkeys.Remove(key);
            }
        }

        private static IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
        {
            using Process currentProcess = Process.GetCurrentProcess();
            using ProcessModule module = currentProcess.MainModule;
            IntPtr moduleHandle = GetModuleHandle(module.ModuleName);
            return SetWindowsHookExKeyboard(WhKeyboardLl, proc, moduleHandle, 0);
        }

        private static IntPtr SetMouseHook(LowLevelMouseProc proc)
        {
            using Process currentProcess = Process.GetCurrentProcess();
            using ProcessModule module = currentProcess.MainModule;
            IntPtr moduleHandle = GetModuleHandle(module.ModuleName);
            return SetWindowsHookExMouse(WhMouseLl, proc, moduleHandle, 0);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Stop();
            disposed = true;
            GC.SuppressFinalize(this);
        }

        ~GlobalInputHook()
        {
            Stop();
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MsllHookStruct
        {
            public Point Point;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr DwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowsHookEx")]
        private static extern IntPtr SetWindowsHookExKeyboard(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowsHookEx")]
        private static extern IntPtr SetWindowsHookExMouse(
            int idHook,
            LowLevelMouseProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    internal sealed class GlobalHotkeyEventArgs : EventArgs
    {
        public Keys Key { get; }
        public bool IsMouse { get; }

        public GlobalHotkeyEventArgs(Keys key, bool isMouse)
        {
            Key = key;
            IsMouse = isMouse;
        }
    }
}
