using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Core.ApiSdk
{
    public class WorkLogEntity
    {
        public int TaskId { get; set; }
        public string DispatchId { get; set; }
        public int LogType { get; set; }
        public string LogText { get; set; }
        public string CreateTime { get; set; }
    }
}
