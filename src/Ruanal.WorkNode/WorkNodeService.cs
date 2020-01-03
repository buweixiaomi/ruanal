using Ruanal.ServiceSchedule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Ruanal.WorkNode
{
    partial class WorkNodeService : ServiceBase
    { 
        public WorkNodeService()
        {
            InitializeComponent();
        }

        ServiceContainer sc = null;
        protected override void OnStart(string[] args)
        {
            RLib.WatchLog.Loger.Log("正在启动...", "");
            sc = new WorkNodeContainer();
            sc.Start();
            RLib.WatchLog.Loger.Log("启动成功！", "");
        }

        protected override void OnStop()
        {
            if (sc != null)
            {
                sc.Stop();
            }
        }
    }
}
