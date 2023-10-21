using KQ.DataDto.Enum;

namespace KQ.DataDto.Calculation
{
    public class CalTest2RequestDto
    {
        public string SynTaxe { get; set; }
        public double Xac { get; set; }
        public string MessageLoi { get; set; }
        public DateTime? DateTime { get; set; }
        public MienEnum? Mien { get; set;}
    }
}
