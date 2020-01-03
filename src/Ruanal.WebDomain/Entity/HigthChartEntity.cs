using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Entity
{
    public class HigthChartEntity
    {
        public string title { get; set; }
        public string xtitle { get; set; }
        public string ytitle { get; set; }
        public string xunit { get; set; }
        public string yunit { get; set; }
        public List<HigthChartEntity_Series> series { get; set; }
    }
    public class HigthChartEntity_Series
    {
        public string name { get; set; }
        public List<object> data { get; set; }

    }
}
