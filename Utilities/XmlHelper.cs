using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using LABPOWER_APC.Model;

namespace LABPOWER_APC.Utilities
{
    public static class XmlHelper
    {
        public static void Serialize<T>(T data, string fileName)
        {
            string directory = System.IO.Path.Combine(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Data"));
            string filePath = Path.Combine(directory, fileName);

            var serializer = new XmlSerializer(typeof(T));
            using (var writer = XmlWriter.Create(filePath, new XmlWriterSettings { CloseOutput = true }))
            {
                serializer.Serialize(writer, data);
            }
        }

        public static T Deserialize<T>(string fileName)
        {
            string directory = System.IO.Path.Combine(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Data"));

            string filePath = Path.Combine(directory, fileName);

            if (!File.Exists(filePath))
            {
                return default(T);
            }

            var serializer = new XmlSerializer(typeof(T));
            using (var reader = XmlReader.Create(filePath))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static ChosenNetworkDevice FindByIPAddress(List<ChosenNetworkDevice> existingData, string ipAddress)
        {
            return existingData.FirstOrDefault(d => d.IPAddress == ipAddress);
        }

        public static void UpdateFile(ChosenNetworkDevice data, string fileName)
        {
            string directory = System.IO.Path.Combine(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Data"));
            string filePath = Path.Combine(directory, fileName);

            var existingData = Deserialize<List<ChosenNetworkDevice>>(fileName);
            var matchingData = FindByIPAddress(existingData, data.IPAddress);

            if (matchingData != null)
            {
                // Update the matching data with the new values
                matchingData.HostName = data.HostName;
                matchingData.nameOfTaks = data.nameOfTaks;
                matchingData.timeOfexecute = data.timeOfexecute;

                Serialize(existingData, fileName);
            }
        }
    }
}
