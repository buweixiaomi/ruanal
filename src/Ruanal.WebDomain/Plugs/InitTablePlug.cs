using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Plugs
{
    public class InitTablePlug : IPlug
    {
        const int CheckTBHours = 1;
        const int TBPreDay = 1;
        const int TBNextDay = 10;
        public override void RunOnce()
        {
            try
            {
                DateTime btime = DateTime.Now.AddDays(-TBPreDay);
                DateTime etime = btime.AddDays(TBPreDay + TBNextDay);
                List<string> tbpartnames = new List<string>();
                int max = 100;
                while (btime <= etime && max > 0)
                {
                    max--;
                    tbpartnames.Add(btime.ToString("yyMMdd"));
                    btime = btime.AddDays(1);
                }
                var filename = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(z => z.EndsWith("createWorkLog.sql"));
                if (string.IsNullOrWhiteSpace(filename))
                    return;
                var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(filename);
                var createSql = string.Empty;
                using (var sr = new System.IO.StreamReader(stream, System.Text.Encoding.Default))
                {
                    createSql = sr.ReadToEnd();
                }

                using (var dbconn = Pub.GetConn())
                {
                    foreach (var tbpartname in tbpartnames)
                    {
                        if (Pub.ExistTableInDB(dbconn, "taskWorkLog" + tbpartname))
                            continue;
                        string sql1 = createSql.Replace("{parttablename}", tbpartname);
                        dbconn.ExecuteSql(sql1);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
