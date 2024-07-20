using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UN7ZO.HamCockpitPlugins.WheelTuning {
    public class MouseWheelHook {

        private const int WM_MOUSEWHEEL = 0x020A;

        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        public event MouseEventHandler MouseWheelEvent;

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseLL_HookStruct {
            /// <summary>
            /// Specifies a Point structure that contains the X- and Y-coordinates of the cursor, in screen coordinates. 
            /// </summary>
            public Point Point;

            /// <summary>
            /// If the message is WM_MOUSEWHEEL, the high-order word of this member is the wheel delta. 
            /// The low-order word is reserved. A positive value indicates that the wheel was rotated forward, 
            /// away from the user; a negative value indicates that the wheel was rotated backward, toward the user. 
            /// One wheel click is defined as WHEEL_DELTA, which is 120. 
            ///If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP,
            /// or WM_NCXBUTTONDBLCLK, the high-order word specifies which X button was pressed or released, 
            /// and the low-order word is reserved. This value can be one or more of the following values. Otherwise, MouseData is not used. 
            ///XBUTTON1
            ///The first X button was pressed or released.
            ///XBUTTON2
            ///The second X button was pressed or released.
            /// </summary>
            public int MouseData;

            /// <summary>
            /// Specifies the event-injected flag. An application can use the following value to test the mouse Flags. Value Purpose 
            ///LLMHF_INJECTED Test the event-injected flag.  
            ///0
            ///Specifies whether the event was injected. The value is 1 if the event was injected; otherwise, it is 0.
            ///1-15
            ///Reserved.
            /// </summary>
            public int Flags;

            /// <summary>
            /// Specifies the Time stamp for this message.
            /// </summary>
            public int Time;

            /// <summary>
            /// Specifies extra information associated with the message. 
            /// </summary>
            public int ExtraInfo;
        }

        public enum HookType {
            WH_KEYBOARD = 2,
            WH_MOUSE = 7,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowsHookEx(HookType hookType, HookProc callback, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        private HookProc _globalLlMouseHookCallback;
        private int _hGlobalLlMouseHook;

        public void SetUpHook() {
            Debug.WriteLine("Setting up global mouse hook");

            _globalLlMouseHookCallback = MouseHookProc;

            _hGlobalLlMouseHook = SetWindowsHookEx(HookType.WH_MOUSE_LL, _globalLlMouseHookCallback, IntPtr.Zero, 0);

            if (_hGlobalLlMouseHook == 0) {
                Debug.WriteLine("Unable to set global mouse hook");
            }
        }

        public void ClearHook() {
            Debug.WriteLine("Deleting global mouse hook");

            if (_hGlobalLlMouseHook != 0) {
                // Unhook the low-level mouse hook
                var err = UnhookWindowsHookEx(_hGlobalLlMouseHook);
                if (err == 0) {
                    Debug.WriteLine("Unable to clear Mouse hook");
                }

                _hGlobalLlMouseHook = 0;
            }
        }

        private int MouseHookProc(int nCode, int wParam, IntPtr lParam) {
            if (!ApplicationIsActivated())
                return CallNextHookEx(_hGlobalLlMouseHook, nCode, wParam, lParam);

            if (nCode >= 0) {
                MouseLL_HookStruct mouseHookStruct = (MouseLL_HookStruct) Marshal.PtrToStructure(lParam, typeof(MouseLL_HookStruct));

                switch (wParam) {
                    case WM_MOUSEWHEEL:
                        //If the message is WM_MOUSEWHEEL, the high-order word of MouseData member is the wheel delta. 
                        //One wheel click is defined as WHEEL_DELTA, which is 120. 
                        //(value >> 16) & 0xffff; retrieves the high-order word from the given 32-bit value
                        var mouseDelta = (short)((mouseHookStruct.MouseData >> 16) & 0xffff);
                        MouseButtons button = MouseButtons.None;

                        //generate event 
                        MouseEventArgs e = new MouseEventArgs(button, 0, 
                            mouseHookStruct.Point.X, mouseHookStruct.Point.Y, mouseDelta);

                        MouseWheelEvent.Invoke(null, e);

                        break;
                }
            }

            //call next hook
            return CallNextHookEx(_hGlobalLlMouseHook, nCode, wParam, lParam);
        }

        public static bool ApplicationIsActivated() {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    }
}