using Ruanal.Job.ImpDispatchs.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using K8erp.ZdService;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestTradeSave();

            Console.WriteLine("OK 完成！！！");
            Console.Read();
        }

        static void Testxxx()
        {
            var ds = new List<string[]>();
            ds.Add(new string[] { "0", "1212", "634232", "998888871.99", "2318.88" });
            ds.Add(new string[] { "1", "不我是的人啊东上", "634232", "2312.99", "218.88" });
            var sb = new StringBuilder();
            foreach (var a in ds)
            {
                sb.AppendFormat("{0} {1} # {2} {3} {4}\r\n", a[0], a[1], a[2], a[3], a[4]);
            }
            Console.WriteLine(sb.ToString());

        }

        static void TestPar()
        {
            List<int> tos = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                tos.Add(i);
            }
            CommsParallel<int> cp = new CommsParallel<int>(tos, 20, (x, y) =>
            {
                Console.WriteLine("x=" + x);
            });
            cp.WaitComplet();
            Console.WriteLine("完成！");
        }

        static void TestNotify()
        {
            Console.WriteLine("开始。。");
            //bool ok = Ruanal.Core.Notify.NotifyHelper.Init("127.0.0.1:8086", "");
            //while (true)
            //{
            //    Console.Write("请输入RunKey:");
            //    string id_key = Console.ReadLine();
            //    if (string.IsNullOrWhiteSpace(id_key))
            //    {
            //        continue;
            //    }
            //    var idkeylist = RLib.Utils.StringHelper.SplitToStringList(id_key, new char[] { ' ' }, 3);
            //    int maxCount = Convert.ToInt32(idkeylist[0]);
            //    string msg = Ruanal.Core.ConfigConst.TalkAskDispatchRunning + string.Format("{0}#{1}", idkeylist[1], idkeylist[2]);
            //    var result = Ruanal.Core.Notify.NotifyHelper.TalkToAll(msg, 3000, maxCount);
            //    Console.WriteLine("收到回复：{0}", string.Join(",", result));
            //}

            // TestWork();
            Console.WriteLine("END..");
        }

        static void TestDispatch()
        {
            Dictionary<string, object> config = new Dictionary<string, object>();
            config.Add(Ruanal.Job.ConstKey.DBConnKey, "xxxxxx");
            Ruanal.Job.DispatcherBase dis = new Ruanal.Job.ImpDispatchs.InnerEnterDis();

            //系统配置
            dis.GlobalInit(RLib.Utils.DataSerialize.SerializeJsonBeauty(config));
            //任务初始化
            dis.Init();
            var items = dis.GetDispatchs();
        }

        static void TestWork()
        {
            Console.WriteLine("开发做任务...");
            Dictionary<string, object> config = new Dictionary<string, object>();
            config.Add(Ruanal.Job.ConstKey.DBConnKey, "server=rm-vy18yhoi91f3kopl6.sqlserver.rds.aliyuncs.com,3433;Initial Catalog=k8erp_zdmain;User ID=zdsa;Password=Zdsa789456;");
            config.Add("RedisConnString", "zdredis_1234@10.25.66.35:6379");
            Ruanal.Job.JobServiceBase job = new K8erp.ZdService.RefundDeleteNoteService();// null;//new Ruanal.Job.ImpDispatchs.InnerEnterDis();
            job.Loger = new Ruanal.Job.CrossLoger("aaa", new Ruanal.Job.CrossLogerHandler((x, y, z, j) =>
            {
                Console.WriteLine("instance={0} msgType={1} msg={2} ", x, y, z);
                return true;
            }));
            job.ParentCaller = new Ruanal.Job.ParentCaller("aaa", new Ruanal.Job.ParentCallerHandler((x, y, z) =>
            {
                Console.WriteLine("instance={0} method={1}", x, y);
                return "";
            }));
            //系统配置
            job.GlobalInit(RLib.Utils.DataSerialize.SerializeJsonBeauty(config));
            //任务初始化
            job.Init();


            var runConfigJson = "{ \"qyid\": 60, \"qybm\": \"kuozaizai\", \"dpid\": 1, \"tids\": \"67195933840955530\" }";
            //非调度任务

            //job.Start(null);
            //调度任务
            try
            {
                Console.WriteLine("正在开始...");
                job.Start(runConfigJson);
                Console.WriteLine("结束.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("出错结束.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            Console.Read();
        }


        static void AutoDeleteRefundNoteNotice()
        {
            var runConfigJson = "{ \"qyid\": 60, \"qybm\": \"kuozaizai\", \"dpid\": 1, \"tids\": \"70917575888388297\" }";

            K8erp.Lib.RedisHelper.PublistRedisMsg(K8erp.Lib.RedisHelper.GetConnect("zdredis_1234@10.25.66.35:6379"), "newrefundin", runConfigJson);
        }


        static void TestDispatchWork()
        {
            Console.WriteLine("开发做任务...");
            Dictionary<string, object> config = new Dictionary<string, object>();

            config.Add(Ruanal.Job.ConstKey.DBConnKey, "server=rm-vy18yhoi91f3kopl6.sqlserver.rds.aliyuncs.com,3433;Initial Catalog=k8erp_zdmain;User ID=zdsa;Password=Zdsa789456;");
            config.Add("RedisConnString", "zdredis_1234@10.25.66.35:6379");

            Ruanal.Job.DispatcherBase job = new K8erp.ZdService.RefundDeleteNoteDispatcher();// null;//new Ruanal.Job.ImpDispatchs.InnerEnterDis();
            job.Loger = new Ruanal.Job.CrossLoger("aaa", new Ruanal.Job.CrossLogerHandler((x, y, z, j) =>
            {
                Console.WriteLine("instance={0} msgType={1} msg={2} ", x, y, z);
                return true;
            }));
            job.ParentCaller = new Ruanal.Job.ParentCaller("aaa", new Ruanal.Job.ParentCallerHandler((x, y, z) =>
            {
                Console.WriteLine("instance={0} method={1}", x, y);
                return "";
            }));
            //系统配置
            job.GlobalInit(RLib.Utils.DataSerialize.SerializeJsonBeauty(config));
            //任务初始化
            job.Init();

            //非调度任务

            //job.Start(null);
            //调度任务
            try
            {
                Console.WriteLine("正在开始...");
                job.GetDispatchs();
                Console.WriteLine("结束.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("出错结束.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            Console.Read();
        }


        private static List<string> GetJsonStruct(string parentLocation, JToken jt)
        {
            List<string> pts = new List<string>();
            switch (jt.Type)
            {
                case JTokenType.Comment:
                case JTokenType.Constructor:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Date:
                case JTokenType.Float:
                case JTokenType.Guid:
                case JTokenType.Integer:
                case JTokenType.String:
                case JTokenType.TimeSpan:
                case JTokenType.Undefined:
                case JTokenType.Uri:
                case JTokenType.Raw:
                case JTokenType.None:
                case JTokenType.Null:
                    break;
                case JTokenType.Array:
                    jt = ((JArray)jt).FirstOrDefault();
                    if (jt != null)
                    {
                        pts.AddRange(GetJsonStruct(parentLocation, jt));
                    }
                    break;
                case JTokenType.Object:
                    foreach (var a in ((JObject)jt).Children<JProperty>())
                    {
                        pts.Add(parentLocation + a.Name);
                        pts.AddRange(GetJsonStruct(parentLocation + a.Name + ".", a.Value));
                    }
                    break;
                case JTokenType.Property:
                    JProperty jp = jt as JProperty;
                    pts.Add(parentLocation + jp.Name);
                    pts.AddRange(GetJsonStruct(parentLocation + jp.Name + ".", jp.Value));
                    break;
                default:
                    break;
            }
            return pts;
        }


        private static void CompareJsonStru()
        {
            Dictionary<string, KeyItem> aa = new Dictionary<string, KeyItem>();
            var runConfig = new Ruanal.Job.ImpDispatchs.Entity.EnterpriseDisWithDBInfoEntity()
            {
                dbtype = 0,
                dbserver = "conn0894z4r7.sqlserver.rds.aliyuncs.com",
                dbuser = "zd_main",
                dbpass = "Zdmain123456",
                rdssl = "ins_0894z4r7",
                zzid = 1,
                qyid = 3,
                qybm = "yaoyao",
                qymc = "要要童装",
                dbserverid = 2,
                dbname = "zhuidian_yaoyao"
            };
            int allcount = 0;
            using (var db = RLib.DB.DbConn.CreateConn(RLib.DB.DbType.SQLSERVER,
                "conn0894z4r7.sqlserver.rds.aliyuncs.com",
                "3433", "sys_info", "zd_main", "Zdmain123456"))
            {
                db.Open();
                string sql = "select jdp_response from [sys_info].[dbo].[jdp_tb_item]";
                var jsons = db.SqlToModel<string>(sql, null);
                allcount = jsons.Count;
                foreach (string j in jsons)
                {
                    var jobj = JObject.Parse(j);
                    List<string> result = GetJsonStruct("", jobj);
                    result = result.Distinct().ToList();
                    result.Sort();
                    var skey = string.Join("\r\n", result);
                    var key = skey.GetHashCode().ToString();


                    if (aa.ContainsKey(key))
                    {
                        aa[key].count++;
                    }
                    else
                    {
                        aa.Add(key, new KeyItem()
                        {
                            count = 1,
                            ps = result
                        });
                    }
                }
            }
            Dictionary<string, ItemItem> bbb = new Dictionary<string, ItemItem>();
            foreach (var a in aa)
            {
                foreach (var b in a.Value.ps)
                {
                    if (bbb.ContainsKey(b))
                    {
                        bbb[b].containtcount += a.Value.count;
                    }
                    else
                    {
                        bbb.Add(b, new ItemItem() { containtcount = a.Value.count, item = b });
                    }
                }
            }
            var ccc = new
            {
                allcount = allcount,
                items = bbb.Select(x => x.Value).Where(x => x.containtcount != allcount).OrderBy(x => x.containtcount).ToList()
            };
            System.IO.File.WriteAllText("E:\\result.com.result2.txt", RLib.Utils.DataSerialize.SerializeJsonBeauty(ccc),
                System.Text.Encoding.GetEncoding("utf-8"));

            System.IO.File.WriteAllText("E:\\result.com.result.txt", string.Join("\r\n\r\nr\n\r\n", aa.Select(x => string.Format("{0}\t{1}\r\n{2}", x.Key, x.Value.count, string.Join("\r\n", x.Value.ps)))),
                System.Text.Encoding.GetEncoding("utf-8"));
        }

        private static void TestShopDownware()
        {
            EnterpriseDisWithDBInfoEntity enterInfo = new Ruanal.Job.ImpDispatchs.Entity.EnterpriseDisWithDBInfoEntity()
            {
                dbtype = 0,
                dbserver = "conn0894z4r7.sqlserver.rds.aliyuncs.com,3433",
                dbuser = "zd_main",
                dbpass = "Zdmain123456",
                rdssl = "ins_0894z4r7",
                zzid = 1,
                qyid = 2,
                qybm = "uoou",
                qymc = "杭州淘萌",
                dbserverid = 2,
                dbname = "zhuidian_uoou"
            };
            try
            {
                Console.WriteLine("开始...");
                using (K8erp.Lib.Db.DbConn qyconn = K8erp.Lib.Db.DbConn.CreateConn(
                    (K8erp.Lib.Db.DbType)enterInfo.dbtype, enterInfo.dbserver, enterInfo.dbname, enterInfo.dbuser, enterInfo.dbpass))
                {
                    qyconn.Open();
                    List<K8erp.Platform.Domain.Store> shops = K8erp.Platform.Dal.Bll.GetQyStores(qyconn);
                    var shop = shops.FirstOrDefault(x => x.Dpid == 43);
                    if (shop == null)
                    {
                        Console.WriteLine("没有找到店铺");
                        return;
                    }
                    Console.WriteLine("开始...");

                    K8erp.Platform.Dal.BllWare2.DownWare(qyconn, shop);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                var innex = ex.InnerException;
                while (innex != null)
                {
                    Console.WriteLine(innex.Message);
                    Console.WriteLine(innex.StackTrace);
                    innex = innex.InnerException;
                }
            }
            finally
            {
                Console.WriteLine("结束");
            }

        }
        //

        private static void TestTaobaoRdsWare()
        {
            var store = new K8erp.Platform.Domain.Store()
            {
                Dpid = 9998,
                Dplx = K8erp.Platform.Domain.StoreTypeEmun.Taobao,
                Key1 = "",
                Key2 = "",
                Key3 = "悠悠童话婴童潮品",
                Key4 = "",
                SellerId = "悠悠童话婴童潮品",
                Session = "aaaaa",

            };

            EnterpriseDisWithDBInfoEntity enterInfo = new Ruanal.Job.ImpDispatchs.Entity.EnterpriseDisWithDBInfoEntity()
            {
                dbtype = 0,
                dbserver = "conn0894z4r7.sqlserver.rds.aliyuncs.com,3433",
                dbuser = "zd_main",
                dbpass = "Zdmain123456",
                rdssl = "ins_0894z4r7",
                zzid = 1,
                qyid = 2,
                qybm = "test2",
                qymc = "test2",
                dbserverid = 2,
                dbname = "zhuidian_test2"
            };
            try
            {
                Console.WriteLine("开始...");
                using (K8erp.Lib.Db.DbConn qyconn = K8erp.Lib.Db.DbConn.CreateConn(
                    (K8erp.Lib.Db.DbType)enterInfo.dbtype, enterInfo.dbserver, enterInfo.dbname, enterInfo.dbuser, enterInfo.dbpass))
                {
                    qyconn.Open();

                    //K8erp.Platform.Dal.BllWare2.DownloadWares(2, qyconn, store);
                    //Console.WriteLine("完成全量！");
                    //Console.ReadLine();


                    //K8erp.Platform.Dal.BllWare2.DownloadWares(0, qyconn, store);
                    //Console.WriteLine("完成online！");
                    //Console.ReadLine();


                    //K8erp.Platform.Dal.BllWare2.DownloadWares(1, qyconn, store);
                    //Console.WriteLine("完成stock！");
                    //Console.ReadLine();


                    K8erp.Platform.Dal.BllWare2.DownloadWares(3, qyconn, store);
                    Console.WriteLine("完成增量！");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("结束");
            }
        }


        private static void TestBeibeiWare()
        {
            var store = new K8erp.Platform.Domain.Store()
            {
                Dpid = 9990,
                Dplx = K8erp.Platform.Domain.StoreTypeEmun.Mogujie,
                Key1 = "",
                Key2 = "",
                Key3 = "ttt",
                Key4 = "",
                SellerId = "ttt",
                Session = "517BAD1B9AF60730CB665135B812F292",

            };

            //EnterpriseDisWithDBInfoEntity enterInfo = new Ruanal.Job.ImpDispatchs.Entity.EnterpriseDisWithDBInfoEntity()
            //{
            //    dbtype = 0,
            //    dbserver = "conn0894z4r7.sqlserver.rds.aliyuncs.com,3433",
            //    dbuser = "zd_main",
            //    dbpass = "Zdmain123456",
            //    rdssl = "ins_0894z4r7",
            //    zzid = 1,
            //    qyid = 2,
            //    qybm = "test2",
            //    qymc = "test2",
            //    dbserverid = 2,
            //    dbname = "zhuidian_test2"
            //};
            EnterpriseDisWithDBInfoEntity enterInfo = new Ruanal.Job.ImpDispatchs.Entity.EnterpriseDisWithDBInfoEntity()
            {
                dbtype = 0,
                dbserver = ".",
                dbuser = "sa",
                dbpass = "123",
                rdssl = "ins_0894z4r7",
                zzid = 1,
                qyid = 2,
                qybm = "test2",
                qymc = "test2",
                dbserverid = 2,
                dbname = "zhuidian_testqy1"
            };
            try
            {
                Console.WriteLine("开始...");
                using (K8erp.Lib.Db.DbConn qyconn = K8erp.Lib.Db.DbConn.CreateConn(
                    (K8erp.Lib.Db.DbType)enterInfo.dbtype, enterInfo.dbserver, enterInfo.dbname, enterInfo.dbuser, enterInfo.dbpass))
                {
                    qyconn.Open();

                    K8erp.Platform.Dal.BllWare2.DownloadWares(2, qyconn, store);
                    Console.WriteLine("完成全量！");
                    Console.ReadLine();


                    //K8erp.Platform.Dal.BllWare2.DownloadWares(0, qyconn, store);
                    //Console.WriteLine("完成online！");
                    //Console.ReadLine();


                    //K8erp.Platform.Dal.BllWare2.DownloadWares(1, qyconn, store);
                    //Console.WriteLine("完成stock！");
                    //Console.ReadLine();


                    //K8erp.Platform.Dal.BllWare2.DownloadWares(3, qyconn, store);
                    //Console.WriteLine("完成增量！");
                    //Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("结束");
            }
        }

        private static void TestRdsTrade()
        {
            var store = new K8erp.Platform.Domain.Store()
            {
                Dpid = 9990,
                Dplx = K8erp.Platform.Domain.StoreTypeEmun.Tmall,
                Key1 = "",
                Key2 = "",
                Key3 = "ttt",
                Key4 = "",
                SellerId = "妞妞家族旗舰店",
                Session = "6201a204451c0dfdea83a9ec2506ZZc3efh0a2540fd5bf51776287027",
                Expires = "",
                Re_Expires = ""
            };
            EnterpriseDisWithDBInfoEntity enterInfo = new Ruanal.Job.ImpDispatchs.Entity.EnterpriseDisWithDBInfoEntity()
            {
                dbtype = 0,
                dbserver = "jconntkgxcygg.sqlserver.rds.aliyuncs.com,3433",
                dbuser = "zd_main",
                dbpass = "Zdmain123456",
                rdssl = "jrdsmhcfizqj",
                zzid = 1,
                qyid = 21,
                qybm = "nnjz",
                qymc = "家族",
                dbserverid = 3,
                dbname = "zhuidian_nnjz"
            };
            using (K8erp.Lib.Db.DbConn qyconn = K8erp.Lib.Db.DbConn.CreateConn(
                 (K8erp.Lib.Db.DbType)enterInfo.dbtype, enterInfo.dbserver, enterInfo.dbname, enterInfo.dbuser, enterInfo.dbpass))
            {
                qyconn.Open();
                K8erp.Platform.Dal.TaoBaoRdsOpen rdsopen = new K8erp.Platform.Dal.TaoBaoRdsOpen(qyconn, store);

                var trademodel = rdsopen.PartTrade(new K8erp.Platform.ApiRequest.PartTradeRequest() { Tid = "24216943691019110", ServerDate = DateTime.Now });
                var sss = Newtonsoft.Json.JsonConvert.SerializeObject(trademodel, Formatting.Indented);
                Console.WriteLine(sss);
            }
        }

        private static void TestPddTrade()
        {
            var store = new K8erp.Platform.Domain.Store()
             {
                 Dpid = 8,
                 Dplx = K8erp.Platform.Domain.StoreTypeEmun.Pinduoduo,
                 Key1 = "74390",
                 Key2 = "pinduoduo",
                 Session = "aaa",
                 Expires = "",
                 Dpmc = "拼多多-煜炬皮具专营店",
                 Re_Expires = "",
                 Key3 = "",
                 SellerId = ""
             };

            K8erp.Platform.Open.PinduoduoOpen2 pdd = new K8erp.Platform.Open.PinduoduoOpen2(store.Key1, store.Key2);
            pdd.OpenStatus = K8erp.Platform.Domain.OpenStatus.Regular;
            var ttt = pdd.Trade(new K8erp.Platform.ApiRequest.TradeRequest() { PageNo = 1, Status = -9, ServerDate = DateTime.Now });

            int kkkkk = 0;
            return;

            EnterpriseDisWithDBInfoEntity enterInfo = new Ruanal.Job.ImpDispatchs.Entity.EnterpriseDisWithDBInfoEntity()
            {
                dbtype = 0,
                dbserver = "rm-vy11h8bio01g37y53.sqlserver.rds.aliyuncs.com,3433",
                dbuser = "zd_main",
                dbpass = "Zdmain123456",
                rdssl = "rm-vy11h8bio01g37y53",
                zzid = 1,
                qyid = 21,
                qybm = "daolang",
                qymc = "稻浪电子商务",
                dbserverid = 5,
                dbname = "zhuidian_daolang"
            };
            using (K8erp.Lib.Db.DbConn qyconn = K8erp.Lib.Db.DbConn.CreateConn(
                 (K8erp.Lib.Db.DbType)enterInfo.dbtype, enterInfo.dbserver, enterInfo.dbname, enterInfo.dbuser, enterInfo.dbpass))
            {
                qyconn.Open();
                K8erp.Platform.Dal.BllTrade.DownFullTradeV2(qyconn, enterInfo.qyid, store, (x, y) =>
                {
                    Console.WriteLine("ok={0} msg={1}", x, y);
                });

            }

        }


        private static void TestMogujieTrade()
        {
            var store = new K8erp.Platform.Domain.Store()
            {
                Dpid = 128,
                Dplx = K8erp.Platform.Domain.StoreTypeEmun.Meilishuo,
                Key1 = "",
                Key2 = "",
                Key4 = "",
                Session = "517BAD1B9AF60730CB665135B812F292",
                Expires = "",
                Dpmc = "淘气麦兜店",
                Re_Expires = "",
                Key3 = "",
                SellerId = ""
            };

            EnterpriseDisWithDBInfoEntity enterInfo = new Ruanal.Job.ImpDispatchs.Entity.EnterpriseDisWithDBInfoEntity()
            {
                dbtype = 0,
                dbserver = "conn0894z4r7.sqlserver.rds.aliyuncs.com,3433",
                dbuser = "zd_main",
                dbpass = "Zdmain123456",
                rdssl = "rm-vy11h8bio01g37y53",
                zzid = 1,
                qyid = 21,
                qybm = "md",
                qymc = "淘气麦兜",
                dbserverid = 5,
                dbname = "zhuidian_md"
            };
            using (K8erp.Lib.Db.DbConn qyconn = K8erp.Lib.Db.DbConn.CreateConn(
                 (K8erp.Lib.Db.DbType)enterInfo.dbtype, enterInfo.dbserver, enterInfo.dbname, enterInfo.dbuser, enterInfo.dbpass))
            {
                qyconn.Open();
                K8erp.Platform.Dal.BllTrade.DownFullTradeV2(qyconn, enterInfo.qyid, store, (x, y) =>
                {
                    Console.WriteLine("ok={0} msg={1}", x, y);
                });

            }

        }


        private static void TestTradeSave()
        {

            var store = new K8erp.Platform.Domain.Store()
            {
                Dpid = 100,
                Dplx = K8erp.Platform.Domain.StoreTypeEmun.Jingdong,
                Dpmc = "",
                Expires = "",
                Key1 = "",
                Key2 = "",
                Key3 = "",
                Key4 = "",
                Re_Expires = "",
                Refresh = "",
                Session = "aa3bd754-5a88-4443-aeba-6caa76a91d28"
            };

            var open = K8erp.Platform.OpenApiHelper.CreateOpen(null, store);
            string connstr = "server=114.55.102.107,3433;Initial Catalog=zhuidian_qy1;User ID=sa;Password=haitian~!@#$%;";
            var trades = open.Trade(new K8erp.Platform.ApiRequest.TradeRequest() { PageNo = 1, Status = 0, ServerDate = DateTime.Now });
            using (var dbconn = K8erp.Lib.Db.DbConn.CreateConn(K8erp.Lib.Db.DbType.SQLSERVER, connstr))
            {
                dbconn.Open();
                foreach (K8erp.Platform.Domain.Trade t in trades.Response.Trades)
                {
                   // string offtid = K8erp.Platform.Dal.BllTrade.SaveTrade2(dbconn, DateTime.Now, 1, store, 1, 1, t, true);
                  //  Console.WriteLine(offtid + " offtid");
                }
            }
            Console.WriteLine(trades.Msg);
            object cs = new { };
            int k = 0;
        }


    }

    public class KeyItem
    {
        public List<string> ps { get; set; }
        public int count { get; set; }
    }

    public class ItemItem
    {
        public string item { get; set; }
        public int containtcount { get; set; }
    }
}
