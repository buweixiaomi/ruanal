using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestTask01
{
    public class TestTask1 : Ruanal.Job.JobServiceBase
    {
        public override void Init()
        {
            base.Init();
            this.IsThreadJob = true;
        }
        public override void Start(string runConfig)
        {
            RLib.WatchLog.Loger.Log("哈哈", "这里是测试log");
            this.Loger.AddErrorLog("我记一下错误日志");
            this.Loger.AddLogAsyn("我记一下日志+");
            this.Loger.AddLogAsyn("运行配置为：" + runConfig);

            System.Threading.Thread t = new System.Threading.Thread((x) =>
            {
                for (int i = 0; i < 100; i++)
                {
                    this.Loger.AddLogAsyn("时间{0} 序号{1}", DateTime.Now.Ticks, i);
                }
                this.ThreadJobEnd();
            });
            t.Start();

            var config = this.ParentCaller.ReGetConfig();

            int ddd = 0;
        }
    }
}
