using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Odbc;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DbProviderFactorie
{
    public  class DbProvider : IDisposable
    {

        private string _ConnectionString;
        private DbProviderFactory _myDataBaseProvider;
        private DbConnection _myDataBaseConn;
        private String ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }

        }
        //private string _myConnString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConnectionString"].ToString();
        private DbCommand objSqlCommand = null;
         public DbProvider()
            : this(System.Configuration.ConfigurationManager.ConnectionStrings["Connectionstring"].ToString())
        {
            _myDataBaseProvider = DbProviderFactories.GetFactory("System.Data.SqlClient");
            _myDataBaseConn = _myDataBaseProvider.CreateConnection();
            _myDataBaseConn.ConnectionString = ConnectionString;

            objSqlCommand = _myDataBaseConn.CreateCommand();
            _myDataBaseConn.Open();
        }

        DbProvider(string strConnectionString)
        {
            ConnectionString = strConnectionString;
        }
        //
        // Instead of writing the specific provider in the code, you can set it on the app.config
        public void AddParameter(string Name, object Value)
        {
            
            DbParameter objSqlParameter = objSqlCommand.CreateParameter();
            objSqlParameter.ParameterName = Name;
            objSqlParameter.Value = Value;
            objSqlCommand.Parameters.Add(objSqlParameter);
        }

        public void AddParameter(string Name, object Value, ParameterDirection SqlParameterDirection)
        {

            DbParameter objSqlParameter = objSqlCommand.CreateParameter();
            objSqlParameter.ParameterName = Name;
            objSqlParameter.Value = Value;
            objSqlParameter.Direction = SqlParameterDirection;
            objSqlCommand.Parameters.Add(objSqlParameter);
        }

        public void AddParameter(string Name, object Value, DbType dbType)
        {
            DbParameter objSqlParameter = objSqlCommand.CreateParameter();
            objSqlParameter.ParameterName = Name;
            objSqlParameter.Value = Value;
            objSqlParameter.DbType = dbType;
            objSqlCommand.Parameters.Add(objSqlParameter);
        }

        public void AddParameter(string Name, object Value, DbType dbType, ParameterDirection SqlParameterDirection)
        {

            DbParameter objSqlParameter = objSqlCommand.CreateParameter();
            objSqlParameter.ParameterName = Name;
            objSqlParameter.Value = Value;
            objSqlParameter.DbType = dbType;
            objSqlParameter.Direction = SqlParameterDirection;
            objSqlCommand.Parameters.Add(objSqlParameter);
        }

        public void AddParameter(string Name, object Value, DbType dbType, ParameterDirection SqlParameterDirection, bool IsNull)
        {

            DbParameter objSqlParameter = objSqlCommand.CreateParameter();
            objSqlParameter.ParameterName = Name;
            objSqlParameter.Value = Value;
            objSqlParameter.DbType = dbType;
            objSqlParameter.IsNullable = IsNull;
            objSqlParameter.Direction = SqlParameterDirection;
            objSqlCommand.Parameters.Add(objSqlParameter);
        }

        public int ExecuteNonQuery(string Query, CommandType Commandtyp)
        {
            int i = 0;
            try
            {
                objSqlCommand.CommandText = Query;
                objSqlCommand.CommandType = Commandtyp;
                if (_myDataBaseConn.State == ConnectionState.Closed) _myDataBaseConn.Open();
                i = objSqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (_myDataBaseConn.State == ConnectionState.Open) _myDataBaseConn.Close();
                throw ex;
            }
            finally
            {
                objSqlCommand.Parameters.Clear();
                _myDataBaseConn.Close();
            }

            return i;

        }

        public async Task<int> ExecuteNonQueryAsync(string Query, CommandType Commandtyp)
        {
            int i = 0;
            try
            {
                objSqlCommand.CommandText = Query;
                objSqlCommand.CommandType = Commandtyp;
                if (_myDataBaseConn.State == ConnectionState.Closed) _myDataBaseConn.Open();
                i = await objSqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                if (_myDataBaseConn.State == ConnectionState.Open) _myDataBaseConn.Close();
                throw ex;
            }
            finally
            {
                objSqlCommand.Parameters.Clear();
                _myDataBaseConn.Close();
            }

            return i;

        }

        public void ExecuteNonQuery(string Query, CommandType Commandtyp, ParameterDirection parameterdirection)
        {

            try
            {

                objSqlCommand.CommandText = Query;
                objSqlCommand.CommandType = Commandtyp;
                if (_myDataBaseConn.State == ConnectionState.Closed) _myDataBaseConn.Open();
                objSqlCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                if (_myDataBaseConn.State == ConnectionState.Open) _myDataBaseConn.Close();
                throw ex;
            }
            finally
            {
                objSqlCommand.Parameters.Clear();
                _myDataBaseConn.Close();
            }



        }

        public long ExecuteNonQuery(string Query, CommandType Commandtyp, ParameterDirection parameterdirection, string OutPutParameter)
        {
            long ReturnValue;
            try
            {

                objSqlCommand.CommandText = Query;
                objSqlCommand.CommandType = Commandtyp;
                if (_myDataBaseConn.State == ConnectionState.Closed) _myDataBaseConn.Open();
                int i = objSqlCommand.ExecuteNonQuery();
                ReturnValue = (!string.IsNullOrEmpty(OutPutParameter)) ? Convert.ToInt64(objSqlCommand.Parameters[OutPutParameter].Value) : 0;
            }
            catch (Exception ex)
            {
                if (_myDataBaseConn.State == ConnectionState.Open) _myDataBaseConn.Close();
                throw ex;
            }
            finally
            {
                objSqlCommand.Parameters.Clear();
                _myDataBaseConn.Close();
            }


            return ReturnValue;
        }

        public  DataSet ExecuteDataSet(string Query, CommandType Commandtyp)
        {
            DataSet ds = new DataSet();
            try
            {
                DbDataAdapter adpt = null ;
                objSqlCommand.CommandText = Query;
                objSqlCommand.CommandType = Commandtyp;
                adpt.SelectCommand = objSqlCommand;
                if (_myDataBaseConn.State == ConnectionState.Closed) _myDataBaseConn.Open();
                adpt.Fill(ds);

            }
            catch (Exception ex)
            {
                if (_myDataBaseConn.State == ConnectionState.Open) _myDataBaseConn.Close();
                throw ex;
            }
            finally
            {
                objSqlCommand.Parameters.Clear();
                _myDataBaseConn.Close();
            }

            return ds;

        }
        public DbDataReader ExecuteDataReader(string Query, CommandType Commandtyp)
        {
            DbDataReader rdr = null;
            try
            {
                objSqlCommand.CommandText = Query;
                objSqlCommand.CommandType = Commandtyp;

                if (_myDataBaseConn.State == ConnectionState.Closed) _myDataBaseConn.Open();
                rdr = objSqlCommand.ExecuteReader();


            }
            catch (DbException ex1)
            {
                _myDataBaseConn.Close();
                throw ex1;
            }
            catch (Exception ex)
            {
                _myDataBaseConn.Close();
                throw ex;
            }
            
            finally
            {
                objSqlCommand.Parameters.Clear();
                //objSqlConnection.Close();
            }
            return rdr;
        }

        public async Task<DbDataReader> ExecuteDataReaderAsync(string Query, CommandType Commandtyp)
        {
            DbDataReader rdr = null;
            try
            {
                objSqlCommand.CommandText = Query;
                objSqlCommand.CommandType = Commandtyp;

                if (_myDataBaseConn.State == ConnectionState.Closed) _myDataBaseConn.Open();
                rdr = await objSqlCommand.ExecuteReaderAsync();


            }
            catch (DbException ex1)
            {
                _myDataBaseConn.Close();
                throw ex1;
            }
            catch (Exception ex)
            {
                _myDataBaseConn.Close();
                throw ex;
            }

            finally
            {
                objSqlCommand.Parameters.Clear();
                //objSqlConnection.Close();
            }
            return rdr;
        }

        public object ExecuteScalar(string Query, CommandType Commandtyp)
        {
            object objValue;
            try
            {
                objSqlCommand.CommandText = Query;
                objSqlCommand.CommandType = Commandtyp;
                if (_myDataBaseConn.State == ConnectionState.Closed) _myDataBaseConn.Open();
                objValue = objSqlCommand.ExecuteScalar();

            }
            catch (Exception ex)
            {
                if (_myDataBaseConn.State == ConnectionState.Open) _myDataBaseConn.Close();
                throw ex;
            }
            finally
            {
                objSqlCommand.Parameters.Clear();
                _myDataBaseConn.Close();
            }

            return objValue;

        }

        public void Dispose()
        {
            _myDataBaseConn.Dispose();
            _myDataBaseConn = null;
            objSqlCommand.Dispose();
        }
    }


 
}