using log4net;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GMA500Helper.System {
    public class MonitorPowerManager : NativeWindow, IDisposable {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //private const int WM_SYSCOMMAND = 0x0112;
        //private const int SC_MONITORPOWER = 0xF170;
        //private const int MONITORON = -1;
        //private const int MONITORSTANBY = 1;
        //private const int MONITOROFF = 2;

        private const int WM_POWERBROADCAST = 0x0218;
        private const int PBT_POWERSETTINGCHANGE = 0x8013;

        private static Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");
        private static Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid("ba3e0f4d-b817-4094-a2d1-d56379e6a0f3");

        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        [DllImport("User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

        [DllImport("User32", EntryPoint = "UnregisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr handle);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct POWERBROADCAST_SETTING {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        private IntPtr handlePower;
        private IntPtr handleLid;

        public MonitorPowerManager() {
            CreateHandle(new CreateParams {  });
            handlePower = RegisterPowerSettingNotification(Handle, ref GUID_CONSOLE_DISPLAY_STATE, DEVICE_NOTIFY_WINDOW_HANDLE);
            handleLid = RegisterPowerSettingNotification(Handle, ref GUID_LIDSWITCH_STATE_CHANGE, DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        public void Dispose() {
            UnregisterPowerSettingNotification(handlePower);
            UnregisterPowerSettingNotification(handleLid);
            ReleaseHandle();
        }

        //private MonitorPowerModes activeMode = MonitorPowerModes.Unknown;

        protected override void WndProc(ref Message m) {
            //if (m.Msg == WM_SYSCOMMAND) {
            //    if ((m.WParam.ToInt32() & 0xFFF0) == SC_MONITORPOWER) {
            //        activeMode = (MonitorPowerModes)m.LParam.ToInt32();
            //        monitorPowerModeChanged?.Invoke(activeMode);
            //    }
            //}
            switch (m.Msg) {
                case WM_POWERBROADCAST:
                    if ((int)m.WParam == PBT_POWERSETTINGCHANGE) {
                        POWERBROADCAST_SETTING pbs = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(POWERBROADCAST_SETTING));
                        if (pbs.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE) {
                            //activeMode = pbs.Data!= 0 ? MonitorPowerModes.On : MonitorPowerModes.Off;
                            //monitorPowerModeChanged?.Invoke(activeMode);
                            Logger.InfoFormat("Lid is {0}", pbs.Data == 0 ? "Closed" : "Open");
                        }
                        else if (pbs.PowerSetting == GUID_CONSOLE_DISPLAY_STATE) {
                            Logger.InfoFormat("Monitor is {0}", pbs.Data == 0 ? "Off" : pbs.Data == 1 ? "On" : "Dimmed");
                        }
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        //public bool PreFilterMessage(ref Message m) {
        //    if (m.Msg == WM_SYSCOMMAND) {
        //        if ((m.WParam.ToInt32() & 0xFFF0) == SC_MONITORPOWER) {
        //            activeMode = (MonitorPowerModes)m.LParam.ToInt32();
        //            monitorPowerModeChanged?.Invoke(activeMode);
        //        }
        //    }
        //    return false;
        //}

        private event Action<MonitorPowerModes> monitorPowerModeChanged;
        public event Action<MonitorPowerModes> MonitorPowerModeChanged {
            add {
                //value(activeMode);
                monitorPowerModeChanged += value;
            }
            remove {
                monitorPowerModeChanged -= value;
            }
        }
    }

    public enum MonitorPowerModes {
        On = -1,
        Unknown = 0,
        Standby = 1,
        Off = 2
    }
}
