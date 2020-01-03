using Ruanal.Core.ApiSdk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ruanal.Core
{
    public delegate void CmdHander(CmdDetail cmdarg);
    public class CommandRouter
    {
        public CmdHander OnStartTask;
        public CmdHander OnStopTask;
        public CmdHander OnDeleteTask;
        public CmdHander OnStopDispatchJob;
        public CmdHander OnConfigUpdate;
        readonly int _maxCount;
        int freeCount;
        object lockergetting = new object();
        object lockercounter = new object();
        private CommandQueue cmdQueue = new CommandQueue();
        Dictionary<string, object> cmdLockers = new Dictionary<string, object>();
        object cmdRootLock = new object();
        public CommandRouter()
        {
            _maxCount = ConfigConst.MaxCMDParallel;
            freeCount = _maxCount;
        }

        public void LockCmdExecute(string key, Action action)
        {
            object cmdlock = null;
            lock (cmdRootLock)
            {
                if (!cmdLockers.ContainsKey(key))
                {
                    cmdLockers[key] = new object();
                }
                cmdlock = cmdLockers[key];
            }
            if (cmdlock != null)
            {
                lock (cmdlock)
                {
                    action.Invoke();
                }
            }
            else
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// 发送重启命令
        /// </summary>
        /// <param name="cmdarg"></param>
        private void OnRestartNode(CmdDetail cmdarg)
        {
            ServiceMaintance.SendRestartCmd();
        }

        private bool TryEnter()
        {
            if (freeCount == 0)
                return false;
            lock (lockercounter)
            {
                if (freeCount == 0)
                    return false;
                freeCount--;
                Console.WriteLine("currCount:" + freeCount);
                return true;
            }
        }
        private bool CanEnter()
        {
            return freeCount > 0;
        }
        private void TryOut()
        {
            lock (lockercounter)
            {
                freeCount++;
                Console.WriteLine("currCount:" + freeCount);
            }

            ThreadPool.QueueUserWorkItem(x =>
            {
                Router();
            });
        }

        private void RequestGetCMD()
        {
            if (cmdQueue.Count > 0) return;
            if (Monitor.TryEnter(lockergetting))
            {
                try
                {
                    var cmds = ApiSdk.CommandApi.GetCommands();
                    if (cmds.code <= 0)
                    {
                        return;
                    }
                    foreach (var a in cmds.data)
                    {
                        cmdQueue.TryEnqueue(a);
                    }
                }
                finally
                {
                    Monitor.Exit(lockergetting);
                }
            }
        }

        public void Router()
        {
            RequestGetCMD();
            if (!CanEnter()) return;
            if (cmdQueue.Count == 0)
                return;
            while (cmdQueue.Count > 0)
            {
                if (!TryEnter()) break;
                CmdDetail cmd = cmdQueue.TryBegin();
                if (cmd == null) break;
                RouterCmd(cmd);
            }
        }

        private void RouterCmd(ApiSdk.CmdDetail arg)
        {
            CmdHander handler = null;
            switch (arg.CmdType)
            {
                case ConfigConst.CmdType_StartTask:
                    arg.TaskArg = RLib.Utils.DataSerialize.DeserializeObject<Ruanal.Core.CmdTaskArg>(arg.CmdArgs);
                    handler = OnStartTask;
                    break;
                case ConfigConst.CmdType_StopTask:
                    arg.TaskArg = RLib.Utils.DataSerialize.DeserializeObject<Ruanal.Core.CmdTaskArg>(arg.CmdArgs);
                    handler = OnStopTask;
                    break;
                case ConfigConst.CmdType_DeleteTask:
                    arg.TaskArg = RLib.Utils.DataSerialize.DeserializeObject<Ruanal.Core.CmdTaskArg>(arg.CmdArgs);
                    handler = OnDeleteTask;
                    break;
                //case ConfigConst.CmdType_DispatchJob:
                //    arg.DispatchArg = RLib.Utils.DataSerialize.DeserializeObject<Ruanal.Core.CmdDispatchArg>(arg.CmdArgs);
                //    handler = OnDispatchJob;
                //    break;
                case ConfigConst.CmdType_StopDispatchJob:
                    arg.DispatchArg = RLib.Utils.DataSerialize.DeserializeObject<Ruanal.Core.CmdDispatchArg>(arg.CmdArgs);
                    handler = OnStopDispatchJob;
                    break;
                case ConfigConst.CmdType_ConfigUpdate:
                    handler = OnConfigUpdate;
                    break;
                case ConfigConst.CmdType_RestartNode:
                    handler = OnRestartNode;
                    break;
            }
            _invoke(handler, arg);
        }


        void _invoke(CmdHander handler, ApiSdk.CmdDetail arg)
        {
            ThreadPool.QueueUserWorkItem((x) =>
            {
                try
                {
                    var result = ApiSdk.CommandApi.BeginExecute(arg.CmdId);
                    if (result.code <= 0)
                    {
                        RLib.WatchLog.Loger.Error("开始命令执行失败", result.msg);
                        return;
                    }
                    if (handler == null)
                        throw new Exception("该命令没有执行程序！");
                    handler.Invoke(arg);
                    ApiSdk.CommandApi.EndExecute(arg.CmdId, true, "");
                }
                catch (Exception ex)
                {
                    RLib.WatchLog.Loger.Error("命令执行失败", ex);
                    ApiSdk.CommandApi.EndExecute(arg.CmdId, false, ex.Message);
                }
                finally
                {
                    cmdQueue.TryEnd(arg);
                    TryOut();
                }
            });
        }

    }
}
