using CommunityToolkit.Mvvm.DependencyInjection;
using LABPOWER_APC.Model;
using LABPOWER_APC.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LABPOWER_APC.View
{
    /// <summary>
    /// Interakční logika pro UPSVIEW.xaml
    /// </summary>
    public partial class UPSVIEW : Window
    {
        public UPSVIEW()
        {
            InitializeComponent();
           

        }

        private void ShutdownTimerCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Border searchButtonBorder = (Border)FindName("SearchButtonBorder");
            Grid searchgrid = (Grid)FindName("listofsearch");

            searchgrid.Visibility = Visibility.Visible;

            // Start the animation
            Storyboard fadeInOutAnimation = (Storyboard)FindResource("FadeInOutAnimation");
            fadeInOutAnimation.Begin(searchButtonBorder);

            // Simulate a long-running operation
            await Task.Run(() =>
            {
                // Your long-running operation here
                System.Threading.Thread.Sleep(5000); // Simulate a 5-second operation
            });

            // Stop the animation
            fadeInOutAnimation.Stop(searchButtonBorder);
        }
    
    }
}
