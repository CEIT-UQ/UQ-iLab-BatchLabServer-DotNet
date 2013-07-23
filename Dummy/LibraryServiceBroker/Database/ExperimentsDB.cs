using System;
using System.Data.SqlClient;
using Library.Lab;
using Library.Lab.Database;
using Library.ServiceBroker.Database.Types;
using System.Data;
using System.Collections;

namespace Library.ServiceBroker.Database
{
    public class ExperimentsDB
    {
        #region Constants
        private const String STR_ClassName = "ExperimentsDB";
        private const Logfile.Level logLevel = Logfile.Level.Finest;
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_ExperimentId_arg = "ExperimentId: {0:d}";
        private const String STRLOG_Count_arg = "Count: {0:d}";
        private const String STRLOG_Success_arg = "Success: {0:s}";
        /*
         * Database column names
         */
        private const String STRCOL_ExperimentId = "ExperimentId";
        private const String STRCOL_LabServerGuid = "LabServerGuid";
        /*
         * Database stored procedure parameters
         */
        private const String STRPRM_ColumnName = "ColumnName";
        private const String STRPRM_IntValue = "IntValue";
        private const String STRPRM_StrValue = "StrValue";
        /*
         * String constants for SQL processing
         */
        private const String STRSQLCMD_Add = "Experiments_Add";
        private const String STRSQLCMD_Delete = "Experiments_Delete";
        private const String STRSQLCMD_GetNextExperimentId = "Experiments_GetNextExperimentId";
        private const String STRSQLCMD_RetrieveBy = "Experiments_RetrieveBy";
        #endregion

        #region Variables
        private SqlConnection sqlConnection;
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
        public ExperimentsDB(DBConnection dbConnection)
        {
            const string methodName = "ExperimentsDB";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Check that parameters are valid
                 */
                if (dbConnection == null)
                {
                    throw new ArgumentNullException(DBConnection.ClassName);
                }

                /*
                 * Initialise locals
                 */
                this.sqlConnection = dbConnection.GetConnection();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public int Add(ExperimentInfo experimentInfo)
        {
            const string methodName = "Add";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            Int32 experimentId = -1;

            try
            {
                /*
                 * Prepare the stored procedure call
                 */
                SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_Add, this.sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.Add(new SqlParameter(SqlParam(STRCOL_LabServerGuid), experimentInfo.LabServerGuid));

                /*
                 * Execute the stored procedure
                 */
                this.sqlConnection.Open();
                Object obj = sqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    experimentId = (Int32)obj;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }
            finally
            {
                this.sqlConnection.Close();
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExperimentId_arg, experimentId));

            return experimentId;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool Delete(int experimentId)
        {
            const string methodName = "Delete";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExperimentId_arg, experimentId));

            bool success = false;

            try
            {
                /*
                 * Prepare the stored procedure call
                 */
                SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_Delete, this.sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.Add(new SqlParameter(SqlParam(STRCOL_ExperimentId), experimentId));

                /*
                 * Execute the stored procedure
                 */
                this.sqlConnection.Open();
                Object obj = sqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    success = ((Int32)obj == experimentId);
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }
            finally
            {
                this.sqlConnection.Close();
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetNextExperimentId()
        {
            const string methodName = "GetNextExperimentId";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            Int32 experimentId = -1;

            try
            {
                /*
                 * Prepare the stored procedure call
                 */
                SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_GetNextExperimentId, this.sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                /*
                 * Execute the stored procedure
                 */
                this.sqlConnection.Open();
                Object obj = sqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    experimentId = (Int32)obj;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }
            finally
            {
                this.sqlConnection.Close();
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExperimentId_arg, experimentId));

            return experimentId;
        }

        //-------------------------------------------------------------------------------------------------//

        public ArrayList RetrieveAll()
        {
            return this.RetrieveBy(null, 0, null);
        }

        //-------------------------------------------------------------------------------------------------//

        public ExperimentInfo RetrieveByExperimentId(int experimentId)
        {
            ArrayList arrayList = this.RetrieveBy(STRCOL_ExperimentId, experimentId, null);
            return arrayList != null ? (ExperimentInfo)arrayList[0] : null;
        }

        //-------------------------------------------------------------------------------------------------//

        private ArrayList RetrieveBy(String columnName, int intval, String strval)
        {
            const string methodName = "RetrieveBy";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ArrayList arrayList = new ArrayList();

            try
            {
                /*
                 * Prepare the stored procedure call
                 */
                SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveBy, this.sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.Add(new SqlParameter(SqlParam(STRPRM_ColumnName), columnName));
                sqlCommand.Parameters.Add(new SqlParameter(SqlParam(STRPRM_IntValue), intval));
                sqlCommand.Parameters.Add(new SqlParameter(SqlParam(STRPRM_StrValue), strval));

                /*
                 * Execute the stored procedure
                 */
                this.sqlConnection.Open();
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                while (sqlDataReader.Read() == true)
                {
                    Object obj = null;

                    ExperimentInfo experimentInfo = new ExperimentInfo();

                    if ((obj = sqlDataReader[STRCOL_ExperimentId]) != System.DBNull.Value)
                        experimentInfo.ExperimentId = (int)obj;
                    if ((obj = sqlDataReader[STRCOL_LabServerGuid]) != System.DBNull.Value)
                        experimentInfo.LabServerGuid = (String)obj;

                    /*
                     * Add ExperimentInfo to the list
                     */
                    arrayList.Add(experimentInfo);
                }
                sqlDataReader.Close();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }
            finally
            {
                this.sqlConnection.Close();
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Count_arg, arrayList.Count));

            return (arrayList.Count > 0) ? arrayList : null;
        }


        //-------------------------------------------------------------------------------------------------//

        private String SqlParam(String name)
        {
            return name != null ? "@" + name : null;
        }
    }
}
