using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Commons
{
    public class Pagination<T>
    {
        public int TotalItemsCount { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int TotalPagesCount => (int)Math.Ceiling((double)TotalItemsCount / PageSize);
        public bool Next => PageIndex < TotalPagesCount;
        public bool Previous => PageIndex > 1;
        public ICollection<T> Items { get; set; }

        public Pagination(List<T> items, int count, int pageIndex, int pageSize)
        {
            TotalItemsCount = count;
            PageIndex = pageIndex;
            PageSize = pageSize;
            Items = items;
        }
    }
}
