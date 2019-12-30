using System;
using System.Collections.Generic;
using System.Linq;
using KISS.DBProvider;
using System.IO;

namespace KISS
{
    public class UpdateDB
    {
        public static bool Process(Properties props)
        {
            bool success = true;
            IDBProvider provider;

            switch (props.Provider.ToUpper())
            {
                case "SQLSERVER":
                    provider = new SQLServer(props);
                    break;
                default:
                    throw new Exception("Invalid DB Provider Provided, Excepted Providers are MSQL");
            }
   
            provider.CheckForKissTables();

            //All files that end in .sql
            DirectoryInfo directory =  new DirectoryInfo(props.VersionScriptsFolder);
            FileInfo[] files = directory.GetFiles("*.sql", new EnumerationOptions
            {
                RecurseSubdirectories = true
            });

            List<FileInfo> goodFiles = new List<FileInfo>();
            
            int scriptNo;
            //ignore any file that does not have a number
            foreach (FileInfo file in files)
            {
                if (file.Name.Length > 1 && file.Name.Contains("-") && int.TryParse(file.Name.Substring(0, file.Name.IndexOf("-")), out scriptNo))
                {
                    goodFiles.Add(file);
                }
            }

            //Take all the files that match our naming convention an process any that havent been processed yet
            foreach (FileInfo file in goodFiles)
            {
                if (props.Verbose)
                {
                    Console.WriteLine("Executing the Script File: {0}", file.FullName);
                }
                    
                try
                {
                    provider.ExecuteScriptAndUpdateVersionTable(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error running script {0}", file.FullName);
                    Console.WriteLine(ex.Message);

                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    
                    success = false;
                    break;
                }
            }

            if (goodFiles.Count == 0 && props.Verbose)
            {
                Console.WriteLine("There are no new DataBase scripts to execute");
            }
   
            return success;
        }
    }
}
