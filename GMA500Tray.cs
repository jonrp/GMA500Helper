using GMA500Helper.System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace GMA500Helper {
    public class GMA500Tray {
        private NotifyIcon icon;

        public GMA500Tray(GMA500Manager manager) {
            icon = new NotifyIcon {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location),
                Text = "GMA500Helper"
            };

            var contextMenu = new ContextMenuStrip {
                ShowCheckMargin = true,
                ShowImageMargin = false,
                ShowItemToolTips = false
            };

            var softwareItem = new ToolStripMenuItem("Software driver", null, (s, e) => manager.SwitchDriver(manager.SoftwareDriver));
            contextMenu.Items.Add(softwareItem);
            var hardwareItem = new ToolStripMenuItem("Hardware driver", null, (s, e) => manager.SwitchDriver(manager.HardwareDriver));
            contextMenu.Items.Add(hardwareItem);
            var direcxaccelerationItem = new ToolStripMenuItem("DirectX acceleration", null, (s, e) => {
                manager.DirectXAccelerationEnabled = !manager.DirectXAccelerationEnabled;
                if (manager.ActiveDriver.Equals(manager.HardwareDriver)) {
                    manager.RestartGPU();
                }
            });
            contextMenu.Items.Add(direcxaccelerationItem);
            contextMenu.Items.Add("Reset DirectX", null, (s, e) => {
                AvalonGraphicsManager.Reset();
                if (manager.ActiveDriver.Equals(manager.HardwareDriver)) {
                    manager.RestartGPU();
                }
            });

            contextMenu.Items.Add(new ToolStripSeparator());

            var restartItem = new ToolStripMenuItem("Restart GPU", null, (s, e) => manager.RestartGPU());
            contextMenu.Items.Add(restartItem);
            var brightnessItem = new ToolStripMenuItem("Apply brightness", null, (s, e) => manager.ApplyBrightness());
            contextMenu.Items.Add(brightnessItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var autoRunItem = new ToolStripMenuItem("AutoRun", null, (s, e) => manager.AutorunEnabled = !manager.AutorunEnabled);
            contextMenu.Items.Add(autoRunItem);
            contextMenu.Items.Add(new ToolStripMenuItem("Logs", null, (s, e) => Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "GMA500Helper.log"))));
            contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, (s, e) => Exit()));

            contextMenu.Opening += (o, s) => {
                softwareItem.Checked = manager.ActiveDriver.Equals(manager.SoftwareDriver);
                hardwareItem.Checked = manager.ActiveDriver.Equals(manager.HardwareDriver);
                direcxaccelerationItem.Checked = manager.DirectXAccelerationEnabled;
                restartItem.Enabled = hardwareItem.Checked;
                brightnessItem.Enabled = softwareItem.Checked;
                autoRunItem.Checked = manager.AutorunEnabled;
            };

            icon.ContextMenuStrip = contextMenu;
        }

        public void Run() {
            icon.Visible = true;
            Application.Run();
        }

        public void Exit() {
            icon.Visible = false;
            Application.Exit();
        }
    }
}
