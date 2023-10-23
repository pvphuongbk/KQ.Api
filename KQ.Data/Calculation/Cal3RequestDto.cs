﻿using KQ.DataAccess.Base;
using KQ.DataAccess.Enum;
using KQ.DataDto.Enum;

namespace KQ.DataDto.Calculation
{
    public class Cal3RequestDto
    {
        public string SynTax { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IDKhach { get; set; }
        public MienEnum Mien { get; set; }
        public bool IsSave { get; set; }
        public CachTrungDa CachTrungDaThang { get; set; }
        public CachTrungDa CachTrungDaXien { get; set; }
    }
}