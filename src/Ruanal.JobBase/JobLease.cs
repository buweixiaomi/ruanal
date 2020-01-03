using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;

namespace Ruanal.Job
{
    public class JobLease : ILease
    {
        bool canrelease = false;

        TimeSpan renewoncalltime, sponsorshiptimeout, initialleasetime, currentleasetime;
        LeaseState currentstate;
        public void SetCanRelease()
        {
            canrelease = true;
            currentstate = LeaseState.Expired;
        }

        public TimeSpan RenewOnCallTime
        {
            get { return renewoncalltime; }
            set { renewoncalltime = value; }
        }
        public TimeSpan SponsorshipTimeout
        {
            get { return sponsorshiptimeout; }
            set { sponsorshiptimeout = value; }
        }
        public TimeSpan InitialLeaseTime
        {
            get { return initialleasetime; }
            set { initialleasetime = value; }
        }

        public TimeSpan CurrentLeaseTime
        {
            get { return currentleasetime; }
        }

        public LeaseState CurrentState
        {
            get { return currentstate; }
        }

        public JobLease()
        {
            initialleasetime = TimeSpan.FromSeconds(3);
            sponsorshiptimeout = TimeSpan.FromSeconds(10);
            renewoncalltime = TimeSpan.FromSeconds(2);
            currentleasetime = TimeSpan.FromSeconds(10);
            currentstate = LeaseState.Null;
        }
        public void Register(ISponsor obj, TimeSpan renewalTime)
        {
        }

        public void Register(ISponsor obj)
        {
        }

        public TimeSpan Renew(TimeSpan renewalTime)
        {
            if (canrelease)
                return TimeSpan.Zero;
            return renewalTime;
        }

        public void Unregister(ISponsor obj)
        {
        }
    }
}
