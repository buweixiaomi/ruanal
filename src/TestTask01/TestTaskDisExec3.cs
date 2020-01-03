using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestTask01
{
    public class TestTaskDisExec3 : Ruanal.Job.JobServiceBase
    {
        public TestTaskDisExec3()
        {
            //如果为true，需要动动设置 IsRuning= false;
            this.IsThreadJob = false;
        }
        public override void Init()
        {
            base.Init();
        }
        SingleOperationChecker soc = new SingleOperationChecker();
        public override void Start(string runConfig)
        {
            soc.In();
            try
            {
                Loger.AddLogAsyn("运行任务调度：当前配置 " + runConfig + " " + DateTime.Now.Ticks);
                //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(new Random().Next(0, 5)));
                if ((new Random().Next(0, 5)) == 3)
                    throw new Exception("bero");
                return;
            }
            finally
            {
                soc.Out();
            }


            for (int i = 0; i < 100; i++)
            {
                this.Loger.AddLogAsyn("时间{0} 序号{1}", DateTime.Now.Ticks, i);
            }
            string v = this.GetTaskConfig(Ruanal.Job.ConstKey.DBConnKey);
            var st1 = System.Diagnostics.Stopwatch.StartNew();
            Loger.AddLogAsyn("运行任务调度：当前配置 " + runConfig);
            st1.Stop();
            Console.WriteLine("写日志1用时:{0}ms", st1.Elapsed.TotalMilliseconds.ToString("0.0"));
            var cmodel = RLib.Utils.DataSerialize.DeserializeObject<ExecConfigEntity>(runConfig);
            st1.Restart();
            Loger.AddLogAsyn("运行任务调度：当前CurrId " + cmodel.Curr);
            st1.Stop();
            Console.WriteLine("写日志2用时:{0}ms", st1.Elapsed.TotalMilliseconds.ToString("0.0"));
            Loger.AddErrorLogAsyn("假错误日志 " + cmodel.Curr);
            Loger.AddErrorLogAsyn("假错误日志2 " + cmodel.Curr);
            //  System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));

        }
    }
}
