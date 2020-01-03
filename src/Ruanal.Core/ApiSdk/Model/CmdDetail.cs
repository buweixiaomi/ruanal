using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Core.ApiSdk
{
    public class CmdDetail
    {
        public int CmdId { get; set; }
        public int NodeId { get; set; }
        public string CmdType { get; set; }
        public string CmdArgs { get; set; }

        public CmdTaskArg TaskArg { get; set; }
        public CmdDispatchArg DispatchArg { get; set; }

    }
}
