using KQ.Common.Extention;
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
using System.Diagnostics;
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
        
        public ResponseBase CountByManyDayRequest(CountByManyDayRequest request)
        {
            try
            {
                List<CountByDayResponse> lst = new List<CountByDayResponse>();
                var allDetails = _detailsRepository.FindAll(x => x.HandlByDate.Date >= request.FromDate.Date 
                        && x.HandlByDate.Date <= request.ToDate.Date
                        && x.UserID == request.UserID).ToList();
                List<Details> listUpdate = new List<Details>();
                var tiles = _tileUserRepository.FindAll(x => allDetails.Select(x => x.IDKhach).Distinct().Contains(x.ID)).ToList();
                for(DateTime handlDate = request.FromDate.Date; handlDate.Date <= request.ToDate.Date; handlDate = handlDate.AddDays(1))
                {
                    var details = allDetails.Where(x => x.HandlByDate.Date == handlDate.Date).ToList();
                    foreach (var item in details)
                    {
                        var tile = tiles.FirstOrDefault(x => x.ID == item.IDKhach);
                        var tileDto = GetAllTiLeByMien(tile, item.Mien);
                        var detail = JsonConvert.DeserializeObject<Cal3DetailDto>(item.Detail.Decrypt());
                        if (!detail.IsTinh)
                        {
                            var isUpdate = _calcualation2Service.UpdateTrungThuong(handlDate, tileDto.CachTrungDaThang, tileDto.CachTrungDaXien, item.Mien, ref detail);
                            if (isUpdate)
                            {
                                _calcualation2Service.UpdateSumTrungThuong(ref detail);
                                item.IsTinh = true;
                                item.Detail = JsonConvert.SerializeObject(detail).Encrypt();
                                listUpdate.Add(item);
                            }
                        }
                        var counting = lst.FirstOrDefault(x => x.IDKhach == item.IDKhach);
                        if (counting == null)
                        {
                            counting = new CountByDayResponse { IDKhach = item.IDKhach, Name = tile.Name, IsChu = tile.IsChu };
                            lst.Add(counting);
                        }

                        var tongCo = (detail.Xac.HaiCB * tileDto.CoBaoHaiCon) + (detail.Xac.HaiCD * tileDto.CoHaiConDD)
                                + (detail.Xac.DaT * tileDto.CoDaThang) + (detail.Xac.DaX * tileDto.CoDaXien) + (detail.Xac.BaCon * tileDto.CoBaCon)
                                + (detail.Xac.BonCon * tileDto.CoBonCon);

                        var tongTrung = (detail.Trung.HaiCB * tileDto.TrungBaoHaiCon) + (detail.Trung.HaiCD * tileDto.TrungHaiConDD)
                                    + (detail.Trung.DaT * tileDto.TrungDaThang) + (detail.Trung.DaX * tileDto.TrungDaXien)
                                    + (detail.Trung.BaCon * tileDto.TrungBaCon) + (detail.Trung.BonCon * tileDto.TrungBonCon);

                        var tong = ((tongCo - tongTrung) * tileDto.PhanTramTong) / 100;
                        if (tile.IsChu)
                            tong = 0 - tong;
                        counting.Total += tong;
                        switch (item.Mien)
                        {
                            case MienEnum.MN:
                                counting.MienNam += tong;
                                break;
                            case MienEnum.MT:
                                counting.MienTrung += tong;
                                break;
                            default:
                                counting.MienBac += tong;
                                break;
                        }
                    }
                }

                foreach(var item in lst)
                {
                    item.Total = Math.Round(item.Total, 1);
                    item.MienNam = Math.Round(item.MienNam, 1);
                    item.MienTrung = Math.Round(item.MienTrung, 1);
                    item.MienBac = Math.Round(item.MienBac, 1);
                }
                if (listUpdate.Any())
                {
                    _commonUoW.BeginTransaction();
                    _detailsRepository.UpdateMultiple(listUpdate.AsQueryable());
                    _commonUoW.Commit();
                }

                return new ResponseBase { Data = lst };
            }
            catch (Exception ex)
            {
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }
        public ResponseBase CountByDayRequest(CountByDayRequest request)
        {
            try
            {
                List<CountByDayResponse> lst = new List<CountByDayResponse>();
                var details = _detailsRepository.FindAll(x => x.HandlByDate.Date == request.HandlDate.Date && x.UserID == request.UserID).ToList();
                List<Details> listUpdate = new List<Details>();
                int no = 0;
                var tiles = _tileUserRepository.FindAll(x => details.Select(x => x.IDKhach).Distinct().Contains(x.ID)).ToList();
                foreach (var item in details)
                {
                    var tile = tiles.FirstOrDefault(x => x.ID == item.IDKhach);
                    var tileDto = GetAllTiLeByMien(tile, item.Mien);
                    var detail = JsonConvert.DeserializeObject<Cal3DetailDto>(item.Detail.Decrypt());
                    if (!detail.IsTinh)
                    {
                        var isUpdate = _calcualation2Service.UpdateTrungThuong(request.HandlDate, tileDto.CachTrungDaThang, tileDto.CachTrungDaXien, item.Mien, ref detail);
                        if (isUpdate)
                        {
                            _calcualation2Service.UpdateSumTrungThuong(ref detail);
                            item.IsTinh = true;
                            item.Detail = JsonConvert.SerializeObject(detail).Encrypt();
                            listUpdate.Add(item);
                        }
                    }
                    var counting = lst.FirstOrDefault(x => x.IDKhach == item.IDKhach);
                    if (counting == null)
                    {
                        counting = new CountByDayResponse { IDKhach = item.IDKhach, Name = tile.Name, IsChu = tile.IsChu };
                        lst.Add(counting);
                    }

                    var tongCo = (detail.Xac.HaiCB * tileDto.CoBaoHaiCon) + (detail.Xac.HaiCD * tileDto.CoHaiConDD)
                            + (detail.Xac.DaT * tileDto.CoDaThang) + (detail.Xac.DaX * tileDto.CoDaXien) + (detail.Xac.BaCon * tileDto.CoBaCon)
                            + (detail.Xac.BonCon * tileDto.CoBonCon);

                    var tongTrung = (detail.Trung.HaiCB * tileDto.TrungBaoHaiCon) + (detail.Trung.HaiCD * tileDto.TrungHaiConDD)
                                + (detail.Trung.DaT * tileDto.TrungDaThang) + (detail.Trung.DaX * tileDto.TrungDaXien)
                                + (detail.Trung.BaCon * tileDto.TrungBaCon) + (detail.Trung.BonCon * tileDto.TrungBonCon);

                    var tong = ((tongCo - tongTrung)* tileDto.PhanTramTong)/100;
                    if (tile.IsChu)
                        tong = 0 - tong;
                    counting.Total += tong;
                    switch (item.Mien)
                    {
                        case MienEnum.MN:
                            counting.MienNam += tong;
                            break;
                        case MienEnum.MT:
                            counting.MienTrung += tong;
                            break;
                        default:
                            counting.MienBac += tong;
                            break;
                    }

                    counting.Total = Math.Round(counting.Total, 1);
                    counting.MienNam = Math.Round(counting.MienNam, 1);
                    counting.MienTrung = Math.Round(counting.MienTrung, 1);
                    counting.MienBac = Math.Round(counting.MienBac, 1);
                }
                if (listUpdate.Any())
                {
                    _commonUoW.BeginTransaction();
                    _detailsRepository.UpdateMultiple(listUpdate.AsQueryable());
                    _commonUoW.Commit();
                }

                return new ResponseBase { Data = lst };
            }
            catch (Exception ex)
            {
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }
        public ResponseBase MessageByID(int messageID)
        {
            try
            {
                MessageByIdDto result = new MessageByIdDto
                {
                    Xac = new Summary(),
                    Trung = new Summary(),
                    TrungDetail = new List<string>()
                };
                var item = _detailsRepository.FindAll(x => x.ID == messageID).FirstOrDefault();
                if (item == null)
                    return new ResponseBase { Data = result }; ;
                var tile = _tileUserRepository.GetById(item.IDKhach);
                Details updateItem = null;
                var tileDto = GetAllTiLeByMien(tile, item.Mien);
                int no = 0;
                no++;
                var detail = JsonConvert.DeserializeObject<Cal3DetailDto>(item.Detail.Decrypt());
                if (!detail.IsTinh)
                {
                    var isUpdate = _calcualation2Service.UpdateTrungThuong(item.HandlByDate, tileDto.CachTrungDaThang, tileDto.CachTrungDaXien, item.Mien, ref detail);
                    if (isUpdate)
                    {
                        _calcualation2Service.UpdateSumTrungThuong(ref detail);
                        item.IsTinh = true;
                        item.Detail = JsonConvert.SerializeObject(detail).Encrypt();
                        updateItem = item;
                    }
                }
                result.TrungDetail = detail.TrungDetail;
                result.HanldDate = item.HandlByDate;
                result.CreatedDate = item.CreatedDate;
                result.Details = detail.Details;
                result.Message = item.Message.Decrypt();
                result.Xac.HaiCB = Math.Round(detail.Xac.HaiCB,2);
                result.Xac.HaiCD = Math.Round(detail.Xac.HaiCD, 2);
                result.Xac.DaT = Math.Round(detail.Xac.DaT, 2);
                result.Xac.DaX = Math.Round(detail.Xac.DaX, 2);
                result.Xac.BaCon = Math.Round(detail.Xac.BaCon, 2);
                result.Xac.BonCon = detail.Xac.BonCon;

                result.Trung.HaiCB = Math.Round(detail.Trung.HaiCB, 2);
                result.Trung.HaiCD = Math.Round(detail.Trung.HaiCD, 2);
                result.Trung.DaT = Math.Round(detail.Trung.DaT,2);
                result.Trung.DaX = Math.Round(detail.Trung.DaX,2);
                result.Trung.BaCon = Math.Round(detail.Trung.BaCon, 2);
                result.Trung.BonCon = Math.Round(detail.Trung.BonCon,2);

                if (updateItem != null)
                {
                    _commonUoW.BeginTransaction();
                    _detailsRepository.Update(updateItem);
                    _commonUoW.Commit();
                }

                return new ResponseBase { Data = result };
            }
            catch (Exception ex)
            {
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }
        public ResponseBase HandleMessage(MessgeByDayRequest request)
        {
            try
            {
                var details = _detailsRepository.FindAll(x => x.HandlByDate.Date == request.HandlDate.Date
                                                && x.IDKhach == request.IDKhach && x.Mien == request.Mien).ToList();
                var result = new List<HandlMessageDto>();
                foreach(var item in details)
                {
                    result.Add(new HandlMessageDto
                    {
                        Id = item.ID,
                        Message = item.Message.Decrypt(),
                        CreatedDate = item.CreatedDate,
                        HandlDate = item.HandlByDate
                    });
                }
                return new ResponseBase { Data = result };
            }
            catch (Exception ex)
            {
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }
        public ResponseBase MessageByDay(MessgeByDayRequest request)
        {
            try
            {
                var details = _detailsRepository.FindAll(x => x.HandlByDate.Date == request.HandlDate.Date 
                                                && x.IDKhach == request.IDKhach && x.Mien == request.Mien).ToList();
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
                    var detail = JsonConvert.DeserializeObject<Cal3DetailDto>(item.Detail.Decrypt());
                    if (!detail.IsTinh)
                    {
                        var isUpdate = _calcualation2Service.UpdateTrungThuong(request.HandlDate, tileDto.CachTrungDaThang, tileDto.CachTrungDaXien, request.Mien, ref detail);
                        if (isUpdate)
                        {
                            _calcualation2Service.UpdateSumTrungThuong(ref detail);
                            item.IsTinh = true;
                            item.Detail = JsonConvert.SerializeObject(detail).Encrypt();
                            listUpdate.Add(item);
                        }
                    }
                    result.DetailMessage.Add(new DetailMessage { Xac = detail.Xac,Trung = detail.Trung, Message = item.Message.Decrypt(), 
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
                var total = (tong * tileDto.PhanTramTong) / 100;
                total = Math.Round(total,2);
                tong = Math.Round(tong, 2);
                result.Message = $"{tong.ToString()}*{tileDto.PhanTramTong}" +
                $"%={tong.ToString()}";
                if (listUpdate.Any())
                {
                    _commonUoW.BeginTransaction();
                    _detailsRepository.UpdateMultiple(listUpdate.AsQueryable());
                    _commonUoW.Commit();
                }
                result.Total.QuaCo.HaiCon = Math.Round(result.Total.QuaCo.HaiCon, 1);
                result.Total.QuaCo.BaCon = Math.Round(result.Total.QuaCo.BaCon, 1);
                result.Total.QuaCo.BonCon = Math.Round(result.Total.QuaCo.BonCon, 1);
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
