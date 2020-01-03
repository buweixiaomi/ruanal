using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestTask01
{
    public class TestTask2 : Ruanal.Job.JobServiceBase
    {
        public override void Init()
        {
            base.Init();
        }
        public override void Start(string runConfig)
        {
            RLib.WatchLog.Loger.Log("哈哈", "这里是测试log");
            this.Loger.AddErrorLog("我记一下错误日志");
            this.Loger.AddLogAsyn("我记一下日志+");
            this.Loger.AddLogAsyn("运行配置为：" + runConfig);
            Console.WriteLine("运行一次啊啊" + DateTime.Now.Ticks);
        }
    }
}
