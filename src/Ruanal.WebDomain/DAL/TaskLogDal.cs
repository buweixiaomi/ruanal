using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.DAL
{
    public class TaskLogDal
    {
        public Model.TaskWorkLog AddWorkLog(RLib.DB.DbConn dbconn, DateTime date, Model.TaskWorkLog model)
        {
            string sql = "insert into " + Pub.BuildWTbName(date) + "(taskid,nodeid,dispatchid,logtype,logtext,servertime) values " +
                " (@taskid,@nodeid,@dispatchid,@logtype,@logtext,@servertime);select @@identity;";
            model.LogId = dbconn.ExecuteScalar<int>(sql, new
            {
                taskid = model.TaskId,
                nodeid = model.NodeId,
                dispatchid = model.DispatchId ?? "",
                logtype = model.LogType,
                logtext = model.LogText ?? "",
                servertime = model.ServerTime
            });
            //  dbconn.GetIdentity();
            return model;
        }
        //public int AddWorkLogs(RLib.DB.DbConn dbconn, DateTime date, List<Model.TaskWorkLog> models)
        //{
        //    if (models == null || models.Count == 0)
        //        return 0;
        //    string sql = "insert into " + Pub.BuildWTbName(date) + "(taskid,nodeid,dispatchid,logtype,logtext,servertime) values ";
        //    string vt = " (@taskid_{0},@nodeid_{0},@dispatchid_{0},@logtype_{0},@logtext_{0},@servertime_{0})";
        //    List<string> valsql = new List<string>();
        //    List<RLib.DB.ProcedureParameter> paras = new List<RLib.DB.ProcedureParameter>();
        //    for (int i = 0; i < models.Count; i++)
        //    {
        //        valsql.Add(string.Format(vt, i));
        //        paras.Add(new RLib.DB.ProcedureParameter("@taskid_" + i, models[i].TaskId));
        //        paras.Add(new RLib.DB.ProcedureParameter("@nodeid_" + i, models[i].NodeId));
        //        paras.Add(new RLib.DB.ProcedureParameter("@dispatchid_" + i, models[i].DispatchId ?? ""));
        //        paras.Add(new RLib.DB.ProcedureParameter("@logtype_" + i, models[i].LogType));
        //        paras.Add(new RLib.DB.ProcedureParameter("@logtext_" + i, models[i].LogText ?? ""));
        //        paras.Add(new RLib.DB.ProcedureParameter("@servertime_" + i, models[i].ServerTime));
        //    }
        //    string insqlsql = sql + string.Join(",", valsql);
        //    int r = dbconn.ExecuteSql(insqlsql, paras);
        //    return r;
        //}

        public int AddWorkLogs(RLib.DB.DbConn dbconn, DateTime date, List<Model.TaskWorkLog> models)
        {
            var tbname = Pub.BuildWTbName(date);
            if (models == null || models.Count == 0)
                return 0;
            //区分大小写
            System.Data.DataTable tb = new System.Data.DataTable(tbname);
            tb.Columns.Add("taskId");
            tb.Columns.Add("nodeId");
            tb.Columns.Add("dispatchId");
            tb.Columns.Add("logType");
            tb.Columns.Add("logText");
            tb.Columns.Add("serverTime");

            foreach (var a in models)
            {
                tb.Rows.Add(
                    a.TaskId,
                    a.NodeId,
                    a.DispatchId ?? "",
                    a.LogType,
                    a.LogText,
                    a.ServerTime
                    );
            }
            dbconn.BuckCopy(tb, null, null);
            return models.Count;
        }


        public Model.TaskRunLog StartRunLog(RLib.DB.DbConn dbconn, Model.TaskRunLog model)
        {
            string sql = "insert into taskrunlog(runguid,taskid,nodeid,runtype,runservertime,logtext) values " +
                " (@runguid,@taskid,@nodeid,@runtype,@runservertime,@logtext)";
            dbconn.ExecuteSql(sql, new
            {
                runguid = model.RunGuid ?? "",
                taskid = model.TaskId,
                nodeid = model.NodeId,
                runtype = model.RunType,
                runservertime = model.RunServerTime,
                logtext = model.LogText ?? ""
            });
            model.LogId = dbconn.GetIdentity();
            return model;
        }

        public int EndRunLog(RLib.DB.DbConn dbconn, Model.TaskRunLog model)
        {
            string sql = "update taskrunlog set " +
                " endservertime=@endservertime " +
                ", enddbtime = getdate()" +
                ",resulttype=@resulttype " +
                ", logtext=@logtext " +
                " where taskid=@taskid and nodeid=@nodeid and runguid=@runguid; ";
            int r = dbconn.ExecuteSql(sql, new
            {
                runguid = model.RunGuid ?? "",
                taskid = model.TaskId,
                nodeid = model.NodeId,
                resulttype = model.ResultType,
                endservertime = model.EndServerTime,
                logtext = model.LogText ?? ""
            });
            return r;
        }

        public List<Model.TaskWorkLog> GetWorkLogPage(RLib.DB.DbConn dbconn,
            DateTime date, int? taskid, int? nodeid, int? logtype, bool isorderbyId,
            List<string> notContainText,
           string runguid, string keywords, DateTime? begintime, DateTime? endtime,
            int pno, int pagesize, out int totalcount)
        {

            List<RLib.DB.ProcedureParameter> paras = new List<RLib.DB.ProcedureParameter>();
            paras.Add(new RLib.DB.ProcedureParameter("begintime", begintime == null ? System.DBNull.Value : (object)begintime.Value));
            paras.Add(new RLib.DB.ProcedureParameter("endtime", endtime == null ? System.DBNull.Value : (object)endtime.Value));
            paras.Add(new RLib.DB.ProcedureParameter("taskid", taskid ?? 0));
            paras.Add(new RLib.DB.ProcedureParameter("nodeid", nodeid ?? 0));
            paras.Add(new RLib.DB.ProcedureParameter("logtype", logtype ?? 0));
            paras.Add(new RLib.DB.ProcedureParameter("runguid", runguid ?? ""));
            paras.Add(new RLib.DB.ProcedureParameter("keywords", keywords ?? ""));
            paras.Add(new RLib.DB.ProcedureParameter("startindex", (pno - 1) * pagesize + 1));
            paras.Add(new RLib.DB.ProcedureParameter("pagesize", pagesize));

            string sqltp = "select {0} from " + Pub.BuildWTbName(date) + "   with(nolock) {1} {2}";

            string fields = "row_number() over(order by " + (isorderbyId ? " logid asc " : " servertime desc,logid desc") + " ) as rownum,*";
            string wherecon = " where 1=1 ";
            if (taskid != null && taskid > 0)
            {
                wherecon += " and taskid=@taskid ";
            }
            if (nodeid != null && nodeid > 0)
            {
                wherecon += " and nodeid=@nodeid ";
            }
            if (logtype != null)
            {
                wherecon += " and logtype=@logtype ";
            }
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                wherecon += " and ( logtext like '%'+@keywords+'%' ) ";
            }
            if (!string.IsNullOrWhiteSpace(runguid))
            {
                wherecon += " and  dispatchId=@runguid  ";
            }
            if (begintime != null)
            {
                wherecon += " and createtime>=@begintime ";
            }
            if (endtime != null)
            {
                wherecon += " and createtime<=@endtime ";
            }
            if (notContainText != null && notContainText.Count > 0)
            {
                int notlikeindex = 0;
                foreach (var a in notContainText)
                {
                    if (string.IsNullOrWhiteSpace(a))
                        continue;
                    notlikeindex++;
                    wherecon += " and logText not like '%'+@notlikeindex" + (notlikeindex) + "+'%'";
                    paras.Add(new RLib.DB.ProcedureParameter("notlikeindex" + notlikeindex, a));
                }
            }

            string countsql = string.Format(sqltp, "count(1)", wherecon, "");
            string querysql = string.Format("select * from ( {0} ) A where  A.rownum>=@startindex and A.rownum<@startindex+@pagesize ",
                string.Format(sqltp, fields, wherecon, ""));
            totalcount = dbconn.ExecuteScalar<int>(countsql, paras);
            var models = dbconn.Query<Model.TaskWorkLog>(querysql, paras);
            return models;
        }

        public List<Model.TaskRunLog> GetRunLogPage(RLib.DB.DbConn dbconn, int? taskid, int? nodeid, int? resultType,
          string runguid, string keywords, DateTime? begintime, DateTime? endtime,
           int pno, int pagesize, out int totalcount)
        {
            string sqltp = "select {0} from taskrunlog  with(nolock) {1} {2}";
            string fields = "row_number() over(order by runServerTime desc) as rownum,*";
            string wherecon = " where 1=1 ";
            if (taskid != null && taskid > 0)
            {
                wherecon += " and taskid=@taskid ";
            }
            if (nodeid != null && nodeid > 0)
            {
                wherecon += " and nodeid=@nodeid ";
            }
            if (resultType != null)
            {
                wherecon += " and resultType=@resulttype ";
            }
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                wherecon += " and (logtext like '%'+@keywords+'%') ";
            }

            if (!string.IsNullOrWhiteSpace(runguid))
            {
                wherecon += " and   runGuid = @runguid ";
            }

            if (begintime != null)
            {
                wherecon += " and runServerTime>=@begintime ";
            }
            if (endtime != null)
            {
                wherecon += " and runServerTime<=@endtime ";
            }

            var paras = new
            {
                begintime = begintime,
                endtime = endtime,
                taskid = taskid,
                nodeid = nodeid,
                resulttype = resultType,
                runguid = runguid ?? "",
                keywords = keywords ?? "",
                startindex = (pno - 1) * pagesize + 1,
                pagesize = pagesize
            };
            string countsql = string.Format(sqltp, "count(1)", wherecon, "");
            string querysql = string.Format("select * from ( {0} ) A where  A.rownum>=@startindex and A.rownum<@startindex+@pagesize ",
                string.Format(sqltp, fields, wherecon, ""));
            totalcount = dbconn.ExecuteScalar<int>(countsql, paras);
            var models = dbconn.Query<Model.TaskRunLog>(querysql, paras);
            return models;
        }

        public int DeleteRunLog(RLib.DB.DbConn dbconn, DateTime endtime)
        {
            int timeOut = 2;//mins
            string sql = "delete from taskRunLog where runServerTime<@endtime ";
            int r = dbconn.ExecuteSql(sql, new { endtime = endtime }, timeOut);
            var obj = dbconn.ExecuteScalar("select min(logid) from taskRunLog", null);
            if (RLib.Utils.Converter.ObjToInt(obj) > 5000 * 10000)
            {
                dbconn.ExecuteSql("DBCC CHECKIDENT (taskrunlog,reseed,1) ", new { });
            }
            return r;
        }
        public int DeleteWorkLog(RLib.DB.DbConn dbconn, DateTime endtime)
        {
            if ((DateTime.Now - endtime).TotalDays <= 2)
                throw new MException("最近两天的日志不能删除");
            int max = 100;
            string tbname = string.Empty;
            int c = 0;
            while (Pub.ExistTableInDB(dbconn, tbname = "taskWorkLog" + endtime.ToString("yyMMdd")) && max > 0)
            {
                string sql = "drop table " + tbname;
                dbconn.ExecuteSql(sql);
                max--;
                endtime = endtime.AddDays(-1);
                c++;
            }
            return c;
        }

        public DateTime GetMinWorkLogDate(RLib.DB.DbConn dbconn)
        {
            string sql = @"select top 1 name from sys.tables where type='U' and name like 'taskWorkLog%' order by name asc";
            var obj = dbconn.ExecuteScalar(sql, null);
            if (obj == null) return DateTime.Now;
            if (string.IsNullOrEmpty(obj.ToString())) return DateTime.Now;
            var dtstring = obj.ToString().Substring("taskWorkLog".Length);
            return DateTime.Parse("20" + dtstring.Substring(0, 2) + "-" + dtstring.Substring(2, 2) + "-" + dtstring.Substring(4, 2));
        }

        public DateTime GetMinRunLogDate(RLib.DB.DbConn dbconn)
        {
            string sql = @"select top 1 runServerTime from taskRunLog(nolock) order by runServerTime asc";
            var obj = dbconn.ExecuteScalar(sql, null);
            if (obj == null) return DateTime.Now;
            if (string.IsNullOrEmpty(obj.ToString())) return DateTime.Now;
            return Convert.ToDateTime(obj.ToString());
        }
    }
}
