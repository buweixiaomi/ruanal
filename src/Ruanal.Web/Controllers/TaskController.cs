using Ruanal.WebDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ruanal.Web.Controllers
{
    public class TaskController : ManageBaseController
    {
        Ruanal.WebDomain.BLL.TaskBll taskbll = new WebDomain.BLL.TaskBll();
        Ruanal.WebDomain.BLL.TaskTagBll tasktagbll = new WebDomain.BLL.TaskTagBll();
        // GET: task
        public ActionResult Index(int tag = -1)
        {
            var models = taskbll.GetAllTaskOfManage(tag);
            var tags = tasktagbll.GetAll();
            ViewBag.tags = tags;
            ViewBag.currtag = tag;
            return View(models);
        }

        #region 任务基本信息
        public ActionResult Edit(int taskId = 0)
        {
            var tags = tasktagbll.GetAll();
            ViewBag.tags = tags;
            if (taskId > 0)
            {
                var model = taskbll.GetDetail(taskId);
                ViewBag.taskId = taskId;
                ViewBag.currTab = "edit";
                return View(model);
            }
            ViewBag.taskId = taskId;
            ViewBag.currTab = "edit";
            return View();
        }

        [HttpPost]
        public ActionResult Edit(Ruanal.WebDomain.Model.Task model, List<int> tasktag)
        {
            if (tasktag == null)
                tasktag = new List<int>();
            tasktag = tasktag.Distinct().ToList();
            var tags = tasktagbll.GetAll();
            model.TaskTags = 0;
            foreach (var a in tasktag)
            {
                model.TaskTags = model.TaskTags | (1 << (a - 1));
            }

            ViewBag.tags = tags;
            var taskconfig = (model.TaskConfig ?? "").Trim();
            Dictionary<string, string> jobj = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(taskconfig))
            {
                try
                {
                    jobj = RLib.Utils.DataSerialize.DeserializeObject<Dictionary<string, string>>(taskconfig);
                }
                catch (Exception ex)
                {
                    throw new MException("任务配置json有错误！");
                }
            }

            //jobj[Ruanal.Job.ConstKey.Dis_SpecifyEntsKey] = model.TaskConfig_SpecifyEnts ?? "";
            //jobj[Ruanal.Job.ConstKey.Dis_ExceptEntsKey] = model.TaskConfig_ExceptEnts ?? "";
            //jobj[Ruanal.Job.ConstKey.Dis_ContainShopInfoKey] = model.TaskConfig_ExceptEnts ?? "";

            //jobj[Ruanal.Job.ConstKey.Dis_SpecifyEntShopsKey] = model.TaskConfig_SpecifyEntShops ?? "";
            //jobj[Ruanal.Job.ConstKey.Dis_ExceptEntShopsKey] = model.TaskConfig_ExceptEntShops ?? "";
            taskconfig = RLib.Utils.DataSerialize.SerializeJsonBeauty(jobj);
            model.TaskConfig = taskconfig;
            if (model.TaskId > 0)
            {
                taskbll.UpdateTask(model);
                return RedirectToAction("edit", new { taskId = model.TaskId });
            }
            else
            {
                model = taskbll.AddTask(model);
                return RedirectToAction("edit", new { taskId = model.TaskId });
            }
        }

        public JsonResult DeleteTask(int taskId)
        {
            var v = taskbll.DeleteTask(taskId);
            if (v)
            {
                return Json(new JsonEntity() { code = 1 });
            }
            else
            {
                return Json(new JsonEntity() { code = -1, msg = "删除失败，请重试！" });
            }
        }


        public ActionResult GetTaskConfigSet(int taskId)
        {
            //string pathname = Pub.GetConfig("taskDllFile", "~/taskDllFile");
            try
            {
                string path = Server.MapPath("~/");
                var v = taskbll.GetTaskConfigSet(taskId, path);
                if (v == null)
                    return Json(new JsonEntity() { code = 1, msg = "取得失败", data = new List<object>() });
                Dictionary<string, object> rda = new Dictionary<string, object>();
                rda["code"] = 1;
                rda["data"] = RLib.Utils.DataSerialize.ToJToken(v);
                return Content(RLib.Utils.DataSerialize.SerializeJsonBeauty(rda), "application/json");
            }
            catch (Exception ex)
            {
                return Json(new JsonEntity() { code = 1, msg = "取得失败", data = new List<object>() });
            }
        }
        #endregion

        #region 任务版本
        public ActionResult TaskVersion(int taskId, int top = 10)
        {
            var model = taskbll.GetDetailWidthVersion(taskId, top);
            ViewBag.taskId = taskId;
            ViewBag.currTab = "version";
            return View(model);
        }
        [HttpPost]
        public ActionResult TaskVersion(Ruanal.WebDomain.Model.TaskVersion model)
        {
            ProcessTaskVersion(Request, model);
            model = taskbll.AddTaskVersion(model);
            return RedirectToAction("taskversion", new { taskid = model.TaskId });

        }

        private void ProcessTaskVersion(HttpRequestBase req, Ruanal.WebDomain.Model.TaskVersion model)
        {
            if (string.IsNullOrWhiteSpace(model.VersionNO))
            {
                model.VersionNO = DateTime.Now.ToString("vyyyy.MM.dd_HH.mm");
            }
            HttpPostedFileBase downloadfile = null;
            if (req.Files.Count > 0)
                downloadfile = req.Files[0];
            if (downloadfile == null || downloadfile.ContentLength == 0)
            {
                throw new Ruanal.WebDomain.MException("请上传文件");
            }

            string filename = model.TaskId + "_" + DateTime.Now.ToString("yyMMddHHmmss") + ".zip";
            string pathname = Pub.GetConfig("taskDllFile", "~/taskDllFile");
            string path = Server.MapPath(pathname);
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            string filefullname = System.IO.Path.Combine(path, filename);
            downloadfile.SaveAs(filefullname);
            model.FilePath = System.IO.Path.Combine(pathname.TrimStart('~'), filename).Replace("\\", "/");
            model.FileSize = downloadfile.ContentLength / 1024m;
        }

        [HttpPost]
        public JsonResult DeleteTaskVersion(int versionid)
        {
            int r = taskbll.DeleteTaskVersion(versionid);
            if (r > 0)
            {
                return Json(new { code = 1 });
            }
            else
            {
                return Json(new { code = -1, msg = "删除版本号失败" });
            }
        }

        public JsonResult SetTaskVersionid(int taskId, int versionId)
        {
            int r = taskbll.SetTaskVersion(taskId, versionId); if (r > 0)
            {
                return Json(new { code = 1 });
            }
            else
            {
                return Json(new { code = -1, msg = "设置当前版本号失败" });
            }
        }
        #endregion

        #region 任务节点绑定

        public ActionResult TaskBinding(int taskId)
        {
            var model = taskbll.GetDetailWidthBinding(taskId);
            ViewBag.taskId = taskId;
            ViewBag.currTab = "binding";
            return View(model);
        }

        public JsonResult DeleteTaskBinding(int bindingId)
        {
            bool v = taskbll.DeleteTaskBinding(bindingId);
            if (v)
            {
                return Json(new JsonEntity() { code = 1 });
            }
            else
            {
                return Json(new JsonEntity() { code = -1, msg = "删除失败，请重试！" });
            }
        }

        public JsonResult AddTaskBinding(int taskId, string nodeIds)
        {
            bool v = taskbll.AddTaskBinding(taskId, nodeIds);
            if (v)
            {
                return Json(new JsonEntity() { code = 1 });
            }
            else
            {
                return Json(new JsonEntity() { code = -1, msg = "绑定失败，请重试！" });
            }
        }

        public JsonResult GetValNodes(int taskId)
        {
            var v = taskbll.GetCanBindingNodes(taskId);
            return Json(new JsonEntity() { code = 1, data = v });
        }


        public JsonResult SetDispatchState(int taskId, int dispatchState, int nodeId = 0)
        {
            bool v = taskbll.SetDispatchState(taskId, nodeId, dispatchState);
            if (v)
            {
                return Json(new JsonEntity() { code = 1 });
            }
            else
            {
                return Json(new JsonEntity() { code = -1, msg = "操作失败，请重试！" });
            }
        }


        public JsonResult ViewStatus(int taskId)
        {
            var vr = taskbll.ViewStatus(taskId);
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


        #endregion


        //批量版本
        #region
        [HttpGet]
        public ActionResult BatchVersion()
        {
            var tags = tasktagbll.GetAll();
            ViewBag.tags = tags;
            return View();
        }


        [HttpPost]
        public ActionResult BatchVersion(Ruanal.WebDomain.Model.TaskVersion model, int tag)
        {
            var tags = tasktagbll.GetAll();
            ViewBag.tags = tags;
            if (tag <= 0)
                throw new Ruanal.WebDomain.MException("请选择标签！");
            ProcessTaskVersion(Request, model);
            taskbll.BatchTaskVersion(model, tag);
            ViewBag.msg = "上传并设置为当前版本！";
            return View();
        }



        #endregion

    }
}