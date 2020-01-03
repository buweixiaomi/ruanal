using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManageSDK
{
    class Program
    {
        static void Main(string[] args)
        {
            Ruanal.ManageApi.Demo d = new Ruanal.ManageApi.Demo();
            d.Test();
            Console.Read();
        }
    }
}
