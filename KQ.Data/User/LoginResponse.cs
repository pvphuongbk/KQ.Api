namespace KQ.DataDto.User
{
    public class LoginResponse
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
        public List<Phonebook> Phonebooks { get; set;}
    }

    public class Phonebook
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double TileXac { get; set; }
        public double TileThuong { get; set;}
        public double TileBaSo { get; set; }
        public string PhoneNumber { get; set; }
    }
    public class TileUserDto
    {
        public int? ID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public double TileXac { get; set; }
        public double TileThuong { get; set; }
        public double TileBaSo { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class UserUpdateDto
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public double TileXac { get; set; }
        public double TileThuong { get; set; }
        public double TileBaSo { get; set; }
    }
}

