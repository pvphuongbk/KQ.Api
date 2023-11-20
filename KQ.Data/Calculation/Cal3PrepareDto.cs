using KQ.DataDto.Enum;

namespace KQ.DataDto.Calculation
{
    public class Cal3PrepareDto
    {
        public CachChoi CachChoi { get; set; }
        public List<int> Numbers { get; set; }
        public List<string> NumbersStr { get; set; }
        public List<int> Chanels { get; set; }
        public double Sl { get; set; }
        public int Slbao { get; set; }
    }
}
