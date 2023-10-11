namespace KQ.Data.Base
{
	public class ResponseBase
	{
		public ResponseBase()
		{
			Message = ErrorCodeMessage.Success.Value;
			Code = ErrorCodeMessage.Success.Key;
		}

		public int Code { get; set; }
		public string Message { get; set; }
		public int Count { get; set; }
		public bool IsSuccessful => Code == ErrorCodeMessage.Success.Key;

		public object Data { get; set; }
	}
}
