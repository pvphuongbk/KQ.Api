using KQ.DataDto.Calculation;

namespace KQ.DataDto.HandlMessage
{
    public class MessgeByDayResponse
    {
        public List<DetailMessage> DetailMessage { get; set; }
        public Total Total { get; set; }
        public string Message { get; set; }
        public bool IsThu { get; set; }
    }
    public class DetailMessage
    {
        public Cal3DetailDto CalDetail { get; set; }
        public string Message { get; set; }
    }

    public class Total
    {
        public Summary Xac { get; set; }
        public Summary Trung { get; set; }
        public QuaCo QuaCo { get; set; }
    }

    public class QuaCo
    {
        public double HaiCon { get; set; }
        public double BaCon { get; set; }
        public double BonCon { get; set; }
    }
}
