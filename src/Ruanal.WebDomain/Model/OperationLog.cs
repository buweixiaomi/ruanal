using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.Model
{
    public class OperationLog
    {
        public int Id { get; set; }
        public string OperationContent { get; set; }
        public string OperationName { get;set; }
        public DateTime Createtime { get; set; }
        public string OperationTitle { get; set; }
        public string Module { get; set; }
    }
}
