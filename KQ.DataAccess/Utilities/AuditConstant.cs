using System.ComponentModel;

namespace KQ.DataAccess.Utilities
{
	public class AuditConstant
	{
		public enum AuditModule
		{
			UnKnown = 0,
			Asset = 1,
			Inspection = 2,
			Monitoring = 3,
			TaskManager = 4,
			Incident = 5,
			Assessment = 6,
		}


		public enum AuditState
		{
			Detached = 0,
			Unchanged = 1,
			[Description("Delete")]
			Deleted = 2,
			[Description("Edit")]
			Modified = 3,
			[Description("Add")]
			Added = 4
		}
	}
}
