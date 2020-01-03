using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.Core
{
    public class Config
    {
        public static Dictionary<string, string> NodeConfig = new Dictionary<string, string>();
        public static List<ApiSdk.TaskDetail> TaskConfig = new List<ApiSdk.TaskDetail>();
        public static string NodeType = "Unknow";
        public static string ClientID = "";
        public static string IPS = "";
        public static string MACS = "";
        private static object _unionConfigLocker = new object();
        private static object _taskConfigLocker = new object();
        public static string GetSystemConfig(string key, string defaultv)
        {
            string value = System.Configuration.ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
                return defaultv;
            return value;
        }

        public static void StoreNodeConfig()
        {
            lock (_unionConfigLocker)
            {
                string json = Utils.Utils.SerializeObject(NodeConfig);
                string filepath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigConst.NodeConfigFileName);
                System.IO.File.WriteAllText(filepath, json, Encoding.UTF8);
            }
        }

        public static void ResumeNodeConfig()
        {
            lock (_unionConfigLocker)
            {
                string filepath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigConst.NodeConfigFileName);
                if (System.IO.File.Exists(filepath))
                {
                    try
                    {
                        NodeConfig = Utils.Utils.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(filepath, Encoding.UTF8));
                    }
                    catch { }
                    if (NodeConfig == null)
                        NodeConfig = new Dictionary<string, string>();
                }
            }
        }

        public static string GetTempFileName()
        {
            string tempdir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\temp";
            if (!System.IO.Directory.Exists(tempdir))
                System.IO.Directory.CreateDirectory(tempdir);
            string fillename = tempdir + "\\" + Guid.NewGuid().ToString().Replace("-", "") + ".tmp";
            return fillename;
        }

        public static string GetTaskDir()
        {
            string tempdir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\" + ConfigConst.TaskDllDirName.TrimStart('\\');
            if (!System.IO.Directory.Exists(tempdir))
                System.IO.Directory.CreateDirectory(tempdir);
            return tempdir;
        }

        public static string GetTempDirName()
        {
            string tempdir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\temp";
            if (!System.IO.Directory.Exists(tempdir))
                System.IO.Directory.CreateDirectory(tempdir);
            string fillename = tempdir + "\\" + Guid.NewGuid().ToString().Replace("-", "") + "\\";
            return fillename;
        }

        public static string BuildClientId()
        {
            string[] macAddress = null;
            string[] ips = null;
            Ruanal.Core.Utils.Utils.GetIpsAndMacs(out ips, out macAddress);
            Config.MACS = string.Join(",", macAddress);//这里
            Config.IPS = string.Join(",", ips);//这里
            string s = System.Environment.MachineName + AppDomain.CurrentDomain.BaseDirectory.ToLower();
            string clientid = RLib.Utils.Security.MakeMD5(s);
            string filefullname = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Ruanal.Core.ConfigConst.ClientIdFileName);
            System.IO.File.WriteAllText(filefullname, clientid);
            Config.ClientID = clientid;//这里
            return clientid;
        }


        public static void StoreTaskConfig()
        {
            lock (_taskConfigLocker)
            {
                string json = Utils.Utils.SerializeObject(TaskConfig);
                string filepath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigConst.TaskConfigFileName);
                System.IO.File.WriteAllText(filepath, json, Encoding.UTF8);
            }
        }

        public static void ResumeTaskConfig()
        {
            List<ApiSdk.TaskDetail> taskConfig = null;
            lock (_taskConfigLocker)
            {
                string filepath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigConst.TaskConfigFileName);
                if (System.IO.File.Exists(filepath))
                {
                    try
                    {
                        taskConfig = Utils.Utils.DeserializeObject<List<ApiSdk.TaskDetail>>(System.IO.File.ReadAllText(filepath, Encoding.UTF8));
                    }
                    catch { }
                    if (TaskConfig == null)
                        taskConfig = new List<ApiSdk.TaskDetail>();
                    TaskConfig = taskConfig;
                }
            }
        }

        public static string GetTaskItemDir(int taskId)
        {
            string dirtask = Ruanal.Core.Config.GetTaskDir().TrimEnd('\\') + "\\task_" + taskId + "\\";
            return dirtask;
        }

    }
}
