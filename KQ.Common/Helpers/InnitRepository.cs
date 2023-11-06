using KQ.Common.Enums;
using KQ.Common.Repository;
using KQ.DataAccess.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Timers;

namespace KQ.Common.Helpers
{
    public class InnitRepository
    {
        static double _time = 120000;
        static bool checkS = false;
        static bool checkN = false;
        static bool checkT = false;
        static bool checkB = false;
        static System.Timers.Timer aTimer = new System.Timers.Timer();
        private static IWebDriver drivers;
        public static ConcurrentDictionary<string, List<int>[]> _totalDic;
        public static ConcurrentDictionary<string, List<int>[]> _totalBaCangDic;
        public static ConcurrentDictionary<string, List<int>[]> _totalBonSoDic;
        public static ConcurrentDictionary<int, List<string>> _chanelCodeForTest;
        public static ConcurrentDictionary<DayOfWeek, Dictionary<MienEnum, Dictionary<int, List<string>>>> _chanelCodeAll;
        private static void GetCurrentChanelCodeAll()
        {
            _chanelCodeAll = CommonFunction.GetCurrentChanelCodeAll();
        }
        private static void UpdateChanelCodeForTest()
        {
            _chanelCodeForTest = new ConcurrentDictionary<int, List<string>>();
            _chanelCodeForTest.TryAdd(1, new List<string> { "Vĩnh Long", "VinhLong", "vinhlong", "Vinh Long", "vlong", "vlong", "vl" });
            _chanelCodeForTest.TryAdd(2, new List<string> { "Bình Dương", "BinhDuong", "binhduong", "Binh Duong", "bduong", "binhDuong", "bd" });
            _chanelCodeForTest.TryAdd(3, new List<string> { "TPHCM", "TP", "tp", "tphcm" });
            _chanelCodeForTest.TryAdd(5, new List<string> { "Khánh Hòa", "Khanh Hoa", "khanh hoa", "khanhhoa", "kh" });
            _chanelCodeForTest.TryAdd(6, new List<string> { "Kom Tum", "komtum", "kt", "kom tum" });
            _chanelCodeForTest.TryAdd(7, new List<string> { "Huế", "hue", "h" });
            _chanelCodeForTest.TryAdd(8, new List<string> { "Miền bắc", "MB" });
        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;
                var nowT = DateTime.Now;
                if (now < new TimeSpan(0, 3, 0))
                {
                    checkS = false;
                    checkN = false;
                    checkT = false;
                    checkB = false;
                    StoreKQRepository.DeleteDetails();
                }
                else if (!checkS && now < new TimeSpan(6, 30, 0))
                {
                    var haicon = JsonConvert.SerializeObject(_totalDic["Now"]);
                    var bacon = JsonConvert.SerializeObject(_totalBaCangDic["Now"]);
                    var boncon = JsonConvert.SerializeObject(_totalBonSoDic["Now"]);
                    try
                    {
                        checkS = StoreKQRepository.InsertStoreKQ(DateTime.Now.AddDays(-1), haicon, bacon, boncon);
                    }
                    catch (Exception ex)
                    {
                        checkS = false;
                        FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "StartDate");
                    }

                    if (now > new TimeSpan(6, 30, 0) && !checkS)
                    {
                        //Gửi tin nhắn zalo
                    }
                    else if (checkS)
                    {
                        _totalDic.Clear();
                        _totalDic.TryAdd("Now", new List<int>[8]);
                        _totalBaCangDic.Clear();
                        _totalBaCangDic.TryAdd("Now", new List<int>[8]);
                        _totalBonSoDic.Clear();
                        _totalBonSoDic.TryAdd("Now", new List<int>[8]);
                    }
                }
                else if (!checkN && now > new TimeSpan(16, 31, 0) && now <= new TimeSpan(19, 0, 0))
                {
                    Stopwatch s1 = new Stopwatch();
                    s1.Start();
                    int countCheck = 0;
                    string dai = "";
                    InitDriver();
                    // Cập nhật đài miền nam
                    try
                    {
                        dai = "Minh Ngọc";
                        lock (_totalDic)
                        {
                            while (!checkN && countCheck < 2)
                            {
                                checkN = UpdateKQMN(nowT.DayOfWeek, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                countCheck++;
                                if (!checkN && countCheck < 3)
                                    Thread.Sleep(5000);
                            }
                        }
                        if (!checkN && now > new TimeSpan(16, 45, 0))
                        {
                            countCheck = 0;
                            dai = "Đại phát";
                            lock (_totalDic)
                            {
                                while (!checkN && countCheck < 2)
                                {
                                    checkN = UpdateKQMN(nowT.DayOfWeek, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                    countCheck++;
                                    if (!checkN && countCheck < 3)
                                        Thread.Sleep(5000);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        checkN = false;
                        FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateMienNam");
                    }
                    finally
                    {
                        DisposeDriver();
                    }

                    s1.Stop();
                    if (checkN)
                        FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật dữ liệu MN lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                            $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms. Số lần thực hiện {countCheck}. Đài {dai}", "UpdateOnTime");
                }
                else if (!checkT && now > new TimeSpan(17, 31, 0) && now <= new TimeSpan(19, 0, 0))
                {
                    Stopwatch s1 = new Stopwatch();
                    s1.Start();
                    InitDriver();
                    string dai = "";
                    int countCheck = 0;
                    try
                    {
                        // Cập nhật đài miền trung
                        dai = "Minh Ngọc";
                        lock (_totalDic)
                        {
                            while (!checkT && countCheck < 2)
                            {
                                checkT = UpdateKQMT(nowT.DayOfWeek, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                countCheck++;
                                if (!checkT && countCheck < 3)
                                    Thread.Sleep(5000);
                            }
                        }
                        if (!checkT && now > new TimeSpan(17, 45, 0))
                        {
                            countCheck = 0;
                            dai = "Đại phát";
                            lock (_totalDic)
                            {
                                while (!checkT && countCheck < 3)
                                {
                                    checkT = UpdateKQMT(nowT.DayOfWeek, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                    countCheck++;
                                    if (!checkT && countCheck < 3)
                                        Thread.Sleep(5000);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        checkT = false;
                        FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateMienTrung");
                    }
                    finally
                    {
                        DisposeDriver();
                    }

                    s1.Stop();
                    if (checkT)
                        FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật dữ liệu MT lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                            $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms. Số lần thực hiện {countCheck}. Đài {dai}", "UpdateOnTime");
                }
                else if (!checkB && now > new TimeSpan(18, 31, 0) && now <= new TimeSpan(19, 0, 0))
                {
                    Stopwatch s1 = new Stopwatch();
                    s1.Start();
                    int countCheck = 0;
                    string dai = "";
                    InitDriver();
                    // Cập nhật đài miền bắc
                    try
                    {
                        dai = "Minh Ngọc";
                        lock (_totalDic)
                        {
                            while (!checkB && countCheck < 3)
                            {
                                checkB = UpdateKQMB(nowT.DayOfWeek, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                countCheck++;
                                if (!checkB && countCheck < 3)
                                    Thread.Sleep(5000);
                            }
                        }
                        if (!checkB && now > new TimeSpan(18, 45, 0))
                        {
                            countCheck = 0;
                            dai = "Đại phát";
                            lock (_totalDic)
                            {
                                while (!checkB && countCheck < 3)
                                {
                                    checkB = UpdateKQMB(nowT.DayOfWeek, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                    countCheck++;
                                    if (!checkB && countCheck < 3)
                                        Thread.Sleep(5000);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        checkB = false;
                        FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateMienBac");
                    }
                    finally
                    {
                        DisposeDriver();
                    }

                    s1.Stop();
                    if (checkB)
                        FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật dữ liệu MB lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                            $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms. Số lần thực hiện {countCheck}. Đài {dai}", "UpdateOnTime");
                }
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "OnTimedEvent");
            }

        }
        public static bool InitAllChanel()
        {
            bool isCheck = false;
            try
            {
                if (_totalDic == null)
                {
                    _totalDic = new ConcurrentDictionary<string, List<int>[]>();
                    _totalDic.TryAdd("Now", new List<int>[8]);
                }
                if (_totalBaCangDic == null)
                {
                    _totalBaCangDic = new ConcurrentDictionary<string, List<int>[]>();
                    _totalBaCangDic.TryAdd("Now", new List<int>[8]);
                }
                if (_totalBonSoDic == null)
                {
                    _totalBonSoDic = new ConcurrentDictionary<string, List<int>[]>();
                    _totalBonSoDic.TryAdd("Now", new List<int>[8]);
                }
                var now = DateTime.Now.TimeOfDay;
                if (now < new TimeSpan(18, 58, 0))
                    return true;
                InitDriver();
                isCheck = true;
                var check1 = UpdateKQ(DateTime.Now.DayOfWeek);
                //var check2 = UpdateKQ(DayOfWeek.Monday);
                return true;

            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.Message, "Init");
                return false;
            }
            finally
            {
                if(isCheck)
                    DisposeDriver();
            }

        }
        public static void Init()
        {
            try
            {
                FileHelper.GeneratorFileByDay(FileStype.Log, $"Khởi động hệ thống lúc {DateTime.Now.ToString("HH:mm:ss")}.", "Init");
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Interval = _time;
                aTimer.Enabled = true;
                GetCurrentChanelCodeAll();
                UpdateChanelCodeForTest();
                InitAllChanel();
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "Init");
            }
        }
        private static bool UpdateKQ(DayOfWeek? day)
        {
            try
            {
                Stopwatch s1 = new Stopwatch();
                s1.Start();
                string key = day != null ? day.ToString() : "Now";
                int countCheck = 0;
                // Cập nhật đài miền nam
                while (!checkN && countCheck < 3)
                {
                    checkN = UpdateKQMN(day, _totalDic[key], _totalBaCangDic[key], _totalBonSoDic[key]);
                    countCheck++;
                    if (!checkN && countCheck < 3)
                        Thread.Sleep(5000);
                }
                if (!checkN)
                {
                    countCheck = 0;
                    while (!checkN && countCheck < 3)
                    {
                        checkN = UpdateKQMN(day, _totalDic[key], _totalBaCangDic[key], _totalBonSoDic[key]);
                        countCheck++;
                        if (!checkN && countCheck < 3)
                            Thread.Sleep(5000);
                    }
                }
                countCheck = 0;
                // Cập nhật đài miền nam
                while (!checkT && countCheck < 3)
                {
                    checkT = UpdateKQMT(day, _totalDic[key], _totalBaCangDic[key], _totalBonSoDic[key]);
                    countCheck++;
                    if (!checkT && countCheck < 3)
                        Thread.Sleep(5000);
                }
                if (!checkT)
                {
                    countCheck = 0;
                    while (!checkT && countCheck < 3)
                    {
                        checkT = UpdateKQMT(day, _totalDic[key], _totalBaCangDic[key], _totalBonSoDic[key]);
                        countCheck++;
                        if (!checkT && countCheck < 3)
                            Thread.Sleep(5000);
                    }
                }
                countCheck = 0;
                // Cập nhật đài miền nam
                while (!checkB && countCheck < 3)
                {
                    checkB = UpdateKQMB(day, _totalDic[key], _totalBaCangDic[key], _totalBonSoDic[key]);
                    countCheck++;
                    if (!checkB && countCheck < 3)
                        Thread.Sleep(5000);
                }
                if (!checkB)
                {
                    countCheck = 0;
                    while (!checkB && countCheck < 3)
                    {
                        checkB = UpdateKQMB(day, _totalDic[key], _totalBaCangDic[key], _totalBonSoDic[key]);
                        countCheck++;
                        if (!checkB && countCheck < 3)
                            Thread.Sleep(5000);
                    }
                }
                s1.Stop();
                FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật tất cả dữ liệu lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                    $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms", "Init");
                return checkN && checkT && checkB;
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateKQ");
                return false;
            }
            finally
            {
            }

        }
        private static bool UpdateKQMB(DayOfWeek? day, List<int>[] sumList, List<int>[] sumBaList, List<int>[] sumBonList, DateTime? date = null)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;
                string link = GetLinkMienBacByDate(day);
                if (date != null)
                {
                    link = @$"https://www.xosodaiphat.com/xsmb-{date.Value.ToString("dd-MM-yyyy")}.html";
                }
                drivers.Navigate().GoToUrl(link);
                List<int> sumListTemp = new List<int>();
                List<int> baCangListTemp = new List<int>();
                List<int> bonSoListTemp = new List<int>();
                int count = 0;
                bool? checkMaDB = null;
                IList<IWebElement> allElement = drivers.FindElements(By.TagName("td"));
                foreach (IWebElement element in allElement)
                {
                    string cellText = element.Text;
                    if (cellText.Contains("G"))
                        continue;
                    if (cellText.Contains("Mã"))
                    {
                        checkMaDB = true;
                        continue;
                    }
                    if (checkMaDB == true)
                    {
                        checkMaDB = false;
                        continue;
                    }
                    count++;
                    foreach (var txt in cellText.Split("\r\n"))
                    {
                        int num = 0;
                        var check = int.TryParse(txt, out num);
                        if (check)
                        {
                            var numLo = num % 100;
                            sumListTemp.Add(numLo);
                            if (count < 8)
                            {
                                var numBa = num % 1000;
                                baCangListTemp.Add(numBa);
                            }
                            if (count < 7)
                            {
                                var numBa = num % 10000;
                                bonSoListTemp.Add(numBa);
                            }
                        }
                    }
                    if (count == 8)
                        break;
                }
                if (sumListTemp != null && sumListTemp.Count == 27)
                {
                    // Update 2 số
                    sumList[7] = sumListTemp;

                    // Update 3 số
                    sumBaList[7] = baCangListTemp;

                    // Update 4 số
                    sumBonList[7] = bonSoListTemp;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateKQMB");
                return false;
            }
            finally
            {
            }
        }
        private static bool UpdateKQMT(DayOfWeek? day, List<int>[] sumList, List<int>[] sumBaList, List<int>[] sumBonList, DateTime? date = null)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;
                string link = GetLinkMienTrungByDate(day);
                if (date != null)
                {
                    link = @$"https://www.xosodaiphat.com/xsmt-{date.Value.ToString("dd-MM-yyyy")}.html";
                }
                drivers.Navigate().GoToUrl(link);
                List<int>[] sumListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>() };
                List<int>[] baCangListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
                List<int>[] bonSoListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
                int count = 0;
                int countList = 0;
                DayOfWeek dayCheck = GetDayCheck(day, RegionEnum.MT);
                int max = (dayCheck == DayOfWeek.Thursday || dayCheck == DayOfWeek.Saturday || dayCheck == DayOfWeek.Sunday) ? 3 : 2;
                int maxLine = (dayCheck == DayOfWeek.Thursday || dayCheck == DayOfWeek.Saturday || dayCheck == DayOfWeek.Sunday) ? 27 : 18;
                IList<IWebElement> allElement = drivers.FindElements(By.TagName("td"));
                foreach (IWebElement element in allElement)
                {
                    string cellText = element.Text;
                    if (cellText.Contains("G") || cellText.Contains("Đ"))
                        continue;
                    count++;
                    foreach (var txt in cellText.Split("\r\n"))
                    {
                        int num = 0;
                        var check = int.TryParse(txt, out num);
                        if (check)
                        {
                            var numLo = num % 100;
                            sumListTemp[countList].Add(numLo);
                            if (count > max)
                            {
                                var numBa = num % 1000;
                                baCangListTemp[countList].Add(numBa);
                            }
                            if (count > (max * 2))
                            {
                                var numBa = num % 10000;
                                bonSoListTemp[countList].Add(numBa);
                            }
                        }
                    }
                    countList++;
                    if (count == maxLine)
                        break;
                    if (countList == max)
                        countList = 0;
                }
                if (sumListTemp.Length == 3 && sumListTemp[0].Count == 18 && sumListTemp[1].Count == 18
                         && ((day != DayOfWeek.Saturday && day != DayOfWeek.Thursday && dayCheck != DayOfWeek.Sunday) || sumListTemp[2].Count == 18))
                {
                    if(day == DayOfWeek.Thursday)
                    {
                        // Cập nhật 2 số
                        sumList[4] = sumListTemp[1];
                        sumList[5] = sumListTemp[2];
                        sumList[6] = sumListTemp[0];

                        // Cập nhật 3 số
                        sumBaList[4] = baCangListTemp[1];
                        sumBaList[5] = baCangListTemp[2];
                        sumBaList[6] = baCangListTemp[0];

                        // Cập nhật 4 số
                        sumBonList[4] = bonSoListTemp[1];
                        sumBonList[5] = bonSoListTemp[2];
                        sumBonList[6] = bonSoListTemp[0];
                    }
                    else if(day == DayOfWeek.Sunday)
                    {
                        // Cập nhật 2 số
                        sumList[4] = sumListTemp[1];
                        sumList[5] = sumListTemp[0];
                        sumList[6] = sumListTemp[2];

                        // Cập nhật 3 số
                        sumBaList[4] = baCangListTemp[1];
                        sumBaList[5] = baCangListTemp[0];
                        sumBaList[6] = baCangListTemp[2];

                        // Cập nhật 4 số
                        sumBonList[4] = bonSoListTemp[1];
                        sumBonList[5] = bonSoListTemp[0];
                        sumBonList[6] = bonSoListTemp[2];
                    }
                    else
                    {
                        // Cập nhật 2 số
                        sumList[4] = sumListTemp[0];
                        sumList[5] = sumListTemp[1];
                        sumList[6] = sumListTemp[2];

                        // Cập nhật 3 số
                        sumBaList[4] = baCangListTemp[0];
                        sumBaList[5] = baCangListTemp[1];
                        sumBaList[6] = baCangListTemp[2];

                        // Cập nhật 4 số
                        sumBonList[4] = bonSoListTemp[0];
                        sumBonList[5] = bonSoListTemp[1];
                        sumBonList[6] = bonSoListTemp[2];
                    }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateKQMT");
                return false;
            }
            finally
            {
            }
        }
        public static bool UpdateKQMBMinhNgoc(DayOfWeek? day, List<int>[] sumList, List<int>[] sumBaList, List<int>[] sumBonList, DateTime? date = null)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;

                string link = "https://www.minhngoc.net.vn/xo-so-truc-tiep/mien-bac.html";
                if (date != null)
                {
                    link = @$"https://www.minhngoc.net.vn/ket-qua-xo-so/mien-trung/{date.Value.ToString("dd-MM-yyyy")}.html";
                }
                drivers.Navigate().GoToUrl(link);
                List<int> sumListTemp = new List<int>();
                List<int> baCangListTemp = new List<int>();
                List<int> bonSoListTemp = new List<int>();
                DayOfWeek dayCheck = GetDayCheck(day, RegionEnum.MN);
                IList<IWebElement> allElement = drivers.FindElements(By.XPath("/html/body/div[1]/div/center/div/div/div[3]/div/div/div/table/tbody/tr/" +
                    "td[2]/div/table/tbody/tr/td[1]/div[2]/div/div[3]/div[3]/center/div/div[5]/div[2]/div/table/tbody/tr/td/table/tbody"));
                if (!allElement.Any())
                    return false;
                var array = allElement[0].Text.Split("\r\n");
                for (int i = 0; i <= 37; i++)
                {
                    int num = 0;
                    var check = int.TryParse(array[i], out num);
                    if (check)
                    {
                        var numLo = num % 100;
                        sumListTemp.Add(numLo);
                        if (i <= 32)
                        {
                            var numBa = num % 1000;
                            baCangListTemp.Add(numBa);
                        }
                        if (i <= 28)
                        {
                            var numBa = num % 10000;
                            bonSoListTemp.Add(numBa);
                        }
                    }
                }
                if (sumListTemp != null && sumListTemp.Count == 27)
                {
                    // Update 2 số
                    sumList[7] = sumListTemp;

                    // Update 3 số
                    sumBaList[7] = baCangListTemp;

                    // Update 4 số
                    sumBonList[7] = bonSoListTemp;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateKQMT");
                return false;
            }
            finally
            {
            }
        }
        public static bool InsertUpdateManyDate(DateTime startDate, DateTime endDate)
        {
            InitDriver();
            for (DateTime date = startDate; date<= endDate; date = date.AddDays(1))
            {
                List<int>[] sumList = new List<int>[8];
                List<int>[] sumBaList = new List<int>[8];
                List<int>[] sumBonList = new List<int>[8];

                UpdateKQMN(date.DayOfWeek, sumList,  sumBaList, sumBonList, date);
                UpdateKQMT(date.DayOfWeek, sumList, sumBaList, sumBonList, date);
                UpdateKQMB(date.DayOfWeek, sumList, sumBaList, sumBonList, date);
                if (sumList.Any(x => x != null) && sumList.Any(x => x != null) && sumList.Any(x => x != null))
                {
                    var haicon = JsonConvert.SerializeObject(sumList);
                    var bacon = JsonConvert.SerializeObject(sumBaList);
                    var boncon = JsonConvert.SerializeObject(sumBonList);

                    StoreKQRepository.AddOrUpdateStoreKq(date, haicon, bacon, boncon);
                }
            }
            DisposeDriver();
            return true;
        }
        public static bool UpdateKQMTMinhNgoc(DayOfWeek? day, List<int>[] sumList, List<int>[] sumBaList, List<int>[] sumBonList, DateTime? date = null)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;

                string link = "https://www.minhngoc.net.vn/xo-so-truc-tiep/mien-trung.html";
                if (date != null)
                {
                    link = @$"https://www.minhngoc.net.vn/ket-qua-xo-so/mien-trung/{date.Value.ToString("dd-MM-yyyy")}.html";
                }
                drivers.Navigate().GoToUrl(link);
                List<int>[] sumListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>() };
                List<int>[] baCangListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>() };
                List<int>[] bonSoListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>() };
                DayOfWeek dayCheck = GetDayCheck(day, RegionEnum.MN);
                int max = (dayCheck == DayOfWeek.Thursday || dayCheck == DayOfWeek.Saturday || dayCheck == DayOfWeek.Sunday) ? 3 : 2;
                IList<IWebElement> allElement = drivers.FindElements(By.XPath("/html/body/div[1]/div/center/div/div/div[3]/div" +
                    "/div/div/table/tbody/tr/td[2]/div/table/tbody/tr/td[1]/div[2]/div/div[3]/div[3]/center/div/div[2]/div[2]/table[1]/tbody/tr"));
                if (!allElement.Any())
                    return false;
                var array = allElement[0].Text.Split("\r\n");
                for (int i = 0; i < max; i++)
                {
                    int start = 13 + 20 * i;
                    int end = start + 18;
                    for (int j = start; j < end; j++)
                    {
                        int num = 0;
                        var check = int.TryParse(array[j], out num);
                        if (check)
                        {
                            var numLo = num % 100;
                            sumListTemp[i].Add(numLo);
                            if (j > start)
                            {
                                var numBa = num % 1000;
                                baCangListTemp[i].Add(numBa);
                            }
                            if (j > (start + 1))
                            {
                                var numBa = num % 10000;
                                bonSoListTemp[i].Add(numBa);
                            }
                        }
                    }
                }
                if (sumListTemp.Length == 3 && sumListTemp[0].Count == 18 && sumListTemp[1].Count == 18
                     && ((day != DayOfWeek.Saturday && day != DayOfWeek.Thursday && dayCheck != DayOfWeek.Sunday) || sumListTemp[2].Count == 18))
                {
                    // Cập nhật 2 số
                    sumList[4] = sumListTemp[0];
                    sumList[5] = sumListTemp[1];
                    sumList[6] = sumListTemp[2];

                    // Cập nhật 3 số
                    sumBaList[4] = baCangListTemp[0];
                    sumBaList[5] = baCangListTemp[1];
                    sumBaList[6] = baCangListTemp[2];

                    // Cập nhật 4 số
                    sumBonList[4] = bonSoListTemp[0];
                    sumBonList[5] = bonSoListTemp[1];
                    sumBonList[6] = bonSoListTemp[2];

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateKQMT");
                return false;
            }
            finally
            {
            }
        }

        public static bool UpdateKQMNMinhNgoc(DayOfWeek? day, List<int>[] sumList, List<int>[] sumBaList, List<int>[] sumBonList, DateTime? date = null)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;
                //01-11-2023
                string link = "https://www.minhngoc.net.vn/xo-so-truc-tiep/mien-nam.html";
                if(date != null)
                {
                    link = @$"https://www.minhngoc.net.vn/ket-qua-xo-so/mien-nam/{date.Value.ToString("dd-MM-yyyy")}.html";
                }
                
                drivers.Navigate().GoToUrl(link);
                List<int>[] sumListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
                List<int>[] baCangListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
                List<int>[] bonSoListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
                DayOfWeek dayCheck = GetDayCheck(day, RegionEnum.MN);
                int max = dayCheck == DayOfWeek.Saturday ? 4 : 3;
                IList<IWebElement> allElement = drivers.FindElements(By.XPath("/html/body/div[1]/div/center/div/div/div[3]/div" +
                    "/div/div/table/tbody/tr/td[2]/div/table/tbody/tr/td[1]/div[2]/div/div[3]/div[3]/center/div/div[2]/div[2]/table[1]/tbody/tr"));
                if (!allElement.Any())
                    return false;
                var array = allElement[0].Text.Split("\r\n");
                for (int i = 0; i < max; i++)
                {
                    int start = 13 + 20 * i;
                    int end = start + 18;
                    for (int j = start; j < end; j++)
                    {
                        int num = 0;
                        var check = int.TryParse(array[j], out num);
                        if (check)
                        {
                            var numLo = num % 100;
                            sumListTemp[i].Add(numLo);
                            if (j > start)
                            {
                                var numBa = num % 1000;
                                baCangListTemp[i].Add(numBa);
                            }
                            if (j > (start + 1))
                            {
                                var numBa = num % 10000;
                                bonSoListTemp[i].Add(numBa);
                            }
                        }
                    }
                }
                if (sumListTemp != null && sumListTemp.Length == 4 && sumListTemp[0].Count == 18 && sumListTemp[1].Count == 18
                    && sumListTemp[2].Count == 18 && (day != DayOfWeek.Saturday || sumListTemp[3].Count == 18))
                {
                    // Cập nhật 2 số
                    sumList[0] = sumListTemp[0];
                    sumList[1] = sumListTemp[1];
                    sumList[2] = sumListTemp[2];
                    sumList[3] = sumListTemp[3];

                    // Cập nhật 3 số
                    sumBaList[0] = baCangListTemp[0];
                    sumBaList[1] = baCangListTemp[1];
                    sumBaList[2] = baCangListTemp[2];
                    sumBaList[3] = baCangListTemp[3];

                    // Cập nhật 4 số
                    sumBonList[0] = bonSoListTemp[0];
                    sumBonList[1] = bonSoListTemp[1];
                    sumBonList[2] = bonSoListTemp[2];
                    sumBonList[3] = bonSoListTemp[3];

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateKQMT");
                return false;
            }
            finally
            {
            }
        }

        private static bool UpdateKQMN(DayOfWeek? day, List<int>[] sumList, List<int>[] sumBaList, List<int>[] sumBonList, DateTime? date = null)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;
                string link = GetLinkMienNamByDate(day);
                if (date != null)
                {
                    link = @$"https://www.xosodaiphat.com/xsmn-{date.Value.ToString("dd-MM-yyyy")}.html";
                }

                drivers.Navigate().GoToUrl(link);
                List<int>[] sumListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
                List<int>[] baCangListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
                List<int>[] bonSoListTemp = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
                int count = 0;
                int countList = 0;
                DayOfWeek dayCheck = GetDayCheck(day, RegionEnum.MN);
                int max = dayCheck == DayOfWeek.Saturday ? 4 : 3;
                int maxLine = dayCheck == DayOfWeek.Saturday ? 36 : 27;
                IList<IWebElement> allElement = drivers.FindElements(By.TagName("td"));
                foreach (IWebElement element in allElement)
                {
                    string cellText = element.Text;
                    if (cellText.Contains("G") || cellText.Contains("Đ"))
                        continue;
                    count++;
                    foreach (var txt in cellText.Split("\r\n"))
                    {
                        int num = 0;
                        var check = int.TryParse(txt, out num);
                        if (check)
                        {
                            var numLo = num % 100;
                            sumListTemp[countList].Add(numLo);
                            if (count > max)
                            {
                                var numBa = num % 1000;
                                baCangListTemp[countList].Add(numBa);
                            }
                            if (count > (max * 2))
                            {
                                var numBa = num % 10000;
                                bonSoListTemp[countList].Add(numBa);
                            }
                        }
                    }
                    countList++;
                    if (count == maxLine)
                        break;
                    if (countList == max)
                        countList = 0;
                }
                if (sumListTemp != null && sumListTemp.Length == 4 && sumListTemp[0].Count == 18 && sumListTemp[1].Count == 18
                    && sumListTemp[2].Count == 18 && (day != DayOfWeek.Saturday || sumListTemp[3].Count == 18))
                {
                    // Cập nhật 2 số
                    sumList[0] = sumListTemp[0];
                    sumList[1] = sumListTemp[1];
                    sumList[2] = sumListTemp[2];
                    sumList[3] = sumListTemp[3];

                    // Cập nhật 3 số
                    sumBaList[0] = baCangListTemp[0];
                    sumBaList[1] = baCangListTemp[1];
                    sumBaList[2] = baCangListTemp[2];
                    sumBaList[3] = baCangListTemp[3];

                    // Cập nhật 4 số
                    sumBonList[0] = bonSoListTemp[0];
                    sumBonList[1] = bonSoListTemp[1];
                    sumBonList[2] = bonSoListTemp[2];
                    sumBonList[3] = bonSoListTemp[3];

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "UpdateKQMT");
                return false;
            }
            finally
            {
            }
        }
        private static void InitDriver()
        {
            try
            {
                //ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                //service.HideCommandPromptWindow = true;
                var options = new ChromeOptions();
                options.AddArguments("--window-position=-32000,-32000");
                //drivers = new ChromeDriver("C:\\tesst\\driver", options);
                drivers = new ChromeDriver(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), options);
                //drivers = new ChromeDriver("D:\\Git\\KQ.Api", options);

                //FirefoxOptions options = new FirefoxOptions();
                //options.AddArguments("--headless");
                ////options.AddArguments("--window-position=-32000,-32000");
                //drivers = new FirefoxDriver("C:\\tesst\\driver", options);
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.Message, "InitDriver");
            }
        }
        private static void DisposeDriver()
        {
            try
            {
                drivers.Dispose();
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.Message, "DisposeDriver");
            }
        }
        private static string GetLinkMienNamByDate(DayOfWeek? day)
        {
            string baseLink = "https://xosodaiphat.com/";
            if (day == DayOfWeek.Monday)
                return baseLink + "xsmn-thu-2.html";
            if (day == DayOfWeek.Tuesday)
                return baseLink + "xsmn-thu-3.html";
            if (day == DayOfWeek.Wednesday)
                return baseLink + "xsmn-thu-4.html";
            if (day == DayOfWeek.Thursday)
                return baseLink + "xsmn-thu-5.html";
            if (day == DayOfWeek.Friday)
                return baseLink + "xsmn-thu-6.html";
            if (day == DayOfWeek.Saturday)
                return baseLink + "xsmn-thu-7.html";
            if (day == DayOfWeek.Sunday)
                return baseLink + "xsmn-chu-nhat-cn.html";
            return baseLink + "xsmn-xo-so-mien-nam.html";
        }
        private static string GetLinkMienTrungByDate(DayOfWeek? day)
        {
            string baseLink = "https://xosodaiphat.com/";
            if (day == DayOfWeek.Monday)
                return baseLink + "xsmt-thu-2.html";
            if (day == DayOfWeek.Tuesday)
                return baseLink + "xsmt-thu-3.html";
            if (day == DayOfWeek.Wednesday)
                return baseLink + "xsmt-thu-4.html";
            if (day == DayOfWeek.Thursday)
                return baseLink + "xsmt-thu-5.html";
            if (day == DayOfWeek.Friday)
                return baseLink + "xsmt-thu-6.html";
            if (day == DayOfWeek.Saturday)
                return baseLink + "xsmt-thu-7.html";
            if (day == DayOfWeek.Sunday)
                return baseLink + "xsmt-chu-nhat-cn.html";
            return baseLink + "xsmt-xo-so-mien-trung.html";
        }
        private static string GetLinkMienBacByDate(DayOfWeek? day)
        {
            string baseLink = "https://xosodaiphat.com/";
            if (day == DayOfWeek.Monday)
                return baseLink + "xsmb-thu-2.html";
            if (day == DayOfWeek.Tuesday)
                return baseLink + "xsmb-thu-3.html";
            if (day == DayOfWeek.Wednesday)
                return baseLink + "xsmb-thu-4.html";
            if (day == DayOfWeek.Thursday)
                return baseLink + "xsmb-thu-5.html";
            if (day == DayOfWeek.Friday)
                return baseLink + "xsmb-thu-6.html";
            if (day == DayOfWeek.Saturday)
                return baseLink + "xsmb-thu-7.html";
            if (day == DayOfWeek.Sunday)
                return baseLink + "xsmb-chu-nhat-cn.html";
            return baseLink + "xsmb-xo-so-mien-bac.html";
        }
        private static DayOfWeek GetDayCheck(DayOfWeek? day, RegionEnum region)
        {
            if (day != null)
                return (DayOfWeek)day;
            else
            {
                var now = DateTime.Now;
                switch (region)
                {
                    case RegionEnum.MN:
                        if (now.TimeOfDay > new TimeSpan(16, 15, 00))
                            return now.DayOfWeek;
                        else
                            return now.AddDays(-1).DayOfWeek;
                    case RegionEnum.MT:
                        if (now.TimeOfDay > new TimeSpan(17, 15, 00))
                            return now.DayOfWeek;
                        else
                            return now.AddDays(-1).DayOfWeek;
                    default:
                        if (now.TimeOfDay > new TimeSpan(18, 15, 00))
                            return now.DayOfWeek;
                        else
                            return now.AddDays(-1).DayOfWeek;
                }
            }
        }
    }
}
