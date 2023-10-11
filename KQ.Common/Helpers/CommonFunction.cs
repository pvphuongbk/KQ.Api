﻿using KQ.Common.Extention;
using KQ.DataDto.Common;
using System.Collections.Concurrent;

namespace KQ.Common.Helpers
{
    public class CommonFunction
    {
        public static ConcurrentDictionary<int, List<string>> GetCurrentChanelCode()
        {
            ConcurrentDictionary<int, List<string>> dic = new ConcurrentDictionary<int, List<string>>();
            var allChanel = GetChanels(null);
            foreach (var gChanel in allChanel)
            {
                foreach (var chanel in gChanel.value)
                {
                    if (chanel.Value == "TPHCM")
                    {
                        dic.TryAdd(chanel.Key, new List<string>
                        {
                            "TPHCM",
                            "TP",
                            "tphcm",
                            "tp",
                            "hcm"
                        });
                    }
                    else
                    {
                        var ch = chanel.Value.RemoveUnicode();
                        var notLower = ch;
                        ch = ch.ToLower();
                        dic.TryAdd(chanel.Key, new List<string>
                        {
                            chanel.Value,
                            notLower,
                            ch,
                            ch.GetFirstChar(),
                            ch.GetFirstCharOnlyFisrt(),
                            ch.Replace(" ",string.Empty),
                            ch.GetFirstCharOnlySecond()
                        });
                    }
                }
            }

            return dic;
        }
        public static GroupChanel[] GetChanels(DateTime? day)
        {
            DayOfWeek mnDay = default;
            DayOfWeek mtDay = default;
            if (day == null)
            {
                var now = DateTime.Now;
                if (now.TimeOfDay >= new TimeSpan(16, 15, 00))
                    mnDay = now.DayOfWeek;
                else
                    mnDay = now.AddDays(-1).DayOfWeek;
                if (now.TimeOfDay >= new TimeSpan(17, 15, 00))
                    mtDay = now.DayOfWeek;
                else
                    mtDay = now.AddDays(-1).DayOfWeek;
            }
            else
            {
                mnDay = ((DateTime)day).DayOfWeek;
                mtDay = ((DateTime)day).DayOfWeek;
            }

            return new GroupChanel[]
            {
                new GroupChanel
                {
                    Key = "Miền Nam",
                    value = GetNorthChanels(mnDay)
                },
                new GroupChanel
                {
                    Key = "Miền Trung",
                    value = GetCenterChanels(mtDay)
                },
                new GroupChanel
                {
                    Key = "Miền Bắc",
                    value = new ChanelDto[] { new ChanelDto {Key = 8, Value = "Miền bắc" } }
                },
            };
        }
        public static ChanelDto[] GetNorthChanels(DayOfWeek day)
        {
            if (day == DayOfWeek.Monday)
                return new ChanelDto[]
                {
                    new ChanelDto { Key = 1, Value = "TPHCM" },
                    new ChanelDto { Key = 2, Value = "Đồng Tháp" },
                    new ChanelDto { Key = 3, Value = "Cà Mau" },
                };
            if (day == DayOfWeek.Tuesday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 1, Value = "Bến Tre" },
                            new ChanelDto { Key = 2, Value = "Vũng Tàu" },
                            new ChanelDto { Key = 3, Value = "Bạc Liêu" }
                };
            if (day == DayOfWeek.Wednesday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 1, Value = "Đồng Nai" },
                            new ChanelDto { Key = 2, Value = "Cần Thơ" },
                            new ChanelDto { Key = 3, Value = "Sóc Trăng" },
                };
            if (day == DayOfWeek.Thursday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 1, Value = "Tây Ninh" },
                            new ChanelDto { Key = 2, Value = "An Giang" },
                            new ChanelDto { Key = 3, Value = "Bình Thuận" },
                };
            if (day == DayOfWeek.Friday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 1, Value = "Vĩnh Long" },
                            new ChanelDto { Key = 2, Value = "Bình Dương" },
                            new ChanelDto { Key = 3, Value = "Trà Vinh" },
                };
            if (day == DayOfWeek.Saturday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 1, Value = "TPHCM" },
                            new ChanelDto { Key = 2, Value = "Long An" },
                            new ChanelDto { Key = 3, Value = "Bình Phước" },
                            new ChanelDto { Key = 4, Value = "Hậu Giang" }
                };
            return new ChanelDto[]
                {
                            new ChanelDto { Key = 1, Value = "Tiền Giang" },
                            new ChanelDto { Key = 2, Value = "Kiên Giang" },
                            new ChanelDto { Key = 3, Value = "Đà Lạt" }
                };
        }
        public static ChanelDto[] GetCenterChanels(DayOfWeek day)
        {
            if (day == DayOfWeek.Monday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 5, Value = "Phú Yên" },
                            new ChanelDto { Key = 6, Value = "Huế" },
                };
            if (day == DayOfWeek.Tuesday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 5, Value = "Đắk Lắk" },
                            new ChanelDto { Key = 6, Value = "Quảng Nam" }
                };
            if (day == DayOfWeek.Wednesday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 5, Value = "Đà Nẵng" },
                            new ChanelDto { Key = 6, Value = "Khánh Hòa" },
                };
            if (day == DayOfWeek.Thursday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 5, Value = "Quảng Bình" },
                            new ChanelDto { Key = 6, Value = "Bình Định" },
                            new ChanelDto { Key = 7, Value = "Quảng Trị" }
                };
            if (day == DayOfWeek.Friday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 5, Value = "Gia Lai" },
                            new ChanelDto { Key = 6, Value = "Ninh Thuận" }
                };
            if (day == DayOfWeek.Saturday)
                return new ChanelDto[]
                {
                            new ChanelDto { Key = 5, Value = "Đà Nẵng" },
                            new ChanelDto { Key = 6, Value = "Quãng Ngãi" },
                            new ChanelDto { Key = 7, Value = "Đắk Nông" },
                };
            return new ChanelDto[]
                {
                            new ChanelDto { Key = 5, Value = "Khánh Hòa" },
                            new ChanelDto { Key = 6, Value = "Kon Tum" },
                            new ChanelDto { Key = 7, Value = "Huế" }
                };
        }
    }
}
