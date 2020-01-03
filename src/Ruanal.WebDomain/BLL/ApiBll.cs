using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.BLL
{
    public class ApiBll
    {
        public static ApiBll Instance = new ApiBll();

        static Dictionary<int, object> taskexeclock = new Dictionary<int, object>();
        static object taskexeclock_diclock = new object();

        DAL.NodeDal nodedal = new DAL.NodeDal();
        DAL.TaskDal taskdal = new DAL.TaskDal();
        DAL.CmdDal cmddal = new DAL.CmdDal();
        DAL.DispatchDal dispatchdal = new DAL.DispatchDal();
        DAL.TaskLogDal tasklogdal = new DAL.TaskLogDal();
        static object dispatchlocker = new object();
        static Dictionary<int, object> taskdisclock = new Dictionary<int, object>();
        private ApiBll() { }

        public void CheckAuth(string clientId, bool isMaster)
        {
            int nodeId = 0;
            CheckAuth(clientId, isMaster, out nodeId);
        }

        public void CheckAuth(string clientId, bool isMaster, out int nodeId)
        {
            nodeId = 0;
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, clientId);
                if (nodemodel == null)
                {
                    throw new MException(MExceptionCode.NoPermission, "无权限！");
                }
                if (nodemodel.NodeType == -1)
                {
                    throw new MException(MExceptionCode.NoPermission, "无权限！");
                }
                if (isMaster && nodemodel.NodeType != 1)
                {
                    throw new MException(MExceptionCode.NoPermission, "无权限！");
                }
                nodeId = nodemodel.NodeId;
            }
        }
        public JsonEntity PingWorkNode(string clientId, List<int[]> taskState)
        {
            return _PingCheck(false, clientId, taskState);
        }


        public JsonEntity PingMasterNode(string clientId, List<int[]> taskState)
        {
            return _PingCheck(true, clientId, taskState);
        }

        private JsonEntity _PingCheck(bool isMaster, string clientId, List<int[]> taskState)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, clientId);
                #region 节点绑定及心跳
                if (nodemodel == null)
                {
                    return new JsonEntity() { code = -10001, msg = "Ping成功，但节点未绑定！" };
                }
                if (isMaster)
                {
                    if (nodemodel.NodeType == 0)
                    {
                        return new JsonEntity() { code = -10001, msg = "Ping成功，但节点类型错误！" };
                    }
                    if (nodemodel.NodeType == -1)
                    {
                        nodedal.SetNodeType(dbconn, nodemodel.NodeId, 1);
                    }
                }
                else
                {
                    if (nodemodel.NodeType == 1)
                    {
                        return new JsonEntity() { code = -10001, msg = "Ping成功，但节点类型错误！" };
                    }
                    if (nodemodel.NodeType == -1)
                    {
                        nodedal.SetNodeType(dbconn, nodemodel.NodeId, 0);
                    }
                }
                nodedal.UpdateLastHeart(dbconn, nodemodel.NodeId);
                #endregion

                #region 任务状态和版本
                var nodetasks = taskdal.NodeTaskSum(dbconn, nodemodel.NodeId);
                List<Model.TaskBinding> needupdate = new List<Model.TaskBinding>();
                foreach (var a in nodetasks)
                {
                    int ns = 0, nv = a.RunVersion;
                    var nsitem = taskState.FirstOrDefault(x => x[0] == a.TaskId);
                    if (nsitem != null)
                    {
                        ns = 1;
                        nv = nsitem[1];
                    }
                    if (a.ServerState != ns || a.RunVersion != nv)
                    {
                        a.ServerState = ns;
                        a.RunVersion = nv;
                        needupdate.Add(a);
                    }
                }
                taskdal.UpdateNodeServerState(dbconn, needupdate);
                #endregion

                int newcmdcount = cmddal.HasNewCmd(dbconn, nodemodel.NodeId);
                return new JsonEntity() { code = 1, data = new Ruanal.Core.ApiSdk.PingResult() { HasCmd = newcmdcount } };
            }
        }

        public JsonEntity AutoEndDispatchExecute(string ClientId, int dispatchId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                dbconn.BeginTransaction();
                try
                {
                    var dispatchmodel = dispatchdal.GetDetail(dbconn, dispatchId);
                    if (dispatchmodel == null || dispatchmodel.DispatchState == -1)
                    {
                        throw new MException("调度不存在！");
                    }
                    if (dispatchmodel.DispatchState != 2)
                    {
                        throw new MException("调度不是执行中状态！");
                    }
                    dispatchdal.EndExec(dbconn, dispatchmodel.DispatchId, false, "自动结束执行" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                    //System.Diagnostics.Trace.WriteLine(string.Format("{0} 自动结束执行 {1}", dispatchmodel.DispatchId, DateTime.Now.ToString("HH:mm:ss.fff")));
                    dbconn.Commit();
                    return new JsonEntity() { code = 1 };
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }

        public JsonEntity GetDispatchWork(string clientId, List<int[]> freeState)
        {
            List<Model.Dispatch> works = new List<Model.Dispatch>();
            if (freeState == null)
                freeState = new List<int[]>();
            List<Model.TaskBinding> nodeTasks;
            Model.Node nodemodel;
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                nodemodel = nodedal.Detail(dbconn, clientId);
                nodeTasks = taskdal.NodeTaskSum(dbconn, nodemodel.NodeId);
            }
            foreach (var a in freeState)
            {
                if (a != null && a.Length == 2 && a[1] > 0)
                {
                    //节点上运行此任务 且 运行状态为 运行
                    var taskbinditem = nodeTasks.FirstOrDefault(x => x.TaskId == a[0] && x.LocalState == 1);
                    if (taskbinditem == null)
                    {
                        continue;
                    }
                    var items = DispatchNodeTasks(a[0], nodemodel.NodeId, a[1], nodemodel.StopDispatch, taskbinditem.StopDispatch);
                    if (items != null)
                        works.AddRange(items);
                }
            }
            return new JsonEntity() { code = 1, data = works };

        }

        private List<Model.Dispatch> DispatchNodeTasks(int taskid, int nodeId, int count, int nodeStopDispatch, int bindStopDispath)
        {
            if (count <= 0)
                return new List<Model.Dispatch>();
            object tasklocker = null;
            if (taskdisclock.ContainsKey(taskid))
            {
                tasklocker = taskdisclock[taskid];
            }
            else
            {
                lock (dispatchlocker)
                {
                    if (!taskdisclock.ContainsKey(taskid))
                    {
                        taskdisclock[taskid] = new object();
                    }
                    tasklocker = taskdisclock[taskid];
                }
            }

            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var disitems = dispatchdal.GetWatchExecDispatchs(dbconn, nodeId, taskid, count, true);
                //节点或节点任务停止分配 或 当前已满足
                if (disitems.Count >= count || nodeStopDispatch == 1 || bindStopDispath == 1)
                    return disitems;
                count -= disitems.Count;
                int todispatchCount = Math.Max(1, count);
                var dispaths = dispatchdal.GetUnDispatchs(dbconn, taskid, todispatchCount, true);
                if (dispaths.Count == 0)
                    return disitems;
                //分任务单路分配
                lock (tasklocker)
                {
                    dbconn.BeginTransaction();
                    try
                    {
                        dispaths = dispatchdal.GetUnDispatchs(dbconn, taskid, todispatchCount, false);

                        if (dispaths.Count > 0)
                        {
                            foreach (var a in dispaths)
                            {
                                a.NodeId = nodeId;
                                dispatchdal.SetDispatch(dbconn, a.DispatchId, nodeId);
                            }
                            dbconn.Commit();
                            disitems.AddRange(dispaths);
                        }
                        return disitems;
                    }
                    catch (Exception ex)
                    {
                        dbconn.Rollback();
                        throw ex;
                    }
                }
            }
        }

        public JsonEntity BuildDispatchs(string clientId, int taskId, string groupId, List<Ruanal.Job.DispatcherItem> configs)
        {

            string noRunKeyRegName = "_NoRunKeyReg";
            string onlyRunKeyRegName = "_OnlyRunKeyReg";
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodedetail = nodedal.Detail(dbconn, clientId);

                var taskmodel = taskdal.GetDetail(dbconn, taskId);
                if (taskmodel == null || taskmodel.State == -1)
                {
                    return new JsonEntity() { code = -1, msg = "任务不存在！" };
                }

                List<string> norunkey = new List<string>();
                List<string> onlyrunkey1 = new List<string>();
                List<string> onlyrunkey2 = new List<string>();
                bool isnorunkey = false;
                #region 得到 排除和仅运行配置
                nodedetail.NodeConfig = (nodedetail.NodeConfig ?? "").Trim();
                taskmodel.TaskConfig = (taskmodel.TaskConfig ?? "").Trim();
                if (nodedetail.NodeConfig.StartsWith("{") && nodedetail.NodeConfig.EndsWith("}"))
                {
                    var configjobj = Newtonsoft.Json.Linq.JObject.Parse(nodedetail.NodeConfig);
                    if (configjobj[noRunKeyRegName] != null)
                    {
                        norunkey.AddRange(configjobj[noRunKeyRegName].ToString().Replace("||", "■").Split(new char[] { '■' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    if (configjobj[onlyRunKeyRegName] != null)
                    {
                        onlyrunkey1.AddRange(configjobj[onlyRunKeyRegName].ToString().Replace("||", "■").Split(new char[] { '■' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                }

                if (taskmodel.TaskConfig.StartsWith("{") && taskmodel.TaskConfig.EndsWith("}"))
                {
                    var configjobj = Newtonsoft.Json.Linq.JObject.Parse(taskmodel.TaskConfig);
                    if (configjobj[noRunKeyRegName] != null)
                    {
                        //排除的key添加到列表
                        norunkey.AddRange(configjobj[noRunKeyRegName].ToString().Replace("||", "■").Split(new char[] { '■' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    if (configjobj[onlyRunKeyRegName] != null)
                    {
                        //var taskonlyrunkey = configjobj[onlyRunKeyRegName].ToString().Replace("||", "■").Split(new char[] { '■' }, StringSplitOptions.RemoveEmptyEntries);
                        onlyrunkey2.AddRange(configjobj[onlyRunKeyRegName].ToString().Replace("||", "■").Split(new char[] { '■' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                }
                norunkey = norunkey.Distinct().ToList();
                onlyrunkey1 = onlyrunkey1.Distinct().ToList();
                onlyrunkey2 = onlyrunkey2.Distinct().ToList();
                if (isnorunkey == true)
                {
                    return new JsonEntity() { code = 1, data = groupId };
                }
                #endregion
                //排除
                if (norunkey.Count() > 0)
                {
                    foreach (var key in norunkey)
                    {
                        System.Text.RegularExpressions.Regex regx = new System.Text.RegularExpressions.Regex(key);
                        configs.RemoveAll((x) =>
                        {
                            return regx.IsMatch(x.RunKey);
                        });
                    }
                }
                //仅包含
                if (onlyrunkey1.Count() > 0 || onlyrunkey2.Count > 0)
                {
                    List<System.Text.RegularExpressions.Regex> regs1 = onlyrunkey1.Select(x => new System.Text.RegularExpressions.Regex(x)).ToList();
                    List<System.Text.RegularExpressions.Regex> regs2 = onlyrunkey2.Select(x => new System.Text.RegularExpressions.Regex(x)).ToList();

                    configs = configs.Where(x =>
                          (regs1.Count == 0 || regs1.Exists(y => y.IsMatch(x.RunKey)))//匹配 仅节点包含
                          &&
                          (regs2.Count == 0 || regs2.Exists(y => y.IsMatch(x.RunKey)))//且匹配 仅任务包含
                          ).ToList();
                }

                var nodetasks = taskdal.NodeTaskSum(dbconn, nodedetail.NodeId);
                var nodetask = nodetasks.FirstOrDefault(x => x.TaskId == taskId);
                if (nodetask == null || nodetask.LocalState != 1)
                {
                    return new JsonEntity() { code = -1, msg = "任务不存在！" };
                }
                if (nodedetail.StopDispatch == 1 || nodetask.StopDispatch == 1)
                {
                    return new JsonEntity() { code = -1, msg = "任务分配调度已停止，" + (nodedetail.StopDispatch == 1 ? "节点停止调度" : "绑定停止调度") };
                }
                dbconn.BeginTransaction();
                try
                {
                    if (string.IsNullOrEmpty(groupId))
                    {
                        return new JsonEntity() { code = -1, msg = "没有分组ID！" };
                    }
                    int rcount = dispatchdal.ExistGroupId(dbconn, groupId);
                    if (rcount > 0)
                    {
                        return new JsonEntity() { code = -1, msg = "分组ID已存在！" };
                    }
                    //var tbinds = taskdal.GetTaskBindsNodes(dbconn, taskId, true);
                    //List<string> clientIds = tbinds.Select(x => x.ClientId).ToList();
                    List<Model.Dispatch> dises = new List<Model.Dispatch>();
                    DateTime st = dbconn.GetServerDate();
                    DateTime expiretime = st;
                    if (taskmodel.ExpireMins <= 0)
                        expiretime = st.AddMinutes(Ruanal.Core.ConfigConst.DispatchDefaultExpireMins);
                    else
                        expiretime = st.AddMinutes((double)taskmodel.ExpireMins);
                    foreach (var a in configs)
                    {
                        dises.Add(new Model.Dispatch()
                        {
                            DispatchState = 0,
                            GroupId = groupId,
                            NickName = a.NickName,
                            RunKey = a.RunKey,
                            RunArgs = a.RunArgs,
                            TaskId = taskId,
                            InvokeId = Guid.NewGuid().ToString().Replace("-", ""),
                            ExpireTime = expiretime
                        });
                    }
                    dispatchdal.AddBatch(dbconn, dises);
                    dbconn.Commit();
                    CmdHelper.Instance.NotifyNewDispatch(taskId.ToString());
                    return new JsonEntity() { code = 1, data = groupId };
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }


            }
        }

        public JsonEntity CheckDispatchGroup(string groupId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    return new JsonEntity() { code = -1, msg = "没有分组ID！" };
                }
                int count = dispatchdal.GroupWaitDispatchCount(dbconn, groupId);
                return new JsonEntity() { code = 1, data = count };
            }
        }

        public JsonEntity TaskDetail(int taskId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var taskmodel = taskdal.GetDetail(dbconn, taskId);
                if (taskmodel == null || taskmodel.State == -1)
                {
                    return new JsonEntity() { code = -1, msg = "任务不存在！" };
                }
                var currversion = taskdal.GetVersionDetail(dbconn, taskmodel.CurrVersionId);
                var rmodel = LocalTaskToReponse(taskmodel, currversion);
                var bs = taskdal.GetTaskBindsNodes(dbconn, taskmodel.TaskId, true);
                rmodel.NodeCount = bs.Count;
                return new JsonEntity() { code = 1, data = rmodel };
            }
        }

        public JsonEntity NodeTasksJustRunning(string clientId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, clientId);
                var tasks = taskdal.GetNodeTasks(dbconn, nodemodel.NodeId, true);
                List<Ruanal.Core.ApiSdk.TaskDetail> rtasks = new List<Core.ApiSdk.TaskDetail>();
                foreach (var taskmodel in tasks)
                {
                    var bs = taskdal.GetTaskBindsNodes(dbconn, taskmodel.TaskId, true);
                    var currversion = taskdal.GetVersionDetail(dbconn, taskmodel.CurrVersionId);
                    var rmodel = LocalTaskToReponse(taskmodel, currversion);
                    rmodel.NodeCount = bs.Count;
                    rtasks.Add(rmodel);
                }
                return new JsonEntity() { code = 1, data = rtasks };
            }
        }

        private Ruanal.Core.ApiSdk.TaskDetail LocalTaskToReponse(
            Model.Task taskmodel, Model.TaskVersion currversion)
        {
            var rmodel = new Ruanal.Core.ApiSdk.TaskDetail()
            {
                CurrVersionId = taskmodel.CurrVersionId,
                DispatchClass = taskmodel.DispatchClass,
                EnterClass = taskmodel.EnterClass,
                EnterDll = taskmodel.EnterDll,
                RunCron = taskmodel.RunCron,
                TaskConfig = taskmodel.TaskConfig,
                TaskId = taskmodel.TaskId,
                TaskType = taskmodel.TaskType,
                Title = taskmodel.Title,
                FileUrl = currversion.FilePath.Replace("\\", "/")
            };
            return rmodel;
        }

        public JsonEntity NodeConfig(string clientId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, clientId);
                Dictionary<string, string> nodecofig = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(nodemodel.NodeConfig))
                {
                    nodecofig = RLib.Utils.DataSerialize.DeserializeObject<Dictionary<string, string>>(nodemodel.NodeConfig);
                }
                return new JsonEntity() { code = 1, data = nodecofig };
            }
        }

        public JsonEntity GetNewCmds(string ClientId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, ClientId);
                int totalcount = 0;
                var cmds = cmddal.GetNodeNewCmd(dbconn,
                    nodemodel.NodeId,
                    Ruanal.Core.ConfigConst.CMD_Page_Size,
                    out totalcount);

                List<Ruanal.Core.ApiSdk.CmdDetail> cmddetails = new List<Core.ApiSdk.CmdDetail>();
                foreach (var a in cmds)
                {
                    cmddetails.Add(new Core.ApiSdk.CmdDetail()
                    {
                        CmdId = a.CmdId,
                        NodeId = a.NodeId,
                        CmdType = a.CmdType,
                        CmdArgs = a.CmdArgs
                    });
                }
                return new JsonEntity() { code = 1, data = cmddetails };
            }
        }

        public JsonEntity CmdBeginExecute(string ClientId, int cmdid)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, ClientId);
                dbconn.BeginTransaction();
                try
                {
                    var cmddetail = cmddal.Detail(dbconn, cmdid);
                    if (cmddetail == null || cmddetail.CmdState == -1)
                    {
                        throw new MException("命令不存在！");
                    }
                    if (cmddetail.CmdState != 0)
                    {
                        throw new MException("命令不是待执行状态！");
                    }
                    if (cmddetail.NodeId != nodemodel.NodeId)
                    {
                        throw new MException("无权限操作该命令！");
                    }
                    int r = cmddal.CallCmd(dbconn, cmdid);
                    if (r == 0)
                    {
                        throw new MException("命令不是待执行状态！");
                    }
                    dbconn.Commit();
                    return new JsonEntity() { code = 1 };
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }


        public JsonEntity CmdEndExecute(string ClientId, int cmdid, bool success, string msg)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, ClientId);
                dbconn.BeginTransaction();
                try
                {
                    var cmddetail = cmddal.Detail(dbconn, cmdid);
                    if (cmddetail == null || cmddetail.CmdState == -1)
                    {
                        throw new MException("命令不存在！");
                    }
                    if (cmddetail.CmdState != 1)
                    {
                        throw new MException("命令不是执行中状态！");
                    }
                    if (cmddetail.NodeId != nodemodel.NodeId)
                    {
                        throw new MException("无权限操作该命令！");
                    }
                    cmddal.EndCmd(dbconn, cmdid, success, msg);
                    dbconn.Commit();
                    return new JsonEntity() { code = 1 };
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }

        public JsonEntity BeginDispatchExecute(string ClientId, int dispatchId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, ClientId);
                var dispatchmodel = dispatchdal.GetDetail(dbconn, dispatchId);
                if (dispatchmodel == null || dispatchmodel.DispatchState == -1)
                {
                    throw new MException("调度不存在！");
                }
                if (dispatchmodel.DispatchState != 1)
                {
                    throw new MException("调度不是待执行状态！");
                }

                try
                {
                    int runingdispatchid = 0;
                    //检查是否需要 运行状态检查（Task同一个RunArgs 只有一个实现在运行，其它跳过）
                    if (!string.IsNullOrWhiteSpace(dispatchmodel.RunArgs) && !string.IsNullOrWhiteSpace(dispatchmodel.RunKey))
                    {
                        //二次检查
                        runingdispatchid = dispatchdal.CheckTaskDispatchKeyIsRunning(dbconn, dispatchmodel.TaskId, dispatchmodel.RunKey);
                    }

                    int r = dispatchdal.SetExec(dbconn, dispatchmodel.DispatchId);
                    if (r == 0)
                    {
                        throw new MException("调度不是待执行状态！");
                    }
                    dispatchdal.SetTaskDispatchKeyRunning(dbconn, dispatchmodel.TaskId, dispatchmodel.RunKey, dispatchmodel.DispatchId);
                    int waitrunseconds = 5;
                    if (runingdispatchid > 0)
                    {
                        var runmodel = dispatchdal.GetDetail(dbconn, runingdispatchid);
                        if (runmodel != null)
                        {
                            var runseconds = DateTime.Now.Subtract(runmodel.ExecuteTime.Value).TotalSeconds;
                            runseconds = Math.Abs(runseconds);
                            if (runseconds < waitrunseconds)
                            {
                                //waitrunseconds - runseconds
                                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(waitrunseconds));
                            }
                        }
                    }

                    return new JsonEntity() { code = 1, data = runingdispatchid };
                }
                catch (Exception ex)
                {
                    return new JsonEntity() { code = -1, msg = ex.Message };
                }
            }

        }


        public JsonEntity EndDispatchExecute(string ClientId, int dispatchId, bool success, string msg)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, ClientId);
                dbconn.BeginTransaction();
                try
                {
                    var dispatchmodel = dispatchdal.GetDetail(dbconn, dispatchId);
                    if (dispatchmodel == null || dispatchmodel.DispatchState == -1)
                    {
                        throw new MException("调度不存在！");
                    }
                    if (success)
                    {
                        dispatchdal.SuccessEndExec(dbconn, dispatchmodel.DispatchId);
                    }
                    else
                    {
                        if (dispatchmodel.DispatchState != 2)
                        {
                            throw new MException("调度不是执行中状态！");
                        }
                        dispatchdal.EndExec(dbconn, dispatchmodel.DispatchId, success, msg);
                    }
                    var runningdispatchid = dispatchdal.CheckTaskDispatchKeyIsRunning(dbconn, dispatchmodel.TaskId, dispatchmodel.RunKey);
                    var canupdateStop = runningdispatchid == dispatchmodel.DispatchId;
                    if (!canupdateStop)
                    {
                        bool isnormal = Math.Abs(runningdispatchid - dispatchmodel.DispatchId) < (int.MaxValue / 2);
                        canupdateStop = isnormal ? dispatchmodel.DispatchId <= runningdispatchid :
                            dispatchmodel.DispatchId >= runningdispatchid;
                    }
                    if (canupdateStop)
                    {
                        dispatchdal.SetTaskDispatchKeyStopped(dbconn, dispatchmodel.TaskId, dispatchmodel.RunKey);
                    }
                    else
                    {
                        // System.Diagnostics.Trace.WriteLine(string.Format("{0} 不能设置未运行 {1}", dispatchmodel.DispatchId, DateTime.Now.ToString("HH:mm:ss.fff")));
                    }
                    dbconn.Commit();
                    return new JsonEntity() { code = 1 };
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }

        public JsonEntity SkipDispatchExecute(string ClientId, int dispatchId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, ClientId);
                dbconn.BeginTransaction();
                try
                {
                    var dispatchmodel = dispatchdal.GetDetail(dbconn, dispatchId);
                    if (dispatchmodel == null || dispatchmodel.DispatchState == -1)
                    {
                        throw new MException("调度不存在！");
                    }
                    if (dispatchmodel.DispatchState != 2)
                    {
                        throw new MException("调度不是执行中状态！");
                    }
                    dispatchdal.SkipExec(dbconn, dispatchmodel.DispatchId);
                    dbconn.Commit();
                    return new JsonEntity() { code = 1 };
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }


        public JsonEntity TaskBeginLog(string ClientId, int taskId, string runGuid, int runType, DateTime time)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, ClientId);
                Model.TaskRunLog taskrunlogmodel = new Model.TaskRunLog()
                {
                    NodeId = nodemodel.NodeId,
                    ResultType = 0,
                    TaskId = taskId,
                    RunType = runType,
                    RunServerTime = time,
                    RunGuid = runGuid ?? ""
                };
                tasklogdal.StartRunLog(dbconn, taskrunlogmodel);
                taskdal.UpdateTaskLastRunTime(dbconn, nodemodel.NodeId, taskId);
                return new JsonEntity() { code = 1 };
            }
        }

        public JsonEntity TaskEndLog(string ClientId, int taskId, string runGuid, int resultType, string logText, DateTime time)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, ClientId);
                Model.TaskRunLog taskrunlogmodel = new Model.TaskRunLog()
                {
                    NodeId = nodemodel.NodeId,
                    TaskId = taskId,
                    RunGuid = runGuid ?? "",
                    ResultType = resultType,
                    EndServerTime = time,
                    LogText = logText ?? ""
                };
                tasklogdal.EndRunLog(dbconn, taskrunlogmodel);
                return new JsonEntity() { code = 1 };
            }
        }


        public JsonEntity AddWorkLog(string ClientId, int taskId, string dispatchId, int logType, string logText, DateTime createTime)
        {
            if (Pub.PauseWorkLog)
                return new JsonEntity() { code = 1 };
            var sw = System.Diagnostics.Stopwatch.StartNew();
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, ClientId);
                Model.TaskWorkLog worklogmodel = new Model.TaskWorkLog()
                {
                    NodeId = nodemodel.NodeId,
                    TaskId = taskId,
                    DispatchId = dispatchId ?? "",
                    LogType = logType,
                    LogText = logText,
                    ServerTime = createTime
                };
                tasklogdal.AddWorkLog(dbconn, createTime, worklogmodel);
            }
            sw.Stop();
            System.Diagnostics.Trace.WriteLine("服务器写日志用时：" + sw.Elapsed.TotalMilliseconds.ToString("0.0"));
            return new JsonEntity() { code = 1 };
        }
        public JsonEntity AddWorkLogs(int nodeId, List<Ruanal.Core.ApiSdk.WorkLogEntity> logs)
        {
            if (Pub.PauseWorkLog)
                return new JsonEntity() { code = 1 };
            var sw = System.Diagnostics.Stopwatch.StartNew();
            List<Model.TaskWorkLog> locallog = new List<Model.TaskWorkLog>();
            foreach (var a in logs)
            {
                Model.TaskWorkLog worklogmodel = new Model.TaskWorkLog()
                {
                    NodeId = nodeId,
                    TaskId = a.TaskId,
                    DispatchId = a.DispatchId ?? "",
                    LogType = a.LogType,
                    LogText = a.LogText,
                    ServerTime = DateTime.Parse(a.CreateTime)
                };
                locallog.Add(worklogmodel);
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                foreach (var a in locallog.GroupBy(x => x.ServerTime.ToString("yyyy-MM-dd")))
                {
                    tasklogdal.AddWorkLogs(dbconn, Convert.ToDateTime(a.Key), a.ToList());
                }
            }
            sw.Stop();
            System.Diagnostics.Trace.WriteLine("服务器写日志用时：" + sw.Elapsed.TotalMilliseconds.ToString("0.0"));
            return new JsonEntity() { code = 1 };
        }
    }
}
