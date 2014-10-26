using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Linq;
using Microsoft.Win32;

namespace VisualStudioCleanup
{
    static class OperatingSystemTasks
    {
        public static IObservable<Unit> TurnOffHyperV()
        {
            return Observable.Start(() =>
            {
                using (var dism = Process.Start("dism.exe", "/Online /Disable-Feature:Microsoft-Hyper-V-All"))
                {
                    dism.WaitForExit();
                }
            },
            RxApp.TaskpoolScheduler);
        }

        public static IObservable<Unit> CleanSetupLogs()
        {
            return Observable.Start(() =>
            {
                var tempDir = Path.GetTempPath();
                Observable.Concat(
                    Directory.EnumerateFiles(tempDir, "dd_*.*").ToObservable(),
                    Directory.EnumerateFiles(tempDir, "VSIXInstaller_*.log").ToObservable(),
                    Directory.EnumerateFiles(tempDir, "MSI*.LOG").ToObservable(),
                    Directory.EnumerateFiles(tempDir, "sql*.*").ToObservable())
                .Subscribe(file => File.Delete(file));
            },
            RxApp.TaskpoolScheduler);
        }

        public static void Uninstall(string program)
        {
            var psi = new ProcessStartInfo("cmd.exe", "/c " + program)
            {
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using (var proc = Process.Start(psi))
            {
                proc.WaitForExit();
            }
        }

        public static IObservable<Uninstallable> GetUninstallables()
        {
           return Observable.Create<Uninstallable>(o => {
               Observable.Start(() => {
                   try
                   {
                       using (var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
                       {
                           GetUninstallablesCore(localMachine, o);
                       }

                       if (Environment.Is64BitProcess)
                       {
                           using (var localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                           {
                               GetUninstallablesCore(localMachine32, o);
                           }
                       }

                       o.OnCompleted();
                   }
                   catch (Exception ex)
                   {
                       o.OnError(ex);
                   }
               }, 
               RxApp.TaskpoolScheduler);

               return Disposable.Create(() => { });
            });
        }

        private static void GetUninstallablesCore(RegistryKey baseKey, IObserver<Uninstallable> outcome)
        {
            using (var uninstallKey = baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                var subKeys = uninstallKey.GetSubKeyNames();

                foreach(var subkeyName in subKeys)
                {
                    using (var subkey = uninstallKey.OpenSubKey(subkeyName))
                    {
                        var name = (string)subkey.GetValue("DisplayName");
                        var command = (string)subkey.GetValue("UninstallString");
                        var source = (string)subkey.GetValue("InstallSource", "");

                        if (!string.IsNullOrEmpty(source) && source.IndexOf(PackageCachePath, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            var uninstallable = new Uninstallable(name, command);
                            outcome.OnNext(uninstallable);
                        }
                    }
                }
            }
        }

        private static string PackageCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Package Cache");
    }
}
