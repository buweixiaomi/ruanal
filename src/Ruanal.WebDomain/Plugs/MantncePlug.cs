using Ruanal.WebDomain.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Ruanal.WebDomain.Plugs
{
    public class MantncePlug : IPlug
    {
        SysToolBll bll = new SysToolBll();
        public override void RunOnce()
        {
            //bool automan = Pub.GetConfig("autoMan", "false").ToLower() == "true";
            //int runlogdays = Convert.ToInt32(Pub.GetConfig("autoMan:runLogDays", "7"));
            //int worklogdays = Convert.ToInt32(Pub.GetConfig("autoMan:workLogDays", "7"));
            //int dispatchlogdays = Convert.ToInt32(Pub.GetConfig("autoMan:dispLogDays", "7"));


            bool automan = Config.GetBool("autoMan", false);
            int runlogdays = Config.GetInt("autoMan:runLogDays", 7);
            int worklogdays = Config.GetInt("autoMan:workLogDays", 7);
            int dispatchlogdays = Config.GetInt("autoMan:dispLogDays", 7);

            if (!automan) return;
            DateTime dtrunlog = DateTime.Now.AddDays(-runlogdays);
            DateTime dtworklog = DateTime.Now.AddDays(-worklogdays);
            DateTime dtdislog = DateTime.Now.AddDays(-dispatchlogdays);
            var dts = bll.GetMinWorkRunDisTime();
            int maxtime = 100;

            maxtime = 100;
            while (maxtime > 0 && dtworklog.Subtract(dts[0]).TotalDays > 0.1)
            {
                bll.DeleteWorkLog(dts[0]);

                dts[0] = dts[0].AddDays(0.3);
                maxtime--;
            }

            maxtime = 100;
            while (maxtime > 0 && dtrunlog.Subtract(dts[1]).TotalDays > 0.1)
            {
                bll.DeleteRunLog(dts[1]);

                dts[1] = dts[1].AddDays(0.3);
                maxtime--;
            }

            maxtime = 100;
            while (maxtime > 0 && dtdislog.Subtract(dts[2]).TotalDays > 0.1)
            {
                bll.DeleteDispatchLog(dts[2]);

                dts[2] = dts[2].AddDays(0.3);
                maxtime--;
            }

        }



    }


}
