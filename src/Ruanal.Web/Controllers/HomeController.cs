using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ruanal.Web.Controllers
{
    public class HomeController : ManageBaseController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            //if (WebDomain.PermissionProvider.ExistWidthCache(WebDomain.SystemPermissionKey.WorkItem_ExecWork))
            //{
            //    WebDomain.BLL.WorkItemBll workitembll = new WebDomain.BLL.WorkItemBll();
            //    ViewBag.waitworks = workitembll.GetPage(Token.Id, null, 2, 1, 5).list;
            //}

            //if (WebDomain.PermissionProvider.ExistWidthCache(WebDomain.SystemPermissionKey.WorkDaily_Add))
            //{
            //    ViewBag.addworkdaily = "";
            //}


            return View();
        }

        //public JsonResult SetNotify()
        //{
        //    Ruanal.Core.Notify.Notifer.Notify(Ruanal.Core.SystemConst.NOTICETOPIC_LIVENEWS_TRYINSERT, "|" + DateTime.Now + "吧吧啊啊");
        //    return JsonE("OK");
        //}

    }
}
