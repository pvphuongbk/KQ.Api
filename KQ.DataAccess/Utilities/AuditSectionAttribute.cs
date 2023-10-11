namespace KQ.DataAccess.Utilities
{
	[AttributeUsage(AttributeTargets.All)]
	public class AuditSectionAttribute : Attribute
	{
		public static readonly AuditSectionAttribute Default = new AuditSectionAttribute();

		public AuditSectionAttribute() : this(string.Empty)
		{

		}
		public AuditSectionAttribute(string description) => DescriptionValue = description;

		public AuditSectionAttribute(string description, bool isMappingValue) : this(description)
			=> IsMappingValue = isMappingValue;

		public virtual string Description => DescriptionValue;
		public virtual bool IsMapping => IsMappingValue;

		protected string DescriptionValue { get; set; }
		protected bool IsMappingValue { get; set; }

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}

			return obj is AuditSectionAttribute other && other.Description == Description;
		}

		public override int GetHashCode() => Description.GetHashCode();

		public override bool IsDefaultAttribute() => Equals(Default);
	}
}
