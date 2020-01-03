using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ruanal.Core.Notify
{

    /// <summary>
    /// #功能说明：服务器通知任务
    /// #运行方式：指定运行表达式
    /// #任务配置参数：
    ///  1）listenerhost 服务器通知地址
    /// </summary>
    public class TcpNotifyServer
    {
        Dictionary<string, List<ClientEntity>> topicclient = new Dictionary<string, List<ClientEntity>>();
        List<ClientEntity> clients = new List<ClientEntity>();
        object clients_locker = new object();
        const int maxBytes = 2048;
        const int Min_DelayTime = 10;//ms
        const int Server_Ping_Seconds = 30;
        const int SendTimeOutMS = 6000;
        private int pingoutms = 5000;
        const float MinCanServerPingVersion = 1.1f;
        public const string System_Ping = "__ping";
        public const string System_Pong = "__pong";
        public const string System_Notify = "__notify";
        System.Threading.CancellationTokenSource CTS = new System.Threading.CancellationTokenSource();
        System.Threading.Thread t1, t2; System.Net.Sockets.TcpListener tcpl;

        MSGQueue queue = new MSGQueue();
        System.Threading.AutoResetEvent ARE = new AutoResetEvent(false);

        const int enby = '\n';
        public TcpNotifyServer()
        {
            queue.callback = () =>
            {
                ARE.Set();
            };
        }
        public void Listen(string listenAddress, int port)
        {
            RLib.WatchLog.Loger.Log("正在运行", "");
            tcpl = new System.Net.Sockets.TcpListener(IPAddress.Parse(listenAddress), port);
            tcpl.Start();

            StartAccept();
            StartSend();
            ReadDataThread();
        }

        private void StartAccept()
        {
            t1 = new System.Threading.Thread(() =>
            {
                RLib.WatchLog.Loger.Log("启动监听", "");
                while (true)
                {
                    try
                    {
                        var it = tcpl.AcceptTcpClient();
                        var item = TryFilter(it);
                        if (item == null)
                        {
                            try { it.Close(); } catch { }

                        }
                        else
                        {
                            AddClient(item);
                        }
                    }
                    catch { }
                    System.Threading.Thread.Sleep(10);
                }
            });
            t1.IsBackground = true;
            t1.Start();
        }

        private void StartSend()
        {
            t2 = new Thread(() =>
            {
                while (true)
                {
                    ARE.WaitOne(Min_DelayTime);
                    if (CTS.IsCancellationRequested)
                        return;
                    QueueItem item = null;
                    while ((item = queue.DeQueue()) != null)
                    {
                        if (CTS.IsCancellationRequested)
                            return;
                        Notify(item);
                    }
                }
            });
            t2.IsBackground = true;
            t2.Start();
        }


        private void ReadDataThread()
        {
            Thread t = new Thread(() =>
             {
                 while (true)
                 {
                     ClientEntity[] toreadclients = null;
                     lock (clients_locker)
                     {
                         toreadclients = clients.ToArray();
                     }
                     bool hasmsg = false;
                     foreach (var a in toreadclients)
                     {
                         try
                         {
                             hasmsg = hasmsg || ClientReceiveNoLoop(a);
                         }
                         catch (Exception ex) { }
                     }
                     if (hasmsg == false)
                     {
                         System.Threading.Thread.Sleep(Min_DelayTime);
                     }
                 }
             });
            t.IsBackground = true;
            t.Start();
        }



        private ClientEntity TryFilter(System.Net.Sockets.TcpClient client)
        {
            try
            {
                client.SendTimeout = SendTimeOutMS;
                RLib.WatchLog.Loger.Log("收到TcpClient", client.Client.RemoteEndPoint.ToString());
                var stream = client.GetStream();
                List<byte> bs = new List<byte>();
                stream.ReadTimeout = 1000 * 6;
                //接收第一行数据
                byte[] tmpread = new byte[512];
                int i = stream.Read(tmpread, 0, tmpread.Length);
                if (i != -1)
                {
                    bs.AddRange(tmpread.Take(i));
                    while (stream.DataAvailable)
                    {
                        i = stream.Read(tmpread, 0, tmpread.Length);
                        if (i == -1) break;
                        bs.AddRange(tmpread.Take(i));
                    }
                }
                var stringv = (System.Text.Encoding.UTF8.GetString(bs.ToArray()).Split(new char[] { '\n' }, 1).FirstOrDefault() ?? "").Trim();
                var vhost = stringv.Split(":".ToArray(), 3);
                if (vhost.Length >= 2)
                {
                    float version = 0f;
                    if (vhost[0].LastIndexOf('[') > 0 && vhost[0].LastIndexOf(']') == vhost[0].Length - 1)
                    {
                        string sversion = vhost[0].Substring(vhost[0].LastIndexOf('[')).TrimStart('[').TrimEnd(']');
                        version = RLib.Utils.Converter.StrToFloat(sversion);
                        vhost[0] = vhost[0].Substring(0, vhost[0].LastIndexOf('['));
                    }
                    var tops = vhost[1].Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                    var name = vhost.Length == 3 ? vhost[2] : "[null]";
                    if (tops.Length > 0)
                    {
                        RLib.WatchLog.Loger.Log("收到一个监听者 v=" + version + " name=" + name, "当前连接数" + clients.Count);
                    }
                    var item = new ClientEntity() { Version = version, Client = client, Stream = stream, Topics = tops };
                    RLib.WatchLog.Loger.Log("收到一个发送者 v=" + version + " name=" + name, "当前连接数" + clients.Count);
                    var respyes = Encoding.UTF8.GetBytes("yes\n");
                    stream.Write(respyes, 0, respyes.Length);
                    stream.Flush();
                    return item;
                }
                else
                {
                    client.Close();
                }
            }
            catch (Exception ex) { }
            return null;
        }

        private void Notify(QueueItem qitem)
        {
            if (!string.IsNullOrEmpty(qitem.ClientId))
            {
                ClientEntity thisclient;
                lock (clients_locker)
                {
                    thisclient = clients.FirstOrDefault(x => x.ClientID == qitem.ClientId);
                }
                if (thisclient != null)
                    SafeClientSendToNetwork(thisclient, qitem);
                return;
            }
            if (!topicclient.ContainsKey(qitem.Topic))
                return;
            var tclients = topicclient[qitem.Topic].ToList();
            Parallel.ForEach<ClientEntity>(tclients, new ParallelOptions() { MaxDegreeOfParallelism = 10, CancellationToken = CTS.Token }, (x) =>
            {
                SafeClientSendToNetwork(x, qitem);
            });
        }

        private void SafeClientSendToNetwork(ClientEntity client, QueueItem item)
        {
            if (client == null)
                return;
            lock (client.clientSendLocker)
            {
                try
                {
#if DEBUG
                    Console.WriteLine("server发送TCP消息:" + item.Msg.TrimEnd('\n'));
#endif
                    client.Stream.Write(item.ByteStr, 0, item.ByteStr.Length);
                }
                catch (Exception ex)
                {
                    try
                    {
                        RemoveClient(client);
                    }
                    catch (Exception) { }
                }
            }
        }

        private bool ClientReceiveNoLoop(ClientEntity c)
        {
            try
            {
                if (!c.Client.Connected)
                {
                    throw new Exception("connected = false!");
                }

                var stream = c.Stream;
                int endindex = -1;
                bool hasmsg = false;
                DateTime nowtime = DateTime.Now;


                if (c.CheckPing == false && c.Version >= MinCanServerPingVersion && (nowtime - c.LastServerPingTime).TotalSeconds > Server_Ping_Seconds)
                {
                    string msg = System_Ping + ":serverping " + nowtime.Ticks + (char)enby;
                    queue.EnQueue(new QueueItem()
                    {
                        Topic = System_Ping,
                        Msg = msg,
                        ClientId = c.ClientID,
                        ByteStr = System.Text.Encoding.UTF8.GetBytes(msg)
                    });
                    c.LastServerPingTime = nowtime;
                    c.LastServerPongTime = null;
                    c.CheckPing = true;
#if DEBUG
                    Console.WriteLine("Ping"+c.ClientID);
#endif
                    //RLib.WatchLog.Loger.Log("Ping", c.ClientID);
                }

                if (stream.DataAvailable)
                {
                    byte[] waitread = new byte[maxBytes];
                    int readlength = stream.Read(waitread, 0, waitread.Length);
                    if (readlength > 0)
                    {
                        c.LastReceived.AddRange(waitread.Take(readlength));
                    }
                    hasmsg = true;
                }

                while (c.LastReceived.Count > 0 && (endindex = c.LastReceived.IndexOf(enby)) >= 0)
                {
                    var stringv = System.Text.Encoding.UTF8.GetString(c.LastReceived.Take(endindex).ToArray());
                    c.LastReceived.RemoveRange(0, endindex + 1);
#if DEBUG
                    Console.WriteLine("server收到TCP消息:" + stringv.TrimEnd((char)enby));
#endif
                    string[] kv = stringv.Split(new char[] { ':' }, 2);
                    if (kv.Length == 2)
                    {
                        if (kv[0] == System_Ping)
                        {
                            c.LastPintTime = DateTime.Now;
                            var smsg = System_Pong + ":" + kv[1] + " OK" + (char)enby;
                            queue.EnQueue(new QueueItem()
                            {
                                Topic = System_Pong,
                                Msg = smsg,
                                ClientId = c.ClientID,
                                ByteStr = System.Text.Encoding.UTF8.GetBytes(smsg)
                            });
                        }
                        else if (kv[0] == System_Pong)
                        {
                            c.LastServerPongTime = DateTime.Now;
                        }
                        else if (kv[0] == System_Notify)
                        {
                            string[] topic_value = kv[1].Split(new[] { ',' }, 2);
                            if (topic_value.Length == 2)
                            {
                                queue.EnQueue(new QueueItem()
                                {
                                    Topic = topic_value[0].Trim(),
                                    Msg = topic_value[1],
                                    ByteStr = System.Text.Encoding.UTF8.GetBytes(System_Notify + ":" + topic_value[0].Trim() + "," + topic_value[1] + (char)enby)
                                });
                            }
                        }
                    }
                    hasmsg = true;
                }

                //非正常数据 判定为非法客户端，则删除
                if (c.LastReceived.Count > maxBytes)
                {
                    throw new Exception("thream max [maxBytes]" + maxBytes + ";非法客户端，则删除");
                }
                if (c.CheckPing)
                {
                    if (c.LastServerPongTime == null)
                    {
                        if (DateTime.Now.Subtract(c.LastServerPingTime).TotalMilliseconds > pingoutms)
                        {
                            throw new Exception("ping 超时，则删除");
                        }
                    }
                    else
                    {
                        if (c.LastServerPongTime.Value.Subtract(c.LastServerPingTime).TotalMilliseconds > pingoutms)
                        {
                            throw new Exception("ping 超时，则删除");
                        }
                        c.CheckPing = false;
                    }
                }

                return hasmsg;
            }
            catch (Exception ex)
            {
                RemoveClient(c);
                return false;
            }
        }

        private void RemoveClient(ClientEntity client)
        {
            if (client == null)
                return;
            try
            {
                if (client != null && client.Client.Connected)
                {
                    RLib.WatchLog.Loger.Log("关闭一个客户端连接", "当前连接数" + clients.Count);
                    client.Client.Close();
                }
            }
            catch (Exception) { }
            try
            {
                lock (clients_locker)
                {
                    clients.Remove(client);
                    if (client.Topics != null)
                    {
                        foreach (var t in client.Topics)
                        {
                            try
                            {
                                topicclient[t].Remove(client);
                                if (topicclient[t].Count == 0)
                                    topicclient.Remove(t);
                            }
                            catch { }
                        }
                    }
                    RLib.WatchLog.Loger.Log("删除一个客户端连接", "当前连接数" + clients.Count);
                }
            }
            catch (Exception) { }
        }
        private void AddClient(ClientEntity client)
        {
            lock (clients_locker)
            {
                clients.Add(client);
                foreach (var t in client.Topics)
                {
                    if (!topicclient.ContainsKey(t))
                        topicclient[t] = new List<ClientEntity>();
                    topicclient[t].Add(client);
                }
            }
        }
        public void Stop()
        {
            try
            {
                CTS.Cancel();
            }
            catch { }
            try { t1.Abort(); }
            catch { }
            try { t2.Abort(); }
            catch { }
            foreach (var a in clients)
            {
                try
                {
                    a.Client.Close();
                }
                catch { }
            }
            try
            {
                tcpl.Stop();
            }
            catch { }
            System.Threading.Thread.Sleep(500);
        }
    }

    public class ClientEntity
    {
        public string ClientID { get; private set; }
        public ClientEntity()
        {
            LastReceived = new List<byte>();
            LastServerPingTime = DateTime.Now;
            LastServerPongTime = LastServerPingTime;
            ClientID = Guid.NewGuid().ToString();
        }
        public float Version { get; set; }

        public System.Net.Sockets.TcpClient Client { get; set; }
        public System.Net.Sockets.NetworkStream Stream { get; set; }

        public DateTime? LastPintTime { get; set; }
        public string[] Topics { get; set; }
        public bool CheckPing { get; set; }
        public DateTime LastServerPingTime { get; set; }
        public DateTime? LastServerPongTime { get; set; }
        public List<byte> LastReceived { get; set; }

        public object clientSendLocker = new object();

    }

    public class QueueItem
    {
        public string Topic { get; set; }
        public string Msg { get; set; }

        public byte[] ByteStr { get; set; }

        public DateTime EnqueueTime { get; set; }
        public DateTime? DequeueTime { get; set; }
        public string ClientId { get; set; }
    }

    public class MSGQueue
    {
        Queue<QueueItem> qt = new Queue<QueueItem>();
        object locker = new object();

        public Action callback;

        public void EnQueue(QueueItem item)
        {
            lock (locker)
            {
                item.EnqueueTime = DateTime.Now;
                qt.Enqueue(item);
            }
            if (callback != null)
                callback();
        }
        public QueueItem DeQueue()
        {
            lock (locker)
            {
                if (qt.Count() > 0)
                {
                    var item = qt.Dequeue();
                    item.DequeueTime = DateTime.Now;
                    return item;
                }
                return null;
            }
        }

    }
}
