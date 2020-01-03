using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.DAL
{
    public class CommDal
    {
        public const string KEY = "_zr";
        public void SetConfig(RLib.DB.DbConn dbconn, string config)
        {
            string sql = "update RuanalCfg set [Value]=@value where [Key]=@key;";
            string sql1 = "insert into RuanalCfg([Key],[Value]) values(@key,@value);";
            var para = new { key = KEY, value = config ?? "" };
            int r = dbconn.ExecuteSql(sql, para);
            if (r == 0)
            {
                dbconn.ExecuteSql(sql1, para);
            }
            return;
        }

        public string GetConfig(RLib.DB.DbConn dbconn)
        {
            string sql = "select [Value] from RuanalCfg where [Key]=@key;";
            var val = dbconn.ExecuteScalar<string>(sql, new { key = KEY });
            return val ?? "";
        }
    }
}
