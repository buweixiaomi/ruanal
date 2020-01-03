using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ruanal.WebDomain.Model;

namespace Ruanal.WebDomain.BLL
{
    public class TaskBll
    {
        private static DateTime _nowtime;
        private static DateTime? _lastgetnowtime;
        public static DateTime GetNowTime()
        {
            if (_lastgetnowtime == null || Math.Abs(_lastgetnowtime.Value.Subtract(DateTime.Now).TotalSeconds) > 30)
            {
                using (RLib.DB.DbConn dbconn = Pub.GetConn())
                {
                    _nowtime = dbconn.GetServerDate();
                    _lastgetnowtime = DateTime.Now;
                }
            }
            return _nowtime.Add(DateTime.Now.Subtract(_lastgetnowtime.Value));
        }
        public static bool HeartIsOk(DateTime? dt)
        {
            int seconds = 40;
            if (dt == null)
                return false;
            var nowtime = GetNowTime();
            TimeSpan ts = nowtime - dt.Value;
            if (Math.Abs(ts.TotalSeconds) > seconds)
            {
                return false;
            }
            return true;
        }
        DAL.TaskDal taskdal = new DAL.TaskDal();
        public List<Model.Task> GetAllTask()
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var tasks = taskdal.GetAllTask(dbconn, -1);
                return tasks;
            }
        }

        public List<Model.Task> GetAllTaskOfManage(int tagindex)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                DAL.NodeDal nodedal = new DAL.NodeDal();
                var tasks = taskdal.GetAllTask(dbconn, tagindex);
                foreach (var a in tasks)
                {
                    a.TaskBindings = taskdal.GetTaskBindings(dbconn, a.TaskId);
                    foreach (var b in a.TaskBindings)
                    {
                        b.Node = nodedal.Detail(dbconn, b.NodeId);
                    }
                    a.TaskBindings = a.TaskBindings.OrderByDescending(x => x.Node.NodeType).ToList();
                }
                return tasks;
            }
        }

        public int AutoEndEnd()
        {
            TimeSpan runtime = TimeSpan.FromHours(1.5);
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var disdpatchdal = new DAL.DispatchDal();
                var dis = disdpatchdal.GetRunDispatchs(dbconn, DateTime.Now.Subtract(runtime));
                int s = 0;
                foreach (var a in dis)
                {
                    var disitem = disdpatchdal.GetLastDispatchKeyItem(dbconn, a.TaskId, a.RunKey);
                    if (disitem == null)
                    {
                        continue;
                    }
                    if (disitem.DispatchState >= 2 && disitem.DispatchState <= 4)
                    {
                        disdpatchdal.EndExec(dbconn, a.DispatchId, true, "检测自动结束！");
                        try
                        {
                            CmdHelper.Instance.StopDispatch(disitem.DispatchId);
                        }
                        catch { }
                        s++;
                    }
                }
                return s;
            }
        }

        public Model.Task AddTask(Model.Task model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                throw new MException("标题不能为空!");
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                model = taskdal.Add(dbconn, model);
                return model;
            }
        }

        public Model.Task UpdateTask(Model.Task model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                throw new MException("标题不能为空!");
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                int r = taskdal.Update(dbconn, model);
                return model;
            }
        }

        public Model.Task GetDetail(int taskId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var r = taskdal.GetDetail(dbconn, taskId);
                return r;
            }
        }

        public bool DeleteTask(int taskId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var model = taskdal.GetDetail(dbconn, taskId);
                var bds = taskdal.GetTaskBindings(dbconn, taskId);
                if (bds.Count > 0 && bds.Count(x => x.LocalState == 1) > 0)
                {
                    throw new MException("任务正在运行，请先停止!");
                }
                taskdal.Delete(dbconn, taskId);
                return true;
            }
        }

        public object GetDetailWidthVersion(int taskId, int top)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var r = taskdal.GetDetail(dbconn, taskId);
                var versions = taskdal.GetTaskAllVersion(dbconn, taskId, top);
                return new Tuple<Model.Task, List<Model.TaskVersion>>(r, versions);
            }
        }

        public object GetDetailWidthBinding(int taskId)
        {
            //得到所有绑定 并确定主节点情况
            var taskandbinds = TaskBll.ToRightTaskNodes(taskId, null, false);
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodeldal = new DAL.NodeDal();
                foreach (var b in taskandbinds.Item2)
                {
                    b.Node = nodeldal.Detail(dbconn, b.NodeId);
                }
            }
            return taskandbinds;
        }

        public TaskVersion AddTaskVersion(TaskVersion model)
        {
            if (model.TaskId <= 0)
            {
                throw new MException("任务ID不能为空!");
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var versions = taskdal.AddVersion(dbconn, model);
                return versions;
            }
        }

        /// <summary>
        /// 批量上传版本变指定为当前版本
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tag"></param>
        public void BatchTaskVersion(TaskVersion model, int tag)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var tasks = taskdal.GetAllTask(dbconn, tag);
                if (tasks.Count == 0) throw new MException("没有任务！");
                dbconn.BeginTransaction();
                try
                {
                    foreach (var a in tasks)
                    {
                        model.TaskId = a.TaskId;
                        var versions = taskdal.AddVersion(dbconn, model);
                        taskdal.SetVersion(dbconn, a.TaskId, versions.VersionId);
                    }
                    dbconn.Commit();
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw;
                }
            }
        }


        public int DeleteTaskVersion(int versionid)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var versions = taskdal.DeleteVersion(dbconn, versionid);
                return versions;
            }
        }

        public int SetTaskVersion(int taskId, int versionId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var versioninfo = taskdal.GetVersionDetail(dbconn, versionId);
                if (versioninfo == null || versioninfo.TaskId != taskId)
                {
                    throw new MException("版本信息错误！");
                }
                var versions = taskdal.SetVersion(dbconn, taskId, versionId);
                return versions;
            }
        }

        public bool DeleteTaskBinding(int bindingId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                //var bindmodel = taskdal.GetTaskBinding(dbconn, bindingId);
                //if (bindmodel.LocalState != 0 || bindmodel.ServerState != 0)
                //{
                //    throw new MException("请选停止节点上的该任务！");
                //}
                var versions = taskdal.DeleteBind(dbconn, bindingId);
                return versions > 0;
            }
        }

        public List<Model.Node> GetCanBindingNodes(int taskId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodedal = new DAL.NodeDal();
                var taskmodel = taskdal.GetDetail(dbconn, taskId);
                var allnodes = nodedal.GetAllNode(dbconn, false);
                var tasknowbinds = taskdal.GetTaskBindings(dbconn, taskId);
                //当前任务已有主节点 或 主节点个数等于1时 不显示主节点
                if (allnodes.Where(x => x.NodeType == 1).ToList().Exists(x => tasknowbinds.Exists(y => y.NodeId == x.NodeId))
                     || allnodes.Count(x => x.NodeType == 1) == 1)
                {
                    allnodes.RemoveAll(x => x.NodeType == 1);
                }
                if (taskmodel.TaskType == 0)
                {
                    allnodes.RemoveAll(x => x.NodeType == 1);
                }

                allnodes.RemoveAll(x => tasknowbinds.Exists(k => k.NodeId == x.NodeId));
                return allnodes;
            }
        }

        public string GetTaskConfigSet(int taskId, string basepath)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var task = taskdal.GetDetail(dbconn, taskId);
                var taskv = taskdal.GetVersionDetail(dbconn, task.CurrVersionId);
                var fillfullname = System.IO.Path.Combine(basepath, taskv.FilePath.Replace("/", "\\").TrimStart('\\'));

                var pathun = System.IO.Path.Combine(new System.IO.FileInfo(fillfullname).DirectoryName, "_tmpUnZip");
                Utils.UnZip(fillfullname, pathun, "", true);
                AppDomain appdoman = null;
                try
                {
                    var fllname = System.IO.Path.Combine(pathun, task.EnterDll);
                    appdoman = AppDomain.CreateDomain("_tmpRunCheck", new System.Security.Policy.Evidence() { Locked = false }, new AppDomainSetup()
                    {
                        ApplicationBase = pathun
                    });
                    object obj = appdoman.CreateInstanceFromAndUnwrap(fllname, task.EnterClass);

                    object jsonvalue = null;
                    if (obj is Ruanal.Job.JobServiceBase)
                    {
                        var currinstance = obj as Ruanal.Job.JobServiceBase;
                        try
                        {
                            jsonvalue = currinstance.GetConfigJson();
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    return jsonvalue == null ? null : jsonvalue.ToString();
                }
                catch
                {
                    return null;
                }
                finally
                {
                    if (appdoman != null)
                    {
                        AppDomain.Unload(appdoman);
                    }
                    Utils.DeleteFileOrDir(pathun);
                }
            }
        }

        public bool AddTaskBinding(int taskId, string nodeIds)
        {
            var nodes = RLib.Utils.StringHelper.SplitToIntList(nodeIds ?? "", new char[] { ',', ' ' });
            if (nodes.Count == 0)
            {
                throw new MException("请选择节点！");
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                dbconn.BeginTransaction();
                try
                {
                    foreach (var n in nodes)
                    {
                        taskdal.AddBinding(dbconn, new TaskBinding()
                        {
                            NodeId = n,
                            ServerState = 0,
                            TaskId = taskId
                        });
                    }
                    dbconn.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }

        public PageModel<Model.Dispatch> GetDispatchPage(int pno, string keywords, DateTime? begintime, DateTime? endtime,
            int? taskid, int? nodeid, int? dispatchState)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var disdpatchdal = new DAL.DispatchDal();
                int totalcount = 0;
                int pagesize = 20;
                var models = disdpatchdal.GetDispatchs(dbconn, taskid, nodeid, dispatchState,
                    keywords, begintime, endtime, pno, pagesize, out totalcount);
                return new PageModel<Dispatch>()
                {
                    List = models,
                    PageNo = pno,
                    PageSize = pagesize,
                    TotalCount = totalcount
                };
            }
        }

        public int DeleteDispath(int dispatchid)
        {
            int r = 0;
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var disdpatchdal = new DAL.DispatchDal();
                r = disdpatchdal.Delete(dbconn, dispatchid);
            }
            try
            {
                CmdHelper.Instance.StopDispatch(dispatchid);
            }
            catch (Exception ex) { }
            return r;
        }

        /// <summary>
        /// 任务自动绑定或删除主节点 返回任务详情和当前所有绑定
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public static Tuple<Model.Task, List<Model.TaskBinding>> ToRightTaskNodes(int taskId, List<int> filterNodeIdds, bool checkThrow)
        {
            var taskdal = new DAL.TaskDal();
            var nodedal = new DAL.NodeDal();
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var taskmodel = taskdal.GetDetail(dbconn, taskId);
                if (taskmodel == null || taskmodel.State != 0)
                {
                    throw new MException("任务不存在或已删除！");
                }
                var taskbinds = taskdal.GetTaskBindings(dbconn, taskId);
                foreach (var a in taskbinds)
                {
                    a.Node = nodedal.Detail(dbconn, a.NodeId);
                }
                Exception throex = null;
                //添加控制节点
                if (taskmodel.TaskType == 1 && taskbinds.Count(x => x.Node.NodeType == 1) == 0)
                {
                    var mainnodes = nodedal.GetMainNode(dbconn);
                    if (mainnodes.Count == 0)
                    {
                        throex = new MException("没有控制节点，不能运行调度任务！");
                    }
                    else if (mainnodes.Count > 1)
                    {
                        throex = new MException("有多个控制节点，请手动添加控制节点！");
                    }
                    else
                    {
                        var binding = taskdal.AddBinding(dbconn, new Model.TaskBinding()
                        {
                            BindId = 0,
                            LastRunTime = null,
                            LocalState = 0,
                            ServerState = 0,
                            NodeId = mainnodes.First().NodeId,
                            RunVersion = 0,
                            TaskId = taskmodel.TaskId
                        });
                        binding.Node = mainnodes.First();
                        taskbinds.Add(binding);
                    }
                }
                if (checkThrow == true && throex != null)
                {
                    throw throex;
                }
                //删除控制节点
                if (taskmodel.TaskType == 0 && taskbinds.Count(x => x.Node.NodeType == 1) > 0)
                {
                    var mainnodes = taskbinds.Where(x => x.Node.NodeType == 1).ToList();
                    foreach (var a in mainnodes)
                    {
                        taskdal.DeleteBind(dbconn, a.BindId);
                        taskbinds.Remove(a);
                    }
                }
                if (filterNodeIdds != null && filterNodeIdds.Count > 0)
                {
                    taskbinds.RemoveAll(x => !filterNodeIdds.Contains(x.NodeId));
                }
                taskbinds = taskbinds.OrderByDescending(x => x.Node.NodeType).ToList();
                return new Tuple<Model.Task, List<Model.TaskBinding>>(taskmodel, taskbinds);
            }
        }

        public bool SetDispatchState(int taskId, int nodeId, int dispatchState)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                List<int> nodeids = new List<int>();
                if (nodeId == 0)
                {
                    nodeids.AddRange(taskdal.GetTaskBindings(dbconn, taskId).Select(x => x.NodeId));
                }
                else
                {
                    nodeids.Add(nodeId);
                }
                dbconn.BeginTransaction();
                try
                {
                    foreach (var a in nodeids)
                    {
                        taskdal.SetDispatchState(dbconn, taskId, a, dispatchState);
                    }
                    dbconn.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }

        public string ShowRuningDispatch(int dispatchid)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var disdpatchdal = new DAL.DispatchDal();
                var dismodel = disdpatchdal.GetDetail(dbconn, dispatchid);
                string msg = Ruanal.Core.ConfigConst.TalkAskDispatchRunning + string.Format("{0}#{1}", dismodel.TaskId, dismodel.RunKey);
                var result = Ruanal.Core.Notify.NotifyHelper.TalkToAll(msg, 4000, 0);
                string resulttex = string.Format("收到回复：[{0}]", string.Join(",", result));
                return resulttex;
            }
        }

        public Dictionary<string, List<string>> ViewStatus(int taskId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var counts = taskdal.GetTaskBindsNodes(dbconn, taskId, true).Count;
                string msg = Ruanal.Core.ConfigConst.TalkTaskInstanceStatus + string.Format("{0}", taskId);
                var result = Ruanal.Core.Notify.NotifyHelper.TalkToAll(msg, 4000, counts);
                Dictionary<string, List<string>> vs = new Dictionary<string, List<string>>();
                DAL.NodeDal nodedal = new DAL.NodeDal();
                foreach (var a in result)
                {
                    var _s = RLib.Utils.StringHelper.SplitToStringList(a, new char[] { '#' });

                    string clientid = _s[0].Substring(1, _s[0].Length - 2);
                    var clientnodedetail = nodedal.Detail(dbconn, clientid);
                    vs[string.Format("[{0}] {1}", clientnodedetail.NodeId, clientnodedetail.Title)] = _s.Skip(1).ToList();
                }
                return vs;
            }
        }
    }
}
