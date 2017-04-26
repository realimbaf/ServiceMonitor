using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Wiki.ServiceMonitor
{
    partial class MonitorService : ServiceBase
    {
        MonitorSrv _service=new MonitorSrv();
        public MonitorService()
        {
            InitializeComponent();
        }

        public void OnStart()
        {
            OnStart(new string[0]);
        }

        protected override void OnStart(string[] args)
        {
            this._service.Start();
        }

        protected override void OnStop()
        {
            this._service.Stop();
        }
    }
}
