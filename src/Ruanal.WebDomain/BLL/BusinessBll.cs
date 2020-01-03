using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.BLL
{
    public class BusinessBll
    {
        public List<Model.MiniEnterprise> GetEnterprise(int top, string keywords)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetBusinessConn())
            {
                return new DAL.BusinessDal().GetEnters(dbconn, keywords, top);
            }
        }
        
    }
}
