using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ruanal.WorkNode
{
    public class ServiceDomainItem : ServiceSchedule.IJobExecutor
    {
        #region 私有字段
        string _dllFullName;
        string _taskDir;
        string _jobBinDir;
        AppDomain _taskDomain;
        int _maxInstanceCount = 1;
        bool _instanceCanReUse = false;
        object servicelock = new object();
        public event Action OnWockComplet;
        #endregion

        #region 公有字段
        public class ServiceAndDispatch
        {
            public Ruanal.Job.JobServiceBase innerService;
            public Ruanal.Core.DispatchArg dispatchArg;
            public DateTime? lastRunTime { get; set; }
            public string RunGuid { get; set; }
            public string InstanceId { get; set; }
            public bool IsRunning { get; set; }
            public System.Threading.Thread RunThread { get; set; }
        }
        public int TaskId { get { return TaskDetail.TaskId; } }
        public int TaskVersion { get; set; }
        public Ruanal.Core.ApiSdk.TaskDetail TaskDetail;
        public int JobType { get { return TaskDetail.TaskType; } }
        // public string RunGuid { get; private set; }
        public Ruanal.ServiceSchedule.JobContext JobContext { get; set; }

        public List<ServiceAndDispatch> innerServices;
        #endregion
        public ServiceDomainItem(Ruanal.Core.ApiSdk.TaskDetail config)
        {
            innerServices = new List<ServiceAndDispatch>();
            TaskDetail = config;
            _MakeLocalFile();
            _CreateDomain();
            _TestOK();
        }

        private void _TestOK()
        {

            try
            {
                var x = GetFree();
                _StopServiceInstance(x, false);
            }
            catch (Exception ex)
            {
                var guid = "Check:" + TaskId + "-" + Guid.NewGuid().ToString().Replace("-", "");
                Ruanal.Core.ApiSdk.SystemApi.TaskBeginRunLog(TaskId, guid, Core.ApiSdk.TaskRunType.Plan);
                Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, guid, false, ex.Message);
            }
        }
        #region 接口实现
        /// <summary>
        /// 仅 任务类型为 基本任务（非调度任务）进会 从该方法进入执行
        /// </summary>
        /// <param name="context"></param>
        public void Execute(ServiceSchedule.JobContext context)
        {
            ServiceAndDispatch service = GetFree();
            if (service == null)
                return;
            service.dispatchArg = null;
            DoServiceStart(service, false);
        }
        #endregion

        #region 私有方法

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
            string dllname = dirtask.TrimEnd('\\') + "\\" + TaskDetail.EnterDll;
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
            try
            {
                TaskVersion = RLib.Utils.Converter.StrToInt(System.IO.File.ReadAllText(versionFile)
                    );
            }
            catch { }
            _dllFullName = dllname;
            _taskDir = dirtask;
        }

        private void _CreateDomain()
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = _taskDir;
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
            setup.ShadowCopyFiles = "true";
            AppDomain.MonitoringIsEnabled = true;
            _taskDomain = AppDomain.CreateDomain("task_" + TaskDetail.TaskId, null, setup);
            Core.DomainAssembleLoader.ConfigLoad(_taskDomain);
            var taskConfig = BuildTaskConfig(TaskDetail.TaskConfig);
            this._instanceCanReUse = true;
            //taskConfig.ContainsKey("_reuseable") ? taskConfig["_reuseable"].ToLower() == "true" : false;
            if (JobType == 1)
            {
                var instanceCount = Ruanal.Core.ConfigConst.DispatchDefaultInstanceCount;
                if (taskConfig.ContainsKey(Ruanal.Job.ConstKey.Dis_InstanceCountKey))
                {
                    instanceCount = RLib.Utils.Converter.StrToInt(taskConfig[Ruanal.Job.ConstKey.Dis_InstanceCountKey]);
                }
                if (instanceCount <= 0)
                    instanceCount = Ruanal.Core.ConfigConst.DispatchDefaultInstanceCount;
                _maxInstanceCount = instanceCount;
            }
        }

        private ServiceAndDispatch BuildServiceInstance()
        {
            var taskConfig = BuildTaskConfig(TaskDetail.TaskConfig);
            var nolink = Ruanal.Job.JobProxy.IsNoLink(_dllFullName, TaskDetail.EnterClass);
            string fname = _dllFullName, fclass = TaskDetail.EnterClass;
            if (nolink)
            {
                fname = Path.Combine(_jobBinDir, "Ruanal.Job.dll");
                fclass = "Ruanal.Job.JobProxy";
            }
            var obj_serviceinstance = _taskDomain.CreateInstanceFromAndUnwrap(fname, fclass);
            if (obj_serviceinstance is Ruanal.Job.JobServiceBase)
            {
                var currinstance = obj_serviceinstance as Ruanal.Job.JobServiceBase;
                string InstanceId = Guid.NewGuid().ToString().Replace("-", "");
                currinstance.ParentCaller = new Job.ParentCaller(InstanceId, JobItem_Caller);
                currinstance.Loger = new Job.CrossLoger(InstanceId, ServiceLogHandler);
                currinstance.GlobalInit(RLib.Utils.DataSerialize.SerializeJson(taskConfig));
                currinstance.Init();
                var sad = new ServiceAndDispatch()
                {
                    innerService = currinstance,
                    InstanceId = InstanceId
                };
                if (nolink)
                {
                    ((Ruanal.Job.JobProxy)obj_serviceinstance).InitProxy(TaskDetail.EnterDll, TaskDetail.EnterClass);
                }
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                return sad;
            }
            else
            {
                throw new Exception(string.Format("类型{0}不是{1}类型", TaskDetail.EnterClass, typeof(Ruanal.Job.JobServiceBase).Name));
            }
        }


        private Dictionary<string, string> BuildTaskConfig(string currTaskConfig)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var a in Ruanal.Core.Config.NodeConfig)
            {
                dic[a.Key] = a.Value;
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(currTaskConfig))
                {
                    var tdic = RLib.Utils.DataSerialize.DeserializeObject<Dictionary<string, string>>(currTaskConfig);
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


        private bool ServiceLogHandler(string instanceId, string msg, int type, bool isAsyn)
        {
            //ServiceAndDispatch currinstance = null;
            string runguid = "";
            lock (servicelock)
            {
                var currinstance = innerServices.FirstOrDefault(x => x.InstanceId == instanceId);
                if (currinstance != null)
                {
                    runguid = currinstance.RunGuid;
                }
            }
            return Ruanal.Core.ServerLog.AddWorkLog(TaskId, runguid, type, msg, isAsyn);
        }

        private void DoServiceStart(ServiceAndDispatch service, bool canThrow)
        {
            service.RunGuid = service.dispatchArg == null ? Guid.NewGuid().ToString().Replace("-", "") : service.dispatchArg.InvokeId;
            Ruanal.Core.ApiSdk.SystemApi.TaskBeginRunLog(TaskId, service.RunGuid,
                 service.dispatchArg == null ? Core.ApiSdk.TaskRunType.Plan :
                Core.ApiSdk.TaskRunType.ExeDispatch);
            try
            {
                _StartServiceInstance(service);
                var arg = service.dispatchArg == null ? null : service.dispatchArg.RunArgs;
                service.innerService.Start(arg);

                if (!service.innerService.IsThreadJob)
                {
                    service.RunThread = null;
                    Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, service.RunGuid, true, "");
                    _StopServiceInstance(service, false);
                    //分配执行完，通知取新任务
                    if (service.dispatchArg != null && OnWockComplet != null)
                        OnWockComplet.Invoke();
                }
            }
            catch (Exception ex)
            {
                service.RunThread = null;
                Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, service.RunGuid, false, ex.Message);
                try
                {
                    _StopServiceInstance(service, false);
                }
                catch
                {
                    if (canThrow)
                        throw;
                }
                if (canThrow)
                    throw;
            }
        }

        /// <summary>
        /// 线程任务完成
        /// </summary>
        /// <param name="instanceID"></param>
        private object JobItem_Caller(string instanceID, string callType, object[] args)
        {
            ServiceAndDispatch currinstance = null;
            try
            {
                lock (servicelock)
                {
                    currinstance = innerServices.FirstOrDefault(x => x.InstanceId == instanceID);
                    if (currinstance == null)
                        return false;
                }
                //回调 线程任务完成
                if (callType == Job.ParentCaller.CallType_EndJob)
                {
                    if (currinstance.IsRunning == false || currinstance.innerService.IsThreadJob == false)
                        return false;
                    //完成日志
                    Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, currinstance.RunGuid, true, "");
                    _StopServiceInstance(currinstance, false);
                    return true;
                }
                if (callType == Job.ParentCaller.CallType_TaskConfig)
                {
                    var rtaskdetail = Ruanal.Core.ApiSdk.TaskApi.GetTaskDetail(TaskId);
                    var newconfig = BuildTaskConfig(rtaskdetail.code > 0 ? rtaskdetail.data.TaskConfig : TaskDetail.TaskConfig);
                    return RLib.Utils.DataSerialize.SerializeJsonBeauty(newconfig);
                }

                return false;
            }
            catch (Exception ex)
            {
                RLib.WatchLog.Loger.Error("任务设用父级方法出错 任务ID:" + TaskId + " callType:" + callType, ex);
                return false;
            }
        }

        #endregion

        #region 公有方法

        public int GetFreeCount()
        {
            lock (servicelock)
            {
                int count = innerServices.Count(x => x.IsRunning == false);
                count += _maxInstanceCount - innerServices.Count;
                return count;
            }
        }

        bool signStop = false;
        public void Stop()
        {
            signStop = true;
            lock (servicelock)
            {
                foreach (var a in innerServices)
                {
                    _StopServiceInstance(a, true);
                }
                AppDomain.Unload(_taskDomain);
            }
        }

        public bool StopDispatch(string runguid)
        {
            lock (servicelock)
            {
                foreach (var a in innerServices)
                {
                    if (a.RunGuid == runguid)
                    {
                        _StopServiceInstance(a, false);
                        return true;
                    }
                }
                return false;
            }
        }

        private void _StopServiceInstance(ServiceAndDispatch a, bool dispose)
        {
            if (a.innerService != null)
            {
                a.innerService.Stop();
                if (dispose)
                {
                    a.innerService.Loger = null;
                    a.innerService.ParentCaller = null;
                    a.innerService.Dispose();
                }
            }
            if (a.RunThread != null)
            {
                if (a.RunThread.IsAlive)
                    a.RunThread.Abort();
                a.RunThread = null;
            }
            a.RunGuid = "";
            a.IsRunning = false;
        }


        private void _StartServiceInstance(ServiceAndDispatch service)
        {
            service.RunThread = System.Threading.Thread.CurrentThread;
            service.lastRunTime = DateTime.Now;
        }

        private ServiceAndDispatch GetFree()
        {
            ServiceAndDispatch service = null;
            lock (servicelock)
            {
                service = innerServices.FirstOrDefault(x => x.IsRunning == false);
                if (_instanceCanReUse == false && service != null)
                {
                    //不能德用的要关闭 删除
                    _StopServiceInstance(service, true);
                    innerServices.Remove(service);
                    service = null;
                }
                if (service == null)
                {
                    if (innerServices.Count < this._maxInstanceCount)
                    {
                        service = BuildServiceInstance();
                        service.IsRunning = true;
                        innerServices.Add(service);
                    }
                }
                else
                {
                    _StopServiceInstance(service, false);
                    service.IsRunning = true;
                    return service;
                }
            }
            return service;
        }
        /// <summary>
        /// 调度任务开始执行
        /// </summary>
        /// <param name="arg"></param>
        public int StartDispatch(Ruanal.Core.DispatchArg arg, Func<bool> begin)
        {
            if (signStop)
                return -1;
            ServiceAndDispatch service = GetFree();
            if (service == null)
                return -1;
            try
            {
                if (begin())
                {
                    service.dispatchArg = arg;
                    DoServiceStart(service, true);
                    return 1;
                }
                else
                {
                    _StopServiceInstance(service, false);
                    return 0;
                }
            }
            catch (Exception)
            {
                _StopServiceInstance(service, false);
                throw;
            }
        }

        /// <summary>
        /// 判断分配任务Key是不是正在运行
        /// </summary>
        /// <param name="runkey"></param>
        /// <returns></returns>
        public bool IsRuningKey(string runkey, out int[] dispatchIds)
        {
            lock (servicelock)
            {
                var r = innerServices
                    .Where(x => x.dispatchArg != null && x.IsRunning == true && x.dispatchArg.RunKey == runkey)
                    .ToArray();
                foreach (var a in r)
                {
                    if (a.IsRunning && a.RunThread != null && a.RunThread.IsAlive == false)
                    {
                        try { _StopServiceInstance(a, false); } catch { }
                    }
                }
                dispatchIds = r.Select(x => x.dispatchArg.DispatchId).ToArray();
                return r.Count() > 0;
            }
        }

        public List<string> InstanceSummary()
        {
            var results = new List<string>();
            lock (servicelock)
            {
                int i = 0;
                foreach (var a in innerServices)
                {
                    string msgf = "[" + i + "]";
                    try
                    {
                        msgf += a.IsRunning ? "[Running]" : "[Free]";
                        if (a.RunThread != null)
                        {
                            msgf += (a.RunThread.IsAlive ? "[IsAlive]" : "[NotAlive]") + "[" + a.RunThread.ThreadState.ToString() + "]";
                        }
                        if (a.lastRunTime != null)
                        {
                            msgf += " [" + a.lastRunTime.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]";
                        }

                        msgf += " [" + GetMemoryMB() + "]";
                    }
                    catch (Exception ex)
                    {
                        msgf += " [Rrror]";
                    }
                    results.Add(msgf);
                    i++;
                }
            }
            return results;
        }

        public string GetMemoryMB()
        {
            if (_taskDomain == null) return string.Empty;
            double use = _taskDomain.MonitoringSurvivedMemorySize / 1024 / 1024;
            double allall = _taskDomain.MonitoringTotalAllocatedMemorySize / 1024 / 1024;
            return string.Format("{0}/{1}MB", use, allall);
        }

        #endregion


    }
}
