using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.ServiceSchedule
{
    public class ImpJobExecutor : IJobExecutor
    {
        Action<JobContext> _action;
        public ImpJobExecutor(Action<JobContext> action)
        {
            _action = action;
        }
        public void Execute(JobContext context)
        {
            _action(context);
        }
    }
}
