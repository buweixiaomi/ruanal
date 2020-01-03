using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.ManageApi.ApiEntity
{
    public class Task
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        /// <summary>0:普通任务    1:调度任务</summary>
        public int TaskType { get; set; }
        public string RunCron { get; set; }
        public string TaskConfig { get; set; }
        public int State { get; set; }
        public string EnterDll { get; set; }
        public string EnterClass { get; set; }
        public string DispatchClass { get; set; }
        public string Remark { get; set; }

        /// <summary>
        /// 添加任务时不填，会默认绑定节点
        /// </summary>
        public List<TaskBinding> TaskBindings { get; set; }
    }
}
