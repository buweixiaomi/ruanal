using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.MApiEntity
{
    public class TaskVersion
    {
        public int TaskId { get; set; }
        public string FilePath { get; set; }
        public decimal FileSize { get; set; }
        public string Remark { get; set; }
    }
}
