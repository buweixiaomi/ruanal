using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ruanal.Web.Controllers
{
    public class DispatchController : ManageBaseController
    {
        Ruanal.WebDomain.BLL.TaskBll taskbll = new WebDomain.BLL.TaskBll();
        // GET: TaskRunLog
        public ActionResult Index(string keywords, int? taskid, int? nodeid, int? dispatchState, string begintime, string endtime, int pno = 1)
        {
            ViewBag.begintime = begintime;
            ViewBag.endtime = endtime;
            ViewBag.taskid = taskid;
            ViewBag.keywords = keywords;
            ViewBag.nodeid = nodeid;
            ViewBag.dispatchState = dispatchState;

            DateTime? b = RLib.Utils.Converter.TryToDateTime(begintime);
            DateTime? e = RLib.Utils.Converter.TryToDateTime(endtime);
            var model = taskbll.GetDispatchPage(pno, keywords, b, e, taskid, nodeid, dispatchState);
            return View(model);
        }

        public JsonResult DeleteDispatch(int dispatchid)
        {
            int r = taskbll.DeleteDispath(dispatchid);
            return Json(new JsonEntity() { code = 1 });
        }

        public JsonResult StopDispatch(int dispatchid)
        {
            Ruanal.WebDomain.BLL.CmdHelper.Instance.StopDispatch(dispatchid);
            return Json(new JsonEntity() { code = 1, msg = "命令发布成功，请关注命令结果！" });
        }

        public JsonResult ShowRuningDispatch(int dispatchid)
        {
            string r = taskbll.ShowRuningDispatch(dispatchid);
            return Json(new JsonEntity() { code = 1, data = r, msg = "OK" });
        }

        public JsonResult AutoEndEnd()
        {
            var r = taskbll.AutoEndEnd();
            return Json(new JsonEntity() { code = 1, data = r, msg = "完成，结束数量："+r });
        }
    }
}