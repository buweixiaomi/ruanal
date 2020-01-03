using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.BLL
{
    public class CommBll
    {
        DAL.CommDal commDal = new DAL.CommDal();

        public void SaveConfig(string config)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                commDal.SetConfig(dbconn, config);
            }
        }

        public string GetConfig()
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var v = commDal.GetConfig(dbconn);
                return v;
            }
        }
        
    }
}
