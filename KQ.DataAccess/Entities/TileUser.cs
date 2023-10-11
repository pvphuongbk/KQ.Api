namespace KQ.DataAccess.Entities
{
    public partial class TileUser
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int UserID { get; set; }
        public virtual User User { get; set; }
        public double TileXac { get; set; }
        public double TileThuong { get; set; }
        public double TileBaSo { get; set; }
        public string PhoneNumber { get; set; }
        public double DaThang { get; set; }
        public double DaXien { get; set; }
        public double BonSo { get; set; }
    }
}