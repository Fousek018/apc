using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABPOWER_APC.Controller;
using LABPOWER_APC.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LABPOWER_APC.ViewModel
{
    public interface IMainVM
    {

    }
    public partial class UPS : ObservableObject, IMainVM
    {
        // Přidejte kolekci pro hodnoty výčtu
        public ObservableCollection<String> GracefulDelayValues { get; }
        public ObservableCollection<String> AlarmValues { get; }

        private string _selectedGracefulDelay;
        public string SelectedGracefulDelay
        {
            get => _selectedGracefulDelay;
            set
            {
                SetProperty(ref _selectedGracefulDelay, value);
                ChangeShutdownDelay(value); // Spustí funkci při změně hodnoty
            }
        }

        private string _selectedAlarmDelay;
        public string SelectedAlarmDelay
        {
            get => _selectedAlarmDelay;
            set
            {
                SetProperty(ref _selectedAlarmDelay, value);
                ChangeAlarmDelay(value); // Spustí funkci při změně hodnoty
            }
        }

        //OBS properties
        [ObservableProperty] string _Port = "UKNOWN";
        [ObservableProperty] string _B_status = "UKNOWN";
        [ObservableProperty] string _Model = "UKNOWN";
        [ObservableProperty] string _OutputVoltage = "UKNOWN";
        [ObservableProperty] string _PowerType = "UKNOWN";
        [ObservableProperty] string _Tries = "UKNOWN";
        [ObservableProperty] string _ShutdownDelay = "UKNOWN";
        [ObservableProperty] string _AlarmDelay = "UKNOWN";
        [ObservableProperty] int _ShutdownTimeLeft;

        public UPSStatus Status;
        public UPSPortManager PortManager;
        public UPSSettings Settings;
        private Timer computerShutdownTimer;




        public UPS()
        {


            Settings = UPSSettings.Deserialize();

            PortManager = new UPSPortManager(Settings);
            Status = new UPSStatus(PortManager);
            Status.PropertyChanged += Status_PropertyChanged;
            PortManager.WriteAndWaitForResponse("Y", 100);

            ShutdownTimeLeft = Settings.ShutdownTimeLeft;

            GracefulDelayValues = new ObservableCollection<string>();
            EnumHelper.FillEnumDescriptions<UPSStatus.GracefulDelay>(GracefulDelayValues);

            AlarmValues = new ObservableCollection<string>();
            EnumHelper.FillEnumDescriptions<UPSStatus.AlarmDelayEnum>(AlarmValues);

            //Shutdowning upc after hibernation or sleep mode
            //SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;

        }


        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionEndReasons.SystemShutdown:
                    ShutdownGracefully();
                    break;
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    ShutdownGracefully();
                    break;
            }
        }

        void Status_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OutputVoltage = Status.OutputVoltage;
            B_status = Status.BatteryLevel;
            Model = Status.Model;
            PowerType = Status.PowerType.ToString();
            Port = Settings.PortName;
            ShutdownDelay = Status.ShutdownDelay;
            AlarmDelay = Status.AlarmDelay;

            if (e.PropertyName.Equals("PowerType"))
            {
                if (Status.PowerType == UPSStatus.PowerTypeEnum.Battery)
                {
                    computerShutdownTimer = new Timer();
                    computerShutdownTimer.Interval = 1000;
                    computerShutdownTimer.Elapsed += computerShutdownTimer_Elapsed;
                    computerShutdownTimer.Start();
                }
                else
                {
                    if (computerShutdownTimer != null)
                    {
                        computerShutdownTimer.Stop();
                        ShutdownTimeLeft  = (Settings.ShutdownTimeLeft = 20);
                    }
                }
            }
        }

        void computerShutdownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ShutdownTimeLeft = (Settings.ShutdownTimeLeft -= 1);
            if (Settings.ShutdownTimeLeft <= 0)
            {
                computerShutdownTimer.Stop();

                ShutdownGracefully();
                //Process.Start("shutdown", "/s /t 0");
            }
        }

        /// <summary>
        /// Gracefully shuts down the UPS. This is how the UPS will shut down when the computer is shutting down.
        /// </summary>
        public void ShutdownGracefully()
        {
            ShutdownTimeLeft = (Settings.ShutdownTimeLeft = 20);

            PortManager.WriteSerial("Y");
            Thread.Sleep(150);
            PortManager.WriteSerial("U");
            Thread.Sleep(150);
            string listen = PortManager.WriteAndWaitForResponse("S", 100);
            int numTries = 0;
            int maxTries = 15;
            while (listen != "OK" && numTries < maxTries)
            {
                Thread.Sleep(50);
                listen = PortManager.WriteAndWaitForResponse("S", 100);
                numTries++;
                Tries = listen;
            }
                        ShutdownTimeLeft  = (Settings.ShutdownTimeLeft = 20);

            PortManager.WriteSerial("K");
            Thread.Sleep(750);
            PortManager.WriteSerial("K");
            //Tries = 1;
        }

        /// <summary>
        /// turns on the UPS if already off. Rarely, if ever, used, since the computer will be plugged into the UPS
        /// </summary>
        public void TurnOnUPS()
        {
            PortManager.WriteSerial(((char)14).ToString());
            Thread.Sleep(1500);
            PortManager.WriteSerial(((char)14).ToString());
        }


        public void ChangeShutdownDelay(string newDelayDescription)
        {
            var newDelay = Enum.GetValues(typeof(UPSStatus.GracefulDelay))
                              .Cast<UPSStatus.GracefulDelay>()
                              .FirstOrDefault(e => UPSStatus.GetEnumDescription(e) == newDelayDescription);

            Status.PauseTimer();
            Thread.Sleep(250);
            string currentDelay = PortManager.WriteAndWaitForResponse("p", 100);
            currentDelay = currentDelay.Replace(".", "");
            while (Convert.ToInt32(currentDelay) != (int)newDelay)
            {
                PortManager.WriteAndWaitForResponse("-", 50);
                currentDelay = PortManager.WriteAndWaitForResponse("p", 100);
                currentDelay = currentDelay.Replace(".", "");
            }
            Status.ShutdownDelay = UPSStatus.GetEnumDescription((UPSStatus.GracefulDelay)Enum.Parse(typeof(UPSStatus.GracefulDelay), currentDelay));
            Status.StartTimer();
        }

        public void ChangeAlarmDelay(string newDelayDescription)
        {
            var newDelay = Enum.GetValues(typeof(UPSStatus.AlarmDelayEnum))
                                .Cast<UPSStatus.AlarmDelayEnum>()
                                .FirstOrDefault(e => UPSStatus.GetEnumDescription(e) == newDelayDescription);

            Status.PauseTimer();
            Thread.Sleep(250);
            string currentDelay = PortManager.WriteAndWaitForResponse("k", 100);
            currentDelay = currentDelay.Replace(".", "");
            UPSStatus.AlarmDelayEnum delayEnum = SetAlarmDelayEnum(currentDelay);

            while (delayEnum != newDelay)
            {
                PortManager.WriteAndWaitForResponse("-", 50);
                currentDelay = PortManager.WriteAndWaitForResponse("k", 100);
                currentDelay = currentDelay.Replace(".", "");
                delayEnum = SetAlarmDelayEnum(currentDelay);
            }
            Status.AlarmDelay = UPSStatus.GetEnumDescription(delayEnum);
            Status.StartTimer();
        }

        private static UPSStatus.AlarmDelayEnum SetAlarmDelayEnum(string currentDelay)
        {
            UPSStatus.AlarmDelayEnum delayEnum = UPSStatus.AlarmDelayEnum.Disabled;
            switch (currentDelay)
            {
                case "0": delayEnum = UPSStatus.AlarmDelayEnum.FiveSeconds; break;
                case "T": delayEnum = UPSStatus.AlarmDelayEnum.ThirtySeconds; break;
                case "L": delayEnum = UPSStatus.AlarmDelayEnum.BatteryOnly; break;
                case "N": delayEnum = UPSStatus.AlarmDelayEnum.Disabled; break;
            }
            return delayEnum;
        }
        [RelayCommand]
        public void SerializeSettings()
        {
            UPSSettings.Serialize(Settings);
        }
    }
    public static class EnumHelper
    {
        public static void FillEnumDescriptions<T>(ObservableCollection<string> collection) where T : Enum
        {
            collection.Clear();
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                FieldInfo field = value.GetType().GetField(value.ToString());
                DescriptionAttribute? attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                                      .FirstOrDefault() as DescriptionAttribute;

                collection.Add(attribute == null ? value.ToString() : attribute.Description);
            }
        }
    }

}
