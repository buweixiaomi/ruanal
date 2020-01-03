using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RLib.WatchLog
{

    internal class ApiLoger : ILoger
    {

        string lastRequestMsg = string.Empty;
        System.Net.Http.HttpClient HttpClient = new System.Net.Http.HttpClient();
        public ApiLoger()
        {
        }

        const int maxThread = 3;
        int currThread = 0;
        object threadlock = new object();
        Queue<List<LogEntity>> groups = new Queue<List<LogEntity>>();
        const int maxGroups = maxThread * 5;

        const int maxtrycount = 3;
        public override void WriteLog(List<LogEntity> logs)
        {
            if (string.IsNullOrWhiteSpace(Config.ApiLogUrl))
                return;
            bool useThread = false;
            bool selfwrite = false;
            lock (threadlock)
            {
                if (groups.Count < maxGroups)
                {
                    groups.Enqueue(logs);
                    selfwrite = false;
                }
                else
                {
                    selfwrite = true;
                    //System.Diagnostics.Trace.WriteLine("out..");
                }
                if (currThread < maxThread)
                {
                    useThread = true;
                    currThread++;
                }
            }
            if (useThread)
            {
                System.Threading.ThreadPool.QueueUserWorkItem((x) =>
                {
                    int trycount = 0;
                    while (true)
                    {
                        List<LogEntity> towrites = null;
                        lock (threadlock)
                        {
                            if (groups.Count > 0)
                            {
                                towrites = groups.Dequeue();
                            }
                        }
                        if (towrites == null)
                        {
                            if (trycount >= maxtrycount)
                            {
                                break;
                            }
                            else
                            {
                                trycount++;
                                System.Threading.Thread.Sleep(10 * trycount);
                                continue;
                            }
                        }
                        try { trycount = 0; DoAction(towrites); } catch { }
                    }
                    lock (threadlock)
                    {
                        currThread--;
                    }
                });
            }
            if (selfwrite)
            {
                System.Threading.SpinWait.SpinUntil(() => groups.Count < maxGroups);
                groups.Enqueue(logs);
            }
        }

        private void DoAction(List<LogEntity> logs)
        {
            string tosignkey = string.Empty;
            string sign = string.Empty;
            BuildSign(this.Config.ApiLogSecret, out tosignkey, out sign);
            ApiRequest(tosignkey, sign, logs);
        }

        private bool ApiRequest(string signkey, string sign, List<LogEntity> logs)
        {
            var reqcontent = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                signKey = signkey,
                sign = sign,
                logs = logs
            }, new Newtonsoft.Json.JsonSerializerSettings()
            {
                DateFormatString = "yyyy/MM/dd HH:mm:ss.fff"
            });
            try
            {
                var task = HttpClient.PostAsync(Config.ApiLogUrl, new System.Net.Http.StringContent(reqcontent, Encoding.UTF8));
                var lastRequestMsg = task.Result.Content.ReadAsStringAsync().Result;
                var jobj = Newtonsoft.Json.Linq.JObject.Parse(lastRequestMsg);
                if (jobj["code"].Value<int>() > 0)
                {
                    lastRequestMsg = "success";
                    return true;
                }
                else
                {
                    lastRequestMsg = jobj["msg"].Value<string>();
                    return false;
                }
            }
            catch (WebException ex)
            {
                using (var dataStream = ex.Response.GetResponseStream())
                using (StreamReader reader = new StreamReader(dataStream, Encoding.UTF8))
                {
                    lastRequestMsg = reader.ReadToEnd();
                }
                lastRequestMsg = ex.Message;
                return false;
            }
            catch (System.Exception ex)
            {
                lastRequestMsg = ex.Message;
                return false;
            }

        }


        private void BuildSign(string secret, out string toSignKey, out string sign)
        {
            toSignKey = DateTime.Now.Ticks.ToString() + Guid.NewGuid().ToString();
            var realtohash = toSignKey.Substring(0, 41);
            sign = RLib.Utils.Security.MakeMD5(realtohash + secret);
        }
    }
}
