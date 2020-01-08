using System;
using System.IO;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace KISS.DBProvider
{
    public class SQLServer : IDBProvider
    {
        private Properties _props;
        private Server _server;
        private ServerConnection _svrConnection;

        public SQLServer(Properties props)
        {
            _props = props;

            _svrConnection = new ServerConnection(new SqlConnection(_props.ConnectionString));
            _server = new Server(_svrConnection);
        }

        public bool BeginTransaction()
        {
            bool success = true;

            try
            {
                _server.ConnectionContext.BeginTransaction();
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex);
            }

            return success;
        }

        public bool HasAlreadyRan(string fileName)
        {
            bool hasRows = false;

            using (SqlDataReader reader = _server.ConnectionContext.ExecuteReader($"SELECT * FROM {_props.VersionTable} WHERE FileName = '{fileName}'"))
            {
                hasRows = reader.HasRows;
            }

            return hasRows;
        }

        public bool ExecuteScriptAndUpdateVersionTable(string scriptText, string fileName)
        {
            bool success = true;

            try
            {
                _server.ConnectionContext.ExecuteNonQuery(scriptText);
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex.ToString());
            }

            return success;
        }

        public bool CommitTransaction()
        {
            bool success = true;

            try
            {
                _server.ConnectionContext.CommitTransaction();
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex.ToString());
            }
            
            return success;
        }

        public bool RollbackTransaction()
        {
            bool success = true;

            try
            {
                _server.ConnectionContext.RollBackTransaction();
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex);
            }

            return success;
        }

        public bool UpdateMigrationLog(string sql, string fileName)
        {
            bool success = true;

            try
            {
                using (SqlConnection conn = new SqlConnection(_props.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {_props.VersionTable} (CreatedDate,FileName,SQL) VALUES(GETUTCDATE(),@fileName,@sql)");
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@fileName", fileName));
                    cmd.Parameters.Add(new SqlParameter("@sql", sql));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex);
            }
            
            return success;
        }

        public bool CheckForKissTables()
        {
            bool success = true;

            try
            {
                using (SqlDataReader reader = _server.ConnectionContext.ExecuteReader($"SELECT count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_props.VersionTable}'"))
                {
                    success = reader.HasRows;
                }
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex.ToString());
            }

            return success;
        }

        public bool CreateTableSchema()
        {
            bool success = true;

            try
            {
                string sql = $"CREATE TABLE {_props.VersionTable}(" +
                             $"Id INT NOT NULL IDENTITY," +
                             "CreatedDate DATETIME NOT NULL," +
                             "FileName NVARCHAR(4000)," +
                             "SQL VARCHAR(MAX))";

                using (SqlConnection conn = new SqlConnection(_props.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql);
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex);
            }

            return success;
        }
    }
}
