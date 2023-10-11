namespace KQ.DataAccess.Utilities
{
	[AttributeUsage(AttributeTargets.All)]
	public class AuditAttribute : Attribute
	{
		public static readonly AuditAttribute Default = new AuditAttribute();

		public AuditAttribute() : this(string.Empty)
		{

		}
		public AuditAttribute(string description) => DescriptionValue = description;

		public AuditAttribute(string description, bool isMappingValue) : this(description)
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

			return obj is AuditAttribute other && other.Description == Description;
		}

		public override int GetHashCode() => Description.GetHashCode();

		public override bool IsDefaultAttribute() => Equals(Default);
	}
}
