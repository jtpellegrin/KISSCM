using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KISS.DBProvider
{
    public interface IDBProvider
    {
        bool BeginTransaction();
        bool HasAlreadyRan(string fileName);
        bool ExecuteScriptAndUpdateVersionTable(string scriptText, string fileName);
        bool CommitTransaction();
        bool RollbackTransaction();
        bool UpdateMigrationLog(string sql, string fileName);
        bool CheckForKissTables();
        bool CreateTableSchema();
    }
}