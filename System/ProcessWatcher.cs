using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GMA500Helper.System {
    public class ProcessWatcher : IDisposable {
        private readonly string name;
        private bool running;

        private Timer bindTimer;
        private Process boundProcess;

        public ProcessWatcher(string name) {
            this.name = name;
            bindTimer = new Timer(Bind);
            Bind(null);
        }

        private event Action<string, bool> statusUpdate;
        public event Action<string, bool> StatusUpdate {
            add {
                value(name, running);
                statusUpdate += value;
            }
            remove {
                statusUpdate -= value;
            }
        }

        private void Bind(object state) {
            bindTimer.Change(Timeout.Infinite, Timeout.Infinite);
            boundProcess = Process.GetProcessesByName(name).FirstOrDefault();
            if (boundProcess == null) {
                boundProcess.EnableRaisingEvents = true;
                bindTimer.Change(1000, Timeout.Infinite);
            } else {
                running = true;
                statusUpdate?.Invoke(name, running);
                boundProcess.Exited += OnProcessExited;
            }
        }

        private void OnProcessExited(object sender, EventArgs e) {
            running = false;
            boundProcess.Exited -= OnProcessExited;
            boundProcess = null;
            statusUpdate?.Invoke(name, running);
            bindTimer.Change(1000, Timeout.Infinite);
        }

        public void Dispose() {
            bindTimer.Change(Timeout.Infinite, Timeout.Infinite);
            bindTimer.Dispose();
            if (boundProcess != null) {
                boundProcess.Exited -= OnProcessExited;
                boundProcess = null;
            }
        }
    }
}
