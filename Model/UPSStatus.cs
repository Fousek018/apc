using CommunityToolkit.Mvvm.ComponentModel;
using LABPOWER_APC.Controller;
using LABPOWER_APC.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;


namespace LABPOWER_APC.Model
{
    public partial class UPSStatus : ObservableObject

    {
        #region Properties

        public enum PowerTypeEnum
        {
            Line,
            Battery
        }

        public enum AlarmDelayEnum
        {
            [Description("Five Seconds")]
            FiveSeconds,
            [Description("Thirty Seconds")]
            ThirtySeconds,
            [Description("Battery Only")]
            BatteryOnly,
            [Description("Disabled")]
            Disabled
        }

        public enum GracefulDelay
        {
            
            [Description("1 Minute")]
            OneMinute = 60,
            [Description("3 Minutes")]
            ThreeMinute = 180,
            [Description("5 Minutes")]
            FiveMinute = 300,
            [Description("10 Minutes")]
            TenMinute = 600
        }

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

        [ObservableProperty]
        private string _ShutdownDelay;

        [ObservableProperty]
        private string _AlarmDelay;

        #endregion

        private UPSPortManager _manager;

        private Timer timer;

        public UPSStatus(UPSPortManager manager)
        {
            _manager = manager;
            timer = new Timer();
            timer.Interval = 5000;
            timer.Elapsed += t_Elapsed;
            timer.Start();
        }

        public void PauseTimer()
        {
            timer.Stop();
        }

        public void StartTimer()
        {
            timer.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_manager.PortActive)
            {
                _manager.WriteAndWaitForResponse("Y", 100); //only waiting to make sure it switched over
                BatteryVoltage = _manager.WriteAndWaitForResponse("B", 100);
                string pType = _manager.WriteAndWaitForResponse("Q", 100);
                switch (pType)
                {
                    case "08": case "0A": PowerType = PowerTypeEnum.Line; break;
                    case "10": PowerType = PowerTypeEnum.Battery; break;
                }
                InputVoltage = _manager.WriteAndWaitForResponse("L", 100);
                OutputVoltage = _manager.WriteAndWaitForResponse("O", 100);
                BatteryLevel = _manager.WriteAndWaitForResponse("f", 100);
                UPSStatus.GracefulDelay shutdownDelayEnum = (GracefulDelay)Enum.Parse(typeof(GracefulDelay), _manager.WriteAndWaitForResponse("p", 100));
                ShutdownDelay = GetEnumDescription(shutdownDelayEnum);
                AlarmDelay = GetEnumDescription(SetAlarmDelayEnum(_manager.WriteAndWaitForResponse("k", 100)));
                Model = _manager.WriteAndWaitForResponse(((char)1).ToString(), 125);
            }
        }

        private static AlarmDelayEnum SetAlarmDelayEnum(string currentDelay)
        {
            AlarmDelayEnum delayEnum = AlarmDelayEnum.Disabled;
            switch (currentDelay)
            {
                case "0": delayEnum = AlarmDelayEnum.FiveSeconds; break;
                case "T": delayEnum = AlarmDelayEnum.ThirtySeconds; break;
                case "L": delayEnum = AlarmDelayEnum.BatteryOnly; break;
                case "N": delayEnum = AlarmDelayEnum.Disabled; break;
            }
            return delayEnum;
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
}

