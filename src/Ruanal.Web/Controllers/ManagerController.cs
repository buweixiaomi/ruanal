using Ruanal.WebDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ruanal.Web.Controllers
{
    [AllowAnonymous]
    public class ManagerController : ManageBaseController
    {
        //
        // GET: /Manager/
        Ruanal.WebDomain.BLL.ManagerBll managerbll = new Ruanal.WebDomain.BLL.ManagerBll();
        public ActionResult Index(int pno = 1, string keywords = "")
        {
            const int pagesize = 20;
            var model = managerbll.GetManagerPage(pno, pagesize, keywords);
            return View(model);
        }


        [HttpGet]
        public ActionResult Edit(int managerid = 0)
        { 
            if (managerid > 0)
            {
                var model = managerbll.GetManagerDetail(managerid);  
                return View(model);
            }
            else
            {
                var model = new Ruanal.WebDomain.Model.Manager();
                ViewBag.branchname = null;
                return View(model);
            }
        }


        [HttpPost]
        public ActionResult Edit(Ruanal.WebDomain.Model.Manager model, string AllowLogin = "")
        {
            try
            { 
                if (model == null)
                {
                    throw new Exception("无效参数！");
                }
                model.AllowLogin = (AllowLogin ?? "").ToLower() == "on" ? 1 : 0;
                if (model.ManagerId > 0)
                {
                    managerbll.UpdateManager(model);
                    
                    ViewBag.msg = "修改成功";
                }
                else
                {
                    model = managerbll.AddManager(model);
                    ViewBag.msg = "新增成功";
                }
                model = managerbll.GetManagerDetail(model.ManagerId);
                  
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return View(model);
            }
        }

        public JsonResult ResetPwd(int managerid)
        {
            if (managerbll.ResetPwd(managerid))
            {
                return Json(new JsonEntity() { code = 1, msg = "重置成功" });
            }
            else
            {
                return Json(new JsonEntity() { code = 1, msg = "重置失败" });
            }
        }
         
          
        [HttpPost]
        public JsonResult DeleteManager(int managerid)
        {
            bool deleteok = managerbll.DeleteManager(managerid);
            if (deleteok)
                return JsonE("OK");
            else
            {
                return Json(new JsonEntity() { code = -1, msg = "删除失败" });
            }
        }
         
    }
}
