using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.BLL
{
    public class CmdBll
    {
        DAL.CmdDal cmddal = new DAL.CmdDal();
        public PageModel<Model.Cmd> PageCmd(int? nodeid,int? cmdState, DateTime? begintime, DateTime? endtime, int pno)
        {
            int pagesize = 20;
            int totalcount = 0;
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var tasks = cmddal.GetCmdList(dbconn, begintime, endtime, nodeid, cmdState, pno, pagesize, out totalcount);
                return new PageModel<Model.Cmd>()
                {
                    List = tasks,
                    PageNo = pno,
                    PageSize = pagesize,
                    TotalCount = totalcount
                };
            }
        }

        public int DeleteCmd(int cmdid)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                cmddal.DeleteCmd(dbconn, cmdid);
                return 1;
            }
        }
    }
}
