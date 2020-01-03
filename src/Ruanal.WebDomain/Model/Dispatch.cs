using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Model
{
    public class Dispatch
    {
        public int DispatchId { get; set; }
        public string GroupId { get; set; }
        public string InvokeId { get; set; }
        public int TaskId { get; set; }
        public int NodeId { get; set; }
        public int DispatchState { get; set; }
        public string NickName { get; set; }
        public string RunKey { get; set; }
        public string RunArgs { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public DateTime? DispatchTime { get; set; }
        public DateTime? ExecuteTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ResultText { get; set; }

    }
}
