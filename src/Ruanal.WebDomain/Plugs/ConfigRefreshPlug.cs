using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Plugs
{
    public class ConfigRefreshPlug : IPlug
    {
        public override void RunOnce()
        {
            Pub.ApiAnalyzeOn = Config.GetBool("ApiAnalyze", false);
            Pub.PauseWorkLog = Config.GetBool("PauseWorkLog", false);
        }
    }
}
