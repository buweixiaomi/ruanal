using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.DAL
{
    public class BusinessDal
    {
        public List<Model.MiniEnterprise> GetEnters(RLib.DB.DbConn dbconn, string keywords, int topcount = 0)
        {
           string sql = "select {0} f_enterpriseid as f_qyid,f_shortname as f_qybm,f_fullname as f_qymc from tbmain_enterprise {1}";
             // string sql = "select {0} f_qybm,f_qymc from tbdesign_enterprise {1}";
            string where = " where [f_status]=0 ";
            if (!string.IsNullOrEmpty(keywords))
            {
                where += " and (f_shortname like '%'+@keywords +'%' or f_fullname like '%'+@keywords +'%' )";
            }
            string querysql = string.Format(sql, topcount > 0 ? (" top " + topcount) : "", where);
            var ents = dbconn.Query<Model.MiniEnterprise>(querysql, new { keywords = keywords ?? "" });
            return ents;
        }
    }
}
