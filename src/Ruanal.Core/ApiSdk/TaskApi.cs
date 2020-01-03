using Ruanal.Core.ApiSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.Core.ApiSdk
{
    public class TaskApi
    {
        public static ApiResult<TaskDetail> GetTaskDetail(int taskid)
        {
            return SdkCore.InvokeApi<TaskDetail>(ConfigConst.API_TASK_DETAIL, new { taskid = taskid });
        }

        public static ApiResult<List<TaskDetail>> GetAllTask()
        {
            return SdkCore.InvokeApi<List<TaskDetail>>(ConfigConst.API_TASK_GETALL, null);
        }



        public static ApiResult<object> BuildDispatch(int taskId, string groupId, string runconfigs)
        {
            var v = SdkCore.InvokeApi<object>(ConfigConst.API_TASK_BUILDDISPATCHS, new
            {
                taskId = taskId,
                groupId = groupId,
                runconfigs = runconfigs
            });
            return v;
        }

        public static ApiResult<int> CheckDispatchGroup(string groupId)
        {
            var v = SdkCore.InvokeApi<int>(ConfigConst.API_TASK_CHECKDISPATCHGROUP, new
            {
                groupId = groupId
            });
            return v;
        }
    }
}
