using KQ.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.Common.Extention
{
    public static class IEnumerableExtensions
    {
        public static List<T> FilerByField<T>(this List<T> items,string filterField, string filterText)
        {
            var result = items;
            try
            {
                var prop = typeof(T).GetProperty(filterField.FirstCharToUpper());
                if (!string.IsNullOrEmpty(filterText) && prop != null)
                {
                    result = items.Where(x => (prop.GetValue(x, null) != null && !string.IsNullOrEmpty(prop.GetValue(x, null).ToString())
                                           && prop.GetValue(x, null).ToString().ToLower().Contains(filterText.ToLower()))).ToList();
                }
            }
            catch
            {
                // có lỗi thì trả về list gốc
                result = items;
            }

            return result;
        }
        /// <summary>
        /// Sorting với đối tượng là List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">list truyền vào</param>
        /// <param name="sortBy">có 2 chế độ "asc" sorting a->z, từ 1-9: "desc" sorting ngược lại</param>
        /// <param name="sortField">tên của trường cần sort , ví dụ sort theo ten</param>
        /// <returns></returns>
        public static List<T> SortByField<T>(this List<T> items, string sortBy, string sortField)
        {
            // tương tự mình có hàm sort
            var sortList = items;
            try
            {
                // Sort by field
                var prop = typeof(T).GetProperty(sortField.FirstCharToUpper());
                if (prop != null)
                {
                    if (sortBy.ToLower() == "asc")
                    {
                        sortList = items.OrderBy(x => prop.GetValue(x, null)).ToList();
                    }
                    else if (sortBy.ToLower() == "desc")
                    {
                        sortList = items.OrderByDescending(x => prop.GetValue(x, null)).ToList();
                    }
                }
            }
            catch
            {
                sortList = items;
            }

            return sortList;
        }

        /// <summary>
        /// Hàm này để sort ngay khi nó mới được lấy ra từ DB (vẫn ở dạng IQueryable<T> )
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortField"></param>
        /// <returns></returns>
        public static IQueryable<T> SortByField<T>(this IQueryable<T> items, string sortBy, string sortField)
        {
            var sortList = items.ToList();
            try
            {
                // Sort by field
                var prop = typeof(T).GetProperty(sortField.FirstCharToUpper());
                if (prop != null)
                {
                    if (sortBy.ToLower() == "asc")
                    {
                        sortList = sortList.OrderBy(x => prop.GetValue(x, null)).ToList();
                    }
                    else if (sortBy.ToLower() == "desc")
                    {
                        sortList = sortList.OrderByDescending(x => prop.GetValue(x, null)).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                sortList = items.ToList();
            }

            return sortList.AsQueryable();
        }

        public static PagingList<T> ConvertToPaging<T>(this IQueryable<T> items, int pageSize, int pageIndex)
        {
            int count = items.Count();
            PagingList<T> result = new();
            if (items == null || count == 0)
            {
                result.MetaData = new Metadata
                {
                    TotalCount = 0,
                    CurrentPage = pageIndex,
                    TotalPages = 0,
                };
                result.Items = new List<T>();

                return result;
            }

            result.MetaData = new Metadata
            {
                TotalCount = count,
                CurrentPage = pageIndex,
                TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling((double)count / pageSize),
            };

            // Paging
            var indexFrom = (pageIndex - 1) * pageSize;

            if (pageSize == 0 || pageIndex == 0 || indexFrom > count)
                result.MetaData.Count = 0;
            else
            {
                if (count >= indexFrom + pageSize)
                    result.MetaData.Count = pageSize;
                else
                    result.MetaData.Count = count - indexFrom;
            }
            if (result.MetaData.Count > 0)
            {
                var queryResultPage = items.Skip(indexFrom)
                                                .Take(result.MetaData.Count);
                result.Items = queryResultPage.ToList();
            }

            return result;
        }
        /// <summary>
        /// Hàm này để paging
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">tổng số record</param>
        /// <param name="pageSize">số record hiển thị trong 1 trang => cái này web truyền cho mày</param>
        /// <param name="pageIndex">index trang hiện tại => cía này user click vào số nào thì web truyền cho mày</param>
        /// <returns></returns>
        public static PagingList<T> ConvertToPaging<T>(this List<T> items, int pageSize, int pageIndex)
        {
            PagingList<T> result = new();

            if(items == null || items.Count == 0)
            {
                result.MetaData = new Metadata
                {
                    TotalCount = 0,
                    CurrentPage = pageIndex,
                    TotalPages = 0,
                };
                result.Items = new List<T>();

                return result;
            }

            result.MetaData = new Metadata
            {
                TotalCount = items.Count,
                CurrentPage = pageIndex,
                TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling((double)items.Count / pageSize),
            };

            // Paging
            var indexFrom = (pageIndex - 1) * pageSize;

            if (pageSize == 0 || pageIndex == 0 || indexFrom > items.Count)
                result.MetaData.Count = 0;
            else
            {
                if (items.Count >= indexFrom + pageSize)
                    result.MetaData.Count = pageSize;
                else
                    result.MetaData.Count = items.Count - indexFrom;
            }
            if (result.MetaData.Count > 0)
            {
                result.Items = items.GetRange(indexFrom, result.MetaData.Count);
            }

            return result;
        }
        public static List<int> CloneList(this List<int> lst1)
        {
            List<int> lst = new List<int>();
            foreach (var num in lst1)
                lst.Add(num);

            return lst;
        }
        public static List<string> CloneList(this List<string> lst1)
        {
            List<string> lst = new List<string>();
            foreach (var num in lst1)
                lst.Add(num);

            return lst;
        }
        public static List<int> CloneListStrToInt(this List<string> lst1)
        {
            List<int> lst = new List<int>();
            foreach (var num in lst1)
                lst.Add(int.Parse(num));

            return lst;
        }
    }
}
