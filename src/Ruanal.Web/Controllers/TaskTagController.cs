using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ruanal.Web.Controllers
{
    public class TaskTagController : ManageBaseController
    {
        const int COUNT = 10;
        WebDomain.BLL.TaskTagBll tasktagbll = new WebDomain.BLL.TaskTagBll();
        // GET: Tag
        public ActionResult Index()
        {
            var dic = tasktagbll.GetAll();
            dic = tasktagbll.ProMini(dic, COUNT);
            ViewBag.tags = dic;
            return View();
        }

        [HttpPost]
        public ActionResult SaveAll()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            for (var k = 1; k <= COUNT; k++)
            { 
                var tagv = Request.Form["tag"+k.ToString()];
                tagv = (tagv??"").Trim();
                dic[k] = tagv;
            }
            tasktagbll.SaveAll(dic);
            return RedirectToAction("Index");
        }
    }
}