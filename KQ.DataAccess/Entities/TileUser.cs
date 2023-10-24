using KQ.DataAccess.Base;
using KQ.DataAccess.Enum;

namespace KQ.DataAccess.Entities
{
    public class TileUser : TiLeBase
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int UserID { get; set; }
        public virtual User User { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsChu { get; set; }
    }
}