using KQ.DataAccess.Base;

namespace KQ.DataAccess.Entities
{
    public partial class User : TiLeBase
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public bool IsDeleted { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public double TileXac { get; set; }
        public double TileThuong { get; set; }
        public double TileBaSo { get; set; }
        public double DaThang { get; set; }
        public double DaXien { get; set; }
        public double BonSo { get; set; }
        public virtual List<TileUser> TileUser { get; set; }
    }
}
