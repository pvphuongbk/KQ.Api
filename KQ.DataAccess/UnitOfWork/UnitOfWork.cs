using KQ.DataAccess.Base;
using KQ.DataAccess.Interface;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KQ.DataAccess.Utilities;

namespace KQ.DataAccess.UnitOfWork
{
	public class UnitOfWork<D> : IUnitOfWork<D> where D : PDataContext
	{
		protected List<string> WhiteListTracking = new List<string>();
		protected List<string> ConvertListTracking = new List<string>();
		protected List<string> ParseListTracking = new List<string>();
		protected Dictionary<string, List<string>> MappingListTracking = new Dictionary<string, List<string>>();
		protected List<string> ExcludeDeletedTables = new List<string>();
		private readonly PDataContext _context;
		private IDbContextTransaction _transaction;
		public virtual void BeginTransaction()
		{
			_transaction = _context.Database.BeginTransaction();
		}

		public UnitOfWork(D context)
		{
			_context = context;
		}

		protected virtual void CreateWhiteList()
		{

		}

		public void RollBack()
		{
			if (_transaction != null)
			{
				_transaction.Rollback();
				_transaction.Dispose();
			}
		}

		public int Commit(bool isTrackChanged = true)
		{
			//var auditEntries = OnBeforeSave(isTrackChanged);            
			var result = _context.SaveChanges();
			//OnAfterSave(auditEntries);
			if (_transaction != null)
			{
				_transaction.Commit();
				_transaction.Dispose();
			}
			return result;
		}
		public async Task<int> CommitAsync(bool isTrackChanged = true)
		{
			//var auditEntries = OnBeforeSave(isTrackChanged);
			AuditDateUpdate();
			var result = await _context.SaveChangesAsync();
			// OnAfterSave(auditEntries);
			return result;
		}

		public async Task<int> CommitWithMalayTimeZone(bool isTrackChanged = true)
		{
			AuditDateUpdateWithMalayTimeZone();
			var result = await _context.SaveChangesAsync();
			return result;
		}

		public async Task<int> SimpleCommitWithMalayTimeZone()
		{
			var result = await _context.SaveChangesAsync();
			return result;
		}

		public void Dispose()
		{
			_context.Dispose();
		}

		private void AuditDateUpdateWithMalayTimeZone()
		{
			var dateTime = DateTime.UtcNow.AddHours(8);
			_context.ChangeTracker.DetectChanges();
			var entries = _context.ChangeTracker.Entries()
				.Where(e => e.Entity is BaseEntity && (
						e.State == EntityState.Added
						|| e.State == EntityState.Modified));

			foreach (var entityEntry in entries)
			{
				if (entityEntry.Entity is BaseEntity trackable)
				{
					if (entityEntry.State == EntityState.Added)
					{
						//trackable.CreatedDate = dateTime;
						//trackable.ModifiedDate = dateTime;
					}
					else
					{
						//trackable.ModifiedDate = dateTime;
						//entityEntry.Property(nameof(trackable.CreatedDate)).IsModified = false;
					}
				}
			}

		}

		private void AuditDateUpdate()
		{
			var dateTime = DateTime.UtcNow;
			_context.ChangeTracker.DetectChanges();
			var entries = _context.ChangeTracker.Entries()
				.Where(e => e.Entity is BaseEntity && (
						e.State == EntityState.Added
						|| e.State == EntityState.Modified));

			foreach (var entityEntry in entries)
			{
				if (entityEntry.Entity is BaseEntity trackable)
				{
					if (entityEntry.State == EntityState.Added)
					{
						//trackable.CreatedDate = dateTime;
						//trackable.ModifiedDate = dateTime;
					}
					else
					{
						//trackable.ModifiedDate = dateTime;
						//entityEntry.Property(nameof(trackable.CreatedDate)).IsModified = false;
					}
				}
			}

		}

		private List<AuditEntry> OnBeforeSave(bool isTrackChanged = true)
		{
			if (!isTrackChanged)
			{
				return null;
			}

			_context.ChangeTracker.DetectChanges();
			var auditEntries = new List<AuditEntry>();
			var entries = _context.ChangeTracker.Entries().ToList();

			foreach (var entry in entries)
			{
				var tableChanged = entry.Metadata.GetTableName();
				if (entry.Entity is Audit || entry.State == EntityState.Detached
										  || entry.State == EntityState.Unchanged
										  || !WhiteListTracking.Any(t =>
											  t.Contains(tableChanged, StringComparison.CurrentCultureIgnoreCase)))
					continue;

				var auditEntry = GetPropertyChanged(entry);
				if (auditEntry != null)
				{
					BeforeSave(entry, auditEntry);
					auditEntries.Add(auditEntry);
				}

			}

			// keep a list of entries where the value of some properties are unknown at this step
			return auditEntries.ToList();
		}

		protected virtual void BeforeSave(EntityEntry entry, AuditEntry auditEntry)
		{ }

		protected virtual AuditEntry GetPropertyChanged(EntityEntry entry)
		{
			if (entry == null)
			{
				return null;
			}

			string tableName = entry.Metadata.GetTableName();
			var auditEntry = new AuditEntry(entry)
			{
				TableName = tableName
			};

			var baseProperties = typeof(BaseEntity).GetProperties();

			foreach (var property in entry.Properties)
			{
				//if (property.IsTemporary)
				//{
				//    // value will be generated by the database, get the value after saving
				//    auditEntry.TemporaryProperties.Add(property);
				//    continue;
				//}

				string propertyName = property.Metadata.Name;
				if (property.Metadata.IsPrimaryKey())
				{
					auditEntry.KeyValues[propertyName] = property.CurrentValue;
					if (property.CurrentValue is Guid)
					{
						auditEntry.PrimaryKey = new Guid(property.CurrentValue.ToString());
					}

					continue;
				}

				GetPrimaryValue(propertyName, auditEntry, property);
				var currentValue = ConvertValue(propertyName, property, out var originalValue);

				if (IgnoreAudit(entry, property, baseProperties) || !property.HasAuditDescription())
				{

					if (propertyName == "IsDeleted" && entry.State != EntityState.Added &&
						property.CurrentValue?.GetHashCode() != property.OriginalValue?.GetHashCode() && !ExcludeDeletedTables.Contains(tableName))
					{
						auditEntry.State = AuditConstant.AuditState.Deleted.GetEnumDescriptionName();
						auditEntry.OldValues[propertyName] = originalValue;
						auditEntry.NewValues[propertyName] = currentValue;
						MappingValue(propertyName, auditEntry, property);
						auditEntry.PropertyChanged.Add(propertyName);
					}

					continue;
				}

				if (string.IsNullOrEmpty(currentValue?.ToString()) && string.IsNullOrEmpty(originalValue?.ToString()) && entry.State == EntityState.Added)
				{
					continue;
				}

				var auditDescriptionName = property.GetAuditDescriptionName();

				if (string.IsNullOrEmpty(auditDescriptionName))
				{
					auditDescriptionName = propertyName;
				}

				auditEntry.DisplayName[propertyName] = auditDescriptionName;

				var auditSectionDescriptionName = property.GetAuditSectionDescriptionName();
				if (auditSectionDescriptionName != null)
				{
					auditEntry.Sections[propertyName] = auditSectionDescriptionName;
				}

				switch (entry.State)
				{
					case EntityState.Added:
						auditEntry.State = AuditConstant.AuditState.Added.GetEnumDescriptionName();
						if (IsParseJsonValue(propertyName, tableName))
						{
							ParseJsonValue(auditEntry, property, entry);
						}
						else
						{
							auditEntry.NewValues[propertyName] = currentValue;
							MappingValue(propertyName, auditEntry, property);
							auditEntry.PropertyChanged.Add(propertyName);
						}

						break;

					case EntityState.Deleted:
						auditEntry.State = AuditConstant.AuditState.Deleted.GetEnumDescriptionName();
						auditEntry.OldValues[propertyName] = originalValue;
						MappingValue(propertyName, auditEntry, property);
						auditEntry.PropertyChanged.Add(propertyName);
						break;

					case EntityState.Modified:
						if (property.IsModified)
						{
							auditEntry.State = AuditConstant.AuditState.Modified.GetEnumDescriptionName();
							if (IsParseJsonValue(propertyName, tableName))
							{
								ParseJsonValue(auditEntry, property, entry);
							}
							else
							{
								auditEntry.OldValues[propertyName] = originalValue;
								auditEntry.NewValues[propertyName] = currentValue;
								MappingValue(propertyName, auditEntry, property);
								auditEntry.PropertyChanged.Add(propertyName);
							}
						}

						break;
				}
			}

			if (string.IsNullOrEmpty(auditEntry.PrimaryValue))
			{
				GetPrimaryValue(auditEntry, entry);
			}

			return auditEntry;
		}

		protected virtual bool IsParseJsonValue(string propertyName, string tableName)
		{
			var isParseJson = ParseListTracking.Any(t => t.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));
			return isParseJson;
		}

		protected virtual AuditEntry ParseJsonValue(AuditEntry auditEntry, PropertyEntry property, EntityEntry entry)
		{
			return auditEntry;
		}

		private object ConvertValue(string propertyName, PropertyEntry property, out object originalValue)
		{
			var isConvert = ConvertListTracking.Any(t => t.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));

			var currentValue = property.CurrentValue;
			originalValue = property.OriginalValue;
			if (isConvert)
			{
				currentValue = ConvertUnit(currentValue);
				originalValue = ConvertUnit(originalValue);
			}
			else
			{
				currentValue = UpdateFormatDecimal(currentValue);
				originalValue = UpdateFormatDecimal(originalValue);
			}

			return currentValue;
		}

		protected virtual void MappingValue(string propertyName, AuditEntry auditEntry, PropertyEntry property)
		{
			var isMap = IsMapValue(propertyName, auditEntry, MappingListTracking) || property.IsAuditMappingValue();

			if (isMap)
			{
				auditEntry.MapValues[propertyName] = true;
			}
		}

		protected virtual AuditEntry GetPrimaryValue(AuditEntry auditEntry, EntityEntry entry)
		{
			return auditEntry;
		}

		protected virtual AuditEntry GetPrimaryValue(string propertyName, AuditEntry auditEntry, PropertyEntry property)
		{
			if (property.IsAuditPrimaryKey())
			{
				auditEntry.PrimaryValue = property.CurrentValue?.ToString();
				auditEntry.PrimaryName = propertyName;
			}

			return auditEntry;
		}


		protected virtual bool IsMapValue(string propertyName, AuditEntry auditEntry, Dictionary<string, List<string>> mappingListTracking)
		{
			var tableName = auditEntry.TableName;
			if (!mappingListTracking.ContainsKey(tableName))
			{
				tableName += "s";
			}
			return mappingListTracking.ContainsKey(tableName) && mappingListTracking[tableName]
				.Any(t => t.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));
		}

		protected virtual bool IgnoreAudit(EntityEntry entry, PropertyEntry property, PropertyInfo[] baseProperties)
		{
			var propertyName = property.Metadata.Name;

			var isBaseProperty =
				baseProperties?.Any(t => t.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase)) ==
				true;
			if (property.Metadata.IsForeignKey() || isBaseProperty)
			{
				return true;
			}

			if (entry.State != EntityState.Added &&
				property.CurrentValue?.GetHashCode() == property.OriginalValue?.GetHashCode())
			{
				return true;
			}

			if (entry.State == EntityState.Added && property.CurrentValue == null)
			{
				return true;
			}

			return false;
		}

		protected virtual object ConvertUnit(object currentValue)
		{
			return currentValue;
		}

		protected virtual void SetBaseData(EntityEntry entry, AuditEntry auditEntry, BaseEntity finding)
		{
			//auditEntry.CreatedBy = finding.CreatedBy;
			//auditEntry.ModifiedBy = finding.ModifiedBy;
			//auditEntry.CreatedDate = finding.CreatedDate;
			//auditEntry.ModifiedDate = finding.ModifiedDate;

			switch (entry.State)
			{
				case EntityState.Added:
					//auditEntry.ActionBy = finding.CreatedBy;
					auditEntry.ActionDate = DateTime.UtcNow;
					break;
				case EntityState.Modified:
					//auditEntry.ActionBy = finding.ModifiedBy;
					auditEntry.ActionDate = DateTime.UtcNow;
					break;
			}
		}

		protected virtual object UpdateFormatDecimal(object currentValue)
		{
			if (currentValue != null)
			{
				try
				{
					if (currentValue is decimal value)
					{
						currentValue = value.ToString("G29");
					}
				}
				catch
				{
					//Ignored.
				}
			}
			return currentValue;
		}

		private void OnAfterSave(List<AuditEntry> auditEntries)
		{
			if (auditEntries == null || auditEntries.Count == 0)
				return;
			Audit auditHeader = new Audit();
			foreach (var auditEntry in auditEntries)
			{
				// Get the final value of the temporary properties
				foreach (var prop in auditEntry.TemporaryProperties)
				{
					if (prop.Metadata.IsPrimaryKey())
					{
						auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
					}
					else
					{
						auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
					}
				}
				// Save the Audit entry
				if (auditEntry.KeyValues.Any())
				{
					auditHeader.Items.Add(auditEntry.ToAudit());
				}

			}
			AfterSave(auditHeader);
		}

		public virtual void AfterSave(Audit audits)
		{

		}
	}
}
