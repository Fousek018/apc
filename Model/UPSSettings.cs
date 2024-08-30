using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace LABPOWER_APC.Model
{
    public partial class UPSSettings : ObservableObject
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [ObservableProperty]
        public string _PortName = "COM0";

        [ObservableProperty]
        public int _BaudRate = 2400;

        [ObservableProperty]
        public Parity _Parity = Parity.None;

        [ObservableProperty]
        public int _DataBits = 8;

        [ObservableProperty]
        public StopBits _StopBits = StopBits.One;


        [ObservableProperty]
        public int _ShutdownTimeLeft = 5;

        public enum ShutdownEnum
        {

            [Description("5 seconds")]
            fiveSecond = 5,
            [Description("10 seconds")]
            tenSecond = 10,
            [Description("15 seconds")]
            fivteenSecond = 15,
            
        }

        #region Methods

        /// <summary>
        /// Send a PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of changed property</param>


        /// <summary>
        /// Serialize data to a settings file.
        /// </summary>
        /// <param name="info">Info to serialize</param>

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

        /// <summary>
        /// Deserialize data from settings file
        /// </summary>
        /// <returns>returns UPSSettings from settings file or default settings if no file found</returns>
        //public static UPSSettings Deserialize()
        //{
        //    string saveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //    string fileName = "ups.xml";
        //    var serializer = new XmlSerializer(typeof(UPSSettings));
        //    UPSSettings settings = new UPSSettings();
        //    string filePath = Path.Combine(saveDirectory, fileName);
        //    if (File.Exists(filePath))
        //    {
        //        using (var reader = XmlReader.Create(filePath))
        //        {
        //            settings = (UPSSettings)serializer.Deserialize(reader);
        //        }
        //    }
        //    return settings;
        //}
    }
}
#endregion