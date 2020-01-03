using Ruanal.WebDomain.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ruanal.WebDomain.MApiEntity;

namespace Ruanal.Web.Areas.Api.Controllers
{
    public class ManageApiController : ManageApiBaseController
    {
        ManageApiBll manageApiBll = new ManageApiBll();


        //重启节点
        public ActionResult NodeRestart(int type = 0,int nodeid=0)
        {
            var count = manageApiBll.NodeRestart(type, nodeid);
            return ApiSuccess(count);
        }

        //节点列表
        public ActionResult NodeList()
        {
            var nodes = manageApiBll.NodeList();
            return ApiSuccess(nodes);
        }

        //任务列表
        public ActionResult TaskList()
        {
            var tasklist = manageApiBll.TaskList();
            return ApiSuccess(tasklist);
        }

        //添加任务
        public ActionResult TaskAdd(Task model)
        {
            var model2 = manageApiBll.TaskAdd(model);
            return ApiSuccess(model2);
        }

        //修改任务
        public ActionResult TaskEdit(Task model)
        {
            var model2 = manageApiBll.TaskEdit(model);
            return ApiSuccess(model2);
        }

        //删除任务
        public ActionResult TaskDelete(int taskid)
        {
            var model2 = manageApiBll.TaskDelete(taskid);
            return ApiSuccess(model2);
        }

        //开始任务
        public ActionResult TaskStart(int taskid)
        {
            var model2 = manageApiBll.TaskStart(taskid);
            return ApiSuccess(model2);
        }

        //停止任务
        public ActionResult TaskStop(int taskid)
        {
            var model2 = manageApiBll.TaskStop(taskid);
            return ApiSuccess(model2);
        }


        //卸载任务
        public ActionResult TaskUnInstall(int taskid)
        {
            var model2 = manageApiBll.TaskUnInstall(taskid);
            return ApiSuccess(model2);
        }


        //设置任务版本
        public ActionResult TaskSetVersion(TaskVersion model)
        {
            var model2 = manageApiBll.TaskSetVersion(model);
            return ApiSuccess(model2);
        }

        //任务运行日志
        public ActionResult TaskRunLog(int taskid = 0)
        {
            var model2 = manageApiBll.TaskRunLog(taskid);
            return ApiSuccess(model2);
        }
    }
}