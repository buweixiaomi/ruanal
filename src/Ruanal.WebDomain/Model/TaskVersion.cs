using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Model
{
    public class TaskVersion
    {
        public int VersionId { get; set; }
        public int TaskId { get; set; }
        public string VersionNO { get; set; }
        public string FilePath { get; set; }
        public decimal FileSize { get; set; }
        public int Vstate { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
