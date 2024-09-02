using CommunityToolkit.Mvvm.ComponentModel;
using LABPOWER_APC.Utilities;
using LiveCharts.Defaults;
using Microsoft.Xaml.Behaviors.Layout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LABPOWER_APC.Model
{
    public partial class logger : ObservableObject
    {
        [ObservableProperty]
        private List<LogEntry> _LogEntries = new List<LogEntry>();
        public logger()
        {
            
        }


        public void Log(LogType type, string message)
        {
            LogEntries.Add(new LogEntry(type, message));
        }

        public IEnumerable<LogEntry> GetLogs(LogType? type = null)
        {
            if (type.HasValue)
            {
                return LogEntries.Where(entry => entry.Type == type.Value);
            }
            return LogEntries;
        }

        public void ClearLogs()
        {
            LogEntries.Clear();
        }
        public void logger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LogEntries))
            {
                XmlHelper.Serialize(LogEntries, "log.xml");
            }
            
        }


    }
    public enum LogType
    {
        Error,
        Warning,
        Success,
        Info
    }

    public class LogEntry
    {
        public LogType Type { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public LogEntry(LogType type, string message)
        {
            Type = type;
            Message = message;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Type}: {Message}";
        }
    }
}
