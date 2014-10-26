using System;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;

namespace VisualStudioCleanup
{
    /// <summary>
    /// Interaction logic for MovePackageCacheControl.xaml
    /// </summary>
    public partial class MovePackageCache : UserControl
    {
        public MovePackageCache()
        {
            InitializeComponent();
        }

        private void ChooseDestinationDir(object sender, System.Windows.RoutedEventArgs e)
        {
            var dirChooser = new SaveFileDialog() {
                FileName = "File name will be ignored",
                InitialDirectory = this.DestinationDir.Text
            };
            if (dirChooser.ShowDialog() == true)
            {
                this.DestinationDir.Text = Path.GetDirectoryName(dirChooser.FileName);
                this.DestinationDir.GetBindingExpression(TextBox.TextProperty).UpdateSource(); // Another hour and half lost. At least the work-around was in MSDN
            }
        }
    }
}
