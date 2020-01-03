using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Core.ApiSdk
{
    public class CommandApi
    {
        public static ApiResult<List<CmdDetail>> GetCommands()
        {
            var v = SdkCore.InvokeApi<List<CmdDetail>>(ConfigConst.API_COMMAND_GETNEWS, null);
            return v;
        }

        public static ApiResult<object> BeginExecute(int cmdid)
        {
            var v = SdkCore.InvokeApi<object>(ConfigConst.API_COMMAND_BEGINEXECUTE, new { cmdid = cmdid });
            return v;
        }

        public static ApiResult<object> EndExecute(int cmdid, bool success, string msg)
        {
            var v = SdkCore.InvokeApi<object>(ConfigConst.API_COMMAND_ENDEXECUTE, new
            {
                cmdid = cmdid,
                success = success ? 1 : 0,
                msg = msg
            });
            return v;
        }
    }
}
