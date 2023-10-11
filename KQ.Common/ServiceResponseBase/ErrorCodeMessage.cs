using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.Common
{
    public class ErrorCodeMessage
    {
        // định nghĩa loại lỗi muốn thông báo cho web
        public static readonly KeyValuePair<int, string> Success = new KeyValuePair<int, string>(0, "The operation completed successfully.");
        public static readonly KeyValuePair<int, string> DeleteFail = new KeyValuePair<int, string>(7, "Delete Fail");
        public static readonly KeyValuePair<int, string> AddFail = new KeyValuePair<int, string>(8, "Add Fail");
        public static readonly KeyValuePair<int, string> EditFail = new KeyValuePair<int, string>(9, "Edit Fail");
        public static readonly KeyValuePair<int, string> AddPictureFail = new KeyValuePair<int, string>(10, "Add Picture Fail");

        public static readonly KeyValuePair<int, string> InternalExeption = new KeyValuePair<int, string>(1, "Internal exeption");
        public static readonly KeyValuePair<int, string> ListNull = new KeyValuePair<int, string>(2, "List Null");
        public static readonly KeyValuePair<int, string> ObjectNull = new KeyValuePair<int, string>(3, "Object Null");
        public static readonly KeyValuePair<int, string> TokenNull = new KeyValuePair<int, string>(4, "Token Null");
        public static readonly KeyValuePair<int, string> OperationFail = new KeyValuePair<int, string>(5, "Operation Fail");
        public static readonly KeyValuePair<int, string>  NoObject = new KeyValuePair<int, string>(6, " No Object");

    }
}
