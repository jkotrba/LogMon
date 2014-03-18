using System;

namespace LogMon
{
    public class LogEntry
    {
        public int ID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Application { get; set; }
        public string Thread { get; set; }
        public string MachineName { get; set; }
        public string Logger { get; set; }
        public string Exception { get; set; }
        public string NTUser { get; set; }
    }
}
