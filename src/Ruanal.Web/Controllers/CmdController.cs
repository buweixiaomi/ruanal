using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ruanal.Web.Controllers
{
    public class CmdController : ManageBaseController
    {
        Ruanal.WebDomain.BLL.CmdBll cmdbll = new WebDomain.BLL.CmdBll();
        // GET: TaskRunLog
        public ActionResult Index(int? nodeid, string begintime, string endtime, int? cmdState , int pno = 1)
        {
            ViewBag.begintime = begintime;
            ViewBag.endtime = endtime;
            ViewBag.nodeid = nodeid;
            ViewBag.cmdState = cmdState;
            DateTime? b = RLib.Utils.Converter.TryToDateTime(begintime);
            DateTime? e = RLib.Utils.Converter.TryToDateTime(endtime);
            var model = cmdbll.PageCmd(nodeid, cmdState, b, e, pno);
            return View(model);
        }

        public JsonResult DeleteCmd(int cmdid)
        {
            int r = cmdbll.DeleteCmd(cmdid);
            return Json(new JsonEntity() { code = 1 });
        }

        [HttpPost]
        public JsonResult BuildCmd(string cmdtype, int taskid, int nodeid = 0)
        {
            List<int> nids = new List<int>();
            if (nodeid > 0)
                nids.Add(nodeid);
            bool ok = false;
            string msg = "";
            switch (cmdtype)
            {
                case "starttask":
                    ok = Ruanal.WebDomain.BLL.CmdHelper.Instance.StartTask(taskid, nids);
                    break;
                case "stoptask":
                    ok = Ruanal.WebDomain.BLL.CmdHelper.Instance.StopTask(taskid, nids);
                    break;
                case "uninstalltask":
                    ok = Ruanal.WebDomain.BLL.CmdHelper.Instance.DeleteTask(taskid, nids);
                    break;
                default:
                    throw new Ruanal.WebDomain.MException("未知命令！");
            }
            return Json(new JsonEntity() { code = 1, msg = msg });
        }
    }
}