using GMA500Helper.Properties;
using GMA500Helper.System;
using log4net;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Reflection;

namespace GMA500Helper {
    public class GMA500Manager : IDisposable {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string Device { get; private set; }
        public Driver ActiveDriver { get; private set; }
        public Driver SoftwareDriver { get; private set; }
        public Driver HardwareDriver { get; private set; }

        public bool DirectXAccelerationEnabled {
            get {
                return (int)(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Avalon.Graphics", "DisableHWAcceleration", 0)) == 0;
            }
            set {
                // TODO
            }
        }
        public bool BrightnessFixEnabled {
            get {
                return Settings.Default.BrightnessFix;
            }
            set {
                // TODO
            }
        }
        public bool DwmFixEnabled {
            get {
                return Settings.Default.DwmFix;
            }
            set {
                // TODO
            }
        }

        public MonitorPowerModes MonitorPowerMode { get; private set; }
        public byte Brightness { get; private set; }

        public void SwitchDriver(Driver driver) {
            if (!ActiveDriver.Equals(driver)) {
                if (DevConManager.SwitchDriver(Device, driver)) {
                    ActiveDriver = driver;
                    Logger.InfoFormat("Active driver switched to: {0}", driver);
                } else {
                    Logger.ErrorFormat("Active driver could not be switched to: {0}", driver);
                }
            }
        }

        public void RestartGPU() {
            if (DevConManager.Restart(Device)) {
                Logger.Info("Device restarted");
            } else {
                Logger.Error("Device could not be restarted");
            }
        }

        public void ApplyBrightness() {
            if (DevConManager.SwitchDriver(Device, HardwareDriver)) {
                ActiveDriver = HardwareDriver;
                if (DevConManager.SwitchDriver(Device, SoftwareDriver)) {
                    ActiveDriver = SoftwareDriver;
                    Logger.Info("Brightness successfully applied");
                    return;
                }
            }
            Logger.Error("Brightness could to be applied");
        }

        private ProcessWatcher dwmWatcher;

        public bool Initialize() {
            Logger.Info("### Starting ###");
            
            string[] devices = DevConManager.GetDevices();
            if (devices.Length != 1) {
                Logger.Fatal("Main device not found");
                return false;
            }
            Device = devices[0];
            Logger.InfoFormat("Main device found: {0}", Device);

            Driver[] drivers = DevConManager.GetAvailableDrivers(Device);
            SoftwareDriver = drivers.FirstOrDefault(d => d.Name.Contains("Basic Display"));
            if (SoftwareDriver == null) {
                Logger.Fatal("Software driver not found");
                return false;
            }
            Logger.InfoFormat("Software driver found: {0}", SoftwareDriver);

            HardwareDriver = drivers.FirstOrDefault(d => d != SoftwareDriver);
            if (HardwareDriver == null) {
                Logger.Fatal("Hardware driver not found");
                return false;
            }
            Logger.InfoFormat("Hardware driver found: {0}", HardwareDriver);

            ActiveDriver = DevConManager.GetActiveDriver(Device);
            if (ActiveDriver == null) {
                Logger.Fatal("Active driver not found");
                return false;
            }
            Logger.InfoFormat("Active driver found: {0}", ActiveDriver);

            Logger.InfoFormat("DirectXAcceleration is {0}", DirectXAccelerationEnabled ? "Enabled" : "Disabled");
            Logger.InfoFormat("BrightnessFix is {0}", BrightnessFixEnabled ? "Enabled" : "Disabled");
            Logger.InfoFormat("DwmFix is {0}", DwmFixEnabled ? "Enabled" : "Disabled");

            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            MonitorPowerEvents.MonitorPowerModeChanged += OnMonitorPowerModeChanged;
            BrightnessEvents.BrightnessChanged += OnBrightnessChanged;
            dwmWatcher = new ProcessWatcher("dwm");
            dwmWatcher.StatusUpdate += OnDwmStatusUpdate;

            Logger.Info("### Started  ###");

            #if !DEBUG
            if (BrightnessFixEnabled && ActiveDriver.Equals(SoftwareDriver)) {
                ApplyBrightness();
            }
            #endif

            return true;
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e) {
            Logger.InfoFormat("PowerMode updated to {0}", e.Mode);
        }

        private void OnMonitorPowerModeChanged(MonitorPowerModes m) {
            Logger.InfoFormat("MonitorPowerMode updated to {0}", m);
        }

        private void OnBrightnessChanged(byte brightness) {
            Logger.InfoFormat("Brightness updated to {0}", brightness);
            if (BrightnessFixEnabled && ActiveDriver.Equals(SoftwareDriver)) {
                ApplyBrightness();
            }
        }

        private void OnDwmStatusUpdate(string name, bool running) {
            Logger.InfoFormat("Process {0} is now {1}", name, running ? "running" : "stopped");
            if (DwmFixEnabled && !running && ActiveDriver.Equals(HardwareDriver)) {
                RestartGPU();
            }
        }

        public void Dispose() {
            Logger.Info("### Stopping ###");

            if (dwmWatcher != null) {
                dwmWatcher.Dispose();
                dwmWatcher = null;
            }
            BrightnessEvents.BrightnessChanged -= OnBrightnessChanged;
            MonitorPowerEvents.MonitorPowerModeChanged -= OnMonitorPowerModeChanged;
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;

            Logger.Info("### Stopped  ###");
        }
    }
}
