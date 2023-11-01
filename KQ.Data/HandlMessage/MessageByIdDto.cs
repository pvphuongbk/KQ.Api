using KQ.DataDto.Calculation;

namespace KQ.DataDto.HandlMessage
{
    public class MessageByIdDto
    {
        public Summary Xac { get; set; }
        public Summary Trung { get; set; }
        public List<string> TrungDetail { get; set; }
        public List<Detail> Details { get; set; }
        public bool IsTinh { get; set; }
        public string Message { get; set; }
        public DateTime HanldDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
