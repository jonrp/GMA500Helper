using System;
using System.Management;

namespace GMA500Helper.System {
    public static class BrightnessEvents {
        private static BrightnessEventsWatcher brightnessEventsWatcher;
        private static event Action<byte> brightnessChanged;
        public static event Action<byte> BrightnessChanged {
            add {
                if (brightnessChanged == null && brightnessEventsWatcher == null) {
                    brightnessEventsWatcher = new BrightnessEventsWatcher();
                    brightnessEventsWatcher.BrightnessEvent += OnBrightnessEvent;
                }
                brightnessChanged += value;
            }
            remove {
                brightnessChanged -= value;
                if (brightnessChanged == null && brightnessEventsWatcher != null) {
                    brightnessEventsWatcher.BrightnessEvent -= OnBrightnessEvent;
                    brightnessEventsWatcher.Dispose();
                    brightnessEventsWatcher = null;
                    brightness = 255;
                }
            }
        }

        private static byte brightness = 255;
        private static void OnBrightnessEvent(byte brightness) {
            if (BrightnessEvents.brightness != brightness) {
                BrightnessEvents.brightness = brightness;
                brightnessChanged(brightness);
            }
        }

        private class BrightnessEventsWatcher : IDisposable {
            private ManagementEventWatcher watcher;

            public BrightnessEventsWatcher() {
                var scope = new ManagementScope(@"root\WMI");
                var query = new EventQuery("select * from WmiMonitorBrightnessEvent");
                watcher = new ManagementEventWatcher(scope, query);
                watcher.EventArrived += OnEventArrived;
                watcher.Start();
            }

            private void OnEventArrived(object sender, EventArrivedEventArgs e) {
                BrightnessEvent?.Invoke((byte)e.NewEvent.GetPropertyValue("Brightness"));
            }

            public event Action<byte> BrightnessEvent;

            public void Dispose() {
                watcher.Stop();
                watcher.Dispose();
                watcher = null;
            }
        }
    }
}
