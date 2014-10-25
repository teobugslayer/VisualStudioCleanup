using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Linq;

namespace VisualStudioCleanup
{
    class OperatingSystemTasks
    {
        public Task<bool> TurnOffHyperV()
        {
            var result = new Task<bool>(() =>
            {
                using (var dism = Process.Start("dism.exe", "/Online /Disable-Feature:Microsoft-Hyper-V-All"))
                {
                    dism.WaitForExit();
                }
                return true;
            },
            TaskCreationOptions.LongRunning);
            result.Start();
            return result;
        }

        public Task<bool> CleanSetupLogs()
        {
            var result = new Task<bool>(() =>
            {
                var tempDir = Path.GetTempPath();
                Directory.EnumerateFiles(tempDir, "dd_*.*").ToObservable().Subscribe(file => File.Delete(file));
                Directory.EnumerateFiles(tempDir, "VSIXInstaller_*.log").ToObservable().Subscribe(file => File.Delete(file));
                Directory.EnumerateFiles(tempDir, "MSI*.LOG").ToObservable().Subscribe(file => File.Delete(file));
                Directory.EnumerateFiles(tempDir, "sql*.*").ToObservable().Subscribe(file => File.Delete(file));
                
                return true;
            },
            TaskCreationOptions.LongRunning);
            result.Start();
            return result;
        }
    }
}
