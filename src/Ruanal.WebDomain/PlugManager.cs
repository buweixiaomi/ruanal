using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ruanal.WebDomain
{
    public class PlugManager
    {
        private class PlugItem
        {
            public DateTime NextRunTime { get; set; }
            public TimeSpan EachTime { get; set; }
            public DateTime LastErrorTime { get; set; }
            public IPlug Item { get; set; }
        }


        static object startlocker = new object();
        bool isRunning = false;
        List<PlugItem> plugs = new List<PlugItem>();
        BLL.CommBll commBll = new BLL.CommBll();
        BLL.OperationLogBll operationLogBll = new BLL.OperationLogBll();
        const int PlugErrorMin = 2;
        RunConfig runConfig = new RunConfig();
        public PlugManager()
        {
            ManualPlugInit();
        }

        private void ManualPlugInit()
        {
            this.AddPlug(new Plugs.InitTablePlug(), TimeSpan.FromHours(2.1));
            this.AddPlug(new Plugs.MantncePlug(), TimeSpan.FromHours(1));
            this.AddPlug(new Plugs.JobMonitorPlug(), TimeSpan.FromMinutes(5));
            this.AddPlug(new Plugs.EndStopedDispatchPlug(), TimeSpan.FromMinutes(4));
            this.AddPlug(new Plugs.ExpireDispatchPlug(), TimeSpan.FromMinutes(30));
            this.AddPlug(new Plugs.ConfigRefreshPlug(), TimeSpan.FromSeconds(20));

        }

        public void AddPlug(IPlug plug, TimeSpan eachtime)
        {
            var et = eachtime.TotalMinutes < 1 ? TimeSpan.FromMinutes(1) : eachtime;
            plug.Config = runConfig;
            plugs.Add(new PlugItem()
            {
                NextRunTime = DateTime.Now.AddMinutes(0.56),
                EachTime = et,
                Item = plug
            });
        }

        public void Start()
        {
            lock (startlocker)
            {
                if (isRunning) return;
                isRunning = true;
                Thread thread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            RefreshConfig();
                            foreach (var a in plugs)
                            {
                                CheckAction(a);
                            }
                            Thread.Sleep(TimeSpan.FromSeconds(25));
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(15));
                        }
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void CheckAction(PlugItem plugItem)
        {
            if (DateTime.Now < plugItem.NextRunTime)
                return;
            try
            {
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    var pl = (PlugItem)x;
                    try
                    {
                        pl.Item.RunOnce();
                    }
                    catch (Exception ex)
                    {
                        WritePlugErrorLog(pl, ex);
                    }
                }, plugItem);
            }
            finally
            {
                plugItem.NextRunTime = DateTime.Now.Add(plugItem.EachTime);
            }
        }

        private void WritePlugErrorLog(PlugItem plug, Exception ex)
        {
            if (DateTime.Now.Subtract(plug.LastErrorTime).TotalMinutes <= PlugErrorMin)
            {
                return;
            }
            plug.LastErrorTime = DateTime.Now;
            operationLogBll.AddLog(new Model.OperationLog()
            {
                Createtime = DateTime.Now,
                Module = "PlugManager",
                OperationName = "SYS",
                OperationTitle = plug.Item.ToString(),
                OperationContent = ex.Message,
                Id = 0
            });

        }


        private void RefreshConfig()
        {
            JToken JConfig = null;
            try
            {
                var configstring = commBll.GetConfig();
                if (string.IsNullOrWhiteSpace(configstring))
                {
                    JConfig = JToken.Parse("{}");
                }
                else
                {
                    JConfig = JToken.Parse(configstring);
                }
            }
            catch
            {
                JConfig = JToken.Parse("{}");
            }
            runConfig.Refresh(JConfig);
        }

    }

    public abstract class IPlug
    {
        public RunConfig Config { get; set; }
        public abstract void RunOnce();

    }

    public class RunConfig
    {
        private JToken jToken;

        public void Refresh(JToken config)
        {
            jToken = config;
        }

        public string GetString(string key, string defvalue)
        {
            var t = GetValue(key);
            if (t == null)
                return defvalue;
            if (string.IsNullOrWhiteSpace(t.ToString()))
                return defvalue;
            return t.ToString();
        }

        public bool GetBool(string key, bool defvalue)
        {
            var t = GetValue(key);
            if (t == null)
                return defvalue;
            if (string.IsNullOrWhiteSpace(t.ToString()))
                return defvalue;
            var sv = t.ToString().ToLower();
            var iv = RLib.Utils.Converter.StrToInt(sv);
            if (iv > 0 || sv == "1" || sv == "true")
            {
                return true;
            }
            return false;
        }

        public string[] GetStringArray(string key)
        {
            var t = GetValue(key);
            if (t == null)
                return new string[0];
            if (string.IsNullOrWhiteSpace(t.ToString()))
                return new string[0];
            if (t is JArray)
            {
                return t.Children().Select(x => x.ToString()).ToArray();
            }
            else
            {
                return new string[] { t.ToString() };
            }
        }

        public int GetInt(string key, int defvalue)
        {
            var t = GetValue(key);
            if (t == null)
                return defvalue;
            if (string.IsNullOrWhiteSpace(t.ToString()))
                return defvalue;
            return RLib.Utils.Converter.StrToInt(t.ToString());
        }

        public JToken GetValue(string key)
        {
            if (this.jToken[key] != null)
            {
                return this.jToken[key];
            }
            if (this.jToken[key.ToLower()] != null)
            {
                return this.jToken[key.ToLower()];
            }
            return null;
        }
    }

}
