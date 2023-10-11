using KQ.DataAccess.Interface;
using Microsoft.EntityFrameworkCore;

namespace KQ.DataAccess.Base
{
	public abstract class PDataContext : DbContext, IDBContext
	{
		protected PDataContext(DbContextOptions options) : base(options)
		{

		}
	}
}
