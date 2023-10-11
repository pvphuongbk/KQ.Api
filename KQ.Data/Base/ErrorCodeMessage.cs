namespace KQ.Data.Base
{
	public class ErrorCodeMessage
	{
		#region Return Code (0 - 99): Common error
		public static readonly KeyValuePair<int, string> Success = new KeyValuePair<int, string>(0, "The operation completed successfully.");
		public static readonly KeyValuePair<int, string> Exception = new KeyValuePair<int, string>(1, "An error occurred.");
        public static readonly KeyValuePair<int, string> NotFound = new KeyValuePair<int, string>(2, "Not Found.");
        public static readonly KeyValuePair<int, string> NoPermission = new KeyValuePair<int, string>(3, "No Permission");
		public static readonly KeyValuePair<int, string> UserNotExisting = new KeyValuePair<int, string>(4, "User not existing.");
		public static readonly KeyValuePair<int, string> AlreadyExist = new KeyValuePair<int, string>(5, "Already exist");
		public static readonly KeyValuePair<int, string> InsertFail = new KeyValuePair<int, string>(6, "Insert fail");
		public static readonly KeyValuePair<int, string> WroongMethod = new KeyValuePair<int, string>(7, "Wroong method");
		public static readonly KeyValuePair<int, string> Existing = new KeyValuePair<int, string>(8, "Existing");
        #endregion
    }

}
