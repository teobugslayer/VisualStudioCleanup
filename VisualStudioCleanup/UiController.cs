using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace VisualStudioCleanup
{
    class UiController: ReactiveObject
    {
        public UiController()
        {
            this.Uninstallables = new ReactiveList<Uninstallable>(this.Refresh());
            this.SelectedUninstallables = new ReactiveList<Uninstallable>();

            this.TurnOffHyperVCommand = ReactiveCommand.CreateAsyncObservable(x => OperatingSystemTasks.TurnOffHyperV());
            this.CleanSetupLogsCommand = ReactiveCommand.CreateAsyncObservable(x => OperatingSystemTasks.CleanSetupLogs());
            this.UninstallCommand = ReactiveCommand.CreateAsyncObservable(this.SelectedUninstallables.CountChanged.Select(count => count != 0), x => DoUninstall());
            this.UninstallCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => {
                this.Uninstallables.Clear();
                this.Uninstallables.AddRange(this.Refresh());
            });
            this.AboutCommand = ReactiveCommand.CreateAsyncObservable(x => Observable.Return(!this.ShowAbout));
            this.showAbout = this.AboutCommand.ToProperty(this, x => x.ShowAbout, false);
            this.ShowPackageCacheCommand = ReactiveCommand.CreateAsyncObservable(x => Observable.Return(!this.ShowPackageCache));
            this.showPackageCache = this.ShowPackageCacheCommand.ToProperty(this, x => x.ShowPackageCache, false);
            this.MovePackageCacheCommand = ReactiveCommand.CreateAsyncObservable(x => OperatingSystemTasks.MovePackageCache(this.PackageCacheDestination));

            this.isBusy = this.WhenAnyObservable(
                x => x.TurnOffHyperVCommand.IsExecuting, 
                x => x.CleanSetupLogsCommand.IsExecuting, 
                x => x.UninstallCommand.IsExecuting,
                x => x.MovePackageCacheCommand.IsExecuting)
                .ToProperty(this, x => x.IsBusy);
        }

        public ReactiveCommand<bool> AboutCommand { get; }
        public ReactiveCommand<Unit> TurnOffHyperVCommand { get; }
        public ReactiveCommand<Unit> CleanSetupLogsCommand { get; }
        public ReactiveCommand<Unit> UninstallCommand { get; }
        public ReactiveCommand<Unit> MovePackageCacheCommand { get; }
        public ReactiveCommand<bool> ShowPackageCacheCommand { get; }

        public bool ShowAbout => this.showAbout.Value;
        public bool IsBusy => this.isBusy.Value;
        public bool ShowPackageCache => this.showPackageCache.Value;
        public ReactiveList<Uninstallable> Uninstallables { get; }
        public ReactiveList<Uninstallable> SelectedUninstallables { get; }
        public string PackageCacheDestination
        {
            get => this.packageCacheDestination;
            set => this.RaiseAndSetIfChanged(ref this.packageCacheDestination, value);
        }

        private IObservable<Unit> DoUninstall()
        {
            return Observable.Start(() =>
            {
                foreach (var item in this.SelectedUninstallables.ToArray())
                {
                    OperatingSystemTasks.Uninstall(item.Command);
                }
            },
            RxApp.TaskpoolScheduler);
        }

        private IOrderedEnumerable<Uninstallable> Refresh()
        {
            return OperatingSystemTasks.GetUninstallables().ToEnumerable().OrderBy(x => x.Name);
        }

        private readonly ObservableAsPropertyHelper<bool> showAbout;
        private readonly ObservableAsPropertyHelper<bool> isBusy;
        private readonly ObservableAsPropertyHelper<bool> showPackageCache;
        private string packageCacheDestination;
    }
}
