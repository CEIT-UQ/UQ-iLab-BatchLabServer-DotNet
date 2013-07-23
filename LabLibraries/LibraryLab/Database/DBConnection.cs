using System;
using System.Data.SqlClient;

namespace Library.Lab.Database
{
    public class DBConnection
    {
        #region Constants
        private const string STR_ClassName = "DBConnection";
        private const Logfile.Level logLevel = Logfile.Level.Finest;
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_DataSourceDatabase_arg2 = "DataSource: {0:s}  Database: {1:s}";
        /*
         * String constants for exception messages
         */
        private const String STRERR_ConnectionString = "connectionString";
        #endregion

        #region Variables
        private String connectionString;
        #endregion

        #region Properties

        public static string ClassName
        {
            get { return STR_ClassName; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public DBConnection(String connectionString)
        {
            const string methodName = "DBConnection";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Check that parameters are valid
                 */
                if (connectionString == null)
                {
                    throw new ArgumentNullException(STRERR_ConnectionString);
                }
                if (connectionString.Trim().Length == 0)
                {
                    throw new ArgumentNullException(STRERR_ConnectionString);
                }

                /*
                 * Save to local variables
                 */
                this.connectionString = connectionString;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SqlConnection GetConnection()
        {
            const String methodName = "GetConnection";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            SqlConnection sqlConnection = null;

            try
            {
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();

                Logfile.Write(String.Format(STRLOG_DataSourceDatabase_arg2, sqlConnection.DataSource, sqlConnection.Database));
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw ex;
            }
            finally
            {
                sqlConnection.Close();
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return sqlConnection;
        }
    }
}
