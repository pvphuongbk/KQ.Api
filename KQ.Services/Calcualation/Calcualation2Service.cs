using KQ.Common.Enums;
using KQ.Common.Extention;
using KQ.Common.Helpers;
using KQ.Data.Base;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Enum;
using KQ.DataAccess.Interface;
using KQ.DataDto.Calculation;
using KQ.DataDto.Enum;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using KQ.DataAccess.Enum;

namespace KQ.Services.Calcualation
{
    public class Calcualation2Service : ICalcualation2Service
    {
        private readonly ICommonRepository<StoreKQ> _storeKQRepository;
        private readonly ICommonRepository<Details> _detailsRepository;
        private readonly ICommonUoW _commonUoW;
        private string _chanelNotFound = "Không xác định được đài";
        private string _dax2dai = "Đá xiên phải đá từ 2 đài";
        public Calcualation2Service(ICommonRepository<StoreKQ> storeKQRepository, ICommonUoW commonUoW, ICommonRepository<Details> detailsRepository, ICommonRepository<TileUser> tileUserRepository)
        {
            _storeKQRepository = storeKQRepository;
            _commonUoW = commonUoW;
            _detailsRepository = detailsRepository;
        }

        public ResponseBase Cal3Request(Cal3RequestDto dto)
        {
            try
            {
                Stopwatch s1 = new Stopwatch();
                #region init
                s1.Start();
                var array = ChuanHoa(dto.SynTax);
                int cursor = 0;
                int cursorTemp = 0;
                bool isStart = true;
                List<int> chanels = new List<int>();
                List<int> numbers = new List<int>();
                List<string> numberStrs = new List<string>();
                List<Cal3PrepareDto> cal3PrepareDtos = new List<Cal3PrepareDto>();
                int numTemp = 0;
                bool isDau = false;
                CachChoi? cachChoi = null;
                string messError = string.Empty;
                List<CachChoi?> cachChoiTemp = new List<CachChoi?>();
                Error error = null;
                bool isCompleted = true;
                #endregion
                if (dto.Mien == MienEnum.MB)
                {
                    chanels.Add(8);
                    isStart = false;
                }
                int i = 0;
                for (; i < array.Length; i++)
                {
                    if (array[i] == " ")
                        continue;

                    if (isStart)
                    {
                        var (check, mess) = GetChanelsStart(dto.HandlByDate, ref chanels, dto.Mien, array[i], array, ref i);
                        isStart = false;
                        if (check)
                        {
                            cursor = i;
                            cursorTemp = i;
                            isCompleted = false;
                        }
                        else
                        {
                            if (mess == _chanelNotFound)
                            {
                                var dais = InnitRepository._chanelCodeAll[dto.HandlByDate.DayOfWeek][dto.Mien].Select(x => x.Value[3]);
                                mess += $". Tên đài đúng: {string.Join(",", dais)}";
                                int count = 0;
                                for (int k = 0; k < dto.SynTax.Length; k++)
                                {
                                    if (dto.SynTax[k] != ' ' && !int.TryParse(dto.SynTax[k].ToString(), out _))
                                        count = k;
                                    if (int.TryParse(dto.SynTax[k].ToString(), out _))
                                    {
                                        count++;
                                        break;
                                    }
                                }
                                error = new Error { Message = mess, Count = count, StartIndex = 0 };
                                break;
                            }
                            else
                            {
                                // lỗi
                                var (startI, endI) = GetIndexForError(cursor, i, array);
                                error = new Error { Message = mess, Count = endI, StartIndex = startI };
                                break;
                            }
                        }
                    }
                    else if (int.TryParse(array[i], out numTemp))
                    {
                        if (array[i].Length == 1)
                        {
                            List<int> chanelsTemp = new List<int>();
                            int ibe = i - 1;
                            var (check, mess) = GetChanelsStart(dto.HandlByDate, ref chanelsTemp, dto.Mien, array[i], array, ref i);
                            if (check)
                            {
                                if (!isCompleted)
                                {
                                    error = CreateErrorForNotComplated(ref ibe, array, numbers, cursorTemp);
                                    break;
                                }

                                chanels = chanelsTemp;
                                numbers.Clear();
                                numberStrs.Clear();
                                cachChoi = null;
                                cursor = i;
                                isCompleted = false;
                            }
                            else
                            {
                                // lỗi
                                var (startI, endI) = GetIndexForError(cursor, i, array);
                                error = new Error { Message = mess, Count = endI, StartIndex = startI };
                                break;
                            }
                        }
                        else if (array[i].Length > 4)
                        {
                            // lỗi
                            var (startI, endI) = GetIndexForError(i, i + 1, array, false);
                            error = new Error { Message = "Số chơi không đúng", Count = endI, StartIndex = startI };
                            break;
                        }
                        else
                        {
                            isCompleted = false;
                            numbers.Clear();
                            numberStrs.Clear();
                            numberStrs.Add(array[i]);
                            numbers.Add(numTemp);
                            var stri = array[i];
                            var (str, iTemp) = FindNextStr(array, i);
                            if (str == "k" || str == "keo" || str == "khc" || str == "kht" || str == "kh")
                            {
                                i = iTemp;
                                var (check, mess) = HandlerKeo(str, stri, numTemp, array, ref i, ref numbers, ref numberStrs);
                                if (!check)
                                {
                                    // lỗi
                                    var (startI, endI) = GetIndexForError(cursor, i, array);
                                    error = new Error { Message = mess, Count = endI, StartIndex = startI };
                                    goto Foo;
                                }
                            }
                            else
                            {
                                while (int.TryParse(str, out numTemp) && str.Length > 1)
                                {
                                    numbers.Add(numTemp);
                                    numberStrs.Add(str);
                                    i = iTemp;
                                    (str, iTemp) = FindNextStr(array, i);
                                }
                            }

                            if (numberStrs.Any(o => o.Length != numberStrs[0].Length))
                            {
                                // Lỗi
                                var (startI, endI) = GetIndexForError(cursor, i, array, true);
                                error = new Error { StartIndex = startI, Count = endI, Message = "Các số chơi phải cùng 2 hoặc 3 hoặc 4 con" };
                                break;
                            }
                        }
                    }
                    else if (GetCachChoi(array[i], ref cachChoi, array, ref i, ref cal3PrepareDtos, ref cachChoiTemp, chanels, numbers, numberStrs, ref messError))
                    {
                        isCompleted = true;
                        int cursorDup = i;
                        if (!string.IsNullOrEmpty(messError))
                        {
                            // lỗi
                            var (startI, endI) = GetIndexForError(cursor, i, array);
                            error = new Error { Message = messError, Count = endI, StartIndex = startI };
                            break;
                        }
                        cachChoiTemp.Add(cachChoi);
                        string mess11 = "";
                        var sl = GetSl(array, ref i, dto.CoN, ref mess11);
                        if (sl > 0)
                        {
                            var (check1, mess1) = CheckAndCreateItem(ref cal3PrepareDtos, cachChoi, chanels, numbers, numberStrs, sl);
                            if (!check1)
                            {
                                //lỗi
                                if (mess1 == _dax2dai)
                                    cursor = cursorTemp;
                                var (startI, endI) = GetIndexForError(cursor, i, array);
                                error = new Error { StartIndex = startI, Count = endI, Message = mess1 };
                                break;
                            }
                            else
                            {
                                TangCurso(ref cursor, ref cursorTemp, array, i);
                            }
                            cachChoi = null;
                            sl = 0;
                            bool check = true;
                            while (check)
                            {
                                var (str, iTemp) = FindNextStr(array, i);
                                if (str.StartsWith("n"))
                                    str = str.Substring(1, str.Length - 1);
                                if (GetCachChoi(str, ref cachChoi, array, ref i, ref cal3PrepareDtos, ref cachChoiTemp, chanels, numbers, numberStrs, ref messError))
                                {
                                    if (!string.IsNullOrEmpty(messError))
                                    {
                                        // lỗi
                                        var (startI, endI) = GetIndexForError(cursor, i, array);
                                        error = new Error { Message = messError, Count = endI, StartIndex = startI };
                                        goto Foo;
                                    }
                                    cachChoiTemp.Add(cachChoi);
                                    if (cachChoiTemp.Distinct().Count() != cachChoiTemp.Count)
                                    {
                                        // lỗi
                                        var (strr, iTempp) = FindNextStr(array, iTemp);
                                        if (int.TryParse(strr, out _))
                                        {
                                            iTemp = iTempp;
                                            (strr, iTempp) = FindNextStr(array, iTemp);
                                            if (strr.StartsWith("n"))
                                                iTemp = iTempp;
                                        }
                                        var (startI, endI) = GetIndexForError(cursorDup, iTemp, array);
                                        error = new Error { StartIndex = startI, Count = endI, Message = "Lỗi trùng cách chơi" };
                                        goto Foo;
                                    }
                                    i = iTemp;
                                    string mess22 = "";
                                    var sl2 = GetSl(array, ref i, dto.CoN, ref mess22);
                                    var cursorTemp2 = cursor;
                                    TangCurso(ref cursor, ref cursorTemp, array, i);
                                    if (sl2 > 0)
                                    {
                                        var (check2, mess2) = CheckAndCreateItem(ref cal3PrepareDtos, cachChoi, chanels, numbers, numberStrs, sl2);
                                        if (!check2)
                                        {
                                            //lỗi
                                            var (startI, endI) = GetIndexForError(cursorTemp2, i, array);
                                            error = new Error { Message = mess2, Count = endI, StartIndex = startI };
                                            goto Foo;
                                        }
                                        else
                                        {
                                            TangCurso(ref cursor, ref cursorTemp, array, i);
                                        }
                                        cachChoi = null;
                                    }
                                    else
                                    {
                                        check = false;
                                        var (strr, iTempp) = FindNextStr(array, i);
                                        if (strr.StartsWith("n"))
                                            i = iTempp;
                                        //lỗi
                                        if (mess22 == "")
                                            mess22 = "Số tiền chơi phải lớn hơn 0";
                                        var (startI, endI) = GetIndexForError(iTemp, i, array, false);
                                        error = new Error { StartIndex = startI, Count = endI, Message = mess22 };
                                        goto Foo;
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
                            //lỗi
                            if (mess11 == "")
                                mess11 = "Số tiền chơi phải lớn hơn 0";
                            var (startI, endI) = GetIndexForError(cursorDup, i, array, false);
                            error = new Error { StartIndex = startI, Count = endI, Message = mess11 };
                            break;
                        }
                        cachChoiTemp.Clear();
                    }
                    else
                    {
                        List<int> chanelsTemp = new List<int>();
                        var (check, mess) = GetChanelsStart(dto.HandlByDate, ref chanelsTemp, dto.Mien, array[i], array, ref i);
                        if (check)
                        {
                            if (!isCompleted)
                            {
                                // lỗi
                                error = CreateErrorForNotComplated(ref i, array, numbers, cursorTemp);
                                break;
                            }
                            //cal3PrepareDtos.ForEach(x => x.Chanels = chanels.CloneList());
                            chanels = chanelsTemp;
                            numbers.Clear();
                            numberStrs.Clear();
                            cachChoi = null;
                            isCompleted = false;
                            cursor = i;
                            cursorTemp = i;
                        }
                        else
                        {
                            // lỗi
                            int count = i;
                            for (int k = i + 1; k < array.Length; k++)
                            {
                                if (array[k] != " " && !int.TryParse(array[k], out _))
                                    count = k;
                                if (int.TryParse(array[k], out _))
                                {
                                    break;
                                }
                            }
                            var (startI, endI) = GetIndexForError(cursor, count, array, false);
                            error = new Error { StartIndex = startI, Count = endI, Message = "Sai cú pháp" };
                            break;
                        }
                    }
                }
                if (!isCompleted && error == null)
                {
                    // lỗi
                    error = CreateErrorForNotComplated(ref i, array, numbers, cursorTemp);
                }
            Foo:
                if (error == null)
                {
                    var detail = CreateDetail(dto, cal3PrepareDtos);
                    var isUpdate = UpdateTrungThuong(dto.HandlByDate, dto.CachTrungDaThang, dto.CachTrungDaXien, dto.Mien, ref detail);
                    if (isUpdate)
                    {
                        UpdateSumTrungThuong(ref detail);
                    }

                    if (dto.IsSave)
                    {
                        _commonUoW.BeginTransaction();
                        var json = JsonConvert.SerializeObject(detail);
                        if (dto.IDMessage == null)
                        {
                            Details de = new Details
                            {
                                CreatedDate = DateTime.Now,
                                IDKhach = dto.IDKhach,
                                UserID = dto.UserID,
                                Detail = json.Encrypt(),
                                HandlByDate = dto.HandlByDate,
                                IsTinh = detail.IsTinh,
                                Message = dto.SynTax.Encrypt(),
                                Mien = dto.Mien,
                            };
                            _detailsRepository.Insert(de);
                        }
                        else
                        {
                            var mess = _detailsRepository.GetById(dto.IDMessage);
                            if (mess != null)
                            {
                                mess.Detail = json.Encrypt();
                                mess.IsTinh = detail.IsTinh;
                                mess.CreatedDate = DateTime.Now;
                                mess.Message = dto.SynTax.Encrypt();
                                _detailsRepository.Update(mess);
                            }
                        }

                        _commonUoW.Commit();
                    }
                    s1.Stop();
                    detail.Trung.HaiCB = Math.Round(detail.Trung.HaiCB, 2);
                    detail.Trung.HaiCD = Math.Round(detail.Trung.HaiCD, 2);
                    detail.Trung.DaT = Math.Round(detail.Trung.DaT, 2);
                    detail.Trung.DaX = Math.Round(detail.Trung.DaX, 2);
                    detail.Trung.BaCon = Math.Round(detail.Trung.BaCon, 2);
                    detail.Trung.BonCon = Math.Round(detail.Trung.BonCon, 2);

                    detail.Xac.HaiCB = Math.Round(detail.Xac.HaiCB, 2);
                    detail.Xac.HaiCD = Math.Round(detail.Xac.HaiCD, 2);
                    detail.Xac.DaT = Math.Round(detail.Xac.DaT, 2);
                    detail.Xac.DaX = Math.Round(detail.Xac.DaX, 2);
                    detail.Xac.BaCon = Math.Round(detail.Xac.BaCon, 2);
                    detail.Xac.BonCon = Math.Round(detail.Xac.BonCon, 2);
                    return new ResponseBase { Data = detail };
                }
                else
                {
                    s1.Stop();
                    return new ResponseBase { Data = new Cal3DetailDto { Error = error } };
                }
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "Cal3Request");
                if (dto.IsSave)
                    _commonUoW.RollBack();
                return new ResponseBase { Code = 501, Message = ex.Message };
            }
        }
        public void TangCurso(ref int cursor, ref int cursorTemp, string[] array, int i)
        {
            if (array[i] == "n" && array.Length > (i + 1))
            {
                cursorTemp = i + 1;
                cursor = i + 1;
            }
            else
            {
                cursorTemp = i;
                cursor = i;
            }
        }
        public Error CreateErrorForNotComplated(ref int i, string[] array, List<int> numbers, int cursorTemp)
        {
            // lỗi
            string mes = "Không xác định được cách chơi";
            if (!numbers.Any())
                mes = "Không xác định được số chơi và cách chơi";
            if (i >= array.Length)
                i = array.Length - 1;
            var (startI, endI) = GetIndexForError(cursorTemp, i, array, false);
            var error = new Error { StartIndex = startI, Count = endI, Message = mes };
            return error;
        }
        public (int, int) GetIndexForError(int cursor, int i, string[] array, bool getNext = true)
        {
            int start = 0;
            int end = 0;
            int current = 0;
            bool isHave = false;
            for (int j = cursor; j < array.Length; j++)
            {
                if (array[j] != " ")
                    break;
                cursor++;
            }
            if (getNext)
            {
                for (int j = cursor + 1; j < array.Length; j++)
                {
                    if (array[j] != " ")
                    {
                        if (isHave)
                            cursor++;
                        break;
                    }
                    cursor++;
                    isHave = true;
                }
            }
            for (int j = i; j >= 0; j--)
            {
                if (array[j] != " ")
                {
                    i = j;
                    break;
                }
            }
            for (int j = 0; j < array.Length; j++)
            {
                if (cursor != 0 && j == cursor)
                    start = current;
                current += array[j].Length;
                if (j == i)
                    end = current;
            }
            if (end <= start)
            {
                return (start, array[i].Length);
            }
            return (start, end - start);
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

            detail.Trung.HaiCB = detail.Trung.HaiCB;
            detail.Trung.HaiCD = detail.Trung.HaiCD;
            detail.Trung.DaT = detail.Trung.DaT;
            detail.Trung.DaX = detail.Trung.DaX;
            detail.Trung.BaCon = detail.Trung.BaCon;
            detail.Trung.BonCon = detail.Trung.BonCon;
        }

        public bool CheckResource(List<int>[] kq2So, List<int>[] kq3So, List<int>[] kq4So, MienEnum mien, DayOfWeek day)
        {
            if (kq2So == null || kq3So == null || kq4So == null)
                return false;
            switch (mien)
            {
                case MienEnum.MN:
                    if (kq2So[0] == null || kq2So[1] == null || kq2So[2] == null
                        || (day == DayOfWeek.Saturday && kq2So[3] == null)
                        || kq3So[0] == null || kq3So[1] == null || kq3So[2] == null
                        || (day == DayOfWeek.Saturday && kq3So[3] == null)
                        || kq4So[0] == null || kq4So[1] == null || kq4So[2] == null
                        || (day == DayOfWeek.Saturday && kq4So[3] == null))
                        return false;
                    break;
                case MienEnum.MT:
                    if (kq2So[4] == null || kq2So[5] == null
                        || ((day == DayOfWeek.Saturday || day == DayOfWeek.Thursday || day == DayOfWeek.Sunday) && kq2So[6] == null)
                        || kq3So[4] == null || kq3So[5] == null
                        || ((day == DayOfWeek.Saturday || day == DayOfWeek.Thursday || day == DayOfWeek.Sunday) && kq3So[6] == null)
                        || kq4So[4] == null || kq4So[5] == null
                        || ((day == DayOfWeek.Saturday || day == DayOfWeek.Thursday || day == DayOfWeek.Sunday) && kq4So[6] == null))
                        return false;
                    break;
                default:
                    if (kq2So[7] == null || kq3So[7] == null || kq4So[7] == null)
                        return false;
                    break;
            }

            return true;
        }
        public bool UpdateTrungThuong(DateTime handlByDate, CachTrungDa dathang, CachTrungDa daxien, MienEnum mien, ref Cal3DetailDto detail)
        {
            Dictionary<string, double> meT2c = new Dictionary<string, double>();
            Dictionary<string, double> meDD = new Dictionary<string, double>();
            Dictionary<string, double> meDaT = new Dictionary<string, double>();
            Dictionary<string, double> meDaX = new Dictionary<string, double>();
            Dictionary<string, double> me3Con = new Dictionary<string, double>();
            Dictionary<string, double> me4Con = new Dictionary<string, double>();

            var kq2So = new List<int>[8];
            var kq3So = new List<int>[8];
            var kq4So = new List<int>[8];
            if (handlByDate.Date == DateTime.Now.Date)
            {
                kq2So = InnitRepository._totalDic["Now"];
                kq3So = InnitRepository._totalBaCangDic["Now"];
                kq4So = InnitRepository._totalBonSoDic["Now"];
            }
            else if (handlByDate.Date < DateTime.Now.Date)
            {
                var kq = _storeKQRepository.FindAll(x => x.CreatedDate.Date == handlByDate.Date).FirstOrDefault();
                if (kq != null && !string.IsNullOrEmpty(kq.HaiCon)
                    && !string.IsNullOrEmpty(kq.BaCon) && !string.IsNullOrEmpty(kq.BonCon))
                {
                    kq2So = JsonConvert.DeserializeObject<List<int>[]>(kq.HaiCon);
                    kq3So = JsonConvert.DeserializeObject<List<int>[]>(kq.BaCon);
                    kq4So = JsonConvert.DeserializeObject<List<int>[]>(kq.BonCon);
                }
            }
            if (!CheckResource(kq2So, kq3So, kq4So, mien, handlByDate.Date.DayOfWeek))
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
                        if (dathang == CachTrungDa.NhieuCap)
                        {
                            var count = count1 < count2 ? count1 : count2;
                            pre.SlTrung = count;
                            if (pre.SlTrung > 0)
                            {
                                string key = $"{pre.SoIn[0].ToString("00")} {pre.SoIn[1].ToString("00")}";
                                if (meDaT.ContainsKey(key))
                                    meDaT[key] += pre.SlTrung * pre.SoTien;
                                else
                                    meDaT.Add(key, pre.SlTrung * pre.SoTien);
                            }
                        }
                        else if (dathang == CachTrungDa.KyRuoi)
                        {
                            pre.SlTrung = (count1 + count2) / 2;
                            if (pre.SlTrung > 0)
                            {
                                string key = $"{pre.SoIn[0].ToString("00")} {pre.SoIn[1].ToString("00")}";
                                if (meDaT.ContainsKey(key))
                                    meDaT[key] += pre.SlTrung * pre.SoTien;
                                else
                                    meDaT.Add(key, pre.SlTrung * pre.SoTien);
                            }
                        }
                        else
                        {
                            pre.SlTrung = (count1 > 0 && count2 > 0) ? 1 : 0;
                            if (pre.SlTrung > 0)
                            {
                                string key = $"{pre.SoIn[0].ToString("00")} {pre.SoIn[1].ToString("00")}";
                                if (meDaT.ContainsKey(key))
                                    meDaT[key] += pre.SlTrung * pre.SoTien;
                                else
                                    meDaT.Add(key, pre.SlTrung * pre.SoTien);
                            }
                        }
                        break;
                    case CachChoi.DaX:
                        var kqx = new List<int>();
                        kqx.AddRange(kq2So[pre.DaiIn[0] - 1]);
                        kqx.AddRange(kq2So[pre.DaiIn[1] - 1]);

                        var count1x = kqx.Count(x => x == pre.SoIn[0]);
                        var count2x = kqx.Count(x => x == pre.SoIn[1]);
                        if (daxien == CachTrungDa.NhieuCap)
                        {
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
                        }
                        else if (daxien == CachTrungDa.KyRuoi)
                        {
                            pre.SlTrung = (count1x + count2x) / 2;
                            if (pre.SlTrung > 0)
                            {
                                string key = $"{pre.SoIn[0].ToString("00")} {pre.SoIn[1].ToString("00")}";
                                if (meDaX.ContainsKey(key))
                                    meDaX[key] += pre.SlTrung * pre.SoTien;
                                else
                                    meDaX.Add(key, pre.SlTrung * pre.SoTien);
                            }
                        }
                        else
                        {
                            pre.SlTrung = (count1x > 0 && count2x > 0) ? 1 : 0;
                            if (pre.SlTrung > 0)
                            {
                                string key = $"{pre.SoIn[0].ToString("00")} {pre.SoIn[1].ToString("00")}";
                                if (meDaX.ContainsKey(key))
                                    meDaX[key] += pre.SlTrung * pre.SoTien;
                                else
                                    meDaX.Add(key, pre.SlTrung * pre.SoTien);
                            }
                        }
                        break;
                    case CachChoi.Dau:
                        if (mien == MienEnum.MB)
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
                        if (mien == MienEnum.MB)
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
                        if (mien == MienEnum.MB)
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
                        if (mien == MienEnum.MB)
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
                        if (mien == MienEnum.MB)
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
                        if (mien == MienEnum.MB)
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
            detail.IsTinh = true;
            if (meT2c.Any())
            {
                var mes = new List<string>();
                foreach (var m in meT2c)
                    mes.Add($"{m.Key}({Math.Round(m.Value, 2)}n)");
                detail.TrungDetail.Add($"*T2C: {string.Join(",", mes)}");
            }
            if (meDD.Any())
            {
                var mes = new List<string>();
                foreach (var m in meDD)
                    mes.Add($"{m.Key}({Math.Round(m.Value, 2)}n)");
                detail.TrungDetail.Add($"*2CĐ: {string.Join(",", mes)}");
            }
            if (meDaT.Any())
            {
                var mes = new List<string>();
                foreach (var m in meDaT)
                    mes.Add($"{m.Key}({Math.Round(m.Value, 2)}n)");
                detail.TrungDetail.Add($"*ĐáT: {string.Join(",", mes)}");
            }
            if (meDaX.Any())
            {
                var mes = new List<string>();
                foreach (var m in meDaX)
                    mes.Add($"{m.Key}({Math.Round(m.Value, 2)}n)");
                detail.TrungDetail.Add($"*ĐáX: {string.Join(",", mes)}");
            }
            if (me3Con.Any())
            {
                var mes = new List<string>();
                foreach (var m in me3Con)
                    mes.Add($"{m.Key}({Math.Round(m.Value, 2)}n)");
                detail.TrungDetail.Add($"*3C: {string.Join(",", mes)}");
            }
            if (me4Con.Any())
            {
                var mes = new List<string>();
                foreach (var m in me4Con)
                    mes.Add($"{m.Key}({Math.Round(m.Value, 2)}n)");
                detail.TrungDetail.Add($"*4C: {string.Join(",", mes)}");
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
            var slDai = InnitRepository._chanelCodeAll[dtos.HandlByDate.DayOfWeek][dtos.Mien];
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
                        foreach (var dai in pre.Chanels)
                        {
                            var daiStr = slDai[dai][3];
                            foreach (var numStr in pre.NumbersStr)
                            {
                                foreach (var dao in numStr.BonSoToBaoDao())
                                {
                                    detail.Details.Add(new Detail
                                    {
                                        CachChoi = CachChoi.BaoBonCon,
                                        DaiIn = new List<int> { dai },
                                        Dai = daiStr,
                                        So = new List<string> { dao },
                                        SoIn = new List<int> { int.Parse(dao) },
                                        SoTien = pre.Sl
                                    });
                                    detail.Xac.BonCon += sl4Con * pre.Sl;
                                }
                            }
                        }
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
                        result = true;
                    }
                }
            }
            else
            {
                mess = $"Sai cú pháp kéo. Ví dụ 12keo92 b10n";
            }

            return (result, mess);
        }
        public (bool, string) CheckAndCreateItem(ref List<Cal3PrepareDto> cal3PrepareDtos, CachChoi? cachChoi, List<int> chanels, List<int> numbers, List<string> numberStrs, double sl)
        {
            bool result = false;
            string mess = string.Empty;
            if (!numberStrs.Any())
            {
                mess = $"Số phải đứng trước cách chơi. Ví dụ : 2d 23 54 {cachChoi.ToString().ToLower()}{sl}n";
                return (result, mess);
            }
            else if (cachChoi == null)
            {
                mess = $"Không xác định được cách chơi";
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

            if (cachChoi == CachChoi.B && numberStrs.All(x => x.Length != 2))
            {
                mess = "Cách chơi này chỉ chơi được 2 hoặc 3 con";
            }
            else if ((cachChoi == CachChoi.Dau || cachChoi == CachChoi.Duoi || cachChoi == CachChoi.DD || cachChoi == CachChoi.Da ||
                cachChoi == CachChoi.DaX) && numberStrs.All(x => x.Length != 2))
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
                mess = _dax2dai;
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
        public double GetSl(string[] array, ref int i, bool coN, ref string mess)
        {
            double result = 0;
            var (strNext, iTemp) = FindNextStr(array, i);
            if (double.TryParse(strNext, out result))
            {
                //if (result == 0 && array[iTemp + 1] == " " && array[iTemp + 2].Length == 1 && double.TryParse(array[iTemp + 2], out result))
                //{
                //    iTemp = iTemp + 2;
                //    result = result / 10;
                //}
                i = iTemp;
                if (result > 0)
                {
                    var (strNext2, iTemp2) = FindNextStr(array, i);
                    if (strNext2 == "n")
                        i = iTemp2;
                    else if (coN)
                    {
                        result = 0;
                        mess = "Phải có n sau số tiền chơi. ví dụ 12 baolo 10n";
                    }
                }
            }
            return result;
        }
        public bool GetCachChoi(string str, ref CachChoi? cachChoi, string[] array, ref int i, ref List<Cal3PrepareDto> cal3PrepareDtos,
                                    ref List<CachChoi?> cachChoiTemp, List<int> chanels, List<int> numbers, List<string> numberStrs, ref string messError)
        {
            bool result = true;
            var (strNext, iTemp) = FindNextStr(array, i);
            if (str == "b" || str == "bao" || str == "blo" || str == "baolo" || str == "lo")
            {
                if (strNext == "dao")
                {
                    cachChoi = CachChoi.BaoDao;
                    i = iTemp;
                }
                else
                    cachChoi = CachChoi.B;
            }
            else if (str == "da" || str == "dt" || str == "dat"
                || str == "dathang")
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
            else if (str == "xcduoi" || str == "xduoi" || str == "xcdui" || str == "xcdui" || str == "xdui")
            {
                if (strNext == "dao")
                {
                    cachChoi = CachChoi.XcDuoiDao;
                    i = iTemp;
                }
                else
                    cachChoi = CachChoi.XcDui;
            }
            else if (str == "xc" || str == "xchu" || str == "xiuchu" || str == "x")
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
            else if (str == "xcduoidao" || str == "xduoidao" || str == "xcduidao" || str == "xcduidao" || str == "xduidao")
            {
                cachChoi = CachChoi.XcDuoiDao;
            }
            else if (str == "xcdao" || str == "xdao" || str == "xiuchudao" || str == "xd")
            {
                cachChoi = CachChoi.XcDao;
            }
            else if (str == "dv" || str == "dav" || str == "davong")
            {
                cachChoi = CachChoi.Da;
                if (numberStrs.Count < 2)
                {
                    messError = "một con không thể đá vòng";
                    result = true;
                }
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
                            cachChoi = CachChoi.Dau;
                            cachChoiTemp.Add(cachChoi);
                            CheckAndCreateItem(ref cal3PrepareDtos, cachChoi, chanels, numbers, numberStrs, sl);
                            cachChoi = CachChoi.Duoi;
                            result = true;
                            i = j;
                        }
                        else
                        {
                            messError = "Cú pháp d phải được nhập 2 lần mới đúng. Vd : d5n d6n";
                            result = true;
                            i = j - 1;
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
                if (isNumber && cstr == "n")
                {
                    if (!string.IsNullOrEmpty(str))
                        lst.Add(str);
                    lst.Add("n");
                    str = "";
                    isNumber = false;
                }
                else if (int.TryParse(cstr, out _))
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
                else if ((cstr == "." || cstr == ",") && isNumber && arr.Length > (i + 1)
                    && int.TryParse(arr[i + 1].ToString(), out _)
                    && (arr.Length <= (i + 2) || (!int.TryParse(arr[i + 2].ToString(), out _) && !FindNextd(arr, i + 2))))
                {
                    lst.Add($"{str}.{arr[i + 1].ToString()}");
                    str = "";
                    i++;
                }
                else if (cstr == "." || cstr == ",")
                {
                    if (!string.IsNullOrEmpty(str))
                        lst.Add(str);
                    lst.Add(" ");
                    str = "";
                }
                else if (cstr == " ")
                {
                    if (!string.IsNullOrEmpty(str))
                        lst.Add(str);
                    lst.Add(" ");
                    str = "";
                }
                else
                {
                    if (isNumber && !string.IsNullOrEmpty(str))
                    {
                        if (!string.IsNullOrEmpty(str))
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
        private bool FindNextd(char[] array, int i)
        {
            for (int j = i; j < array.Length; j++)
            {
                var t = array[j].ToString();
                if (array[j].ToString() != " " && array[j].ToString() != string.Empty)
                {
                    if (array[j].ToString() == "d")
                    {
                        if (array.Length > (j + 1))
                        {
                            var txt = array[j + 1].ToString();
                            if (txt == "d" || txt == "x" || txt == "a" || txt == "t" || txt == "u")
                                return false;
                        }

                        int count = 0;
                        for (int k = j + 1; k < array.Length; k++)
                        {
                            if (array[k].ToString() == " " || (count > 1 && array[k].ToString() == "n"))
                            {
                                continue;
                            }
                            else if (int.TryParse(array[k].ToString(), out _))
                                count++;
                            else if (array[k].ToString() == "d" && count > 1)
                                return false;
                            else
                                break;
                        }

                        return true;
                    }
                    return false;
                }
            }

            return false;
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
                if (array[j] != " " && array[j] != string.Empty)
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
                else
                    chanels.Add(8);

                return (result, mess);
            }
            else if (sys == "dp" || sys == "daiphu" || sys == "dphu" || (sys == "dai" && FindNext(array, ref i, "phu")))
            {
                if (mien == MienEnum.MN)
                    chanels.Add(2);
                else if (mien == MienEnum.MT)
                    chanels.Add(6);
                else
                {
                    mess = $"Miền bắc chỉ có 1 đài";
                    result = false;
                }

                return (result, mess);
            }
            else if (int.TryParse(sys, out num))
            {
                if (FindNext(array, ref i, "d", "dai"))
                {
                    var slDai = InnitRepository._chanelCodeAll[date.DayOfWeek][mien].Count;
                    if (slDai < num)
                    {
                        if (mien == MienEnum.MB)
                            mess = $"Miền bắc chỉ có 1 đài";
                        else
                            mess = $"{date.ToString("dd-MM-yyyy")} chỉ có {slDai} đài";
                        result = false;
                    }
                    else
                    {
                        chanels.AddRange(InnitRepository._chanelCodeAll[date.DayOfWeek][mien].Select(x => x.Key).Take(num));
                    }
                }
                else
                {
                    for (int k = i + 1; k < array.Length; k++)
                    {
                        if (int.TryParse(array[k], out _))
                            break;
                        i = k;
                    }
                    mess = $"Sai cách xác định đài. Ví dụ 2d hoặc 3dai mới đúng !";
                    result = false;
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
                    isTangi = true;
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
            for (int k = i; k >= 0; k--)
            {
                if (array[k] == " ")
                    i--;
                else
                    break;
            }
            if (chanelsTemp.Any())
                chanels = chanelsTemp.CloneList();
            else
            {
                mess = _chanelNotFound;
                result = false;
            }
            return (result, mess);
        }

        public string[] ChuanHoa(string sys)
        {
            var arr = new List<string>();
            sys = sys.ToLower();
            sys = sys.RemoveUnicode();
            //sys = sys.Replace(".", " ");
            sys = sys.Replace("-", " ");
            //sys = sys.Replace(",", " ");
            sys = sys.Replace("\r", " ");
            sys = sys.Replace("\n", " ");
            sys = sys.Replace("\\", " ");
            sys = sys.Replace(":", " ");
            sys = sys.Replace("@", " ");
            //var array = sys.Split(" ").ToArray();
            arr.AddRange(HandlerStringNoSpace(sys));
            //foreach (var s in array)
            //{
            //    if (string.IsNullOrEmpty(s))
            //        arr.Add(" ");
            //    else
            //    {
            //        arr.AddRange(HandlerStringNoSpace(s));
            //        arr.Add(" ");
            //    }
            //}
            return arr.ToArray();
        }
    }
}