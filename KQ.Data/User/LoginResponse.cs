using KQ.DataAccess.Base;

namespace KQ.DataDto.User
{
    public class LoginResponse : TiLeBase
    {
        public bool IsLoginSuccess { get; set; }
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Account { get; set; }
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
        public List<Phonebook> Phonebooks { get; set;}
    }

    public class Phonebook : TiLeBase
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double TileXac { get; set; }
        public double TileThuong { get; set;}
        public double TileBaSo { get; set; }
        public double DaThang { get; set; }
        public double DaXien { get; set; }
        public double BonSo { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsChu { get; set; }
    }
    public class TileUserDto : TiLeBase
    {
        public int? ID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public double TileXac { get; set; }
        public double TileThuong { get; set; }
        public double TileBaSo { get; set; }
        public string PhoneNumber { get; set; }
        public double DaThang { get; set; }
        public double DaXien { get; set; }
        public double BonSo { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsChu { get; set; }
    }

    public class UserUpdateDto : TiLeBase
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public double TileXac { get; set; }
        public double TileThuong { get; set; }
        public double TileBaSo { get; set; }
        public double DaThang { get; set; }
        public double DaXien { get; set; }
        public double BonSo { get; set; }
    }
}

