using KQ.Common.Constants;
using KQ.Common.Extention;
using KQ.Common.Helpers;
using KQ.Data.Base;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Enum;
using KQ.DataAccess.Interface;
using KQ.DataAccess.UnitOfWork;
using KQ.DataAccess.Utilities;
using KQ.DataDto.Calculation;
using KQ.DataDto.Enum;
using KQ.Services.Calcualation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenQA.Selenium.Remote;
using System.IO;
using System.Reflection;

namespace KQ.Services.CommonServices
{
    public class CommonService : ICommonService
    {
        private readonly ICalcualationService _calcualationService;
        private readonly ICalcualation2Service _calcualation2Service;
        private readonly ICommonRepository<StoreKQ> _storeKQRepository;
        private readonly ICommonUoW _commonUoW;
        public CommonService(ICalcualationService calcualationService, ICalcualation2Service calcualation2Service, ICommonRepository<StoreKQ> storeKQRepository, ICommonUoW commonUoW)
        {
            _calcualationService = calcualationService;
            _calcualation2Service = calcualation2Service;
            _storeKQRepository = storeKQRepository;
            _commonUoW = commonUoW;
        }
        public ResponseBase CheckKQ(DayOfWeek? day)
        {
            ResponseBase response = new ResponseBase();
            string key = day != null ? ((DayOfWeek)day).ToString() : "Now";
            lock(InnitRepository._totalDic)
            {
                var data = new
                {
                    Lo = InnitRepository._totalDic[key],
                    BaCang = InnitRepository._totalBaCangDic[key],
                    BonSo = InnitRepository._totalBonSoDic[key],
                };
                //_commonUoW.BeginTransaction();
                //_storeKQRepository.Insert(new StoreKQ
                //{
                //    CreatedDate = new DateTime(2023,10,18),
                //    HaiCon = JsonConvert.SerializeObject(data.Lo),
                //    BaCon = JsonConvert.SerializeObject(data.BaCang),
                //    BonCon = JsonConvert.SerializeObject(data.BonSo),
                //});
                //_commonUoW.Commit();

                response.Data = data;
                return response;
            }
        }
        public ResponseBase CheckChanelCode()
        {
            ResponseBase response = new ResponseBase();
            var codes = CommonFunction.GetChanelCodeForNow();
            response.Data = codes;
            return response;
        }
        public ResponseBase UnitTestCal3()
        {
            ResponseBase response = new ResponseBase();
            List<string> result = new List<string>();
            var teststos = new List<CalTest2RequestDto>
            {
                // Miền nam
                new CalTest2RequestDto{SynTaxe = "2d  72 89 04 83 10 dd100n 90 96 17 b25 15 33 b20 68 78 dd10n 78 d50n d140n 278 678 xc20n 3d 35 55 da1n"  //0
                , Xac = 6976, Trung = 100},
                new CalTest2RequestDto{SynTaxe = "3d 28 87 59 dax10n b10n", DateTime = new DateTime(2023,10,20),Xac = 8100, Trung =130},                   //1
                new CalTest2RequestDto{SynTaxe = "3d  34 56 78 dax20nb30", Xac = 17820, Trung = 130},                                                      //2
                new CalTest2RequestDto{SynTaxe = "2d 11 22 33 b10 d20 d30 da15 223 670 xdau40n xcdao 50n", Xac = 6580, Trung = 0},                         //3
                new CalTest2RequestDto{SynTaxe = "2d  72keo27 dd100n  027k072 xc30 bao10 0027kht1027 bao10", Xac= 43080, Trung =220},                      //4
                new CalTest2RequestDto{SynTaxe = "2d  072kht272 xcdau100n xc30 05k95 bao10", Xac = 33720, Trung = 340},                                    //5
                new CalTest2RequestDto{SynTaxe = "2d  113khc563 xcdau100n 563khc113 bao10", Xac = 24840, Trung = 0},                                       //6
                new CalTest2RequestDto{SynTaxe = "2d  15khc95 dd100n 95k13 bao10", Xac = 33480, Trung = 290},                                              //7
                new CalTest2RequestDto{SynTaxe = "2d  213kht513 xcdau100n 513kht213 bao10", Xac = 2160, Trung = 0},                                        //8
                new CalTest2RequestDto{SynTaxe = "2d  72 89  04 83 27 dd100n bao50n 690 xc30n", Xac = 11120, Trung = 50},                                  //9
                new CalTest2RequestDto{SynTaxe = "2d   72 89  04 83 27 dd100n  bao50n 690 xc30n", Xac = 11120, Trung = 50},                                //10
                new CalTest2RequestDto{SynTaxe = "1 dai 72 89 04 83 27 b100n 2d 067 345 b20", Xac = 10360},                                                //11
                new CalTest2RequestDto{SynTaxe = "dn,Cần Thơ st 72 89 04 dui100ndax20", Xac = 13860, Trung = 20},                                          //12
                new CalTest2RequestDto{SynTaxe = "dn,Cần Thơ st 72 89 04 d100d20nda20nb40", Xac = 20520, Trung = 140},                                     //13
                new CalTest2RequestDto{SynTaxe = "Đài Chánh.10₫15.n.₫10n.72₫₫10n. 518b2n..438.731b1n.xc5n.511b2n.xc10n..11b5n.  2dai..59₫₫10n.29₫5n.₫15n. 429xc15n.359.b1n.xc5n.731b2n.xc20n." +
                ".538.583b1n.xc6n. 38.83b5n.29.31.10b10n. 38.83da2n. 31.29.10₫a2n. 64.11.72da1n. 3dai.18.43b10n.", Xac = 4047, Trung = 42},                //14

                // Lỗi
                new CalTest2RequestDto{SynTaxe = "2d  29k79 xc100n", MessageLoi = "29k79 xc100n"},                                                         //15
                new CalTest2RequestDto{SynTaxe = "2d  072keo527 dd100n xc30 05k95 bao10", MessageLoi = "072keo527 dd100n"},                                //16
                new CalTest2RequestDto{SynTaxe = "2d 11 b10n 29ka79 82 dd100n", MessageLoi = "29ka"},                                                      //17
                new CalTest2RequestDto{SynTaxe = "vl la  72ka29 dd100n 27a72 bao10", MessageLoi = "vl la"},                                                //18
                new CalTest2RequestDto{SynTaxe = "4d  72keo29 dd100n 27k72 bao10", MessageLoi = "4d"},                                                     //19
                new CalTest2RequestDto{SynTaxe = "2d  112khc563 dd100n 563k113 bao10", Mien = MienEnum.MB, MessageLoi = "2d"},                             //20
                new CalTest2RequestDto{SynTaxe = "hn 11 22 dd100n 95k13 bao10 1d 23dd4n", Mien = MienEnum.MB,Xac = 23430, Trung = 210},                    //21
                new CalTest2RequestDto{SynTaxe = "2d  11 dd100n 513kht212 bao10", MessageLoi = "513kht212"},                                               //22
                new CalTest2RequestDto{SynTaxe = "2d  112 dd100n 3d 127k72 bao10", MessageLoi = "112 dd100n"},                                             //23
                new CalTest2RequestDto{SynTaxe = "2d dd100n  72 27 dd100n bao50n 690 xc30n 2k 23 36 bao10", MessageLoi = "dd100n"},                        //24
                new CalTest2RequestDto{SynTaxe = "2d  72 27 dd100n bao50n 690 xc30n VL,la 23 36 bao10", MessageLoi = "VL,la"},                             //25
                new CalTest2RequestDto{SynTaxe = "2d  72 27 dd100n bao50n 690 xc30n 2t 23 36 bao10", MessageLoi = "2t"},                                   //26
                new CalTest2RequestDto{SynTaxe = "2d  45678 bao10", MessageLoi = "45678"},                                                                 //27
                new CalTest2RequestDto{SynTaxe = "2d  4567 xc10", MessageLoi = "4567 xc10"},                                                               //28
                new CalTest2RequestDto{SynTaxe = "2d  112khc563 dd100n 563k113 bao10", MessageLoi = "112khc563"},                                          //29
                new CalTest2RequestDto{SynTaxe = "2d  456 dd10", MessageLoi = "456 dd10"},                                                                 //30
                new CalTest2RequestDto{SynTaxe = "Đài Chánh.10₫15.n.₫10n.72₫₫10n. 518b2n..438.731b1n.xc5n.511b2n.xc10n..11b5n. " +                         
                " 2dai..59₫₫10n.29₫5n.₫15n. 429xc15n.359.b1n.xc5n.731b2n.xc20n." +                                                                         
                ".538.583b1n.xc6n. 38.83b5n.29.31.10b10n. 38.83da2n. 31.29.10₫a2n. 64.11.72da1n. 4dai.18.43b10n.", MessageLoi = "4dai"},                   //31
                new CalTest2RequestDto{SynTaxe = "dc 34 56 78 d20b30", MessageLoi = "34 56 78 d20"},                                                       //32
                new CalTest2RequestDto{SynTaxe = "2d  72 27 dd100n bao50n 690 xc30n 2tfdfdf vl 23 36 bao10", MessageLoi = "2tfdfdf vl"},                   //33
                new CalTest2RequestDto{SynTaxe = "dc 34 56 78 d20b30", MessageLoi = "34 56 78 d20", Mien = MienEnum.MB},                                   //34
                new CalTest2RequestDto{SynTaxe = "dc 34 56 78 b30 1d 245 34 b10n", MessageLoi = "245 34", Mien = MienEnum.MB},                             //35
                new CalTest2RequestDto{SynTaxe = "mb 34 56 78 b30 hn 2456 123 34 b10n", MessageLoi = "2456 123 34", Mien = MienEnum.MB},                   //36
                new CalTest2RequestDto{SynTaxe = "hn 34 56 78 b30 hn 24 12 b10nb20", MessageLoi = "b10nb20", Mien = MienEnum.MB},                          //37
                new CalTest2RequestDto{SynTaxe = "1d dd20b30 34 56 78 dc 24 12 b10nb20", MessageLoi = "dd20", Mien = MienEnum.MB},                         //38
                new CalTest2RequestDto{SynTaxe = "1d 3467 b10n", Xac = 200, Trung =0, Mien = MienEnum.MB},                                                 //39
                new CalTest2RequestDto{SynTaxe = "1d 346 dd10n", MessageLoi = "346 dd10n", Mien = MienEnum.MB},                                            //40
                new CalTest2RequestDto{SynTaxe = "1d 34 xc10n xcdui10", MessageLoi = "34 xc10n", Mien = MienEnum.MB},                                      //41
                new CalTest2RequestDto{SynTaxe = "1d 34 56 dx100 b10n", MessageLoi = "1d 34 56 dx100", Mien = MienEnum.MB},                                //42
                new CalTest2RequestDto{SynTaxe = "1d 34 da100 b10n", MessageLoi = "34 da100", Mien = MienEnum.MB},                                         //43
                new CalTest2RequestDto{SynTaxe = "1d 34 45 da00 b10n", MessageLoi = "da00", Mien = MienEnum.MB},                                           //44
                new CalTest2RequestDto{SynTaxe = "1d 34 45 da1 b10n 2d 11 b10n", MessageLoi = "2d", Mien = MienEnum.MB},                                   //45
                new CalTest2RequestDto{SynTaxe = "dc 34 45 da1 b10n 2d b10n", MessageLoi = "b10n", Mien = MienEnum.MN},                                    //46
                new CalTest2RequestDto{SynTaxe = "dc 34 45 12 44", MessageLoi = "dc 34 45 12 44", Mien = MienEnum.MN},                                     //47
                new CalTest2RequestDto{SynTaxe = "hn 34 56 78 b30 hn 24 12 b10b20", MessageLoi = "b10b20", Mien = MienEnum.MB},                            //48
                new CalTest2RequestDto{SynTaxe = "hn 34 56 78 b30 hn 24 12 b10nb20n", MessageLoi = "b10nb20n", Mien = MienEnum.MB},                        //49
                new CalTest2RequestDto{SynTaxe = "dc 34 45 bao10n 2d 45 56", MessageLoi = "2d 45 56", Mien = MienEnum.MN},                                 //50
                new CalTest2RequestDto{SynTaxe = "dc 34 45 bao10n 2d", MessageLoi = "2d", Mien = MienEnum.MN},                                             //51
                new CalTest2RequestDto{SynTaxe = "dc 34 45 bao10n 2fdfd", MessageLoi = "2fdfd", Mien = MienEnum.MN},                                       //52
                new CalTest2RequestDto{SynTaxe = "dc 34 45 2d 12 13 bao10n", MessageLoi = "dc 34 45", Mien = MienEnum.MN},                                 //53
                new CalTest2RequestDto{SynTaxe = "dc 11 33 bao10n 34 45 2d 12 13 bao10n", MessageLoi = "34 45", Mien = MienEnum.MN},                       //54
                new CalTest2RequestDto{SynTaxe = "dc 11 22 bao10n 2d 34 45 da10nb0n", MessageLoi = "b0n", Mien = MienEnum.MN},                             //55
                new CalTest2RequestDto{SynTaxe = "dc 11 22 bao10n 2d 34 45 da10nb0ndd10", MessageLoi = "b0n", Mien = MienEnum.MN},                         //56
                new CalTest2RequestDto{SynTaxe = "1d 34 45 da10 1d b10", MessageLoi = "b10", Mien = MienEnum.MB},                                          //57
                new CalTest2RequestDto{SynTaxe = "1d 34 45 da10 hn b10", MessageLoi = "b10", Mien = MienEnum.MB},                                          //58
                new CalTest2RequestDto{SynTaxe = "1d 34 45 da10 10d 11 b10", MessageLoi = "10d 11", Mien = MienEnum.MB},                                   //59
                new CalTest2RequestDto{SynTaxe = "1d 34 45 da10 9d 11 b10", MessageLoi = "9d", Mien = MienEnum.MB},                                        //60

                new CalTest2RequestDto{SynTaxe = "Cần Thơ 72 89 d100 n d20 n da20 nb40 2d 34 b10 3d 10d100d50", Xac = 3210, Trung = 250},                  //61
                new CalTest2RequestDto{SynTaxe = "ct 72 89 d100d20nda20nb40 2d 34 b10 3d 10d100d50 dc 11 33 bao10n", Xac = 3570, Trung = 250},             //62
                new CalTest2RequestDto{SynTaxe = "Cần Thơ 72 89 d100d20nda20nb40 2d 34 b10 3d 10d100d50", Xac = 3210, Trung = 250},
                new CalTest2RequestDto{SynTaxe = "Cần Thơ 72 89 d100d20nda20nb40@ 2d 34 b10 3d 10d100d50", Xac = 3210, Trung = 250},                       //64
                new CalTest2RequestDto{SynTaxe = "dc 11 22 bao10n@ 2d 34 45 da10nb0ndd10", MessageLoi = "b0n", Mien = MienEnum.MN},                        //65
                new CalTest2RequestDto{SynTaxe = "3d 1234 baodao", MessageLoi = "baodao", Mien = MienEnum.MN},                                             //66
                new CalTest2RequestDto{SynTaxe = "3d 123 baodao 13n dd", MessageLoi = "dd", Mien = MienEnum.MN},                                           //67
                new CalTest2RequestDto{SynTaxe = "2d 12 13 dv 13n", Mien = MienEnum.MN, Xac = 936, Trung = 0},                                             //68
                new CalTest2RequestDto{SynTaxe = "2d 12 13 14 dv 13n bl 10n", Xac = 3888, Trung = 10, Mien = MienEnum.MN},                                 //69
                new CalTest2RequestDto{SynTaxe = "2d 12 dv 13n bl 10n", MessageLoi = "12 dv", Mien = MienEnum.MN},                                         //70
                new CalTest2RequestDto{SynTaxe = "2d 122 334 dav 13n bl 10n", MessageLoi = "122 334 dav 13n", Mien = MienEnum.MN},                         //71
                new CalTest2RequestDto{SynTaxe = "2d 22 34 14 dav 13n bl 10n", Xac = 3888, Trung = 10, Mien = MienEnum.MN},                                //72
                new CalTest2RequestDto{SynTaxe = "dc 10b10xc10", MessageLoi = "10xc10", Mien = MienEnum.MN},                                               //73
                new CalTest2RequestDto{SynTaxe = "dc 122b10dd10n", MessageLoi = "10dd10n", Mien = MienEnum.MN},                                            //74
                new CalTest2RequestDto{SynTaxe = "3₫ 11 b10ndd10n.00k05 b10.123 xc10n.56789 bdao10n", MessageLoi = "56789", Mien = MienEnum.MN},           //75
            };
            for(int i = 0; i < teststos.Count; i++)
            {
                if(i == 75)
                {

                }
                var dto = new Cal3RequestDto
                {
                    SynTax = teststos[i].SynTaxe,
                };
                dto.HandlByDate = teststos[i].DateTime == null ? new DateTime(2023, 10, 18) : (DateTime)teststos[i].DateTime;
                dto.Mien = teststos[i].Mien == null ? MienEnum.MN : (MienEnum)teststos[i].Mien;
                dto.CachTrungDaXien = CachTrungDa.NhieuCap;
                dto.CachTrungDaThang = CachTrungDa.NhieuCap;

                var re = (Cal3DetailDto)_calcualation2Service.Cal3Request(dto).Data;
                if(re.Error == null)
                {
                    var totalX = re.Xac.HaiCB + re.Xac.HaiCD + re.Xac.DaT + re.Xac.DaX + re.Xac.BaCon + re.Xac.BonCon;
                    var totalT = re.Trung.HaiCB + re.Trung.HaiCD + re.Trung.DaT + re.Trung.DaX + re.Trung.BaCon + re.Trung.BonCon;
                    if (teststos[i].Xac == totalX && teststos[i].Trung == totalT)
                        result.Add("Pass");
                    else
                        result.Add("Fail");
                }
                else
                {
                    var str = dto.SynTax.Substring(re.Error.StartIndex, re.Error.Count);
                    if(str == teststos[i].MessageLoi)
                        result.Add("Pass");
                    else
                        result.Add("Fail");
                }

            }
            response.Data = result;
            Constants.IstestMode = false;
            return response;
        }
        public ResponseBase UnitTest()
        {
            List<string> result = new List<string>();
            ResponseBase response = new ResponseBase();
            var teststos = new List<CalTestRequestDto>
            {
                new CalTestRequestDto { SynTaxe = "vinh long bl 34,45 50k", Xac = 1368 },                                 //0
                new CalTestRequestDto { SynTaxe = "vl bl 45 50", Xac = 684 },                                             //1
                new CalTestRequestDto { SynTaxe = "vl bl 45 35 60", Xac = 1641.6 },                                       //2
                new CalTestRequestDto { SynTaxe = "vl 45 35 bl 60", Xac = 1641.6 },                                       //3
                new CalTestRequestDto { SynTaxe = "vinh long 45 35 bl 60k", Xac = 1641.6 },                               //4
                new CalTestRequestDto { SynTaxe = "722b10nxc20", Xac = 159.6 },                                           //5
                new CalTestRequestDto { SynTaxe = "72 b10ndd20", Xac = 167.2 },                                           //6
                new CalTestRequestDto { SynTaxe = "72 b10ndd20 hn", Xac = 281.2 },                                        //7
                new CalTestRequestDto { SynTaxe = "Bl 14 30n bl 914 10n xc 30n 2d", Xac = 1170.4 },                       //8
                new CalTestRequestDto { SynTaxe = "Bl 14 20n bl 914 5n xc 20n HN \r\n32 b50 232b10 xc50", Xac = 1911.4 }, //9
                new CalTestRequestDto { SynTaxe = "51 bl50n. Hn", Xac = 1026 },                                           //10
                new CalTestRequestDto { SynTaxe = "03.43.83.đx.5n.36.43.76.đx.5n.hn", Xac = 1231.2,
                    MessageLoi = "Miền bắc chỉ có 1 đài. Không thể đá xiên 2 đài miền bắc." },                            //11
                new CalTestRequestDto { SynTaxe = "Hn 35,59,92 bl 50n đa 10n", Xac = 4309.2 },                            //12
                new CalTestRequestDto { SynTaxe = "03.43.83.đx.5n.mtr", Xac = 820.8 },                                    //13
                new CalTestRequestDto { SynTaxe = "085,734 bl 2ng xchu 5ng 2dai mt", Xac = 133.8 },                       //14
                new CalTestRequestDto { SynTaxe = "34 38 78 đa 1ng hn", Xac = 123.1 },                                    //15
                new CalTestRequestDto { SynTaxe = "722b10nxc20 mb", Xac = 235.6 },                                        //16
                new CalTestRequestDto { SynTaxe = "722b10nxc20 hn", Xac = 235.6 },                                        //17
                new CalTestRequestDto { SynTaxe = "38 bao 100k mot dai", Xac = 1368 },                                    //18
                new CalTestRequestDto { SynTaxe = "38 34 bao 100k một đài", Xac = 2736 },                                 //19
                new CalTestRequestDto { SynTaxe = "722b10nduoi20 mb", Xac = 220.4 },                                       //20
                new CalTestRequestDto { SynTaxe = "722b10nduoi20 hn", Xac = 220.4 },                                       //21
                new CalTestRequestDto { SynTaxe = "72 b10nxc20", Xac = 167.2, MessageLoi = "72 : Không thể đánh 3 số" },   //22
                new CalTestRequestDto { SynTaxe = "2₫ài 22,33đá10nbl10ndd10n", Xac = 1155.2 },                                                 //23
                new CalTestRequestDto { SynTaxe = "Tp 22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n2₫ai 22bl10ndd10n", Xac = 942.4 },            //24
                new CalTestRequestDto { SynTaxe = "Tp 22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n2₫ai mt 22bl10ndd10n", Xac = 942.4 },         //25
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n2₫ai mt 22bl10ndd10n tp,vl,bd", Xac = 1732.8 },  //26
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\nvl,bd 22bl10ndd10n tp,vl,bd", Xac = 1732.8 },    //27
                new CalTestRequestDto { SynTaxe = "vl,bd 22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n3₫ai mt 22bl10ndd10n", Xac = 1732.8 },     //28
                new CalTestRequestDto { SynTaxe = "Tp,vl 22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n4₫ai mt 22bl10ndd10n", Xac = 1732.8,
                                                                                        MessageLoi = "Miền trung hôm nay chỉ có 3 đài" },      //29
                new CalTestRequestDto { SynTaxe = "Tp,vl,bd 22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n2₫ai mb 22bl10ndd10n", Xac = 1732.8,
                                                                                        MessageLoi = "Miền bắc chỉ có 1 đài." },               //30
                new CalTestRequestDto { SynTaxe = "Tp,vl 22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n2₫ai mb 22bl10ndd10n", Xac = 1732.8,
                                                                                        MessageLoi = "Miền bắc chỉ có 1 đài." },               //31
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n2₫ai mb 22bl10ndd10n tp,vl", Xac = 1732.8,
                                                                                        MessageLoi = "Miền bắc chỉ có 1 đài." },                //32
                new CalTestRequestDto { SynTaxe = "Tp,vl 22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n4₫ai mn 22bl10ndd10n", Xac = 1732.8,
                                                                                        MessageLoi = "Miền nam hôm nay chỉ có 3 đài" },        //33
                new CalTestRequestDto { SynTaxe = "Tp,vl 22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\nvl,bd 22bl10ndd10n", Xac = 1580.8 },       //34
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n Tp 2₫ai 22bl10ndd10n", Xac = 942.4 },           //35  
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\n Tp 22bl10ndd10n 2₫ai", Xac = 942.4 },           //36  
                new CalTestRequestDto { SynTaxe = "Tp 22,33đá10nbl10ndd10n.22,33,44,55 dd 10n\r\nvl 22bl10ndd10n", Xac = 790.4 },              //37
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n Tp\r\nvl 22bl10ndd10n", Xac = 1580.8 },             //38  
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n Tp\r\n 22bl10ndd10n vl", Xac = 790.4 },             //39
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n Tp,vl\r\n 22bl10ndd10n", Xac = 1580.8 },            //40  
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n mn 2d\r\n 22bl10ndd10n", Xac = 1580.8 },            //41
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n mn\r\n 22bl10ndd10n", Xac = 790.4 },                //42
                new CalTestRequestDto { SynTaxe = "22,33đá10nbl10ndd10n.22,33,44,55 dd 10n huế\r\n 22bl10ndd10n", Xac = 790.4 },               //43
                new CalTestRequestDto { SynTaxe = "vl,bd 1234 b10n", Xac = 243.2 },                                                            //44
                new CalTestRequestDto { SynTaxe = "TP 20 kéo 30 dd10n", Xac = 167.2 },                                                         //45
                new CalTestRequestDto { SynTaxe = "TP 20-30 bl10n", Xac = 1504.8 },                                                            //46
                new CalTestRequestDto { SynTaxe = "TP 20 - 30 bl10n", Xac = 1504.8 },                                                          //47
                new CalTestRequestDto { SynTaxe = "tp 22b10n.3dai 10b10n.vl 20 b10n.2dai 30 b10n", Xac = 957.6 },                              //48
                new CalTestRequestDto { SynTaxe = "tp 22b10n.3dai mt 10b10n.vl 20 b10n.2dai 30 b10n", Xac = 957.6 },                           //49
                new CalTestRequestDto { SynTaxe = "tp 22b10n.3dai mt 10b10n.vl 20 b10n.2dai mt 30 b10n", Xac = 957.6  },                       //50
                new CalTestRequestDto { SynTaxe = "3dai 22b10n.tp 10b10n.vl 20 b10n.2dai 30 b10n", Xac = 957.6 },                              //51
                new CalTestRequestDto { SynTaxe = "3dai 20 b10n. TP 10b10n", Xac = 547.2 },                                                    //52
                new CalTestRequestDto { SynTaxe = "10 b10n 3dai.tp 20b10n. 2dai 30b10n", Xac = 820.8 },                                        //53
                new CalTestRequestDto { SynTaxe = "10 b10n 3dai.tp 20b10n. 30 b10n 2dai", Xac = 1504.8, MessageLoi = "Không hiểu :  2 dai" },  //54
                new CalTestRequestDto { SynTaxe = "10 b10n 3dai.tp 20b10n. vl 30 b10n", Xac = 684 },                                           //55
                new CalTestRequestDto { SynTaxe = "10 b10n 3dai.tp 20b10n. 30 b10n vl", Xac = 1504.8 , MessageLoi = "Không hiểu :  vl"},       //56
                new CalTestRequestDto { SynTaxe = "3dai 10 b10n .tp 20b10n. vl 30 b10n", Xac = 684 },                                          //57
                new CalTestRequestDto { SynTaxe = "TP 10 b10n .vl 20b10n.2dai 30 b10n .3 dài 40b10n mt", Xac = 957.6 },                        //58
                new CalTestRequestDto { SynTaxe = "TP 00 kéo 10 bl10n", Xac = 1504.8 },                                                        //59
                new CalTestRequestDto { SynTaxe = "00 kéo 10 đđ10n tp", Xac = 167.2 },                                                         //60
                new CalTestRequestDto { SynTaxe = "TP 00 kéo 10 đđ10n", Xac = 167.2 },                                                         //61
                new CalTestRequestDto { SynTaxe = "Dx 10,22 10n", Xac =  547.2 },                                                              //62
                new CalTestRequestDto { SynTaxe = "10,22 dx10n", Xac =  547.2 },                                                               //63
                new CalTestRequestDto { SynTaxe = "372 bl 5n 2₫\r\n 78 bao 50N 3dai. 78.46 đá 5n 2đài. 78.49 đá 5n 2đài. 78.90 đá 5n Kom Tum" +
                " với Khánh Hòa .\r\n 79.67.58 đá vòng 5N 2đài . 79 bao 20N 2đài ", Xac =  4370 },                                             //64
                new CalTestRequestDto { SynTaxe = "372 bl 5n 2₫\r\n 78 bao 50N 3dai. 78.46 đá 5n 2đài. 78.49 đá 5n 2đài. 78.90 đá 5n Kom Tum" +
                " với Khánh Hòa .\r\n 79.67.58 dau duoi 5N  hai đài . 79 bao 20N 2đài ", Xac =  3594.8 },                                      //65
                new CalTestRequestDto { SynTaxe = "372 bl 5n 2₫\r\n 78 bao 50N 3dai. 78.46 đá 5n 2đài. 78.49 đá 5n 2đài. 78.90 đá 5n Kom Tum" +
                " với Khánh Hòa .\r\n 079.167 xỉu chủ 5N  hai đài . 79 bao 20N 2đài ", Xac =  3579.6 },                                        //66
                new CalTestRequestDto { SynTaxe = "mt. 78 bao 50N 3dai. 78.46 đá 5n.2đài. 78.49 đá 5n.2đài. 78.90 đá" +                        //67
                " 5n.2đài", Xac =  2872.8 },
                new CalTestRequestDto { SynTaxe = "mt. 78 bao 50N 3dai. 78.46 đá 5n.2đài. 78.49 đá 5n.2đài. 78.90 đá" +                        //68
                " 5n.2đài mn", Xac =  2872.8 },
                new CalTestRequestDto { SynTaxe = "mt. 78 bao 50N 3dai. 78.46 đá 5n.2đài. 78.49 đá 5n.2đài mn. 78.90 đá" +                     //69
                " 5n.2đài", Xac =  2872.8 },
                new CalTestRequestDto { SynTaxe = "Đx 1n hai đài 12.75.82.37,blo 5n", Xac =  875.5 },                                               //70
                new CalTestRequestDto { SynTaxe = "Đá 33.58.75.40 đá 2n , blo 33.75.40 con 5n ", Xac =  875.5, MessageLoi = "Không hiểu : con" },    //71
                new CalTestRequestDto { SynTaxe = "Đá 33.58.75.40 đá 2n , blo 33.75.40 5n ", Xac =  533.5 },                                         //72
                new CalTestRequestDto { SynTaxe = "Đá 33.58.75.40 đá 2 , blo 33.75.40 5n ", Xac =  533.5 },                                          //73
                new CalTestRequestDto { SynTaxe = "MN. 78 bao 50N 3dai. 78.46 đá 5n.2đài. 78.49 đá 5n.2đài. " +
                                              "78.90 đá 5n. Vĩnh Long với tp", Xac =  2872.8 },                                                      //74
                new CalTestRequestDto { SynTaxe = "MN. 79.67.58 đá Vòng 5N 2đài TP với Vĩnh Long. 79 bao 20N 2đài TP với Bình Dương", Xac =  1368 }, //75
                new CalTestRequestDto { SynTaxe = "Đầu 09 , 80ng bao 13 , 30ng vinh long", Xac =  471.2 },                                            //76
                new CalTestRequestDto { SynTaxe = "MT. 70 bao 50N 1đài Kom Tum. 70.52 đá 5n.2đài 70.85 đá 5n.2đài. 70.58 đá 5n.2đài ", Xac =  1504.8 },//77
                new CalTestRequestDto { SynTaxe = "MB. 85.38.78 đá Vòng 15N", Xac =  1846.8 },                                                         //78
                new CalTestRequestDto { SynTaxe = "Đa vòng 31 , 61 , 91 2ng bao 61 , 20ng", Xac =  437.8 },                                           //79
                new CalTestRequestDto { SynTaxe = "Đa vòng 70 , 31, 19 3ng bao 7 ng Vĩnh Long", Xac =  533.5 },                                        //80
                new CalTestRequestDto { SynTaxe = "Đa thẳng 70 , 31, 19 3 ng bao 7 ng Vĩnh Long", Xac =  533.5 },                                      //81
                new CalTestRequestDto { SynTaxe = "Đa xien 70 , 31, 19 3 ng bao 7 ng Vĩnh Long", Xac =  1368, MessageLoi = "Phải đá xiên 2 đài" },     //82
                new CalTestRequestDto { SynTaxe = "Đa vòng 70 , 31, 19 3ng 2đài bao 7 ng Vĩnh Long", Xac =  779.8 },                                    //83
                new CalTestRequestDto { SynTaxe = "04 44 đá 10n dc 04 13dx 10n", Xac =  820.8 },                                                       //84
                new CalTestRequestDto { SynTaxe = "Đa thẳng 70 , 31, 19 3 ng hai đài bao 7 ng Vĩnh Long", Xac =  779.8 },                              //85
                new CalTestRequestDto { SynTaxe = "Đa xiên 70 , 31, 19 3ng 2đài bao 7 ng Vĩnh Long", Xac =  779.8 },                                   //86
                new CalTestRequestDto { SynTaxe = "76đau 50n Miền trung 3đài 576xc30n Khánh Hòa Kom Tum  538xc30n 38.58 bl30n", Xac =  1938 },         //87
                new CalTestRequestDto { SynTaxe = "Miền trung 3đài 76đau 50n 576xc30n Khánh Hòa Kom Tum 538xc30n 38.58 bl30n", Xac =  1983.6 },        //88
                new CalTestRequestDto { SynTaxe = "76đau 50n Miền trung 3đài 576xc30n kh kt 38.58b30n.538xc30n", Xac =  1938 },                        //89
                new CalTestRequestDto { SynTaxe = "Miền trung 3đài 76đau 50n 576xc30n kh kt 538xc30n 38.58 bl30n", Xac =  1983.6 },                    //90
                new CalTestRequestDto { SynTaxe = "234 bd 10 n", Xac =  775.2 },                                                                      //91
                new CalTestRequestDto { SynTaxe = "234 247 daolo 10 n", Xac =  1550.4 },                                                               //92
                new CalTestRequestDto { SynTaxe = "234.247bao đảo 10n 576xc40n", Xac =  1611.2 },                                                      //93
                new CalTestRequestDto { SynTaxe = "Mb 34.43.38.83.33.73bl100n 963xc250n 234.247bao đảo 10n 576xc40n 43.41bl30n 43.41đa20n 27.72.24.42 đa10n 16.61" +
                " .35.53đv10n 41.16.61đa10n 5555bl10n 43bl50n 779xc20n 963xc50n 168xc20n", Xac =  24950.8 },                                            //94
                new CalTestRequestDto { SynTaxe = "722b10nxc đuôi đảo 20 mb", Xac = 448.4 },                                                            //95
                new CalTestRequestDto { SynTaxe = "722b10nxc đầu đảo 20 hn", Xac = 266 },                                                             //96
                new CalTestRequestDto { SynTaxe = "722b10nxc đảo 20 hn", Xac = 539.6 },                                                                 //97
                new CalTestRequestDto { SynTaxe = "722b10đuôi đảo 20 hn", Xac = 448.4 },                                                                //98
                new CalTestRequestDto { SynTaxe = "722 455 b10đuôi đảo 20 hn", Xac = 896.8 },                                                           //99
                new CalTestRequestDto { SynTaxe = "722 455 b10đầu đảo 20 hn", Xac = 532 },                                                              //100
                new CalTestRequestDto { SynTaxe = "722 455 b10 xỉu chủ đảo 20 hn", Xac = 1079.2 },                                                       //101
                new CalTestRequestDto { SynTaxe = "722 455 b10nxc đuôi 20 mb", Xac = 440.8 },                                                           //102
                new CalTestRequestDto { SynTaxe = "722 455 b10nxc đầu 20 hn", Xac = 380 },                                                              //103
                new CalTestRequestDto { SynTaxe = "722 455b10nxc đảo 20 hn", Xac = 1079.2 },                                                            //104
                new CalTestRequestDto { SynTaxe = "722 45 b10nxc đầu 20 hn", Xac = 220.4, MessageLoi = "722 : Không thể đánh lô" },                     //105
                new CalTestRequestDto { SynTaxe = "22 55b10nxc đảo 20 hn", Xac = 220.4 , MessageLoi = "22,55 : Không thể đánh 3 số"},                   //106
                new CalTestRequestDto { SynTaxe = "722b10nxc đầu đảo 20 mt", Xac = 220.4 },                                                             //107
                new CalTestRequestDto { SynTaxe = "722b10nxc đảo 20 mn", Xac = 311.6 },                                                                 //108
                new CalTestRequestDto { SynTaxe = "722b10đuôi đảo 20 mn", Xac = 220.4 },                                                                //109
                new CalTestRequestDto { SynTaxe = "mn 00keo10 bao 10n", Xac = 1504.8 },                                                                 //110
                new CalTestRequestDto { SynTaxe = "hai đài MIỀN nam bao 76 10 ngìn", Xac = 273.6 },                                                    //111
                new CalTestRequestDto { SynTaxe = "2 dai15.51dax5n 3dai 515xcdao30n", Xac = 273.6 },                                                   //112
                
            };
            Constants.IstestMode = true;
            var arr = teststos.ToArray();

            for (int i = 0; i < arr.Length;i++)
            {
                if(i ==112)
                {

                }
                Cal2RequestDto dto = new Cal2RequestDto
                {
                    SynTaxes = new List<string> { arr[i].SynTaxe },
                    TileXac = 0.76,
                    TileThuong = 76,
                    TileBaso = 650
                };
                var re = (Cal2ResponseDto)_calcualationService.Cal2Request(dto).Data;
                if (re.Xac == arr[i].Xac || (re.MessageLoi.Any() && !string.IsNullOrEmpty(re.MessageLoi[0]) && re.MessageLoi[0] == arr[i].MessageLoi))
                    result.Add($"Test {i} : PASS");
                else
                    result.Add($"Test {i} : FAIL");
            }

            response.Data = result;
            Constants.IstestMode = false;
            return response;
        }
        public ResponseBase GetChanel(DateTime? date)
        {
            ResponseBase response = new ResponseBase();
            var con = CommonFunction.GetChanels(date);
            response.Data = con;
            return response;
        }

        public ResponseBase ReadLogs(DateTime? date)
        {
            ResponseBase response = new ResponseBase();
            DateTime da = date == null ? DateTime.Now : (DateTime)date;
            var fullPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + $"\\Logs\\{da.ToString("ddMMyyyy")}.txt";
            if (File.Exists(fullPath))
            {
                response.Data = File.ReadAllLines(fullPath);
            }
            return response;
        }
    }
}