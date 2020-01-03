using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.BLL
{
    public class SysToolBll
    {
        public int DeleteRunLog(DateTime endtime)
        {
            if ((DateTime.Now - endtime).TotalDays < 3)
            {
                throw new MException("近三天的日志不能清除！");
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                int deletecount = new DAL.TaskLogDal().DeleteRunLog(dbconn, endtime);
                return deletecount;
            }
        }
        public int DeleteWorkLog(DateTime endtime)
        {
            if ((DateTime.Now - endtime).TotalDays < 3)
            {
                throw new MException("近三天的日志不能清除！");
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                int deletecount = new DAL.TaskLogDal().DeleteWorkLog(dbconn, endtime);
                return deletecount;
            }
        }

        public int DeleteDispatchLog(DateTime endtime)
        {
            if ((DateTime.Now - endtime).TotalDays < 3)
            {
                throw new MException("近3天的分配日志不能清除！");
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                int deletecount = new DAL.DispatchDal().DeleteDispatchLog(dbconn, endtime);
                return deletecount;
            }
        }

        public DateTime[] GetMinWorkRunDisTime()
        {
            DateTime[] ts = new DateTime[3];
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                ts[0] = new DAL.TaskLogDal().GetMinWorkLogDate(dbconn);
                ts[1] = new DAL.TaskLogDal().GetMinRunLogDate(dbconn);
                ts[2] = new DAL.DispatchDal().GetMinLogDate(dbconn);
                return ts;
            }
        }

        public Dictionary<int, List<Model.TaskVersion>> GetTasksTopVersions(int keeppackagecount)
        {
            int topcount = Math.Max(5, keeppackagecount);
            Dictionary<int, List<Model.TaskVersion>> rdata = new Dictionary<int, List<Model.TaskVersion>>();
            DAL.TaskDal taskdal = new DAL.TaskDal();
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var tasks = taskdal.GetAllTask(dbconn, -1);
                foreach (var a in tasks)
                {
                    rdata.Add(a.TaskId, taskdal.GetTaskAllVersion(dbconn, a.TaskId, topcount));
                }
            }
            return rdata;
        }

    }
}
