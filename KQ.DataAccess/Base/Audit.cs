using KQ.DataAccess.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.DataAccess.Base
{
	public class Audit
	{
		public Audit()
		{
			Id = Guid.NewGuid();
			Items = new List<AuditItem>();
		}
		public Guid Id { get; set; }
		public Guid? ParentKey { get; set; }
		public Guid? ActionBy { get; set; }
		public DateTime? ActionDate { get; set; }

		public string PerformedBy { get; set; }
		public DateTime DateTime { get; set; }
		public string URL { get; set; }
		public string UserAgent { get; set; }
		public string UserHostAddress { get; set; }
		public string Operation { get; set; }
		public string ObjectName { get; set; }
		public string Remark { get; set; }
		public string HashCode { get; set; }
		public AuditConstant.AuditModule AuditModule { get; set; }

		public List<AuditItem> Items { get; set; }
	}

	public class AuditItem
	{
		public AuditItem()
		{
			Id = Guid.NewGuid();
			AuditFields = new List<AuditField>();
		}

		public Guid AuditId { get; set; }
		public Guid Id { get; set; }
		public Guid? PrimaryKey { get; set; }
		public Guid? ParentKey { get; set; }
		public Guid? ActionBy { get; set; }
		public DateTime? ActionDate { get; set; }

		public string TableName { get; set; }
		public string State { get; set; }
		public string KeyValues { get; set; }
		public DateTime DateTime { get; set; }
		public List<AuditField> AuditFields { get; set; }
	}

	public class AuditField
	{
		public AuditField()
		{
			Id = Guid.NewGuid();
		}

		public Guid AuditItemId { get; set; }
		public Guid Id { get; set; }
		public string Section { get; set; }
		public string FieldName { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
		public string MapValue { get; set; }
		public string HashCode { get; set; }

		public string DisplayName { get; set; }
		public string PrimaryValue { get; set; }
		public string PrimaryName { get; set; }
		public string Remarks { get; set; }

		public Dictionary<string, object> PrimaryValues { get; set; } = new Dictionary<string, object>();
		public Dictionary<string, object> PrimaryNames { get; set; } = new Dictionary<string, object>();

	}
}
