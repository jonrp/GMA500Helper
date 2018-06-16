using GMA500Helper.System;
using log4net;
using log4net.Config;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using GMA500Helper.Properties;
using Microsoft.Win32;
using System.Security.Principal;
using System.Diagnostics;
using System.IO;

namespace GMA500Helper.Runners {
    public static class TrayRunner {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread]
        public static void Run() {
            XmlConfigurator.Configure();

            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) {
                Logger.Fatal("Must be run as Administrator");
                return;
            }
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1) {
                Logger.Fatal("Another instance is already running");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (Tray tray = new Tray()) {
                if (tray.Start()) {
                    //Application.AddMessageFilter(tray);
                    Application.Run(tray);
                    //Application.RemoveMessageFilter(tray);
                }
            }
        }

        private class Tray : /*NativeWindow*/ApplicationContext/*, IMessageFilter*/, IDisposable {
            private NotifyIcon icon;

            private string device;
            private Driver activeDriver;
            private Driver softwareDriver;
            private Driver hardwareDriver;

            private byte currentBrightness;

            private readonly MonitorPowerManager mpManager = new MonitorPowerManager();
            private ProcessWatcher dwmWatcher;

            //public bool PreFilterMessage(ref Message m) {
            //    //mpManager.PreFilterMessage(ref m);
            //    return false;
            //}

            public bool Start() {
                Logger.Info("### Starting ###");

                #region System bindings

                string[] devices = DevConManager.GetDevices();
                if (devices.Length != 1) {
                    Logger.Fatal("Main device not found");
                    return false;
                }
                device = devices[0];
                Logger.InfoFormat("Main device found: {0}", device);

                Driver[] drivers = DevConManager.GetAvailableDrivers(device);
                softwareDriver = drivers.FirstOrDefault(d => d.Name.Contains("Basic Display"));
                if (softwareDriver == null) {
                    Logger.Fatal("Software driver not found");
                    return false;
                }
                Logger.InfoFormat("Software driver found: {0}", softwareDriver);
                hardwareDriver = drivers.FirstOrDefault(d => d != softwareDriver);
                if (hardwareDriver == null) {
                    Logger.Fatal("Hardware driver not found");
                    return false;
                }
                Logger.InfoFormat("Hardware driver found: {0}", hardwareDriver);
                activeDriver = DevConManager.GetActiveDriver(device);
                if (activeDriver == null) {
                    Logger.Fatal("Active driver not found");
                    return false;
                }
                Logger.InfoFormat("Active driver found: {0}", activeDriver);

                currentBrightness = BrightnessManager.Get().CurrentValue;
                Logger.InfoFormat("Brightness set to {0}", currentBrightness);
                BrightnessManager.BrightnessUpdate += OnBrightnessUpdate;

                SystemEvents.PowerModeChanged += OnPowerModeChanged;
                //mpManager.MonitorPowerModeChanged += OnMonitorPowerModeChanged;

                dwmWatcher = new ProcessWatcher("dwm");
                dwmWatcher.StatusUpdate += OnDwmUpdate;

                #endregion

                #region NotifyIcon bindings

                icon = new NotifyIcon();
                icon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
                icon.Text = "GMA500Helper"; // TODO add version

                var contextMenu = new ContextMenuStrip {
                    ShowCheckMargin = true,
                    ShowImageMargin = false,
                    ShowItemToolTips = false
                };

                var softwareItem = new ToolStripMenuItem("Software driver", null,
                    (s, e) => SwitchDriver(softwareDriver));
                contextMenu.Items.Add(softwareItem);
                var hardwareItem = new ToolStripMenuItem("Hardware driver", null,
                    (s, e) => SwitchDriver(hardwareDriver));
                contextMenu.Items.Add(hardwareItem);
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add(new ToolStripMenuItem("Restart GPU", null,
                    (s, e) => RestartDevice()));
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add(new ToolStripMenuItem("Logs", null,
                    (s, e) => Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "activity.log"))));
                contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, Exit));

                contextMenu.Opening += (o, s) => {
                    softwareItem.Checked = activeDriver.Equals(softwareDriver);
                    hardwareItem.Checked = activeDriver.Equals(hardwareDriver);
                };

                icon.ContextMenuStrip = contextMenu;
                icon.Visible = true;

                #endregion

                Logger.Info("### Started  ###");

                #if !DEBUG
                OnBrightnessUpdate(currentBrightness);
                #endif

                return true;
            }
            
            private void OnBrightnessUpdate(byte newBrightness) {
                if (newBrightness != currentBrightness) {
                    Logger.InfoFormat("Brightness updated to {0}", newBrightness);
                    currentBrightness = newBrightness;
                    ApplyBrightness();
                }
            }

            private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e) {
                Logger.InfoFormat("PowerMode updated to {0}", e.Mode);
            }

            //private void OnMonitorPowerModeChanged(MonitorPowerModes m) {
            //    Logger.InfoFormat("MonitorPowerMode updated to {0}", m);
            //}

            private void SwitchDriver(Driver driver) {
                if (!activeDriver.Equals(driver)) {
                    if (DevConManager.SwitchDriver(device, driver)) {
                        activeDriver = driver;
                        Logger.InfoFormat("Active driver switched to: {0}", driver);
                    }
                    else {
                        Logger.ErrorFormat("Active driver could not be switched to: {0}", driver);
                    }
                }
            }

            private void RestartDevice() {
                if (DevConManager.Restart(device)) {
                    Logger.Info("Device restarted");
                } else {
                    Logger.Error("Device could not be restarted");
                }
            }

            private void ApplyBrightness() {
                if (Settings.Default.BrightnessFix && activeDriver.Equals(softwareDriver)) {
                    if (DevConManager.SwitchDriver(device, hardwareDriver)) {
                        activeDriver = hardwareDriver;
                        if (DevConManager.SwitchDriver(device, softwareDriver)) {
                            activeDriver = softwareDriver;
                            Logger.Info("Brightness successfully applied");
                            return;
                        }
                    }
                    Logger.Error("Brightness could to be applied");
                }
            }

            private void OnDwmUpdate(string name, bool running) {
                Logger.InfoFormat("Process {0} is now {1}", name, running ? "running" : "stopped");
                if (Settings.Default.DwmFix && !running && activeDriver.Equals(hardwareDriver)) {
                    RestartDevice();
                }
            }

            private void Exit(object sender, EventArgs args) {
                icon.Visible = false;
                Logger.Info("### Stopping ###");

                dwmWatcher.Dispose();
                BrightnessManager.BrightnessUpdate -= OnBrightnessUpdate;
                //mpManager.MonitorPowerModeChanged -= OnMonitorPowerModeChanged;
                mpManager.Dispose();
                SystemEvents.PowerModeChanged -= OnPowerModeChanged;

                Logger.Info("### Stopped  ###");
                Application.Exit();
            }

            //public void Dispose() {
            //    icon.Visible = false;
            //}

            //protected override void WndProc(ref Message m) {
            //    // TODO intercept special keys
            //    base.WndProc(ref m);
            //}
        }
    }
}
