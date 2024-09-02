using CommunityToolkit.Mvvm.DependencyInjection;
using LABPOWER_APC.ViewModel;
using LABPOWER_APC.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using LABPOWER_APC.View;
using LABPOWER_APC.VM;
using FluentValidation;
using LABPOWER_APC.Utilities;


namespace LABPOWER_APC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private NotifyIcon _notifyIcon;
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
                 .AddSingleton<UPSSettings>()
                 .AddSingleton<NotifyIconViewModel>()
                 .AddSingleton<logger>()
                 .BuildServiceProvider()

             );
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Inicializace NotifyIcon s ViewModel
            _notifyIcon = new NotifyIcon();

        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Uvolnění NotifyIcon při ukončení aplikace
            _notifyIcon.Dispose();
            base.OnExit(e);
        }
    }

}
