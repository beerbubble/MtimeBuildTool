using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Win.ClientCompress.Components
{
	class JsValidater
	{
		//H:\Backup>jsl -process postblog.js -nologo
		private static readonly int Timeout = 60;

		public static string Process( string filepath )
		{
			using ( Process process = new Process () )
			{
				process.EnableRaisingEvents = false;
				process.StartInfo.FileName = "jsl.exe";
				process.StartInfo.Arguments = string.Format ( "-process {0} -nologo", filepath );
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.CreateNoWindow = true;
				process.Start ();
				if ( process.WaitForExit ( Timeout * 1000 ) )
				{
					string result = process.StandardOutput.ReadToEnd ();
					return result;
				}
			}
			return string.Empty;
		}
	}
}
