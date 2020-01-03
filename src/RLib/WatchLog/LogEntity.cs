using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLib.WatchLog
{
    internal class LogEntity
    {
        public string ProjectName { get; set; }
        public long GroupID { get; set; }
        public long InnerGroupID { get; set; }
        public int LogType { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreateTime { get; set; }

        public double Elapsed { get; set; }


        public string ProjectGroup { get; set; }
        public int TimeWatchType { get; set; }
        public string Addition { get; set; }
    }
}
