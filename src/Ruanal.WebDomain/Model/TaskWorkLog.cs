using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Model
{
    public class TaskWorkLog
    {
        public int LogId { get; set; }
        public int TaskId { get; set; }
        public int NodeId { get; set; }
        public string DispatchId { get; set; }
        public DateTime ServerTime { get; set; }
        /// <summary>
        /// -- 0:一般日志  1:重要日志  2:错误日志
        /// </summary>
        public int LogType { get; set; }
        public string LogText { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
