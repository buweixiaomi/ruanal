using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

namespace Ruanal.WebDomain.BLL
{
    public class OperationLogBll
    {
        DAL.OperationLogDal dal = new DAL.OperationLogDal();
        public Model.OperationLog AddLog(Model.OperationLog logmodel)
        {
            using (var dbconn = Pub.GetConn())
            {
                dbconn.BeginTransaction();
                try
                {
                    logmodel = dal.AddLog(dbconn, logmodel);
                    dbconn.Commit();
                    return logmodel;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }
        public PageModel<Model.OperationLog> GetLogPage(int pno, int pagesize, string keywords, string begintime, string endtime)
        {
            using (var dbconn = Pub.GetConn())
            {
                int totalcount = 0;
                var model = dal.GetLogPage(dbconn, pno, pagesize, keywords, begintime, endtime, out totalcount);
                return new PageModel<Model.OperationLog>() { List = model, PageNo = pno, PageSize = pagesize, TotalCount = totalcount };
            }
        }
        public Model.OperationLog GetLogDetail(int logid)
        {
            using (var dbconn = Pub.GetConn())
            {
                var model = dal.GetLogDetail(dbconn, logid);
                if (model == null)
                    throw new MException(MExceptionCode.NotExist, "用户不存在！");     
                return model;
            }
        }


    }
}
