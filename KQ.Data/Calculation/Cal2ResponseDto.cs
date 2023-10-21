namespace KQ.DataDto.Calculation
{
    public class Cal2ResponseDto
    {
        public double Xac { get; set; }
        public double Thuong { get; set; }
        public double Loi { get; set; }
        public List<string> MessageThuong { get; set; }
        public List<string> MessageXac { get; set; }
        public string[] MessageLoi { get; set; }
    }
}