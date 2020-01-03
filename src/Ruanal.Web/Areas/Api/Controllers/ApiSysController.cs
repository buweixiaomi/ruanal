using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ruanal.Web.Areas.Api.Controllers
{
    public class ApiSysController : ApiBaseController
    {
        public ActionResult PingWorkNode(string taskState)
        {
            List<int[]> _tasks = new List<int[]>();
            try { _tasks = RLib.Utils.DataSerialize.DeserializeObject<List<int[]>>(taskState); }
            catch (Exception ex) { }
            if (_tasks == null) _tasks = new List<int[]>();
            var r = Ruanal.WebDomain.BLL.ApiBll.Instance.PingWorkNode(ClientId, _tasks);
            return Json(r);
        }

        public ActionResult PingMasterNode(string taskState)
        {
            List<int[]> _tasks = new List<int[]>();
            try { _tasks = RLib.Utils.DataSerialize.DeserializeObject<List<int[]>>(taskState); }
            catch (Exception ex) { }

            if (_tasks == null) _tasks = new List<int[]>();
            var r = Ruanal.WebDomain.BLL.ApiBll.Instance.PingMasterNode(ClientId, _tasks);
            return Json(r);
        }

        public ActionResult GetDispatchWork(string freeState)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            List<int[]> _tasksFree = new List<int[]>();
            _tasksFree = RLib.Utils.DataSerialize.DeserializeObject<List<int[]>>(freeState);
            var r = Ruanal.WebDomain.BLL.ApiBll.Instance.GetDispatchWork(ClientId, _tasksFree);
            return Json(r);
        }
        public JsonResult BeginDispatchExecute(int dispatchid)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.BeginDispatchExecute(ClientId, dispatchid);
            return Json(r);
        }
        public JsonResult EndDispatchExecute(int dispatchid, int success, string msg)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.EndDispatchExecute(ClientId, dispatchid, success > 0 ? true : false, msg);
            return Json(r);
        }
        public JsonResult SkipDispatchExecute(int dispatchid)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.SkipDispatchExecute(ClientId, dispatchid);
            return Json(r);
        }

        public JsonResult AutoEndDispatchExecute(int dispatchId)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.AutoEndDispatchExecute(ClientId, dispatchId);
            return Json(r);
        }

        public ActionResult GetNodeConfig()
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.NodeConfig(ClientId);
            return Json(r);
        }

        public ActionResult TaskBeginRunLog(int taskId, string runGuid, int runType, DateTime time)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.TaskBeginLog(
                ClientId, taskId, runGuid, runType, time);
            return Json(r);
        }

        public ActionResult TaskEndRunLog(int taskId, string runGuid, int resultType, string logText, DateTime time)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.TaskEndLog(
                ClientId, taskId, runGuid, resultType, logText, time);
            return Json(r);
        }

        public ActionResult AddWorkLog(int taskId, string dispatchId, int logType, string logText, DateTime createTime)
        {
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.AddWorkLog(
                ClientId, taskId, dispatchId, logType, logText, createTime);
            return Json(r);
        }

        public ActionResult AddWorkLogBatch(string logJson)
        {
            int nodeId = 0;
            Ruanal.WebDomain.BLL.ApiBll.Instance.CheckAuth(ClientId, false, out nodeId);
            List<Ruanal.Core.ApiSdk.WorkLogEntity> logs =
                RLib.Utils.DataSerialize.DeserializeObject<List<Ruanal.Core.ApiSdk.WorkLogEntity>>(logJson);
            Ruanal.WebDomain.JsonEntity r = Ruanal.WebDomain.BLL.ApiBll.Instance.AddWorkLogs(
                nodeId, logs);
            return Json(r);
        }
    }
}