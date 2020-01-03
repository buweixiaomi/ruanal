using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Ruanal.ServiceSchedule
{
    internal class RuanalInnerJob : Quartz.IJob
    {

        public void Execute(Quartz.IJobExecutionContext context)
        {
            string jobId = context.JobDetail.Key.Name;
            var container = context.Scheduler.Context.Get("Container") as ScheduleContainer;
            container.CallExecute(jobId);
        }
    }
}
