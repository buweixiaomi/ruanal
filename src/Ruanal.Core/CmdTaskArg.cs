using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Core
{
    public class CmdTaskArg
    {
        public int TaskId { get; set; }
      //  public string CmdConfig { get; set; }

    }

    public class CmdDispatchArg
    {
        public int TaskId { get; set; }
        public int DispatchId { get; set; }
        public string InvokeId { get; set; }
    }

}
