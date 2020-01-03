using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Core
{
    public class DispatchArg
    {
        public int DispatchId { get; set; }
        public string GroupId { get; set; }
        public string InvokeId { get; set; }
        public int TaskId { get; set; }
        public string RunArgs { get; set; }
        public string NickName { get; set; }
        public string RunKey { get; set; }
    }
}
