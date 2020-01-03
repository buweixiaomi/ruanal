using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace RLib.WatchLog
{
    public class Loger
    {
        private static AutoResetEvent are = new AutoResetEvent(false);
        private static List<LogEntity> logs = new List<LogEntity>();
        private static object logs_locker = new object();
        private static ILoger innerlog = null;
        private static bool IsAdding = false;
        public static object initLocker = new object();
        private static WatchLogConfig config;
        private static System.Threading.Thread backAddThread = null;
        static Loger()
        {
            try
            {
                config = new WatchLogConfig();
                config.OnChange += _InitLoger;
                config.Init();
            }
            catch (Exception ex) { }
            _InitLoger();
        }

        private static void _InitLoger()
        {
            lock (initLocker)
            {
                if (config.WriteBlock == true && backAddThread == null)
                {
                    //后台写线程
                    backAddThread = new Thread(ThreadAdd);
                    backAddThread.IsBackground = true;
                    backAddThread.Start();
                }
                switch (config.LogerType)
                {
                    case LogerType.FileLog:
                        if (innerlog is FileLoger)
                            break;
                        innerlog = new FileLoger();
                        break;
                    case LogerType.DBLog:
                        if (innerlog is DBLoger)
                            break;
                        innerlog = new DBLoger();
                        break;
                    case LogerType.ConsoleLog:
                        if (innerlog is ConsoleLoger)
                            break;
                        innerlog = new ConsoleLoger();
                        break;
                    case LogerType.ApiLog:
                        if (innerlog is ApiLoger)
                            break;
                        innerlog = new ApiLoger();
                        break;
                    case LogerType.None:
                    default:
                        innerlog = null;
                        break;
                }
                if (innerlog != null)
                    innerlog.Config = config;
            }
        }
        #region 私有方法
        private static void _AddLog(LogEntity log)
        {
            try
            {
                if (innerlog == null)
                    return;
                log.Addition = "";
                log.ProjectGroup = config.ProjectGroup;
                log.ProjectName = config.ProjectName;
                if (System.Web.HttpContext.Current != null)
                {
                    log.InnerGroupID = System.Web.HttpContext.Current.GetHashCode();
                }
                else
                {
                    log.InnerGroupID = Guid.NewGuid().ToString().GetHashCode();
                }
                if (!config.WriteBlock)
                {
                    innerlog.WriteLog(new List<LogEntity>() { log });
                    return;
                }
                lock (logs_locker)
                {
                    if (IsAdding && logs.Count >= config.MaxStackSize)
                    {
                        //如果正在写日志数据，但新的日志又满了，这里新来的日志不添加进去
                        System.Diagnostics.Trace.WriteLine("out stack");
                        return;
                    }
                    logs.Add(log);
                    if (logs.Count >= config.BatchSize)
                    {
                        are.Set();
                    }
                }
            }
            catch (Exception ex) { }
        }
        private static void ThreadAdd()
        {
            while (true)
            {
                if (logs.Count < config.BatchSize)
                {
                    //等待
                    try { are.WaitOne(config.TimeOutSeconds * 1000); }
                    catch (Exception ex) { Thread.Sleep(TimeSpan.FromSeconds(1)); }
                }
                List<LogEntity> waitwrite = null;
                lock (logs_locker)
                {
                    waitwrite = logs.Take(config.BatchSize).ToList();
                    if (waitwrite.Count > 0)
                        logs.RemoveRange(0, waitwrite.Count);
                }
                try
                {
                    IsAdding = true;
                    innerlog.WriteLog(waitwrite);
                }
                catch (Exception ex) { }
                finally { IsAdding = false; }
            }
        }

        #endregion

        #region 公开方法

        public static void Notify()
        {
            are.Set();
        }

        public static int WaitCount()
        {
            return logs.Count;
        }

        public static void Log(string title, string content)
        {
            if (innerlog == null)
                return;
            LogEntity log = new LogEntity();
            log.CreateTime = DateTime.Now;
            log.Title = title ?? "";
            log.Content = content ?? "";
            log.Elapsed = 0;
            log.GroupID = log.Title.GetHashCode();
            log.LogType = 0;
            _AddLog(log);
        }
        public static void Error(string title, string msg)
        {
            if (innerlog == null)
                return;
            LogEntity log = new LogEntity();
            log.CreateTime = DateTime.Now;
            log.Title = title ?? "";
            log.Content = msg ?? "";
            log.Elapsed = 0;
            log.GroupID = log.Title.GetHashCode();
            log.LogType = 2;
            _AddLog(log);
        }

        public static void Error(Exception ex)
        {
            if (innerlog == null)
                return;
            Error(ex.Message, ex);
        }


        public static void Error(string title, Exception ex)
        {
            if (innerlog == null)
                return;
            Error(title, string.Format("【信息】{0} \r\n\t【内部异常】{1} \r\n\t【跟踪】{2}", ex.Message, ex.InnerException, ex.StackTrace));
        }
        public static void TimeWatch(int timewatchtype, string title, string content, double elapsed)
        {
            if (innerlog == null)
            {
                return;
            }
            LogEntity log = new LogEntity();
            log.CreateTime = DateTime.Now;
            log.TimeWatchType = timewatchtype;
            log.Title = title ?? "";
            log.Content = content ?? "";
            log.Elapsed = elapsed;
            log.GroupID = log.Title.GetHashCode();
            log.LogType = 1;
            _AddLog(log);
        }

        public static void TimeWatch(string title, string content, Action act)
        {
            if (innerlog == null)
            {
                act();
                return;
            }
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (act != null)
                    act();
                sw.Stop();
                TimeWatch(1, title, content, sw.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static T TimeWatch<T>(int timewatchtype, string title, string content, Func<T> act)
        {
            if (innerlog == null)
            {
                return act();
            }
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                T r = act.Invoke();
                sw.Stop();
                TimeWatch(timewatchtype, title, content, sw.Elapsed.TotalSeconds);
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void TimeWatchSql(string sql, List<DB.ProcedureParameter> paras, Action act)
        {
            if (innerlog == null)
            {
                act();
                return;
            }

            #region 没有sql监控时，只记错误
            if (config.OpenSqlWatch == false)
            {
                try
                {
                    act();
                    return;
                }
                catch (Exception ex)
                {
                    StringBuilder sb1 = new StringBuilder();
                    sb1.Append("【错误说明】" + ex.Message + ";\r\n\t");
                    sb1.AppendFormat("【sql】:{0};", sql);
                    if (paras != null && paras.Count > 0)
                    {
                        sb1.AppendFormat("\r\n\t【参数】:{0};", string.Join(",", paras.Select(x => "@" + x.Name + ":" + Utils.Converter.NullToStr(x.Value) + "  ")));
                    }
                    else
                    {
                        sb1.AppendFormat("\r\n\t【参数】:{0};", "[无]");
                    }
                    Error("sql错误[" + sql.GetHashCode() + "]", sb1.ToString());
                    throw;
                }
            }
            #endregion

            string title = "sql耗时[" + sql.GetHashCode() + "]";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("【sql】:{0};", sql);
            if (paras != null && paras.Count > 0)
            {
                sb.AppendFormat("\r\n\t【参数】:{0};", string.Join(",", paras.Select(x => "@" + x.Name + ":" + Utils.Converter.NullToStr(x.Value) + "  ")));
            }
            else
            {
                sb.AppendFormat("\r\n\t【参数】:{0};", "[无]");
            }
            try
            {
                TimeWatch(title, sb.ToString(), act);
            }
            catch (Exception ex)
            {
                title = "sql错误[" + sql.GetHashCode() + "]";
                sb.Insert(0, "【错误说明】" + ex.Message + ";\r\n\t");
                Error(title, sb.ToString());
                throw ex;
            }
        }

        public static T TimeWatchSql<T>(string sql, List<DB.ProcedureParameter> paras, Func<T> act)
        {
            if (innerlog == null)
            {
                return act();
            }
            #region 没有sql监控时，只记错误
            if (config.OpenSqlWatch == false)
            {
                try
                {
                    return act();
                }
                catch (Exception ex)
                {
                    StringBuilder sb1 = new StringBuilder();
                    sb1.Append("【错误说明】" + ex.Message + ";\r\n\t");
                    sb1.AppendFormat("【sql】:{0};", sql);
                    if (paras != null && paras.Count > 0)
                    {
                        sb1.AppendFormat("\r\n\t【参数】:{0};", string.Join(",", paras.Select(x => "@" + x.Name + ":" + Utils.Converter.NullToStr(x.Value) + "  ")));
                    }
                    else
                    {
                        sb1.AppendFormat("\r\n\t【参数】:{0};", "[无]");
                    }
                    Error("sql错误[" + sql.GetHashCode() + "]", sb1.ToString());
                    throw;
                }
            }
            #endregion

            string title = "sql耗时[" + sql.GetHashCode() + "]";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("【sql】:{0};", sql);
            if (paras != null && paras.Count > 0)
            {
                sb.AppendFormat("\r\n\t【参数】:{0};", string.Join(",", paras.Select(x => "@" + x.Name + ":" + Utils.Converter.NullToStr(x.Value) + "  ")));
            }
            else
            {
                sb.AppendFormat("\r\n\t【参数】:{0};", "[无]");
            }
            try
            {
                return TimeWatch(0, title, sb.ToString(), act);
            }
            catch (Exception ex)
            {
                title = "sql错误[" + sql.GetHashCode() + "]";
                sb.Insert(0, "【错误说明】" + ex.Message + ";\r\n\t");
                Error(title, sb.ToString());
                throw ex;
            }
        }
        #endregion
    }

}
