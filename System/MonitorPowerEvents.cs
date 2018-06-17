using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GMA500Helper.System {
    public static class MonitorPowerEvents {
        private static MonitorPowerModes powerMode = MonitorPowerModes.Unknown;

        private static void Update() {
            MonitorPowerModes mode = displayState == DisplayState.Unknown || lidState == LidState.Unknown
                ? MonitorPowerModes.Unknown
                : displayState == DisplayState.On && lidState == LidState.Open
                    ? MonitorPowerModes.On
                    : MonitorPowerModes.Off;
            if (mode != powerMode) {
                powerMode = mode;
                monitorPowerModeChanged(mode);
            }
        }

        private static MonitorPowerEventsWatcher monitorPowerEventsWatcher;
        private static event Action<MonitorPowerModes> monitorPowerModeChanged;
        public static event Action<MonitorPowerModes> MonitorPowerModeChanged {
            add {
                if (monitorPowerModeChanged == null && monitorPowerEventsWatcher == null) {
                    monitorPowerEventsWatcher = new MonitorPowerEventsWatcher();
                    monitorPowerEventsWatcher.DisplayEvent += OnDisplayEvent;
                    monitorPowerEventsWatcher.LidEvent += OnLidEvent;
                }
                monitorPowerModeChanged += value;
            }
            remove {
                monitorPowerModeChanged -= value;
                if (monitorPowerModeChanged == null && monitorPowerEventsWatcher != null) {
                    monitorPowerEventsWatcher.DisplayEvent -= OnDisplayEvent;
                    monitorPowerEventsWatcher.LidEvent -= OnLidEvent;
                    monitorPowerEventsWatcher.Dispose();
                    monitorPowerEventsWatcher = null;
                    displayState = DisplayState.Unknown;
                    lidState = LidState.Unknown;
                }
            }
        }

        private static DisplayState displayState = DisplayState.Unknown;
        private static void OnDisplayEvent(DisplayState displayState) {
            MonitorPowerEvents.displayState = displayState;
            Update();
        }

        private static LidState lidState = LidState.Unknown;
        private static void OnLidEvent(LidState lidState) {
            MonitorPowerEvents.lidState = lidState;
            Update();
        }
        
        private class MonitorPowerEventsWatcher : NativeWindow, IDisposable {
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

            private IntPtr handleDisplay;
            private IntPtr handleLid;

            public MonitorPowerEventsWatcher() {
                CreateHandle(new CreateParams());
                handleDisplay = RegisterPowerSettingNotification(Handle, ref GUID_CONSOLE_DISPLAY_STATE, DEVICE_NOTIFY_WINDOW_HANDLE);
                handleLid = RegisterPowerSettingNotification(Handle, ref GUID_LIDSWITCH_STATE_CHANGE, DEVICE_NOTIFY_WINDOW_HANDLE);
            }

            protected override void WndProc(ref Message m) {
                switch (m.Msg) {
                    case WM_POWERBROADCAST:
                        if ((int)m.WParam == PBT_POWERSETTINGCHANGE) {
                            POWERBROADCAST_SETTING pbs = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(POWERBROADCAST_SETTING));
                            if (pbs.PowerSetting == GUID_CONSOLE_DISPLAY_STATE) {
                                DisplayEvent?.Invoke((DisplayState)pbs.Data);
                            } else if (pbs.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE) {
                                LidEvent?.Invoke((LidState)pbs.Data);
                            }
                        }
                        break;
                }
                base.WndProc(ref m);
            }

            public event Action<DisplayState> DisplayEvent;
            public event Action<LidState> LidEvent;

            public void Dispose() {
                UnregisterPowerSettingNotification(handleDisplay);
                UnregisterPowerSettingNotification(handleLid);
                ReleaseHandle();
            }
        }
        
        private enum DisplayState {
            Unknown =-1,
            Off = 0,
            On = 1,
            Dimmed = 2
        }

        private enum LidState {
            Unknown = -1,
            Closed = 0,
            Open = 1
        }
    }

    public enum MonitorPowerModes {
        Unknown = -1,
        Off = 0,
        On = 1
    }
}
