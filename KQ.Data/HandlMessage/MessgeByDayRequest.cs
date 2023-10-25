using KQ.DataAccess.Enum;
using KQ.DataDto.Enum;

namespace KQ.DataDto.HandlMessage
{
    public class MessgeByDayRequest
    {
        public DateTime HandlDate { get; set; }
        public int IDKhach { get; set; }
        public MienEnum Mien { get; set; }
    }
}
