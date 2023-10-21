using KQ.DataAccess.Base;
using KQ.DataDto.Enum;

namespace KQ.DataDto.Calculation
{
    public class Cal3RequestDto : TiLeBase
    {
        public string SynTax { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IDKhach { get; set; }
        public MienEnum Mien { get; set; }
    }
}
