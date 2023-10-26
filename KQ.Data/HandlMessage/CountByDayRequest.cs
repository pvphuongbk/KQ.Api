using KQ.DataAccess.Enum;
using KQ.DataDto.Enum;

namespace KQ.DataDto.HandlMessage
{
    public class CountByDayRequest
    {
        public DateTime HandlDate { get; set; }
        public int UserID { get; set; }
    }
}
