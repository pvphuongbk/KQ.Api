using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static KQ.DataAccess.Utilities.AuditConstant;

namespace KQ.DataAccess.Base
{
	public class AuditEntry : BaseEntity
	{
		public AuditEntry(EntityEntry entry)
		{
			Entry = entry;
		}

		public EntityEntry Entry { get; }

		public Guid PrimaryKey { get; set; }
		public Guid? ParentKey { get; set; }
		public Guid? ActionBy { get; set; }
		public DateTime? ActionDate { get; set; }

		public string TableName { get; set; }
		public string State { get; set; }
		public string PrimaryValue { get; set; }
		public string PrimaryName { get; set; }

		public string Remarks { get; set; }

		public AuditModule AuditModule { get; set; }

		public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
		public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
		public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
		public Dictionary<string, object> MapValues { get; } = new Dictionary<string, object>();

		public Dictionary<string, object> DisplayName { get; } = new Dictionary<string, object>();
		public Dictionary<string, object> Sections { get; } = new Dictionary<string, object>();

		public Dictionary<string, object> PrimaryValues { get; set; } = new Dictionary<string, object>();
		public Dictionary<string, object> PrimaryNames { get; set; } = new Dictionary<string, object>();

		public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();
		public List<string> PropertyChanged { get; } = new List<string>();
		public bool HasTemporaryProperties => TemporaryProperties.Any();

		public AuditItem ToAudit()
		{
			var auditItem = new AuditItem
			{
				TableName = TableName,
				PrimaryKey = PrimaryKey,
				ParentKey = ParentKey,
				ActionBy = ActionBy,
				ActionDate = ActionDate,
				State = State,
				KeyValues = JsonSerializer.Serialize(KeyValues), //JsonConvert.SerializeObject(KeyValues),   
				DateTime = DateTime.UtcNow,
			};
			foreach (var propName in PropertyChanged)
			{
				var auditField = new AuditField { FieldName = propName };
				if (OldValues.ContainsKey(propName))
					auditField.OldValue = OldValues[propName]?.ToString();

				if (NewValues.ContainsKey(propName))
					auditField.NewValue = NewValues[propName]?.ToString();

				if (MapValues.ContainsKey(propName))
				{
					auditField.MapValue = MapValues[propName]?.ToString();
				}

				if (DisplayName.ContainsKey(propName))
				{
					auditField.DisplayName = DisplayName[propName]?.ToString();
				}

				if (Sections.ContainsKey(propName))
				{
					auditField.Section = Sections[propName]?.ToString();
				}

				auditField.PrimaryValue = PrimaryValue;
				auditField.PrimaryName = PrimaryName;
				auditField.PrimaryNames = PrimaryNames;
				auditField.PrimaryValues = PrimaryValues;
				auditField.Remarks = Remarks;
				auditItem.AuditFields.Add(auditField);
			}
			return auditItem;
		}
	}
}
