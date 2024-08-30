using CommunityToolkit.Mvvm.ComponentModel;
using LABPOWER_APC.Model;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using LABPOWER_APC.Utilities;
using System.Windows.Documents;

namespace LABPOWER_APC.VM
{


    public partial class remotePCVM : ObservableObject
    {
        [ObservableProperty]
        public string? _IPAddress;

        [ObservableProperty]
        public string? _RemoteName;

        [ObservableProperty]
        public string _SelectedTask;

        [ObservableProperty]
        public List<string> _AvailableTasks;

        public remotePCVM(ChosenNetworkDevice chosedDevice)
        {
            IPAddress = chosedDevice.IPAddress; // Assign IPAddress from chosedDevice to _IPAddress
            RemoteName = chosedDevice.HostName; // Assign HostName from chosedDevice to _RemoteName

            // Deserialize field tasks from XML
            _AvailableTasks = XmlHelper.Deserialize<List<string>>("fieldTaks.xml");
            

        }
    }
}
