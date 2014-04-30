using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;


namespace OFunnelEmailScheduler.OFunnelDbLogic
{
    /// <summary>
    /// SqlHelper class to execute the SQL statement/stored procedure
    /// </summary>
    public sealed class SqlHelper
    {
        //*********************************************************************
        // Since this class provides only static methods, make the default constructor private to prevent 
        // instances from being created with "new SqlHelper()".
        //*********************************************************************
        private SqlHelper() 
        { 

        }

        /// <summary>
        /// This method is used to attach array of SqlParameters to a SqlCommand.
        /// This method will assign a value of DbNull to any parameter with a direction of InputOutput and a value of null. 
        /// This behavior will prevent default values from being used, but
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
        /// where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">an array of SqlParameters tho be added to command</param>
        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            foreach (SqlParameter p in commandParameters)
            {
                //check for derived output value with no value assigned
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }

        /// <summary>
        /// This method assigns an array of values to an array of SqlParameters.
        /// </summary>
        /// <param name="commandParameters">array of SqlParameters to be assigned values</param>
        /// <param name="parameterValues">array of objects holding the values to be assigned</param>
        private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                //do nothing if we get no data
                return;
            }
            // we must have the same number of values as we pave parameters to put them in
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }
            //iterate through the SqlParameters, assigning the values from the corresponding position in the 
            //value array
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                commandParameters[i].Value = parameterValues[i];
            }
        }

        /// <summary>
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
        ///  to the provided command.
        /// </summary>
        /// <param name="command">the SqlCommand to be prepared</param>
        /// <param name="connection">a valid SqlConnection, on which to execute this command</param>
        /// <param name="transaction">a valid SqlTransaction, or 'null'</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters)
        {
            //if the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            //associate the connection with the command
            command.Connection = connection;
            //set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;
            //if we were provided a transaction, assign it.
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            //set the command type
            command.CommandType = commandType;
            //attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }

        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            //create & open a SqlConnection, and dispose of it after we are done.
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();
                //call the overload that takes a connection in place of the connection string
                return ExecuteDataset(cn, commandType, commandText, commandParameters);
            }
        }

        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            //if we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                //assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);
                //call the overload that takes an array of SqlParameters
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
            else
            {
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);

            ////create the DataAdapter & DataSet
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            ////fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds);

            // detach the SqlParameters from the command object, so they can be used again.   
            cmd.Parameters.Clear();

            //return the dataset
            return ds;
        }
    }


    /// <summary>
    /// SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters, 
    /// and the ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class SqlHelperParameterCache
    {
        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Since this class provides only static methods, make the default constructor private to prevent instances from being created with "new SqlHelperParameterCache()".
        /// </summary>
        private SqlHelperParameterCache() { }

        /// <summary>
        /// Resolve at run time the appropriate set of SqlParameters for a stored procedure
        /// </summary>
        /// <param name="connectionString">a valid connection string for a SqlConnection </param>
        /// <param name="spName">the name of the stored procedure </param>
        /// <param name="includeReturnValueParameter">whether or not to include their return value parameter </param>
        /// <returns>returns a SqlParameter</returns>
        private static SqlParameter[] DiscoverSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(spName, cn))
            {
                cn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(cmd);
                if (!includeReturnValueParameter)
                {
                    cmd.Parameters.RemoveAt(0);
                }
                SqlParameter[] discoveredParameters = new SqlParameter[cmd.Parameters.Count]; ;
                cmd.Parameters.CopyTo(discoveredParameters, 0);
                return discoveredParameters;
            }
        }

        private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
        {
            //deep copy of cached SqlParameter array
            SqlParameter[] clonedParameters = new SqlParameter[originalParameters.Length];
            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
            }
            return clonedParameters;
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </summary>
        /// <param name="connectionString">a valid connection string for a SqlConnection </param>
        /// <param name="spName">the name of the stored procedure </param>
        /// <returns>returns an array of SqlParameters</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </summary>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">a bool value indicating whether the return value parameter should be included in the results.</param>
        /// <returns>returns an array of SqlParameters</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            string hashKey = connectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");
            SqlParameter[] cachedParameters;

            cachedParameters = (SqlParameter[])paramCache[hashKey];
            if (cachedParameters == null)
            {
                cachedParameters = (SqlParameter[])(paramCache[hashKey] = DiscoverSpParameterSet(connectionString, spName, includeReturnValueParameter));
            }

            return CloneParameters(cachedParameters);
        }
    }
}
