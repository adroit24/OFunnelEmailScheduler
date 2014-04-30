using System.Data;
using OFunnelEmailScheduler.OFunnelUtilities;

namespace OFunnelEmailScheduler.OFunnelDbLogic
{
    /// <summary>
    /// BusinessLogic class to execute the all Sql queries using SqlHelper Class 
    /// </summary>
    public class BusinessLogic
    {
        private static string strConnection;

        /// <summary>
        /// Counctructor BusinessLogic 
        /// </summary>
        static BusinessLogic()
        {
            strConnection = Config.OFunnelDbConnection;
        }

        #region ExecuteDataSet

        public static DataSet Exec_Dataset_string(string sqlString)
        {
            DataSet ds;
            ds = SqlHelper.ExecuteDataset(strConnection, CommandType.Text, sqlString);
            return ds;
        }
        #endregion
    }
}