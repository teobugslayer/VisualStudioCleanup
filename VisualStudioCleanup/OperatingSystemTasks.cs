using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
                    dism?.WaitForExit();
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

        public static IObservable<Unit> MovePackageCache(string destinationRoot)
        {
            return Observable.Start(() =>
            {
                var dest = Path.Combine(destinationRoot, "Package Cache");
                MoveDirectory(PackageCachePath, dest);
                Directory.Delete(PackageCachePath);
                CreateJunction(PackageCachePath, dest);
            },
            RxApp.TaskpoolScheduler);
        }

        public static void Uninstall(string program)
        {
            ExecProg(program);
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
                if (uninstallKey == null) return;
                var subKeys = uninstallKey.GetSubKeyNames();

                foreach(var subkeyName in subKeys)
                {
                    using (var subkey = uninstallKey.OpenSubKey(subkeyName))
                    {
                        if (subkey == null) continue;
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

        private static void CreateJunction(string sourceDir, string destDir)
        {
            ExecProg($"mklink /j \"{sourceDir}\" \"{destDir}\"");
        }

        private static void ExecProg(string program)
        {
            var psi = new ProcessStartInfo("cmd.exe", $"/c {program}")
            {
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using (var proc = Process.Start(psi))
            {
                proc?.WaitForExit();
            }
        }

        private static void MoveDirectory(string sourceDir, string destDir)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDir);

            // create target dir (we may have just recursed into it
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // Move files
            foreach (var file in dir.GetFiles())
            {
                file.MoveTo(Path.Combine(destDir, file.Name));
            }

            // Move sub-dirs
            foreach (var subdir in dir.GetDirectories())
            {
                var temp = Path.Combine(destDir, subdir.Name);
                MoveDirectory(subdir.FullName, temp);
                Directory.Delete(subdir.FullName);
            }
        }

        private static readonly string PackageCachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
            "Package Cache");
    }
}
