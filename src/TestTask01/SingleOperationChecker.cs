using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestTask01
{ 
    public class SingleOperationChecker
    {
        string opeid;
        object locker = new object();
        public void In()
        {
            lock (locker)
            {
                if (!string.IsNullOrEmpty(opeid))
                    throw new Exception("检查到非线程安全！(in)");
                opeid = Guid.NewGuid().ToString();
            }
        }

        public void Out()
        {
            lock (locker)
            {
                if (string.IsNullOrEmpty(opeid))
                    throw new Exception("检查到非线程安全！(out)");
                opeid = null;
            }
        }

        public void TryOut()
        {
            lock (locker)
            {
                opeid = null;
            }
        }

    }
}
