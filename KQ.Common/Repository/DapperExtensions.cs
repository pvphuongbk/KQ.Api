using Dapper;
using KQ.Common.Configuration;
using KQ.Common.Enums;
using KQ.Common.Helpers;
using System.Data;
using System.Reflection;

namespace KQ.Common.Repository
{
    public class DapperExtensions
    {
        internal static string ConnectionString = AppConfigs.SqlConnection;

        public static IEnumerable<T> QueryDapperStoreProc<T>(string store, object param = null)
        {
            try
            {
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConnectionString))
                {
                    conn.Open();
                    var result = conn.Query<T>(store, param, commandType: CommandType.StoredProcedure);
                    conn.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                FileHelper.GeneratorFileByDay(FileStype.Error, $"store: {store}. " + ex.Message, "DapperExtensions." + MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }

        public static int ExecuteDapperStoreProc(string store, object param = null)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConnectionString))
            {
                conn.Open();
                int result = 0;
                try
                {
                    if (param == null)
                        param = new DynamicParameters();

                    ((DynamicParameters)param).Add(name: "@Result", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                    conn.Execute(store, param, commandType: CommandType.StoredProcedure);

                    result = ((DynamicParameters)param).Get<int>("@Result");
                }
                catch (Exception ex)
                {
                    FileHelper.GeneratorFileByDay(FileStype.Error, $"store: {store}. " + ex.ToString(), "DapperExtensions." + MethodBase.GetCurrentMethod().Name);
                    return result;
                }
                conn.Close();
                return result;
            }
        }

        public static List<T> QueryEnumerableByText<T>(string query, object param)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConnectionString))
            {
                conn.Open();
                try
                {
                    var result = conn.Query<T>(query, param).ToList();
                    return result;
                }
                catch (Exception ex)
                {
                    FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "DapperExtensions." + MethodBase.GetCurrentMethod().Name);
                    return default(List<T>);
                }
            }
        }

        /// <summary>
        ///  Query by text - Chỉ query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int NonQueryByText(string query, object param = null)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConnectionString))
            {
                conn.Open();
                try
                {
                    var result = conn.Execute(query, param);
                    return result;
                }
                catch (Exception ex)
                {
                    FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "DapperExtensions." + MethodBase.GetCurrentMethod().Name);
                    return -1;
                }
            }
        }

        /// <summary>
        /// Query by text - Select một đối tượng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T QueryNonEnumerableByText<T>(string query, object param)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConnectionString))
            {
                conn.Open();
                try
                {
                    var result = conn.Query<T>(query, param).FirstOrDefault();
                    return result;
                }
                catch (Exception ex)
                {
                    FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "DapperExtensions." + MethodBase.GetCurrentMethod().Name);
                    return default(T);
                }
            }
        }
        public static int ExecuteByQuery(string query, object ob)
        {
            var result = -1;
            using (System.Data.SqlClient.SqlConnection db = new System.Data.SqlClient.SqlConnection(ConnectionString))
            {
                result = db.Execute(query, ob);

                return result;
            }
        }
        public static T GetFirstOrDefault<T>(string query)
        {
            var result = default(T);
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();

                    result = conn.Query<T>(query).ToList().FirstOrDefault();
                }
                catch (Exception ex)
                {
                    FileHelper.GeneratorFileByDay(FileStype.Error, ex.ToString(), "DapperExtensions." + MethodBase.GetCurrentMethod().Name);
                    result = default(T);
                }
                conn.Close();

                return result;
            }
        }
    }
}
