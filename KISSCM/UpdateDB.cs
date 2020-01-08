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
            Console.WriteLine("Enter Process KissProps.VersionScriptsFolder: {0}", props.VersionScriptsFolder);
            Console.WriteLine("Enter Process KissProps.ScriptsFolderFullPath: {0}", props.ScriptsFolderFullPath);

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

            if (!provider.CheckForKissTables())
            {
                provider.CreateTableSchema();
            }

            //All files that end in .sql
            DirectoryInfo directory =  new DirectoryInfo(props.ScriptsFolderFullPath);
            FileInfo[] files = directory.GetFiles("*.sql", new EnumerationOptions
            {
                RecurseSubdirectories = true
            });

            Console.WriteLine("Begin Transaction");
            success = provider.BeginTransaction();

            //Take all the files that match our naming convention an process any that havent been processed yet
            foreach (FileInfo file in files)
            {
                try
                {
                    string fileName = file.FullName.Substring(file.FullName.IndexOf(props.VersionScriptsFolder, StringComparison.CurrentCultureIgnoreCase));
                    string scriptText = string.Empty;

                    if (!provider.HasAlreadyRan(fileName))
                    {
                        Console.WriteLine("Executing the Script File: {0}", fileName);

                        if (string.IsNullOrWhiteSpace(props.Encoding))
                        {
                            scriptText = File.ReadAllText(file.FullName);
                        }
                        else
                        {
                            scriptText = File.ReadAllText(file.FullName, ArgumentHelper.GetEncoding(props.Encoding));
                        }

                        if (success)
                        {
                            success = provider.ExecuteScriptAndUpdateVersionTable(scriptText, fileName);

                            if (success)
                            {
                                success = provider.UpdateMigrationLog(scriptText, fileName);
                            }
                        }

                        if (!success)
                        {
                            break;
                        }
                    }
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

            if (success)
            {
                Console.WriteLine("Committing transaction");
                provider.CommitTransaction();
            }
            else
            {
                Console.WriteLine("Rollback transaction");
                provider.RollbackTransaction();
            }

            return success;
        }
    }
}
