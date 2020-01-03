using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Core
{
    public class ServerLog
    {
        public const int WaitSeconds = 10;
        public const int WillWriteCount = 600;
        static System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
        static DateTime _LastRunLogTime = DateTime.MinValue;
        static List<ApiSdk.WorkLogEntity> _cachelog = new List<ApiSdk.WorkLogEntity>();
        static ServerLog()
        {
            System.Threading.Thread t = new System.Threading.Thread(_WriteToApi);
            t.IsBackground = false;
            t.Start();
        }
        public static bool AddWorkLog(int taskId, string dispatchId, int logtype, string msg, bool isAsyn)
        {
            if (isAsyn)
            {
                return AsynWrite(taskId, dispatchId, logtype, msg);
            }
            else
            {
                return DirectWrite(taskId, dispatchId, logtype, msg);
            }
        }

        static bool DirectWrite(int taskId, string dispatchId, int logtype, string msg)
        {
            var logitem = new ApiSdk.WorkLogEntity()
            {
                CreateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                DispatchId = dispatchId,
                TaskId = taskId,
                LogText = msg,
                LogType = logtype
            };
            var v = ApiSdk.SystemApi.AddWorkLog(logitem);
            return v.code > 0 ? true : false;
        }
        static bool AsynWrite(int taskId, string dispatchId, int logtype, string msg)
        {
            var logitem = new ApiSdk.WorkLogEntity()
            {
                CreateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                DispatchId = dispatchId,
                TaskId = taskId,
                LogText = msg,
                LogType = logtype
            };
            lock (_cachelog)
            {
                _cachelog.Add(logitem);
            }
            are.Set();
            return true;
        }

        static void _WriteToApi()
        {
            while (true)
            {
                try
                {
                    DateTime nt = DateTime.Now;
                    if (_cachelog.Count >= WillWriteCount || (_cachelog.Count > 0 && (nt - _LastRunLogTime).TotalSeconds > WaitSeconds))
                    {
                        List<ApiSdk.WorkLogEntity> towrite = null;
                        lock (_cachelog)
                        {
                            towrite = _cachelog.ToList();
                            _cachelog.Clear();
                        }
                        if (towrite.Count > 0)
                        {
                            var v = ApiSdk.SystemApi.AddWorkLogs(towrite);
                        }
                        _LastRunLogTime = nt;
                    }
                }
                catch (Exception ex) { }
                are.WaitOne(WaitSeconds * 1000);
            }
        }


    }
}
