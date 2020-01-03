using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.Job
{
    public delegate bool CrossLogerHandler(string instanceId, string msg, int logType, bool isAsyn);
    public class CrossLoger : MarshalByRefObject
    {
        private CrossLogerHandler LogHandler;
        public string InstanceId { get; private set; }
        public CrossLoger(string instanceId, CrossLogerHandler logHandler)
        {
            InstanceId = instanceId;
            LogHandler = logHandler;
        }

        /// <summary>
        /// 添加日志，返回添加结果
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool AddLog(string msg)
        {
            return LogHandler.Invoke(InstanceId, msg, 0, false);
        }

        /// <summary>
        /// 添加日志，返回添加结果
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool AddLog(string format, params object[] args)
        {
            return LogHandler.Invoke(InstanceId, string.Format(format, args), 0, false);
        }

        /// <summary>
        /// 添加错误日志
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool AddErrorLog(string msg)
        {
            return LogHandler(InstanceId, msg, 1, false);
        }
        /// <summary>
        /// 添加错误日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool AddErrorLog(string format, params object[] args)
        {
            return LogHandler(InstanceId, string.Format(format, args), 1, false);
        }

        /// <summary>
        /// 异步添加日志
        /// </summary>
        /// <param name="msg"></param>
        public void AddLogAsyn(string msg)
        {
            LogHandler.Invoke(InstanceId, msg, 0, true);
        }

        /// <summary>
        /// 异步添加日志
        /// </summary>
        /// <param name="msg"></param>
        public void AddLogAsyn(string format, params object[] args)
        {
            LogHandler.Invoke(InstanceId, string.Format(format, args), 0, true);
        }

        /// <summary>
        /// 异步添加错误日志
        /// </summary>
        /// <param name="msg"></param>
        public void AddErrorLogAsyn(string msg)
        {
            LogHandler(InstanceId, msg, 1, true);
        }
        /// <summary>
        /// 异步添加错误日志
        /// </summary>
        /// <param name="msg"></param>
        public void AddErrorLogAsyn(string format, params object[] args)
        {
            LogHandler(InstanceId, string.Format(format, args), 1, true);
        }
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
