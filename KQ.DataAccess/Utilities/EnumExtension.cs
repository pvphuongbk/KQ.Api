using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KQ.DataAccess.Utilities
{
	public static class EnumExtension
	{
		/// <summary>
		/// Get Name Enum
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetEnumDescriptionName<T>(this T value)
		{
			try
			{
				var fi = value.GetType().GetField(value.ToString());

				var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

				return attributes.Length > 0 ? attributes[0].Description : value.ToString();
			}
			catch
			{
				return string.Empty;
			}
		}

		public static string GetAuditDescription<T>(this T value)
		{
			try
			{
				var attributes = (AuditAttribute[])null;
				var fi = value.GetType().GetField(value.ToString());

				if (fi != null)
				{
					attributes = (AuditAttribute[])fi.GetCustomAttributes(typeof(AuditAttribute), false);
				}

				return attributes != null && attributes.Length > 0 ? attributes[0].Description : string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		public static bool HasAuditDescription<T>(this T value)
		{
			try
			{
				var attributes = (AuditAttribute[])null;
				var fi = (value as PropertyEntry);

				if (fi != null)
				{
					attributes = (AuditAttribute[])fi.Metadata.PropertyInfo.GetCustomAttributes(typeof(AuditAttribute), false);
				}

				return attributes != null && attributes.Length > 0;
			}
			catch
			{
				return false;
			}
		}

		public static string GetAuditDescriptionName<T>(this T value)
		{
			try
			{
				var attributes = (AuditAttribute[])null;
				var fi = (value as PropertyEntry);

				if (fi != null)
				{
					attributes = (AuditAttribute[])fi.Metadata.PropertyInfo.GetCustomAttributes(typeof(AuditAttribute), false);
				}

				return attributes != null && attributes.Length > 0 ? attributes[0].Description : string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}


		public static bool IsAuditPrimaryKey<T>(this T value)
		{
			try
			{
				var fi = (value as PropertyEntry);
				var attributes = (AuditPrimaryKeyAttribute[])null;
				if (fi != null)
				{
					attributes = (AuditPrimaryKeyAttribute[])fi.Metadata.PropertyInfo.GetCustomAttributes(typeof(AuditPrimaryKeyAttribute), false);
				}
				return attributes != null && attributes.Length > 0;
			}
			catch
			{
				return false;
			}
		}

		public static bool IsAuditMappingValue<T>(this T value)
		{
			try
			{
				var fi = (value as PropertyEntry);
				var attributes = (AuditAttribute[])null;
				if (fi != null)
				{
					attributes = (AuditAttribute[])fi.Metadata.PropertyInfo.GetCustomAttributes(typeof(AuditAttribute), false);
				}

				return attributes?.FirstOrDefault()?.IsMapping ?? false;
			}
			catch
			{
				return false;
			}
		}

		public static string GetAuditDescriptionName(this PropertyInfo prop)
		{
			var attributes = prop.GetCustomAttributes(typeof(AuditAttribute), false) as AuditAttribute[];
			return attributes != null && attributes.Length > 0 ? attributes[0].Description : string.Empty;
		}

		public static bool HasAuditDescriptionName(this PropertyInfo prop)
		{
			var attributes = prop.GetCustomAttributes(typeof(AuditAttribute), false) as AuditAttribute[];
			return attributes != null && attributes.Length > 0;
		}

		public static bool IsAuditPrimaryKeyProperty(this PropertyInfo prop)
		{
			try
			{
				var attributes = prop.GetCustomAttributes(typeof(AuditPrimaryKeyAttribute), false) as AuditPrimaryKeyAttribute[];
				return attributes != null && attributes.Length > 0;
			}
			catch
			{
				return false;
			}
		}

		public static string GetAuditSectionDescriptionName<T>(this T value)
		{
			try
			{
				var attributes = (AuditSectionAttribute[])null;
				var fi = (value as PropertyEntry);

				if (fi != null)
				{
					attributes = (AuditSectionAttribute[])fi.Metadata.PropertyInfo.GetCustomAttributes(typeof(AuditSectionAttribute), false);
				}

				return attributes != null && attributes.Length > 0 ? attributes[0].Description : null;
			}
			catch
			{
				return null;
			}
		}

		public static string GetAuditSectionDescriptionName(this PropertyInfo prop)
		{
			var attributes = prop.GetCustomAttributes(typeof(AuditSectionAttribute), false) as AuditSectionAttribute[];
			return attributes != null && attributes.Length > 0 ? attributes[0].Description : null;
		}
	}
}
