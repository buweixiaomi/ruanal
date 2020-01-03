using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ruanal.WebDomain;

namespace Ruanal.Web
{
    public class MenuBar
    {
        public static List<WebDomain.MenuGroup> Menu = new List<WebDomain.MenuGroup>();
        static MenuBar()
        {
            Menu.Add(
                new WebDomain.MenuGroup()
                {
                    Title = "任务管理",
                    Items = new List<WebDomain.MenuItem>(){
                          new WebDomain.MenuItem(){ Text = "节点管理",  PermissionKey = "",Url = "/node/index" , UrlKey = "node.index"},
                          new WebDomain.MenuItem(){ Text = "任务列表",  PermissionKey = "",Url = "/task/index" , UrlKey = "task.index"},
                          new WebDomain.MenuItem(){ Text = "任务分配/调度",  PermissionKey = "",Url = "/dispatch/index" , UrlKey = "dispatch.index"},
                          //new WebDomain.MenuItem(){ Text = "任务运行日志",  PermissionKey = "",Url = "/tasklog/runlog" , UrlKey = "tasklog.runlog"},
                          //new WebDomain.MenuItem(){ Text = "任务业务日志",  PermissionKey = "",Url = "/tasklog/worklog" , UrlKey = "tasklog.worklog"},
                    }
                }
            );
            Menu.Add(
              new WebDomain.MenuGroup()
              {
                  Title = "日志",
                  Items = new List<WebDomain.MenuItem>(){
                          new WebDomain.MenuItem(){ Text = "运行日志",  PermissionKey = "",Url = "/tasklog/runlog" , UrlKey = "tasklog.runlog"},
                          new WebDomain.MenuItem(){ Text = "业务日志",  PermissionKey = "",Url = "/tasklog/worklog" , UrlKey = "tasklog.worklog"},
                    }
              }
          );
            Menu.Add(
                new WebDomain.MenuGroup()
                {
                    Title = "系统管理",
                    Items = new List<WebDomain.MenuItem>(){
                       new WebDomain.MenuItem(){ Text = "命令列表",  PermissionKey =  "",Url = "/cmd/index" , UrlKey = "cmd.index"},
                          new WebDomain.MenuItem(){ Text = "员工列表",  PermissionKey = "",Url = "/manager/index" , UrlKey = "manager.index"},
                          new WebDomain.MenuItem(){ Text = "任务标签",  PermissionKey = "",Url = "/tasktag/index" , UrlKey = "tasktag.index"},
                          new WebDomain.MenuItem(){ Text = "系统工具",  PermissionKey = "",Url = "/systool/index" , UrlKey = "systool.index"},
                      // new WebDomain.MenuItem(){ Text = "操作日志",  PermissionKey =  "",Url = "/operationlog/index" , UrlKey = "operationlog.index"},
                    }
                }
            );
        }

        public static HtmlString GetUserMenu(string urlkey, string cssClass = "list-group")
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int id = Pub.GetManagerId();
            var rdata = GetUserMenu(id, urlkey, cssClass);
            sw.Stop();
            System.Diagnostics.Trace.WriteLine("用时:" + sw.Elapsed.TotalMilliseconds + "ms;");
            return rdata;
        }

        public static HtmlString GetUserMenu(int userid, string urlkey, string cssClass)
        {
            List<WebDomain.MenuGroup> currmenu = new List<MenuGroup>();
            #region
            foreach (var a in Menu)
            {
                var currgroup = new MenuGroup() { Title = a.Title, Items = new List<MenuItem>() };
                foreach (var b in a.Items)
                {
                    currgroup.Items.Add(b);
                }
                if (currgroup.Items.Count > 0)
                    currmenu.Add(currgroup);
            }
            #endregion
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var a in currmenu)
            {
                sb.AppendLine(string.Format("<div class=\"{0}\">", cssClass));
                sb.AppendLine(string.Format("\t<p class=\"{0}-header\" href=\"javascript:void(0)\">" + a.Title + "</p>", cssClass));
                foreach (var b in a.Items)
                {
                    sb.AppendFormat("\t\t<a href=\"{0}\" class=\"{3}-item{1} navitem\">{2}</a>\r\n",
                        b.Url,
                        b.UrlKey.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower()).Contains(urlkey.ToLower()) ? " active" : "",
                        b.Text,
                        cssClass
                        );
                }
                sb.AppendLine("</div>");
            }
            return new HtmlString(sb.ToString());
        }

    }
}