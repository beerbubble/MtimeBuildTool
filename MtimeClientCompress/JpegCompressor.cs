using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Yahoo.Yui.Compressor;

namespace MtimeClientCompress
{
    public class JpegCompressor
    {
        private static readonly int Timeout = 60;

        public static bool Process( string filename )
        {
            File.SetAttributes ( filename, FileAttributes.Archive );
            File.Copy ( filename, filename + ".old" );
            //
            bool success = false;
            //
            using( Process process = new Process () )
            {
                process.EnableRaisingEvents = false;
                process.StartInfo.FileName = "jpegtran.exe";
                process.StartInfo.Arguments = string.Format ( " -optimize \"{0}\" \"{1}\"", filename, filename );
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start ();
                if ( process.WaitForExit ( Timeout * 1000 ) )
                {
                    string result = process.StandardOutput.ReadToEnd ();
                    if ( result.Length == 0 )
                    {
                        success = true;
                    }
                }
            }
            return success;
        }
    }
}
