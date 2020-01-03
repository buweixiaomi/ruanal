using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ruanal.ServiceSchedule;
using System.Reflection;

namespace Ruanal.DispatchNode
{
    public class DispatchItem : ServiceSchedule.IJobExecutor
    {
        #region 私有字段
        //System.Threading.Thread _checkThread;
        Ruanal.Job.DispatcherBase _dispatchbase;
        object execlocker = new object();
        //System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
        AppDomain _taskDomain;
        string _enterDll;
        string _dispatchClass;
        string _taskDir;
        string _jobBinDir;
        #endregion

        #region 公有字段
        public JobContext JobContext { get; set; }
        public bool IsRunning { get; private set; }
        public int TaskId { get; private set; }
        public int TaskVersion { get; private set; }
        public Ruanal.Core.ApiSdk.TaskDetail TaskDetail;
        public string GroupId { get; private set; }
        public string RunGuid { get; private set; }
        #endregion

        public DispatchItem(Ruanal.Core.ApiSdk.TaskDetail taskdetail)
        {
            this.TaskDetail = taskdetail;
            TaskId = this.TaskDetail.TaskId;
            _MakeLocalFile();
            _GetDispatcher();
            _InitDispatch();
        }

        #region 私有方法
        private void _CheckIsRuningState()
        {
            if (string.IsNullOrWhiteSpace(GroupId))
                return;
            var checkresult = Ruanal.Core.ApiSdk.TaskApi.CheckDispatchGroup(GroupId);
            if (checkresult.code > 0 && checkresult.data == 0)
            {
                Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, RunGuid, true, "");
                GroupId = "";
            }
        }

        private bool ApiCheckIsRuning()
        {
            if (string.IsNullOrWhiteSpace(GroupId))
                return false;
            var checkresult = Ruanal.Core.ApiSdk.TaskApi.CheckDispatchGroup(GroupId);
            if (checkresult.code <= 0)
            {
                Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, RunGuid, true, "检查执行状态失败");
                return false;
            }
            if (checkresult.data == 0)
            {
                Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, RunGuid, true, "");
                GroupId = "";
                return false;
            }
            return true;
        }


        private void _MakeLocalFile()
        {
            string dirtask = Ruanal.Core.Config.GetTaskItemDir(TaskId);
            _taskDir = dirtask;
            if (string.IsNullOrWhiteSpace(Core.ConfigConst.JobBin))
            {
                _jobBinDir = dirtask;
            }
            else
            {
                _jobBinDir = System.IO.Path.Combine(dirtask, Core.ConfigConst.JobBin);
            }
            var dllname = System.IO.Path.Combine(dirtask, TaskDetail.EnterDll);
            string versionFile = System.IO.Path.Combine(dirtask, Ruanal.Core.ConfigConst.TaskVersionFileName);
            if (!System.IO.File.Exists(dllname) || !System.IO.File.Exists(versionFile))
            {
                var file = Ruanal.Core.ApiSdk.SystemApi.DownloadFile2(TaskDetail.FileUrl);
                if (file.code <= 0)
                {
                    throw new Exception("下载任务文件出错：" + file.msg);
                }
                string filename = Ruanal.Core.Config.GetTempFileName();
                System.IO.File.WriteAllBytes(filename, file.data);

                Ruanal.Core.Utils.Utils.CopyDir(AppDomain.CurrentDomain.BaseDirectory, _jobBinDir, false, "*.dll");
                Ruanal.Core.Utils.Utils.CopyDir(AppDomain.CurrentDomain.BaseDirectory, _jobBinDir, false, "*.pdb");
                Ruanal.Core.Utils.Utils.CopyDir(AppDomain.CurrentDomain.BaseDirectory, _jobBinDir, false, "*.exe");

                Ruanal.Core.Utils.Utils.UnZip(filename, dirtask, "", true);
                DeleteSameDll();
                System.IO.File.WriteAllText(versionFile, TaskDetail.CurrVersionId.ToString());
                Ruanal.Core.Utils.Utils.DeleteFileOrDir(filename);
                if (!System.IO.File.Exists(dllname))
                {
                    throw new Exception("没有找到运行dll");
                }
            }
            try { TaskVersion = RLib.Utils.Converter.StrToInt(System.IO.File.ReadAllText(versionFile)); }
            catch { }
            _enterDll = dllname;
        }

        private void DeleteSameDll()
        {
            var files = new string[] {
                System.IO.Path.Combine(_taskDir, "Ruanal.Core.dll"),
                System.IO.Path.Combine(_taskDir, "Ruanal.Job.dll"),
                System.IO.Path.Combine(_taskDir, "Ruanal.ServiceSchedule.dll"),
                System.IO.Path.Combine(_taskDir, "RLib.dll")
            };
            foreach (var x in files)
            {
                if (System.IO.File.Exists(x))
                    System.IO.File.Delete(x);
            }
        }


        bool _noLink = false;
        private void _GetDispatcher()
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = _taskDir;
            setup.ShadowCopyFiles = "true";
            if (!string.IsNullOrWhiteSpace(Core.ConfigConst.JobBin))
            {
                setup.PrivateBinPath = Core.ConfigConst.JobBin;
            }
            string appconfigfile = System.IO.Path.Combine(_taskDir, "App.config");
            string appconfigfile2 = System.IO.Path.Combine(_taskDir, TaskDetail.EnterDll + ".config");
            if (System.IO.File.Exists(appconfigfile))
            {
                setup.ConfigurationFile = appconfigfile;
            }
            if (System.IO.File.Exists(appconfigfile2))
            {
                setup.ConfigurationFile = appconfigfile2;
            }
            AppDomain.MonitoringIsEnabled = true;
            _taskDomain = AppDomain.CreateDomain("task_" + TaskDetail.TaskId, null, setup);
            Core.DomainAssembleLoader.ConfigLoad(_taskDomain);
            var taskConfig = BuildTaskConfig();

            List<string> trydisclass = new List<string>();
            object obj_serviceinstance = null;
            _dispatchClass = "";

            var nolink = Ruanal.Job.DispatchProxy.IsNoLink(_enterDll, TaskDetail.DispatchClass);
            _noLink = nolink;
            if (nolink)
            {
                _enterDll = System.IO.Path.Combine(_jobBinDir, "Ruanal.Job.dll");
                trydisclass.Add("Ruanal.Job.DispatchProxy");
            }
            else
            {
                if (TaskDetail.DispatchClass.IndexOf('.') > 0)
                {
                    trydisclass.Add(TaskDetail.DispatchClass);
                }
                else
                {
                    trydisclass.Add(TaskDetail.DispatchClass);
                    var indexext = TaskDetail.EnterDll.LastIndexOf('.');
                    var sub = indexext > 0 ? TaskDetail.EnterDll.Substring(0, indexext) : TaskDetail.EnterDll;
                    sub += ".";
                    trydisclass.Add(sub + TaskDetail.DispatchClass);
                    trydisclass.Add(sub + "ImpDispatchs." + TaskDetail.DispatchClass);
                    trydisclass.Add(sub + "Dispatchs." + TaskDetail.DispatchClass);
                }
            }
            foreach (var a in trydisclass)
            {
                try
                {
                    obj_serviceinstance = _taskDomain.CreateInstanceFromAndUnwrap(_enterDll, a);
                    if (obj_serviceinstance != null)
                    {
                        _dispatchClass = a;
                        break;
                    }
                    break;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            if (obj_serviceinstance == null)
                throw new Exception("调度器" + TaskDetail.DispatchClass + "不存在！，查找位置:" + string.Join(",", trydisclass));
            if (obj_serviceinstance is Ruanal.Job.DispatcherBase)
            {
                var currinstance = obj_serviceinstance as Ruanal.Job.DispatcherBase;
                //string InstanceId = Guid.NewGuid().ToString().Replace("-", "");
                //currinstance.GlobalInit(RLib.Utils.DataSerialize.SerializeJson(taskConfig));
                //var crosslog = new Job.CrossLoger(InstanceId, ServiceLogHandler);
                //currinstance.Loger = crosslog;
                //currinstance.Init();
                _dispatchbase = currinstance;
            }
            else
            {
                throw new Exception(string.Format("类型{0}不是{1}类型", TaskDetail.EnterClass, typeof(Ruanal.Job.JobServiceBase).Name));
            }
        }

        private void _InitDispatch()
        {
            _dispatchbase.GlobalInit(RLib.Utils.DataSerialize.SerializeJson(BuildTaskConfig()));
            _dispatchbase.Loger = new Job.CrossLoger("", ServiceLogHandler);
            _dispatchbase.ParentCaller = new Job.ParentCaller("", JobItem_Caller);
            _dispatchbase.Init();
            if (_noLink)
            {
                ((Ruanal.Job.DispatchProxy)_dispatchbase).InitProxy(TaskDetail.EnterDll, TaskDetail.DispatchClass);
            }
        }
        public bool ServiceLogHandler(string instanceId, string msg, int type, bool isAsyn)
        {
            return Ruanal.Core.ServerLog.AddWorkLog(TaskId, RunGuid, type, msg, isAsyn);
        }

        /// <summary>
        /// 任务回调方法
        /// </summary>
        /// <param name="instanceID"></param>
        private object JobItem_Caller(string instanceID, string callType, object[] args)
        {
            try
            {
                //回调 线程任务完成
                if (callType == Job.ParentCaller.CallType_EndJob)
                {
                    this.IsRunning = false;
                    return true;
                }
                if (callType == Job.ParentCaller.CallType_TaskConfig)
                {
                    return false;
                }
                if (callType == Job.ParentCaller.CallType_PostDispatch)
                {
                    string msg = "";
                    bool postok = PostDispatch(args[0] as List<Job.DispatcherItem>, out msg);
                    if (postok)
                    {
                        return msg;
                    }
                    return null;
                }
                return false;
            }
            catch (Exception ex)
            {
                RLib.WatchLog.Loger.Error("任务设用父级方法出错 任务ID:" + TaskId + " callType:" + callType, ex);
                return false;
            }
        }


        private Dictionary<string, string> BuildTaskConfig()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var a in Ruanal.Core.Config.NodeConfig)
            {
                dic[a.Key] = a.Value;
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(TaskDetail.TaskConfig))
                {
                    var tdic = RLib.Utils.DataSerialize.DeserializeObject<Dictionary<string, string>>(TaskDetail.TaskConfig);

                    foreach (var a in tdic)
                    {
                        if (!dic.ContainsKey(a.Key) || !string.IsNullOrEmpty(a.Value))
                        {
                            dic[a.Key] = a.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("任务配置有误！" + ex.Message);
            }
            return dic;
        }

        public string GetMemoryMB()
        {
            if (_taskDomain == null) return string.Empty;
            double use = _taskDomain.MonitoringSurvivedMemorySize / 1024 / 1024;
            double allall = _taskDomain.MonitoringTotalAllocatedMemorySize / 1024 / 1024;
            return string.Format("{0}/{1}MB", use, allall);
        }

        #endregion
        #region 接口实现方法

        public void Execute(JobContext context)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            if (IsRunning)
            {
                if (ApiCheckIsRuning())
                {
                    return;
                }
                IsRunning = false;
            }
            lock (execlocker)
            {
                if (IsRunning)
                    return;
                if (context.IsRunOnce)
                {
                    RunOnceExec();
                }
                else
                {
                    NormalExec();
                }
            }
        }

        private void RunOnceExec()
        {
            try
            {
                RunGuid = Guid.NewGuid().ToString().Replace("-", "");
                Ruanal.Core.ApiSdk.SystemApi.TaskBeginRunLog(TaskId, RunGuid, Core.ApiSdk.TaskRunType.DoDispatch);
                _dispatchbase.GetDispatchs();
                IsRunning = true;
                Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, RunGuid, true, "");
            }
            catch (Exception ex)
            {
                Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, RunGuid, false, ex.Message);
                IsRunning = false;
            }
        }

        private void NormalExec()
        {
            IsRunning = true;
            RunGuid = Guid.NewGuid().ToString().Replace("-", "");
            Ruanal.Core.ApiSdk.SystemApi.TaskBeginRunLog(TaskId, RunGuid, Core.ApiSdk.TaskRunType.DoDispatch);
            try
            {
                var dispatchconfigs = _dispatchbase.GetDispatchs();
                var t_groupId = string.Empty;
                var postok = PostDispatch(dispatchconfigs, out t_groupId);
                if (postok == false)
                {
                    postok = PostDispatch(dispatchconfigs, out t_groupId);
                }
                if (postok == false)
                {
                    Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, RunGuid, false, t_groupId);
                    IsRunning = false;
                    return;
                }
                GroupId = t_groupId;
            }
            catch (Exception ex)
            {
                Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, RunGuid, false, ex.Message);
                IsRunning = false;
            }
            finally
            {
            }
        }


        /// <summary>
        /// 提交分配，返回true时，msg为当前组编号
        /// </summary>
        /// <param name="disps"></param>
        /// <returns></returns>
        private bool PostDispatch(List<Job.DispatcherItem> disps, out string msg)
        {
            var disitems = RLib.Utils.DataSerialize.SerializeJson(disps);
            var t_groupId = Guid.NewGuid().ToString().Replace("-", "");
            var result = Ruanal.Core.ApiSdk.TaskApi.BuildDispatch(TaskId, t_groupId, disitems);
            if (result.code <= 0)
            {
                msg = result.msg;
                return false;
            }
            msg = t_groupId;
            return true;
        }

        #endregion

        public void Dispose()
        {
            try
            {
                this._dispatchbase.Dispose();
                System.Threading.Thread.Sleep(1 * 1000);
                AppDomain.Unload(_taskDomain);
                this.IsRunning = false;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
