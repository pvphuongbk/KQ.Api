using Microsoft.EntityFrameworkCore;
using KQ.DataAccess.Base;
using KQ.DataAccess.Entities;

namespace KQ.DataAccess.DBContext
{
    public partial class CommonDBContext : PDataContext
    {
        public CommonDBContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<User> User { get; set; } = null!;
        public virtual DbSet<TileUser> TileUser { get; set; } = null!;
        public virtual DbSet<StoreKQ> StoreKQ { get; set; } = null!;
        public virtual DbSet<Details> Details { get; set; } = null!;

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
