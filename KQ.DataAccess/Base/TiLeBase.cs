using KQ.DataAccess.Enum;

namespace KQ.DataAccess.Base
{
    public class TiLeBase
    {
        #region - Cài đặt miền nam
        public double NCoBaoHaiCon { get; set; }
        public double NTrungBaoHaiCon { get; set; }
        public double NCoHaiConDD { get; set; }
        public double NTrungHaiConDD { get; set; }
        public double NCoDaThang { get; set; }
        public double NTrungDaThang { get; set; }
        public CachTrungDa NCachTrungDaThang { get; set; }
        public double NCoDaXien { get; set; }
        public double NTrungDaXien { get; set; }
        public CachTrungDa NCachTrungDaXien { get; set; }
        public double NCoBaCon { get; set; }
        public double NTrungBaCon { get; set; }
        public double NCoBonCon { get; set; }
        public double NTrungBonCon { get; set; }
        public double NPhanTramTong { get; set; }
        #endregion
        #region - Cài đặt miền trung
        public double TCoBaoHaiCon { get; set; }
        public double TTrungBaoHaiCon { get; set; }
        public double TCoHaiConDD { get; set; }
        public double TTrungHaiConDD { get; set; }
        public double TCoDaThang { get; set; }
        public double TTrungDaThang { get; set; }
        public CachTrungDa TCachTrungDaThang { get; set; }
        public double TCoDaXien { get; set; }
        public double TTrungDaXien { get; set; }
        public CachTrungDa TCachTrungDaXien { get; set; }
        public double TCoBaCon { get; set; }
        public double TTrungBaCon { get; set; }
        public double TCoBonCon { get; set; }
        public double TTrungBonCon { get; set; }
        public double TPhanTramTong { get; set; }
        #endregion
        #region - Cài đặt miền bắc
        public double BCoBaoHaiCon { get; set; }
        public double BTrungBaoHaiCon { get; set; }
        public double BCoHaiConDD { get; set; }
        public double BTrungHaiConDD { get; set; }
        public double BCoDaThang { get; set; }
        public double BTrungDaThang { get; set; }
        public CachTrungDa BCachTrungDaThang { get; set; }
        public double BCoBaCon { get; set; }
        public double BTrungBaCon { get; set; }
        public double BCoBonCon { get; set; }
        public double BTrungBonCon { get; set; }
        public double BPhanTramTong { get; set; }
        public double BCoXienHai { get; set; }
        public double BTrungXienHai { get; set; }
        public double BCoXienBa { get; set; }
        public double BTrungXienBa { get; set; }
        public double BCoXienBon { get; set; }
        public double BTrungXienBon { get; set; }
        #endregion
    }
}
