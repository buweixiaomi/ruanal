using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Plugs
{
    public class EndStopedDispatchPlug : IPlug
    {
        BLL.TaskBll taskbll = new BLL.TaskBll();
        public override void RunOnce()
        {
            taskbll.AutoEndEnd();
        }
    }
}
