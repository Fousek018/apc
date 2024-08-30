using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABPOWER_APC.Controller;
using LABPOWER_APC.Model;
using LABPOWER_APC.Utilities;
using LABPOWER_APC.View;
using LABPOWER_APC.VM;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Threading;
using static LABPOWER_APC.Model.UPSStatus;
using Timer = System.Timers.Timer;


namespace LABPOWER_APC.ViewModel
{
    public interface IMainVM
    {
    }
    public partial class UPS : ObservableObject, IMainVM
    {
        // Přidejte kolekci pro hodnoty výčtu

        private string computerName;

        [ObservableProperty]
        private string _MainPc;

        int _ShutdownTimeLeft;

        [ObservableProperty]
        private string _CS = "ZK";

        [ObservableProperty]
        private PowerTypeEnum _PowerType;

        [ObservableProperty]
        private string _BatteryVoltage;

        [ObservableProperty]
        private string _InputVoltage;

        [ObservableProperty]
        private string _OutputVoltage;

        [ObservableProperty]
        private string _BatteryLevel;

        [ObservableProperty]
        private string _Model;

        //Settings properties
        [ObservableProperty]
        private string _ShutdownDelay;

        [ObservableProperty]
        private string _AlarmDelay;

        [ObservableProperty]
        private string _Port;

        [ObservableProperty]
        public int _BaudRate;

        [ObservableProperty]
        public Parity _Parity;

        [ObservableProperty]
        public int _DataBits;

        [ObservableProperty]
        public StopBits _StopBits;



        [ObservableProperty]
        private EnumHelper<UPSStatus.GracefulDelay> _SelectedGracefulDelay;

        [ObservableProperty]
        private EnumHelper<UPSStatus.AlarmDelayEnum> _SelectedAlarmDelay;

        [ObservableProperty]
        private EnumHelper<UPSSettings.ShutdownEnum> _SelectedShutdownTimer;

        [ObservableProperty]
        private ObservableCollection<NetworkDevice>? devices;

        [ObservableProperty]
        private ObservableCollection<ChosenNetworkDevice>? chosenNetworkDevices;

        [ObservableProperty]
        private ObservableCollection<NetworkDevice>? selectedDevices;

        public List<EnumHelper<UPSStatus.GracefulDelay>> GracefulDelayOptions { get; }
        public List<EnumHelper<UPSStatus.AlarmDelayEnum>> AlarmlDelayOptions { get; }
        public List<EnumHelper<UPSSettings.ShutdownEnum>> ShutdownTimerOptions { get; }


        private string SaveDirectory;
        private const string conDevFile = "connectedDevices.xml";
        private const string userSettingFile = "userSetting.xml";

        public UPSStatus Status;
        public UPSPortManager PortManager;
        public UPSSettings Settings;

        private Timer computerShutdownTimer;
        private readonly DispatcherTimer _timer;
        private CancellationTokenSource _cancellationTokenSource;


        public SeriesCollection OutputVoltageSeries { get; set; }
        public ObservableCollection<string> TimeLabels { get; set; }
        public Func<double, string> VoltageFormatter { get; set; }

        public UPS()
        {
            SaveDirectory = System.IO.Path.Combine(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Data"));
            SelectedDevices = new ObservableCollection<NetworkDevice>();

            //Inicialization of the settings
            Settings = XmlHelper.Deserialize<UPSSettings>(userSettingFile) ?? new UPSSettings();
            CS = Settings.ShutdownTimeLeft.ToString() + " second";

            //Inicialization of the port manager
            PortManager = new UPSPortManager(Settings);
            
            //Inicialization of the status
            Status = new UPSStatus(PortManager);
            //Subscribing to the property changes of the status class
            Status.PropertyChanged += Status_PropertyChanged;


            //Settings Propertyupdate
            Settings_Propertyupdate();

            //Entering to the smart Mode, need to do that before anything else
            PortManager.WriteAndWaitForResponse("Y", 100);

            GracefulDelayOptions = Enum.GetValues(typeof(UPSStatus.GracefulDelay))
                                   .Cast<UPSStatus.GracefulDelay>()
                                   .Select(e => new EnumHelper<UPSStatus.GracefulDelay>(e, UPSStatus.GetEnumDescription(e)))
                                   .ToList();

            AlarmlDelayOptions = Enum.GetValues(typeof(UPSStatus.AlarmDelayEnum))
                                  .Cast<UPSStatus.AlarmDelayEnum>()
                                  .Select(e => new EnumHelper<UPSStatus.AlarmDelayEnum>(e, UPSStatus.GetEnumDescription(e)))
                                  .ToList();

            ShutdownTimerOptions = Enum.GetValues(typeof(UPSSettings.ShutdownEnum))
                                 .Cast<UPSSettings.ShutdownEnum>()
                                 .Select(e => new EnumHelper<UPSSettings.ShutdownEnum>(e, UPSSettings.GetEnumDescription(e)))
                                 .ToList();

            

            // Initialize chart data
            OutputVoltageSeries = new SeriesCollection
                {
            new LineSeries
            {
                Title = "Output Voltage",
                Values = new ChartValues<double>()
            }
                };

            TimeLabels = new ObservableCollection<string>();
            VoltageFormatter = value => value.ToString("N");

            // Timer to update data every 10 seconds
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();


            // Load saved devices
            Devices = new ObservableCollection<NetworkDevice>();
            ChosenNetworkDevices = new ObservableCollection<ChosenNetworkDevice>(XmlHelper.Deserialize<List<ChosenNetworkDevice>>(conDevFile));

            // Get the name of the main computer
            computerName = Dns.GetHostName();
            MainPc = computerName + " " + (Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString());

            //Shutdowning upc after hibernation or sleep mode
            //SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            CS = UPSStatus.GetEnumDescription((UPSSettings.ShutdownEnum)Settings.ShutdownTimeLeft);

        }

        private void Settings_Propertyupdate()
        {

            BaudRate = Settings.BaudRate;
            Parity = Settings.Parity;
            DataBits = Settings.DataBits;
            StopBits = Settings.StopBits;

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateChart();
        }

        private void UPS_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InputVoltage))
            {
                UpdateChart();
            }
        }
        private void UpdateChart()
        {
            string cleanedInput = InputVoltage.Trim();
            // Simulate data update
            if (double.TryParse(cleanedInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double voltage))
            {
                OutputVoltageSeries[0].Values.Add(voltage);
                TimeLabels.Add(DateTime.Now.ToString("HH:mm:ss"));

                // Keep only the last 10 values
                if (OutputVoltageSeries[0].Values.Count > 10)
                {
                    OutputVoltageSeries[0].Values.RemoveAt(0);
                    TimeLabels.RemoveAt(0);
                }
            }
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

            Port = Settings.PortName;

            switch (e.PropertyName)
            {
                case nameof(UPSStatus.PowerType):
                    PowerType = Status.PowerType;
                    break;
                case nameof(UPSStatus.BatteryVoltage):
                    BatteryVoltage = Status.BatteryVoltage;
                    break;
                case nameof(UPSStatus.InputVoltage):
                    InputVoltage = Status.InputVoltage;
                    break;
                case nameof(UPSStatus.OutputVoltage):
                    OutputVoltage = Status.OutputVoltage;
                    break;
                case nameof(UPSStatus.BatteryLevel):
                    BatteryLevel = Status.BatteryLevel;
                    break;
                case nameof(UPSStatus.Model):
                    Model = Status.Model;
                    break;
                case nameof(UPSStatus.ShutdownDelay):
                    ShutdownDelay = Status.ShutdownDelay;
                    break;
                case nameof(UPSStatus.AlarmDelay):
                    AlarmDelay = Status.AlarmDelay;
                    break;
            }

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
                        _ShutdownTimeLeft = Settings.ShutdownTimeLeft;
                    }
                }
            }
        }

        void computerShutdownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ShutdownTimeLeft -= 1;
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
            _ShutdownTimeLeft = (Settings.ShutdownTimeLeft = 20);

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
            }
            _ShutdownTimeLeft = Settings.ShutdownTimeLeft;

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


        public void ChangeShutdownDelay(int newDelay)
        {


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

        public void ChangeAlarmDelay(UPSStatus.AlarmDelayEnum newDelay)
        {
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
        private async Task<PingReply> PingDeviceAsync(string ipAddress)
        {
            using var ping = new Ping();
            return await ping.SendPingAsync(ipAddress, 1000);
        }

        private async Task<string> GetHostNameAsync(string ipAddress)
        {
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(ipAddress);
                return hostEntry.HostName;
            }
            catch
            {
                return "Unknown";
            }
        }

        // Funkce pro získání IP adresy
        [RelayCommand]
        public async Task GetIp()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Devices.Clear();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "arp",
                    Arguments = "-a",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var regex = new Regex(@"(\d+\.\d+\.\d+\.\d+)\s+([\w-]+)\s+([\w-]+)");
            var matches = regex.Matches(output);

            foreach (Match match in matches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                var ipAddress = match.Groups[1].Value;
                var pingReply = await PingDeviceAsync(ipAddress);

                if (pingReply.Status == IPStatus.Success)
                {
                    var hostName = await GetHostNameAsync(ipAddress);
                    Devices.Add(new NetworkDevice
                    {
                        IPAddress = ipAddress,
                        MACAddress = match.Groups[2].Value,
                        HostName = hostName
                    });
                }
            }
        }
        // Funkce pro zastavení získávání IP adres
        [RelayCommand]
        public void StopGetIp()
        {
            _cancellationTokenSource?.Cancel();
        }
        // Funkce pro přidání vybraných IP adres do listu.
        [RelayCommand]
        public void AddSelectedDevices()
        {


            foreach (var device in SelectedDevices)
            {
                if (!ChosenNetworkDevices.Any(d => d.IPAddress == device.IPAddress))
                {
                    var chosenDevice = new ChosenNetworkDevice
                    {
                        IPAddress = device.IPAddress,
                        HostName = device.HostName,

                    };
                    ChosenNetworkDevices.Add(chosenDevice);

                }
            }

            XmlHelper.Serialize(ChosenNetworkDevices.ToList(), conDevFile);
        }


        [RelayCommand]
        public void RemoveChosenDevice(ChosenNetworkDevice device)
        {
            ChosenNetworkDevices.Remove(device);
            XmlHelper.Serialize(ChosenNetworkDevices.ToList(), conDevFile);
        }
        [RelayCommand]
        private void SaveGracefulDelay()
        {
            if (SelectedGracefulDelay != null && SelectedGracefulDelay.Description != Status.ShutdownDelay)
            {
                ChangeShutdownDelay((int)SelectedGracefulDelay.Value);
            }

            if (SelectedAlarmDelay != null && SelectedAlarmDelay.Description != Status.AlarmDelay)
            {
                ChangeAlarmDelay(SelectedAlarmDelay.Value);
            }
            if (SelectedShutdownTimer != null && (int)SelectedShutdownTimer.Value != Settings.ShutdownTimeLeft)
            {
                Settings.ShutdownTimeLeft = (int)SelectedShutdownTimer.Value;
                XmlHelper.Serialize(Settings, userSettingFile);
                CS = UPSStatus.GetEnumDescription((UPSSettings.ShutdownEnum)Settings.ShutdownTimeLeft);

            }
           
        }
        [RelayCommand]
        public void PcSettings(ChosenNetworkDevice device)
        {
            if (device != null)
            {
                // Například: otevřete nové okno s informacemi o zařízení
                var viewModel = new remotePCVM(device);
                var window = new remotePCView { DataContext = viewModel };
                window.Show();
            }
        }
    }


}
