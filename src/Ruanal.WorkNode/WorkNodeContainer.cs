using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WorkNode
{
    public class WorkNodeContainer : ServiceSchedule.ServiceContainer
    {
        string _ListenAddress;
        int _ListenPort;
        int _PingSeconds;
        System.Threading.Thread _pingThread;
        bool useThreadpoolDispatch = false;
        Ruanal.Core.CommandRouter taskRouter;
        List<ServiceDomainItem> serviceItems = new List<ServiceDomainItem>();
        Ruanal.Core.Notify.TcpNotifyServer listenServer;
        bool _DoHostListening = false;
        string listenHostAddress = "";
        int listenHostPort = 0;
        //Dictionary<int, System.Threading.Thread> _disExecThreads = new Dictionary<int, System.Threading.Thread>();
        //object _disExecThreadslocker = new object();
        object servicelocker = new object();
        System.Threading.Thread _dispatchThread;
        System.Threading.AutoResetEvent _getDispatchARE;
        System.Threading.AutoResetEvent _pingARE;
        public override void Start()
        {
            //AppDomain.CurrentDomain.AssemblyResolve += _ALoader;
            _InitConfig();
            _NodeConfig();
            _TaskAllConfig();
            _ResumeTask();
            _StartListenHost();//新加，工作节点也可以设置TCP中心服务器
            _Listening();
            _StartPingTask();
            _StartDispatchTask();
        }


        public override void Stop()
        {
            //关闭所有任务
            try { _pingThread.Abort(); }
            catch (Exception ex) { }
            Schedule.Dispose();

            foreach (var s in serviceItems.ToList())
            {
                try { s.Stop(); }
                catch (Exception ex) { }
            }
            _synCacheTask();

        }
        #region 内部管理
        private void _InitConfig()
        {
            Ruanal.Core.Config.NodeType = "WorkNode";
            Ruanal.Core.Config.BuildClientId();
            string[] listenstring = Ruanal.Core.Config.GetSystemConfig(Ruanal.Core.ConfigConst.NotifyListenName, "").Trim().Split(':');
            if (listenstring.Length == 2)
            {
                _ListenAddress = listenstring[0];
                _ListenPort = RLib.Utils.Converter.StrToInt(listenstring[1]);
            }

            _DoHostListening = Ruanal.Core.Config.GetSystemConfig(Ruanal.Core.ConfigConst.DispatchDoListeningName, "true").ToLower() == "true";
            string[] hostlistenstring = Ruanal.Core.Config.GetSystemConfig(Ruanal.Core.ConfigConst.NotifyHostListenName, "").Trim().Split(':');
            if (hostlistenstring.Length == 2)
            {
                listenHostAddress = hostlistenstring[0];
                listenHostPort = RLib.Utils.Converter.StrToInt(hostlistenstring[1]);
            }

            var strSeconds = Ruanal.Core.Config.GetSystemConfig(Ruanal.Core.ConfigConst.PingSecondsName, Ruanal.Core.ConfigConst.PING_TIMESPAN_SECONDS.ToString());
            _PingSeconds = Math.Max(5, RLib.Utils.Converter.StrToInt(strSeconds));
            taskRouter = new Core.CommandRouter();
            taskRouter.OnStartTask += TaskRouter_OnStartTask;
            taskRouter.OnStopTask += TaskRouter_OnStopTask;
            taskRouter.OnDeleteTask += TaskRouter_OnDeleteTask;
            taskRouter.OnConfigUpdate += TaskRouter_OnConfigUpdate;
            taskRouter.OnStopDispatchJob += TaskRouter_OnStopDispatchJob;
        }


        private void _StartListenHost()
        {
            Console.WriteLine("_StartListenHost");
            //不监听 多主节点情况等
            if (_DoHostListening == false)
                return;
            if (!string.IsNullOrWhiteSpace(listenHostAddress))
            {
                listenServer = new Core.Notify.TcpNotifyServer();
                listenServer.Listen(listenHostAddress, listenHostPort);
            }
        }


        private void _Listening()
        {
            if (!string.IsNullOrWhiteSpace(_ListenAddress))
            {
                Core.Notify.NotifyHelper.OnReveiveMsg += (t, m) =>
                 {
                     switch (t)
                     {
                         case Ruanal.Core.ConfigConst.NotifyTopic_NewCmd:
                             if ((m ?? "").Contains(Ruanal.Core.Config.ClientID))
                                 DoPingSign();
                             break;
                         case Ruanal.Core.ConfigConst.NotifyTopic_RestartNode:
                             if ((m ?? "").Contains(Ruanal.Core.Config.ClientID))
                                 Ruanal.Core.ServiceMaintance.SendRestartCmd();
                             break;
                         case Ruanal.Core.ConfigConst.NotifyTopic_NewDispatch:
                             if (serviceItems.ToList().Exists(x => x.TaskId.ToString() == m))
                                 DoGetDispatchSign();
                             break;
                     }
                 };
                Core.Notify.NotifyHelper.OnTalk += _OnTalk;
                Core.Notify.NotifyHelper.Init("worknode", _ListenAddress + ":" + _ListenPort.ToString(),
                    Ruanal.Core.ConfigConst.NotifyTopic_NewCmd + "," +
                    Ruanal.Core.ConfigConst.NotifyTopic_NewDispatch + "," +
                    Ruanal.Core.ConfigConst.NotifyTopic_RestartNode
                    );
            }
        }
        private void _StartPingTask()
        {
            if (_pingThread != null)
                return;
            _pingARE = new System.Threading.AutoResetEvent(false);
            _pingThread = new System.Threading.Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var pingresult = Ruanal.Core.ApiSdk.SystemApi.WorkNodePing(
                                serviceItems.ToList().Select(x => new int[] { x.TaskId, x.TaskVersion }).ToList(),
                                serviceItems.ToList().Where(x => x.JobType == 1).Select(x => new int[] { x.TaskId, x.GetFreeCount() }).ToList()
                                );
                        if (pingresult.code > 0 && pingresult.data.HasCmd > 0)
                        {
                            _pingARE.Reset();
                            System.Threading.ThreadPool.QueueUserWorkItem(x =>
                            {
                                try { taskRouter.Router(); } catch (Exception) { }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        RLib.WatchLog.Loger.Error("Ping", ex);
                    }
                    _pingARE.WaitOne(TimeSpan.FromSeconds(_PingSeconds));
                    //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(_PingSeconds)); 
                }
            });
            _pingThread.IsBackground = true;
            _pingThread.Start();
        }

        private void _StartDispatchTask()
        {
            if (_dispatchThread != null)
                return;
            _getDispatchARE = new System.Threading.AutoResetEvent(false);
            _dispatchThread = new System.Threading.Thread(() =>
            {
                List<int> lastExecDis = new List<int>();
                int keepcount = 100;
                while (true)
                {
                    var freestate = serviceItems.ToList().Where(x => x.JobType == 1).Select(x => new int[] { x.TaskId, x.GetFreeCount() }).ToList();
                    if (freestate.Count > 0 && freestate.Count(x => x[1] > 0) > 0)
                    {
                        try
                        {
                            var pingresult = Ruanal.Core.ApiSdk.SystemApi.GetDispatchWork(freestate);
                            if (pingresult.code > 0)
                            {
                                _getDispatchARE.Reset();
                                foreach (var a in pingresult.data)
                                {
                                    if (lastExecDis.Contains(a.DispatchId))
                                        continue;
                                    OnDispatchJob(a);
                                    lastExecDis.Add(a.DispatchId);
                                    while (lastExecDis.Count > keepcount)
                                        lastExecDis.RemoveAt(0);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            RLib.WatchLog.Loger.Error("GetDispatchWork", ex);
                        }
                    }
                    _getDispatchARE.WaitOne(TimeSpan.FromSeconds(_PingSeconds));
                }
            });
            _dispatchThread.IsBackground = true;
            _dispatchThread.Start();
        }

        private void _NodeConfig()
        {
            //nodeConfig
            var result = Ruanal.Core.ApiSdk.SystemApi.GetNodeConfig();
            if (result.code > 0 && result.data != null)
            {
                Ruanal.Core.Config.NodeConfig = result.data;
                Ruanal.Core.Config.StoreNodeConfig();
            }
            else
            {
                Ruanal.Core.Config.ResumeNodeConfig();
            }
            var threadPoolDis = "";
            if (Ruanal.Core.Config.NodeConfig.ContainsKey("threadPoolDis"))
            {
                threadPoolDis = (Ruanal.Core.Config.NodeConfig["threadPoolDis"] ?? "").ToLower();
            }
            useThreadpoolDispatch = threadPoolDis == "1" || threadPoolDis == "true";

        }
        private void _TaskAllConfig()
        {
            var taskresult = Ruanal.Core.ApiSdk.TaskApi.GetAllTask();
            if (taskresult.code > 0 && taskresult.data != null)
            {
                Ruanal.Core.Config.TaskConfig = taskresult.data;
                Ruanal.Core.Config.StoreTaskConfig();
            }
            else
            {
                Ruanal.Core.Config.ResumeTaskConfig();
            }
        }

        private void _ResumeTask()
        {
            RLib.WatchLog.Loger.Log("ResumeTask", "");
            foreach (var t in Ruanal.Core.Config.TaskConfig.ToArray())
            {
                try
                {
                    _innerStart(t);
                }
                catch (Exception ex)
                {
                    RLib.WatchLog.Loger.Error("ResumeTask_" + t.TaskId, ex);
                }
            }
        }

        private void DoGetDispatchSign()
        {
            _getDispatchARE.Set();
        }

        private void DoPingSign()
        {
            _pingARE.Set();
        }
        private bool _innerStart(Ruanal.Core.ApiSdk.TaskDetail taskdetail)
        {
            lock (servicelocker)
            {
                if (serviceItems.Exists(x => x.TaskId == taskdetail.TaskId))
                {
                    RLib.WatchLog.Loger.Log("启动任务", "已在运行，无需启动");
                    return true;
                }
                ServiceDomainItem serviceitem = new ServiceDomainItem(taskdetail);
                serviceitem.OnWockComplet += DoGetDispatchSign;
                if (serviceitem.JobType == 0)
                {
                    bool setOk = this.Schedule.StartJob(taskdetail.TaskId.ToString(), serviceitem, taskdetail.RunCron);
                    if (setOk == false)
                    {
                        RLib.WatchLog.Loger.Log("启动任务", "加入计划失败！");
                        return false;
                    }
                    var JobContext = this.Schedule.GetJobContext(taskdetail.TaskId.ToString());
                    serviceitem.JobContext = JobContext;
                }
                serviceItems.Add(serviceitem);
                RLib.WatchLog.Loger.Log("启动任务", "完成");
                return true;
            }
        }

        private void TaskRouter_OnDeleteTask(Core.ApiSdk.CmdDetail cmdarg)
        {
            string key = "taskcmd-" + cmdarg.TaskArg.TaskId;
            taskRouter.LockCmdExecute(key, () =>
            {
                lock (servicelocker)
                {
                    RLib.WatchLog.Loger.Log("删除任务", cmdarg.TaskArg.TaskId.ToString());
                    _StopWork(cmdarg.TaskArg.TaskId);
                    string dirtask = Ruanal.Core.Config.GetTaskItemDir(cmdarg.TaskArg.TaskId);
                    Ruanal.Core.Utils.Utils.DeleteFileOrDir(dirtask);
                }
            });
        }

        private void TaskRouter_OnStopTask(Core.ApiSdk.CmdDetail cmdarg)
        {
            string key = "taskcmd-" + cmdarg.TaskArg.TaskId;
            taskRouter.LockCmdExecute(key, () =>
            {
                RLib.WatchLog.Loger.Log("停止任务", cmdarg.TaskArg.TaskId.ToString());
                _StopWork(cmdarg.TaskArg.TaskId);
            });
        }

        private void TaskRouter_OnStartTask(Core.ApiSdk.CmdDetail cmdarg)
        {
            string key = "taskcmd-" + cmdarg.TaskArg.TaskId;
            taskRouter.LockCmdExecute(key, () =>
            {
                RLib.WatchLog.Loger.Log("启动任务", cmdarg.TaskArg.TaskId.ToString());
                _StartWork(cmdarg.TaskArg.TaskId);
            });
        }
        private void TaskRouter_OnConfigUpdate(Core.ApiSdk.CmdDetail cmdarg)
        {
            RLib.WatchLog.Loger.Log("更新配置", "");
            _NodeConfig();
        }

        #endregion

        #region  任务管理

        private bool _StartWork(int taskId)
        {
            if (this.serviceItems.ToList().Exists(x => x.TaskId == taskId))
            {
                return true;
            }
            var taskdetail = Ruanal.Core.ApiSdk.TaskApi.GetTaskDetail(taskId);
            if (taskdetail.code <= 0)
            {
                throw new Exception("任务获取失败！");
            }
            try
            {
                _innerStart(taskdetail.data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _synCacheTask();
            }
            DoPingSign();
            return true;
        }
        private void _synCacheTask()
        {
            var list = new List<Ruanal.Core.ApiSdk.TaskDetail>();
            foreach (var a in serviceItems.ToList())
            {
                list.Add(a.TaskDetail);
            }
            Ruanal.Core.Config.TaskConfig = list;
            Ruanal.Core.Config.StoreTaskConfig();
        }
        private bool _StopWork(int taskId)
        {
            if (!this.serviceItems.ToList().Exists(x => x.TaskId == taskId))
            {
                return true;
            }
            try
            {
                ServiceDomainItem currtask = null;
                lock (servicelocker)
                {
                    currtask = serviceItems.FirstOrDefault(x => x.TaskId == taskId);
                    if (currtask == null)
                        return true;
                    currtask.Stop();
                    serviceItems.Remove(currtask);
                }
                if (currtask != null && currtask.JobType == 0)
                {
                    this.Schedule.StopJob(currtask.JobContext.JobName);
                }
                DoPingSign();
                return true;
            }
            finally
            {
                _synCacheTask();
            }
        }
        private string _OnTalk(string msg)
        {
            if (msg.StartsWith(Ruanal.Core.ConfigConst.TalkAskDispatchRunning))
            {
                string[] info = msg.Substring(Ruanal.Core.ConfigConst.TalkAskDispatchRunning.Length).Split(new char[] { '#' }, 2);
                if (info.Length != 2)
                    return null;
                int taskId = RLib.Utils.Converter.StrToInt(info[0]);
                string runkey = info[1];
                ServiceDomainItem taskitem = null;
                lock (servicelocker)
                {
                    taskitem = serviceItems.FirstOrDefault(x => x.TaskId == taskId);
                }
                if (taskitem != null)
                {
                    int[] dispathIds = null;
                    if (taskitem.IsRuningKey(runkey, out dispathIds))
                    {
                        return dispathIds.Length + ":" + string.Join("_", dispathIds);
                    }
                    return "0";
                }
            }
            if (msg.StartsWith(Ruanal.Core.ConfigConst.TalkTaskInstanceStatus))
            {
                string[] info = msg.Substring(Ruanal.Core.ConfigConst.TalkTaskInstanceStatus.Length).Split(new char[] { '#' }, 1);
                if (info.Length != 1)
                    return null;
                int taskId = RLib.Utils.Converter.StrToInt(info[0]);
                ServiceDomainItem taskitem = null;
                lock (servicelocker)
                {
                    taskitem = serviceItems.FirstOrDefault(x => x.TaskId == taskId);
                }
                if (taskitem != null)
                {
                    string ss = string.Format("【{0}】#{1}", Ruanal.Core.Config.ClientID,
                        string.Join("#", taskitem.InstanceSummary()));
                    return ss;
                }
            }

            if (msg.StartsWith(Ruanal.Core.ConfigConst.TalkNodeTaskStatus))
            {
                string[] info = msg.Substring(Ruanal.Core.ConfigConst.TalkNodeTaskStatus.Length).Split(new char[] { '#' }, 1);
                if (info.Length != 1)
                    return null;
                if (info[0] != Ruanal.Core.Config.ClientID)
                    return null;
                StringBuilder sb = new StringBuilder();
                foreach (var a in serviceItems.ToList())
                {
                    sb.AppendFormat("【{0}】[P:{1}] [M:{2}] [T:{3}]#", a.TaskId, a.innerServices.Count(), a.GetMemoryMB(), a.TaskDetail.Title);
                }
                return sb.ToString();
            }
            return null;
        }
        private bool _AskDispathIsRun(int taskId, string runKey, int maxCount)
        {
            string msg = Ruanal.Core.ConfigConst.TalkAskDispatchRunning + string.Format("{0}#{1}", taskId, runKey);
            var result = Ruanal.Core.Notify.NotifyHelper.TalkToAll(msg, 3000, maxCount);
            if (result.Count(x => x == "0") == result.Count)
                return false;
            return true;
        }
        private void OnDispatchJob(Core.DispatchArg arg)
        {
            ServiceDomainItem servicedomain = null;
            lock (servicelocker)
            {
                servicedomain = serviceItems.FirstOrDefault(x => x.TaskId == arg.TaskId);
                if (servicedomain == null || servicedomain.GetFreeCount() == 0)
                    return;
            }
            //RLib.WatchLog.Loger.Log("分配任务", RLib.Utils.DataSerialize.SerializeJson(arg));
            if (useThreadpoolDispatch)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(x =>
                {
                    _dispatchJobExec(servicedomain, arg);
                });
            }
            else
            {
                System.Threading.Thread t = new System.Threading.Thread(() =>
                {
                    _dispatchJobExec(servicedomain, arg);
                });
                t.IsBackground = true;
                t.Start();
            }
        }

        private void _dispatchJobExec(ServiceDomainItem servicedomain, Core.DispatchArg arg)
        {
            try
            {
                int startStatus = servicedomain.StartDispatch(arg, () =>
                {
                    var result = Core.ApiSdk.SystemApi.BeginDispatchExecute(arg.DispatchId);
                    if (result.code <= 0)
                    {
                        RLib.WatchLog.Loger.Error("开始调度执行失败", arg.DispatchId + "\r\n" + result.msg);
                        return false;
                    }
                    //需要检查是否在运行
                    if (result.data > 0 && !string.IsNullOrEmpty(arg.RunKey))
                    {
                        int[] currDispathIds = null;
                        var isrunning = servicedomain.IsRuningKey(arg.RunKey, out currDispathIds);
                        if (!isrunning)
                        {
                            isrunning = _AskDispathIsRun(arg.TaskId, arg.RunKey, servicedomain.TaskDetail.NodeCount);
                        }
                        if (isrunning)
                        {
                            Core.ApiSdk.SystemApi.SkipDispatchExecute(arg.DispatchId);
                            DoGetDispatchSign();
                            return false;
                        }
                        else
                        {
                            //自动结束
                            Core.ApiSdk.SystemApi.AutoEndDispatchExecute(result.data);
                        }
                    }
                    return true;
                });
                if (startStatus == -1)
                {
                    RLib.WatchLog.Loger.Error("分配任务失败 没有取得空闲实例", "DispatchId=" + arg.DispatchId + " ;RunArgs" + arg.RunArgs);
                    return;
                }
                if (startStatus == 0)
                {
                    RLib.WatchLog.Loger.Error("分配任务失败 有空闲实例，但分配不允许运行", "DispatchId=" + arg.DispatchId + " ;RunArgs" + arg.RunArgs);
                    return;
                }
                Core.ApiSdk.SystemApi.EndDispatchExecute(arg.DispatchId, true, "");
            }
            catch (Exception ex)
            {
                RLib.WatchLog.Loger.Error("调度执行失败", ex);
                Core.ApiSdk.SystemApi.EndDispatchExecute(arg.DispatchId, false, ex.Message);
            }
        }
        private bool _InnerStopDispatch(int taskId, string invokeid)
        {
            if (!this.serviceItems.ToList().Exists(x => x.TaskId == taskId))
            {
                return true;
            }
            ServiceDomainItem currtask = null;
            lock (servicelocker)
            {
                currtask = serviceItems.FirstOrDefault(x => x.TaskId == taskId);
                if (currtask == null)
                    return true;
                var r = currtask.StopDispatch(invokeid);
                if (r == false)
                {
                    throw new Exception("分配不存在或已运行结束！");
                }
            }
            return true;
        }


        private void TaskRouter_OnStopDispatchJob(Core.ApiSdk.CmdDetail cmdarg)
        {
            _InnerStopDispatch(cmdarg.DispatchArg.TaskId, cmdarg.DispatchArg.InvokeId);
        }

        #endregion

    }
}
