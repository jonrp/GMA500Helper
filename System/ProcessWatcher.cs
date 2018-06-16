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
        //private ManagementEventWatcher watcher;

        private void Bind(object state) {
            bindTimer.Change(Timeout.Infinite, Timeout.Infinite);
            boundProcess = Process.GetProcessesByName(name).FirstOrDefault();
            if (boundProcess == null) {
                boundProcess.EnableRaisingEvents = true;
                bindTimer.Change(1000, Timeout.Infinite);
            }
            else {
                running = true;
                statusUpdate?.Invoke(name, running);
                boundProcess.Exited += OnProcessExited;
            }
        }

        private void OnProcessExited(object sender, EventArgs e) {
            running = false;
            //boundProcess.Exited -= OnProcessExited;
            boundProcess = null;
            statusUpdate?.Invoke(name, running);
            bindTimer.Change(1000, Timeout.Infinite);
        }

        public ProcessWatcher(string name/*, double pollTimeS = 1*/) {
            this.name = name;
            bindTimer = new Timer(Bind);
            Bind(null);
            //this.running = IsRunning(name);

            //var scope = new ManagementScope(@"root\CIMV2");
            //var query = new EventQuery(string.Format("select * from __InstanceOperationEvent within {0} where TargetInstance isa 'Win32_Process' and TargetInstance.Name = '{1}'", pollTimeS, name));
            //watcher = new ManagementEventWatcher(scope, query);
            //watcher.EventArrived += OnEvent;
            //watcher.Start();
        }

        //public static bool IsRunning(string name) {
        //    var scope = new ManagementScope(@"root\CIMV2");
        //    var query = new SelectQuery(string.Format("select * from Win32_Process where Name = '{0}'", name));
        //    using (var searcher = new ManagementObjectSearcher(scope, query)) {
        //        using (var objects = searcher.Get()) {
        //            return objects.Cast<ManagementObject>().Any();
        //        }
        //    }
        //}

        //private void OnEvent(object sender, EventArrivedEventArgs e) {
        //    switch (e.NewEvent.ClassPath.ClassName) {
        //        case "__InstanceCreationEvent":
        //            running = true;
        //            break;
        //        case "__InstanceDeletionEvent":
        //            running = false;
        //            break;
        //        default:
        //            return;
        //    }
        //    statusUpdate?.Invoke(name, running);
        //}

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

        public void Dispose() {
            bindTimer.Change(Timeout.Infinite, Timeout.Infinite);
            if (boundProcess != null) {
                //boundProcess.Exited -= OnProcessExited;
                boundProcess = null;
            }
            //watcher.Stop();
            //watcher.Dispose();
            //watcher = null;
        }
    }
}
