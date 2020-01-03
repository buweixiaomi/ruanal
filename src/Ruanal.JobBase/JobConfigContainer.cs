using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Job
{
    public class JobConfigContainer : List<JobConfigItem>
    {
        public JobConfigContainer AddConfig(string key, string title, string desc, string defaultvalue)
        {
            this.Add(new JobConfigItem() { key = key, title = title, desc = desc, defaultvalue = defaultvalue });
            return this;
        }
    }
    public class JobConfigItem
    {
        public string key { get; set; }
        public string title { get; set; }

        public string desc { get; set; }
        public string defaultvalue { get; set; }

    }
}
