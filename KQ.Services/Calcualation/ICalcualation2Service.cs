using KQ.Common.Helpers;
using KQ.Common.Repository;
using KQ.Data.Base;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Enum;
using KQ.DataDto.Calculation;
using KQ.DataDto.Enum;
using Newtonsoft.Json;

namespace KQ.Services.Calcualation
{
    public interface ICalcualation2Service
    {
        ResponseBase Cal3Request(Cal3RequestDto dtos);
        public bool UpdateTrungThuong(DateTime handlByDate, CachTrungDa dathang, CachTrungDa daxien, MienEnum mien, ref Cal3DetailDto detail, StoreKQ? kq = null);
        public void UpdateSumTrungThuong(ref Cal3DetailDto detail);
        public string ChuanHoaTin(string sys);
        public ResponseBase Filter(Cal3RequestDto cal3);
    }
}
