using KQ.Common.Constants;
using KQ.Common.Extention;
using KQ.Common.Helpers;
using KQ.Data.Base;
using KQ.DataDto.Calculation;
using KQ.Services.Calcualation;

namespace KQ.Services.CommonServices
{
    public class CommonService : ICommonService
    {
        private readonly ICalcualationService _calcualationService;
        public CommonService(ICalcualationService calcualationService)
        {
            _calcualationService = calcualationService;
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
                    
                response.Data = data;
                return response;
            }
        }
        public ResponseBase CheckChanelCode()
        {
            ResponseBase response = new ResponseBase();
            lock (InnitRepository._chanelCode)
            {
                response.Data = InnitRepository._chanelCode;
                return response;
            }
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
                
            };
            Constants.IstestMode = true;
            var arr = teststos.ToArray();

            for (int i = 0; i < arr.Length;i++)
            {
                if(i ==91)
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
    }
}