using KQ.Common.Constants;
using KQ.Common.Enums;
using KQ.Common.Extention;
using KQ.Common.Helpers;
using KQ.Data.Base;
using KQ.DataDto.Calculation;
using KQ.DataDto.Common;
using KQ.DataDto.Enum;
using Newtonsoft.Json;

namespace KQ.Services.Calcualation
{
    public class CalcualationService : ICalcualationService
    {
        private ChanelDto[] _currentChanels { get; set; }
        private static List<string> _slStr = new List<string> { "n", "ng", "ngin"};
        public CalcualationService()
        {
            _currentChanels = CommonFunction.GetChanels(null).SelectMany(x => x.value).ToArray();
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
                if ((cstr == "n" || cstr == "N" || ((cstr == "k" || cstr == "K") && !s.ToLower().Contains("keo"))) && isNumber)
                {
                    lst.Add(str);
                    lst.Add("n");
                    str = "";
                    if (arr.Length > (i + 1) && (arr[i + 1] == 'g' || arr[i + 1] == 'G'))
                        i++;
                    if (arr.Length > (i + 1) && (arr[i + 1] == 'i' || arr[i + 1] == 'I'))
                        i++;
                    if (arr.Length > (i + 1) && (arr[i + 1] == 'n' || arr[i + 1] == 'N'))
                        i++;
                    //n -ngin- ng-k
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
        public ResponseBase Cal2Request(Cal2RequestDto syntaxes)
        {
            try
            {
                var res = new ResponseBase();
                Cal2ResponseDto result = new Cal2ResponseDto
                {
                    MessageThuong = new List<string>(),
                    MessageXac = new List<string>(),
                    MessageLoi = new string[syntaxes.SynTaxes.Count]
                };
                List<Cal1RequestDto> dtos = new List<Cal1RequestDto>();
                int index = -1;
                foreach (var sys in syntaxes.SynTaxes)
                {
                    if (string.IsNullOrEmpty(sys))
                        continue;
                    FileHelper.GeneratorFileByDay(FileStype.Log, sys, "Cal2Request");
                    var sy = sys.ChuanHoaString();
                    List<Cal1RequestDto> dtoTemp = new List<Cal1RequestDto>();
                    index++;
                    List<string> items = new List<string>();
                    List<string> itemsGoc = new List<string>();
                    foreach (var it2 in sy.Split(" "))
                        foreach (var it3 in it2.Split("."))
                            foreach (var it4 in it3.Split(","))
                                if (!string.IsNullOrEmpty(it4))
                                {
                                    var lst = HandlerStringNoSpace(it4);
                                    itemsGoc.AddRange(lst);
                                    lst = lst.ChuanHoaString2();
                                    items.AddRange(lst);
                                }

                    List<int> chanels = new List<int>();
                    List<int> chanelsCache = new List<int>();
                    List<string> numbers = new List<string>();
                    List<string> numberCache = new List<string>();
                    List<string> unknowList = new List<string>();
                    List<string> unknowListGoc = new List<string>();
                    int? type = null;
                    int sl = 0;
                    int indexTrue = 0;
                    var array = items.ToArray();
                    var arrayGoc = itemsGoc.ToArray();
                    string chanel = string.Empty;
                    int slDai = 0;
                    bool isNumberContinute = false;
                    string previous = "";
                    string previousTo = "";
                    for (int i = 0; i < array.Length; i++)
                    {
                        isNumberContinute = false;
                        int number = 0;
                        var it = array[i];
                        int indexGoc = i;
                        int? tType = null;
                        if (type == null)
                        {
                            tType = GetType(it, array, ref i);
                        }
                        if(it == "dc")
                        {
                            slDai = 1;
                            if (dtoTemp.Any(x => x.Chanels == null || !x.Chanels.Any()))
                            {
                                CreateDto(ref dtoTemp, ref chanels, ref chanel, ref slDai, ref indexTrue, ref i, ref type, ref numberCache,
                                    ref numbers, ref sl, ref result.MessageLoi[index], ref chanelsCache, true);
                            }
                        }
                        else if ((it == "d" || it == "dai") && previous.Length == 1 && numbers.Any() && numbers.Last().StringToInt() <= 4)
                        {
                            if (!chanels.Any() && array.Length > (i + 1) && array[i + 1].GetChanel(ref chanel))
                            {
                                i++;
                            }
                            var slDaiStr = numbers.Last();
                            var slDaiTruoc = slDai;
                            if (slDai > 0 && !chanels.Any())
                            {
                                if (chanel == "hn")
                                    chanels.Add(8);
                                else if (chanel == "mt")
                                    for (int j = 0; j < slDai; j++)
                                        chanels.Add(5 + j);
                                else if (chanel == "mn" || string.IsNullOrEmpty(chanel))
                                    for (int j = 0; j < slDai; j++)
                                        chanels.Add(1 + j);
                                chanel = string.Empty;
                                slDai = 0;
                            }
                            var slDaiTem = slDaiStr.StringToInt();
                            numbers.Remove(slDaiStr);
                            if(type == 1)
                            {
                                if(slDaiTruoc == 2 && slDaiTem != 2)
                                {
                                    result.MessageLoi[index] = "Đá xiên phải đá 2 đài";
                                }
                            }
                            else
                            {
                                slDai = slDaiTem;
                            }
                            if (dtoTemp.Any(x => x.Chanels == null || !x.Chanels.Any()))
                            {
                                CreateDto(ref dtoTemp, ref chanels, ref chanel, ref slDai, ref indexTrue, ref i, ref type, ref numberCache,
                                    ref numbers, ref sl, ref result.MessageLoi[index], ref chanelsCache);
                            }
                        }
                        else if (tType != null)
                        {
                            type = tType;
                            int num = 0;
                            if (numbers.Any() && array.Length > (i + 1) && int.TryParse(array[i + 1], out num) && sl == 0)
                            {
                                sl = num;
                                i++;
                                if (array.Length > (i + 1) && array[i + 1] == "n")
                                {
                                    i++;
                                }
                            }
                        }
                        else if (int.TryParse(it, out number))
                        {
                            if (type != null && (numbers.Any() || numberCache.Any()) && sl == 0
                                && number > 0 && (i == (array.Length - 1) || !_slStr.Contains(array[i + 1]) && !int.TryParse(array[i + 1], out _)))
                            {
                                if (array.Length > (i + 1) && array[i + 1] == "da")
                                {
                                    numbers.Add(array[i]);
                                    i++;
                                }
                                else
                                    sl = number;
                            }
                            else
                            {
                                numbers.Add(it);
                                if (it.Length > 1)
                                    isNumberContinute = true;
                            }

                        }
                        else if (_slStr.Contains(it) && numbers.Any())
                        {
                            var slStr = numbers.Last();
                            sl = slStr.StringToInt();
                            numbers.Remove(slStr);
                        }
                        else if (it.GetChanel(ref chanel))
                        {
                            //indexTrue = i;
                        }
                        else if ((it == "keo" || it == "-" || it == "toi" || it == "den")
                                && numbers.Any() && array.Length > (i + 1) && int.TryParse(array[i + 1], out _))
                        {
                            var nuStr = numbers.Last();
                            var num1 = int.Parse(nuStr);
                            var num2 = int.Parse(array[i + 1]);
                            i++;
                            if (num1 < num2)
                            {
                                if (nuStr.Length == 2)
                                    for (int k = num1 + 1; k <= num2; k++)
                                        numbers.Add(k.ToString("00"));
                                else if (nuStr.Length == 3)
                                    for (int k = num1 + 1; k <= num2; k++)
                                        numbers.Add(k.ToString("000"));
                                else if (nuStr.Length == 4)
                                    for (int k = num1 + 1; k <= num2; k++)
                                        numbers.Add(k.ToString("0000"));
                                else if (nuStr.Length == 4)
                                    result.MessageLoi[index] = $"Không thể kéo từ {nuStr}";
                            }
                            else
                            {
                                result.MessageLoi[index] = $"Không thể kéo từ {num1} đến {num2}";
                            }
                        }
                        else
                        {
                            lock (InnitRepository._chanelCode)
                            {
                                var ch = InnitRepository._chanelCode.FirstOrDefault(x => x.Value.Contains(it));
                                if (Constants.IstestMode)
                                {
                                    ch = InnitRepository._chanelCodeForTest.FirstOrDefault(x => x.Value.Contains(it));
                                }
                                if (ch.Value != null && ch.Value.Any())
                                {
                                    List<int> chanelsTemps = new List<int>();
                                    CheckChanel(ref array, ref i, ref chanelsTemps);
                                    if (chanels.Any() || slDai > 0)
                                    {
                                        if (dtoTemp.Any(x => x.Chanels == null || !x.Chanels.Any()))
                                        {
                                            CreateDto(ref dtoTemp, ref chanels, ref chanel, ref slDai, ref indexTrue, ref i, ref type,
                                                ref numberCache, ref numbers, ref sl, ref result.MessageLoi[index], ref chanelsCache);
                                        }
                                        chanels.Add(ch.Key);
                                        chanels.AddRange(chanelsTemps.CloneList());
                                        if((previous == "dai" || previousTo == "dai") && dtoTemp.Any())
                                        {
                                            dtoTemp.First().Chanels = chanels;
                                            chanels.Clear();
                                            indexTrue = i;
                                        }
                                    }
                                    else
                                    {
                                        chanels.Add(ch.Key);
                                        chanels.AddRange(chanelsTemps.CloneList());
                                        if (dtoTemp.Any(x => x.Chanels == null || !x.Chanels.Any()))
                                        {
                                            CreateDto(ref dtoTemp, ref chanels, ref chanel, ref slDai, ref indexTrue, ref i, ref type,
                                                ref numberCache, ref numbers, ref sl, ref result.MessageLoi[index], ref chanelsCache, true);
                                        }
                                        else if ((previous == "dai" || previousTo == "dai") && dtoTemp.Any())
                                        {
                                            var dto = dtoTemp.Last();
                                            if(dto.Chanels != null && dto.Chanels.Count == chanels.Count)
                                            {
                                                dto.Chanels = chanels.CloneList();
                                                chanels.Clear();
                                                indexTrue = i;
                                            }
                                        }
                                    }

                                }
                                else if (unknowList.Any())
                                {
                                    var itl = string.Join(" ", unknowList) + it;
                                    var chl = InnitRepository._chanelCode.FirstOrDefault(x => x.Value.Contains(itl));
                                    if (Constants.IstestMode)
                                    {
                                        chl = InnitRepository._chanelCodeForTest.FirstOrDefault(x => x.Value.Contains(itl));
                                    }
                                    if (chl.Value != null && chl.Value.Any())
                                    {
                                        if (chanels.Any() || slDai > 0 && (dtoTemp.Any(x => x.Chanels == null || !x.Chanels.Any())))
                                        {
                                            CreateDto(ref dtoTemp, ref chanels, ref chanel, ref slDai, ref indexTrue, ref i, ref type,
                                                 ref numberCache, ref numbers, ref sl, ref result.MessageLoi[index], ref chanelsCache, true);
                                        }

                                        chanels.Add(chl.Key);
                                        unknowList.Clear();
                                        unknowListGoc.Clear();
                                        CheckChanel(ref array, ref i, ref chanels);
                                        if ((previous == "dai" || previousTo == "dai") && dtoTemp.Any())
                                        {
                                            dtoTemp.Last().Chanels = chanels.CloneList();
                                            chanels.Clear();
                                        }
                                        if (type != null && sl > 0 && (numbers.Any() || numberCache.Any()))
                                        {
                                            if(!numbers.Any())
                                                numbers = numberCache.CloneList();
                                            dtoTemp.Add(new Cal1RequestDto
                                            {
                                                Chanels = chanels,
                                                LotteryType = (LotteryType)type,
                                                Numbers = numbers.CloneListStrToInt(),
                                                NumbersStr = numbers.CloneList(),
                                                Sl = sl,
                                                TileBaso = syntaxes.TileBaso,
                                                TileXac = syntaxes.TileXac,
                                                TileThuong = syntaxes.TileThuong
                                            });
                                            indexTrue = i;
                                            type = null;
                                            chanels.Clear();
                                            numberCache = numbers.CloneList();
                                            numbers.Clear();
                                            sl = 0;
                                        }

                                        if (dtoTemp.Any(x => x.Chanels == null || !x.Chanels.Any()))
                                        {
                                            CreateDto(ref dtoTemp, ref chanels, ref chanel, ref slDai, ref indexTrue, ref i, ref type, ref numberCache, 
                                                ref numbers, ref sl, ref result.MessageLoi[index], ref chanelsCache, true);
                                        }
                                    }
                                    else if (CheckType(itl, ref type))
                                    {
                                        unknowList.Clear();
                                        unknowListGoc.Clear();
                                        if (array.Length > (i + 1) && type == 8 && array[i + 1] == "dao")
                                        {
                                            type = 12;
                                            i++;
                                        }
                                    }
                                    else if (CheckMien(itl, ref chanel))
                                    {
                                        unknowList.Clear();
                                        unknowListGoc.Clear();
                                    }
                                    else
                                    {
                                        unknowList.Add(it);
                                        unknowListGoc.Add(itemsGoc[indexGoc]);
                                    }
                                }
                                else
                                {
                                    unknowList.Add(it);
                                    unknowListGoc.Add(itemsGoc[indexGoc]);
                                }
                            }
                        }
                        if (type != null && sl != 0 && ((numbers.Any() && numbers.All(x => x.Length > 1)) || (numberCache.Any() && numberCache.All(x => x.Length > 1))))
                        {
                            int numberTemp;
                            while (isNumberContinute && array.Length > (i + 1) && int.TryParse(array[i + 1], out numberTemp) && array[i + 1].Length > 1)
                            {
                                numbers.Add(array[i + 1]);
                                i++;
                            }
                            if (!numbers.Any())
                            {
                                numbers = numberCache.CloneList();
                            }
                            if (type != null && type < 5)
                            {
                                if (type == 0 && numbers.All(x => x.Length == 3))
                                    type = 5;
                                else if (type == 1)
                                    type = 1;
                                else if (numbers.All(x => x.Length == 3))
                                    type += 4;
                                else if (numbers.All(x => x.Length == 4))
                                    type = 9;
                            }
                            dtoTemp.Add(new Cal1RequestDto
                            {
                                LotteryType = (LotteryType)type,
                                Numbers = numbers.CloneListStrToInt(),
                                NumbersStr = numbers.CloneList(),
                                Sl = sl,
                                TileBaso = syntaxes.TileBaso,
                                TileXac = syntaxes.TileXac,
                                TileThuong = syntaxes.TileThuong
                            });
                            indexTrue = i;
                            type = null;
                            numberCache = numbers.CloneList();
                            numbers.Clear();
                            sl = 0;
                        }


                        previous = array[i];
                        if(i > 0)
                            previousTo = array[i - 1];
                        if (i == (array.Length - 1))
                        {
                            var sslDai = slDai == 0 ? 1 : slDai;
                            if (!chanels.Any() && dtoTemp.Any(x => x.Chanels == null || !x.Chanels.Any()))
                            {
                                if (chanel == "hn")
                                {
                                    for (int j = 0; j < sslDai; j++)
                                        chanels.Add(8 + j);
                                }
                                else if (chanel == "mt")
                                    for (int j = 0; j < sslDai; j++)
                                        chanels.Add(5 + j);
                                else if (string.IsNullOrEmpty(chanel) && slDai == 0 && chanelsCache.Any())
                                {
                                    chanels = chanelsCache.CloneList();
                                }
                                else if (chanel == "mn" || string.IsNullOrEmpty(chanel))
                                    for (int j = 0; j < sslDai; j++)
                                        chanels.Add(1 + j);
                                indexTrue = i;
                            }
                            if (i > indexTrue)
                            {
                                string thua = "";
                                for (int j = indexTrue + 1; j <= i; j++)
                                    thua = thua + " " + arrayGoc[j];
                                result.MessageLoi[index] = $"Không hiểu : {thua}";
                                FileHelper.GeneratorFileByDay(FileStype.Error, $"Không hiểu : {thua}. sys : {sys}" , "Cal2Request");
                                break;
                            }
                            if (unknowListGoc.Any())
                            {
                                result.MessageLoi[index] = $"Không hiểu : {string.Join(",", unknowListGoc)}";
                                FileHelper.GeneratorFileByDay(FileStype.Error, $"Không hiểu : {string.Join(",", unknowListGoc)}. sys : {sys}", "Cal2Request");
                                break;
                            }
                            if (!dtoTemp.Any())
                            {
                                result.MessageLoi[index] = "Không phân tích được ";
                                FileHelper.GeneratorFileByDay(FileStype.Error, $"Không phân tích được sys : {sys}", "Cal2Request");
                                break;
                            }
                        }
                    }
                    foreach (var dto in dtoTemp)
                    {
                        if (string.IsNullOrEmpty(result.MessageLoi[index]))
                        {
                            if (dto.Chanels == null && chanels.Any())
                            {
                                dto.Chanels = chanels;
                                if (dto.LotteryType == LotteryType.DaXien && chanels.Count == 1)
                                {
                                    dto.Chanels.Add(chanels.First() + 1);
                                }
                            }
                            var er1 = CheckChanelValid(dto.Chanels, chanel, (int)dto.LotteryType);
                            if (!string.IsNullOrEmpty(er1))
                            {
                                result.MessageLoi[index] = er1;
                                break;
                            }
                            var error = CheckErrorTypeAndNumber(dto.LotteryType, dto.NumbersStr, dto.Chanels);
                            if (!string.IsNullOrEmpty(error))
                            {
                                result.MessageLoi[index] = error;
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(result.MessageLoi[index]))
                    {
                        dtos.Clear();
                        break;
                    }
                    dtos.AddRange(dtoTemp);
                    dtoTemp.Clear();
                }

                foreach (var dto in dtos)
                {
                    dto.DaThang = syntaxes.DaThang == 0 ? 650 : syntaxes.DaThang;
                    dto.DaXien = syntaxes.DaXien == 0 ? 550 : syntaxes.DaXien;
                    dto.BonSo = syntaxes.BonSo == 0 ? 5500 : syntaxes.BonSo;
                }
                foreach (var dto in dtos)
                {
                    var (xac, mess) = CalXac(dto);
                    result.Xac += xac;
                    if (!string.IsNullOrEmpty(mess))
                        result.MessageXac.Add(mess);
                }
                foreach (var dto in dtos)
                {
                    var (thuong, mes) = CalThuong(dto);
                    result.Thuong += thuong;
                    result.MessageThuong.AddRange(mes);
                }
                result.Loi = result.Thuong - result.Xac;
                result.Xac = Math.Round(result.Xac, 1);
                result.Thuong = Math.Round(result.Thuong, 1);
                result.Loi = Math.Round(result.Loi, 1);
                res.Data = result;

                return res;
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString() + $"Boby : {JsonConvert.SerializeObject(syntaxes)}", "Cal2Request");
                return new ResponseBase { Code = 500, Message = ex.ToString() };
            }

        }
        public void CheckChanel(ref string[] array, ref int i, ref List<int> chanels)
        {
            bool isDai = true;
            while (isDai)
            {
                if (array.Length > (i + 1) && (array[i + 1] == "voi" || array[i + 1] == "va" || array[i + 1] == "dai"))
                {
                    i++;
                }
                else if (array.Length > (i + 1))
                {
                    isDai = false;
                    var itll = array[i + 1];
                    var chll = InnitRepository._chanelCode.FirstOrDefault(x => x.Value.Contains(itll));
                    if (Constants.IstestMode)
                    {
                        chll = InnitRepository._chanelCodeForTest.FirstOrDefault(x => x.Value.Contains(itll));
                    }
                    if (chll.Value != null && chll.Value.Any())
                    {
                        chanels.Add(chll.Key);
                        i = i + 1;
                        isDai = true;
                    }
                    else if (array.Length > (i + 2))
                    {
                        var itlll = array[i + 1] + array[i + 2];
                        var chlll = InnitRepository._chanelCode.FirstOrDefault(x => x.Value.Contains(itlll));
                        if (Constants.IstestMode)
                        {
                            chlll = InnitRepository._chanelCodeForTest.FirstOrDefault(x => x.Value.Contains(itlll));
                        }
                        if (chlll.Value != null && chlll.Value.Any())
                        {
                            chanels.Add(chlll.Key);
                            i = i + 2;
                            isDai = true;
                        }
                    }
                }
                else
                    isDai = false;
            }
        }
        public bool CheckType(string str, ref int? type)
        {
            bool result = true;
            if (str == "xiuchu" || str == "xiuc")
                type = 8;
            else if (str == "bonso")
                type = 9;
            else
                result = false;
            return result;
        }
        public bool CheckMien(string str, ref string chanel)
        {
            bool result = true;
            if (str == "mientrung" || str == "mien tr")
                chanel = "mt";
            else if (str == "miennam")
                chanel = "mn";
            else if (str == "mienbac")
                chanel = "mb";
            else
                result = false;
            return result;
        }
        public void CreateDto(ref List<Cal1RequestDto> dtoTemp, ref List<int> chanels, ref string chanel, ref int slDai, ref int indexTrue, ref int i, ref int? type,
             ref List<string> numberCache, ref List<string> numbers, ref int sl, ref string message, ref List<int> chanelsCache, bool isIndex = false)
        {
            var chanTemp = chanel;
            var slDaiTemp = slDai;
            if (!chanels.Any())
            {
                if (chanel == "hn")
                    chanels.Add(8);
                else if (chanel == "mt")
                    for (int j = 0; j < slDai; j++)
                        chanels.Add(5 + j);
                else if (chanel == "mn" || string.IsNullOrEmpty(chanel))
                    for (int j = 0; j < slDai; j++)
                        chanels.Add(1 + j);
                //chanel = string.Empty;
                slDai = 0;
                indexTrue = i;
            }
            else if (isIndex)
                indexTrue = i;

            var me = CheckChanelValid(chanels, chanTemp, type, slDaiTemp);
            if (!string.IsNullOrEmpty(me) && string.IsNullOrEmpty(message))
                message = me;
            foreach (var dto in dtoTemp)
            {
                if (dto.Chanels == null || !dto.Chanels.Any())
                    dto.Chanels = chanels.CloneList();
            }
            type = null;
            chanelsCache = chanels.CloneList();
            chanels.Clear();
            chanel = "";
            //numberCache.Clear();
            numbers.Clear();
            sl = 0;
        }
        public bool CheckDaiXien(int[] slDai)
        {
            if (slDai.Length == 1
                || (slDai[0] < 5 && slDai[1] < 5)
                || (slDai[0] >= 5 && slDai[1] >= 5 && slDai[0] != 8 && slDai[1] != 8))
                return true;

            return false;
        }
        public string CheckChanelValid(List<int> chanels, string chanel, int? type, int? slDai = null)
        {
            if (string.IsNullOrEmpty(chanel))
                chanel = "mn";
            string error = "";
            var chs = InnitRepository._chanelCode.Select(x => x.Key).ToList();
            if (Constants.IstestMode)
                chs = InnitRepository._chanelCodeForTest.Select(x => x.Key).ToList();
            if ((chanel == "mb" || chanel == "hn") && (type == 10 || chanels.Count > 1 || (slDai != null && slDai > 1)))
            {
                string add = "";
                if (type == 10)
                    add = " Không thể đá xiên 2 đài miền bắc.";
                error = $"Miền bắc chỉ có 1 đài.{add}";
            }
            else if(type == 10 && chanels.Count != 2)
            {
                error = $"Phải đá xiên 2 đài";
            }
            else if (chanel == "mn" && chanels.Count > 3)
            {
                var count = chs.Count(x => x == 1 || x == 2 || x == 3 || x == 4);
                if (chanels.Count > count)
                    error = $"Miền nam hôm nay chỉ có {count} đài";
            }
            else if (chanel == "mt" && chanels.Count > 2)
            {
                var count = chs.Count(x => x == 5 || x == 6 || x == 7);
                if (chanels.Count > count)
                    error = $"Miền trung hôm nay chỉ có {count} đài";
            }
            else
            {
                var check = chanels.All(x => chs.Contains(x));
                if (!check)
                    error = "Danh sách đài không thỏa mãn";
            }

            return error;
        }
        public string CheckErrorTypeAndNumber(LotteryType type, List<string> numbers, List<int> slDai)
        {
            string error = "";
            switch (type)
            {
                case LotteryType.Lo:
                case LotteryType.LoDau:
                case LotteryType.LoDuoi:
                case LotteryType.LoDauDuoi:
                    if (!numbers.All(x => x.Length == 2))
                    {
                        var lst = numbers.Where(x => x.Length != 2).ToList();
                        error = $"{string.Join(",", lst)} : Không thể đánh lô";
                    }
                    break;
                case LotteryType.Xien:
                    if (!numbers.All(x => x.Length == 2))
                    {
                        var lst = numbers.Where(x => x.Length != 2).ToList();
                        error = $"{string.Join(",", lst)} : Không thể đá xiên";
                    }
                    else if (slDai.Count > 2)
                    {
                        error = $"Không thể đá xiên quá 2 đài";
                    }
                    else if (!CheckDaiXien(slDai.ToArray()))
                    {
                        error = $"Không thể đá xiên 2 đài khác miền";
                    }
                    break;
                case LotteryType.BaoBaCang:
                case LotteryType.BaCangDau:
                case LotteryType.BaCangDuoi:
                case LotteryType.BaCangDauDuoi:
                case LotteryType.XCDauDao:
                case LotteryType.XCDuoiDao:
                case LotteryType.XCDao:
                    if (!numbers.All(x => x.Length == 3))
                    {
                        var lst = numbers.Where(x => x.Length != 3).ToList();
                        error = $"{string.Join(",", lst)} : Không thể đánh 3 số";
                    }
                    break;
                case LotteryType.BaoBonSo:
                    if (!numbers.All(x => x.Length == 4))
                    {
                        var lst = numbers.Where(x => x.Length != 2).ToList();
                        error = $"{string.Join(",", lst)} : Không thể đánh 4 số";
                    }
                    break;
                case LotteryType.BaoDao:
                    if (!numbers.All(x => x.Length == 3))
                    {
                        var lst = numbers.Where(x => x.Length != 3).ToList();
                        error = $"{string.Join(",", lst)} : Không thể bao đảo";
                    }
                    break;
                default:
                    break;
            }
            return error;
        }
        public int? GetType(string it, string[] array, ref int i)
        {
            int? type = null;
            if (it == "b" || it == "bl" || it == "bao" || it == "baolo" || it == "blo")
            {
                type = 0;
            }
            else if (it == "dx")
            {
                type = 10;
            }
            else if (it == "dv" || it == "da" || it == "dav")
            {
                type = 1;
            }
            else if (it == "d" || it == "dau")
            {
                type = 2;
                if (array.Length > (i + 1) && array[i + 1] == "dao")
                {
                    i++;
                    type = 13;
                }
            }
            else if (it == "duoi")
            {
                type = 3;
                if (array.Length > (i + 1) && array[i + 1] == "dao")
                {
                    i++;
                    type = 14;
                }
            }
            else if (it == "dd")
            {
                type = 4;
            }
            else if (it == "xc" || it == "xchu" || it == "xiuchu")
            {
                type = 8;
                if(array.Length > (i + 1) && array[i + 1] == "dau")
                {
                    i++;
                    type = 6;
                }
                if (array.Length > (i + 1) && array[i + 1] == "duoi")
                {
                    i++;
                    type = 7;
                }
            }
            else if (it == "bd" || it == "daolo" || it == "baodao")
            {
                type = 11;
            }

            if (type == 1 && array.Length > (i + 1) && (array[i + 1] == "vong" || array[i + 1] == "v" || array[i + 1] == "thang"))
            {
                i++;
            }
            else if (type == 0 && array.Length > (i + 1) && array[i + 1] == "dao")
            {
                i++;
                type = 11;
            }
            else if (array.Length > (i + 1) && array[i] == "da" && (array[i + 1] == "xien" || array[i + 1] == "x"))
            {
                type = 10;
                i++;
            }
            else if (type == 2 && array.Length > (i + 1) && (array[i + 1] == "duoi" || array[i + 1] == "d"))
            {
                i++;
                type = 4;
            }
            else if (type == 6 && array.Length > (i + 1) && array[i + 1] == "dao")
            {
                i++;
                type = 13;
            }
            else if (type == 7 && array.Length > (i + 1) && array[i + 1] == "dao")
            {
                i++;
                type = 14;
            }
            else if (type == 8 && array.Length > (i + 1) && array[i + 1] == "dao")
            {
                i++;
                type = 12;
            }

            return type;
        }
        public ResponseBase Cal1Request(List<Cal1RequestDto> dtos)
        {
            try
            {
                var res = new ResponseBase();
                Cal1ResponseDto result = new Cal1ResponseDto { Message = new List<string>() };
                foreach (var dto in dtos)
                {
                    var (xac, mess) = CalXac(dto);
                    result.Xac += xac;
                }
                foreach (var dto in dtos)
                {
                    var (thuong, mes) = CalThuong(dto);
                    result.Thuong += thuong;
                    result.Message.AddRange(mes);
                }
                result.Loi = result.Thuong - result.Xac;
                result.Xac = Math.Round(result.Xac, 1);
                result.Thuong = Math.Round(result.Thuong, 1);
                result.Loi = Math.Round(result.Loi, 1);
                res.Data = result;

                return res;
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString() + $"Boby : {JsonConvert.SerializeObject(dtos)}", "Cal1Request");
                return new ResponseBase { Code = 500, Message = ex.ToString() };
            }
        }
        // Tính tiền xác
        public (double, string) CalXac(Cal1RequestDto dto)
        {
            var typeChanel = "mt";
            if (dto.Chanels.First() == 8)
                typeChanel = "mb";
            else if (dto.Chanels.First() < 5)
                typeChanel = "mn";
            switch (dto.LotteryType)
            {
                case LotteryType.Lo:
                    var xacLo = CalXacLo(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messLo = $"Lô [{string.Join(",", dto.Numbers.Select(x => x.ToString("00")))}]. {dto.Sl} ngìn. {dto.Chanels.Count} đài {typeChanel}" +
                        $" ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacLo, 1)}";
                    return (xacLo, messLo);
                case LotteryType.Xien:
                case LotteryType.DaXien:
                    int chanel1 = 0;
                    int chanel2 = 0;
                    if (dto.Chanels.Count == 1)
                        chanel1 = dto.Chanels[0];
                    else if (dto.Chanels.Count == 2)
                    {
                        chanel1 = dto.Chanels[0];
                        chanel2 = dto.Chanels[1];
                    }
                    var sl = (dto.Numbers.Count * (dto.Numbers.Count - 1)) / 2;
                    var xacXien = CalXacXien(chanel1, chanel2, sl * dto.Sl, dto.TileXac);
                    var ttype = dto.Chanels.Count == 1 ? "Đá thẳng" : "Đá xiên";
                    var messXien = $"{ttype} [{string.Join(",", dto.Numbers.Select(x => x.ToString("00")))}]. {dto.Sl} ngìn. {dto.Chanels.Count} đài {typeChanel} " +
                        $"({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacXien, 1)}";
                    return (xacXien, messXien);
                case LotteryType.LoDau:
                    var xacLobd = CalXacLoBaoDau(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messLobd = $"Lô đầu [{string.Join(",", dto.Numbers.Select(x => x.ToString("00")))}]. {dto.Sl} ngìn. {dto.Chanels.Count} đài {typeChanel}" +
                        $" ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacLobd, 1)}";
                    return (xacLobd, messLobd);
                case LotteryType.LoDuoi:
                    var xacLod = CalXacLoBaoDuoi(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messLod = $"Lô đuôi [{string.Join(",", dto.Numbers.Select(x => x.ToString("00")))}]. {dto.Sl} ngìn. {dto.Chanels.Count} đài {typeChanel}" +
                        $" ( {string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacLod, 1)}";
                    return (xacLod, messLod);
                case LotteryType.LoDauDuoi:
                    var xacLodd = CalXacLoBaoDauDuoi(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messLodd = $"Lô đầu đuôi [{string.Join(",", dto.Numbers.Select(x => x.ToString("00")))}]. {dto.Sl} ngìn. {dto.Chanels.Count} đài {typeChanel}" +
                        $" ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacLodd, 1)}";
                    return (xacLodd, messLodd);
                case LotteryType.BaoBaCang:
                    var xacbaoba = CalXacBaoBaCang(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messbaoba = $"Bao ba số [{string.Join(",", dto.Numbers.Select(x => x.ToString("000")))}]. " +
                        $"{dto.Sl} ngìn. {dto.Chanels.Count} đài {typeChanel} ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacbaoba, 1)}";
                    return (xacbaoba, messbaoba);
                case LotteryType.BaCangDau:
                    var xacbad = CalXacBaCangBaoDau(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messbad = $"XC đầu [{string.Join(",", dto.Numbers.Select(x => x.ToString("000")))}]. " +
                        $"{dto.Sl} ngìn. {dto.Chanels.Count} đài {typeChanel} ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacbad, 1)}";
                    return (xacbad, messbad);
                case LotteryType.BaCangDuoi:
                    var xacbadu = CalXacBaCangBaoDuoi(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messbadu = $"XC đuôi [{string.Join(",", dto.Numbers.Select(x => x.ToString("000")))}]. {dto.Sl} ngìn. " +
                        $"{dto.Chanels.Count} đài {typeChanel} ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacbadu, 1)}";
                    return (xacbadu, messbadu);
                case LotteryType.BaCangDauDuoi:
                    var xacxc = CalXacBaCangBaoDauDuoi(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messxc = $"Xỉu chủ [{string.Join(",", dto.Numbers.Select(x => x.ToString("000")))}]. {dto.Sl} ngìn. " +
                        $"{dto.Chanels.Count} đài {typeChanel} ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacxc, 1)}";
                    return (xacxc, messxc);
                case LotteryType.BaoBonSo:
                    var xacBon = CalXacBonSo(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messxbon = $"Bốn số [{string.Join(",", dto.Numbers.Select(x => x.ToString("000")))}]. {dto.Sl} ngìn. " +
                        $"{dto.Chanels.Count} đài {typeChanel} ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacBon, 1)}";
                    return (xacBon, messxbon);
                case LotteryType.BaoDao:
                    var xacBaoDao = CalXacBaoDao(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messBaoDao = $"Bao Đảo [{string.Join(",", dto.Numbers.Select(x => x.ToString("000")))}]. {dto.Sl} ngìn. " +
                        $"{dto.Chanels.Count} đài {typeChanel} ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacBaoDao, 1)}";
                    return (xacBaoDao, messBaoDao);
                case LotteryType.XCDao:
                    var xacxcDao = CalXacXiuChuDao(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messxcDao = $"Xỉu chủ đảo [{string.Join(",", dto.Numbers.Select(x => x.ToString("000")))}]. {dto.Sl} ngìn. " +
                        $"{dto.Chanels.Count} đài {typeChanel} ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xacxcDao, 1)}";
                    return (xacxcDao, messxcDao);
                case LotteryType.XCDauDao:
                    var xcdauDao = CalXacXiuChuDauDao(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messxcDauDao = $"Xỉu chủ đầu đảo [{string.Join(",", dto.Numbers.Select(x => x.ToString("000")))}]. {dto.Sl} ngìn. " +
                        $"{dto.Chanels.Count} đài {typeChanel} ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xcdauDao, 1)}";
                    return (xcdauDao, messxcDauDao);
                case LotteryType.XCDuoiDao:
                    var xcduoiDao = CalXacXiuChuDuoiDao(dto.Chanels, dto.Numbers, dto.Sl, dto.TileXac);
                    var messxcDuoiDao = $"Xỉu chủ đuôi đảo [{string.Join(",", dto.Numbers.Select(x => x.ToString("000")))}]. {dto.Sl} ngìn. " +
                        $"{dto.Chanels.Count} đài {typeChanel} ({string.Join(",", dto.Chanels.ChanelIntToString())}). Xác : {Math.Round(xcduoiDao, 1)}";
                    return (xcduoiDao, messxcDuoiDao);
                default:
                    return (0, "");
            }

        }
        public double CalXacLo(List<int> chanels, List<int> lo, int sl, double tile)
        {
            int mucXac = 0;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 0)
                    continue;

                if (chanel == 8)
                {
                    mucXac = 27;
                }
                else
                {
                    mucXac = 18;
                }
                total += mucXac * lo.Count * sl * tile;
            }

            return total;
        }
        public double CalXacXien(int chanel1, int chanel2, int sl, double tile)
        {
            int mucXac = 18;
            int slChanel = chanel2 == 0 ? 1 : 2;
            if (chanel1 == 8 && chanel2 == 0)
            {
                mucXac = 27;
            }


            return mucXac * slChanel * 2 * sl * tile;
        }
        public double CalXacLoBaoDau(List<int> chanels, List<int> lo, int sl, double tile)
        {
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 0)
                    continue;

                total += lo.Count * sl * tile;
            }


            return total;
        }
        public double CalXacLoBaoDuoi(List<int> chanels, List<int> lo, int sl, double tile)
        {
            int soLuong = 1;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 0)
                    continue;
                if (chanel == 8)
                    soLuong = 4;

                total += soLuong * lo.Count * sl * tile;
            }


            return total;
        }
        public double CalXacLoBaoDauDuoi(List<int> chanels, List<int> lo, int sl, double tile)
        {
            int soLuong = 2;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                if (chanel == 8)
                    soLuong = 5;

                total += soLuong * lo.Count * sl * tile;
            }

            return total;
        }
        public double CalXacBaoBaCang(List<int> chanels, List<int> baCang, int sl, double tile)
        {
            int mucXac = 0;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 0)
                    continue;

                if (chanel == 8)
                {
                    mucXac = 23;
                }
                else
                {
                    mucXac = 17;
                }
                total += mucXac * baCang.Count * sl * tile;
            }

            return total;
        }
        public double CalXacBaCangBaoDau(List<int> chanels, List<int> baCang, int sl, double tile)
        {
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 0)
                    continue;

                total += baCang.Count * sl * tile;
            }


            return total;
        }
        public double CalXacBaCangBaoDuoi(List<int> chanels, List<int> baCang, int sl, double tile)
        {
            int soLuong = 1;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 0)
                    continue;
                if (chanel == 8)
                    soLuong = 3;

                total += soLuong * baCang.Count * sl * tile;
            }


            return total;
        }
        public double CalXacBaCangBaoDauDuoi(List<int> chanels, List<int> baCang, int sl, double tile)
        {
            int soLuong = 2;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                if (chanel == 8)
                    soLuong = 4;

                total += soLuong * baCang.Count * sl * tile;
            }

            return total;
        }
        public double CalXacBonSo(List<int> chanels, List<int> bonSo, int sl, double tile)
        {
            int soLuong = 16;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                if (chanel == 8)
                    soLuong = 20;

                total += soLuong * bonSo.Count * sl * tile;
            }

            return total;
        }
        public double CalXacBaoDao(List<int> chanels, List<int> baso, int sl, double tile)
        {
            int soLuong = 17;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                if (chanel == 8)
                    soLuong = 23;

                total += soLuong * baso.Count * sl * tile * 6;
            }

            return total;
        }
        public double CalXacXiuChuDao(List<int> chanels, List<int> baso, int sl, double tile)
        {
            int soLuong = 2;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                if (chanel == 8)
                    soLuong = 4;

                total += soLuong * baso.Count * sl * tile * 6;
            }

            return total;
        }
        public double CalXacXiuChuDauDao(List<int> chanels, List<int> baso, int sl, double tile)
        {
            int soLuong = 1;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                if (chanel == 8)
                    soLuong = 1;

                total += soLuong * baso.Count * sl * tile * 6;
            }

            return total;
        }
        public double CalXacXiuChuDuoiDao(List<int> chanels, List<int> baso, int sl, double tile)
        {
            int soLuong = 1;
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                if (chanel == 8)
                    soLuong = 3;

                total += soLuong * baso.Count * sl * tile * 6;
            }

            return total;
        }
        // Tính tiền thưởng
        public (double, List<string>) CalThuong(Cal1RequestDto dto)
        {
            switch (dto.LotteryType)
            {
                case LotteryType.Lo:
                    return CalThuongLo(dto.Chanels, dto.Numbers, dto.Sl, dto.TileThuong);
                case LotteryType.Xien:
                    var chal1 = dto.Chanels[0];
                    var chal2 = 0;
                    if (dto.Chanels.Count == 2)
                    {
                        chal2 = dto.Chanels[1];
                    }
                    double totals = 0;
                    List<string> message = new List<string>();
                    for (int i = 0; i < dto.Numbers.Count; i++)
                    {
                        for (int j = i + 1; j < dto.Numbers.Count; j++)
                        {
                            var (total, mes) = CalThuongXien(chal1, chal2, dto.Sl, dto.Numbers[i], dto.Numbers[j], dto.DaThang, dto.DaXien);
                            totals += total;
                            message.AddRange(mes);
                        }
                    }

                    return (totals, message);

                case LotteryType.LoDau:
                    return CalThuongLoBaoDau(dto.Chanels, dto.Numbers, dto.Sl, dto.TileThuong);
                case LotteryType.LoDuoi:
                    return CalThuongBaoDuoi(dto.Chanels, dto.Numbers, dto.Sl, dto.TileThuong);
                case LotteryType.LoDauDuoi:
                    return CalThuongBaoDauDuoi(dto.Chanels, dto.Numbers, dto.Sl, dto.TileThuong);
                case LotteryType.BaoBaCang:
                    return CalThuongBaoBaCang(dto.Chanels, dto.Numbers, dto.Sl, dto.TileBaso);
                case LotteryType.BaCangDau:
                    return CalThuongBaCangBaoDau(dto.Chanels, dto.Numbers, dto.Sl, dto.TileBaso);
                case LotteryType.BaCangDuoi:
                    return CalThuongBaCangDuoi(dto.Chanels, dto.Numbers, dto.Sl, dto.TileBaso);
                case LotteryType.BaCangDauDuoi:
                    return CalThuongBaCangDauDuoi(dto.Chanels, dto.Numbers, dto.Sl, dto.TileBaso);
                case LotteryType.BaoBonSo:
                    return CalThuongBonSo(dto.Chanels, dto.Numbers, dto.Sl, dto.BonSo);
                case LotteryType.BaoDao:
                    return CalThuongBaoDao(dto.Chanels, dto.Numbers, dto.Sl, dto.TileBaso);
                case LotteryType.XCDauDao:
                    return CalThuongXCBaoDau(dto.Chanels, dto.Numbers, dto.Sl, dto.TileBaso);
                case LotteryType.XCDuoiDao:
                    return CalThuongXCDuoiDao(dto.Chanels, dto.Numbers, dto.Sl, dto.TileBaso);
                case LotteryType.XCDao:
                    return CalThuongXCDao(dto.Chanels, dto.Numbers, dto.Sl, dto.TileBaso);
                    
                default:
                    return (0, new List<string>());
            }

        }
        public (double, List<string>) CalThuongLo(List<int> chanels, List<int> los, int sl, double tile)
        {
            List<string> message = new List<string>();
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                foreach (var lo in los)
                {

                    var count = InnitRepository._totalDic["Now"][chanel - 1].Count(x => x == lo);
                    if (count > 0)
                    {
                        var chung = count * sl * tile;
                        var mes = $"Lô {lo.ToString("00")} đài {chan} ăn {count}x{sl}={count * sl} điểm. Chung {chung} đ";
                        message.Add(mes);
                        total += chung;
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongXien(int chanel1, int chanel2, int sl, int lo1, int lo2, double dathang, double daxien)
        {
            List<string> message = new List<string>();
            double mucThuong = daxien;
            List<int> lst = new List<int>();
            if (chanel1 > 0 && chanel2 == 0)
            {
                mucThuong = dathang;
                lst = InnitRepository._totalDic["Now"][chanel1 - 1];
            }
            else
            {
                lst = TowListTo1(InnitRepository._totalDic["Now"][chanel1 - 1], InnitRepository._totalDic["Now"][chanel2 - 1]);
            }

            var count1 = lst.Count(x => x == lo1);
            var count2 = lst.Count(x => x == lo2);
            var min = 0;
            if (lo1 == lo2)
            {
                min = count1 / 2;
            }
            else
            {
                min = count1 > count2 ? count2 : count1;
            }
            var chung = min * mucThuong * sl;
            if (min > 0)
            {
                var mes = $"Ăn xiên {lo1.ToString("00")}-{lo2.ToString("00")} {min}x{sl}={min * sl} điểm. Chung {chung} đ";
                message.Add(mes);

            }
            return (chung, message);
        }
        public (double, List<string>) CalThuongLoBaoDau(List<int> chanels, List<int> los, int sl, double tile)
        {
            double total = 0;
            List<string> message = new List<string>();
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                foreach (var lo in los)
                {
                    var dau = InnitRepository._totalDic["Now"][chanel - 1].FirstOrDefault();
                    if (dau == lo)
                    {
                        var chung = sl * tile;
                        var mes = $"Lô bao đầu {lo.ToString("00")} đài {chan} ăn {sl} điểm. Chung {chung} đ";
                        message.Add(mes);
                        total += chung;
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongBaoDuoi(List<int> chanels, List<int> los, int sl, double tile)
        {
            double total = 0;
            List<string> message = new List<string>();
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                List<int> lst = new List<int>();
                if (chanel == 8)
                {
                    lst = InnitRepository._totalDic["Now"][chanel - 1].GetRange(24, 3);
                }
                else
                {
                    lst.Add(InnitRepository._totalDic["Now"][chanel - 1].LastOrDefault());
                }
                foreach (var lo in los)
                {
                    var count = lst.Count(x => x == lo);
                    if (count > 0)
                    {
                        var chung = sl * tile * count;
                        var mes = $"Lô bao đuôi {lo.ToString("00")} đài {chan} ăn {sl}x{count}={sl * count} điểm. Chung {chung} đ";
                        message.Add(mes);
                        total += chung;
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongBaoDauDuoi(List<int> chanels, List<int> los, int sl, double tile)
        {
            List<string> message = new List<string>();
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                List<int> lst = new List<int>();
                if (chanel == 8)
                {
                    lst = InnitRepository._totalDic["Now"][chanel - 1].GetRange(23, 4);
                }
                else
                {
                    lst.Add(InnitRepository._totalDic["Now"][chanel - 1].LastOrDefault());
                }
                lst.Add(InnitRepository._totalDic["Now"][chanel - 1].FirstOrDefault());
                foreach (var lo in los)
                {
                    var count = lst.Count(x => x == lo);

                    if (count > 0)
                    {
                        var chung = sl * tile * count;
                        var mes = $"Lô bao đầu đuôi {lo.ToString("00")} đài {chan} ăn {sl}x{count}={sl * count} điểm. Chung {chung} đ";
                        message.Add(mes);
                        total += chung;
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongBaoBaCang(List<int> chanels, List<int> bacangs, int sl, double tileBaCang)
        {
            tileBaCang = tileBaCang == 0 ? 650 : tileBaCang;
            List<string> message = new List<string>();
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                foreach (var ba in bacangs)
                {

                    var count = InnitRepository._totalBaCangDic["Now"][chanel - 1].Count(x => x == ba);
                    if (count > 0)
                    {
                        var chung = count * sl * tileBaCang;
                        var mes = $"Bao ba số {ba.ToString("000")} đài {chan} ăn {count}x{sl}={count * sl} điểm. Chung {chung} đ";
                        message.Add(mes);
                        total += chung;
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongBaoDao(List<int> chanels, List<int> bacangs, int sl, double tileBaCang)
        {
            tileBaCang = tileBaCang == 0 ? 650 : tileBaCang;
            List<string> message = new List<string>();
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                foreach (var ba in bacangs)
                {
                    foreach (var dao in ba.BaSoToBaoDao())
                    {
                        var count = InnitRepository._totalBaCangDic["Now"][chanel - 1].Count(x => x == dao);
                        if (count > 0)
                        {
                            var chung = count * sl * tileBaCang;
                            var mes = $"Bao Đảo {ba.ToString("000")} trúng {dao} đài {chan} ăn {count}x{sl}={count * sl} điểm. Chung {chung} đ";
                            message.Add(mes);
                            total += chung;
                        }
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongXCDao(List<int> chanels, List<int> baCangs, int sl, double tileBaCang)
        {
            tileBaCang = tileBaCang == 0 ? 650 : tileBaCang;
            List<string> message = new List<string>();
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                List<int> lst = new List<int>();
                if (chanel == 8)
                {
                    lst = InnitRepository._totalBaCangDic["Now"][chanel - 1].GetRange(20, 3);
                }
                else
                {
                    lst.Add(InnitRepository._totalBaCangDic["Now"][chanel - 1].LastOrDefault());
                }
                lst.Add(InnitRepository._totalBaCangDic["Now"][chanel - 1].FirstOrDefault());
                foreach (var ba in baCangs)
                {
                    foreach (var dao in ba.BaSoToBaoDao())
                    {

                        var count = lst.Count(x => x == dao);

                        if (count > 0)
                        {
                            var chung = sl * tileBaCang * count;
                            var mes = $"XC đảo {ba.ToString("000")}. Trúng {dao.ToString("000")} đài {chan} ăn {sl}x{count}={sl * count} điểm. Chung {chung} đ";
                            message.Add(mes);
                            total += chung;
                        }
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongXCDuoiDao(List<int> chanels, List<int> baCangs, int sl, double tileBaCang)
        {
            tileBaCang = tileBaCang == 0 ? 650 : tileBaCang;
            double total = 0;
            List<string> message = new List<string>();
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                List<int> lst = new List<int>();
                if (chanel == 8)
                {
                    lst = InnitRepository._totalBaCangDic["Now"][chanel - 1].GetRange(20, 3);
                }
                else
                {
                    lst.Add(InnitRepository._totalBaCangDic["Now"][chanel - 1].LastOrDefault());
                }
                foreach (var ba in baCangs)
                {
                    foreach (var dao in ba.BaSoToBaoDao())
                    {
                        var count = lst.Count(x => x == dao);
                        if (count > 0)
                        {
                            var chung = sl * tileBaCang * count;
                            var mes = $"XC đuôi đảo {ba.ToString("000")}. Trúng {dao.ToString("000")} đài {chan} ăn {sl}x{count}={sl * count} điểm. Chung {chung} đ";
                            message.Add(mes);
                            total += chung;
                        }
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongXCBaoDau(List<int> chanels, List<int> baCangs, int sl, double tileBaCang)
        {
            tileBaCang = tileBaCang == 0 ? 650 : tileBaCang;
            double total = 0;
            List<string> message = new List<string>();
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                foreach (var ba in baCangs)
                {
                    foreach (var dao in ba.BaSoToBaoDao())
                    {
                        var dau = InnitRepository._totalBaCangDic["Now"][chanel - 1].FirstOrDefault();
                        if (dau == dao)
                        {
                            var chung = sl * tileBaCang;
                            var mes = $"XC đầu đảo {ba.ToString("000")}. Trúng {dao.ToString("000")} đài {chan} ăn {sl} điểm. Chung {chung} đ";
                            message.Add(mes);
                            total += chung;
                        }
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongBaCangBaoDau(List<int> chanels, List<int> baCangs, int sl, double tileBaCang)
        {
            tileBaCang = tileBaCang == 0 ? 650 : tileBaCang;
            double total = 0;
            List<string> message = new List<string>();
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                foreach (var ba in baCangs)
                {
                    var dau = InnitRepository._totalBaCangDic["Now"][chanel - 1].FirstOrDefault();
                    if (dau == ba)
                    {
                        var chung = sl * tileBaCang;
                        var mes = $"XC đầu {ba.ToString("000")} đài {chan} ăn {sl} điểm. Chung {chung} đ";
                        message.Add(mes);
                        total += chung;
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongBaCangDuoi(List<int> chanels, List<int> baCangs, int sl, double tileBaCang)
        {
            tileBaCang = tileBaCang == 0 ? 650 : tileBaCang;
            double total = 0;
            List<string> message = new List<string>();
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                List<int> lst = new List<int>();
                if (chanel == 8)
                {
                    lst = InnitRepository._totalBaCangDic["Now"][chanel - 1].GetRange(20, 3);
                }
                else
                {
                    lst.Add(InnitRepository._totalBaCangDic["Now"][chanel - 1].LastOrDefault());
                }
                foreach (var ba in baCangs)
                {
                    var count = lst.Count(x => x == ba);
                    if (count > 0)
                    {
                        var chung = sl * tileBaCang * count;
                        var mes = $"XC đuôi {ba.ToString("000")} đài {chan} ăn {sl}x{count}={sl * count} điểm. Chung {chung} đ";
                        message.Add(mes);
                        total += chung;
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongBaCangDauDuoi(List<int> chanels, List<int> baCangs, int sl, double tileBaCang)
        {
            tileBaCang = tileBaCang == 0 ? 650 : tileBaCang;
            List<string> message = new List<string>();
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                List<int> lst = new List<int>();
                if (chanel == 8)
                {
                    lst = InnitRepository._totalBaCangDic["Now"][chanel - 1].GetRange(20, 3);
                }
                else
                {
                    lst.Add(InnitRepository._totalBaCangDic["Now"][chanel - 1].LastOrDefault());
                }
                lst.Add(InnitRepository._totalBaCangDic["Now"][chanel - 1].FirstOrDefault());
                foreach (var ba in baCangs)
                {
                    var count = lst.Count(x => x == ba);

                    if (count > 0)
                    {
                        var chung = sl * tileBaCang * count;
                        var mes = $"XC {ba.ToString("000")} đài {chan} ăn {sl}x{count}={sl * count} điểm. Chung {chung} đ";
                        message.Add(mes);
                        total += chung;
                    }
                }
            }

            return (total, message);
        }
        public (double, List<string>) CalThuongBonSo(List<int> chanels, List<int> bonSo, int sl, double tileBonSo)
        {
            List<string> message = new List<string>();
            double total = 0;
            foreach (var chanel in chanels)
            {
                if (chanel < 1)
                    continue;
                var chan = _currentChanels.FirstOrDefault(x => x.Key == chanel)?.Value;
                List<int> lst = new List<int>();
                lst = InnitRepository._totalBonSoDic["Now"][chanel - 1];
                foreach (var bon in bonSo)
                {
                    var count = lst.Count(x => x == bon);

                    if (count > 0)
                    {
                        var chung = sl * tileBonSo * count;
                        var mes = $"Bốn số {bon.ToString("0000")} đài {chan} ăn {sl}x{count}={sl * count} điểm. Chung {chung} đ";
                        message.Add(mes);
                        total += chung;
                    }
                }
            }

            return (total, message);
        }
        public List<int> TowListTo1(List<int> lst1, List<int> lst2)
        {
            List<int> lst = new List<int>();
            foreach (var num in lst1)
                lst.Add(num);
            foreach (var num in lst2)
                lst.Add(num);

            return lst;
        }
    }
}
