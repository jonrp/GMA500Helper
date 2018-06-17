using log4net;
using log4net.Config;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

namespace GMA500Helper {
    public class Program {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread]
        public static void Main() {
            XmlConfigurator.Configure();

            if (Environment.OSVersion.Version.Major < 10) {
                return;
            }
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) {
                return;
            }
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1) {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var manager = new GMA500Manager()) {
                if (manager.Initialize()) {
                    var tray = new GMA500Tray(manager);
                    tray.Run();
                }
            }
        }
    }
}
