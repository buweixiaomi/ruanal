using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.BLL
{
    public class TaskTagBll
    {
        DAL.TaskTagDal dal = new DAL.TaskTagDal();
        public void SaveAll(Dictionary<int, string> dic)
        {
          
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                 dal.SaveAll(dbconn, dic);
            }
        }

        public Dictionary<int, string> GetAll()
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var dic = dal.GetAll(dbconn);
                return dic;
            }
        }

        public Dictionary<int, string> ProMini(Dictionary<int, string> dic, int min = 10)
        {
            for (var k = 0; k < min; k++)
            {
                if (!dic.ContainsKey(k + 1))
                {
                    dic[k + 1] = "";
                }
            }
            return dic;
        }
    }
}
