using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ruanal.Core.Notify
{
    public delegate void ReveiveMsgHandler(string topic, string msg);
    public delegate string TalkHandler(string msg);
    public class NotifyHelper
    {

        public const string NotifyTalkTopic = "__systalk";
        public static ReveiveMsgHandler OnReveiveMsg;
        public static TalkHandler OnTalk;
        private static Dictionary<int, TalkItem> talkrequests = new Dictionary<int, TalkItem>();
        private static object talkrequestslocker = new object();
        static int _requestID { get; set; }
        static object _requestidlocker = new object();
        private static int requestID
        {
            get
            {
                lock (_requestidlocker)
                {
                    _requestID++;
                    if (_requestID == int.MaxValue)
                        _requestID = 1;
                    return _requestID;
                }
            }
        }

        static TcpNotifyClient client;

        static object initlockers = new object();

        private static void OnReceiveTalkBack(int id, string msg)
        {
            lock (talkrequestslocker)
            {
                if (talkrequests.ContainsKey(id))
                {
                    var waitItem = talkrequests[id];
                    waitItem.Response.Add(msg);
                    if (waitItem.MaxCount > 0 && waitItem.Response.Count >= waitItem.MaxCount)
                    {
                        waitItem.MRE.Set();
                    }
                }
            }
        }
        private static void ReplyTalk(string reqmsg)
        {
            var msgarr = reqmsg.Split(new char[] { '#' }, 3);
            if (msgarr.Length != 3)
                return;
            int id = RLib.Utils.Converter.StrToInt(msgarr[0]);
            int ty = RLib.Utils.Converter.StrToInt(msgarr[1]);
            string msg = msgarr[2];
            if (ty == 0)
            {
                try
                {
                    if (OnTalk == null)
                        return;
                    string resmsg = OnTalk.Invoke(msg);
                    if (resmsg == null)
                        return;
                    resmsg = resmsg.Replace("\r\n", "").Replace("\n", "");
                    string requestMsg = string.Format("{0}#{1}#{2}", id, "1", resmsg);
                    NotifyHelper.Notify(NotifyTalkTopic, requestMsg);
                }
                catch (Exception ex) { }
            }
            else if (ty == 1)
            {
                OnReceiveTalkBack(id, msg);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="server"></param>
        /// <param name="topics"></param>
        /// <returns></returns>
        public static bool Init(string name, string server, string topics)
        {
            name = (name ?? "").Replace(":", "").Trim();
            if (client != null)
                return true;
            lock (initlockers)
            {
                if (client != null)
                    return true;
                //加入 系统 topic
                var newtopic = (topics ?? "").Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                newtopic.Add(NotifyTalkTopic);
                topics = string.Join(",", newtopic);

                if (string.IsNullOrWhiteSpace(server))
                    throw new Exception("notifer error");
                string[] s_p = server.Split(new char[] { ',', ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (s_p.Length != 2)
                    throw new Exception("notifer error");
                var tclient = new TcpNotifyClient(s_p[0], Convert.ToInt32(s_p[1]), "senderandlistener", topics, name, (x, y) =>
                 {
                     if (x == NotifyTalkTopic)
                     {
                         ReplyTalk(y);
                     }
                     else
                     {
                         if (OnReveiveMsg != null)
                             OnReveiveMsg.Invoke(x, y);
                     }
                 });
                client = tclient;
                if (tclient.Open())
                {
                    tclient.StartReceive();
                    return true;
                }
                tclient.StartReceive();
                return false;
            }
        }
        /// <summary>
        /// topic不能包含有 \n ,
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public static void Notify(string topic, string msg)
        {
            if (topic.IndexOf('\n') >= 0 || topic.IndexOf(',') >= 0)
                throw new Exception("topic中不能包含回车和逗号");
            if (msg.IndexOf('\n') >= 0)
                throw new Exception("msg中不能包含回车和逗号");
            client.Send(string.Format("{0}:{1},{2}", TcpNotifyServer.System_Notify, topic, msg));
        }
        /// <summary>
        /// 发送谈话，等待时间后，返回收到的回答
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="waitms"></param>
        /// <returns></returns>
        public static List<string> TalkToAll(string msg, int waitms = 3000, int maxCount = 0)
        {
            msg = msg.Replace("\r\n", "").Replace("\n", "");
            int id = requestID;
            string requestMsg = string.Format("{0}#{1}#{2}", id, "0", msg);
            try
            {
                Notify(NotifyTalkTopic, requestMsg);
            }
            catch (Exception ex)
            { return new List<string>(); }
            var waititem = new TalkItem(id, maxCount);
            lock (talkrequestslocker)
            {
                talkrequests.Add(waititem.RequestID, waititem);
            }
            waititem.MRE.WaitOne(waitms);
            lock (talkrequestslocker)
            {
                if (talkrequests.ContainsKey(id))
                {
                    var item = talkrequests[id];
                    talkrequests.Remove(id);
                    return item.Response;
                }
                else
                    return new List<string>();
            }
        }

        class TalkItem
        {
            public int RequestID { get; private set; }
            public List<string> Response { get; set; }
            public int MaxCount { get; private set; }
            public ManualResetEvent MRE { get; private set; }
            public TalkItem(int id, int maxCount)
            {
                RequestID = id;
                MaxCount = maxCount;
                MRE = new ManualResetEvent(false);
                Response = new List<string>();
            }
        }
    }
}
