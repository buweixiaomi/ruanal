using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.JobSchedule
{
    public class ScheduleBuilder
    {
        public Quartz.IScheduler Scheduler = null;
        public ScheduleBuilder()
        {
            var ssf = new StdSchedulerFactory();
            Scheduler = ssf.GetScheduler();
            Scheduler.Start();
        }

        public bool PrepareJob()
        { return false; }
        pu
    }
}
