using KQ.Common.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Timers;

namespace KQ.Common.Helpers
{
    public class InnitRepository
    {
        static bool checkAll = false;
        static System.Timers.Timer aTimer = new System.Timers.Timer();
        static int endCheck = 45;
        private static IWebDriver drivers;
        public static ConcurrentDictionary<string, List<int>[]> _totalDic;
        public static ConcurrentDictionary<string, List<int>[]> _totalBaCangDic;
        public static ConcurrentDictionary<string, List<int>[]> _totalBonSoDic;
        public static ConcurrentDictionary<int, List<string>> _chanelCode;
        public static ConcurrentDictionary<int, List<string>> _chanelCodeForTest;

        private static void UpdateChanelCode()
        {
            _chanelCode = CommonFunction.GetCurrentChanelCode();
        }
        private static void UpdateChanelCodeForTest()
        {
            _chanelCodeForTest = new ConcurrentDictionary<int, List<string>>();
            _chanelCodeForTest.TryAdd(1, new List<string> { "Vĩnh Long","VinhLong","vinhlong", "Vinh Long", "vlong", "vlong", "vl" });
            _chanelCodeForTest.TryAdd(2, new List<string> { "Bình Dương", "BinhDuong", "binhduong", "Binh Duong", "bduong", "binhDuong", "bd" });
            _chanelCodeForTest.TryAdd(3, new List<string> { "TPHCM", "TP", "tp", "tphcm"});
            _chanelCodeForTest.TryAdd(5, new List<string> { "Khánh Hòa", "Khanh Hoa", "khanh hoa","khanhhoa","kh"});
            _chanelCodeForTest.TryAdd(6, new List<string> { "Kom Tum", "komtum", "kt","kom tum"});
            _chanelCodeForTest.TryAdd(7, new List<string> { "Huế", "hue","h"});
            _chanelCodeForTest.TryAdd(8, new List<string> { "Miền bắc", "MB" });
        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                //FileHelper.GeneratorFileByDay(FileStype.Log, $"Đang chạy này", "Report");
                var now = DateTime.Now.TimeOfDay;
                if (now <= new TimeSpan(0, 2, 0))
                {
                    checkAll = false;
                }
                else if (now > new TimeSpan(2, 0, 0) && now <= new TimeSpan(2, 2, 0))
                {
                    bool check = false;
                    int countCheck = 0;
                    lock (_totalDic)
                    {
                        while (!check && countCheck < 2)
                        {
                            check = InitAllChanel();
                            countCheck++;
                            checkAll = check;
                            if (!check && countCheck < 3)
                                Thread.Sleep(5000);
                        }
                    }
                }
                else if (!checkAll && now > new TimeSpan(4, 0, 0) && now <= new TimeSpan(4, 2, 0))
                {
                    bool check = false;
                    int countCheck = 0;
                    lock (_totalDic)
                    {
                        while (!check && countCheck < 2)
                        {
                            check = InitAllChanel();
                            countCheck++;
                            checkAll = check;
                            if (!check && countCheck < 3)
                                Thread.Sleep(5000);
                        }
                    }
                }
                else if (!checkAll && now > new TimeSpan(6, 0, 0) && now <= new TimeSpan(6, 2, 0))
                {
                    bool check = false;
                    int countCheck = 0;
                    lock (_totalDic)
                    {
                        while (!check && countCheck < 2)
                        {
                            check = InitAllChanel();
                            countCheck++;
                            checkAll = check;
                            if (!check && countCheck < 3)
                                Thread.Sleep(5000);
                        }
                    }
                    if(!checkAll)
                    {
                        FileHelper.GeneratorFileByDay(FileStype.Error, $"Chưa cập nhật được kết quả mới nhất", "Inportant");
                    }
                }
                else if (now > new TimeSpan(16, 15, 0) && now <= new TimeSpan(16, endCheck, 0))
                {
                    if(now <= new TimeSpan(16, 17, 0))
                    {
                        UpdateChanelCode();
                    }
                    Stopwatch s1 = new Stopwatch();
                    s1.Start();
                    bool check = false;
                    int countCheck = 0;
                    InitDriver();
                    // Cập nhật đài miền nam
                    try
                    {
                        lock (_totalDic)
                        {
                            while (!check && countCheck < 2)
                            {
                                check = UpdateKQMN(null, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                countCheck++;
                                if (!check && countCheck < 3)
                                    Thread.Sleep(5000);
                            }
                        }
                    }
                    finally
                    {
                        DisposeDriver();
                    }
                    
                    s1.Stop();
                    FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật dữ liệu MN lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                        $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms. Số lần thực hiện {countCheck}", "Init");
                }
                else if (now > new TimeSpan(17, 0, 0) && now <= new TimeSpan(17, 2, 0))
                {
                    Stopwatch s1 = new Stopwatch();
                    s1.Start();
                    bool check = false;
                    int countCheck = 0;
                    InitDriver();
                    // Cập nhật đài miền nam
                    try
                    {
                        lock (_totalDic)
                        {
                            while (!check && countCheck < 2)
                            {
                                check = UpdateKQMN(null, _totalDic["Now"], _totalBaCangDic["Now"],_totalBonSoDic["Now"]);
                                countCheck++;
                                if (!check && countCheck < 3)
                                    Thread.Sleep(5000);
                            }
                        }
                    }
                    finally
                    {
                        DisposeDriver();
                    }

                    s1.Stop();
                    FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật dữ liệu MN lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                        $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms. Số lần thực hiện {countCheck}", "Init");
                }
                else if (now > new TimeSpan(17, 15, 0) && now <= new TimeSpan(17, endCheck, 0))
                {
                    if (now <= new TimeSpan(17, 17, 0))
                    {
                        UpdateChanelCode();
                    }
                    Stopwatch s1 = new Stopwatch();
                    s1.Start();
                    InitDriver();
                    bool check = false;
                    int countCheck = 0;
                    try
                    {
                        // Cập nhật đài miền trung
                        lock (_totalDic)
                        {
                            while (!check && countCheck < 2)
                            {
                                check = UpdateKQMT(null, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                countCheck++;
                                if (!check && countCheck < 3)
                                    Thread.Sleep(5000);
                            }
                        }
                    }
                    finally
                    {
                        DisposeDriver();
                    }
                    
                    s1.Stop();
                    FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật dữ liệu MT lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                        $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms. Số lần thực hiện {countCheck}", "Update---");
                }
                else if (now > new TimeSpan(18, 0, 0) && now <= new TimeSpan(18, 2, 0))
                {
                    Stopwatch s1 = new Stopwatch();
                    s1.Start();
                    InitDriver();
                    bool check = false;
                    int countCheck = 0;
                    try
                    {
                        // Cập nhật đài miền trung
                        lock (_totalDic)
                        {
                            while (!check && countCheck < 2)
                            {
                                check = UpdateKQMT(null, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                countCheck++;
                                if (!check && countCheck < 3)
                                    Thread.Sleep(5000);
                            }
                        }
                    }
                    finally
                    {
                        DisposeDriver();
                    }

                    s1.Stop();
                    FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật dữ liệu MT lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                        $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms. Số lần thực hiện {countCheck}", "Update---");
                }
                else if (now > new TimeSpan(18, 15, 0) && now <= new TimeSpan(18, endCheck, 0))
                {
                    if (now <= new TimeSpan(18, 17, 0))
                    {
                        UpdateChanelCode();
                    }
                    Stopwatch s1 = new Stopwatch();
                    s1.Start();
                    bool check = false;
                    int countCheck = 0;
                    InitDriver();
                    // Cập nhật đài miền bắc
                    try
                    {
                        lock (_totalDic)
                        {
                            while (!check && countCheck < 2)
                            {
                                check = UpdateKQMB(null, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                countCheck++;
                                if (!check && countCheck < 3)
                                    Thread.Sleep(5000);
                            }
                        }
                    }
                    finally
                    {
                        DisposeDriver();
                    }
                    
                    s1.Stop();
                    FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật dữ liệu MB lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                        $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms. Số lần thực hiện {countCheck}", "Init");
                }
                else if (now > new TimeSpan(19, 0, 0) && now <= new TimeSpan(19, 0, 0))
                {
                    Stopwatch s1 = new Stopwatch();
                    s1.Start();
                    bool check = false;
                    int countCheck = 0;
                    InitDriver();
                    // Cập nhật đài miền bắc
                    try
                    {
                        lock (_totalDic)
                        {
                            while (!check && countCheck < 2)
                            {
                                check = UpdateKQMB(null, _totalDic["Now"], _totalBaCangDic["Now"], _totalBonSoDic["Now"]);
                                countCheck++;
                                if (!check && countCheck < 3)
                                    Thread.Sleep(5000);
                            }
                        }
                    }
                    finally
                    {
                        DisposeDriver();
                    }

                    s1.Stop();
                    FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật dữ liệu MB lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                        $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms. Số lần thực hiện {countCheck}", "Init");
                }
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "OnTimedEvent");
            }

        }
        public static bool InitAllChanel()
        {
            try
            {
                Stopwatch s1 = new Stopwatch();
                s1.Start();
                if (_totalDic == null)
                {
                    _totalDic = new ConcurrentDictionary<string, List<int>[]>();
                    _totalDic.TryAdd("Monday", new List<int>[8]);
                    _totalDic.TryAdd("Tuesday", new List<int>[8]);
                    _totalDic.TryAdd("Wednesday", new List<int>[8]);
                    _totalDic.TryAdd("Thursday", new List<int>[8]);
                    _totalDic.TryAdd("Friday", new List<int>[8]);
                    _totalDic.TryAdd("Sunday", new List<int>[8]);
                    _totalDic.TryAdd("Saturday", new List<int>[8]);
                    _totalDic.TryAdd("Now", new List<int>[8]);
                }
                if(_totalBaCangDic == null)
                {
                    _totalBaCangDic = new ConcurrentDictionary<string, List<int>[]>();
                    _totalBaCangDic.TryAdd("Monday", new List<int>[8]);
                    _totalBaCangDic.TryAdd("Tuesday", new List<int>[8]);
                    _totalBaCangDic.TryAdd("Wednesday", new List<int>[8]);
                    _totalBaCangDic.TryAdd("Thursday", new List<int>[8]);
                    _totalBaCangDic.TryAdd("Friday", new List<int>[8]);
                    _totalBaCangDic.TryAdd("Sunday", new List<int>[8]);
                    _totalBaCangDic.TryAdd("Saturday", new List<int>[8]);
                    _totalBaCangDic.TryAdd("Now", new List<int>[8]);
                }
                if (_totalBonSoDic == null)
                {
                    _totalBonSoDic = new ConcurrentDictionary<string, List<int>[]>();
                    _totalBonSoDic.TryAdd("Monday", new List<int>[8]);
                    _totalBonSoDic.TryAdd("Tuesday", new List<int>[8]);
                    _totalBonSoDic.TryAdd("Wednesday", new List<int>[8]);
                    _totalBonSoDic.TryAdd("Thursday", new List<int>[8]);
                    _totalBonSoDic.TryAdd("Friday", new List<int>[8]);
                    _totalBonSoDic.TryAdd("Sunday", new List<int>[8]);
                    _totalBonSoDic.TryAdd("Saturday", new List<int>[8]);
                    _totalBonSoDic.TryAdd("Now", new List<int>[8]);
                }
                InitDriver();
                var check1 = UpdateKQ(null);
                //var check2 = UpdateKQ(DayOfWeek.Monday);
                //var check3 = UpdateKQ(DayOfWeek.Tuesday);
                //var check4 = UpdateKQ(DayOfWeek.Wednesday);
                //var check5 = UpdateKQ(DayOfWeek.Thursday);
                //var check6 = UpdateKQ(DayOfWeek.Friday);
                //var check7 = UpdateKQ(DayOfWeek.Saturday);
                //var check8 = UpdateKQ(DayOfWeek.Sunday);
                s1.Stop();
                FileHelper.GeneratorFileByDay(FileStype.Log, $"Cập nhật tất cả dữ liệu lúc {DateTime.Now.ToString("HH:mm:ss")}." +
                    $" Thời gian thực hiện {s1.ElapsedMilliseconds} ms", "Init");

                return true;

            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "Init");
                return false;
            }
            finally
            {
                DisposeDriver();
            }

        }
        public static void Init()
        {
            try
            {
                FileHelper.GeneratorFileByDay(FileStype.Log, $"Khởi động hệ thống lúc {DateTime.Now.ToString("HH:mm:ss")}.", "Init");
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Interval = 120000;
                aTimer.Enabled = true;
                UpdateChanelCode();
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
                string key = day != null ? day.ToString() : "Now";
                bool check1 = false;
                int countCheck = 0;
                // Cập nhật đài miền nam
                while (!check1 && countCheck < 3)
                {
                    check1 = UpdateKQMN(day, _totalDic[key], _totalBaCangDic[key], _totalBonSoDic[key]);
                    countCheck++;
                    if (!check1 && countCheck < 3)
                        Thread.Sleep(5000);
                }
                var check2 = false;
                countCheck = 0;
                // Cập nhật đài miền nam
                while (!check2 && countCheck < 3)
                {
                    check2 = UpdateKQMT(day, _totalDic[key], _totalBaCangDic[key], _totalBonSoDic[key]);
                    countCheck++;
                    if (!check2 && countCheck < 3)
                        Thread.Sleep(5000);
                }
                var check3 = false;
                countCheck = 0;
                // Cập nhật đài miền nam
                while (!check3 && countCheck < 3)
                {
                    check3 = UpdateKQMB(day, _totalDic[key], _totalBaCangDic[key], _totalBonSoDic[key]);
                    countCheck++;
                    if (!check3 && countCheck < 3)
                        Thread.Sleep(5000);
                }

                return check1 && check2 && check3;
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
        private static bool UpdateKQMB(DayOfWeek? day, List<int>[] sumList, List<int>[] sumBaList, List<int>[] sumBonList)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;
                string link = GetLinkMienBacByDate(day);
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
                    if(cellText.Contains("Mã"))
                    {
                        checkMaDB = true;
                        continue;
                    }
                    if(checkMaDB == true)
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
                if (now >= new TimeSpan(18, 15, 00) && now <= new TimeSpan(18, 30, 00))
                {
                    sumListTemp.Remove(sumListTemp.FirstOrDefault());
                    sumListTemp.Remove(sumListTemp.LastOrDefault());
                    sumList[7] = sumListTemp;

                    // Update cho 3 số
                    baCangListTemp.Remove(baCangListTemp.FirstOrDefault());
                    baCangListTemp.Remove(baCangListTemp.LastOrDefault());
                    sumBaList[7] = baCangListTemp;

                    // Update cho 4 số
                    bonSoListTemp.Remove(bonSoListTemp.FirstOrDefault());
                    bonSoListTemp.Remove(bonSoListTemp.LastOrDefault());
                    sumBonList[7] = bonSoListTemp;
                    return true;
                }
                else if (sumListTemp != null && sumListTemp.Count == 27)
                {
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
        private static bool UpdateKQMT(DayOfWeek? day, List<int>[] sumList, List<int>[] sumBaList, List<int>[] sumBonList)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;
                string link = GetLinkMienTrungByDate(day);
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
                if (now >= new TimeSpan(17, 15, 00) && now <= new TimeSpan(17, 30, 00))
                {
                    // Update 2 số
                    sumListTemp[0].Remove(sumListTemp[0].LastOrDefault());
                    sumListTemp[1].Remove(sumListTemp[1].LastOrDefault());
                    sumListTemp[2].Remove(sumListTemp[2].LastOrDefault());;
                    sumList[4] = sumListTemp[0];
                    sumList[5] = sumListTemp[1];
                    sumList[6] = sumListTemp[2];

                    // Update 3 số
                    baCangListTemp[0].Remove(baCangListTemp[0].LastOrDefault());
                    baCangListTemp[1].Remove(baCangListTemp[1].LastOrDefault());
                    baCangListTemp[2].Remove(baCangListTemp[2].LastOrDefault()); ;
                    sumBaList[4] = baCangListTemp[0];
                    sumBaList[5] = baCangListTemp[1];
                    sumBaList[6] = baCangListTemp[2];

                    // Update 4 số
                    bonSoListTemp[0].Remove(bonSoListTemp[0].LastOrDefault());
                    bonSoListTemp[1].Remove(bonSoListTemp[1].LastOrDefault());
                    bonSoListTemp[2].Remove(bonSoListTemp[2].LastOrDefault()); ;
                    sumBonList[4] = bonSoListTemp[0];
                    sumBonList[5] = bonSoListTemp[1];
                    sumBonList[6] = bonSoListTemp[2];
                    return true;
                }
                else if (sumListTemp.Length == 3 && sumListTemp[0].Count == 18 && sumListTemp[1].Count == 18
                         && ((day != DayOfWeek.Saturday && day != DayOfWeek.Thursday) || sumListTemp[2].Count == 18))
                {
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
        private static bool UpdateKQMN(DayOfWeek? day, List<int>[] sumList, List<int>[] sumBaList,List<int>[] sumBonList)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;
                string link = GetLinkMienNamByDate(day);
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
                            if(count > max)
                            {
                                var numBa = num % 1000;
                                baCangListTemp[countList].Add(numBa);
                            }
                            if(count > (max * 2))
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
                if(now >= new TimeSpan(16,15,00) && now <= new TimeSpan(16, 30, 00))
                {
                    // Cập nhật cho 2 số
                    sumListTemp[0].Remove(sumListTemp[0].LastOrDefault());
                    sumListTemp[1].Remove(sumListTemp[1].LastOrDefault());
                    sumListTemp[2].Remove(sumListTemp[2].LastOrDefault());
                    sumListTemp[3].Remove(sumListTemp[3].LastOrDefault());
                    sumList[0] = sumListTemp[0];
                    sumList[1] = sumListTemp[1];
                    sumList[2] = sumListTemp[2];
                    sumList[3] = sumListTemp[3];

                    // Cập nhật cho 3 số
                    baCangListTemp[0].Remove(baCangListTemp[0].LastOrDefault());
                    baCangListTemp[1].Remove(baCangListTemp[1].LastOrDefault());
                    baCangListTemp[2].Remove(baCangListTemp[2].LastOrDefault());
                    baCangListTemp[3].Remove(baCangListTemp[3].LastOrDefault());
                    sumBaList[0] = baCangListTemp[0];
                    sumBaList[1] = baCangListTemp[1];
                    sumBaList[2] = baCangListTemp[2];
                    sumBaList[3] = baCangListTemp[3];

                    // Cập nhật cho 4 số
                    bonSoListTemp[0].Remove(bonSoListTemp[0].LastOrDefault());
                    bonSoListTemp[1].Remove(bonSoListTemp[1].LastOrDefault());
                    bonSoListTemp[2].Remove(bonSoListTemp[2].LastOrDefault());
                    bonSoListTemp[3].Remove(bonSoListTemp[3].LastOrDefault());
                    sumBonList[0] = bonSoListTemp[0];
                    sumBonList[1] = bonSoListTemp[1];
                    sumBonList[2] = bonSoListTemp[2];
                    sumBonList[3] = bonSoListTemp[3];

                    return true;
                }
                else if (sumListTemp != null && sumListTemp.Length == 4 && sumListTemp[0].Count == 18 && sumListTemp[1].Count == 18
                    && sumListTemp[2].Count == 18 && (day != DayOfWeek.Saturday || sumListTemp[3].Count == 18))
                {
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
            //ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            //service.HideCommandPromptWindow = true;
            var options = new ChromeOptions();
            options.AddArguments("--window-position=-32000,-32000");
            drivers = new ChromeDriver("C:\\tesst\\driver", options);
            //drivers = new ChromeDriver("D:\\Private\\KQ.Api", options);
            //drivers = new ChromeDriver("D:\\Git\\KQ.Api", options);

            //FirefoxOptions options = new FirefoxOptions();
            //options.AddArguments("--headless");
            ////options.AddArguments("--window-position=-32000,-32000");
            //drivers = new FirefoxDriver("C:\\tesst\\driver", options);
        }
        private static void DisposeDriver()
        {
            drivers.Dispose();
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
