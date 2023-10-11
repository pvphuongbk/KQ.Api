using KQ.DataAccess.Interface;
using System.ComponentModel.DataAnnotations;

namespace KQ.DataAccess.Base
{
	public class BaseEntity : IBaseEntity
	{
		public BaseEntity()
		{
			Deleted = 0;
		}

		[Key]
		public int Id { get; set; }
		public int Deleted { get; set; }
	}
}
