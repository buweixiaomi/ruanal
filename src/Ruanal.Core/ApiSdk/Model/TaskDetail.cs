using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.Core.ApiSdk
{
    public class TaskDetail
    {
        public int TaskId { get; set; }

        public string Title { get; set; }

        public int TaskType { get; set; }
        public string RunCron { get; set; }

        public string TaskConfig { get; set; }
        public int CurrVersionId { get; set; }
        public string EnterDll { get; set; }

        public string EnterClass { get; set; }
        public string DispatchClass { get; set; }
        public string FileUrl { get; set; }
        public int NodeCount { get; set; }


    }
}
