using CommunityToolkit.Mvvm.DependencyInjection;
using LABPOWER_APC.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace LABPOWER_APC.View
{
    /// <summary>
    /// Interakční logika pro NotifyIcon.xaml
    /// </summary>
    public partial class NotifyIcon 
    {
        public NotifyIcon()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<NotifyIconViewModel>();

        }
    }
}
