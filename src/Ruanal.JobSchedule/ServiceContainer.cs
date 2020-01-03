using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.ServiceSchedule
{
    public abstract class ServiceContainer
    {
        protected Ruanal.ServiceSchedule.ScheduleContainer Schedule;
        public ServiceContainer()
        {
            Schedule = new ServiceSchedule.ScheduleContainer();
        }

        public abstract void Start();

        public abstract void Stop();


    }
}
