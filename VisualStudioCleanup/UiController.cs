using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace VisualStudioCleanup
{
    class UiController: ReactiveObject
    {
        public UiController()
        {
            this.Uninstallables = new ReactiveList<Uninstallable>(OperatingSystemTasks.GetUninstallables().ToEnumerable().OrderBy(x => x.Name));
            this.SelectedUninstallables = new ReactiveList<Uninstallable>();

            this.TurnOffHyperVCommand = ReactiveCommand.CreateAsyncTask(x => OperatingSystemTasks.TurnOffHyperV());
            this.CleanSetupLogsCommand = ReactiveCommand.CreateAsyncTask(x => OperatingSystemTasks.CleanSetupLogs());
            this.UninstallCommand = ReactiveCommand.CreateAsyncTask(this.SelectedUninstallables.CountChanged.Select(count => count != 0), x => DoUninstall());
            this.AboutCommand = ReactiveCommand.CreateAsyncObservable(x => Observable.Return(true));
            this.showAbout = this.AboutCommand.ToProperty(this, x => x.ShowAbout, false);

            this.isBusy = this.WhenAnyObservable(
                x => x.TurnOffHyperVCommand.IsExecuting, 
                x => x.CleanSetupLogsCommand.IsExecuting, 
                x => x.UninstallCommand.IsExecuting)
                .ToProperty(this, x => x.IsBusy);
        }

        public ReactiveCommand<bool> AboutCommand { get; private set; }
        public ReactiveCommand<Unit> TurnOffHyperVCommand { get; private set; }
        public ReactiveCommand<Unit> CleanSetupLogsCommand { get; private set; }
        public ReactiveCommand<Unit> UninstallCommand { get; private set; }

        public bool ShowAbout { get { return this.showAbout.Value; } }
        public bool IsBusy { get { return this.isBusy.Value; } }
        public ReactiveList<Uninstallable> Uninstallables { get; private set; }
        public ReactiveList<Uninstallable> SelectedUninstallables { get; private set; }

        private Task DoUninstall()
        {
            var result = new Task(() =>
            {
                foreach (var item in this.SelectedUninstallables)
                {
                    OperatingSystemTasks.Uninstall(item.Command);
                }
            },
            TaskCreationOptions.LongRunning);
            result.Start();
            return result;
        }

        private ObservableAsPropertyHelper<bool> showAbout;
        private ObservableAsPropertyHelper<bool> isBusy;
    }
}
