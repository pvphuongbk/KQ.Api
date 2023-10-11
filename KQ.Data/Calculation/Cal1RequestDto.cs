using KQ.DataDto.Enum;

namespace KQ.DataDto.Calculation
{
    public class Cal1RequestDto
    {
        public LotteryType LotteryType { get; set; }
        public List<int> Numbers { get; set; }
        public List<string> NumbersStr { get; set; }
        public List<int> Chanels { get; set; }
        public int Sl { get; set; }
        public double TileXac { get; set; }
        public double TileThuong { get; set; }
        public double TileBaso { get; set; }
    }
}
