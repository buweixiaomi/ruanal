using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.BLL
{
    public class TaskLogBll
    {
        DAL.TaskLogDal tasklogdal = new DAL.TaskLogDal();
        public PageModel<Model.TaskRunLog> PageRunLog(int? taskid, int? nodeid, int? resultType, string runguid, string keywords, DateTime? begintime, DateTime? endtime, int pno)
        {
            int pagesize = 20;
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                int totalcount = 0;
                var model = tasklogdal.GetRunLogPage(dbconn, taskid, nodeid, resultType, runguid, keywords, begintime, endtime, pno, pagesize, out totalcount);
                return new PageModel<Model.TaskRunLog>()
                {
                    List = model,
                    PageNo = pno,
                    PageSize = pagesize,
                    TotalCount = totalcount
                };
            }
        }


        public PageModel<Model.TaskWorkLog> PageWorkLog(DateTime date, int? taskid, int? nodeid, int? logtype, string runguid, string keywords, DateTime? begintime, DateTime? endtime, int pno)
        {
            int pagesize = 20;
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                int totalcount = 0;
                if (!Pub.ExistTableInDB(dbconn, Pub.BuildWTbName(date)))
                {
                    return new PageModel<Model.TaskWorkLog>()
                    {
                        List = new List<Model.TaskWorkLog>(),
                        PageNo = pno,
                        PageSize = pagesize,
                        TotalCount = 0
                    };
                }
                var model = tasklogdal.GetWorkLogPage(dbconn, date, taskid, nodeid, logtype, false, null,
                    runguid, keywords, begintime, endtime, pno, pagesize, out totalcount);
                return new PageModel<Model.TaskWorkLog>()
                {
                    List = model,
                    PageNo = pno,
                    PageSize = pagesize,
                    TotalCount = totalcount
                };
            }
        }

        public PageModel<Model.TaskWorkLog> PageWorkLogErrorMode(DateTime date, List<string> notContainText, DateTime? begintime, DateTime? endtime, int pno)
        {
            int pagesize = 20;
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                int totalcount = 0;
                if (!Pub.ExistTableInDB(dbconn, Pub.BuildWTbName(date)))
                {
                    return new PageModel<Model.TaskWorkLog>()
                    {
                        List = new List<Model.TaskWorkLog>(),
                        PageNo = pno,
                        PageSize = pagesize,
                        TotalCount = 0
                    };
                }
                var model = tasklogdal.GetWorkLogPage(dbconn, date, null, null, 1, false, notContainText,
                    null, null, begintime, endtime, pno, pagesize, out totalcount);
                return new PageModel<Model.TaskWorkLog>()
                {
                    List = model,
                    PageNo = pno,
                    PageSize = pagesize,
                    TotalCount = totalcount
                };
            }
        }
    }
}
