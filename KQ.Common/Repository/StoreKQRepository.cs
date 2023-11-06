namespace KQ.Common.Repository
{
    public class StoreKQRepository
    {
        public static bool InsertStoreKQ(DateTime CreatedDate, string HaiCon, string BaCon, string BonCon)
        {
            string insertQuery = @"INSERT INTO [dbo].[StoreKQ](CreatedDate, HaiCon, BaCon,BonCon) 
                                VALUES (@CreatedDate, @HaiCon, @BaCon, @BonCon)";
            var parram = new
            {
                CreatedDate,
                HaiCon,
                BaCon,
                BonCon
            };
            var kq = DapperExtensions.ExecuteByQuery(insertQuery, parram);

            return kq > 0;
        }
        public static bool DeleteDetails()
        {
            DateTime now = DateTime.Now.AddDays(-30);
            string insertQuery = @"Delete [dbo].[Details] where HandlByDate <= @now ";
            var parram = new
            {
                now,
            };
            var kq = DapperExtensions.ExecuteByQuery(insertQuery, parram);
            DateTime now2 = DateTime.Now.AddDays(-90);
            string insertQuery2 = @"Delete [dbo].[StoreKQ] where CreatedDate <= @now2 ";
            var parram2 = new
            {
                now2,
            };
            var kq2 = DapperExtensions.ExecuteByQuery(insertQuery2, parram2);
            return kq > 0;
        }

        public static bool AddOrUpdateStoreKq(DateTime CreatedDate, string HaiCon, string BaCon, string BonCon)
        {
            string insertQuery = @"if (select COUNT(id) from StoreKQ where CAST(CreatedDate as date) = CAST(@CreatedDate as date)) > 0
	                begin
		                update StoreKQ set HaiCon = @HaiCon, BaCon = @BaCon, BonCon = @BonCon
	                end
                    else
  	                begin
		                insert into StoreKQ (CreatedDate,HaiCon, BaCon, BonCon) values (@CreatedDate,@HaiCon,@BaCon,@BonCon)
	                end";
            var parram = new
            {
                CreatedDate,
                HaiCon,
                BaCon,
                BonCon
            };
            var kq = DapperExtensions.ExecuteByQuery(insertQuery, parram);
            return kq > 0;
        }
    }
}
