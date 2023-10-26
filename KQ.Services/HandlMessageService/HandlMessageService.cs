using KQ.Data.Base;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Enum;
using KQ.DataAccess.Interface;
using KQ.DataAccess.UnitOfWork;
using KQ.DataDto.Calculation;
using KQ.DataDto.Enum;
using KQ.DataDto.HandlMessage;
using KQ.Services.Calcualation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KQ.Services.HandlMessageService
{
    public class HandlMessageService : IHandlMessageService
    {
        private readonly ICommonRepository<Details> _detailsRepository;
        private readonly ICommonRepository<TileUser> _tileUserRepository;
        private ICalcualation2Service _calcualation2Service;
        private readonly ICommonUoW _commonUoW;
        public HandlMessageService(ICommonRepository<Details> detailsRepository, ICalcualation2Service calcualation2Service, ICommonUoW commonUoW, ICommonRepository<TileUser> tileUserRepository)
        {
            _detailsRepository = detailsRepository;
            _calcualation2Service = calcualation2Service;
            _commonUoW = commonUoW;
            _tileUserRepository = tileUserRepository;
        }
        public ResponseBase Delete(int id)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                var mess = _detailsRepository.FindAll(x => x.ID == id).FirstOrDefault();
                if (mess != null)
                {
                    _commonUoW.BeginTransaction();
                    _detailsRepository.Remove(mess);
                    _commonUoW.Commit();
                }
                else
                {
                    response.Message = "Tin không tồn tại";
                }
            }
            catch (Exception ex)
            {
                _commonUoW.RollBack();
                response.Code = 500;
                response.Message = ex.Message;
            }

            return response;
        }
        public ResponseBase MessageByDay(MessgeByDayRequest request)
        {
            try
            {
                var details = _detailsRepository.FindAll(x => x.HandlByDate.Date == request.HandlDate.Date && x.IDKhach == request.IDKhach).ToList();
                var tile = _tileUserRepository.GetById(request.IDKhach);
                MessgeByDayResponse result = new MessgeByDayResponse
                {
                    DetailMessage = new List<DetailMessage>(),
                    Total = new Total { QuaCo = new QuaCo(), Trung = new Summary(), Xac = new Summary() },
                };
                List<Details> listUpdate = new List<Details>();
                var tileDto = GetAllTiLeByMien(tile, request.Mien);
                int no = 0;
                foreach (var item in details)
                {
                    no++;
                    var detail = JsonConvert.DeserializeObject<Cal3DetailDto>(item.Detail);
                    if (!detail.IsTinh)
                    {
                        var isUpdate = _calcualation2Service.UpdateTrungThuong(request.HandlDate, tileDto.CachTrungDaThang, tileDto.CachTrungDaXien, request.Mien, ref detail);
                        if (isUpdate)
                        {
                            _calcualation2Service.UpdateSumTrungThuong(ref detail);
                            item.IsTinh = true;
                            item.Detail = JsonConvert.SerializeObject(detail);
                            listUpdate.Add(item);
                        }
                    }
                    result.DetailMessage.Add(new DetailMessage { CalDetail = detail, Message = item.Message, 
                        CreatedDate = item.CreatedDate, HandlByDate = item.HandlByDate, ID = item.ID, No = no});
                    result.Total.Xac.HaiCB += detail.Xac.HaiCB;
                    result.Total.Xac.HaiCD += detail.Xac.HaiCD;
                    result.Total.Xac.DaT += detail.Xac.DaT;
                    result.Total.Xac.DaX += detail.Xac.DaX;
                    result.Total.Xac.BaCon += detail.Xac.BaCon;
                    result.Total.Xac.BonCon += detail.Xac.BonCon;

                    result.Total.Trung.HaiCB += detail.Trung.HaiCB;
                    result.Total.Trung.HaiCD += detail.Trung.HaiCD;
                    result.Total.Trung.DaT += detail.Trung.DaT;
                    result.Total.Trung.DaX += detail.Trung.DaX;
                    result.Total.Trung.BaCon += detail.Trung.BaCon;
                    result.Total.Trung.BonCon += detail.Trung.BonCon;
                }
                result.Total.QuaCo.HaiCon = (result.Total.Xac.HaiCB * tileDto.CoBaoHaiCon) + (result.Total.Xac.HaiCD * tileDto.CoHaiConDD)
                                            + (result.Total.Xac.DaT * tileDto.CoDaThang) + (result.Total.Xac.DaX * tileDto.CoDaXien);
                result.Total.QuaCo.BaCon = (result.Total.Xac.BaCon * tileDto.CoBaCon);
                result.Total.QuaCo.BonCon = (result.Total.Xac.BonCon * tileDto.CoBonCon);

                var tongTrung = (result.Total.Trung.HaiCB * tileDto.TrungBaoHaiCon) + (result.Total.Trung.HaiCD * tileDto.TrungHaiConDD)
                            + (result.Total.Trung.DaT * tileDto.TrungDaThang) + (result.Total.Trung.DaX * tileDto.TrungDaXien)
                            + (result.Total.Trung.BaCon * tileDto.TrungBaCon) + (result.Total.Trung.BonCon * tileDto.TrungBonCon);

                var tong = (result.Total.QuaCo.HaiCon + result.Total.QuaCo.BaCon + result.Total.QuaCo.BonCon) - tongTrung;
                if (tile.IsChu)
                    tong = 0 - tong;
                result.IsThu = tong >= 0;
                result.Message = $"{tong}*{tileDto.PhanTramTong}%={tong * tileDto.PhanTramTong}";
                if (listUpdate.Any())
                {
                    _commonUoW.BeginTransaction();
                    _detailsRepository.UpdateMultiple(listUpdate.AsQueryable());
                    _commonUoW.Commit();
                }

                return new ResponseBase { Data = result };
            }
            catch (Exception ex)
            {
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }
        private TileDto GetAllTiLeByMien(TileUser tileUser, MienEnum mien)
        {
            if (mien == MienEnum.MN)
                return new TileDto
                {
                    CachTrungDaThang = tileUser.NCachTrungDaThang,
                    CachTrungDaXien = tileUser.NCachTrungDaXien,
                    CoBaoHaiCon = tileUser.NCoBaoHaiCon,
                    TrungBaoHaiCon = tileUser.NTrungBaoHaiCon,
                    CoHaiConDD = tileUser.NCoHaiConDD,
                    TrungHaiConDD = tileUser.NTrungHaiConDD,
                    CoDaThang = tileUser.NCoDaThang,
                    TrungDaThang = tileUser.NTrungDaThang,
                    CoDaXien = tileUser.NCoDaXien,
                    TrungDaXien = tileUser.NTrungDaXien,
                    CoBaCon = tileUser.NCoBaCon,
                    TrungBaCon = tileUser.NTrungBaCon,
                    CoBonCon = tileUser.NCoBonCon,
                    TrungBonCon = tileUser.NTrungBonCon,
                    PhanTramTong = tileUser.NPhanTramTong,
                    IsChu = tileUser.IsChu,
                };
            else if (mien == MienEnum.MT)
                return new TileDto
                {
                    CachTrungDaThang = tileUser.TCachTrungDaThang,
                    CachTrungDaXien = tileUser.TCachTrungDaXien,
                    CoBaoHaiCon = tileUser.TCoBaoHaiCon,
                    TrungBaoHaiCon = tileUser.TTrungBaoHaiCon,
                    CoHaiConDD = tileUser.TCoHaiConDD,
                    TrungHaiConDD = tileUser.TTrungHaiConDD,
                    CoDaThang = tileUser.TCoDaThang,
                    TrungDaThang = tileUser.TTrungDaThang,
                    CoDaXien = tileUser.TCoDaXien,
                    TrungDaXien = tileUser.TTrungDaXien,
                    CoBaCon = tileUser.TCoBaCon,
                    TrungBaCon = tileUser.TTrungBaCon,
                    CoBonCon = tileUser.TCoBonCon,
                    TrungBonCon = tileUser.TTrungBonCon,
                    PhanTramTong = tileUser.TPhanTramTong,
                    IsChu = tileUser.IsChu,
                };
            else
                return new TileDto
                {
                    CachTrungDaThang = tileUser.BCachTrungDaThang,
                    CoBaoHaiCon = tileUser.BCoBaoHaiCon,
                    TrungBaoHaiCon = tileUser.BTrungBaoHaiCon,
                    CoHaiConDD = tileUser.BCoHaiConDD,
                    TrungHaiConDD = tileUser.BTrungHaiConDD,
                    CoDaThang = tileUser.BCoDaThang,
                    TrungDaThang = tileUser.BTrungDaThang,
                    CoBaCon = tileUser.BCoBaCon,
                    TrungBaCon = tileUser.BTrungBaCon,
                    CoBonCon = tileUser.BCoBonCon,
                    TrungBonCon = tileUser.BTrungBonCon,
                    PhanTramTong = tileUser.BPhanTramTong,
                    IsChu = tileUser.IsChu,
                };
        }
    }
}
