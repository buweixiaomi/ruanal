using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.DAL
{
    public class OperationLogDal
    {
        public Model.OperationLog AddLog(RLib.DB.DbConn dbconn, Model.OperationLog logmodel)
        {
            string sql = @"INSERT INTO OperationLog (OperationContent,OperationName,Createtime,OperationTitle,Module)
                  values (@OperationContent,@OperationName,getdate(),@OperationTitle,@Module) ";
            dbconn.ExecuteSql(sql, new
            {
                OperationContent = logmodel.OperationContent,
                OperationName = logmodel.OperationName,
                Createtime = logmodel.Createtime,
                OperationTitle = logmodel.OperationTitle,
                Module = logmodel.Module
            });
            return logmodel;
        }

        public List<Model.OperationLog> GetLogPage(RLib.DB.DbConn dbconn, int pno, int pagesize, string keywords, string begintime, string endtime, out int totalcount)
        {
            string sql = @"select * from (select ROW_NUMBER() over(order by id desc ) as rownum, * from OperationLog where
                                    (OperationName like ('%'+@keywords+'%')
                                     or OperationTitle like ('%'+@keywords+'%')
                                     or Module like ('%'+@keywords+'%')) 
                                     and Createtime between @begintime and @endtime ) A
                                    where A.rownum> @startindex and A.rownum<=@startindex+@pagesize;";
            string countsql = @"select count(1) from OperationLog where
                                    (OperationName like ('%'+@keywords+'%')
                                     or OperationTitle like ('%'+@keywords+'%')
                                      or Module like ('%'+@keywords+'%')) 
                                     and Createtime between @begintime and @endtime";
            var models = dbconn.Query<Model.OperationLog>(sql, new
            {
                keywords = keywords ?? "",
                begintime = begintime == "" ? DateTime.Now.AddMonths(-3).ToString() : begintime,
                endtime = endtime == "" ? DateTime.Now.AddDays(1).ToString() : endtime,
                startindex = (pno - 1) * pagesize,
                pagesize = pagesize
            });
            totalcount = dbconn.ExecuteScalar<int>(countsql, new
            {
                keywords = keywords ?? "",
                begintime = begintime == "" ? DateTime.Now.AddMonths(-3).ToString() : begintime,
                endtime = endtime == "" ? DateTime.Now.AddDays(1).ToString() : endtime
            });
            return models;
        }

        public Model.OperationLog GetLogDetail(RLib.DB.DbConn dbconn, int logid)
        {
            string sql = "select * from OperationLog where id=@id;";
            var model = dbconn.Query<Model.OperationLog>(sql, new { id = logid }).FirstOrDefault();
            return model;
        }
    }
}
