using KQ.DataAccess.DBContext;
using KQ.DataAccess.Interface;

namespace KQ.DataAccess.Repositories
{
	public class CommonRepository<T> : Repository<CommonDBContext, T>, ICommonRepository<T> where T : class
	{
		public CommonRepository(CommonDBContext context) : base(context)
		{

		}
	}
}
