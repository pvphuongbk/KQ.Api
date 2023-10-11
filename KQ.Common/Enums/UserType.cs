using System.ComponentModel;

namespace KQ.Common.Enums
{
	public enum UserType
	{
		[Description("Admin")]
		Admin = 1,
		[Description("Teacher")]
		Teacher = 2,
		[Description("User")]
		User = 3
	}
}
