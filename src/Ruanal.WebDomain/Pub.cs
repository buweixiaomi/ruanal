using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.WebDomain
{
    public static class Pub
    {
        /// <summary>
        /// 0 sqlserver 1mysql 
        /// </summary> 
        public const string CONN_CONFIG_NAME = "MainSqlConn";
        public const string CONN_Business_CONFIG_NAME = "BusSqlConn";

        public static bool ApiAnalyzeOn = false;
        public static bool PauseWorkLog = false;

        public static RLib.DB.DbConn GetConn()
        {
            RLib.DB.DbConn conn = RLib.DB.DbConn.CreateConn(RLib.DB.DbType.SQLSERVER,
                GetConnnectionString(CONN_CONFIG_NAME));
            conn.Open();
            return conn;
        }
        public static RLib.DB.DbConn GetBusinessConn()
        {
            RLib.DB.DbConn conn = RLib.DB.DbConn.CreateConn(RLib.DB.DbType.SQLSERVER,
                GetConnnectionString(CONN_Business_CONFIG_NAME));
            conn.Open();
            return conn;
        }

        public static string GetConfig(string key, string deval)
        {
            var conn = System.Configuration.ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(conn))
                return deval;
            return conn;
        }

        public static RLib.DB.DbConn BuildConn(string server, string database, string user, string pwd)
        {
            string sql = "server={0};database={1};user id={2};password={3};";
            string connstring = string.Format(sql, server, database, user, pwd);
            RLib.DB.DbConn conn = RLib.DB.DbConn.CreateConn(RLib.DB.DbType.SQLSERVER, connstring);
            conn.Open();
            return conn;
        }

        public static string GetConnnectionString(string key)
        {
            var conn = System.Configuration.ConfigurationManager.ConnectionStrings[key];
            if (conn == null)
                return null;
            return conn.ConnectionString;
        }

        public static int GetManagerId()
        {
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
                return 0;
            var token = Utils.GetTokenModel(System.Web.HttpContext.Current.User.Identity.Name);
            if (token == null)
                return 0;
            return RLib.Utils.Converter.ObjToInt(token.Id);
        }
        

        public static string BuildWTbName(DateTime date)
        {
            return "taskWorkLog" + date.ToString("yyMMdd");
        }

        public static bool ExistTableInDB(RLib.DB.DbConn dbconn, string tbname)
        {
            string sqlexist = "SELECT COUNT(1) FROM dbo.SysObjects where [type]='U' and name=@tablename";
            int r = dbconn.ExecuteScalar<int>(sqlexist, new { tablename = tbname });
            return r > 0;
        }
    }
}
