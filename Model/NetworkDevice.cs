﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Drawing;

namespace LABPOWER_APC.Model
{
    public class NetworkDevice
    {
        public string? IPAddress { get; set; }
        public string? MACAddress { get; set; }
        public string? HostName { get; set; }
    }
    public class ChosenNetworkDevice
    {
        public string? IPAddress { get; set; }
        public string? HostName { get; set; }
        public string? nameOfTaks { get; set; } = "None";
        public int? timeOfexecute { get; set; } = 0;

    }
}
