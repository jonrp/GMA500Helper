using GMA500Helper.System;
using GMA500Helper.Tools;
using System;
using System.Linq;

namespace GMA500Helper.Runners {
    public static class ConsoleRunner {
        public static void Run() {
            string[] devices = DevConManager.GetDevices();
            Driver[][] drivers = new Driver[devices.Length][];
            for (int i = 0; i < devices.Length; i++) {
                Console.WriteLine("### [" + i + "] " + devices[i] + " ###");
                Console.WriteLine("Active Driver: " + DevConManager.GetActiveDriver(devices[i]));
                Console.WriteLine("Available Drivers:");
                drivers[i] = DevConManager.GetAvailableDrivers(devices[i]);
                for (int j = 0; j < drivers[i].Length; j++) {
                    Console.WriteLine("[" + j + "] " + drivers[i][j]);
                }
            }

            Console.WriteLine("### Brightness ###");
            var brightness = BrightnessManager.Get();
            Console.WriteLine("Current: [" + brightness.CurrentLevel + "] " + brightness.CurrentValue);
            Console.WriteLine("Supported: { " + string.Join(", ", brightness.SupportedLevels.Select((v, i) => "[" + i + "] " + v)) + " }");

            Console.WriteLine();

            BrightnessManager.BrightnessUpdate += b => Console.WriteLine("* Brightness updated to " + b + "*");

            ConsoleProcessor cp = new ConsoleProcessor();
            cp.Register("switch-driver", new[] { "device", "driver" }, a => DevConManager.SwitchDriver(devices[int.Parse(a[1])], drivers[int.Parse(a[1])][int.Parse(a[2])]));
            cp.Register("restart-device", new[] { "device" }, a => DevConManager.Restart(devices[int.Parse(a[1])]));
            cp.Register("set-brightness", new[] { "brightness" }, a => BrightnessManager.Set(byte.Parse(a[1])));

            using (var pw = new ProcessWatcher("dwm.exe")) {
                pw.StatusUpdate += (n, s) => Console.WriteLine("* " + n + " is " + (s ? "running" : "stopped") + " *");

                cp.Run();
            }
        }
    }
}
