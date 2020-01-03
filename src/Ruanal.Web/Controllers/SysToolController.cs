using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ruanal.WebDomain;

namespace Ruanal.Web.Controllers
{
    public class SysToolController : ManageBaseController
    {
        // GET: SysTool
        WebDomain.BLL.SysToolBll systool = new WebDomain.BLL.SysToolBll();
        public ActionResult Index()
        {
            var runconfig = new WebDomain.BLL.CommBll().GetConfig();
            ViewBag.RunConfig = runconfig;
            return View();
        }

        public JsonResult ClearLog(string logtype, string endTime)
        {
            DateTime etime = DateTime.MinValue;
            if (!DateTime.TryParse(endTime, out etime))
            {
                etime = DateTime.Now.AddDays(-7);
            }
            int rcount = 0;
            switch ((logtype ?? "").ToLower())
            {
                case "runlog":
                    rcount = systool.DeleteRunLog(etime);
                    break;
                case "worklog":
                    rcount = systool.DeleteWorkLog(etime);
                    break;
                case "dispatchlog":
                    rcount = systool.DeleteDispatchLog(etime);
                    break;
                default:
                    throw new MException("无该日志类型！");
            }
            return Json(new JsonEntity() { code = 1, msg = "删除" + rcount + "条" });
        }

        public JsonResult DeleteTaskPackage(int keeppackagecount = 10)
        {
            var data = systool.GetTasksTopVersions(keeppackagecount);
            string pathname = Pub.GetConfig("taskDllFile", "~/taskDllFile");
            string path = Server.MapPath(pathname);
            List<string> keepfiles = new List<string>();
            foreach (var a in data)
            {
                foreach (var b in a.Value)
                {
                    var fn = b.FilePath.Split("/\\".ToArray()).Last();
                    keepfiles.Add(fn.ToLower());
                }
            }
            int count = 0;
            if (System.IO.Directory.Exists(path))
            {
                System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(path);
                foreach (var f in directory.GetFiles())
                {
                    if (keepfiles.Contains(f.Name.ToLower()))
                    {
                        continue;
                    }
                    f.Delete();
                    count++;
                }
            }

            return Json(new JsonEntity() { code = 1, msg = "删除" + count + "条" });
        }



        public JsonResult SaveConfig(string config)
        {
            if (string.IsNullOrWhiteSpace(config))
            {
                config = "{}";
            }
            try
            {
                var jobj = RLib.Utils.DataSerialize.BeautyJson(config); 
            }
            catch
            {
                return Json(new JsonEntity() { code = -1, msg = "错误的json" });
            }
            new WebDomain.BLL.CommBll().SaveConfig(config);
            return Json(new JsonEntity() { code = 1, msg = "ok" });
        }
    }
}