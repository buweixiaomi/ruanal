using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLib.WatchLog
{
    internal abstract class ILoger
    {
        public WatchLogConfig Config { get; set; }
       public abstract void WriteLog(List<LogEntity> logs);
    }
}
