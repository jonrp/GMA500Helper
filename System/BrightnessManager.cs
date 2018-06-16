using System;
using System.Linq;
using System.Management;

namespace GMA500Helper.System {
    public class Brightness {
        public byte CurrentValue { get; private set; }
        public byte[] SupportedLevels { get; private set; }
        public int CurrentLevel { get; private set; }

        public Brightness(byte value, byte[] supported, int level) {
            CurrentValue = value;
            SupportedLevels = supported;
            CurrentLevel = level;
        }
    }

    public static class BrightnessManager {
        public static Brightness Get() {
            try {
                var scope = new ManagementScope(@"root\WMI");
                var query = new SelectQuery("select * from WmiMonitorBrightness");
                using (var searcher = new ManagementObjectSearcher(scope, query)) {
                    using (var objects = searcher.Get()) {
                        var wmi = objects.Cast<ManagementObject>().FirstOrDefault();
                        if (wmi != null) {
                            byte value = (byte)wmi.GetPropertyValue("CurrentBrightness");
                            byte[] supported = (byte[])wmi.GetPropertyValue("Level");
                            return new Brightness(value, supported, Array.IndexOf(supported, value));
                        }
                    }
                }
            }
            catch {
                // Not supported
            }
            return new Brightness(0, new byte[0], -1);
        }

        public static bool Set(byte target) {
            if (target > 100) {
                return false;
            }

            try {
                byte supportedTarget = target;

                // In case it's needed
                //var supportedLevels = Get().SupportedLevels;
                //for (int i = 0; i < supportedLevels.Length; i++) {
                //    if (supportedLevels[i] == target) {
                //        break;
                //    }
                //    if (supportedLevels[i] > target) {
                //        supportedTarget = supportedLevels[i == 0 || supportedLevels[i] - target < target - supportedLevels[i - 1] ? i : i - 1];
                //        break;
                //    }
                //}

                var scope = new ManagementScope(@"root\WMI");
                var query = new SelectQuery("select * from WmiMonitorBrightnessMethods");
                using (var searcher = new ManagementObjectSearcher(scope, query)) {
                    using (var objects = searcher.Get()) {
                        var wmi = objects.Cast<ManagementObject>().FirstOrDefault();
                        if (wmi != null) {
                            wmi.InvokeMethod("WmiSetBrightness", new object[] { 10, supportedTarget });
                        }
                    }
                }
            }
            catch {
                // Not supported
            }
            return false;
        }

        private static BrightnessEventWatcher brightnessEventWatcher;
        private static event Action<byte> brightnessUpdate;
        public static event Action<byte> BrightnessUpdate {
            add {
                try {
                    if (brightnessUpdate == null) {
                        brightnessEventWatcher = new BrightnessEventWatcher();
                        brightnessEventWatcher.Event += OnBrightnessEvent;
                    }
                    brightnessUpdate += value;
                }
                catch {
                    // Not supported
                }
            }
            remove {
                try {
                    brightnessUpdate -= value;
                    if (brightnessUpdate == null) {
                        brightnessEventWatcher.Event -= OnBrightnessEvent;
                        brightnessEventWatcher.Dispose();
                        brightnessEventWatcher = null;
                    }
                }
                catch {
                    // Not supported
                }
            }
        }

        private static void OnBrightnessEvent(byte value) {
            brightnessUpdate(value);
        }

        private class BrightnessEventWatcher : IDisposable {
            private ManagementEventWatcher watcher;

            public BrightnessEventWatcher() {
                var scope = new ManagementScope(@"root\WMI");
                var query = new EventQuery("select * from WmiMonitorBrightnessEvent");
                watcher = new ManagementEventWatcher(scope, query);
                watcher.EventArrived += OnEvent;
                watcher.Start();
            }

            private void OnEvent(object sender, EventArrivedEventArgs e) {
                try {
                    Event?.Invoke((byte)e.NewEvent.GetPropertyValue("Brightness"));
                }
                catch {
                    // Not supported
                }
            }

            public event Action<byte> Event;

            public void Dispose() {
                watcher.Stop();
                watcher.Dispose();
                watcher = null;
            }
        }
    }
}
