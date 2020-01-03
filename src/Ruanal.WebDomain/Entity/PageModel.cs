using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.WebDomain
{
    public class PageModel<T>
    {
        public List<T> List { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
    }
}
