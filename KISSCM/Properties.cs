using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KISS
{
    public class Properties
    {
        public bool Verbose { get; set; }
        public bool Wait { get; set; }
        public string Provider { get; set; }
        public string ConnectionString { get; set; }
        public string ScriptTool { get; set; }
        public string ScriptLogin { get; set; }
        public string ScriptSymbol { get; set; }
        public string VersionTable { get; set; }
        public string VersionScriptsFolder { get; set; }
        public string ScriptsFolderFullPath { get; set; }
        public string Encoding { get; set; }
    }
}
