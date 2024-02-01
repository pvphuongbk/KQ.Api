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
			SqlConnection = GetConfigValue("ConnectionStrings:KQConn", "No connection");
            Isbackup = GetConfigValue("AppSettings:Isbackup", false);
            SaveRestoreBak = GetConfigValue("AppSettings:SaveRestoreBak", "No connection");
            IsRestore = GetConfigValue("AppSettings:IsRestore", false);
        }

        public static bool Isbackup { get; set; }
		public static string SqlConnection { get; set; }
        public static bool IsRestore { get; set; }
        public static string SaveRestoreBak { get; set; }
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
    }
}
