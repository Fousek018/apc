using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace LABPOWER_APC.Utilities
{
    public static class XmlHelper
    {
        public static void Serialize<T>(T data, string fileName)
        {
            string directory = System.IO.Path.Combine(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Data"));
            string filePath = Path.Combine(directory, fileName);

            var serializer = new XmlSerializer(typeof(T));
            using (var writer = XmlWriter.Create(filePath))
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
    }
}
