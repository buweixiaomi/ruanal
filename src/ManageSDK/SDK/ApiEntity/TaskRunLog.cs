using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.ManageApi.ApiEntity
{
    public class TaskRunLog
    {

        public int LogId { get; set; }
        public string RunGuid { get; set; }
        public int TaskId { get; set; }
        public int NodeId { get; set; }
        /// <summary>
        /// 0:计划 1:调度 2:分配
        /// </summary>
        public int RunType { get; set; }
        public DateTime RunServerTime { get; set; }
        public DateTime RunDbTime { get; set; }
        public DateTime? EndServerTime { get; set; }
        public DateTime? EndDbTime { get; set; }
        /// <summary>
        /// 0待完成 1:执行成功 2:执行失败
        /// </summary>
        public int ResultType { get; set; }
        public string LogText { get; set; }
    }
}
