using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ruanal.Web.Areas.Api.Controllers
{
    public class ApiTaskController : ApiBaseController
    {

        public ActionResult BuildDispatchs(int taskId, string groupId, string runconfigs)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, true);
            List<Ruanal.Job.DispatcherItem> configs = RLib.Utils.DataSerialize.DeserializeObject<List<Ruanal.Job.DispatcherItem>>(runconfigs);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.BuildDispatchs(ClientId,taskId, groupId, configs);
            return Json(r);
        }

        public ActionResult CheckDispatchGroup(string groupId)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, true);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.CheckDispatchGroup(groupId);
            return Json(r);
        }

        public ActionResult TaskDetail(int taskId)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.TaskDetail(taskId);
            return Json(r);
        }

        public ActionResult NodeTasks()
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.NodeTasksJustRunning(ClientId);
            return Json(r);
        }
    }
}