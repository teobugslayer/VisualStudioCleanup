using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VisualStudioCleanup
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
        }

        private void GitHubClick(object sender, RoutedEventArgs e)
        {
            using (Process.Start("https://github.com/teobugslayer/VisualStudioCleanup")) { }
        }
    }
}
