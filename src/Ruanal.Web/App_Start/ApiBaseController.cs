using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace Ruanal.Web
{
    public class ApiBaseController : Controller
    {
        Ruanal.WebDomain.BLL.NodeBll nodebll = new WebDomain.BLL.NodeBll();
        public string IPS { get; private set; }
        public string MACS { get; private set; }
        public string ClientId { get; private set; }

        private static List<Ruanal.WebDomain.Model.Node> _allNodes = new List<WebDomain.Model.Node>();
        public static DateTime? _lastGetNodeTime;
        public static int _cacheTimeOutSecs = 30;
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string macs = filterContext.RequestContext.HttpContext.Request.Headers["Client_Macs"] ?? "";
            string ips = filterContext.RequestContext.HttpContext.Request.Headers["Client_IPS"] ?? "";
            string clientid = filterContext.RequestContext.HttpContext.Request.Headers["Client_ID"] ?? "";
            string nodeType = filterContext.RequestContext.HttpContext.Request.Headers["Node_Type"] ?? "";
            IPS = ips;
            MACS = macs;
            ClientId = clientid;
            ApiResultSummary.StartRequest(System.Web.HttpContext.Current.Request, clientid);
            if (_lastGetNodeTime == null || (DateTime.Now - _lastGetNodeTime.Value).TotalSeconds > _cacheTimeOutSecs)
            {
                lock (_allNodes)
                {
                    if (_lastGetNodeTime == null || (DateTime.Now - _lastGetNodeTime.Value).TotalSeconds > _cacheTimeOutSecs)
                    {
                        _allNodes = nodebll.GetAllNode();
                        _lastGetNodeTime = DateTime.Now;
                    }
                }
            }
            Ruanal.WebDomain.Model.Node model = _allNodes.FirstOrDefault(x => (x.ClientId ?? "").Trim() == clientid.Trim());
            //  Ruanal.WebDomain.Model.Node model = nodebll.GetServerByClientId(clientid);// serverbll.GetUnionServer(arrmac, arrip, clientid);
            if (model == null)
            {
                ClientsCache.AddClientInfo(clientid, string.Format("【ClientId】{2} 【{3}】【MAC】{0} 【IP】{1}", macs, ips, clientid, nodeType));
            }
            else
            {
                ClientsCache.Remove(clientid);
            }
            base.OnActionExecuting(filterContext);
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
    }
}
