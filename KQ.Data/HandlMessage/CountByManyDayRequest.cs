namespace KQ.DataDto.HandlMessage
{
    public class CountByManyDayRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int UserID { get; set; }
    }
}
