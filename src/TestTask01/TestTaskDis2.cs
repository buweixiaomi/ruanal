using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestTask01
{
    public class TestTaskDis2 : Ruanal.Job.DispatcherBase
    {
        int From;
        int End;

        /// <summary>
        /// 只执行一次
        /// </summary>
        public override void Init()
        {
            From = RLib.Utils.Converter.StrToInt(this.GetTaskConfig("from", "1"));
            End = RLib.Utils.Converter.StrToInt(this.GetTaskConfig("end", "100"));
        }

        public override List<Ruanal.Job.DispatcherItem> GetDispatchs()
        {
            List<Ruanal.Job.DispatcherItem> dispatchconfigs = new List<Ruanal.Job.DispatcherItem>();
            //dispatchconfigs.Add(new Ruanal.Job.DispatcherItem()
            //{
            //    NickName = "TestDispatch",
            //    RunKey = "TestDispatch_01",
            //    RunArgs = "{}"
            //});
            //return dispatchconfigs;



            for (int i = 1; i <= 50; i++)
            {

                dispatchconfigs.Add(
                     new Ruanal.Job.DispatcherItem()
                     {
                         NickName = "测试分配",
                         RunKey = "abc_" + i,
                         RunArgs = "{}"
                     });
            }
            return dispatchconfigs;
        }
    }
}
