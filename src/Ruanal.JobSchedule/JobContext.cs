using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.ServiceSchedule
{

    public class JobContext
    {
        //  public string InnerJobId { get; set; }
        public bool IsRunOnce { get; set; }
        public string JobName { get; set; }
        public IJobExecutor OnInvoke { get; set; }
        public DateTime? LastRunTime { get; set; }
        public Quartz.IJobDetail JobDetail { get; set; }
        public List<Quartz.ITrigger> Triggers { get; private set; }
        public JobContext()
        {
            Triggers = new List<Quartz.ITrigger>();
        }
    }
}
