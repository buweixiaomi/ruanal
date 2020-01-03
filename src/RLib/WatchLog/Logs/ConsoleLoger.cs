using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RLib.WatchLog
{
    internal class ConsoleLoger : ILoger
    {
        public override void WriteLog(List<LogEntity> logs)
        {

            foreach (var a in logs)
            {
                string mm = string.Format("【项目】:{0};\r\n【大分组】:{1};\t【小分组】:{2};\r\n【日志类型】:{3};\r\n【标题】:{4};\r\n【内容】:{5};\r\n【createtime】:{6};\r\n\r\n",
                       a.ProjectName ?? "", a.GroupID, a.InnerGroupID, "普通日志", a.Title, a.Content ?? "", a.CreateTime.ToString(Config.DateTimeFormat));

                System.Diagnostics.Trace.WriteLine(mm);
                if (a.LogType == 0)
                {
                    Console.WriteLine("【项目】:{0};\r\n【大分组】:{1};\t【小分组】:{2};\r\n【日志类型】:{3};\r\n【标题】:{4};\r\n【内容】:{5};\r\n【createtime】:{6};\r\n\r\n",
                       a.ProjectName ?? "", a.GroupID, a.InnerGroupID, "普通日志", a.Title, a.Content ?? "", a.CreateTime.ToString(Config.DateTimeFormat));
                }
                if (a.LogType == 1)
                {
                    Console.WriteLine("【项目】:{0};\r\n【大分组】:{1};\t【小分组】:{2};\r\n【日志类型】:{3};\r\n【标题】:{4};\r\n【内容】:{5};\r\n【createtime】:{6};\r\n【耗时】:{7}s\r\n\r\n",
                          a.ProjectName ?? "", a.GroupID, a.InnerGroupID, "耗时日志", a.Title, a.Content ?? "", a.CreateTime.ToString(Config.DateTimeFormat), a.Elapsed);
                }
                if (a.LogType == 2)
                {
                    Console.WriteLine("【项目】:{0};\r\n【大分组】:{1};\t【小分组】:{2};\r\n【日志类型】:{3};\r\n【标题】:{4};\r\n【内容】:{5};\r\n【createtime】:{6};\r\n\r\n",
                          a.ProjectName ?? "", a.GroupID, a.InnerGroupID, "错误日志", a.Title, a.Content ?? "", a.CreateTime.ToString(Config.DateTimeFormat));
                }
            }
        }

    }
}
