using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.ManageApi.ApiEntity
{
    public class TaskVersion
    {
        public int TaskId { get; set; }
        public string FilePath { get; set; }

        /// <summary>
        /// 单位 kb
        /// </summary>
        public decimal FileSize { get; set; }
        public string Remark { get; set; }
    }
}
