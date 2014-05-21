using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using MtimeBuildTool.Helper;
using System.IO;
using MtimeBuildTool.Utility;
using System.Diagnostics;

namespace MtimePackageTool
{
    class Program
    {
        static void Main(string[] args)
        {
            //string project = "MtimeMovieCommunityRoot";
            const string rarPath = @"C:\Progra~1\WinRAR\Rar.exe";

            string project = "MtimeChannel";

            //获取当前部署的项目
            ProjectModel projectModel;

            if (!ProjectMapHelper.ProjectDic.TryGetValue(args[0], out projectModel))
            {
                Environment.Exit(1);
                return;
            }

            if (!string.IsNullOrEmpty(projectModel.LocalSitePackagePath))
            {
                DirectoryHelper.CreateDateFolder(projectModel.LocalSitePackagePath);

                string packagePath = projectModel.LocalSitePackagePath + DateTime.Now.ToString("yyyyMMdd");

                string zipPath = string.Empty;

                if (projectModel.Name == "MtimeMovieCommunityRoot")
                {
                    zipPath = Path.Combine(projectModel.LocalSitePath, VersionHelper.GetVersionVariable(projectModel.Name), "server", VersionHelper.GetVersionVariable(projectModel.Name));
                }
                else
                {
                    zipPath = projectModel.LocalSitePath;
                }

                RAR(new DirectoryInfo(zipPath).Name, packagePath, projectModel.SitePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".rar", new DirectoryInfo(zipPath).Parent.FullName);

                //CmdExecute cmd = new CmdExecute();
                //cmd.ExecuteCommandSyncByWorkingDirectory(rarPath + @" a " + Path.Combine(packagePath, projectModel.SitePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".rar") + " " + new DirectoryInfo(zipPath).Name, @"E:\Package\WebSite\");

                //using (ZipFile zip = new ZipFile())
                //{
                //    zip.AddDirectory(zipPath);

                //    zip.Save(Path.Combine(packagePath, projectModel.SitePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".zip"));

                    
                //}

                DirectoryHelper.DirectoryCopy(packagePath, Path.Combine(@"\\192.168.0.25\ftproot\mtime\upversion\"+projectModel.Name, DateTime.Now.ToString("yyyyMMdd")));
            }

            if (!string.IsNullOrEmpty(projectModel.LocalToolPackagePath))
            {
                DirectoryHelper.CreateDateFolder(projectModel.LocalToolPackagePath);

                string packagePath = projectModel.LocalToolPackagePath + DateTime.Now.ToString("yyyyMMdd");

                string zipPath = string.Empty;

                zipPath = projectModel.LocalToolPath;

                RAR(new DirectoryInfo(zipPath).Name, packagePath, projectModel.ToolPackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".rar", new DirectoryInfo(zipPath).Parent.FullName);

                DirectoryHelper.DirectoryCopy(packagePath, Path.Combine(@"\\192.168.0.25\ftproot\mtime\upversion\" + projectModel.Name, DateTime.Now.ToString("yyyyMMdd")));
            }
        }

        public static bool RAR(string path, string rarPath, string rarName,string workingDirectory)
        {
            bool flag = false;
            string rarexe = @"C:\Progra~1\WinRAR\RAR.exe";;       //WinRAR.exe 的完整路径
            string cmd;          //WinRAR 命令参数
            ProcessStartInfo startinfo;
            Process process;
            try
            {
                cmd = string.Format("a {0} {1}",
                                    Path.Combine(rarPath ,rarName),
                                    path);
                startinfo = new ProcessStartInfo();
                startinfo.FileName = rarexe;
                startinfo.Arguments = cmd;                          //设置命令参数
                startinfo.WindowStyle = ProcessWindowStyle.Normal;  //隐藏 WinRAR 窗口
                startinfo.RedirectStandardOutput = true;
                startinfo.UseShellExecute = false;

                // Do not create the black window.
                startinfo.CreateNoWindow = true;

                startinfo.WorkingDirectory = workingDirectory;
                process = new Process();
                process.StartInfo = startinfo;
                process.Start();
                // Get the output into a string
                string result = process.StandardOutput.ReadToEnd();
                //process.WaitForExit(); //无限期等待进程 winrar.exe 退出

                // Display the command output.
                Console.WriteLine(result);


                if (process.HasExited)
                {
                    flag = true;
                }
                process.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            return flag;
        }
    }
}
