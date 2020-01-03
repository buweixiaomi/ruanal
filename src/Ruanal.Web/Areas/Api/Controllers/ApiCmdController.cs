using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ruanal.Web.Areas.Api.Controllers
{
    public class ApiCmdController : ApiBaseController
    {
        // GET: Api/ApiCmd
        public JsonResult GetNews()
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.GetNewCmds(ClientId);
            return Json(r);
        }

        public JsonResult BeginExecute(int cmdid)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.CmdBeginExecute(ClientId, cmdid);
            return Json(r);
        }
        public JsonResult EndExecute(int cmdid, int success, string msg)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.CmdEndExecute(ClientId, cmdid, success > 0 ? true : false, msg);
            return Json(r);
        }
    }
}