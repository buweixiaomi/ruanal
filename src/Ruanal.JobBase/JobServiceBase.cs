using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading;

namespace Ruanal.Job
{
    public abstract class JobServiceBase : MarshalByRefObject, IDisposable
    {
        //JobLease jobLease;
        public CrossLoger Loger;
        public ParentCaller ParentCaller;
        private JobConfigContainer _jobconfig = new JobConfigContainer();
        public JobConfigContainer JobConfig { get { return _jobconfig; } }
        public string GetConfigJson()
        {
            var json =RLib.Utils.DataSerialize.SerializeJson(JobConfig);
            return json;
        }
        private bool _isThreadJob = false;
        /// <summary>
        /// 是否是线程任务 默认为false，调用Start()后返回，当前工作状态就为停止；如果是线程，则设置该值为ture，
        /// 并内部控制 ThreadJobEnd()或ParentCaller.ThreadJobEnd()
        /// </summary>
        public bool IsThreadJob
        {
            get { return _isThreadJob; }
            set { _isThreadJob = value; }
        }
        protected readonly Dictionary<string, string> TaskConfig = new Dictionary<string, string>();

        #region protected 方法
        /// <summary>
        /// 得到任务配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultv"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public string GetTaskConfig(string key, string defaultv = null, bool ignoreCase = false)
        {
            var item = ignoreCase ? TaskConfig.FirstOrDefault(x => x.Key.ToLower() == key.ToLower()) :
                TaskConfig.FirstOrDefault(x => x.Key == key);
            if (string.IsNullOrEmpty(item.Value) && defaultv != null)
                return defaultv;
            return item.Value;
        }
        #endregion

        #region 公用方法
        /// <summary>
        /// 返回null，不进行对象回收
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            //jobLease = new JobLease();
            //return jobLease;
            return null;
        }




        public void GlobalInit(string parajson)
        {
            if (!string.IsNullOrWhiteSpace(parajson))
            {
                var para = RLib.Utils.DataSerialize.DeserializeObject<Dictionary<string, string>>(parajson);
                foreach (var a in para)
                {
                    TaskConfig[a.Key] = a.Value;
                }
            }
        }

        #endregion

        #region vitual ak abstract 方法

        /// <summary>
        /// 任务对象在启动前调用，仅且只调用一次，下一次在运行时，不调用
        /// </summary>
        public virtual void Init() { }
        /// <summary>
        /// 开始任务
        /// </summary>
        public abstract void Start(string runConfig);

        /// <summary>
        /// 结束任务
        /// </summary>
        public virtual void Stop() { }


        /// <summary>
        /// 如果有必要，请进行有必要的资源释放
        /// </summary>
        public virtual void Dispose()
        {
            try
            {
                this.Stop();
            }
            catch { }
        }
        /// <summary>
        /// 结束线程任务 非线程任务调用无效
        /// </summary>
        public void ThreadJobEnd()
        {
            if (!IsThreadJob)
                return;
            if (ParentCaller != null)
                ParentCaller.ThreadJobEnd();
        }

        #endregion
    }
}
