using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace KISS
{
    public class ArgumentHelper
    {
        /// <summary>
        /// Process all scripts that have not been successfully run.
        /// </summary>
        /// <param name="kissPropsFile">kissprops.xml configuration file location plus filename</param>
        /// <returns></returns>
        public static bool ProcessScripts(string kissPropsFile)
        {
            Properties properties = new Properties();
            XmlSerializer s = new XmlSerializer(properties.GetType());

            using (TextReader r = new StreamReader(kissPropsFile))
            {
                properties = (Properties) s.Deserialize(r);

                //We just want to process files recursively at the base level, in the included kissprops.xml file we use the "Scripts" folder
                if (!properties.VersionScriptsFolder.Contains(":") 
                    || !properties.VersionScriptsFolder.Contains(@"\\"))
                {
                    var settingPath = Path.GetDirectoryName(kissPropsFile);
                    properties.ScriptsFolderFullPath = Path.Combine(settingPath, properties.VersionScriptsFolder);
                }
            }

            bool success =  UpdateDB.Process(properties);
            if (properties.Wait)
            {
                Console.Read();
            }
            
            return success;
        }

        public static Encoding GetEncoding(string encodingName)
        {
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(encodingName);
            }
            catch (System.ArgumentException)
            {
                encoding = Encoding.Default;
            }
            return encoding;
        }
    }
}