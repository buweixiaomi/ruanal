using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp2
{
    public class Class1
    {
        public void Function1()
        {
            Console.WriteLine("Begin: This is Function1 徐品" + DateTime.Now.Ticks);

            JsonUse();

            Console.WriteLine("End: This is Function1 徐品" + DateTime.Now.Ticks);
        }

        private void JsonUse()
        {
            try
            {
                var ps = JsonConvert.SerializeObject(new { a = "a", b = 123 });
            }
            catch (Exception ex)
            {
                Console.WriteLine("JsonUse Error:" + ex.Message);
            }
        }

    }
}
