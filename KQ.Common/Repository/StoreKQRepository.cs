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
    }
}
