using System;
using System.IO;
using System.Data.SqlClient;

namespace KISS.DBProvider
{
    public class SQLServer:IDBProvider
    {
        Properties _props;
        public SQLServer(Properties props)
        {
            _props = props;
        }

        public void ExecuteScriptAndUpdateVersionTable(FileInfo fi)
        {
            bool success = true;
            Exception exc = null;
            string scriptText = null;

            if (string.IsNullOrWhiteSpace(_props.Encoding))
            {
                scriptText = File.ReadAllText(fi.FullName);
            }
            else
            {
                scriptText = File.ReadAllText(fi.FullName, ArgumentHelper.GetEncoding(_props.Encoding));
            }
                
            using (SqlConnection sqlConnection = new SqlConnection(_props.ConnectionString))
            {
                Microsoft.SqlServer.Management.Common.ServerConnection svrConnection = new Microsoft.SqlServer.Management.Common.ServerConnection(sqlConnection);
                Microsoft.SqlServer.Management.Smo.Server server = new Microsoft.SqlServer.Management.Smo.Server(svrConnection);

                string fileName = fi.FullName.Substring(fi.FullName.IndexOf(_props.VersionScriptsFolder, StringComparison.Ordinal));

                bool hasRows = false;
                using (SqlDataReader reader = server.ConnectionContext.ExecuteReader($"SELECT * FROM {_props.VersionTable} WHERE FileName = '{fileName}'"))
                {
                    hasRows = reader.HasRows;
                }

                if (!hasRows)
                {
                    server.ConnectionContext.BeginTransaction();
                    try
                    {
                        server.ConnectionContext.ExecuteNonQuery(scriptText);
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        exc = ex;
                        server.ConnectionContext.RollBackTransaction();
                    }

                    if (success)
                    {
                        server.ConnectionContext.CommitTransaction();
                        UpdateVersion(fileName, scriptText);
                    }
                }
            }

            if (!success)
            {
                throw exc;
            }
        }

        private void UpdateVersion(string fileName, string sql)
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

        public string GetVersionTableSchema()
        {
            return $"CREATE TABLE {_props.VersionTable}(" +
                   $"Id INT NOT NULL IDENTITY," +
                   "CreatedDate DATETIME NOT NULL," +
                   "FileName NVARCHAR(4000)," +
                   "SQL VARCHAR(MAX))";
        }
    }
}
