using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.DispatchNode
{
    public class DispatchContainer : Ruanal.ServiceSchedule.ServiceContainer
    {

        //Host Listen
        string listenHostAddress;
        int listenHostPort;

        string _ListenAddress;
        int _ListenPort;
        int _PingSeconds;
        bool _DoHostListening;
        System.Threading.Thread _pingThread;
        System.Threading.AutoResetEvent _pingARE;
        Ruanal.Core.Notify.TcpNotifyServer listenServer;
        Ruanal.Core.CommandRouter taskRouter;
        List<DispatchItem> diswork = new List<DispatchItem>();
        object dislocker = new object();
        public override void Start()
        {
            _InitConfig();
            _NodeConfig();
            _TaskAllConfig();
            _StartListenHost();
            _Listening();
            _ResumeTask();
            _StartPingTask();
        }
        public override void Stop()
        {
            //关闭监听
            if (listenServer != null)
            {
                listenServer.Stop();
            }
            try { _pingThread.Abort(); }
            catch (Exception ex) { }
            //关闭所有任务
            foreach (var s in diswork.ToList())
            {
                Schedule.StopJob(s.JobContext.JobName);
            }
            Schedule.Dispose();
            _synCacheTask();
        }
        #region 内部管理
        private void _InitConfig()
        {
            Ruanal.Core.Config.BuildClientId();
            _DoHostListening = Ruanal.Core.Config.GetSystemConfig(Ruanal.Core.ConfigConst.DispatchDoListeningName, "true").ToLower() == "true";
            //api地址 监听地址 ping间隔 
            string[] listenstring = Ruanal.Core.Config.GetSystemConfig(Ruanal.Core.ConfigConst.NotifyListenName, "").Trim().Split(':');
            if (listenstring.Length == 2)
            {
                _ListenAddress = listenstring[0];
                _ListenPort = RLib.Utils.Converter.StrToInt(listenstring[1]);
            }
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
        }

        private void TaskRouter_OnDeleteTask(Core.ApiSdk.CmdDetail cmdarg)
        {
            lock (dislocker)
            {
                _StopWork(cmdarg.TaskArg.TaskId);
                string dirtask = Ruanal.Core.Config.GetTaskItemDir(cmdarg.TaskArg.TaskId);
                Ruanal.Core.Utils.Utils.DeleteFileOrDir(dirtask);
            }
        }

        private void TaskRouter_OnStopTask(Core.ApiSdk.CmdDetail cmdarg)
        {
            _StopWork(cmdarg.TaskArg.TaskId);
        }

        private void TaskRouter_OnStartTask(Core.ApiSdk.CmdDetail cmdarg)
        {
            _StartWork(cmdarg.TaskArg.TaskId);
        }
        private void TaskRouter_OnConfigUpdate(Core.ApiSdk.CmdDetail cmdarg)
        {
            _NodeConfig();
        }
        private void _NodeConfig()
        {
            Ruanal.Core.Config.NodeType = "MainNode";
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
            Console.WriteLine("_Listening");
            if (!string.IsNullOrWhiteSpace(_ListenAddress))
            {
                Core.Notify.NotifyHelper.OnReveiveMsg += (t, m) =>
                 {
                     switch (t)
                     {
                         case Ruanal.Core.ConfigConst.NotifyTopic_NewCmd:
                             if ((m ?? "").Contains(Ruanal.Core.Config.ClientID))
                                 _pingARE.Set();
                             break;
                         case Ruanal.Core.ConfigConst.NotifyTopic_RestartNode:
                             if ((m ?? "").Contains(Ruanal.Core.Config.ClientID))
                                 Ruanal.Core.ServiceMaintance.SendRestartCmd();
                             break;
                     }
                 };
                Core.Notify.NotifyHelper.OnTalk += _OnTalk;
                Core.Notify.NotifyHelper.Init("dispatch", _ListenAddress + ":" + _ListenPort.ToString(), 
                    Ruanal.Core.ConfigConst.NotifyTopic_NewCmd+","+ 
                    Ruanal.Core.ConfigConst.NotifyTopic_RestartNode);
            }
        }
        private void DoPingSign()
        {
            _pingARE.Set();
        }
        private void _StartPingTask()
        {
            Console.WriteLine("_StartPingTask");
            if (_pingThread != null)
                return;
            _pingARE = new System.Threading.AutoResetEvent(false);
            _pingThread = new System.Threading.Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var pingresult = Ruanal.Core.ApiSdk.SystemApi.MasterNodePing(
                       diswork.ToList().Select(x => new int[] { x.TaskId, x.TaskVersion }).ToList()
                       );
                        if (pingresult.code > 0 && pingresult.data.HasCmd > 0)
                        {
                            taskRouter.Router();
                        }
                    }
                    catch (Exception ex) { }
                    _pingARE.WaitOne(TimeSpan.FromSeconds(_PingSeconds));
                }
            });
            _pingThread.IsBackground = true;
            _pingThread.Start();
        }



        private void _ResumeTask()
        {
            Console.WriteLine("_ResumeTask");
            foreach (var t in Ruanal.Core.Config.TaskConfig.ToArray())
            {
                if (t.TaskType != 1)
                    continue;
                try
                {
                    _innerStart(t);
                }
                catch (Exception ex) { }
            }

        }

        private bool _innerStart(Ruanal.Core.ApiSdk.TaskDetail taskdetail)
        {
            if (taskdetail.TaskType != 1)
                throw new Exception("非调度任务，启动失败！");
            lock (dislocker)
            {
                if (diswork.Exists(x => x.TaskId == taskdetail.TaskId))
                    return true;

                DispatchItem dw = new DispatchItem(taskdetail);
                bool setOk = this.Schedule.StartJob(taskdetail.TaskId.ToString(), dw, taskdetail.RunCron);
                if (setOk)
                {
                    var JobContext = this.Schedule.GetJobContext(taskdetail.TaskId.ToString());
                    dw.JobContext = JobContext;
                    diswork.Add(dw);
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region  任务管理

        private bool _StartWork(int taskId)
        {
            if (this.diswork.ToList().Exists(x => x.TaskId == taskId))
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
                DoPingSign();
            }
            finally
            {
                _synCacheTask();
            }
            return true;
        }
        private void _synCacheTask()
        {
            var list = new List<Ruanal.Core.ApiSdk.TaskDetail>();
            foreach (var a in diswork.ToList())
            {
                list.Add(a.TaskDetail);
            }
            Ruanal.Core.Config.TaskConfig = list;
            Ruanal.Core.Config.StoreTaskConfig();
        }

        private bool _StopWork(int taskId)
        {
            if (!this.diswork.ToList().Exists(x => x.TaskId == taskId))
            {
                return true;
            }
            try
            {
                DispatchItem task = null;
                lock (dislocker)
                {
                    task = diswork.FirstOrDefault(x => x.TaskId == taskId);
                    if (task == null)
                        return true;
                    Schedule.StopJob(task.JobContext.JobName);
                    diswork.Remove(task);

                    task.Dispose();
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
            if (msg.StartsWith(Ruanal.Core.ConfigConst.TalkNodeTaskStatus))
            {
                string[] info = msg.Substring(Ruanal.Core.ConfigConst.TalkNodeTaskStatus.Length).Split(new char[] { '#' }, 1);
                if (info.Length != 1)
                    return null;
                if (info[0] != Ruanal.Core.Config.ClientID)
                    return null;
                StringBuilder sb = new StringBuilder();
                foreach (var a in diswork.ToList())
                {
                    sb.AppendFormat("【{0}】 [M:{1}] [T:{2}]#", a.TaskId, a.GetMemoryMB(), a.TaskDetail.Title);
                }
                return sb.ToString();
            }
            return null;
        }
        #endregion

    }

}
