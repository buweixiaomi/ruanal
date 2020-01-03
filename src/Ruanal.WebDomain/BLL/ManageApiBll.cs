using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ruanal.WebDomain.MApiEntity;

namespace Ruanal.WebDomain.BLL
{
    public class ManageApiBll
    {
        DAL.NodeDal nodedal = new DAL.NodeDal();
        DAL.TaskDal taskdal = new DAL.TaskDal();
        DAL.TaskLogDal tasklogdal = new DAL.TaskLogDal();
        public int NodeRestart(int type, int nodeid)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodes = nodedal.GetAllNode(dbconn);
                if (nodeid > 0)
                    nodes = nodes.Where(x => x.NodeId == nodeid).ToList();
                if (type == 0)
                {
                    nodes.ForEach(node =>
                    {
                        var v = Ruanal.WebDomain.BLL.CmdHelper.Instance.RestartNode(node.NodeId);
                    });
                }
                else
                {
                    var clientids = string.Join("|", nodes.Select(x => x.ClientId));
                    Ruanal.Core.Notify.NotifyHelper.Notify(Ruanal.Core.ConfigConst.NotifyTopic_RestartNode, clientids);
                }
                return nodes.Count;
            }
        }

        public List<MApiEntity.Task> TaskList()
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                return taskdal.GetAllTask(dbconn, -1)
                      .Select(x => new MApiEntity.Task()
                      {
                          DispatchClass = x.DispatchClass,
                          EnterClass = x.EnterClass,
                          EnterDll = x.EnterDll,
                          RunCron = x.RunCron,
                          TaskBindings = taskdal.GetTaskBindings(dbconn, x.TaskId)
                              .Select(y => new MApiEntity.TaskBinding()
                              {
                                  BindId = y.BindId,
                                  TaskId = y.TaskId,
                                  LastRunTime = y.LastRunTime,
                                  LocalState = y.LocalState,
                                  NodeId = y.NodeId,
                                  ServerState = y.ServerState
                              }).ToList(),
                          Remark = x.Remark,
                          State = x.State,
                          Title = x.Title,
                          TaskId = x.TaskId,
                          TaskConfig = x.TaskConfig,
                          TaskType = x.TaskType
                      }).ToList();
            }
        }

        public List<TaskRunLog> TaskRunLog(int? taskid)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                int total = 0;
                var logs = tasklogdal.GetRunLogPage(dbconn, taskid, null, null, "", "", null, null, 1, 50, out total);
                var ml = logs.Select(x => new TaskRunLog()
                {
                    EndDbTime = x.EndDbTime,
                    EndServerTime = x.EndServerTime,
                    LogId = x.LogId,
                    LogText = x.LogText,
                    NodeId = x.NodeId,
                    ResultType = x.ResultType,
                    RunDbTime = x.RunDbTime,
                    RunGuid = x.RunGuid,
                    RunServerTime = x.RunServerTime,
                    RunType = x.RunType,
                    TaskId = x.TaskId
                }).ToList();
                return ml;
            }
        }

        public int TaskDelete(int taskid)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var taskdetail = taskdal.GetDetail(dbconn, taskid);
                if (taskdetail == null || taskdetail.State == -1)
                    throw new MException("任务不存在！");
                var taskbind = taskdal.GetTaskBindings(dbconn, taskid);
                if (taskbind.Count(x => x.LocalState == 1) > 0)
                    throw new MException("请先停止！");
                if (taskbind.Count(x => x.ServerState == 1) > 0)
                    throw new MException("请等待节点任务停止或重启节点！");
                taskdal.Delete(dbconn, taskid);
                return taskid;
            }
        }

        public int TaskAdd(Task model)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    throw new MException("标题不能为空!");
                }
                SetDefaultNodeBind(model);

                Model.Task taskmodel = new Model.Task()
                {
                    CreateTime = DateTime.Now,
                    CurrVersionId = 0,
                    DispatchClass = model.DispatchClass,
                    EnterClass = model.EnterClass,
                    EnterDll = model.EnterDll,
                    ExpireMins = 1000,
                    Remark = model.Remark,
                    RunCron = model.RunCron,
                    State = model.State,
                    TaskConfig = model.TaskConfig,
                    TaskId = model.TaskId,
                    TaskTags = 0,
                    Title = model.Title,
                    TaskType = model.TaskType,
                    UpdateTime = null,
                };
                dbconn.BeginTransaction();
                try
                {
                    taskmodel = taskdal.Add(dbconn, taskmodel);
                    foreach (var a in model.TaskBindings)
                    {
                        taskdal.AddBinding(dbconn, new Model.TaskBinding()
                        {
                            TaskId = taskmodel.TaskId,
                            NodeId = a.NodeId
                        });
                    }

                    dbconn.Commit();
                    return taskmodel.TaskId;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw;
                }
            }
        }


        public int TaskEdit(Task model)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    throw new MException("标题不能为空!");
                }
                var taskmodel = taskdal.GetDetail(dbconn, model.TaskId);
                if (taskmodel == null || taskmodel.State == -1)
                    throw new MException("任务不存在！");
                SetDefaultNodeBind(model);
                taskmodel.DispatchClass = model.DispatchClass;
                taskmodel.EnterClass = model.EnterClass;
                taskmodel.EnterDll = model.EnterDll;
                taskmodel.Remark = model.Remark;
                taskmodel.RunCron = model.RunCron;
                taskmodel.State = model.State;
                taskmodel.TaskConfig = model.TaskConfig;
                taskmodel.Title = model.Title;
                taskmodel.TaskType = model.TaskType;
                taskmodel.UpdateTime = DateTime.Now;
                taskmodel.TaskBindings = taskdal.GetTaskBindings(dbconn, model.TaskId);

                var delnodes = new List<int>();
                foreach (var t in taskmodel.TaskBindings)
                {
                    if (!model.TaskBindings.Exists(x => t.NodeId == x.NodeId))
                    {
                        if (t.LocalState == 1)
                            throw new Exception("节点" + t.Node + "本在状态为运行中，不能删除！");
                    }
                }
                taskmodel.TaskBindings.Select(x => x.NodeId).Except(model.TaskBindings.Select(x => x.NodeId)).ToList();
                var addnodes = model.TaskBindings.Select(x => x.NodeId).Except(taskmodel.TaskBindings.Select(x => x.NodeId)).ToList();

                dbconn.BeginTransaction();
                try
                {
                    taskdal.Update(dbconn, taskmodel);
                    foreach (var a in delnodes)
                        taskdal.DeleteBind(dbconn, model.TaskId, a);
                    foreach (var a in addnodes)
                        taskdal.AddBinding(dbconn, new Model.TaskBinding() { TaskId = taskmodel.TaskId, NodeId = a });
                    dbconn.Commit();
                    return taskmodel.TaskId;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw;
                }
            }
        }



        private void SetDefaultNodeBind(Task model)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                if (model.TaskBindings == null)
                    model.TaskBindings = new List<TaskBinding>();
                for (var k = model.TaskBindings.Count - 1; k >= 0; k--)
                {
                    var tnode = nodedal.Detail(dbconn, model.TaskBindings[k].NodeId);
                    if (tnode == null)
                    {
                        model.TaskBindings.RemoveAt(k);
                    }
                    model.TaskBindings[k].Node = new Node()
                    {
                        ClientId = tnode.ClientId,
                        LastHeartTime = tnode.LastHeartTime,
                        NodeConfig = tnode.NodeConfig,
                        NodeId = tnode.NodeId,
                        NodeType = tnode.NodeType,
                        Remark = tnode.Remark,
                        State = tnode.State,
                        Title = tnode.Title,
                    };
                }
                if (model.TaskType == 0)
                {
                    model.TaskBindings.RemoveAll(x => x.Node.NodeType == 1);
                    if (model.TaskBindings.Count(x => x.Node.NodeType == 0) == 0)
                    {
                        var node1 = nodedal.GetAllNode(dbconn).FirstOrDefault(x => x.NodeType == 0);
                        if (node1 != null)
                        {
                            model.TaskBindings.Add(new TaskBinding()
                            {
                                BindId = 0,
                                LastRunTime = null,
                                LocalState = 0,
                                ServerState = 0,
                                NodeId = node1.NodeId,
                                TaskId = model.TaskId,
                                Node = new Node()
                                {
                                    ClientId = node1.ClientId,
                                    LastHeartTime = node1.LastHeartTime,
                                    NodeConfig = node1.NodeConfig,
                                    NodeId = node1.NodeId,
                                    NodeType = node1.NodeType,
                                    Remark = node1.Remark,
                                    State = node1.State,
                                    Title = node1.Title,
                                }
                            });
                        }
                    }
                }
                else if (model.TaskType == 1)
                {
                    if (model.TaskBindings.Count(x => x.Node.NodeType == 0) == 0)
                    {
                        var node1 = nodedal.GetAllNode(dbconn).FirstOrDefault(x => x.NodeType == 0);
                        if (node1 != null)
                        {
                            model.TaskBindings.Add(new TaskBinding()
                            {
                                BindId = 0,
                                LastRunTime = null,
                                LocalState = 0,
                                ServerState = 0,
                                NodeId = node1.NodeId,
                                TaskId = model.TaskId,
                                Node = new Node()
                                {
                                    ClientId = node1.ClientId,
                                    LastHeartTime = node1.LastHeartTime,
                                    NodeConfig = node1.NodeConfig,
                                    NodeId = node1.NodeId,
                                    NodeType = node1.NodeType,
                                    Remark = node1.Remark,
                                    State = node1.State,
                                    Title = node1.Title,
                                }
                            });
                        }
                    }
                    if (model.TaskBindings.Count(x => x.Node.NodeType == 1) == 0)
                    {
                        var node1 = nodedal.GetAllNode(dbconn).FirstOrDefault(x => x.NodeType == 1);
                        if (node1 != null)
                        {
                            model.TaskBindings.Add(new TaskBinding()
                            {
                                BindId = 0,
                                LastRunTime = null,
                                LocalState = 0,
                                ServerState = 0,
                                NodeId = node1.NodeId,
                                TaskId = model.TaskId,
                                Node = new Node()
                                {
                                    ClientId = node1.ClientId,
                                    LastHeartTime = node1.LastHeartTime,
                                    NodeConfig = node1.NodeConfig,
                                    NodeId = node1.NodeId,
                                    NodeType = node1.NodeType,
                                    Remark = node1.Remark,
                                    State = node1.State,
                                    Title = node1.Title,
                                }
                            });
                        }
                    }
                }
            }
        }

        public List<MApiEntity.Node> NodeList()
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                return nodedal.GetAllNode(dbconn)
                      .Select(x => new MApiEntity.Node()
                      {
                          ClientId = x.ClientId,
                          LastHeartTime = x.LastHeartTime,
                          NodeConfig = x.NodeConfig,
                          NodeId = x.NodeId,
                          NodeType = x.NodeType,
                          Remark = x.Remark,
                          State = x.State,
                          Title = x.Title
                      }).ToList();
            }
        }


        public int TaskStop(int taskid)
        {
            Ruanal.WebDomain.BLL.CmdHelper.Instance.StopTask(taskid, null);
            Ruanal.WebDomain.BLL.CmdHelper.Instance.DeleteTask(taskid, null);
            return taskid;
        }

        public int TaskStart(int taskid)
        {
            Ruanal.WebDomain.BLL.CmdHelper.Instance.StartTask(taskid, null);
            return taskid;
        }


        public int TaskUnInstall(int taskid)
        {
            Ruanal.WebDomain.BLL.CmdHelper.Instance.DeleteTask(taskid, null);
            return taskid;
        }


        public int TaskSetVersion(TaskVersion model)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var task = taskdal.GetDetail(dbconn, model.TaskId);
                if (task == null) throw new MException("没有任务！");
                dbconn.BeginTransaction();
                try
                {
                    model.TaskId = task.TaskId;
                    var versions = taskdal.AddVersion(dbconn, new Model.TaskVersion()
                    {
                        CreateTime = DateTime.Now,
                        FilePath = model.FilePath ?? "",
                        FileSize = model.FileSize,
                        Remark = model.Remark ?? "",
                        TaskId = model.TaskId,
                        VersionId = 0,
                        VersionNO = DateTime.Now.ToString("yyyyMMddHHmmssfff" + "-" + model.TaskId),
                        Vstate = 0
                    });
                    taskdal.SetVersion(dbconn, task.TaskId, versions.VersionId);
                    dbconn.Commit();
                    return model.TaskId;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw;
                }
            }
        }

    }
}
