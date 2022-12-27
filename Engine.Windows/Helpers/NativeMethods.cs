using SharpDX.Mathematics.Interop;
using SharpDX.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Engine.Windows.Helpers
{
    /// <summary>
    /// Windows API functions
    /// </summary>
    static partial class NativeMethods
    {
        /// <summary>
        /// Gets current keyboard state
        /// </summary>
        /// <param name="lpKeyState">Key state array</param>
        /// <returns>Returns true if the state retrieved</returns>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetKeyboardState(byte[] lpKeyState);
        /// <summary>
        /// Converts key code to unicode string
        /// </summary>
        /// <param name="uVirtKey">Virtual key code</param>
        /// <param name="scanCode">Scan code</param>
        /// <param name="lpKeyState">Keyboard state</param>
        /// <param name="lpChar">Result string</param>
        /// <param name="bufferSize">Result capacity</param>
        /// <param name="uFlags">Flags</param>
        /// <returns>Return the number of characters in the result buffer</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int ToUnicode(uint uVirtKey, uint scanCode, byte[] lpKeyState, [Out] StringBuilder lpChar, int bufferSize, uint uFlags);
        /// <summary>
        /// Maps a virtual key code
        /// </summary>
        /// <param name="uVirtKey">Virtual key code</param>
        /// <param name="uMapType">Map type</param>
        /// <returns>Returns the scan code</returns>
        [LibraryImport("user32.dll")]
        private static partial uint MapVirtualKey(uint uVirtKey, uint uMapType);

        /// <summary>
        /// Main keyboard state buffer
        /// </summary>
        private static readonly byte[] keyState = new byte[256];
        /// <summary>
        /// Previous keyboard state buffer
        /// </summary>
        private static byte[] prevKeyState = new byte[256];
        /// <summary>
        /// Pressed key collection
        /// </summary>
        private static readonly List<Keys> pressedKeys = new List<Keys>(256);

        /// <summary>
        /// Gets the pressed key collection
        /// </summary>
        /// <returns>Returns a pressed key collection</returns>
        public static Keys[] GetPressedKeys()
        {
            pressedKeys.Clear();

            prevKeyState = (byte[])keyState.Clone();

            if (GetKeyboardState(keyState))
            {
                for (int i = 0; i < keyState.Length; i++)
                {
                    if ((keyState[i] & 0x80) != 0)
                    {
                        //Pressed
                        pressedKeys.Add((Keys)i);
                    }
                }
            }

            return pressedKeys.ToArray();
        }

        /// <summary>
        /// Last stroked key is dead
        /// </summary>
        private static bool lastIsDead = false;
        /// <summary>
        /// Last dead key code
        /// </summary>
        private static uint deadKey = 0;
        /// <summary>
        /// Last dead scan code
        /// </summary>
        private static uint deadScanCode = 0;
        /// <summary>
        /// Keyboard state buffer when the dead key was stroked
        /// </summary>
        private static byte[] deadKeyState = new byte[256];

        /// <summary>
        /// Gets the keyboard key strokes
        /// </summary>
        /// <returns>Returns the stroked key strings</returns>
        public static string GetStrokes()
        {
            StringBuilder sb = new StringBuilder();

            for (uint i = 0; i < keyState.Length; i++)
            {
                if (IgnoreKey(i))
                {
                    continue;
                }

                bool lastIsDown = (prevKeyState[i] & 0x80) != 0;
                bool currIsDown = (keyState[i] & 0x80) != 0;

                if (lastIsDown && !currIsDown)
                {
                    //Just released
                    string res = ConvertToUnicode(i, keyState);

                    sb.Append(res);
                }
            }

            return sb.ToString();
        }
        /// <summary>
        /// Evaluates if the specified key must be ignored or not
        /// </summary>
        /// <param name="keyCode">Key code</param>
        /// <returns>Returns true if the specified key must be ignored</returns>
        private static bool IgnoreKey(uint keyCode)
        {
            Keys key = (Keys)keyCode;

            if (key == Keys.ShiftKey || key == Keys.RShiftKey || key == Keys.LShiftKey)
            {
                return true;
            }

            if (key == Keys.ControlKey || key == Keys.RControlKey || key == Keys.LControlKey)
            {
                return true;
            }

            if (key == Keys.Alt || key == Keys.Menu)
            {
                return true;
            }

            if (key == Keys.RWin || key == Keys.LWin)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Convert key to unicode string
        /// </summary>
        /// <param name="key">Key code</param>
        /// <param name="keyState">Keyboard state</param>
        /// <returns>Returns the resulting unicode string</returns>
        private static string ConvertToUnicode(uint key, byte[] keyState)
        {
            string res = string.Empty;

            if (deadKey != 0 && lastIsDead)
            {
                StringBuilder dsb = new StringBuilder(5);
                _ = ToUnicode(deadKey, deadScanCode, deadKeyState, dsb, dsb.Capacity, 0);
                lastIsDead = false;
                deadKey = 0;
                deadScanCode = 0;
            }

            uint scanCode = MapVirtualKey(key, 0);
            StringBuilder sb = new StringBuilder(5);
            int result = ToUnicode(key, scanCode, keyState, sb, sb.Capacity, 0);
            switch (result)
            {
                case 0:
                    break;
                case 1:
                    res = sb[0].ToString();
                    break;
                default:
                    lastIsDead = true;
                    deadKey = key;
                    deadScanCode = scanCode;
                    deadKeyState = (byte[])keyState.Clone();
                    ClearKeyboardBuffer(key, scanCode);
                    break;
            }

            return res;
        }
        /// <summary>
        /// Clears the keyboard buffer
        /// </summary>
        /// <param name="key">Key code</param>
        /// <param name="scanCode">Scan code</param>
        private static void ClearKeyboardBuffer(uint key, uint scanCode)
        {
            StringBuilder sb = new StringBuilder(10);

            int rc;
            do
            {
                byte[] keyStateNull = new byte[256];
                rc = ToUnicode(key, scanCode, keyStateNull, sb, sb.Capacity, 0);
            } while (rc < 0);
        }

        [DllImport("user32.dll", EntryPoint = "GetClientRect")]
        public static extern bool GetClientRect(IntPtr hWnd, out RawRectangle lpRect);
        [DllImport("user32.dll", EntryPoint = "PeekMessage")]
        public static extern int PeekMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax, int wRemoveMsg);
        [DllImport("user32.dll", EntryPoint = "GetMessage")]
        public static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax);
        [DllImport("user32.dll", EntryPoint = "TranslateMessage")]
        public static extern int TranslateMessage(ref NativeMessage lpMsg);
        [DllImport("user32.dll", EntryPoint = "DispatchMessage")]
        public static extern int DispatchMessage(ref NativeMessage lpMsg);
    }
}
