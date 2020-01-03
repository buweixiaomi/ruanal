using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc; 

namespace Ruanal.Web.Controllers
{
    public class OperationLogController : ManageBaseController
    {
        //
        // GET: /OperationLog/
        WebDomain.BLL.OperationLogBll logbll = new WebDomain.BLL.OperationLogBll();
        public ActionResult Index(int pno = 1, string BeginTime = "", string EndTime = "", string keywords = "")
        {
            ViewBag.keywords = keywords;
            ViewBag.BeginTime = BeginTime;
            ViewBag.EndTime = EndTime;
            const int pagesize = 20;
            var model = logbll.GetLogPage(pno, pagesize, keywords, BeginTime, EndTime);
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id = 0)
        { 
            if (id > 0)
            {
                var model = logbll.GetLogDetail(id);
                return View(model);
            }
            else
            {
                return View();
            }
        }

    }
}
