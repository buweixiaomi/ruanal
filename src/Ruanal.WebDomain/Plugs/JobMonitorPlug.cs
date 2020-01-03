using Newtonsoft.Json.Linq;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;

namespace Ruanal.WebDomain.Plugs
{
    public class JobMonitorPlug : IPlug
    {
        DAL.NodeDal nodedal = new DAL.NodeDal();
        DAL.TaskDal taskDal = new DAL.TaskDal();

        class CheckItem
        {
            public Func<string> Checker;
            public DateTime LastNotifyTime;
            public string Title;
            public CheckItem(string title, Func<string> checker)
            {
                this.Title = title;
                this.Checker = checker;
                this.LastNotifyTime = DateTime.MinValue;
            }
        }

        List<CheckItem> checkers = new List<CheckItem>();
        public JobMonitorPlug()
        {
            checkers.Add(new CheckItem("Tcp", TcpConnectChecker));
            checkers.Add(new CheckItem("节点心跳", NodeHeartChecker));
            checkers.Add(new CheckItem("任务状态", JobRunChecker));
            checkers.Add(new CheckItem("任务长运行", RunKeyLongRunChecker));
            checkers.Add(new CheckItem("任务长等待", RunKeyLongWaitChecker));
            checkers.Add(new CheckItem("任务长跳过", RunKeyNoRunChecker));
            checkers.Add(new CheckItem("Redis", RedisConnectionChecker));
            checkers.Add(new CheckItem("Rabbit", RabbitConnectionChecker));
            checkers.Add(new CheckItem("RabbitQueue", RabbitQueueChecker));
        }

        public override void RunOnce()
        {
            var dojobmoni = Config.GetBool("jobmoni", false);
            if (dojobmoni == false)
                return;
            var notifyEachMins = Config.GetInt("jobmoni:notifyEachMins", 30);
            var notifyMobiles = Config.GetStringArray("jobmoni:notifyMobile");
            var monititle = Config.GetString("jobmoni:title", "任务监控");
            if (notifyMobiles.Length == 0)
                return;
            List<string> errormsgs = new List<string>();
            foreach (var a in checkers)
            {
                try
                {
                    var msg = a.Checker.Invoke();
                    if (string.IsNullOrWhiteSpace(msg))
                        continue;
                    if (DateTime.Now.Subtract(a.LastNotifyTime).TotalMinutes < notifyEachMins)
                        continue;
                    errormsgs.Add(msg);
                    a.LastNotifyTime = DateTime.Now;
                }
                catch (Exception ex)
                { }
            }
            if (errormsgs.Count <= 0)
                return;
            StringBuilder notifyMsg = new StringBuilder();
            notifyMsg.Append("【追电网络】");
            notifyMsg.Append(monititle + " ");
            notifyMsg.Append(string.Join(";", errormsgs));
            SendMobileMsg(notifyMobiles, notifyMsg.ToString());
        }

        private void SendMobileMsg(string[] mobiles, string msg)
        {
            string Url = Config.GetString("msgBox:Url", "");
            string AppKey = Config.GetString("msgBox:AppKey", "");
            string AppSecret = Config.GetString("msgBox:AppSecret", "");
            string CustomerKey = Config.GetString("msgBox:CustomerKey", "");
            Zd.MsgBox.SDK.MsgClient msgClient =
                new Zd.MsgBox.SDK.MsgClient(Url, AppKey, AppSecret, CustomerKey);

            var rd = new Random();
            var fromip = string.Format("{0}.{1}.{2}.{3}", rd.Next(1, 256), rd.Next(1, 256), rd.Next(1, 256), rd.Next(1, 256));
            msgClient.SendSmsBatch(new Zd.MsgBox.SDK.BatchSendSmsPara()
            {
                receiverphones = string.Join(",", mobiles),
                content = msg,
                uuid = Guid.NewGuid().ToString(),
                clientid = Guid.NewGuid().ToString(),
                fromip = fromip,
                smstype = 0,
                expireminutes = 10,
                providercode = ""
            });

        }

        private string TcpConnectChecker()
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodes = this.nodedal.GetAllNode(dbconn, false);
                List<Model.Node> unoknodes = new List<Model.Node>();
                nodes.AddRange(nodes);
                for (var k = 0; k < 2; k++)
                {
                    if (unoknodes.Count == 0)
                        break;
                    for (var j = unoknodes.Count - 1; j >= 0; j--)
                    {

                        string msg = Ruanal.Core.ConfigConst.TalkNodeTaskStatus + string.Format("{0}", unoknodes[j].ClientId);
                        var result = Ruanal.Core.Notify.NotifyHelper.TalkToAll(msg, 5000, 1);
                        if (result != null && result.Count > 0)
                        {
                            unoknodes.RemoveAt(j);
                        }
                    }
                }
                if (unoknodes.Count == 0)
                    return null;
                StringBuilder sb = new StringBuilder();
                sb.Append("节点")
                    .Append(string.Join("、", unoknodes.Select(x => x.Title).Take(2)))
                    .Append("等通知Tcp无法连接");
                return sb.ToString();
            }
        }
        private string NodeHeartChecker()
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodes = this.nodedal.GetAllNode(dbconn, false);
                var noheart = nodes.Where(x =>
                x.LastHeartTime == null ||
                DateTime.Now.Subtract(x.LastHeartTime.Value).TotalMinutes > 4
                ).ToList();
                if (noheart.Count == 0)
                    return null;

                StringBuilder sb = new StringBuilder();
                sb.Append("节点")
                    .Append(string.Join("、", noheart.Select(x => x.Title).Take(2)))
                    .Append("等心跳异常");
                return sb.ToString();
            }
        }
        private string JobRunChecker()
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var tasks = taskDal.GetAllTask(dbconn, 0);
                List<Model.Task> unoktasks = new List<Model.Task>();
                foreach (var a in tasks)
                {
                    foreach (var b in taskDal.GetTaskBindings(dbconn, a.TaskId))
                    {
                        if (b.LocalState == 0)
                            continue;
                        if (b.LocalState == 1 && b.ServerState != 1)
                        {
                            unoktasks.Add(a);
                            break;
                        }
                    }
                }

                if (unoktasks.Count == 0)
                    return null;

                StringBuilder sb = new StringBuilder();
                sb.Append("任务")
                    .Append(string.Join("、", unoktasks.Select(x => x.Title).Take(2)))
                    .Append("等运行异常");
                return sb.ToString();
            }
        }
        private string RunKeyLongRunChecker()
        {
            var longRunMins = Config.GetInt("jobmoni:LongRunMins", 120);
            string sql = @"SELECT  dp.[runKey],ta.[Title] FROM [dbo].[dispatch](nolock) dp
        join [Task](nolock) ta on dp.[taskId]=ta.[taskId] 
          where dp.dispatchState=2 and   dp.executeTime<@btime;";
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var tb = dbconn.SqlToDataTable(sql, new { btime = DateTime.Now.AddMinutes(-longRunMins) });
                if (tb.Rows.Count == 0)
                    return null;
                List<string> taskkey = new List<string>();
                foreach (DataRow dr in tb.Rows)
                {
                    taskkey.Add(dr["Title"].ToString() + "：" + dr["runKey"].ToString() + "");
                }
                StringBuilder sb = new StringBuilder();
                sb.Append("调度任务")
                    .Append(string.Join("、", taskkey.Take(2)))
                    .Append("等运行" + longRunMins + "分钟未停止");
                return sb.ToString();
            }
        }

        private string RunKeyLongWaitChecker()
        {
            var longRunMins = Config.GetInt("jobmoni:LongWaitMins", 40);
            string sql = @"SELECT  dp.[runKey],ta.[Title] FROM [dbo].[dispatch](nolock) dp
        join [Task](nolock) ta on dp.[taskId]=ta.[taskId] 
          where dp.dispatchState<2 and dp.dispatchState>=0  and   dp.createTime<@btime;";
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var tb = dbconn.SqlToDataTable(sql, new { btime = DateTime.Now.AddMinutes(-longRunMins) });
                if (tb.Rows.Count == 0)
                    return null;
                List<string> taskkey = new List<string>();
                foreach (DataRow dr in tb.Rows)
                {
                    taskkey.Add(dr["Title"].ToString() + "：" + dr["runKey"].ToString());
                }
                StringBuilder sb = new StringBuilder();
                sb.Append("调度任务")
                    .Append(string.Join("、", taskkey.Take(2)))
                    .Append("等 等待" + longRunMins + "分钟未运行");
                return sb.ToString();
            }
        }

        private string RunKeyNoRunChecker()
        {
            var noRunMins = Config.GetInt("jobmoni:NoRunMins", 250);
            var noRunningMins = Config.GetInt("jobmoni:NoRunRunningMins", 5);
            string sql = @"select T_SKIP.[runKey],ta.[Title]
from 
(
SELECT  taskId,runKey,count(1) as SCount FROM  [dbo].[dispatch](nolock)
where  dispatchState =5     and createTime>@ctime
group by taskId,runKey
)
T_SKIP 
left join 
(SELECT  taskId,runKey ,count(1) as SCount FROM  [dbo].[dispatch](nolock)
where    (dispatchState=3  or (dispatchState=2 and executeTime<@etime ))
and createTime>@ctime
group by taskId,runKey
)
T_OK
on T_OK.taskId=T_SKIP.taskId and T_OK.runKey=T_SKIP.runKey
left join [task](nolock) ta on T_SKIP.taskId=ta.taskId
where T_OK.runKey is null";
            var para = new
            {
                ctime = DateTime.Now.AddMinutes(-noRunMins),
                etime = DateTime.Now.AddMinutes(-noRunningMins)
            };
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var tb = dbconn.SqlToDataTable(sql, para);
                if (tb.Rows.Count == 0)
                    return null;
                List<string> taskkey = new List<string>();
                foreach (DataRow dr in tb.Rows)
                {
                    taskkey.Add(dr["Title"].ToString() + "：" + dr["runKey"].ToString());
                }
                StringBuilder sb = new StringBuilder();
                sb.Append("调度任务")
                    .Append(string.Join("、", taskkey.Take(2)))
                    .Append("等最近" + noRunMins + "分钟没有成功运行记录");
                return sb.ToString();
            }
        }

        private string RedisConnectionChecker()
        {
            var conn = Config.GetString("jobmoni:redisconn", "");
            if (string.IsNullOrWhiteSpace(conn))
                return null;
            try
            {
                using (var rd = GetConnect(conn))
                {
                    rd.Set<string>("_runaltestconnkey_", DateTime.Now.ToString(), TimeSpan.FromSeconds(5));
                }
                return null;
            }
            catch (Exception ex)
            {
                return "redis状态异常！";
            }
        }




        private string RabbitConnectionChecker()
        {
            var connstr = Config.GetString("jobmoni:rabbitconn", "");
            if (string.IsNullOrWhiteSpace(connstr))
                return null;
            try
            {

                RabbitMQ.Client.ConnectionFactory cf = new RabbitMQ.Client.ConnectionFactory();
                cf.AutomaticRecoveryEnabled = true;
                cf.Uri = connstr;
                cf.RequestedConnectionTimeout = 5000;
                var currConn = cf.CreateConnection();
                var md = currConn.CreateModel();
                md.Close();
                currConn.Close();
                return null;
            }
            catch (Exception ex)
            {
                return "rabbit状态异常！";
            }
        }

        private string RabbitQueueChecker()
        {
            var connstr = Config.GetString("jobmoni:rabbitconn", "");
            var rabbitmqmsgsafecount = Config.GetInt("jobmoni:rabbitmqmsgsafecount", 1000);
            if (string.IsNullOrWhiteSpace(connstr))
                return null;
            int trytimes = 2;
            int errorcount = 0;
            while (trytimes > 0)
            {
                trytimes--;

                try
                {
                    rabbitmqmsgsafecount = Math.Max(200, rabbitmqmsgsafecount);
                    RabbitMQMonitor rabbitMQMonitor = new RabbitMQMonitor(connstr);
                    var msg = rabbitMQMonitor.Check(rabbitmqmsgsafecount);
                    if (string.IsNullOrWhiteSpace(msg))
                        return null;
                    return msg;
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                    errorcount++;
                }
            }
            if (errorcount > 0)
                return "rabbitapi状态异常！";
            return null;
        }

        public static IRedisClient GetConnect(string connstring)
        {
            #region 连接字符串处理
            if (string.IsNullOrWhiteSpace(connstring))
            {
                throw new ArgumentException("无效redis地址！");
            }
            string host = string.Empty;
            int port = 6379;
            string passwd = null;
            int dbid = 0;
            var sps = connstring.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (sps.Length == 1)
            {
                if (sps[0].Contains("@"))
                {
                    passwd = sps[0].Substring(0, sps[0].IndexOf('@'));
                    sps[0] = sps[0].Substring(sps[0].IndexOf('@') + 1);
                }
                if (sps[0].Contains(":"))
                {
                    host = sps[0].Substring(0, sps[0].IndexOf(':'));
                    port = Convert.ToInt32(sps[0].Substring(sps[0].IndexOf(':') + 1));
                }
                else
                {
                    host = sps[0];
                }
            }
            else
            {
                foreach (var a in sps)
                {
                    var kvp = a.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (kvp.Length != 2)
                        continue;
                    kvp[0] = kvp[0].ToLower();

                    //server
                    if (kvp[0] == "server")
                    {
                        if (kvp[1].Contains(":"))
                        {
                            host = kvp[1].Substring(0, sps[0].IndexOf(':'));
                            int t_port = 0;
                            if (int.TryParse(kvp[1].Substring(sps[0].IndexOf(':') + 1), out t_port))
                                port = t_port;
                        }
                        else
                        {
                            host = kvp[1];
                        }
                    }
                    //port
                    if (kvp[0] == "port")
                    {
                        int t_port = 0;
                        if (int.TryParse(kvp[1], out t_port))
                            port = t_port;
                    }

                    //password
                    if (kvp[0] == "password" || kvp[0] == "pwd")
                    {
                        passwd = kvp[1];
                    }
                    //database
                    if (kvp[0] == "database" || kvp[0] == "db")
                    {
                        var t_dbid = 0;
                        if (int.TryParse(kvp[1], out dbid))
                            dbid = t_dbid;
                    }
                }
            }
            #endregion

            ServiceStack.Redis.RedisClient client = new ServiceStack.Redis.RedisClient(host, port, passwd, dbid);
            return client;
        }

        public class RabbitMQMonitor
        {
            public string Url { get; set; }
            string HostAndPort;
            string Host;
            int Port;
            string UserName;
            string UserPass;
            string HttpApiUrl;

            public static void Test()
            {
                var msg = new RabbitMQMonitor("amqp://zdrabbit02:zdrabbit02123@192.168.33.144:5672//").Check(1000);
                Console.WriteLine(msg);
            }
            public RabbitMQMonitor(string rabbitmqurl)
            {
                this.Url = rabbitmqurl;
                this.Decode();
            }

            private void Decode()
            {
                Uri uri = new Uri(Url);
                var a = uri.UserInfo.Split(":".ToArray(), 2);
                this.HostAndPort = uri.Authority;
                this.Host = uri.Host;
                this.Port = uri.Port;
                this.UserName = a[0];
                this.UserPass = a[1];
                this.HttpApiUrl = "http://" + this.Host + ":15672";
            }

            public string Check(int safecount)
            {
                StringBuilder sb = new StringBuilder();
                var qs = GetQueues();
                foreach (var q in qs)
                {
                    if (q.ReadyCount >= safecount)
                    {
                        sb.Append("队列" + q.Name + "有" + q.ReadyCount + "消息未消费:" + q.AckRate + "ips;");
                    }
                }
                return sb.ToString();
            }



            private List<QueueInfo> GetQueues()
            {
                var ss = new List<QueueInfo>();
                var jtoken = InvokeApi("/api/queues", null);
                foreach (var jque in jtoken.Children())
                {
                    var name = jque["name"].ToString();
                    var count = jque["messages_ready"].Value<int>();
                    var rate = 0d;
                    var jrate = jque["message_stats"];
                    if (jrate != null)
                    {
                        if (jrate["ack_details"] != null)
                        {
                            rate = jrate["ack_details"]["rate"].Value<double>();
                        }
                        else if (jrate["deliver_get_details"] != null)
                        {
                            rate = jrate["deliver_get_details"]["rate"].Value<double>();
                        }
                        else if (jrate["deliver_details"] != null)
                        {
                            rate = jrate["deliver_details"]["rate"].Value<double>();
                        }
                    }
                    ss.Add(new QueueInfo()
                    {
                        Name = name,
                        AckRate = rate,
                        ReadyCount = count
                    });
                }
                return ss;
            }

            private class QueueInfo
            {
                public string Name { get; set; }
                public int ReadyCount { get; set; }
                public double AckRate { get; set; }
            }

            private JToken InvokeApi(string api, object data, string method = "")
            {
                string url = this.HttpApiUrl + api;
                var req = (HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                req.ContentType = "application/json";
                var bs = Encoding.UTF8.GetBytes(this.UserName + ":" + this.UserPass);
                var auth = Convert.ToBase64String(bs);
                req.Headers.Set(HttpRequestHeader.Authorization, "Basic " + auth);
                if (string.IsNullOrWhiteSpace(method))
                    method = "GET";
                req.Method = method.ToUpper();
                if (data != null)
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    using (var reqstream = req.GetRequestStream())
                    {
                        bs = Encoding.UTF8.GetBytes(json);
                        reqstream.Write(bs, 0, bs.Length);
                        reqstream.Flush();
                    }
                }
                var resp = (HttpWebResponse)req.GetResponse();
                using (var respstream = resp.GetResponseStream())
                using (var streamread = new System.IO.StreamReader(respstream, Encoding.UTF8))
                {
                    var respjson = streamread.ReadToEnd();
                    //Console.WriteLine();
                    //Console.WriteLine(api);
                    //Console.WriteLine(respjson);
                    return JToken.Parse(respjson);
                }
            }
        }
    }
}
