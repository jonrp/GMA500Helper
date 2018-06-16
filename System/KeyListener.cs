//using System;
//using System.Diagnostics;
//using System.Runtime.InteropServices;

//namespace GMA500Helper.System {
//    public class KeyListener : IDisposable {
//        #region DllImport

//        private const int WH_KEYBOARD_LL = 13;
//        private const int WM_KEYDOWN = 0x0100;
//        private const int WM_KEYUP = 0x101;
//        private const int WM_SYSKEYDOWN = 0x0104;
//        private const int WM_SYSKEYUP = 0x105;

//        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

//        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
//        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

//        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

//        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
//        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

//        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
//        private static extern IntPtr GetModuleHandle(string lpModuleName);

//        #endregion

//        public event Action<int> KeyPressed;

//        private IntPtr hookID = IntPtr.Zero;
//        private LowLevelKeyboardProc hookCallback;

//        public KeyListener() {
//            using (var proc = Process.GetCurrentProcess()) {
//                using (var mod = proc.MainModule) {
//                    hookCallback = HookCallback;
//                    hookID = SetWindowsHookEx(WH_KEYBOARD_LL, hookCallback, GetModuleHandle(mod.ModuleName), 0);
//                }
//            }
//        }

//        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
//            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)) {
//                if (KeyPressed != null) {;
//                    int key = Marshal.ReadInt32(lParam);
//                    KeyPressed(key);
//                }
//            }
//            return CallNextHookEx(hookID, nCode, wParam, lParam);
//        }

//        public void Dispose() {
//            if (hookID != IntPtr.Zero) {
//                UnhookWindowsHookEx(hookID);
//                hookID = IntPtr.Zero;
//            }
//        }
//    }
//}
