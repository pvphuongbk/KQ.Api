using KQ.Common.Helpers;
using System.ComponentModel;
using System.Text.RegularExpressions;
using KQ.DataAccess.Base;
using KQ.DataAccess.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using System.Text;
using System;

namespace KQ.Common.Extention
{
    public static class StringExtensions
    {
        private static string key = "%2*";

        public static string[] arr1 = new string[] { "á", "à","à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                                            "đ","₫",
                                            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                                            "í","ì","ỉ","ĩ","ị",
                                            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                                            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                                            "ý","ỳ","ỷ","ỹ","ỵ",};
        public static string[] arr2 = new string[] { "a", "a", "a","a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                                            "d","d",
                                            "e","e","e","e","e","e","e","e","e","e","e",
                                            "i","i","i","i","i",
                                            "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                                            "u","u","u","u","u","u","u","u","u","u","u",
                                            "y","y","y","y","y",};
        public static string FirstCharToUpper(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return string.Concat(str[0].ToString().ToUpper(), str.AsSpan(1));
        }
        public static string GetEnumDescription(this System.Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }

        public static string RemoveUnicode(this string text)
        {

            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }
        public static string GetFirstChar(this string s)
        {
            string result = "";
            foreach (var c in s.Split(" "))
            {
                result += c.FirstOrDefault().ToString();
            }

            return result;
        }
        public static string GetFirstCharOnlyFisrt(this string s)
        {
            var arr = s.Split(" ").ToArray();
            string result = arr[0];
            for (int i = 1; i < arr.Length; i++)
            {
                result += arr[i].FirstOrDefault().ToString();
            }

            return result;
        }
        public static string GetFirstCharOnlySecond(this string s)
        {
            var arr = s.Split(" ").ToArray();
            string result = "";
            for (int i = 0; i < arr.Length - 1; i++)
            {
                result += arr[i].FirstOrDefault().ToString();
            }
            result += arr[arr.Length - 1].FirstOrDefault().ToString();
            return result;
        }
        public static string GetFirstCharOnlyForFisrt(this string s)
        {
            var arr = s.Split(" ").ToArray();
            string result = arr[0].FirstOrDefault().ToString();
            for (int i = 1; i < arr.Length; i++)
            {
                result += arr[i];
            }
            return result;
        }
        public static string ChuanHoaString(this string? input)
        {
            //var sy = input.ToLower();
            var sy = Regex.Replace(input, @"\t|\n|\r", " ");
            //sy = sy.RemoveUnicode();
            return sy;
        }
        public static List<string> ChuanHoaString2(this List<string?> input, bool isStr = false)
        {
            List<string> re = new List<string>();
            foreach (var sys in input)
            {
                bool dao = false;
                var sy = sys.ToLower();
                sy = sy.RemoveUnicode();
                if (sy.EndsWith("dao"))
                {
                    sy = sy.Replace("dao", string.Empty);
                    dao = true;

                }
                var num = StrToNumber(sy);
                if (!isStr && num > 0)
                    re.Add(num.ToString());
                else if(!string.IsNullOrEmpty(sy))
                    re.Add(sy);

                if(dao)
                    re.Add("dao");
            }
            return re;
        }
        public static string ChuanHoaString2(this string? input)
        {

            var sy = input.ToLower();
            sy = sy.RemoveUnicode();
            return sy;
        }
        public static bool GetChanel(this string it, ref string chanel)
        {
            if (it == "hn" || it == "h" || it == "mb")
            {
                chanel = "hn";
                return true;
            }
            else if (it == "mt" || it == "mtr")
            {
                chanel = "mt";
                return true;
            }
            else if (it == "mn")
            {
                chanel = "mn";
                return true;
            }
            else
                return false;
        }
        //public static List<int> BaSoToBaoDao(this int it)
        //{
        //    var str = it.ToString("000");
        //    return new List<int> { int.Parse(str), int.Parse(str[0].ToString()+ str[2].ToString() + str[1].ToString()),
        //        int.Parse(str[1].ToString() + str[0].ToString() + str[2].ToString()),int.Parse(str[1].ToString()+ str[2].ToString() + str[0].ToString()),
        //        int.Parse(str[2].ToString()+ str[0].ToString() + str[1].ToString()),int.Parse(str[2].ToString()+ str[1].ToString() + str[0].ToString()) };
        //}
        public static int StringToInt(this string it)
        {
            return int.Parse(it);
        }
        public static List<string> ChanelIntToString(this List<int> it)
        {
            List<string> list = new List<string>();
            var chanels = CommonFunction.GetChanelCodeForNow();
            if (Constants.Constants.IstestMode)
            {
                chanels = InnitRepository._chanelCodeForTest;
            }
            foreach (int i in it)
            {
                list.Add(chanels.FirstOrDefault(x => x.Key == i).Value.FirstOrDefault());
            }
            return list;
        }
        public static void ConvertConfigTo(this TiLeBase input, TiLeBase result)
        {
            result.NCoBaoHaiCon = input.NCoBaoHaiCon;
            result.NTrungBaoHaiCon = input.NTrungBaoHaiCon;
            result.NCoHaiConDD = input.NCoHaiConDD;
            result.NTrungHaiConDD = input .NTrungHaiConDD;
            result.NCoDaThang = input .NCoDaThang;
            result.NTrungDaThang = input.NTrungDaThang;
            result.NCachTrungDaThang = input.NCachTrungDaThang;
            result.NCoDaXien = input.NCoDaXien;
            result.NTrungDaXien = input.NTrungDaXien;
            result.NCachTrungDaXien = input.NCachTrungDaXien;
            result.NCoBaCon = input.NCoBaCon;
            result.NTrungBaCon = input.NTrungBaCon;
            result.NCoBonCon = input.NCoBonCon;
            result.NTrungBonCon = input.NTrungBonCon;
            result.NPhanTramTong = input.NPhanTramTong;

            result.TCoBaoHaiCon = input.TCoBaoHaiCon;
            result.TTrungBaoHaiCon = input.TTrungBaoHaiCon;
            result.TCoHaiConDD = input.TCoHaiConDD;
            result.TTrungHaiConDD = input.TTrungHaiConDD;
            result.TCoDaThang = input.TCoDaThang;
            result.TTrungDaThang = input.TTrungDaThang;
            result.TCachTrungDaThang = input.TCachTrungDaThang;
            result.TCoDaXien = input.TCoDaXien;
            result.TTrungDaXien = input.TTrungDaXien;
            result.TCachTrungDaXien = input.TCachTrungDaXien;
            result.TCoBaCon = input.TCoBaCon;
            result.TTrungBaCon = input.TTrungBaCon;
            result.TCoBonCon = input.TCoBonCon;
            result.TTrungBonCon = input.TTrungBonCon;
            result.TPhanTramTong = input.TPhanTramTong;

            result.BCoBaoHaiCon = input.BCoBaoHaiCon;
            result.BTrungBaoHaiCon = input.BTrungBaoHaiCon;
            result.BCoHaiConDD = input.BCoHaiConDD;
            result.BTrungHaiConDD = input.BTrungHaiConDD;
            result.BCoDaThang = input.BCoDaThang;
            result.BTrungDaThang = input.BTrungDaThang;
            result.BCachTrungDaThang = input.BCachTrungDaThang;
            result.BCoBaCon = input.BCoBaCon;
            result.BTrungBaCon = input.BTrungBaCon;
            result.BCoBonCon = input.BCoBonCon;
            result.BTrungBonCon = input.BTrungBonCon;
            result.BPhanTramTong = input.BPhanTramTong;
            result.BCoXienHai = input.BCoXienHai;
            result.BTrungXienHai = input.BTrungXienHai;
            result.BCoXienBa = input.BCoXienBa;
            result.BTrungXienBa = input.BTrungXienBa;
            result.BCoXienBon = input.BCoXienBon;
            result.BTrungXienBon = input.BTrungXienBon;
        }

        public static int StrToNumber(this string? input)
        {
            switch (input)
            {
                case "mot":
                    return 1;
                case "hai":
                    return 2;
                case "ba":
                    return 3;
                case "bon":
                    return 4;
                case "nam":
                    return 5;
                case "sau":
                    return 6;
                case "bay":
                    return 7;
                case "tam":
                    return 8;
                case "chin":
                    return 9;
                case "muoi":
                    return 10;
                default:
                    return 0;
            }
        }

        public static string Encrypt(this string toEncrypt)
        {
            if (string.IsNullOrEmpty(toEncrypt))
                return toEncrypt;
            bool useHashing = true;
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        public static string Decrypt(this string toDecrypt)
        {
            if (string.IsNullOrEmpty(toDecrypt))
                return toDecrypt;
            bool useHashing = true;
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }
}
