using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLib.WatchLog
{
    internal class DBLoger : ILoger
    {
        public const int PreDayCount = 10;
        private DateTime? lastInit = null;

        private object init_locker = new object();

        public override void WriteLog(List<LogEntity> logs)
        {
            if (!InitTable())
                return;
            if (logs == null || logs.Count == 0)
                return;
            _WriteLog(logs);
        }

        private bool InitTable()
        {
            if (lastInit != null && (DateTime.Now - lastInit.Value).TotalHours < 24)
            {
                return true;
            }
            lock (init_locker)
            {
                if (lastInit != null && (DateTime.Now - lastInit.Value).TotalHours < 24)
                {
                    return true;
                }
                if (string.IsNullOrEmpty(Config.DBConnString))
                    return false;
                using (MySql.Data.MySqlClient.MySqlConnection dbconn = new MySql.Data.MySqlClient.MySqlConnection(Config.DBConnString))
                {
                    DateTime begindate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                    DateTime endtime = begindate.AddDays(PreDayCount);
                    dbconn.Open();
                    MySql.Data.MySqlClient.MySqlDataAdapter ada = new MySql.Data.MySqlClient.MySqlDataAdapter(" show create table timewatch;", dbconn);
                    System.Data.DataTable tb = new System.Data.DataTable();
                    ada.Fill(tb);
                    if (tb.Rows.Count == 0)
                        return false;
                    string createsql = tb.Rows[0][1].ToString();
                    createsql = createsql.Substring(createsql.IndexOf('('));

                    ada = new MySql.Data.MySqlClient.MySqlDataAdapter("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='" + dbconn.Database + "' " +
                     " and TABLE_NAME like 'timewatch________' order by TABLE_NAME desc limit 1;", dbconn);
                    tb = new System.Data.DataTable();
                    ada.Fill(tb);
                    if (tb.Rows.Count > 0)
                    {
                        string t = tb.Rows[0][0].ToString();
                        begindate = DateTime.Parse(string.Format("{0}-{1}-{2}", t.Substring(9, 4), t.Substring(13, 2), t.Substring(15, 2)));
                        begindate = begindate.AddDays(1);
                    }
                    StringBuilder sb = new StringBuilder();
                    while (begindate.CompareTo(endtime) < 0)
                    {
                        sb.AppendFormat("CREATE TABLE {0} {1};\r\n", "`timewatch" + begindate.ToString("yyyyMMdd") + "`", createsql);
                        begindate = begindate.AddDays(1);
                    }
                    if (sb.Length > 0)
                    {
                        MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand();
                        cmd.Connection = dbconn;
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandText = sb.ToString();
                        cmd.ExecuteNonQuery();
                    }
                }
                lastInit = DateTime.Now;
                return true;
            }
        }

        private void _WriteLog(List<LogEntity> logs)
        {
            if (string.IsNullOrWhiteSpace(Config.DBConnString))
                return;
            using (MySql.Data.MySqlClient.MySqlConnection dbconn = new MySql.Data.MySqlClient.MySqlConnection(Config.DBConnString))
            {
                dbconn.Open();
                BatchInsert(dbconn, logs);
            }
        }

        private void BatchInsert(MySql.Data.MySqlClient.MySqlConnection dbconn, List<LogEntity> logs)
        {
            string tablename = "timewatch" + DateTime.Now.ToString("yyyyMMdd");
            MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand();
            cmd.Connection = dbconn;
            cmd.CommandType = System.Data.CommandType.Text;
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO " + tablename + "(`projectName`,`groupID`,`innerGroupID`,`logType`,`title`,`content`,`createTime`,`createTimeMs`,`dbCreateTime`,`elapsed`)VALUES ");
            List<string> values = new List<string>();
            List<RLib.DB.ProcedureParameter> paras = new List<DB.ProcedureParameter>();
            for (int i = 0; i < logs.Count; i++)
            {
                string title = logs[i].Title ?? "";
                if (title.Length > 400)
                    title = title.Substring(0, 400);
                string appendname = "_" + i;
                values.Add(string.Format("(@projectName{0},@groupID{0},@innerGroupID{0},@logType{0},@title{0},@content{0},@createTime{0},@createTimeMs{0},now(),@elapsed{0}) ", appendname));
                cmd.Parameters.AddWithValue("projectName" + appendname, logs[i].ProjectName ?? "");
                cmd.Parameters.AddWithValue("groupID" + appendname, logs[i].GroupID);
                cmd.Parameters.AddWithValue("innerGroupID" + appendname, logs[i].InnerGroupID);
                cmd.Parameters.AddWithValue("logType" + appendname, logs[i].LogType);
                cmd.Parameters.AddWithValue("title" + appendname, title);
                cmd.Parameters.AddWithValue("content" + appendname, logs[i].Content ?? "");
                cmd.Parameters.AddWithValue("createTimeMs" + appendname, logs[i].CreateTime.Millisecond);
                //logs[i].CreateTime = logs[i].CreateTime.AddMilliseconds(logs[i].CreateTime.Millisecond);//去掉毫秒
                cmd.Parameters.AddWithValue("createTime" + appendname, logs[i].CreateTime);
                cmd.Parameters.AddWithValue("elapsed" + appendname, logs[i].Elapsed);
            }
            sb.Append(string.Join(",", values));
            cmd.CommandText = sb.ToString();
            cmd.ExecuteNonQuery();
        }

    }
}
