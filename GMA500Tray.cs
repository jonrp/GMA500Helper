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
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(new ToolStripMenuItem("Restart GPU", null, (s, e) => manager.RestartGPU()));
            var brightnessItem = new ToolStripMenuItem("Apply brightness", null, (s, e) => manager.ApplyBrightness());
            contextMenu.Items.Add(brightnessItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            var optionsItem = new ToolStripMenuItem("Options");
            var direcxaccelerationItem = new ToolStripMenuItem("DirectX acceleration", null, (s, e) => manager.DirectXAccelerationEnabled = !manager.DirectXAccelerationEnabled);
            optionsItem.DropDownItems.Add(direcxaccelerationItem);
            var brightnessFixItem = new ToolStripMenuItem("Brightness fix", null, (s, e) => manager.BrightnessFixEnabled = !manager.BrightnessFixEnabled);
            optionsItem.DropDownItems.Add(brightnessFixItem);
            var dwmFixItem = new ToolStripMenuItem("Dwm fix", null, (s, e) => manager.DwmFixEnabled = !manager.DwmFixEnabled);
            optionsItem.DropDownItems.Add(dwmFixItem);
            contextMenu.Items.Add(optionsItem);
            contextMenu.Items.Add(new ToolStripMenuItem("Logs", null, (s, e) => Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "activity.log"))));
            contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, (s, e) => Exit()));

            contextMenu.Opening += (o, s) => {
                softwareItem.Checked = manager.ActiveDriver.Equals(manager.SoftwareDriver);
                hardwareItem.Checked = manager.ActiveDriver.Equals(manager.HardwareDriver);
                brightnessItem.Enabled = softwareItem.Checked;
                direcxaccelerationItem.Checked = manager.DirectXAccelerationEnabled;
                brightnessFixItem.Checked = manager.BrightnessFixEnabled;
                dwmFixItem.Checked = manager.DwmFixEnabled;
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
