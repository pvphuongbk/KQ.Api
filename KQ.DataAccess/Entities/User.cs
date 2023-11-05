using KQ.DataAccess.Base;

namespace KQ.DataAccess.Entities
{
    public partial class User
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
        public string? Imei { get; set; }
        public virtual List<TileUser> TileUser { get; set; }
    }
}
