using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.Common
{
    public class FilterBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SortField { get; set; }
        public string SortBy { get; set; }
        public string FilterText { get; set; }
        public string FilterField { get; set; }
        public string stringsearch { get; set; }
        public int IdObj { get; set; }
    }
}
