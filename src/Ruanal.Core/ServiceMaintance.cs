using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;

namespace Ruanal.Core
{
    /// <summary>
    /// 使用要求：使用该类自身需要为可执行性的Windows服务（*.exe)
    /// </summary>
    public class ServiceMaintance
    {
        public const string RestartCMDName = "-restartservice";
        /// <summary>
        /// 返回true时为维护命令
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static bool CheckMaintanceCmd(string[] arg)
        {
            if (arg != null && arg.Length == 1)
            {
                arg = arg[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            WriteLog("arg=" + (arg == null ? "[null]" : string.Join(" ", arg)));
            if (arg == null || arg.Length < 2 || arg[0].ToLower() != RestartCMDName)
                return false;
            int processId = 0;
            int.TryParse(arg[1].Trim(), out processId);
            DoRestart(processId);
            return true;
        }

        public static void DoRestart(int processId)
        {
            var process = System.Diagnostics.Process.GetProcessById(processId);
            var serviceName = GetServiceName(processId);
            if (!string.IsNullOrEmpty(serviceName))
            {
                //重启服务
                var sc = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == serviceName);
                if (sc == null)
                {
                    WriteLog("服务名" + serviceName + "不存在，无法重启！");
                    return;
                }
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    WriteLog("正在停止服务...");
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(15)); //等待服务停止
                    if (process != null)
                    {
                        try { process.Kill(); }
                        catch { }
                    }
                    WriteLog("已停止服务");
                }
                WriteLog("正在启动...");
                try
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5)); //等待服务启动
                    WriteLog("启动完成");
                }
                catch (Exception ex)
                {
                    WriteLog("启动出错." + ex.Message);
                }
            }
            else
            {
                //重启一般程序
                if (process != null)
                {
                    WriteLog("正在停止程序...");
                    try
                    {
                        process.Kill();
                        WriteLog("已停止程序");
                    }
                    catch (Exception ex)
                    {
                        WriteLog("停止程序出错：" + ex.Message);
                        return;
                    }
                }
                WriteLog("正在启动...");
                string location = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                System.Diagnostics.Process.Start(location);
                WriteLog("启动完成");
            }
        }

        public static void SendRestartCmd()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var processId = process.Id;
            string location = process.MainModule.FileName;
            Console.WriteLine(location);
            System.Diagnostics.Process.Start(location, RestartCMDName + " " + processId);
        }

        public static string GetServiceName(int processId)
        {
            string serviceName = string.Empty;
            try
            {
                ConnectionOptions options = new ConnectionOptions();
                options.Impersonation = System.Management.ImpersonationLevel.Impersonate;
                ManagementScope scope = new ManagementScope("\\\\localhost\\root\\cimv2", options);
                scope.Connect();
                using (var svc = new ManagementObjectSearcher(scope, new ObjectQuery("Select Name,DisplayName,ProcessId,State from Win32_Service where ProcessId=" + processId + " ")))
                {
                    var tbs = svc.Get();
                    foreach (var a in tbs)
                    {
                        serviceName = a["Name"].ToString();
                        break;
                    }
                }
            }
            catch (Exception) { }
            return serviceName;
        }

        public static void WriteLog(string msg)
        {
            try
            {
                var filefullname = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "maintance.log");
                string writemsg = string.Format("[{0}]{1}{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), msg, Environment.NewLine);
                System.IO.File.AppendAllText(filefullname, writemsg, Encoding.UTF8);
            }
            catch { }
        }
    }
}
