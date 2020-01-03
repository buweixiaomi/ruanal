using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestTask01
{
    public class TestDis4
    {
        public List<object> Getx()
        {
            var list = new List<object>();
            list.Add(new
            {
                RunKey = "a1",
                NickName = "a.1.1",
                RunArgs = "abdafsd"
            });
            list.Add(new
            {
                RunKey = "a2",
                NickName = "a.2.1",
                RunArgs = "dfafdasfasdfasdf"
            });
            return list;
        }


        public void Setx(string cfg)
        {
            Console.WriteLine("run  " + cfg);
        }

    }
}
