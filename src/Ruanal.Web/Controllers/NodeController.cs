using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ruanal.WebDomain;

namespace Ruanal.Web.Controllers
{
    public class NodeController : ManageBaseController
    {
        Ruanal.WebDomain.BLL.NodeBll nodebll = new WebDomain.BLL.NodeBll();
        // GET: Node
        public ActionResult Index()
        {
            var models = nodebll.GetAllNode();
            return View(models);
        }

        public ActionResult Edit(int nodeId = 0)
        {
            if (nodeId > 0)
            {
                var model = nodebll.GetDetail(nodeId);
                return View(model);
            }
            return View();
        }

        [HttpPost]
        public ActionResult Edit(Ruanal.WebDomain.Model.Node model)
        {
            if (model.NodeId > 0)
            {
                nodebll.UpdateNode(model);
                return RedirectToAction("edit", new { nodeId = model.NodeId });
            }
            else
            {
                model = nodebll.AddNode(model);
                return RedirectToAction("edit", new { nodeId = model.NodeId });
            }
        }

        public JsonResult DeleteNode(int nodeId)
        {
            var v = nodebll.DeleteNode(nodeId);
            if (v)
            {
                return Json(new JsonEntity() { code = 1 });
            }
            else
            {
                return Json(new JsonEntity() { code = -1, msg = "删除失败，请重试！" });
            }
        }

        public JsonResult RestartNode(int nodeId)
        {
            var v = Ruanal.WebDomain.BLL.CmdHelper.Instance.RestartNode(nodeId);
            if (v)
            {
                return Json(new JsonEntity() { code = 1 });
            }
            else
            {
                return Json(new JsonEntity() { code = -1, msg = "重启失败，请重试！" });
            }
        }

        public ActionResult GetNobindNodes()
        {
            return Json(new JsonEntity() { code = 1, data = ClientsCache.GetAll().Select(x => x.Value).ToList() });
        }

        public JsonResult SetDispatchState(int nodeId, int dispatchState)
        {
            bool v = nodebll.SetDispatchState(nodeId, dispatchState);
            if (v)
            {
                return Json(new JsonEntity() { code = 1 });
            }
            else
            {
                return Json(new JsonEntity() { code = -1, msg = "操作失败，请重试！" });
            }
        }

        public JsonResult ViewStatus(int nodeId)
        {
            var vr = nodebll.ViewStatus(nodeId);
            List<object> rdata = new List<object>();
            foreach (var a in vr.OrderBy(x => x.Key))
            {
                rdata.Add(new
                {
                    nodeKey = a.Key,
                    instanceStatus = a.Value
                });
            }
            return Json(new JsonEntity() { code = 1, data = rdata, msg = "OK" });
        }

    }
}