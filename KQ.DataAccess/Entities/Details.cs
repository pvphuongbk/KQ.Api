﻿namespace KQ.DataAccess.Entities
{
    public partial class Details
    {
        public int ID { get; set; }
        public int IDKhach { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Detail { get; set; }
        public bool IsTinh { get; set; }
        public DateTime HandlByDate { get; set; }
        public string Message { get; set; }
    }
}
