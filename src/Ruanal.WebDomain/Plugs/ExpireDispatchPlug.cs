using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Plugs
{
    public class ExpireDispatchPlug : IPlug
    {
        DAL.DispatchDal dispatchDal = new DAL.DispatchDal();
        public override void RunOnce()
        {
            using (var dbconn = Pub.GetConn())
            {
                dispatchDal.AutoExpire(dbconn);
            }
        }
    }
}
