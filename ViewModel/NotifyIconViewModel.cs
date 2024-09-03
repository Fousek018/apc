using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace LABPOWER_APC.ViewModel
{
    public partial class NotifyIconViewModel : ObservableObject
    {
        private UPS? _UPS;
        private TaskbarIcon? _taskbarIcon;
        private Window? _mainWindow;
        public NotifyIconViewModel()
        {

            _UPS = Ioc.Default.GetService<UPS>();
            _UPS.PropertyChanged += HomeVM_PropertyChanged;


        }
        private void HomeVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UPS.PowerType2))
            {
                ShowNotification("Power supply change ", "shutdown sequence triggered");
            }
        }
        private void ShowNotification(string title, string message)
        {
            _taskbarIcon = new TaskbarIcon
            {
                IconSource = new BitmapImage(new Uri("pack://application:,,,/Zkouska_Get_Ip;component/Img/energy.ico")),
                ToolTipText = "Zkouška Get IP",
                Visibility = Visibility.Visible
            };
            _taskbarIcon?.ShowBalloonTip(title, message, BalloonIcon.Info);
        }

        [RelayCommand]
        private void Open()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_mainWindow == null)
                {
                    _mainWindow = new View.UPSVIEW
                    {
                        DataContext = _UPS
                    };
                }

                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
            });
        }

        [RelayCommand]
        private static void Exit()
        {
            // Implementace logiky pro ukončení aplikace
            Application.Current.Shutdown();
        }
    }
}

