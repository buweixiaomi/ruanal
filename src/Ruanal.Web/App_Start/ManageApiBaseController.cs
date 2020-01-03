using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace Ruanal.Web
{
    public abstract class ManageApiBaseController : Controller
    {

        protected override void OnAuthentication(AuthenticationContext filterContext)
        {
            base.OnAuthentication(filterContext);
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            var value = filterContext.RequestContext.HttpContext.Request.Headers.Get("Authorization");
            if (string.IsNullOrWhiteSpace(value))
            {
                filterContext.Result = ApiError("请登录！");
                return;
            }
            var kv = value.Split(" ".ToArray(), 2);
            if (kv.Length != 2 || kv[0] != "Basic")
            {
                filterContext.Result = ApiError("请登录！");
                return;
            }
            kv = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(kv[1])).Split(":".ToArray(), 2);
            if (kv.Length != 2)
            {
                filterContext.Result = ApiError("请登录！");
                return;
            }

            Ruanal.WebDomain.BLL.ManagerBll managerbll = new Ruanal.WebDomain.BLL.ManagerBll();
            var model = managerbll.LoginIn(kv[0], kv[1]);
            if (model == null)
            {
                filterContext.Result = ApiError("登录失败！");
                return;
            }
            base.OnAuthorization(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ApiResultSummary.EndRequest(System.Web.HttpContext.Current.Request);
            base.OnActionExecuted(filterContext);
        }
        public JsonResult ApiResult(object obj)
        {
            return JsonE(obj);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            int code = (int)Ruanal.WebDomain.MExceptionCode.ServerError;
            if (filterContext.Exception is Ruanal.WebDomain.MException)
            {
                code = (filterContext.Exception as Ruanal.WebDomain.MException).Code;
            }

            filterContext.HttpContext.Response.StatusCode = 200;
            var vresult = Json(new JsonEntity() { code = code, data = null, msg = filterContext.Exception.Message }, JsonRequestBehavior.AllowGet);
            vresult.ExecuteResult(filterContext.Controller.ControllerContext);
            filterContext.Controller.ControllerContext.HttpContext.Response.End();
        }


        public JsonResult JsonE(object data)
        {
            return Json(new JsonEntity() { code = 1, data = data, msg = "" });
        }

        public JsonResult ApiSuccess(object data)
        {
            return Json(new JsonEntity() { code = 1, data = data, msg = "" });
        }

        public JsonResult ApiError(string msg)
        {
            return Json(new JsonEntity() { code = -1, data = null, msg = msg ?? "" });
        }
    }
}
