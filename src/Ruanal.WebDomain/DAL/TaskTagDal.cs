using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.DAL
{
    public class TaskTagDal
    {
        public void SaveAll(RLib.DB.DbConn dbconn, Dictionary<int, string> tags)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("delete from tasktag;");
            List<RLib.DB.ProcedureParameter> paras = new List<RLib.DB.ProcedureParameter>();
            int index = 0;
            foreach (var a in tags)
            {
                index++;
                sb.AppendLine(string.Format("insert into tasktag(tagindex,tagname) values (@tagindex{0},@tagname{0});", index));
                paras.Add(new RLib.DB.ProcedureParameter("tagindex" + index, a.Key));
                paras.Add(new RLib.DB.ProcedureParameter("tagname" + index, a.Value));
            }
            dbconn.ExecuteSql(sb.ToString(), paras);
        }

        public Dictionary<int, string> GetAll(RLib.DB.DbConn dbconn)
        {
            string sql = "select tagindex,tagname from tasktag order by tagindex;";
            var tb = dbconn.SqlToDataTable(sql, null);
            Dictionary<int, string> rdata = new Dictionary<int, string>();
            foreach (System.Data.DataRow dr in tb.Rows)
            {
                rdata[RLib.Utils.Converter.ObjToInt(dr["tagindex"])] = dr["tagname"].ToString();
            }
            return rdata;
        }
    }
}
