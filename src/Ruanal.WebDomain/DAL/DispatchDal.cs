using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.DAL
{
    public class DispatchDal
    {
        public int AddBatch(RLib.DB.DbConn dbconn, List<Model.Dispatch> modelsx)
        {
            if (modelsx == null || modelsx.Count == 0)
                return 0;
            var skp = 0;
            var psize = 30;
            if (dbconn.GetBaseConnection().DataSource.ToUpper().EndsWith("SQLEXPRESS"))
            {
                psize = 1;
            }
            while (skp < modelsx.Count)
            {
                var models = modelsx.Skip(skp).Take(psize);
                skp += psize;
                string sql = @"INSERT INTO [dbo].[dispatch]
            ([groupId],[invokeId],[taskId],[dispatchState],[runKey],[nickName],[runArgs],[createTime],[expireTime],nodeId) VALUES ";
                string vlt = "(@groupid_{0},@invokeid_{0},@taskid_{0},0,@runkey_{0},@nickname_{0},@runargs_{0},getdate(),@expiretime_{0},0 )";
                List<string> sqls = new List<string>();
                List<RLib.DB.ProcedureParameter> para = new List<RLib.DB.ProcedureParameter>();
                int i = 0;
                foreach (var a in models)
                {
                    i++;
                    para.Add(new RLib.DB.ProcedureParameter("@groupid_" + i, a.GroupId));
                    para.Add(new RLib.DB.ProcedureParameter("@invokeid_" + i, a.InvokeId));
                    para.Add(new RLib.DB.ProcedureParameter("@taskid_" + i, a.TaskId));
                    para.Add(new RLib.DB.ProcedureParameter("@runkey_" + i, a.RunKey));
                    para.Add(new RLib.DB.ProcedureParameter("@nickname_" + i, a.NickName));
                    para.Add(new RLib.DB.ProcedureParameter("@runargs_" + i, a.RunArgs ?? ""));
                    para.Add(new RLib.DB.ProcedureParameter("@expiretime_" + i, a.ExpireTime));
                    sqls.Add(string.Format(vlt, i));
                }
                sql = sql + string.Join(",", sqls);
                int r = dbconn.ExecuteSql(sql, para);
            }
            return modelsx.Count;
        }

        public List<Model.Dispatch> GetUnDispatchs(RLib.DB.DbConn dbconn, int taskId, int topcount, bool nolock)
        {
            string limit = topcount > 0 ? " top " + topcount + " " : "";
            string sql = " select " + limit + " * from dispatch" + (nolock ? "(nolock)" : "") + " where taskId=@taskid and dispatchState=0 ;";
            var models = dbconn.Query<Model.Dispatch>(sql, new
            {
                taskid = taskId
            });
            return models;
        }

        public int AutoExpire(RLib.DB.DbConn dbconn)
        { 
            string sql = " update  dispatch set dispatchState=10 where  dispatchState=0 and expireTime<getdate()   ;";
            var models = dbconn.ExecuteSql(sql, new { });
            return models;
        }

        public List<Model.Dispatch> GetWatchExecDispatchs(RLib.DB.DbConn dbconn, int nodeId, int taskId, int topcount, bool nolock)
        {
            string limit = topcount > 0 ? " top " + topcount + " " : "";
            string sql = " select " + limit + " * from dispatch" + (nolock ? "(nolock)" : "") + " where nodeid=@nodeid and taskId = @taskid and dispatchState=1  order by dispatchid asc   ;";
            var models = dbconn.Query<Model.Dispatch>(sql, new
            {
                nodeid = nodeId,
                taskid = taskId
            });
            return models;
        }

        public Model.Dispatch GetDetail(RLib.DB.DbConn dbconn, int dispatchId)
        {
            string sql = " select * from dispatch where dispatchid=@dispatchid; ";
            var models = dbconn.Query<Model.Dispatch>(sql, new
            {
                dispatchId = dispatchId
            }).FirstOrDefault();
            return models;
        }

        public int Delete(RLib.DB.DbConn dbconn, int dispatchid)
        {
            string sql = "update dispatch set dispatchState=-1  where dispatchId=@dispatchid ;";
            int r = dbconn.ExecuteSql(sql, new { dispatchid = dispatchid });
            return r;
        }


        public int SetDispatch(RLib.DB.DbConn dbconn, int dispatchid, int nodeId)
        {
            string sql = "update dispatch set dispatchState=1,dispatchTime = getdate(),nodeId=@nodeid where dispatchId=@dispatchid and dispatchState=0 ";
            int r = dbconn.ExecuteSql(sql, new { dispatchid = dispatchid, nodeid = nodeId });
            return r;
        }

        public int SetExec(RLib.DB.DbConn dbconn, int dispatchid)
        {
            string sql = "update dispatch set dispatchState=2,executeTime = getdate() where dispatchId=@dispatchid and dispatchState=1;";
            int r = dbconn.ExecuteSql(sql, new { dispatchid = dispatchid });
            return r;
        }

        public int EndExec(RLib.DB.DbConn dbconn, int dispatchid, bool success, string resulttext)
        {
            string sql = "update dispatch set dispatchState=@rstate,resultText=@resulttext,endtime = getdate() " +
                " where dispatchId=@dispatchid and dispatchState=2;";
            int r = dbconn.ExecuteSql(sql, new { rstate = success ? 3 : 4, dispatchid = dispatchid, resulttext = resulttext ?? "" });
            return r;
        }

        public int SuccessEndExec(RLib.DB.DbConn dbconn, int dispatchid)
        {
            string sql = "update dispatch set dispatchState=@rstate,endtime = getdate() " +
                " where dispatchId=@dispatchid;";
            int r = dbconn.ExecuteSql(sql, new { rstate = 3, dispatchid = dispatchid});
            return r;
        }

        public int SkipExec(RLib.DB.DbConn dbconn, int dispatchid)
        {
            string sql = "update dispatch set dispatchState=@rstate,resultText=@resulttext,endTime=getdate() " +
                " where dispatchId=@dispatchid and dispatchState=2;";
            int r = dbconn.ExecuteSql(sql, new { rstate = 5, dispatchid = dispatchid, resulttext = "" });
            return r;
        }
        public int ExistGroupId(RLib.DB.DbConn dbconn, string groupid)
        {
            string sql = "select count(1) from dispatch where groupid=@groupid  and dispatchState<>-1 ;";
            int r = dbconn.ExecuteScalar<int>(sql, new { groupid = groupid });
            return r;
        }

        public int GroupWaitDispatchCount(RLib.DB.DbConn dbconn, string groupid)
        {
            string sql = "select count(1) from dispatch where groupid=@groupid and dispatchState=0 and expireTime>getdate() ;";
            int r = dbconn.ExecuteScalar<int>(sql, new { groupid = groupid });
            return r;
        }

        public Model.Dispatch GetLastDispatchKeyItem(RLib.DB.DbConn dbconn, int taskId, string runKey)
        {
            string sql = "select top 1 * from dispatch(nolock) where taskid=@taskid and runkey=@runkey and executeTime>@executeTime " +
                " and dispatchState between 2 and 4 order by executeTime desc ";
            var item = dbconn.SqlToModel<Model.Dispatch>(sql, new { runkey = runKey, taskid = taskId, executeTime = DateTime.Now.AddDays(-1) }).FirstOrDefault();
            return item;
        }


        public List<Model.Dispatch> GetDispatchs(RLib.DB.DbConn dbconn, int? taskid, int? nodeid, int? dispatchState,
            string keywords, DateTime? begintime, DateTime? endtime, int pno, int pagesize, out int totalcount)
        {
            string sqltp = "select {0} from dispatch   with(nolock) {1} {2}";
            string fields = "row_number() over(order by dispatchId desc) as rownum,*";
            string wherecon = string.Empty;
            if (dispatchState == -1)
            {
                wherecon = " where 1=1 ";
            }
            else
            {
                wherecon = " where dispatchState<>-1 ";
            }
            if (taskid != null && taskid > 0)
            {
                wherecon += " and taskid=@taskid ";
            }
            if (nodeid != null)
            {
                wherecon += " and nodeid=@nodeid ";
            }
            if (dispatchState != null)
            {
                wherecon += " and dispatchState=@dispatchstate ";
            }

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                wherecon += " and ( nickname like '%'+@keywords+'%' or " +
                    " runkey like '%'+@keywords+'%' or " +
                    "groupId like '%'+@keywords+'%' or invokeId like '%'+@keywords+'%'  )";
            }
            if (begintime != null)
            {
                wherecon += " and createtime>=@begintime ";
            }
            if (endtime != null)
            {
                wherecon += " and createtime<=@endtime ";
            }
            var paras = new
            {
                begintime = begintime,
                endtime = endtime,
                taskid = taskid,
                nodeid = nodeid,
                dispatchstate = dispatchState,
                keywords = keywords ?? "",
                startindex = (pno - 1) * pagesize + 1,
                pagesize = pagesize
            };
            string countsql = string.Format(sqltp, "count(1)", wherecon, "");
            string querysql = string.Format("select * from ( {0} ) A where  A.rownum>=@startindex and A.rownum<@startindex+@pagesize ",
                string.Format(sqltp, fields, wherecon, ""));
            totalcount = dbconn.ExecuteScalar<int>(countsql, paras);
            var models = dbconn.Query<Model.Dispatch>(querysql, paras);
            return models;
        }



        public List<Model.Dispatch> GetRunDispatchs(RLib.DB.DbConn dbconn, DateTime startbefore)
        {
            string querysql = "select top 20000 * from dispatch   with(nolock) where dispatchState=2 and ExecuteTime<@startbefore and runkey<>''; ";

            var paras = new
            {
                startbefore = startbefore
            };
            var models = dbconn.Query<Model.Dispatch>(querysql, paras);
            return models;
        }


        public int DeleteDispatchLog(RLib.DB.DbConn dbconn, DateTime endtime)
        {
            int timeOut = 2;//mins
            string sql = "delete from dispatch where createTime<@endtime ";
            int r = dbconn.ExecuteSql(sql, new { endtime = endtime }, timeOut);
            var obj = dbconn.ExecuteScalar("select min(dispatchId) from dispatch", null);
            if (RLib.Utils.Converter.ObjToInt(obj) > 5000 * 10000)
            {
                dbconn.ExecuteSql("DBCC CHECKIDENT (dispatch,reseed,1) ", new { });
            }
            return r;
        }

        public DateTime GetMinLogDate(RLib.DB.DbConn dbconn)
        {
            string sql = @"select top 1 createTime from dispatch(nolock) order by createTime asc";
            var obj = dbconn.ExecuteScalar(sql, null);
            if (obj == null) return DateTime.Now;
            if (string.IsNullOrEmpty(obj.ToString())) return DateTime.Now;
            return Convert.ToDateTime(obj.ToString());
        }

        private string BuildDispatchKey(int taskid, string runkey)
        {
            return taskid + "-" + runkey;
        }
        public void SetTaskDispatchKeyRunning(RLib.DB.DbConn dbconn, int taskid, string runkey,int dispathid)
        {
            if (string.IsNullOrWhiteSpace(runkey))
                return;
            string sql = "update dispatchKeyState set [keyState]=1,[dispatchId]=@dispatchid where [disKey]=@diskey ;";
            string sql2 = "insert into dispatchKeyState(disKey,[keyState],[dispatchId]) values (@diskey,1,@dispatchid) ;";
            var key = BuildDispatchKey(taskid, runkey);
            int r = dbconn.ExecuteSql(sql, new { diskey = key, dispatchid = dispathid });
            if (r == 0)
            {
                r = dbconn.ExecuteSql(sql2, new { diskey = key, dispatchid = dispathid });
            }
        }

        public void SetTaskDispatchKeyStopped(RLib.DB.DbConn dbconn, int taskid, string runkey)
        {

            if (string.IsNullOrWhiteSpace(runkey))
                return;
            string sql = "update dispatchKeyState set [keyState]=0,[dispatchId]=0 where [disKey]=@diskey ;";
            var key = BuildDispatchKey(taskid, runkey);
            int r = dbconn.ExecuteSql(sql, new { diskey = key });
        }

        public int CheckTaskDispatchKeyIsRunning(RLib.DB.DbConn dbconn, int taskid, string runkey)
        {
            if (string.IsNullOrWhiteSpace(runkey))
                return 0;
            string sql = "select top 1 [keyState],[dispatchId] from dispatchKeyState where [disKey]=@diskey ;";
            var key = BuildDispatchKey(taskid, runkey);
            var tb= dbconn.SqlToDataTable(sql, new { diskey = key });
            if (tb.Rows.Count == 0)
                return 0;
            var state = RLib.Utils.Converter.ObjToInt(tb.Rows[0]["keyState"]);
            var dispatchId = RLib.Utils.Converter.ObjToInt(tb.Rows[0]["dispatchId"]);
            if (state == 1)
                return dispatchId;
            return 0;
        }
    }
}
