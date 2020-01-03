using Ruanal.ServiceSchedule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Ruanal.DispatchNode
{
    partial class DispatchNodeService : ServiceBase
    {
        ServiceContainer sc;
        public DispatchNodeService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            RLib.WatchLog.Loger.Log("正在启动...", "");
            sc = new DispatchContainer();
            sc.Start();
            RLib.WatchLog.Loger.Log("启动成功！", "");
        }

        protected override void OnStop()
        {
            RLib.WatchLog.Loger.Log("停止...", "");
            try
            {
                sc.Stop();
                RLib.WatchLog.Loger.Log("停止成功！", "");
            }
            catch (Exception ex)
            {
                RLib.WatchLog.Loger.Log("失败！", ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
