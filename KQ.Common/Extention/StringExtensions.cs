using KQ.Common.Helpers;
using System.ComponentModel;
using System.Text.RegularExpressions;
using KQ.Common.Constants;

namespace KQ.Common.Extention
{
	public static class StringExtensions
	{
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
            for (int i = 1; i < arr.Length;i++)
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
            var sy = input.ToLower();
            sy = Regex.Replace(sy, @"\t|\n|\r", " ");
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
        public static int StringToInt(this string it)
        {
            return int.Parse(it);
        }
        public static List<string> ChanelIntToString(this List<int> it)
        {
            List<string> list = new List<string>();
            var chanels = InnitRepository._chanelCode;
            if(Constants.Constants.IstestMode)
            {
                chanels = InnitRepository._chanelCodeForTest;
            }
            foreach (int i in it)
            {
                list.Add(chanels.FirstOrDefault(x => x.Key == i).Value.FirstOrDefault());
            }
            return list;
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
    }
}
