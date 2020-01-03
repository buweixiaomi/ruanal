using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.DispatchNode
{
    using Ruanal.Core;
    using Ruanal.Core.Utils;
    using ServiceSchedule;
    using System.Reflection;
    using System.ServiceProcess;

    public class Program
    {
        public static string CurrServiceName;
        static void Main(string[] args)
        {
            //自维护代码
            if (ServiceMaintance.CheckMaintanceCmd(args)) { return; }

            bool isServiceModel = Environment.UserInteractive ? false : true;
           // bool isServiceModel = Config.GetSystemConfig(ConfigConst.RunInServiceMode_Name, "false").ToLower() == "true";
            if (isServiceModel)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new DispatchNodeService() };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                Console.Write("服务管理 1:安装， 2:卸载  ，其它进入调试模式：");
                string s = Console.ReadLine();
                CurrServiceName = Config.GetSystemConfig(ConfigConst.ServiceName_Name, typeof(DispatchNodeService).FullName);

                switch (s)
                {
                    case "1":
                        ServiceHelper.Install(CurrServiceName, Assembly.GetExecutingAssembly().Location);
                        Console.ReadKey();
                        break;
                    case "2":
                        if (ServiceHelper.ServiceIsExisted(CurrServiceName))
                            ServiceHelper.Uninstall(CurrServiceName, Assembly.GetExecutingAssembly().Location);
                        Console.ReadKey();
                        break;
                    default:
                        RLib.WatchLog.Loger.Log("正在启动...", "");
                        ServiceContainer sc = new DispatchContainer();
                        sc.Start();
                        RLib.WatchLog.Loger.Log("启动成功！", "");
                        Console.Read();
                        break;
                }

            }

        }
    }
}
