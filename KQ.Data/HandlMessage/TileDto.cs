using KQ.DataAccess.Enum;

namespace KQ.DataDto.HandlMessage
{
    public class TileDto
    {
        public double CoBaoHaiCon { get; set; }
        public double TrungBaoHaiCon { get; set; }
        public double CoHaiConDD { get; set; }
        public double TrungHaiConDD { get; set; }
        public double CoDaThang { get; set; }
        public double TrungDaThang { get; set; }
        public CachTrungDa CachTrungDaThang { get; set; }
        public double CoDaXien { get; set; }
        public double TrungDaXien { get; set; }
        public CachTrungDa CachTrungDaXien { get; set; }
        public double CoBaCon { get; set; }
        public double TrungBaCon { get; set; }
        public double CoBonCon { get; set; }
        public double TrungBonCon { get; set; }
        public double PhanTramTong { get; set; }
        public bool IsChu { get; set; }
    }
}
