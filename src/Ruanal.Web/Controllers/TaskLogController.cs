using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ruanal.Web.Controllers
{
    public class TaskLogController : ManageBaseController
    {
        Ruanal.WebDomain.BLL.TaskLogBll tasklogbll = new WebDomain.BLL.TaskLogBll();
        // GET: TaskRunLog
        public ActionResult RunLog(int? taskid, int? nodeid, int? resultType, string begintime, string endtime, string runguid = "", string keywords = "", int pno = 1)
        {
            ViewBag.taskid = taskid;
            ViewBag.nodeid = nodeid;
            ViewBag.keywords = keywords;
            ViewBag.begintime = begintime;
            ViewBag.endtime = endtime;
            ViewBag.resultType = resultType;
            ViewBag.runguid = runguid;


            DateTime? b = RLib.Utils.Converter.TryToDateTime(begintime);
            DateTime? e = RLib.Utils.Converter.TryToDateTime(endtime);

            var model = tasklogbll.PageRunLog(taskid, nodeid, resultType, runguid, keywords, b, e, pno);
            return View(model);
        }

        public ActionResult WorkLog(int? taskid, int? nodeid, int? logtype, string date, string begintime, string endtime, string runguid = "", string keywords = "", int pno = 1)
        {

            ViewBag.taskid = taskid;
            ViewBag.nodeid = nodeid;
            ViewBag.keywords = keywords;
            ViewBag.begintime = begintime;
            ViewBag.endtime = endtime;
            ViewBag.logtype = logtype;
            ViewBag.runguid = runguid;
            DateTime tbdate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(date))
            {
                if (!DateTime.TryParse(date, out tbdate))
                    tbdate = DateTime.Now;
            }
            ViewBag.date = date = tbdate.ToString("yyyy-MM-dd");
            if (!string.IsNullOrWhiteSpace(begintime))
            {
                begintime = date + " " + begintime;
            }
            if (!string.IsNullOrWhiteSpace(endtime))
            {
                endtime = date + " " + endtime;
            }

            DateTime? b = RLib.Utils.Converter.TryToDateTime(begintime);
            DateTime? e = RLib.Utils.Converter.TryToDateTime(endtime);

            var model = tasklogbll.PageWorkLog(tbdate, taskid, nodeid, logtype, runguid, keywords, b, e, pno);
            return View(model);
        }

        public ActionResult WorkLogErrorMode(string date, string[] nottext,
            string begintime, string endtime, int pno = 1)
        {
            if (nottext == null)
                nottext = new string[0];
            if (nottext != null && nottext.Length == 1 && nottext[0].IndexOf(',') > 0)
            {
                nottext = nottext[0].Split(',').ToArray();
            }
            ViewBag.IsErrorMode = "true";
            ViewBag.begintime = begintime;
            ViewBag.endtime = endtime;
            ViewBag.nottext = nottext;

            DateTime tbdate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(date))
            {
                if (!DateTime.TryParse(date, out tbdate))
                    tbdate = DateTime.Now;
            }
            ViewBag.date = date = tbdate.ToString("yyyy-MM-dd");
            if (!string.IsNullOrWhiteSpace(begintime))
            {
                begintime = date + " " + begintime;
            }
            if (!string.IsNullOrWhiteSpace(endtime))
            {
                endtime = date + " " + endtime;
            }

            DateTime? b = RLib.Utils.Converter.TryToDateTime(begintime);
            DateTime? e = RLib.Utils.Converter.TryToDateTime(endtime);
            List<string> notContainText = nottext == null ? null : nottext.ToList();

            var model = tasklogbll.PageWorkLogErrorMode(tbdate, notContainText, b, e, pno);
            return View("WorkLog", model);
        }
    }
}