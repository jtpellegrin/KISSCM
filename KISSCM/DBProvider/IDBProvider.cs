using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KISS.DBProvider
{
    public interface IDBProvider
    {
        void ExecuteScriptAndUpdateVersionTable(FileInfo fi);
        string GetVersionTableSchema();
    }
}