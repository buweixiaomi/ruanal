using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestTask01
{
    public class TestTask3
    {

        //任务的json配置会到这里来
        public Dictionary<string, string> TaskConfig { get; set; }
        public void Start(string arg)
        {
            Console.WriteLine("Start... ");
        }

        public void Start2()
        {
            Console.WriteLine("Start2... ");
        }

        //可选
        public void Stop()
        {
            Console.WriteLine("Stop... ");
        }
    }
}
