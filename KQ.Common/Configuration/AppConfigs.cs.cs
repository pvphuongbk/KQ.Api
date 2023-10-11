using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace KQ.Common.Configuration
{
	public class AppConfigs
	{
		private static IConfiguration _configuration;
		public static void LoadAll(IConfiguration configuration)
		{
			_configuration = configuration;
			ApiUrlBase = GetConfigValue("Appconfig:ApiUrlBase", "");
            ApiAir= GetConfigValue("Appconfig:ApiAir", "");
            ItemPerPage = GetConfigValue("Appconfig:ItemPerPage", 50);
			SqlConnection = GetConfigValue("ConnectionStrings:KQConn", "No connection");
			GoogleClientId = GetConfigValue("Google:ClientId", "No value");
			GoogleClientSecret = GetConfigValue("Google:ClientSecret", "No value");
			FacebookClientId = GetConfigValue("Facebook:ClientId", "No value");
			FacebookClientSecret = GetConfigValue("Facebook:ClientSecret", "No value");
            TemplateFolder = GetConfigValue("Template:Folder", "No value");
            FileNhanSu = GetConfigValue("Template:FileNhanSu", "No value");
            FileTongHopNhanSu = GetConfigValue("Template:FileTongHopNhanSu", "No value");
            FileBaoCaoKetQuaKinhDoanh = GetConfigValue("Template:FileBaoCaoKetQuaKinhDoanh", "No value");
            LogPath = GetConfigValue("Log:Path", "No value");
        }

        public static string FormatCurrency(string currencyCode, decimal amount)
        {
            CultureInfo culture = (from c in CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                   let r = new RegionInfo(c.LCID)
                                   where r != null
                                   && r.ISOCurrencySymbol.ToUpper() == currencyCode.ToUpper()
                                   select c).FirstOrDefault();
            if (culture == null)
            {
                // fall back to current culture if none is found
                // you could throw an exception here if that's not supposed to happen
                culture = CultureInfo.CurrentCulture;

            }
            culture = (CultureInfo)culture.Clone();
            culture.NumberFormat.CurrencySymbol = currencyCode;
            culture.NumberFormat.CurrencyPositivePattern = culture.NumberFormat.CurrencyPositivePattern == 0 ? 2 : 3;
            var cnp = culture.NumberFormat.CurrencyNegativePattern;
            switch (cnp)
            {
                case 0: cnp = 14; break;
                case 1: cnp = 9; break;
                case 2: cnp = 12; break;
                case 3: cnp = 11; break;
                case 4: cnp = 15; break;
                case 5: cnp = 8; break;
                case 6: cnp = 13; break;
                case 7: cnp = 10; break;
            }
            culture.NumberFormat.CurrencyNegativePattern = cnp;

            return amount.ToString("C" + ((amount % 1) == 0 ? "0" : "2"), culture);
        }
        const string BuildVersionMetadataPrefix = "+build";
        const string dateFormat = "yyyy-MM-ddTHH:mm:ss:fffZ";


        public static DateTime GetLinkerTime(Assembly assembly)
        {
            var attribute = assembly
              .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value[(index + BuildVersionMetadataPrefix.Length)..];

                    return DateTime.ParseExact(
                        value,
                      dateFormat,
                      CultureInfo.InvariantCulture);
                }
            }
            return default;
        }
        public static string LogPath { get; set; }
        public static string ApiUrlBase { get; set; }
        public static string ApiAir { get; set; }
        public static string TemplateFolder { get; set; }
        public static string FileNhanSu { get; set; }
        public static string FileTongHopNhanSu { get; set; }
        public static string FileBaoCaoKetQuaKinhDoanh { get; set; }
        public static string GoogleClientId { get; set; }
		public static string GoogleClientSecret { get; set; }
		public static string FacebookClientId { get; set; }
		public static string FacebookClientSecret { get; set; }
		public static int ItemPerPage { get; set; }
		public static string SqlConnection { get; set; }
        /// <summary>
        /// plhd
        /// </summary>
        /// 

        public static int acvivemenu = 0;
        /// <summary>
        /// Lấy ra giá trị config trong file .config
        /// </summary>
        private static T GetConfigValue<T>(string configKey, T defaultValue)
		{
			var value = defaultValue;
			var converter = TypeDescriptor.GetConverter(typeof(T));
			try
			{
				if (converter != null)
				{
					var setting = _configuration.GetSection(configKey).Value;
					if (!string.IsNullOrEmpty(setting))
					{
						value = (T)converter.ConvertFromString(setting);
					}
				}
			}
			catch
			{
				value = defaultValue;
			}
			return value;
		}

        public static string Activecss(int tab, int tabactive)
		{
			if(tab == tabactive)
			{
				return "active";
			}
			else
			{
				return "";
			}
		}
    }
}
