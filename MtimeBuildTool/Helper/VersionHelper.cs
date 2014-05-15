using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace MtimeBuildTool.Helper
{
    public class VersionHelper
    {
        private static readonly string vsersionFolderPath = ConfigurationManager.AppSettings["MtimeVersionFolderPath"];
        private const string versionFileName = "VERSION.txt";

        public static string GetVersionVariable(string projectName)
        {
            string result = string.Empty;
            try
            {
                result = ReadVersionTxt(vsersionFolderPath + projectName + @"\" + versionFileName);
            }
            catch (Exception e)
            {

            }
            return result;

        }

        private static string ReadVersionTxt(string path)
        {
            return File.ReadAllText(path);
        }

    }
}
