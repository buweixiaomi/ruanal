using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.DAL
{
    public class TaskDal
    {
        public Model.Task Add(RLib.DB.DbConn dbconn, Model.Task model)
        {
            string sql = @"INSERT INTO [dbo].[task]
           ([title]
           ,[taskType]
           ,[runCron]
           ,[taskConfig]
           ,[state]
           ,[taskTags]
           ,[currVersionId]
           ,[enterDll]
           ,[enterClass]
           ,[dispatchClass]
            ,[expireMins]
           ,[createTime]
        --   ,[updateTime]
           ,[remark])
     VALUES
           (@title
           ,@taskType
           ,@runCron
           ,@taskConfig
           ,@state
           ,@taskTags
           ,@currVersionId
           ,@enterDll
           ,@enterClass
          ,@dispatchClass
            ,@expireMins
           ,getdate()
          --  ,@updateTime
           ,@remark)";

            dbconn.ExecuteSql(sql, new
            {
                title = model.Title ?? "",
                taskType = model.TaskType,
                runCron = model.RunCron ?? "",
                taskConfig = model.TaskConfig ?? "",
                state = 0,// model.State,
                taskTags = model.TaskTags,
                currVersionId = 0,
                enterDll = model.EnterDll ?? "",
                enterClass = model.EnterClass ?? "",
                dispatchClass = model.DispatchClass ?? "",
                expireMins = model.ExpireMins,
                remark = model.Remark ?? ""
            });
            model.TaskId = dbconn.GetIdentity();
            return model;
        }

        public int Update(RLib.DB.DbConn dbconn, Model.Task model)
        {
            string sql = @"UPDATE [dbo].[task]
                               SET [title] =@title
                                  ,[taskType] = @taskType
                                  ,[runCron] =@runCron
                                  ,[taskConfig] = @taskConfig
                                  ,[taskTags] = @taskTags
                                  ,[enterDll] = @enterDll
                                  ,[enterClass] = @enterClass
                                  ,[dispatchClass]=@dispatchClass
                                    ,expireMins = @expireMins
                                  ,[updateTime] = getdate()
                                  ,[remark] = @remark
                             WHERE taskId=@taskId ";
            int r = dbconn.ExecuteSql(sql, new
            {
                taskId = model.TaskId,
                title = model.Title ?? "",
                taskTags = model.TaskTags,
                taskType = model.TaskType,
                runCron = model.RunCron ?? "",
                taskConfig = model.TaskConfig ?? "",
                enterDll = model.EnterDll ?? "",
                enterClass = model.EnterClass ?? "",
                dispatchClass = model.DispatchClass ?? "",
                expireMins = model.ExpireMins,
                remark = model.Remark ?? ""
            });
            return r;
        }

        public Model.Task GetDetail(RLib.DB.DbConn dbconn, int taskId)
        {
            string sql = "select * from task where taskid=@taskid ;";
            var mdoel = dbconn.Query<Model.Task>(sql, new { taskid = taskId }).FirstOrDefault();
            return mdoel;
        }

        public int Delete(RLib.DB.DbConn dbconn, int taskId)
        {
            string sql = "update task set [state]=-1 where taskid=@taskid;";
            int r = dbconn.ExecuteSql(sql, new { taskid = taskId });
            return r;
        }

        public int SetVersion(RLib.DB.DbConn dbconn, int taskId, int versionId)
        {
            string sql = "update task set currVersionId=@versionid where taskid=@taskid";
            int r = dbconn.ExecuteSql(sql, new { versionid = versionId, taskid = taskId });
            return r;
        }

        public List<Model.Task> GetAllTask(RLib.DB.DbConn dbconn, int tagindex)
        {
            string wherecon = "";
            if (tagindex > 0)
            {
                int v = (1 << (tagindex - 1));
                wherecon += " and (tasktags&" + v + ")<>0 ";
            }
            string sql = "select * from task where [state]<>-1 " + wherecon + " order by taskid asc;";
            var models = dbconn.Query<Model.Task>(sql, new { });
            return models;
        }



        #region taskVersion
        public Model.TaskVersion AddVersion(RLib.DB.DbConn dbconn, Model.TaskVersion model)
        {
            string sql = @"INSERT INTO [dbo].[taskVersion]
           ([taskId]
           ,[versionNO]
           ,[filePath]
           ,[fileSize]
           ,[vstate]
           ,[createTime]
           ,[remark])
     VALUES
           (@taskId
           ,@versionNO
           ,@filePath
           ,@fileSize
           ,@vstate
           ,getdate()
           ,@remark)";

            dbconn.ExecuteSql(sql, new
            {
                taskId = model.TaskId,
                versionNO = model.VersionNO ?? "",
                filePath = model.FilePath ?? "",
                fileSize = model.FileSize,
                vstate = 0,
                remark = model.Remark ?? ""
            });

            model.VersionId = dbconn.GetIdentity();
            return model;
        }

        public int DeleteVersion(RLib.DB.DbConn dbconn, int versionId)
        {
            string sql = "update taskVersion set vstate=-1 where versionId = @versionId";
            int r = dbconn.ExecuteSql(sql, new { versionId = versionId });
            return r;
        }

        public Model.TaskVersion GetVersionDetail(RLib.DB.DbConn dbconn, int versionId)
        {
            string sql = "select * from taskVersion where versionId=@versionId;";
            var model = dbconn.Query<Model.TaskVersion>(sql, new { versionId = versionId }).FirstOrDefault();
            return model;
        }

        public List<Model.TaskVersion> GetTaskAllVersion(RLib.DB.DbConn dbconn, int taskId, int top)
        {
            string sql = "select " + (top > 0 ? " top " + top : "") + " * from taskVersion where taskId=@taskId and vstate=0 order by versionId desc ";
            var models = dbconn.Query<Model.TaskVersion>(sql, new { taskId = taskId });
            return models;
        }

        #endregion

        #region taskBinding
        public Model.TaskBinding AddBinding(RLib.DB.DbConn dbconn, Model.TaskBinding model)
        {
            string sql = @"INSERT INTO [dbo].[taskBinding]
           ([taskId]
           ,[nodeId]
            ,[runVersion]
           ,[localState]
        --   ,[lastRunTime]
           ,[serverState])
     VALUES
           (@taskId
           ,@nodeId
            ,@runVersion
           ,@localState
      --     ,@lastRunTime
           ,@serverState)";

            dbconn.ExecuteSql(sql, new
            {
                taskId = model.TaskId,
                nodeId = model.NodeId,
                localState = 0,
                serverState = 0,
                //   lastRunTime = model.LastRunTime,
                runVersion = 0
            });

            model.BindId = dbconn.GetIdentity();
            return model;
        }


        public int DeleteBind(RLib.DB.DbConn dbconn, int bindId)
        {
            string sql = " update taskbinding set localState=-1 where bindId=@bindId; ";
            int r = dbconn.ExecuteSql(sql, new { bindId = bindId });
            return r;
        }
        public int DeleteBind(RLib.DB.DbConn dbconn, int taskid, int nodeid)
        {
            string sql = " update taskbinding set localState=-1 where taskid=@taskid and nodeid=@nodeid and localState<>-1; ";
            int r = dbconn.ExecuteSql(sql, new { taskid = taskid, nodeid = nodeid });
            return r;
        }

        public List<Model.TaskBinding> GetTaskBindings(RLib.DB.DbConn dbconn, int taskId)
        {
            string sql = "select * from taskbinding where taskId=@taskId and localState<>-1 order by nodeId asc; ";
            var models = dbconn.Query<Model.TaskBinding>(sql, new { taskId = taskId });
            return models;
        }

        public Model.TaskBinding GetTaskBinding(RLib.DB.DbConn dbconn, int bindId)
        {
            string sql = "select * from taskbinding where bindId=@bindId ";
            var models = dbconn.Query<Model.TaskBinding>(sql, new { bindId = bindId });
            return models.FirstOrDefault();
        }
        public List<Model.Task> GetNodeTasks(RLib.DB.DbConn dbconn, int nodeId, bool justRunning)
        {
            string wherecon = " where tb.nodeId=@nodeId ";
            if (justRunning)
            {
                wherecon += " and tb.localstate=1 ";
            }
            string sql = "select t.* from task t join taskbinding tb on t.taskId=tb.taskId and tb.localState<>-1 " +
                wherecon +
                "  order by t.taskId asc ";
            var models = dbconn.Query<Model.Task>(sql, new { nodeId = nodeId });
            return models;
        }

        public List<Model.TaskBinding> NodeTaskSum(RLib.DB.DbConn dbconn, int nodeId)
        {
            string sql = "select * from taskbinding where nodeId=@nodeId and localstate<>-1;";
            var models = dbconn.Query<Model.TaskBinding>(sql, new { nodeId = nodeId });
            return models;
        }

        public int UpdateBindLocalState(RLib.DB.DbConn dbconn, int bindid, int localstate)
        {
            string sql = " update taskbinding set localState=@localstate where bindid=@bindid;";
            int r = dbconn.ExecuteSql(sql, new { bindid = bindid, localstate = localstate });
            return r;
        }

        public int UpdateTaskLastRunTime(RLib.DB.DbConn dbconn, int nodeId, int taskId)
        {
            string sql = " update taskbinding set lastruntime=getdate() where taskid=@taskid and nodeid=@nodeid;";
            int r = dbconn.ExecuteSql(sql, new { taskid = taskId, nodeid = nodeId });
            return r;
        }

        public List<Model.Node> GetTaskBindsNodes(RLib.DB.DbConn dbconn, int taskid, bool jsworknode)
        {
            string sql = "   select n.* from node as n join taskbinding tb on n.nodeId=tb.nodeId and tb.localState<>-1   where tb.taskId=@taskid ";
            if (jsworknode)
            {
                sql += " and n.nodeType=0 ";
            }
            var r = dbconn.Query<Model.Node>(sql, new { taskid = taskid });
            return r;
        }

        public int UpdateNodeServerState(RLib.DB.DbConn dbconn, List<Model.TaskBinding> newstate)
        {
            if (newstate == null || newstate.Count == 0)
                return 0;
            string sql = " update taskbinding set serverState={0},runVersion={1} where bindid={2};";
            List<string> sqls = new List<string>();
            for (var i = 0; i < newstate.Count; i++)
            {
                sqls.Add(string.Format(sql, newstate[i].ServerState,
                    newstate[i].RunVersion,
                    newstate[i].BindId));
            }
            int r = dbconn.ExecuteSql(string.Join(" \r\n ", sqls));
            return r;
        }

        public int SetDispatchState(RLib.DB.DbConn dbconn, int taskId, int nodeId, int stopDispathState)
        {
            string sql = "update  [dbo].[taskbinding] set stopDispatch=@stopdispatch WHERE taskId=@taskId and nodeId=@nodeId;";
            int r = dbconn.ExecuteSql(sql, new { taskId = taskId, nodeId = nodeId, stopdispatch = stopDispathState });
            return r;
        }
        #endregion

    }
}
