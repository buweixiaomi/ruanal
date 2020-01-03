using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.ServiceSchedule
{
    public interface IJobExecutor
    {
        void Execute(JobContext context);
    }
}
