using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Mtime.Community.Utility;
using Mtime.Community.Utility.Helper;
using Win.ClientCompress.Components;
using Yahoo.Yui.Compressor;

namespace MtimeClientCompress
{
	public class YuiCompressor
	{
		private static readonly int Timeout =
			SafeConvert.ToInt32( System.Configuration.ConfigurationManager.AppSettings[ "CompressTimeout" ], 5 );
		static JavaScriptCompressor jsCompressor;
		private static CssCompressor cssCompressor;
		//java -jar yuicompressor.jar --charset UTF-8 -o ManageDrafts1.js ManageDrafts1.js
		static YuiCompressor()
		{
			jsCompressor = new JavaScriptCompressor ();
			jsCompressor.Encoding = Encoding.UTF8;
			cssCompressor = new CssCompressor();
		}
		public static bool Compress( string appPath, bool gb2312, string filePath, ref string content )
		{
			if ( content.Length == 0 )
			{
				return true;
			}
			if ( SafeConvert.ToBoolean ( System.Configuration.ConfigurationManager.AppSettings ["ApplyNewCompressor"] ) )
			{
				try
				{
                    if ( filePath.EndsWith( ".css" ) )
                    {
						content = cssCompressor.Compress ( content );
                    }
                    else
                    {
                        //Encoding encoding = gb2312 ? System.Text.Encoding.GetEncoding( "gb2312" ) : System.Text.Encoding.UTF8;
						content = jsCompressor.Compress ( content );
                    }
					return true;
				}
				catch ( Exception e )
				{
					string filename = Path.GetFileNameWithoutExtension(filePath);
					Logger.Current.Log ( filename, filePath + "\r\n" + JsValidater.Process ( filePath ) );
					Logger.Current.Log ( "ErrorCompressorFile.txt", filePath );
					content = string.Empty;
					ExceptionService.Current.Handle ( e );
                    return false;
				}
			}
			else
			{
				string charset = gb2312 ? "gb2312" : "UTF-8";
                using ( Process process = new Process () )
                {
                    process.StartInfo.FileName = "java";
                    process.StartInfo.Arguments = "-jar " + appPath + "\\yuicompressor.jar --charset " + charset + " -o " + filePath + " " + filePath;
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
                            using ( StreamReader sr = new StreamReader ( filePath, ( gb2312 ? System.Text.Encoding.GetEncoding ( "gb2312" ) : System.Text.Encoding.UTF8 ) ) )
                            {
                                content = sr.ReadToEnd ();
                            }
                            return true;
                        }
                        else
                        {
                            content = string.Empty;
                            ExceptionService.Current.Handle ( new Exception ( result ) );
                            return false;
                        }
                    }
                    else
                    {
                        content = FileHelper.GetFileText ( filePath );
                        if ( content.Length != 0 )
                        {
                            ExceptionService.Current.Handle ( new Exception ( "Timeout:" + filePath ) );
                            throw new Exception ( "Timeout:" + filePath );
                        }
                        return false;
                    }
                }

			}

		}
	}
}
