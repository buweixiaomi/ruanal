using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var ps = JsonConvert.SerializeObject(new { a = "a", b = 123 });
                Console.WriteLine(ps);
            }
            catch (Exception ex)
            {

            }

        }
    }
}
