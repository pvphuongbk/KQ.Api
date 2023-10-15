using KQ.DataAccess.Base;

namespace KQ.DataDto.Calculation
{
    public class Cal2RequestDto : TiLeBase
    {
        public List<string> SynTaxes { get; set; }
        public double TileXac { get; set; }
        public double TileThuong { get; set; }
        public double TileBaso { get; set; }
        public double DaThang { get; set; }
        public double DaXien { get; set; }
        public double BonSo { get; set; }
        public int? UserId { get; set; }
    }
}
