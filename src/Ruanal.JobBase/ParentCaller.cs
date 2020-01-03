using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Job
{

    public delegate object ParentCallerHandler(string instanceId, string callType, object[] args);
    public class ParentCaller : MarshalByRefObject
    {
        /// <summary>
        /// 线程任务完成
        /// </summary>
        public const string CallType_EndJob = "EndJob";

        public const string CallType_TaskConfig = "TaskConfig";

        public const string CallType_PostDispatch = "PostDispatch";
        public string InstanceId { get; private set; }
        private ParentCallerHandler OnCallerHandler;

        public ParentCaller(string instanceId, ParentCallerHandler handler)
        {
            this.InstanceId = instanceId;
            OnCallerHandler = handler;
        }

        public void ThreadJobEnd()
        {
            if (OnCallerHandler == null)
                return;
            OnCallerHandler.Invoke(InstanceId, CallType_EndJob, null);
        }

        public Dictionary<string, string> ReGetConfig()
        {
            Dictionary<string, string> detail = new Dictionary<string, string>();
            if (OnCallerHandler == null)
                return detail;
            object config = OnCallerHandler.Invoke(InstanceId, CallType_TaskConfig, null);
            if (config == null)
                return detail;
            if (config is string)
            {
                detail = RLib.Utils.DataSerialize.DeserializeObject<Dictionary<string, string>>(config as string);
                return detail;
            }
            return detail;
        }

        public string PostDispatch(List<DispatcherItem> dispatchs)
        {
            if (OnCallerHandler == null)
                return "调用不存在!";
            var reponse = OnCallerHandler.Invoke(this.InstanceId, CallType_PostDispatch, new object[] { dispatchs });
            if (reponse is bool &&(bool)reponse==false)
            {
                return "调用不存在!";
            }
            return reponse as string;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
