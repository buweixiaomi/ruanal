using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ruanal.Core.Notify
{
    public class TcpNotifyClient
    {
        public System.Net.Sockets.TcpClient client;
        private System.Net.Sockets.NetworkStream stream;
        private string ip;
        private int port;
        private string type;
        private string topic;
        private string name;
        private int pingoutms = 17000;
        const int maxBytes = 2048;
        private object _sendLocker = new object();
        DateTime? lastping = null;
        public DateTime LastServerPingTime { get; set; }
        public DateTime? LastServerPongTime { get; set; }
        public bool CheckPing { get; set; }
        const int ReConnectSeconds = 10;
        const int Min_DelayTime = 5;//ms
        const int SendTimeOutMS = 6000;
        const int ReadTimeOutMS = 12000;
        const int PingTimeSeconds = 18;//s
        const int ConnectTimeOutSeconds = 26;

        System.Threading.Thread receive;
        Action<string, string> callback;
        public const char ENDWIDTH = '\n';
        public const char SWITCHCHAR = '\\';
        public const string System_Ping = "__ping";
        public const string System_Pong = "__pong";
        public const string System_Notify = "__notify";
        public const string VERSION = "1.2";
        public TcpNotifyClient(string ip, int port, string type, string topic, string name, Action<string, string> callback)
        {
            this.ip = ip;
            this.port = port;
            this.type = type;
            this.topic = topic;
            this.name = name;
            this.callback = callback;
        }
        private int NIndex = 1;
        private static string ToSafeString(string orig)
        {
            return orig.Replace(ENDWIDTH.ToString(), new string(SWITCHCHAR, 2)).Replace(ENDWIDTH.ToString(), SWITCHCHAR.ToString() + ENDWIDTH.ToString());
        }

        private static byte[] GetSendFrame(string str)
        {
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(str);
            return bs;
        }

        public bool Open()
        {
            RLib.WatchLog.Loger.Log("打开连接..." + Thread.CurrentThread.ManagedThreadId, "");
            Console.WriteLine("打开连接...");
            try
            {
                RLib.WatchLog.Loger.Log("正在打开tcp..." + Thread.CurrentThread.ManagedThreadId, "");
                var tclient = new System.Net.Sockets.TcpClient();
                tclient.SendTimeout = SendTimeOutMS;
                tclient.NoDelay = true;
                tclient.Connect(ip, port);
                if (tclient.Connected)
                {
                    var tstream = tclient.GetStream();
                    string link_msg = string.Format("{0}[{1}]:{2}:{3}\n", type, VERSION, topic, name + "_" + (NIndex++).ToString());
                    byte[] bs = System.Text.Encoding.UTF8.GetBytes(link_msg);
                    tstream.Write(bs, 0, bs.Length);
                    tstream.Flush();

                    int waittimes = 3;
                    while (waittimes > 0 && tstream.DataAvailable == false)
                    {
                        waittimes--;
                        Thread.Sleep(1000);
                    }

                    tclient.ReceiveTimeout = ConnectTimeOutSeconds * 1000;
                    byte[] bsyes = new byte[10];
                    var count = tstream.Read(bsyes, 0, bsyes.Length);
                    if (System.Text.Encoding.UTF8.GetString(bsyes.Take(count).ToArray()).Trim() != "yes")
                    {
                        tclient.Close();
                        throw new Exception("非法TCP服务端");
                    }
                    tclient.ReceiveTimeout = ReadTimeOutMS;
                    this.client = tclient;
                    this.stream = tstream;
                    this.LastServerPingTime = DateTime.Now;
                    this.LastServerPongTime = this.LastServerPingTime;

                    RLib.WatchLog.Loger.Log("打开tcp成功！" + Thread.CurrentThread.ManagedThreadId, "");

                    Console.WriteLine("打开tcp成功！" + Thread.CurrentThread.ManagedThreadId);
                    return true;
                }
                Console.WriteLine("打开连接失败:Connected=false" + Thread.CurrentThread.ManagedThreadId);
                RLib.WatchLog.Loger.Log("打开连接失败:Connected=false" + Thread.CurrentThread.ManagedThreadId, "");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("打开连接失败:" + ex.Message);
                RLib.WatchLog.Loger.Log("打开tcp失败！" + Thread.CurrentThread.ManagedThreadId,
                    ex.Message + Environment.NewLine + ex.StackTrace);
                RLib.WatchLog.Loger.Error("打开tcp失败！" + Thread.CurrentThread.ManagedThreadId,
                    ex.Message + Environment.NewLine + ex.StackTrace);
                return false;
            }
            finally
            {
            }
        }


        public void StartReceive()
        {
            if (receive == null)
            {
                receive = new System.Threading.Thread(ThreadDo);
                receive.IsBackground = true;
                receive.Start();
            }
        }

        // List<byte> bs = new List<byte>();
        List<byte> LastReceived = new List<byte>();
        private void ThreadDo()
        {
            //byte[] receiveBuffer = new byte[maxBytes];
            while (true)
            {
                try
                {
                    if (stream == null) throw new Exception("Tcp未连接");
                    int endindex = -1;
                    while (LastReceived.Count > 0 && (endindex = LastReceived.IndexOf((int)ENDWIDTH)) >= 0)
                    {
                        var stringv = System.Text.Encoding.UTF8.GetString(LastReceived.Take(endindex).ToArray());
                        LastReceived.RemoveRange(0, endindex + 1);
#if DEBUG
                        Console.WriteLine("client收到TCP消息:" + stringv.TrimEnd('\n'));
#endif

                        string[] kv = stringv.Split(new char[] { ':' }, 2);
                        if (kv.Length == 2)
                        {
                            if (kv[0] == System_Ping)
                            {
                                this.lastping = DateTime.Now;
                                Send(string.Format("{0}:{1}", System_Pong, kv[1] + " OK"));
                            }
                            else if (kv[0] == System_Pong)
                            {
                                this.LastServerPongTime = DateTime.Now;
                            }
                            else if (kv[0] == System_Notify)
                            {
                                var topic_value = kv[1].Split(new char[] { ',' }, 2);
                                if (topic_value.Length == 2)
                                {
                                    try
                                    {
                                        callback(topic_value[0], topic_value[1]);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    //非正常数据 判定为非法客户端，则删除
                    if (LastReceived.Count > maxBytes)
                    {
                        throw new Exception("thream max [maxBytes]" + maxBytes + ";非法客户端，则删除");
                    }

                    if (this.CheckPing)
                    {
                        if (this.LastServerPongTime == null)
                        {
                            if (DateTime.Now.Subtract(this.LastServerPingTime).TotalMilliseconds > pingoutms)
                            {
                                throw new Exception("ping 超时，则删除");
                            }
                        }
                        else
                        {
                            if (this.LastServerPongTime.Value.Subtract(this.LastServerPingTime).TotalMilliseconds > pingoutms)
                            {
                                throw new Exception("ping 超时，则删除");
                            }
                            this.CheckPing = false;
                        }
                    }

                    if (this.CheckPing == false && DateTime.Now.Subtract(this.LastServerPingTime).TotalSeconds > PingTimeSeconds)
                    {
                        this.LastServerPingTime = DateTime.Now;
                        this.LastServerPongTime = null;
                        this.CheckPing = true;
                        Send(string.Format("{0}:{1}", System_Ping, "clientping " + DateTime.Now.Ticks));
                    }
                    if (stream.DataAvailable)
                    {
                        byte[] waitread = new byte[maxBytes];
                        int readlength = stream.Read(waitread, 0, waitread.Length);
                        if (readlength > 0)
                        {
                            LastReceived.AddRange(waitread.Take(readlength));
                        }
                    }
                    else
                    {
                        Thread.Sleep(Min_DelayTime);
                    }
                    if (!client.Connected)
                    {
                        throw new Exception("connected = false!");
                    }
                }
                catch (Exception ex)
                {
                    RLib.WatchLog.Loger.Error("ThreadDo", ex.Message);

                    while (!this.Open())
                    {
                        CloseConn();
                        System.Threading.Thread.Sleep(ReConnectSeconds * 1000);
                    }
                }
            }
        }

        public void Send(string msg)
        {
#if DEBUG
            Console.WriteLine("client发送TCP消息:" + msg.TrimEnd('\n'));
#endif
            lock (_sendLocker)
            {
                msg = msg.Replace("\n", "");
                byte[] bs = System.Text.Encoding.UTF8.GetBytes(msg + "\n");
                stream.Write(bs, 0, bs.Length);
            }
        }

        private void CloseConn()
        {
#if DEBUG
            Console.WriteLine("关闭连接...");
#endif
            try
            {
                if (client != null)
                {
                    RLib.WatchLog.Loger.Log("正在关闭tcp！", "");
                    if (client.Connected)
                        client.Close();
#if DEBUG
                    Console.WriteLine("关闭连接 成功");
#endif
                }
            }
            finally
            {
#if DEBUG
                Console.WriteLine("关闭连接完结束");
#endif
                client = null;
                stream = null;
                this.lastping = null;
                LastReceived.Clear();
            }
        }

        private void StopThread()
        {

            try
            {
                if (receive != null)
                    receive.Abort();
            }
            finally
            {
                receive = null;
            }
        }



    }
}
