using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GMA500Helper.System {
    public class Driver {
        public string INF { get; private set; }
        public string Name { get; private set; }

        public Driver(string inf, string name) {
            INF = inf;
            Name = name;
        }

        public override string ToString() {
            return Name + " @ [" + INF + "]";
        }

        public override bool Equals(object obj) {
            Driver other = obj as Driver;
            return other != null && INF == other.INF && Name == other.Name;
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + INF.GetHashCode();
                hash = hash * 23 + Name.GetHashCode();
                return hash;
            }
        }
    }

    public static class DevConManager {
        public static string[] GetDevices() {
            var output = Execute("listclass", "Display");
            if (output.Length > 0) {
                return output.Skip(1).Select(l => l.Split(new[] { " : " }, StringSplitOptions.None))
                    // TODO Ensure this works on other GPUs
                    .Select(s => s[0].Substring(null, "&SUBSYS").Trim()).ToArray();
            }
            return new string[0];
        }

        public static Driver GetActiveDriver(string device) {
            var output = Execute("driverfiles", '"' + device + '"');
            if (output.Length >= 4) {
                return new Driver(output[2].Substring("from ", "[").Trim(), output[1].Substring(": ", null).Trim());
            }
            return null;
        }

        public static Driver[] GetAvailableDrivers(string device) {
            List<Driver> drivers = new List<Driver>();
            var output = Execute("drivernodes", '"' + device + '"');
            for (int i = 0; i < output.Length; i++) {
                if (output[i].StartsWith("Driver node #")) {
                    drivers.Add(new Driver(output[i + 1].Substring("is ", null).Trim(), output[i + 3].Substring("is ", null).Trim()));
                }
            }
            return drivers.ToArray();
        }

        public static bool SwitchDriver(string device, Driver driver) {
            if (!Equals(GetActiveDriver(device), driver)) {
                var output = Execute("updateni", '"' + driver.INF + '"', '"' + device + '"');
                return output.Length > 1 && output[1] == "Drivers installed successfully.";
            }
            return true;
        }

        public static bool Restart(string device) {
            var output = Execute("restart", '"' + device + '"');
            return output.Length > 0 && output[0].StartsWith(device) && output[0].EndsWith(" : Restarted");
        }

        #region Helpers

        private static string[] Execute(params string[] args) {
            try {
                ProcessStartInfo psi = new ProcessStartInfo("devcon.exe", string.Join(" ", args));
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                using (Process p = Process.Start(psi)) {
                    return p.StandardOutput.ReadToEnd()
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim())
                        .ToArray();
                }
            } catch {
                return new string[0];
            }
        }

        private static string Substring(this string str, string prefix, string suffix) {
            int offset = 0;
            int length = str.Length;
            if (!string.IsNullOrEmpty(prefix)) {
                offset = str.IndexOf(prefix);
                offset = offset >= 0 ? offset + prefix.Length : 0;
            }
            if (!string.IsNullOrEmpty(suffix)) {
                length = str.IndexOf(suffix);
                length = length >= 0 ? length : str.Length;
            }
            return str.Substring(offset, length - offset);
        }

        #endregion
    }
}
