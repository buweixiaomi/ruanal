using Ruanal.Job;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TestTask01
{
    public class RunOnceDis : Ruanal.Job.DispatcherBase
    {
        string dbconnstring;

        System.Threading.Thread t = null;

        public override void Init()
        {
            return;
        }
        public override List<Ruanal.Job.DispatcherItem> GetDispatchs()
        {
            if (t != null)
                return null;
            t = new System.Threading.Thread(() =>
            {
                while (true)
                {
                    var items = new List<DispatcherItem>();
                    var count = new System.Random().Next(3, 20);
                    //while (count > 0)
                    //{
                    //    items.Add(new DispatcherItem() { NickName = "哈哈", RunArgs = "{}", RunKey = "aa_bb_" + count % 3 });
                    //    count--;
                    //}


                    items.Add(new DispatcherItem() { NickName = "哈哈", RunArgs = "{}", RunKey = "12_sofu" });
                    items.Add(new DispatcherItem() { NickName = "哈哈", RunArgs = "{}", RunKey = "34_sofutest" });
                    items.Add(new DispatcherItem() { NickName = "哈哈", RunArgs = "{}", RunKey = "44_alieia" });
                    items.Add(new DispatcherItem() { NickName = "哈哈", RunArgs = "{}", RunKey = "400_teiqs" });
                    items.Add(new DispatcherItem() { NickName = "哈哈", RunArgs = "{}", RunKey = "tq_teiqs" });

                    this.ParentCaller.PostDispatch(items);
                    System.Threading.Thread.Sleep(new System.Random().Next(200, 1500));
                }
            });
            t.Start();
            return null;
        }


    }
}
