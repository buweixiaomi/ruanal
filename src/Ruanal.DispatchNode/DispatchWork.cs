using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ruanal.ServiceSchedule;
using System.Reflection;

namespace Ruanal.DispatchNode
{
    public class DispatchWork : ServiceSchedule.IJobExecutor
    {
        public JobContext JobContext { get; set; }
        public bool IsRunning { get; private set; }
        public int TaskId { get; private set; }
        public int TaskVersion { get; private set; }
        public Ruanal.Core.ApiSdk.TaskDetail TaskDetail;
        Ruanal.Job.DispatcherBase _dispatchbase;
        public string GroupId { get; private set; }
        public string RunGuid { get; private set; }
        System.Threading.Thread _checkThread;
        public DispatchWork(Ruanal.Core.ApiSdk.TaskDetail taskdetail)
        {
            this.TaskDetail = taskdetail;
            TaskId = this.TaskDetail.TaskId;
            if (string.IsNullOrEmpty(TaskDetail.DispatchClass))
            {
                _dispatchbase = new EntDispatcher();
                TaskVersion = taskdetail.CurrVersionId;
            }
            else
            {
                _dispatchbase = GetDispatcher();
                if (_dispatchbase == null)
                {
                    throw new Exception("调度器初始化失败！");
                }
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(TaskDetail.TaskConfig))
                    _dispatchbase.TaskConfig = RLib.Utils.DataSerialize.DeserializeObject<Dictionary<string, string>>(TaskDetail.TaskConfig);
                else
                    _dispatchbase.TaskConfig = new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                throw new Exception("任务配置json出错！");
            }
        }

        public void Execute(JobContext context)
        {
            if (IsRunning)
                return;
            IsRunning = true;
            RunGuid = Guid.NewGuid().ToString().Replace("-", "");
            Ruanal.Core.ApiSdk.SystemApi.TaskBeginRunLog(TaskId, RunGuid, Core.ApiSdk.TaskRunType.DoDispatch);

            var dispatchconfigs = _dispatchbase.GetDispatchs();
            var t_groupId = Guid.NewGuid().ToString().Replace("-", "");
            var result = Ruanal.Core.ApiSdk.TaskApi.BuildDispatch(TaskId, t_groupId, dispatchconfigs);
            if (result.code <= 0)
            {
                IsRunning = false;
            }
            else
            {
                GroupId = t_groupId;
                CheckState();
            }
        }

        private void CheckState()
        {
            if (_checkThread != null)
                _checkThread.Abort();
            _checkThread = new System.Threading.Thread(() =>
            {
                while (true)
                {
                    var checkresult = Ruanal.Core.ApiSdk.TaskApi.CheckDispatchGroup(GroupId);
                    if (checkresult.code > 0 && checkresult.data == 0)
                    {
                        IsRunning = false;
                        Ruanal.Core.ApiSdk.SystemApi.TaskEndRunLog(TaskId, RunGuid, true, "");
                        break;
                    }
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(Ruanal.Core.ConfigConst.Dispatch_Check_Seconds));
                }
            });
            _checkThread.IsBackground = true;
            _checkThread.Start();
        }

        private Ruanal.Job.DispatcherBase GetDispatcher()
        {
            Ruanal.Job.DispatcherBase item = null;
            bool existdll = false;
            string dirtask = Ruanal.Core.Config.GetTaskItemDir(TaskDetail.TaskId);
            string dllname = dirtask.TrimEnd('\\') + TaskDetail.EnterDll;
            string versionFile = System.IO.Path.Combine(dirtask, Ruanal.Core.ConfigConst.TaskVersionFileName);
            if (System.IO.File.Exists(dllname))
            {
                existdll = true;
            }
            else
            {
                var file = Ruanal.Core.ApiSdk.SystemApi.DownloadFile2(TaskDetail.FileUrl);
                if (file.code > 0)
                {
                    string filename = Ruanal.Core.Config.GetTempFileName();
                    System.IO.File.WriteAllBytes(filename, file.data);
                    Ruanal.Core.Utils.Utils.UnZip(filename, dirtask, "", true);
                    System.IO.File.WriteAllText(versionFile, TaskDetail.CurrVersionId.ToString());
                    Ruanal.Core.Utils.Utils.DeleteFileOrDir(filename);
                    if (System.IO.File.Exists(dllname))
                    {
                        existdll = true;
                    }
                }
            }
            if (existdll == false)
                return null;
            //当前版本
            try
            {
                TaskVersion = RLib.Utils.Converter.StrToInt(
                    System.IO.File.ReadAllText(versionFile)
                    );
            }
            catch { }
            var dllas = Assembly.LoadFile(dllname);
            var typeofdis = dllas.GetType(TaskDetail.DispatchClass, false, true);
            if (typeofdis != null)
            {
                object instance = Activator.CreateInstance(typeofdis);
                item = instance as Ruanal.Job.DispatcherBase;
            }
            return item;
        }
    }
}
