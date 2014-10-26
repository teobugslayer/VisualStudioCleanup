using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace VisualStudioCleanup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new UiController();

            // Yes, I wasted 4 hours on this (half Saturday). ListBox.SelectedItems is a) CLR property and not a DependencyProperty and b) read-only. Fuck MVVM/MVC/separation of whatever.
            // Someone somewhere said WPF was productive. I vehemently disagree. You can find an attached propery work-around on SO which is 10 times the size after accounting for all the corner cases
            // Yes. I am wasting time complaining about this. Yes, I hate this code passionately
            this.UninstallablesList.SelectionChanged += (object sender, SelectionChangedEventArgs e) => 
            {
                var controller = this.DataContext as UiController;
                if (controller != null)
                {
                    using (controller.SelectedUninstallables.SuppressChangeNotifications())
                    {
                        foreach(var item in e.RemovedItems.Cast<Uninstallable>())
                        {
                            controller.SelectedUninstallables.Remove(item);
                        }
                        controller.SelectedUninstallables.AddRange(e.AddedItems.Cast<Uninstallable>());
                    }
                }
            };
        }
    }
}
