using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace VisualStudioCleanup
{
    class ComponentStore
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
    }
}
