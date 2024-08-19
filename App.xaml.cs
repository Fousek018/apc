using CommunityToolkit.Mvvm.DependencyInjection;
using LABPOWER_APC.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;


namespace LABPOWER_APC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            ConfigureServices();
        }
        private void ConfigureServices()
        {
            Ioc.Default.ConfigureServices(
             new ServiceCollection()
                 .AddSingleton<IMainVM, UPS>()
                 .AddSingleton<UPS>()
                 .BuildServiceProvider()

             );
        }
    }

}
