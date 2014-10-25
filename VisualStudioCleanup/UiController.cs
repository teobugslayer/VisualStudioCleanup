using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualStudioCleanup
{
    class UiController: ReactiveObject
    {
        public UiController()
        {
            this.TurnOffHyperVCommand = ReactiveCommand.CreateAsyncTask(x => new OperatingSystemTasks().TurnOffHyperV());

            this.CleanSetupLogsCommand = ReactiveCommand.CreateAsyncTask(x => new OperatingSystemTasks().CleanSetupLogs());

            this.isBusy = this.WhenAnyObservable(x => x.TurnOffHyperVCommand.IsExecuting, x => x.CleanSetupLogsCommand.IsExecuting).ToProperty(this, x => x.IsBusy);

            this.AboutCommand = ReactiveCommand.CreateAsyncObservable(x => Observable.Start(() => true, RxApp.MainThreadScheduler));
            this.showAbout = this.AboutCommand.ToProperty(this, x => x.ShowAbout, false);
        }

        public ReactiveCommand<bool> TurnOffHyperVCommand { get; private set; }
        public ReactiveCommand<bool> CleanSetupLogsCommand { get; private set; }
        public ReactiveCommand<bool> AboutCommand { get; private set; }

        private ObservableAsPropertyHelper<bool> showAbout;
        public bool ShowAbout { get { return this.showAbout.Value; } }

        private ObservableAsPropertyHelper<bool> isBusy;
        public bool IsBusy { get { return this.isBusy.Value; } }
    }
}
