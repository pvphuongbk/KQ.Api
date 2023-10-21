﻿using KQ.Common.Enums;
using KQ.Common.Extention;
using KQ.Common.Helpers;
using KQ.Data.Base;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Interface;
using KQ.DataDto.Calculation;
using KQ.DataDto.Enum;
using Newtonsoft.Json;
using System.Diagnostics;

namespace KQ.Services.Calcualation
{
    public class Calcualation2Service : ICalcualation2Service
    {
        private readonly ICommonRepository<StoreKQ> _storeKQRepository;
        private readonly ICommonRepository<Details> _detailsRepository;
        private readonly ICommonUoW _commonUoW;
        public Calcualation2Service(ICommonRepository<StoreKQ> storeKQRepository, ICommonUoW commonUoW, ICommonRepository<Details> detailsRepository)
        {
            _storeKQRepository = storeKQRepository;
            _commonUoW = commonUoW;
            _detailsRepository = detailsRepository;
        }

        public ResponseBase Cal3Request(Cal3RequestDto dtos)
        {
            try
            {
                Stopwatch s1 = new Stopwatch();
                s1.Start();
                var array = ChuanHoa(dtos.SynTax);
                int cursor = 0;
                bool isStart = true;
                List<int> chanels = new List<int>();
                List<int> numbers = new List<int>();
                List<string> numberStrs = new List<string>();
                List<Cal3PrepareDto> cal3PrepareDtos = new List<Cal3PrepareDto>();
                int numTemp = 0;
                bool isDau = false;
                CachChoi? cachChoi = null;
                List<CachChoi?> cachChoiTemp = new List<CachChoi?>();
                if (dtos.Mien == MienEnum.MB)
                {
                    chanels.Add(8);
                    isStart = false;
                }
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == " " || (dtos.Mien == MienEnum.MB && (array[i] == "hn" || array[i] == "mb")))
                        continue;

                    if (isStart)
                    {
                        var (check, mess) = GetChanelsStart(dtos.CreatedDate, ref chanels, dtos.Mien, array[i], array, ref i);
                        isStart = false;
                        if (check)
                        {
                            cursor = i;
                        }
                        else
                        {
                            // lỗi
                        }
                    }
                    else if (int.TryParse(array[i], out numTemp))
                    {
                        if (array[i].Length == 1)
                        {
                            List<int> chanelsTemp = new List<int>();
                            var (check, mess) = GetChanelsStart(dtos.CreatedDate, ref chanelsTemp, dtos.Mien, array[i], array, ref i);
                            if (check)
                            {
                                cal3PrepareDtos.ForEach(x => x.Chanels = chanels.CloneList());
                                chanels = chanelsTemp;
                                numbers.Clear();
                                numberStrs.Clear();
                                cachChoi = null;
                            }
                            else
                            {
                                // lỗi
                            }
                        }
                        else
                        {
                            numbers.Clear();
                            numberStrs.Clear();
                            numberStrs.Add(array[i]);
                            numbers.Add(numTemp);
                            var stri = array[i];
                            var (str, iTemp) = FindNextStr(array, i);
                            if (str == "k" || str == "keo" || str == "khc" || str == "kht")
                            {
                                i = iTemp;
                                HandlerKeo(str, stri, numTemp, array, ref i, ref numbers, ref numberStrs);
                            }
                            else
                            {
                                while (int.TryParse(str, out numTemp))
                                {
                                    numbers.Add(numTemp);
                                    numberStrs.Add(str);
                                    i = iTemp;
                                    (str, iTemp) = FindNextStr(array, i);
                                }
                            }

                            if (numberStrs.Any(o => o.Length != numberStrs[0].Length))
                            {
                                // Các số chơi phải cùng 2 hoặc 3 hoặc 4 con
                            }
                        }
                    }
                    else if (GetCachChoi(array[i], ref cachChoi, array, ref i, ref cal3PrepareDtos, ref cachChoiTemp, chanels, numbers, numberStrs))
                    {
                        cachChoiTemp.Add(cachChoi);
                        var sl = GetSl(array, ref i);
                        if (sl > 0)
                        {
                            var (check1, mess1) = CheckAndCreateItem(ref cal3PrepareDtos, cachChoi, chanels, numbers, numberStrs, sl);
                            if (!check1)
                            {
                                //lỗi
                            }
                            cachChoi = null;
                            sl = 0;
                            bool check = true;
                            while (check)
                            {
                                var (str, iTemp) = FindNextStr(array, i);
                                if (str.StartsWith("n"))
                                    str = str.Substring(1, str.Length - 1);
                                if (GetCachChoi(str, ref cachChoi, array, ref i, ref cal3PrepareDtos, ref cachChoiTemp, chanels, numbers, numberStrs))
                                {
                                    cachChoiTemp.Add(cachChoi);
                                    if (cachChoiTemp.Distinct().Count() != cachChoiTemp.Count)
                                    {
                                        // lỗi trùng cách chơi
                                        break;
                                    }
                                    i = iTemp;
                                    var sl2 = GetSl(array, ref i);
                                    if (sl2 > 0)
                                    {
                                        var (check2, mess2) = CheckAndCreateItem(ref cal3PrepareDtos, cachChoi, chanels, numbers, numberStrs, sl2);
                                        if (!check2)
                                        {
                                            //lỗi
                                        }
                                        cachChoi = null;
                                    }
                                    else
                                    {
                                        check = false;
                                    }
                                }
                                else
                                {
                                    check = false;
                                }
                            }
                        }
                        else
                        {
                            // lỗi
                        }
                        cachChoiTemp.Clear();
                    }
                    else
                    {
                        List<int> chanelsTemp = new List<int>();
                        var (check, mess) = GetChanelsStart(dtos.CreatedDate, ref chanelsTemp, dtos.Mien, array[i], array, ref i);
                        if (check)
                        {
                            cal3PrepareDtos.ForEach(x => x.Chanels = chanels.CloneList());
                            chanels = chanelsTemp;
                            numbers.Clear();
                            numberStrs.Clear();
                            cachChoi = null;
                        }
                        else
                        {
                            // lỗi
                            break;
                        }
                    }
                }

                var detail = CreateDetail(dtos, cal3PrepareDtos);
                var isUpdate = UpdateTrungThuong(dtos, ref detail);
                if (isUpdate)
                {
                    UpdateSumTrungThuong(ref detail);
                }

                if (dtos.IsSave)
                {
                    _commonUoW.BeginTransaction();
                    var json = JsonConvert.SerializeObject(detail);
                    Details de = new Details
                    {
                        CreatedDate = dtos.CreatedDate,
                        IDKhach = dtos.IDKhach,
                        Detail = json
                    };
                    _detailsRepository.Insert(de);
                }
                s1.Stop();
                if (dtos.IsSave)
                    _commonUoW.Commit();
                return new ResponseBase { Data = detail };
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateTrungThuong");
                if (dtos.IsSave)
                    _commonUoW.RollBack();
                return new ResponseBase { Code = 501, Message = ex.Message };
            }
        }
        public void UpdateSumTrungThuong(ref Cal3DetailDto detail)
        {
            detail.Trung.HaiCB = detail.Details.Where(x => x.CachChoi == CachChoi.B && x.SlTrung > 0).Sum(x => x.SoTien * x.SlTrung);
            detail.Trung.HaiCD = detail.Details.Where(x => (x.CachChoi == CachChoi.Dau
                || x.CachChoi == CachChoi.Duoi || x.CachChoi == CachChoi.DD)).Sum(x => x.SoTien * x.SlTrung);
            detail.Trung.DaT = detail.Details.Where(x => x.CachChoi == CachChoi.Da && x.SlTrung > 0).Sum(x => x.SoTien * x.SlTrung);
            detail.Trung.DaX = detail.Details.Where(x => x.CachChoi == CachChoi.DaX && x.SlTrung > 0).Sum(x => x.SoTien * x.SlTrung);
            detail.Trung.BaCon = detail.Details.Where(x => (x.CachChoi == CachChoi.Xc || x.CachChoi == CachChoi.XcDau || x.CachChoi == CachChoi.XcDui || x.CachChoi == CachChoi.BaoBaCon
                        || x.CachChoi == CachChoi.XcDao || x.CachChoi == CachChoi.XcDauDao || x.CachChoi == CachChoi.XcDuoiDao || x.CachChoi == CachChoi.BaoDao))
                        .Sum(x => x.SoTien * x.SlTrung);
            detail.Trung.BonCon = detail.Details.Where(x => (x.CachChoi == CachChoi.BaoBonCon || x.CachChoi == CachChoi.BonConDao) && x.SlTrung > 0)
                    .Sum(x => x.SoTien * x.SlTrung);
        }
        public bool UpdateTrungThuong(Cal3RequestDto dtos, ref Cal3DetailDto detail)
        {
            Dictionary<string, int> meT2c = new Dictionary<string, int>();
            Dictionary<string, int> meDD = new Dictionary<string, int>();
            Dictionary<string, int> meDaT = new Dictionary<string, int>();
            Dictionary<string, int> meDaX = new Dictionary<string, int>();
            Dictionary<string, int> me3Con = new Dictionary<string, int>();
            Dictionary<string, int> me4Con = new Dictionary<string, int>();

            var kq2So = new List<int>[8];
            var kq3So = new List<int>[8];
            var kq4So = new List<int>[8];
            if (dtos.CreatedDate.Date != DateTime.Now.Date)
            {
                var kq = _storeKQRepository.FindAll(x => x.CreatedDate.Date == dtos.CreatedDate.Date).FirstOrDefault();
                if (kq != null)
                {
                    kq2So = JsonConvert.DeserializeObject<List<int>[]>(kq.HaiCon);
                    kq3So = JsonConvert.DeserializeObject<List<int>[]>(kq.BaCon);
                    kq4So = JsonConvert.DeserializeObject<List<int>[]>(kq.BonCon);
                }
            }
            else
            {
                // Cap nhat gia tri hom nay
            }
            if (kq2So == null || kq2So.All(x => x.Count == 0) || kq3So == null || kq3So.All(x => x.Count == 0)
                || kq4So == null || kq4So.All(x => x.Count == 0))
                return false;
            foreach (var pre in detail.Details)
            {
                switch (pre.CachChoi)
                {
                    case CachChoi.B:
                        pre.SlTrung = kq2So[pre.DaiIn[0] - 1].Count(x => x == pre.SoIn[0]);
                        if (pre.SlTrung > 0)
                        {
                            string key = pre.SoIn[0].ToString("00");
                            if (meT2c.ContainsKey(key))
                                meT2c[key] += pre.SlTrung * pre.SoTien;
                            else
                                meT2c.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.Da:
                        var count1 = kq2So[pre.DaiIn[0] - 1].Count(x => x == pre.SoIn[0]);
                        var count2 = kq2So[pre.DaiIn[0] - 1].Count(x => x == pre.SoIn[1]);
                        var count = count1 < count2 ? count1 : count2;
                        pre.SlTrung = count;
                        pre.SlTrung = kq2So[pre.DaiIn[0] - 1].Count(x => x == pre.SoIn[0]);
                        if (pre.SlTrung > 0)
                        {
                            string key = $"{pre.SoIn[0].ToString("00")} {pre.SoIn[1].ToString("00")}";
                            if (meDaT.ContainsKey(key))
                                meDaT[key] += pre.SlTrung * pre.SoTien;
                            else
                                meDaT.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.DaX:
                        var kqx = new List<int>();
                        kqx.AddRange(kq2So[pre.DaiIn[0] - 1]);
                        kqx.AddRange(kq2So[pre.DaiIn[1] - 1]);

                        var count1x = kqx.Count(x => x == pre.SoIn[0]);
                        var count2x = kqx.Count(x => x == pre.SoIn[1]);
                        var countx = count1x < count2x ? count1x : count2x;
                        pre.SlTrung = countx;
                        if (pre.SlTrung > 0)
                        {
                            string key = $"{pre.SoIn[0].ToString("00")} {pre.SoIn[1].ToString("00")}";
                            if (meDaX.ContainsKey(key))
                                meDaX[key] += pre.SlTrung * pre.SoTien;
                            else
                                meDaX.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.Dau:
                        if (dtos.Mien == MienEnum.MB)
                        {
                            var kqd = kq2So[pre.DaiIn[0] - 1].GetRange(23, 4);
                            var countd = kqd.Count(x => x == pre.SoIn[0]);
                            pre.SlTrung = countd;
                        }
                        else
                        {
                            if (kq2So[pre.DaiIn[0] - 1].First() == pre.SoIn[0])
                                pre.SlTrung = 1;
                        }
                        if (pre.SlTrung > 0)
                        {
                            string key = pre.SoIn[0].ToString("00");
                            if (meDD.ContainsKey(key))
                                meDD[key] += pre.SlTrung * pre.SoTien;
                            else
                                meDD.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.Duoi:
                        if (dtos.Mien == MienEnum.MB)
                        {
                            if (kq2So[pre.DaiIn[0] - 1].First() == pre.SoIn[0])
                                pre.SlTrung = 1;
                        }
                        else
                        {
                            if (kq2So[pre.DaiIn[0] - 1].Last() == pre.SoIn[0])
                                pre.SlTrung = 1;
                        }
                        if (pre.SlTrung > 0)
                        {
                            string key = pre.SoIn[0].ToString("00");
                            if (meDD.ContainsKey(key))
                                meDD[key] += pre.SlTrung * pre.SoTien;
                            else
                                meDD.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.DD:
                        List<int> kqdd = new List<int>();
                        if (dtos.Mien == MienEnum.MB)
                        {
                            kqdd.Add(kq2So[pre.DaiIn[0] - 1].First());
                            kqdd.AddRange(kq2So[pre.DaiIn[0] - 1].GetRange(23, 4));
                        }
                        else
                        {
                            kqdd.Add(kq2So[pre.DaiIn[0] - 1].First());
                            kqdd.Add(kq2So[pre.DaiIn[0] - 1].Last());
                        }
                        pre.SlTrung = kqdd.Count(x => x == pre.SoIn[0]);
                        if (pre.SlTrung > 0)
                        {
                            string key = pre.SoIn[0].ToString("00");
                            if (meDD.ContainsKey(key))
                                meDD[key] += pre.SlTrung * pre.SoTien;
                            else
                                meDD.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.BaoBaCon:
                        pre.SlTrung = kq3So[pre.DaiIn[0] - 1].Count(x => x == pre.SoIn[0]);
                        if (pre.SlTrung > 0)
                        {
                            string key = pre.SoIn[0].ToString("000");
                            if (me3Con.ContainsKey(key))
                                me3Con[key] += pre.SlTrung * pre.SoTien;
                            else
                                me3Con.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.BaoDao:
                        pre.SlTrung = kq3So[pre.DaiIn[0] - 1].Count(x => x == pre.SoIn[0]);
                        if (pre.SlTrung > 0)
                        {
                            string key = pre.SoIn[0].ToString("000");
                            if (me3Con.ContainsKey(key))
                                me3Con[key] += pre.SlTrung * pre.SoTien;
                            else
                                me3Con.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.XcDau:
                    case CachChoi.XcDauDao:
                        if (dtos.Mien == MienEnum.MB)
                        {
                            var kqd = kq3So[pre.DaiIn[0] - 1].GetRange(20, 3);
                            var countd = kqd.Count(x => x == pre.SoIn[0]);
                            pre.SlTrung = countd;
                        }
                        else
                        {
                            if (kq3So[pre.DaiIn[0] - 1].First() == pre.SoIn[0])
                                pre.SlTrung = 1;
                        }
                        if (pre.SlTrung > 0)
                        {
                            string key = pre.SoIn[0].ToString("000");
                            if (me3Con.ContainsKey(key))
                                me3Con[key] += pre.SlTrung * pre.SoTien;
                            else
                                me3Con.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.XcDuoiDao:
                    case CachChoi.XcDui:
                        if (dtos.Mien == MienEnum.MB)
                        {
                            if (kq3So[pre.DaiIn[0] - 1].First() == pre.SoIn[0])
                                pre.SlTrung = 1;
                        }
                        else
                        {
                            if (kq3So[pre.DaiIn[0] - 1].Last() == pre.SoIn[0])
                                pre.SlTrung = 1;
                        }
                        if (pre.SlTrung > 0)
                        {
                            string key = pre.SoIn[0].ToString("000");
                            if (me3Con.ContainsKey(key))
                                me3Con[key] += pre.SlTrung * pre.SoTien;
                            else
                                me3Con.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.XcDao:
                    case CachChoi.Xc:
                        List<int> kqxc = new List<int>();
                        if (dtos.Mien == MienEnum.MB)
                        {
                            kqxc.Add(kq3So[pre.DaiIn[0] - 1].First());
                            kqxc.AddRange(kq2So[pre.DaiIn[0] - 1].GetRange(20, 3));
                        }
                        else
                        {
                            kqxc.Add(kq3So[pre.DaiIn[0] - 1].First());
                            kqxc.Add(kq3So[pre.DaiIn[0] - 1].Last());
                        }
                        pre.SlTrung = kqxc.Count(x => x == pre.SoIn[0]);
                        if (pre.SlTrung > 0)
                        {
                            string key = pre.SoIn[0].ToString("000");
                            if (me3Con.ContainsKey(key))
                                me3Con[key] += pre.SlTrung * pre.SoTien;
                            else
                                me3Con.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                    case CachChoi.BonConDao:
                    case CachChoi.BaoBonCon:
                        pre.SlTrung = kq4So[pre.DaiIn[0] - 1].Count(x => x == pre.SoIn[0]);
                        if (pre.SlTrung > 0)
                        {
                            string key = pre.SoIn[0].ToString("0000");
                            if (me4Con.ContainsKey(key))
                                me4Con[key] += pre.SlTrung * pre.SoTien;
                            else
                                me4Con.Add(key, pre.SlTrung * pre.SoTien);
                        }
                        break;
                }
            }

            if(meT2c.Any())
            {
                var mes = new List<string>();
                foreach (var m in meT2c)
                    mes.Add($"{m.Key}({m.Value}n)");
                detail.TrungDetail.Add($"T2C: {string.Join(",", mes)}");
            }
            if (meDD.Any())
            {
                var mes = new List<string>();
                foreach (var m in meDD)
                    mes.Add($"{m.Key}({m.Value}n)");
                detail.TrungDetail.Add($"2CĐ: {string.Join(",", mes)}");
            }
            if (meDaT.Any())
            {
                var mes = new List<string>();
                foreach (var m in meDaT)
                    mes.Add($"{m.Key}({m.Value}n)");
                detail.TrungDetail.Add($"ĐáT: {string.Join(",", mes)}");
            }
            if (meDaX.Any())
            {
                var mes = new List<string>();
                foreach (var m in meDaX)
                    mes.Add($"{m.Key}({m.Value}n)");
                detail.TrungDetail.Add($"ĐáX: {string.Join(",", mes)}");
            }
            if (me3Con.Any())
            {
                var mes = new List<string>();
                foreach (var m in me3Con)
                    mes.Add($"{m.Key}({m.Value}n)");
                detail.TrungDetail.Add($"3C: {string.Join(",", mes)}");
            }
            if (me4Con.Any())
            {
                var mes = new List<string>();
                foreach (var m in me4Con)
                    mes.Add($"{m.Key}({m.Value}n)");
                detail.TrungDetail.Add($"4C: {string.Join(",", mes)}");
            }
            detail.IsTinh = true;
            return true;

        }

        public Cal3DetailDto CreateDetail(Cal3RequestDto dtos, List<Cal3PrepareDto> cal3PrepareDtos)
        {
            Cal3DetailDto detail = new Cal3DetailDto
            {
                Details = new List<Detail>(),
                Trung = new Summary(),
                TrungDetail = new List<string>(),
                Xac = new Summary()
            };
            var slDai = InnitRepository._chanelCodeAll[dtos.CreatedDate.DayOfWeek][dtos.Mien];
            int sl2Con = dtos.Mien == MienEnum.MB ? 27 : 18;
            int sl2Dau = dtos.Mien == MienEnum.MB ? 4 : 1;
            int sl3Con = dtos.Mien == MienEnum.MB ? 23 : 17;
            int sl3Dau = dtos.Mien == MienEnum.MB ? 3 : 1;
            int sl4Con = dtos.Mien == MienEnum.MB ? 20 : 16;
            foreach (var pre in cal3PrepareDtos)
            {
                switch (pre.CachChoi)
                {
                    case CachChoi.B:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            for (int i = 0; i < pre.Numbers.Count; i++)
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.B,
                                    DaiIn = new List<int> { dai },
                                    Dai = daiStr,
                                    So = new List<string> { pre.NumbersStr[i] },
                                    SoIn = new List<int> { pre.Numbers[i] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.HaiCB += sl2Con * pre.Sl;
                            }
                        }
                        break;
                    case CachChoi.Da:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            foreach (var ns in pre.Numbers.Take2InList())
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.Da,
                                    DaiIn = new List<int> { dai },
                                    Dai = daiStr,
                                    So = new List<string> { pre.NumbersStr[ns.First()], pre.NumbersStr[ns.Last()] },
                                    SoIn = new List<int> { pre.Numbers[ns.First()], pre.Numbers[ns.Last()] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.DaT += sl2Con * pre.Sl * 2;
                            }
                        }
                        break;
                    case CachChoi.DaX:
                        foreach (var dai in pre.Chanels.Take2InList())
                        {
                            var daiStr1 = slDai[pre.Chanels[dai.First()]][3];
                            var daiStr2 = slDai[pre.Chanels[dai.Last()]][3];
                            foreach (var ns in pre.Numbers.Take2InList())
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.DaX,
                                    DaiIn = new List<int> { pre.Chanels[dai.First()], pre.Chanels[dai.Last()] },
                                    Dai = $"{daiStr1} {daiStr2}",
                                    So = new List<string> { pre.NumbersStr[ns.First()], pre.NumbersStr[ns.Last()] },
                                    SoIn = new List<int> { pre.Numbers[ns.First()], pre.Numbers[ns.Last()] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.DaX += sl2Con * pre.Sl * 4;
                            }
                        }
                        break;
                    case CachChoi.Dau:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            for (int i = 0; i < pre.Numbers.Count; i++)
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.Dau,
                                    DaiIn = new List<int> { dai },
                                    Dai = daiStr,
                                    So = new List<string> { pre.NumbersStr[i] },
                                    SoIn = new List<int> { pre.Numbers[i] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.HaiCD += sl2Dau * pre.Sl;
                            }
                        }
                        break;
                    case CachChoi.Duoi:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            for (int i = 0; i < pre.Numbers.Count; i++)
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.Duoi,
                                    DaiIn = new List<int> { dai },
                                    Dai = daiStr,
                                    So = new List<string> { pre.NumbersStr[i] },
                                    SoIn = new List<int> { pre.Numbers[i] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.HaiCD += pre.Sl;
                            }
                        }
                        break;
                    case CachChoi.DD:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            for (int i = 0; i < pre.Numbers.Count; i++)
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.DD,
                                    DaiIn = new List<int> { dai },
                                    Dai = daiStr,
                                    So = new List<string> { pre.NumbersStr[i] },
                                    SoIn = new List<int> { pre.Numbers[i] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.HaiCD += (sl2Dau + 1) * pre.Sl;
                            }
                        }
                        break;
                    case CachChoi.BaoBaCon:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            for (int i = 0; i < pre.Numbers.Count; i++)
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.BaoBaCon,
                                    DaiIn = new List<int> { dai },
                                    Dai = daiStr,
                                    So = new List<string> { pre.NumbersStr[i] },
                                    SoIn = new List<int> { pre.Numbers[i] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.BaCon += sl3Con * pre.Sl;
                            }
                        }
                        break;
                    case CachChoi.BaoDao:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            foreach (var numStr in pre.NumbersStr)
                            {
                                foreach (var dao in numStr.BaSoToBaoDao())
                                {
                                    detail.Details.Add(new Detail
                                    {
                                        CachChoi = CachChoi.BaoDao,
                                        DaiIn = new List<int> { dai },
                                        Dai = daiStr,
                                        So = new List<string> { dao },
                                        SoIn = new List<int> { int.Parse(dao) },
                                        SoTien = pre.Sl
                                    });
                                    detail.Xac.BaCon += sl3Con * pre.Sl;
                                }
                            }
                        }
                        break;
                    case CachChoi.XcDau:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            for (int i = 0; i < pre.Numbers.Count; i++)
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.XcDau,
                                    DaiIn = new List<int> { dai },
                                    Dai = daiStr,
                                    So = new List<string> { pre.NumbersStr[i] },
                                    SoIn = new List<int> { pre.Numbers[i] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.BaCon += sl3Dau * pre.Sl;
                            }
                        }
                        break;
                    case CachChoi.XcDui:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            for (int i = 0; i < pre.Numbers.Count; i++)
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.XcDui,
                                    DaiIn = new List<int> { dai },
                                    Dai = daiStr,
                                    So = new List<string> { pre.NumbersStr[i] },
                                    SoIn = new List<int> { pre.Numbers[i] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.BaCon += pre.Sl;
                            }
                        }
                        break;
                    case CachChoi.Xc:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            for (int i = 0; i < pre.Numbers.Count; i++)
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.Xc,
                                    DaiIn = new List<int> { dai },
                                    Dai = daiStr,
                                    So = new List<string> { pre.NumbersStr[i] },
                                    SoIn = new List<int> { pre.Numbers[i] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.BaCon += (sl3Dau + 1) * pre.Sl;
                            }
                        }
                        break;
                    case CachChoi.XcDauDao:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            foreach (var numStr in pre.NumbersStr)
                            {
                                foreach (var dao in numStr.BaSoToBaoDao())
                                {
                                    detail.Details.Add(new Detail
                                    {
                                        CachChoi = CachChoi.XcDauDao,
                                        DaiIn = new List<int> { dai },
                                        Dai = daiStr,
                                        So = new List<string> { dao },
                                        SoIn = new List<int> { int.Parse(dao) },
                                        SoTien = pre.Sl
                                    });
                                    detail.Xac.BaCon += sl3Dau * pre.Sl;
                                }
                            }
                        }
                        break;
                    case CachChoi.XcDuoiDao:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            foreach (var numStr in pre.NumbersStr)
                            {
                                foreach (var dao in numStr.BaSoToBaoDao())
                                {
                                    detail.Details.Add(new Detail
                                    {
                                        CachChoi = CachChoi.XcDuoiDao,
                                        DaiIn = new List<int> { dai },
                                        Dai = daiStr,
                                        So = new List<string> { dao },
                                        SoIn = new List<int> { int.Parse(dao) },
                                        SoTien = pre.Sl
                                    });
                                    detail.Xac.BaCon += pre.Sl;
                                }
                            }
                        }
                        break;
                    case CachChoi.XcDao:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            foreach (var numStr in pre.NumbersStr)
                            {
                                foreach (var dao in numStr.BaSoToBaoDao())
                                {
                                    detail.Details.Add(new Detail
                                    {
                                        CachChoi = CachChoi.XcDao,
                                        DaiIn = new List<int> { dai },
                                        Dai = daiStr,
                                        So = new List<string> { dao },
                                        SoIn = new List<int> { int.Parse(dao) },
                                        SoTien = pre.Sl
                                    });
                                    detail.Xac.BaCon += (sl3Dau + 1) * pre.Sl;
                                }
                            }
                        }
                        break;
                    case CachChoi.BaoBonCon:
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            for (int i = 0; i < pre.Numbers.Count; i++)
                            {
                                detail.Details.Add(new Detail
                                {
                                    CachChoi = CachChoi.BaoBonCon,
                                    DaiIn = new List<int> { dai },
                                    Dai = daiStr,
                                    So = new List<string> { pre.NumbersStr[i] },
                                    SoIn = new List<int> { pre.Numbers[i] },
                                    SoTien = pre.Sl
                                });
                                detail.Xac.BonCon += sl4Con * pre.Sl;
                            }
                        }
                        break;
                    case CachChoi.BonConDao:
                        break;
                }
            }

            return detail;
        }
        public (bool, string) HandlerKeo(string keo, string numberStr, int number, string[] array, ref int i, ref List<int> numbers, ref List<string> numberStrs)
        {
            bool result = false;
            string mess = string.Empty;

            var (str, iTemp) = FindNextStr(array, i);
            int num = 0;
            if (int.TryParse(str, out num))
            {
                i = iTemp;
                if (numberStr.Length != str.Length)
                {
                    mess = "Hai số kéo phải cùng 2 con, 3 con hoặc 4 con";
                }
                else if (numberStr == str)
                {
                    mess = "Phải kéo hai con khác nhau";
                }
                else if (keo == "khc" && (number % 10) != (num % 10))
                {
                    mess = $"Kéo hàng chục thì hàng đơn vị phải giống nhau. Ví dụ : 123khc193 b10n";
                }
                else if (keo == "kht" && (number % 100) != (num % 100))
                {
                    mess = $"Kéo hàng trăm thì hai số cuối phải giống nhau. Ví dụ : 123khc923 b10n";
                }
                else
                {
                    int num2 = 0;
                    var (str2, iTemp2) = FindNextStr(array, iTemp);
                    if (int.TryParse(str2, out num2))
                    {
                        i = iTemp2;
                        mess = $"Đã kéo thì không thêm được số";
                    }
                    else
                    {
                        var bc = 1;
                        if (keo == "khc") bc = 10;
                        if (keo == "kht") bc = 100;
                        var le = numberStr.Length;
                        var start = number > num ? num : number;
                        var end = number < num ? num : number;
                        numberStrs.Clear();
                        numbers.Clear();
                        for (int k = start; k <= end; k = k + bc)
                        {
                            numbers.Add(k);
                            if (le == 2)
                                numberStrs.Add(k.ToString("00"));
                            else if (le == 3)
                                numberStrs.Add(k.ToString("000"));
                            else if (le == 4)
                                numberStrs.Add(k.ToString("0000"));
                        }
                    }
                }
            }
            else
            {
                mess = $"Sai cú pháp kéo. Ví dụ 12keo92 b10n";
            }

            return (result, mess);
        }
        public (bool, string) CheckAndCreateItem(ref List<Cal3PrepareDto> cal3PrepareDtos, CachChoi? cachChoi, List<int> chanels, List<int> numbers, List<string> numberStrs, int sl)
        {
            bool result = false;
            string mess = string.Empty;
            if (!numberStrs.Any())
            {
                mess = $"Số phải đứng trước cách chơi. Ví dụ : 2d 23 54 {cachChoi.ToString().ToLower()}{sl}n";
                return (result, mess);
            }

            if (cachChoi == CachChoi.B && numberStrs.All(x => x.Length == 3))
            {
                cachChoi = CachChoi.BaoBaCon;
            }
            else if (cachChoi == CachChoi.B && numberStrs.All(x => x.Length == 4))
            {
                cachChoi = CachChoi.BaoBonCon;
            }
            else if (cachChoi == CachChoi.BaoDao && numberStrs.All(x => x.Length == 4))
            {
                cachChoi = CachChoi.BonConDao;
            }
            else if (cachChoi == CachChoi.Da && chanels.Count > 1)
            {
                cachChoi = CachChoi.DaX;
            }


            if ((cachChoi == CachChoi.B || cachChoi == CachChoi.Dau || cachChoi == CachChoi.Duoi
                || cachChoi == CachChoi.DD || cachChoi == CachChoi.Da || cachChoi == CachChoi.DaX) && numberStrs.All(x => x.Length != 2))
            {
                mess = "Cách chơi này chỉ chơi được 2 con";
            }
            else if ((cachChoi == CachChoi.Xc || cachChoi == CachChoi.XcDau || cachChoi == CachChoi.XcDui
               || cachChoi == CachChoi.XcDao || cachChoi == CachChoi.XcDauDao || cachChoi == CachChoi.XcDuoiDao) && numberStrs.All(x => x.Length != 3))
            {
                mess = "Cách chơi này chỉ chơi được 3 con";
            }
            else if (cachChoi == CachChoi.DaX && chanels.Count < 2)
            {
                mess = "Đá xiên phải đá từ 2 đài";
            }
            else if ((cachChoi == CachChoi.DaX || cachChoi == CachChoi.Da) && numbers.Count < 2)
            {
                mess = "Đá phải từ hai con";
            }
            else
            {
                var pre = new Cal3PrepareDto
                {
                    CachChoi = (CachChoi)cachChoi,
                    Chanels = chanels.CloneList(),
                    Numbers = numbers.CloneList(),
                    NumbersStr = numberStrs.CloneList(),
                    Sl = sl,
                };
                cal3PrepareDtos.Add(pre);
                result = true;
            }

            return (result, mess);
        }
        public int GetSl(string[] array, ref int i)
        {
            int result = 0;
            var (strNext, iTemp) = FindNextStr(array, i);
            if (int.TryParse(strNext, out result))
            {
                i = iTemp;
                if (result > 0)
                {
                    var (strNext2, iTemp2) = FindNextStr(array, i);
                    if (strNext2 == "n")
                        i = iTemp2 + 1;
                }
            }
            return result;
        }
        public bool GetCachChoi(string str, ref CachChoi? cachChoi, string[] array, ref int i, ref List<Cal3PrepareDto> cal3PrepareDtos,
                                    ref List<CachChoi?> cachChoiTemp, List<int> chanels, List<int> numbers, List<string> numberStrs)
        {
            bool result = true;
            var (strNext, iTemp) = FindNextStr(array, i);
            if (str == "b" || str == "bao" || str == "bl" || str == "blo" || str == "baolo")
            {
                if (strNext == "dao")
                {
                    cachChoi = CachChoi.BaoDao;
                    i = iTemp;
                }
                else
                    cachChoi = CachChoi.B;
            }
            else if (str == "da" || str == "dt")
            {
                cachChoi = CachChoi.Da;
            }
            else if (str == "dx" || str == "dax")
            {
                cachChoi = CachChoi.DaX;
            }
            else if (str == "dau")
            {
                cachChoi = CachChoi.Dau;
            }
            else if (str == "duoi" || str == "dui")
            {
                cachChoi = CachChoi.Duoi;
            }
            else if (str == "dd" || str == "dauduoi" || str == "daudui")
            {
                cachChoi = CachChoi.DD;
            }
            else if (str == "bdao" || str == "baodao" || str == "bldao" || str == "blodao" || str == "baolodao")
            {
                cachChoi = CachChoi.BaoDao;
            }
            else if (str == "xcdau" || str == "xdau")
            {
                if (strNext == "dao")
                {
                    cachChoi = CachChoi.XcDauDao;
                    i = iTemp;
                }
                else
                    cachChoi = CachChoi.XcDau;
            }
            else if (str == "xcduoi" || str == "xduoi" || str == "xcdui" || str == "xcdui")
            {
                if (strNext == "dao")
                {
                    cachChoi = CachChoi.XcDuoiDao;
                    i = iTemp;
                }
                else
                    cachChoi = CachChoi.XcDui;
            }
            else if (str == "xc" || str == "xchu" || str == "xiuchu")
            {
                if (strNext == "dao")
                {
                    cachChoi = CachChoi.XcDao;
                    i = iTemp;
                }
                else
                    cachChoi = CachChoi.Xc;
            }
            else if (str == "xcdaudao" || str == "xdaudao" || str == "xcdaodau" || str == "xdaodau")
            {
                cachChoi = CachChoi.XcDauDao;
            }
            else if (str == "xcduoidao" || str == "xduoidao" || str == "xcduidao" || str == "xcduidao")
            {
                cachChoi = CachChoi.XcDuoiDao;
            }
            else if (str == "xcdao" || str == "xdao" || str == "xiuchudao")
            {
                cachChoi = CachChoi.XcDao;
            }
            else if (str == "d")
            {
                result = false;
                var (s, j) = FindNextStr(array, i);
                int sl = 0;
                if (int.TryParse(s, out sl))
                {
                    if (sl > 0)
                    {
                        (s, j) = FindNextStr(array, j);
                        if (s == "n")
                        {
                            (s, j) = FindNextStr(array, j);
                        }
                        if (s == "d")
                        {
                            i = j;
                            result = true;
                            cachChoi = CachChoi.Dau;
                            cachChoiTemp.Add(cachChoi);
                            CheckAndCreateItem(ref cal3PrepareDtos, cachChoi, chanels, numbers, numberStrs, sl);
                            cachChoi = CachChoi.Duoi;
                        }

                    }
                }
            }
            else
                result = false;

            return result;
        }
        public List<string> HandlerStringNoSpace(string s)
        {
            List<string> lst = new List<string>();
            var arr = s.ToArray();
            bool isNumber = false;
            string str = "";
            for (int i = 0; i < arr.Length; i++)
            {
                var cstr = arr[i].ToString();
                if (int.TryParse(cstr, out _))
                {
                    if (!isNumber && !string.IsNullOrEmpty(str))
                    {
                        var num = str.StrToNumber();
                        if (num > 0)
                            lst.Add(num.ToString());
                        else
                            lst.Add(str);
                        str = "";
                    }
                    str = str + cstr;
                    isNumber = true;
                }
                else
                {
                    if (isNumber && !string.IsNullOrEmpty(str))
                    {
                        lst.Add(str);
                        str = "";
                    }
                    str = str + cstr;
                    isNumber = false;
                }
            }
            if (!string.IsNullOrEmpty(str))
            {
                lst.Add(str);
            }


            return lst;
        }
        private bool FindNext(string[] array, ref int i, params string[] str)
        {
            for (int j = i + 1; j < array.Length; j++)
            {
                if (array[j] != " ")
                {
                    if (str.Contains(array[j]))
                    {
                        i = j;
                        return true;
                    }
                    return false;
                }
            }

            return false;
        }
        private (string, int) FindNextStr(string[] array, int i)
        {
            for (int j = i + 1; j < array.Length; j++)
            {
                if (array[j] != " ")
                {
                    i = j;
                    return (array[j], j);
                }
            }

            return (" ", array.Length - 1);
        }
        public (bool, string) GetChanelsStart(DateTime date, ref List<int> chanels, MienEnum mien, string sys, string[] array, ref int i)
        {
            bool result = true;
            string mess = string.Empty;
            int num = 0;
            if (sys == "dc" || sys == "daichinh" || sys == "dchinh" || (sys == "dai" && FindNext(array, ref i, "chinh", "chanh")))
            {
                if (mien == MienEnum.MN)
                    chanels.Add(1);
                else if (mien == MienEnum.MT)
                    chanels.Add(5);

                return (result, mess);
            }
            else if (sys == "dp" || sys == "daiphu" || sys == "dphu" || (sys == "dai" && FindNext(array, ref i, "phu")))
            {
                if (mien == MienEnum.MN)
                    chanels.Add(2);
                else if (mien == MienEnum.MT)
                    chanels.Add(6);

                return (result, mess);
            }
            else if (int.TryParse(sys, out num))
            {
                if (FindNext(array, ref i, "d", "dai"))
                {
                    var slDai = InnitRepository._chanelCodeAll[date.DayOfWeek][mien].Count;
                    if (slDai < num)
                    {
                        mess = $"{date.ToString("dd-mm-yyyy")} chỉ có {slDai} đài";
                        result = false;
                    }
                    else
                    {
                        chanels.AddRange(InnitRepository._chanelCodeAll[date.DayOfWeek][mien].Select(x => x.Key).Take(num));
                    }
                }
                else
                {
                    mess = $"Sai cách xác định đài. Ví dụ {num}d hoặc {num}dai mới đúng !";
                }
                return (result, mess);
            }
            bool check = true;
            List<int> chanelsTemp = new List<int>();
            var dais = InnitRepository._chanelCodeAll[date.DayOfWeek][mien];
            bool isTangi = false;
            while (check)
            {
                if (array[i] == " ")
                {
                    i++;
                    isTangi = true;
                    continue;
                }
                var sy = array[i];
                var ch = dais.FirstOrDefault(x => x.Value.Contains(sy));
                if (ch.Value != null && ch.Value.Any())
                {
                    chanelsTemp.Add(ch.Key);
                    i++;
                    check = true;
                }
                else
                {
                    var (str, j) = FindNextStr(array, i);
                    var syl = sy + str;
                    var chl = dais.FirstOrDefault(x => x.Value.Contains(syl));
                    if (chl.Value != null && chl.Value.Any())
                    {
                        chanelsTemp.Add(chl.Key);
                        check = true;
                        i = j + 1;
                    }
                    else
                    {
                        check = false;
                        if (isTangi)
                        {
                            i = i - 1;
                        }
                    }
                }
            }
            if (chanelsTemp.Any())
                chanels = chanelsTemp.CloneList();
            else
            {
                mess = $"Không xác định được đài";
                result = false;
            }
            return (result, mess);
        }

        public string[] ChuanHoa(string sys)
        {
            var arr = new List<string>();
            sys = sys.ToLower();
            sys = sys.RemoveUnicode();
            sys = sys.Replace(".", " ");
            sys = sys.Replace(",", " ");
            sys = sys.Replace("\r", " ");
            sys = sys.Replace("\n", " ");
            sys = sys.Replace("\\", " ");
            sys = sys.Replace(":", " ");
            var array = sys.Split(" ").ToArray();
            foreach (var s in array)
            {
                if (string.IsNullOrEmpty(s))
                    arr.Add(" ");
                else
                {
                    arr.AddRange(HandlerStringNoSpace(s));
                    arr.Add(" ");
                }
            }
            return arr.ToArray();
        }
    }
}