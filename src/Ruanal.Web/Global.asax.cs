using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Ruanal.Web
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        static readonly object lockers = new object();
        static bool IsInitNotify = false;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);


            Ruanal.WebDomain.BLL.ManagerBll.InitDefault();

            WebDomain.PlugManager plugManager = new WebDomain.PlugManager();
            plugManager.Start();
            
            InitNotifyServer();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
        }

        private void InitNotifyServer()
        {
            lock (lockers)
            {
                if (IsInitNotify) return;
                IsInitNotify = true;
            }
            System.Threading.ThreadPool.QueueUserWorkItem((x) =>
            {
                try
                {
                    var notifyserver = Ruanal.WebDomain.Pub.GetConfig(Ruanal.Core.ConfigConst.NotifyListenName, "");
                    if (!string.IsNullOrEmpty(notifyserver))
                    {
                       Ruanal.Core.Notify.NotifyHelper.Init("ruanalweb", notifyserver, "");
                    }
                }
                catch { }
            });
        }

    }
}