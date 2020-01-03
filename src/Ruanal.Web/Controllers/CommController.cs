using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ruanal.WebDomain;

namespace Ruanal.Web.Controllers
{
    public class CommController : ManageBaseController
    {
        //
        // GET: /Comm/

        public ActionResult Select(string type)
        {
            string keywords = Request.Params["keywords"] ?? "";
            ViewBag.type = (type ?? "").Trim().ToLower();
            object model = null;
            switch (type)
            {
                case "manager":
                    model = new Ruanal.WebDomain.BLL.ManagerBll().GetManagerTop(1000);
                    break;
                case "node":
                    model = new Ruanal.WebDomain.BLL.NodeBll().GetAllNode();
                    break;
                case "task":
                    model = new Ruanal.WebDomain.BLL.TaskBll().GetAllTask();
                    break;
                case "enterprise":
                    model = new Ruanal.WebDomain.BLL.BusinessBll().GetEnterprise(100, keywords);
                    break;
            }
            ViewBag.model = model;
            return View(model);
        }

        public JsonResult CalcRunTime(string CronExpression)
        {
            Quartz.CronExpression e = new Quartz.CronExpression(CronExpression);

            List<string> results = new List<string>();
            DateTimeOffset dto = DateTimeOffset.Now;
            DateTimeOffset? lastfire = dto;
            for (int i = 0; i < 5 && lastfire != null; i++)
            {
                lastfire = e.GetNextValidTimeAfter(lastfire.Value);
                if (lastfire != null)
                {
                    results.Add(lastfire.Value.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss"));
                }
            }
            results = results.Take(5).ToList();
            while (results.Count < 5)
            {
                results.Add("");
            }
            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}
