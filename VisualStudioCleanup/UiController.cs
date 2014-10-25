using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace VisualStudioCleanup
{
    class UiController: ReactiveObject
    {
        public UiController()
        {
            this.TurnOffHyperVCommand = ReactiveCommand.CreateAsyncTask(x => new ComponentStore().TurnOffHyperV());
            this.TurnOffHyperVCommand.ToProperty(this, x => x.IsBusy, false);
        }
        
        public ReactiveCommand<bool> TurnOffHyperVCommand { get; private set; }

        public bool IsBusy { get; private set; }

    }
}
