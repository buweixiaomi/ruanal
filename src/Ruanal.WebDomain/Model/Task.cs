using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Model
{
    public class Task
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// 0:普通任务    1:调度任务
        /// </summary>
        public int TaskType { get; set; }
        public string RunCron { get; set; }
        public string TaskConfig { get; set; }
        public int State { get; set; }
        public int TaskTags { get; set; }
        public int CurrVersionId { get; set; }
        public string EnterDll { get; set; }
        public string EnterClass { get; set; }
        public string DispatchClass { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public decimal ExpireMins { get; set; }
        public string Remark { get; set; }
        //extend
        //public string TaskConfig_SpecifyEnts { get; set; }
        //public string TaskConfig_ExceptEnts { get; set; }


        //public string TaskConfig_SpecifyEntShops { get; set; }
        //public string TaskConfig_ExceptEntShops { get; set; }

        public List<TaskBinding> TaskBindings { get; set; }

    }
}
