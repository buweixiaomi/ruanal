using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Model
{
    public class Cmd
    {
        public int CmdId { get; set; }
        public int NodeId { get; set; }
        public string CmdType { get; set; }
        public string CmdArgs { get; set; }
        public DateTime CreateTime { get; set; }
        public int CmdState { get; set; }
        public DateTime? CallTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ResultText { get; set; }
    }
}
