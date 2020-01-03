using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Core
{
    public class ConfigConst
    {

        public const string NodeConfigFileName = "cacheNodeConfig.json";
        public const string TaskConfigFileName = "cacheTaskConfig.json";
        public const string ClientIdFileName = "clientId.info";
        public const string TaskVersionFileName = "taskversion.info";
        public const string TaskDllDirName = "tasksDLL";
        public const string ServerUrlKeyName = "serverUrl";
        public const string PingSecondsName = "pingSeconds";
        public const string NotifyListenName = "notifyListen";
        public const string NotifyHostListenName = "notifyHostListen";
        public const string DispatchDoListeningName = "dispatchDoListening";
        public const string TalkAskDispatchRunning = "whoisrundispath#";
        public const string TalkTaskInstanceStatus = "taskinstancestatus#";
        public const string TalkNodeTaskStatus = "nodetaskstatus#";
        public const string JobBin = "JobBin";

        public const string ServiceName_Name = "ServiceName";
        public const int PING_TIMESPAN_SECONDS = 10;
        public const int MAX_CACHE_WAITSEND = 500;
        public const int Dispatch_Check_Seconds = 10;
        public const int CMD_Page_Size = 10;
        public const int DispatchDefaultExpireMins = 24 * 60;
        public const int DispatchDefaultInstanceCount = 8;
        public const int MaxCMDParallel = 3;


        public const string API_SYSTEM_GETCONFIG = "/api/apisys/getnodeconfig";
        public const string API_SYSTEM_WORKPING = "/api/apisys/pingworknode";
        public const string API_SYSTEM_MASTERPING = "/api/apisys/pingmasternode";
        public const string API_SYSTEM_BEGINRUNLOG = "/api/apisys/taskbeginrunlog";
        public const string API_SYSTEM_ENDRUNLOG = "/api/apisys/taskendrunlog";
        public const string API_SYSTEM_ADDWORKLOG = "/api/apisys/addworklog";
        public const string API_SYSTEM_ADDWORKLOGS = "/api/apisys/addworklogbatch";
        public const string API_SYSTEM_GETDISPATCHWORK = "/api/apisys/getdispatchwork";
        public const string API_SYSTEM_BEGINDISPATCHEXEC = "/api/apisys/begindispatchexecute";
        public const string API_SYSTEM_ENDDISPATCHEXEC = "/api/apisys/enddispatchexecute";
        public const string API_SYSTEM_SKIPDISPATCHEXEC = "/api/apisys/skipdispatchexecute";
        public const string API_SYSTEM_AUTOENDDISPATCHEXEC = "/api/apisys/autoenddispatchexecute";

        public const string API_TASK_GETALL = "/api/apitask/nodetasks";
        public const string API_TASK_DETAIL = "/api/apitask/taskdetail";
        public const string API_TASK_BUILDDISPATCHS = "/api/apitask/builddispatchs";
        public const string API_TASK_CHECKDISPATCHGROUP = "/api/apitask/checkdispatchgroup";

        public const string API_COMMAND_GETNEWS = "/api/apicmd/getnews";
        public const string API_COMMAND_BEGINEXECUTE = "/api/apicmd/beginexecute";
        public const string API_COMMAND_ENDEXECUTE = "/api/apicmd/endexecute";

        public const string API_UPLOAD_DATA = "/api/config/uploaddata";

        //public const string Task_RangeShopTypeKey = "_RangeShopType";

        public const string CmdType_StartTask = "starttask";
        public const string CmdType_StopTask = "stoptask";
        public const string CmdType_DeleteTask = "deletetask";
        //public const string CmdType_DispatchJob = "dispatchjob";
        public const string CmdType_StopDispatchJob = "stopdispatchjob";
        public const string CmdType_ConfigUpdate = "configupdate";
        public const string CmdType_RestartNode = "restartnode";

        public const string NotifyTopic_NewCmd = "newcmd";
        public const string NotifyTopic_NewDispatch = "newdispatch";
        public const string NotifyTopic_RestartNode = "restartnode";
        // public const string NotifyTopic_DispatchStatus = "dispatchstatus";
        //public const string NotifyTopic_StopDispatchJob = "stopdispatchjob";
    }
}
