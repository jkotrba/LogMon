using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Macu.Apply.LogMonService
{
    public partial class LogMonService : ServiceBase
    {
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private LogMon.Monitor monitor = new LogMon.Monitor();
        public LogMonService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _timer.AutoReset = true;
            _timer.Interval = Interval;
            _timer.Elapsed += OnElapsedEvent;
            _timer.Start();
        }

        protected override void OnStop()
        {
            _timer.Stop();
        }

        private void OnElapsedEvent(object sender, ElapsedEventArgs e)
        {
            monitor.Run();
        }
        public int Interval
        {
            get { return int.Parse(ConfigurationManager.AppSettings["Interval"]); }
        }
    }
}
