using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;

namespace Ruanal.Web
{
    public class ApiResultSummary
    {
        class RItem
        {
            public string clientid;
            public string url;
            public DateTime reqtime;
            public DateTime endtime;
            public bool isend;
            public double usetime;
        }
        static ConcurrentDictionary<int, RItem> waititems = new ConcurrentDictionary<int, RItem>();
        static ConcurrentQueue<RItem> queue = new ConcurrentQueue<RItem>();
        static ApiResultSummary()
        {
            System.Threading.Thread thread = new System.Threading.Thread(Analize);
            thread.IsBackground = true;
            thread.Start();
        }

        private static void Analize(object s)
        {
            while (true)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromMinutes(1));

                try
                {
                    if (!WebDomain.Pub.ApiAnalyzeOn)
                    { 
                    }
                    List<RItem> waitana = new List<RItem>();
                    RItem witem = null;
                    while (queue.TryDequeue(out witem))
                    {
                        if (witem == null)
                            break;
                        waitana.Add(witem);
                    }
                    if (!WebDomain.Pub.ApiAnalyzeOn)
                    {
                        waitana.Clear();
                        continue;
                    }
                    if (waitana.Count == 0)
                        continue;

                    DateTime mintime = waitana.Min(x => x.reqtime);
                    DateTime maxtime = waitana.Max(x => x.reqtime);
                    var scounts = (maxtime - mintime).TotalSeconds;
                    System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                    //每个客户端的请求频率
                    stringBuilder.AppendLine(string.Format("【{0}】分析", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    stringBuilder.AppendLine("每个客户端的请求频率");
                    foreach (var a in waitana.GroupBy(x => x.clientid))
                    {
                        var times = a.Count() / scounts;
                        stringBuilder.AppendLine(string.Format("{0}: {1}/s", a.Key, times.ToString("0.00")));
                    }
                    // 每个接口的请求频率
                    stringBuilder.AppendLine("每个接口的请求频率");
                    foreach (var a in waitana.GroupBy(x => x.url))
                    {
                        var times = a.Count() / scounts;
                        var avg = a.Average(x => x.usetime);
                        stringBuilder.AppendLine(string.Format("{0}: {1}/s  平均用时: {2}s", a.Key, times.ToString("0.00"), avg.ToString("0.000")));
                    }
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "apianalize.text");
                    System.IO.File.AppendAllText(path, stringBuilder.ToString(), System.Text.Encoding.UTF8);
                }
                catch { }
            }
        }

        public static void StartRequest(HttpRequest request, string clientId)
        {
            if (!WebDomain.Pub.ApiAnalyzeOn)
            {
                return;
            }
            var item = new RItem()
            {
                clientid = clientId,
                reqtime = DateTime.Now,
                endtime = DateTime.Now,
                isend = false,
                url = request.RawUrl
            };
            waititems.TryAdd(request.GetHashCode(), item);
        }

        public static void EndRequest(HttpRequest request)
        {
            if (!WebDomain.Pub.ApiAnalyzeOn)
            {
                return;
            }
            RItem item = null;
            waititems.TryRemove(request.GetHashCode(), out item);
            if (item == null)
                return;
            item.endtime = DateTime.Now;
            item.usetime = (item.endtime - item.reqtime).TotalSeconds;
            item.isend = true;
            queue.Enqueue(item);
        }
    }
}