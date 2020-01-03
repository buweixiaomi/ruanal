using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.DAL
{
    public class CmdDal
    {
        public Model.Cmd AddCmd(RLib.DB.DbConn dbconn, Model.Cmd model)
        {
            string sql = @"INSERT INTO [dbo].[cmd]
           ([nodeId]
           ,[cmdType]
           ,[cmdArgs]
           ,[createTime]
           ,[cmdState]
      --     ,[callTime]
      --     ,[endTime]
      --     ,[resultText]
        )
     VALUES
           (@nodeid
           ,@cmdtype
           ,@cmdargs
           ,getdate()
           ,@cmdstate
     --      ,@callTime
     --      ,@endTime
      --     ,@resultText
      )";

            dbconn.ExecuteSql(sql, new
            {
                nodeid = model.NodeId,
                cmdtype = model.CmdType ?? "",
                cmdargs = model.CmdArgs ?? "",
                cmdstate = 0
            });
            model.CmdId = dbconn.GetIdentity();
            return model;
        }


        public int CallCmd(RLib.DB.DbConn dbconn, int cmdid)
        {
            string sql = "update cmd set cmdState=1 ,callTime=getdate() where cmdid=@cmdid and cmdState=0 ";
            int r = dbconn.ExecuteSql(sql, new { cmdid = cmdid });
            return r;
        }

        public int EndCmd(RLib.DB.DbConn dbconn, int cmdid, bool success, string msg)
        {
            string sql = "update cmd set cmdState=@cmdstate ,endTime=getdate(),resultText = @resulttext where cmdid=@cmdid and cmdState=1 ;";
            int r = dbconn.ExecuteSql(sql, new { cmdid = cmdid, cmdstate = success ? 2 : 3, resulttext = msg ?? "" });
            return r;
        }

        public Model.Cmd Detail(RLib.DB.DbConn dbconn, int cmdid)
        {
            string sql = "select * from cmd where cmdid=@cmdid;";
            var model = dbconn.Query<Model.Cmd>(sql, new { cmdid = cmdid }).FirstOrDefault();
            return model;
        }

        public int DeleteCmd(RLib.DB.DbConn dbconn, int cmdid)
        {
            string sql = "update cmd set cmdState=-1 where cmdid=@cmdid";
            int r = dbconn.ExecuteSql(sql, new { cmdid = cmdid });
            return r;
        }

        public List<Model.Cmd> GetCmdList(RLib.DB.DbConn dbconn, DateTime? begintime, DateTime? endtime, int? nodeid, int? cmdstate, int pno, int pagesize, out int totalcount)
        {
            string sqltp = "select {0} from cmd with(nolock) {1} {2}";
            string fields = "row_number() over(order by createTime desc) as rownum,*";
            string wherecon = " where 1=1 ";
            if (nodeid != null && nodeid > 0)
            {
                wherecon += " and nodeid=@nodeid ";
            }
            if (cmdstate != null && cmdstate >= 0)
            {
                wherecon += " and cmdState=@cmdstate ";
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
                cmdstate = cmdstate ?? 0,
                begintime = begintime,
                endtime = endtime,
                nodeid = nodeid,
                startindex = (pno - 1) * pagesize + 1,
                pagesize = pagesize
            };
            string countsql = string.Format(sqltp, "count(1)", wherecon, "");
            string querysql = string.Format("select * from ( {0} ) A where  A.rownum>=@startindex and A.rownum<@startindex+@pagesize ",
                string.Format(sqltp, fields, wherecon, ""));
            totalcount = dbconn.ExecuteScalar<int>(countsql, paras);
            var models = dbconn.Query<Model.Cmd>(querysql, paras);
            return models;
        }

        public int HasNewCmd(RLib.DB.DbConn dbconn, int nodeId)
        {
            string sql = "select count(1) from cmd where nodeid=@nodeid and cmdstate=0 ";
            int count = dbconn.ExecuteScalar<int>(sql, new { nodeid = nodeId });
            return count;
        }

        public List<Model.Cmd> GetNodeNewCmd(RLib.DB.DbConn dbconn, int nodeid, int topcount, out int totalcount)
        {
            string sqltp = "select {0} from cmd {1} {2}";
            string fields = "row_number() over(order by createTime asc) as rownum,*";
            string wherecon = " where cmdstate=0 and nodeid=@nodeid ";
            var paras = new
            {
                nodeid = nodeid,
                startindex = 1,
                pagesize = topcount
            };
            string countsql = string.Format(sqltp, "count(1)", wherecon, "");
            string querysql = string.Format("select * from ( {0} ) A where  A.rownum>=@startindex and A.rownum<@startindex+@pagesize ",
                string.Format(sqltp, fields, wherecon, ""));
            totalcount = dbconn.ExecuteScalar<int>(countsql, paras);
            var models = dbconn.Query<Model.Cmd>(querysql, paras);
            return models;
        }

    }
}
