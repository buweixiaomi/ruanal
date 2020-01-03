using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask02
{
    public class TestTask1 : Ruanal.Job.JobServiceBase
    {
        public TestTask1()
        {
            this.JobConfig.AddConfig("key1", "名定", "啦erqwr啦啦xxx", "0");
            this.JobConfig.AddConfig("key2", "名定x", "rq", "0");
            this.JobConfig.AddConfig("key3", "名定e", "", "");
            this.JobConfig.AddConfig("key4", "名定4", "EEE啦aaaerqr啦啦", "0");
        }
        public override void Init()
        {
            base.Init();
        }
        public override void Start(string runConfig)
        {
            RLib.WatchLog.Loger.Log("哈哈", "这里是测试log");
            this.Loger.AddErrorLog("我记一下错误日志");
            this.Loger.AddLogAsyn("我记一下日志+" + DateTime.Now.ToString());
            this.Loger.AddLogAsyn("运行配置为：" + runConfig);
             
        }
    }
}
