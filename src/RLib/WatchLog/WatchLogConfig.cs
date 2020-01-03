using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RLib.WatchLog
{
    public class WatchLogConfig
    {
        //常量
        public const string CONFIG_WatchLog_ProjectGroup = "watchlog:ProjectGroup";
        public const string CONFIG_WatchLog_BatchSize = "watchlog:BatchSize";
        public const string CONFIG_WatchLog_StackSize = "watchlog:WriteStackSize";
        public const string CONFIG_WatchLog_TimeOutSeconds = "watchlog:TimeOutSeconds";
        public const string CONFIG_WatchLog_LogerType = "watchlog:LogerType";
        public const string CONFIG_WatchLog_ProjectName = "watchlog:ProjectName";
        public const string CONFIG_WatchLog_WriteBlock = "watchlog:WriteBlock";
        public const string CONFIG_WatchLog_OpenSqlWatch = "watchlog:OpenSqlWatch";
        public const string CONFIG_WatchLog_DateTimeFormat = "watchlog:DateTimeFormat";
        public const string CONFIG_WatchLog_FilePath = "watchlog:FileLogPath";
        public const string CONFIG_WatchLog_FileUnion = "watchlog:FileUnion";
        public const string CONFIG_WatchLog_DBConnString = "watchlog:DBConnString";
        public const string CONFIG_WatchLog_ApiLogUrl = "watchlog:ApiLogUrl";
        public const string CONFIG_WatchLog_ApiLogSecret = "watchlog:ApiLogSecret";
        private string ConfigFileName = string.Empty;
        //变量
        public int BatchSize { get; private set; }
        public int MaxStackSize { get; private set; }
        public int TimeOutSeconds { get; private set; }
        public LogerType LogerType { get; private set; }
        public bool OpenSqlWatch { get; private set; }
        public string ProjectGroup { get; private set; }
        public string ProjectName { get; private set; }
        public bool WriteBlock { get; private set; }
        public string DateTimeFormat { get; private set; }
        public string FilePath { get; set; }
        public bool FileUnion { get; set; }
        public string DBConnString { get; set; }

        public string ApiLogUrl { get; set; }
        public string ApiLogSecret { get; set; }
        public event Action OnChange;

        public WatchLogConfig()
        {
            BatchSize = 100;
            MaxStackSize = 1000;
            TimeOutSeconds = 8;
            LogerType = LogerType.None;
            OpenSqlWatch = false;
            ProjectName = string.Empty;
            ProjectGroup = string.Empty;
            WriteBlock = false;
            DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
            FilePath = "~/RWatchLog";
            FileUnion = false;
            DBConnString = string.Empty;
        }

        private void LoadConfig()
        {
            #region size
            string size = ConfigHelper.GetAppConfig(CONFIG_WatchLog_BatchSize);
            if (!string.IsNullOrEmpty(size))
            {
                int tsize = Utils.Converter.StrToInt(size);
                if (tsize > 0)
                    BatchSize = tsize;
            }
            #endregion

            #region size
            string stacksize = ConfigHelper.GetAppConfig(CONFIG_WatchLog_StackSize);
            if (!string.IsNullOrEmpty(stacksize))
            {
                int tstacksize = Utils.Converter.StrToInt(stacksize);
                if (tstacksize > 0)
                    MaxStackSize = tstacksize;
            }
            #endregion
            #region timeout
            string timeout = ConfigHelper.GetAppConfig(CONFIG_WatchLog_TimeOutSeconds);
            if (!string.IsNullOrEmpty(timeout))
            {
                int ttimeout = Utils.Converter.StrToInt(timeout);
                if (ttimeout > 0)
                    TimeOutSeconds = ttimeout;
            }
            #endregion

            #region logertype
            string logerType = ConfigHelper.GetAppConfig(CONFIG_WatchLog_LogerType, "none");
            switch (logerType.ToLower())
            {
                case "fileloger":
                    LogerType = LogerType.FileLog;
                    break;
                case "dbloger":
                    LogerType = LogerType.DBLog;
                    break;
                case "consoleloger":
                    LogerType = LogerType.ConsoleLog;
                    break;
                case "apiloger":
                    LogerType = LogerType.ApiLog;
                    break;
                case "none":
                    LogerType = LogerType.None;
                    break;
                default:
                    LogerType = LogerType.None;
                    break;
            }
            #endregion

            #region projectname
            ProjectGroup = ConfigHelper.GetAppConfig(CONFIG_WatchLog_ProjectGroup, "");
            ProjectName = ConfigHelper.GetAppConfig(CONFIG_WatchLog_ProjectName, "未配置项目名");
            #endregion

            #region OpenTimeWatch
            OpenSqlWatch = ConfigHelper.GetAppConfig(CONFIG_WatchLog_OpenSqlWatch, "false").ToLower() == "true";
            #endregion

            #region writeNoBlock
            WriteBlock = ConfigHelper.GetAppConfig(CONFIG_WatchLog_WriteBlock, "true").ToLower() == "true";
            #endregion
            DateTimeFormat = ConfigHelper.GetAppConfig(CONFIG_WatchLog_DateTimeFormat, "yyyy-MM-dd HH:mm:ss.fff");

            FilePath = ConfigHelper.GetAppConfig(CONFIG_WatchLog_FilePath, "~/WatchLog");
            FileUnion = ConfigHelper.GetAppConfig(CONFIG_WatchLog_FileUnion, "false") == "true";
            DBConnString = ConfigHelper.GetAppConfig(CONFIG_WatchLog_DBConnString, "");
            
            ApiLogUrl = ConfigHelper.GetAppConfig(CONFIG_WatchLog_ApiLogUrl, "");
            ApiLogSecret = ConfigHelper.GetAppConfig(CONFIG_WatchLog_ApiLogSecret, "");
        }
        private void StartMonitor()
        {
            var configInfo = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
            ConfigFileName = configInfo.FilePath;
            string filename = new System.IO.FileInfo(configInfo.FilePath).FullName;
            FileSystemWatcher fsw = new FileSystemWatcher(AppDomain.CurrentDomain.BaseDirectory, "*.config");
            fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            fsw.Created += OnConfig_Change;
            fsw.Changed += OnConfig_Change;
            fsw.EnableRaisingEvents = true;
            fsw.IncludeSubdirectories = false;
        }

        private void OnConfig_Change(object sender, FileSystemEventArgs e)
        {
            if (!System.IO.File.Exists(ConfigFileName))
            {
                return;
            }
            try { System.Configuration.ConfigurationManager.RefreshSection("appSettings"); }
            catch (Exception ex) { return; }
            LoadConfig();
            if (OnChange != null)
                OnChange.Invoke();
        }


        public void Init()
        {
            LoadConfig();
            StartMonitor();
        }
    }

    public enum LogerType
    {
        None,
        FileLog,
        DBLog,
        ConsoleLog,
        ApiLog
    }
}
