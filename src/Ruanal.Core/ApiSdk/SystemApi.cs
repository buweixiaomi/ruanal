using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.Core.ApiSdk
{
    public class SystemApi
    {
        public static ApiResult<PingResult> WorkNodePing(List<int[]> tasksummary, List<int[]> freetasks)
        {
            if (tasksummary == null)
                tasksummary = new List<int[]>();
            if (freetasks == null)
                freetasks = new List<int[]>();
            var v = SdkCore.InvokeApi<PingResult>(ConfigConst.API_SYSTEM_WORKPING, new
            {
                taskState = RLib.Utils.DataSerialize.SerializeJson(tasksummary),
                freeState = RLib.Utils.DataSerialize.SerializeJson(freetasks),
            });
            return v;
        }


        public static ApiResult<PingResult> MasterNodePing(List<int[]> tasksummary)
        {
            if (tasksummary == null)
                tasksummary = new List<int[]>();
            var v = SdkCore.InvokeApi<PingResult>(ConfigConst.API_SYSTEM_MASTERPING, new
            {
                taskState = RLib.Utils.DataSerialize.SerializeJson(tasksummary)
            });
            return v;
        }

        public static ApiResult<List<DispatchArg>> GetDispatchWork(List<int[]> freeState)
        {
            if (freeState == null)
                freeState = new List<int[]>();
            var v = SdkCore.InvokeApi<List<DispatchArg>>(ConfigConst.API_SYSTEM_GETDISPATCHWORK, new
            {
                freeState = RLib.Utils.DataSerialize.SerializeJson(freeState)
            });
            return v;
        }
        public static ApiResult<int> BeginDispatchExecute(int dispatchId)
        {
            var v = SdkCore.InvokeApi<int>(ConfigConst.API_SYSTEM_BEGINDISPATCHEXEC, new { dispatchId = dispatchId });
            return v;
        }

        public static ApiResult<object> EndDispatchExecute(int dispatchId, bool success, string msg)
        {
            var v = SdkCore.InvokeApi<object>(ConfigConst.API_SYSTEM_ENDDISPATCHEXEC, new
            {
                dispatchId = dispatchId,
                success = success ? 1 : 0,
                msg = msg
            });
            return v;
        }
        public static ApiResult<object> SkipDispatchExecute(int dispatchId)
        {
            var v = SdkCore.InvokeApi<object>(ConfigConst.API_SYSTEM_SKIPDISPATCHEXEC, new
            {
                dispatchId = dispatchId
            });
            return v;
        }

        public static ApiResult<object> AutoEndDispatchExecute(int dispatchid)
        {
            var v = SdkCore.InvokeApi<object>(ConfigConst.API_SYSTEM_AUTOENDDISPATCHEXEC, new
            {
                dispatchId = dispatchid
            });
            return v;
        }

        public static ApiResult<Dictionary<string, string>> GetNodeConfig()
        {
            var v = SdkCore.InvokeApi<Dictionary<string, string>>(ConfigConst.API_SYSTEM_GETCONFIG, null);
            return v;
        }

        public static ApiResult<byte[]> DownloadFile(string url)
        {
            return SdkCore.Download(url);
        }
        public static ApiResult<byte[]> DownloadFile2(string url)
        {
            return SdkCore.Download2(url);
        }

        public static ApiResult<object> TaskBeginRunLog(int taskId, string runGuid, TaskRunType runType)
        {
            var v = SdkCore.InvokeApi<object>(ConfigConst.API_SYSTEM_BEGINRUNLOG, new
            {
                taskId = taskId,
                runGuid = runGuid,
                runType = (int)runType,
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            });
            return v;
        }

        public static ApiResult<object> TaskEndRunLog(int taskId, string runGuid, bool success, string msg)
        {
            var v = SdkCore.InvokeApi<object>(ConfigConst.API_SYSTEM_ENDRUNLOG, new
            {
                taskId = taskId,
                runGuid = runGuid,
                resultType = success ? 1 : 2,
                logText = msg ?? "",
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            });
            return v;
        }

        /// <summary>
        /// logType  0:一般日志  1:重要日志  2:错误日志
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="dispatchId"></param>
        /// <param name="logType"> 0:一般日志  1:重要日志  2:错误日志</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ApiResult<object> AddWorkLog(WorkLogEntity log)
        {
            var v = SdkCore.InvokeApi<object>(ConfigConst.API_SYSTEM_ADDWORKLOG, log);
            return v;
        }
        /// <summary>
        /// logType  0:一般日志  1:重要日志  2:错误日志
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="dispatchId"></param>
        /// <param name="logType"> 0:一般日志  1:重要日志  2:错误日志</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ApiResult<object> AddWorkLogs(List<WorkLogEntity> logs)
        {
            var v = SdkCore.InvokeApi<object>(ConfigConst.API_SYSTEM_ADDWORKLOGS, new
            {
                logJson = RLib.Utils.DataSerialize.SerializeJsonWithTimeMs(logs)
            });
            return v;
        }
    }
}
