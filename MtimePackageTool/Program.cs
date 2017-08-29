using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using MtimeBuildTool.Helper;
using System.IO;
using MtimeBuildTool.Utility;
using System.Diagnostics;
using System.Net;

namespace MtimePackageTool
{
    class Program
    {
        static void Main(string[] args)
        {
            //string project = "MtimeMovieCommunityRoot";
            //const string rarPath = @"C:\Progra~1\WinRAR\Rar.exe";

            string project = args[0];
            //string project = "MtimeDataService2";

            //获取当前部署的项目
            ProjectModel projectModel;

            //if (!ProjectMapHelper.ProjectDic.TryGetValue(args[0], out projectModel))
            if (!ProjectMapHelper.ProjectDic.TryGetValue(project, out projectModel))
            {
                Environment.Exit(1);
                return;
            }

            Log.WriteMessageByProject(projectModel, project);

            if (!string.IsNullOrEmpty(projectModel.LocalSitePackagePath))
            {
                DirectoryHelper.CreateDateFolder(projectModel.LocalSitePackagePath);

                string packagePath = projectModel.LocalSitePackagePath + DateTime.Now.ToString("yyyyMMdd");

                string zipPath = string.Empty;

                string packagefile = projectModel.SitePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".rar";

                if (projectModel.SitePacker)
                {
                    if (string.IsNullOrEmpty(projectModel.Setting))
                    {
                        SitePacker(projectModel.LocalSitePath, packagePath);
                    }
                    else
                    {
                        SitePacker(projectModel.LocalSitePath, packagePath, projectModel.Setting);
                    }
                }
                else
                {

                    if (projectModel.Name == "MtimeMovieCommunityRoot" || projectModel.Name == "MtimeMovieCommunityRootPackage")
                    {
                        zipPath = Path.Combine(projectModel.LocalSitePath, VersionHelper.GetVersionVariable(projectModel.Name));
                        RAR(packagePath, VersionHelper.GetVersionVariable(projectModel.Name) + ".rar", new DirectoryInfo(zipPath).FullName);
                    }
                    else if (projectModel.Name == "js.front_web")
                    {
                        zipPath = projectModel.LocalSitePath;
                        using (ZipFile zip = new ZipFile())
                        {
                            zip.AddDirectory(zipPath);

                            zip.Save(Path.Combine(packagePath, projectModel.SitePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".zip"));
                        }
                    }
                    else
                    {
                        zipPath = projectModel.LocalSitePath;
                        //RAR(packagePath, projectModel.SitePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".rar", new DirectoryInfo(zipPath).FullName);
                        RAR(packagePath, packagefile, new DirectoryInfo(zipPath).FullName);
                    }
                }


                //CmdExecute cmd = new CmdExecute();
                //cmd.ExecuteCommandSyncByWorkingDirectory(rarPath + @" a " + Path.Combine(packagePath, projectModel.SitePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".rar") + " " + new DirectoryInfo(zipPath).Name, @"E:\Package\WebSite\");

                //using (ZipFile zip = new ZipFile())
                //{
                //    zip.AddDirectory(zipPath);

                //    zip.Save(Path.Combine(packagePath, projectModel.SitePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".zip"));


                //}

                //DirectoryHelper.DirectoryCopy(packagePath, Path.Combine(@"\\192.168.0.25\ftproot\mtime\upversion\" + projectModel.Name, DateTime.Now.ToString("yyyyMMdd")));
                //DirectoryHelper.DirectoryCopy(packagePath, Path.Combine(@"\\192.168.50.22\e$\Publish\" + projectModel.Name, DateTime.Now.ToString("yyyyMMdd")));


                FtpCreateFolder("10.10.20.15/codestore/upversion/" + project + @"/" + DateTime.Now.ToString("yyyyMMdd"), "codeuser", "codeuser");

                string[] fileEntries = Directory.GetFiles(packagePath);
                foreach (string filePath in fileEntries)
                {
                    Console.WriteLine(filePath);

                    string fileName = Path.GetFileName(filePath);

                    using (System.Net.WebClient client = new System.Net.WebClient())
                    {
                        client.Credentials = new System.Net.NetworkCredential("codeuser", "codeuser");
                        try
                        {
                            client.UploadFile("ftp://10.10.20.15/codestore/upversion/" + project + @"/" + DateTime.Now.ToString("yyyyMMdd") + @"/" + fileName, "STOR", filePath);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Log.WriteMessage(e.Message);
                        }
                    }

                    //// Get the object used to communicate with the server.
                    //FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://10.10.20.15/codestore/upversion/" + project + @"/" + DateTime.Now.ToString("yyyyMMdd") + @"/" + fileName);
                    //request.Method = WebRequestMethods.Ftp.UploadFile;

                    //// This example assumes the FTP site uses anonymous logon.
                    //request.Credentials = new NetworkCredential("codeuser", "codeuser");

                    //// Copy the contents of the file to the request stream.
                    ////StreamReader sourceStream = new StreamReader(packagePath + @"\" + packagefile);
                    //StreamReader sourceStream = new StreamReader(filePath);
                    //byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                    //sourceStream.Close();
                    //request.ContentLength = fileContents.Length;

                    //Stream requestStream = request.GetRequestStream();
                    //requestStream.Write(fileContents, 0, fileContents.Length);
                    //requestStream.Close();

                    //FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                    //Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

                    //response.Close();
                }

            }

            if (!string.IsNullOrEmpty(projectModel.LocalServicePackagePath))
            {
                DirectoryHelper.CreateDateFolder(projectModel.LocalServicePackagePath);

                string packagePath = projectModel.LocalServicePackagePath + DateTime.Now.ToString("yyyyMMdd");

                string zipPath = string.Empty;

                zipPath = projectModel.LocalServicePath;

                string packagefile = projectModel.ServicePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".rar";

                //RAR(packagePath, projectModel.ServicePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".rar", new DirectoryInfo(zipPath).FullName);
                RAR(packagePath, packagefile, new DirectoryInfo(zipPath).FullName);

                //DirectoryHelper.DirectoryCopy(packagePath, Path.Combine(@"\\192.168.0.25\ftproot\mtime\upversion\" + projectModel.Name, DateTime.Now.ToString("yyyyMMdd")));
                //DirectoryHelper.DirectoryCopy(packagePath, Path.Combine(@"\\192.168.50.22\e$\Publish\" + projectModel.Name, DateTime.Now.ToString("yyyyMMdd")));

                FtpCreateFolder("10.10.20.15/codestore/upversion/" + project + @"/" + DateTime.Now.ToString("yyyyMMdd"), "codeuser", "codeuser");

                string[] fileEntries = Directory.GetFiles(packagePath);
                foreach (string filePath in fileEntries)
                {
                    Console.WriteLine(filePath);

                    string fileName = Path.GetFileName(filePath);

                    using (System.Net.WebClient client = new System.Net.WebClient())
                    {
                        client.Credentials = new System.Net.NetworkCredential("codeuser", "codeuser");
                        client.UploadFile("ftp://10.10.20.15/codestore/upversion/" + project + @"/" + DateTime.Now.ToString("yyyyMMdd") + @"/" + fileName, "STOR", filePath);
                    }

                    //// Get the object used to communicate with the server.
                    //FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://10.10.20.15/codestore/upversion/" + project + @"/" + DateTime.Now.ToString("yyyyMMdd") + @"/" + fileName);
                    //request.Method = WebRequestMethods.Ftp.UploadFile;

                    //// This example assumes the FTP site uses anonymous logon.
                    //request.Credentials = new NetworkCredential("codeuser", "codeuser");

                    //// Copy the contents of the file to the request stream.
                    ////StreamReader sourceStream = new StreamReader(packagePath + @"\" + packagefile);
                    //StreamReader sourceStream = new StreamReader(filePath);
                    //byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                    //sourceStream.Close();
                    //request.ContentLength = fileContents.Length;

                    //Stream requestStream = request.GetRequestStream();
                    //requestStream.Write(fileContents, 0, fileContents.Length);
                    //requestStream.Close();

                    //FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                    //Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

                    //response.Close();
                }
            }

            if (!string.IsNullOrEmpty(projectModel.LocalToolPackagePath))
            {
                DirectoryHelper.CreateDateFolder(projectModel.LocalToolPackagePath);
                string packagePath = projectModel.LocalToolPackagePath + DateTime.Now.ToString("yyyyMMdd");

                string packagefile = projectModel.ToolPackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".rar";

                if (File.Exists(projectModel.LocalToolPath + "/Package.config"))
                {
                    Package(projectModel.LocalToolPath, packagePath);
                }
                else
                {
                    string zipPath = string.Empty;

                    zipPath = projectModel.LocalToolPath;

                    //RAR(packagePath, projectModel.ToolPackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".rar", new DirectoryInfo(zipPath).FullName);

                    RAR(packagePath, packagefile, new DirectoryInfo(zipPath).FullName);
                }

                //DirectoryHelper.DirectoryCopy(packagePath, Path.Combine(@"\\192.168.0.25\ftproot\mtime\upversion\" + projectModel.Name, DateTime.Now.ToString("yyyyMMdd")));
                //DirectoryHelper.DirectoryCopy(packagePath, Path.Combine(@"\\192.168.50.22\e$\Publish\" + projectModel.Name, DateTime.Now.ToString("yyyyMMdd")));

                FtpCreateFolder("10.10.20.15/codestore/upversion/" + project + @"/" + DateTime.Now.ToString("yyyyMMdd"), "codeuser", "codeuser");

                string[] fileEntries = Directory.GetFiles(packagePath);
                foreach (string filePath in fileEntries)
                {
                    Console.WriteLine(filePath);

                    string fileName = Path.GetFileName(filePath);

                    using (System.Net.WebClient client = new System.Net.WebClient())
                    {
                        client.Credentials = new System.Net.NetworkCredential("codeuser", "codeuser");
                        client.UploadFile("ftp://10.10.20.15/codestore/upversion/" + project + @"/" + DateTime.Now.ToString("yyyyMMdd") + @"/" + fileName, "STOR", filePath);
                    }

                    //// Get the object used to communicate with the server.
                    //FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://10.10.20.15/codestore/upversion/" + project + @"/" + DateTime.Now.ToString("yyyyMMdd") + @"/" + fileName);
                    //request.Method = WebRequestMethods.Ftp.UploadFile;

                    //// This example assumes the FTP site uses anonymous logon.
                    //request.Credentials = new NetworkCredential("codeuser", "codeuser");

                    //// Copy the contents of the file to the request stream.
                    ////StreamReader sourceStream = new StreamReader(packagePath + @"\" + packagefile);
                    //StreamReader sourceStream = new StreamReader(filePath);
                    //byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                    //sourceStream.Close();
                    //request.ContentLength = fileContents.Length;

                    //Stream requestStream = request.GetRequestStream();
                    //requestStream.Write(fileContents, 0, fileContents.Length);
                    //requestStream.Close();

                    //FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                    //Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

                    //response.Close();
                }
            }
        }

        private static void FtpCreateFolder(string ftpAddress, string ftpUName, string ftpPWord)
        {
            try
            {
                WebRequest ftpRequest = WebRequest.Create("ftp://" + ftpAddress);
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                ftpRequest.Credentials = new NetworkCredential(ftpUName, ftpPWord);

                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                response.Close();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    response.Close();
                }
                else
                {
                    response.Close();
                }
            }
           
        }

        public static bool RAR(string rarPath, string rarName, string workingDirectory)
        {
            bool flag = false;
            string rarexe = @"C:\Progra~1\WinRAR\RAR.exe"; ;       //WinRAR.exe 的完整路径
            string cmd;          //WinRAR 命令参数
            ProcessStartInfo startinfo;
            Process process;
            try
            {
                cmd = string.Format("a -r {0}",
                                    Path.Combine(rarPath, rarName));
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

        public static bool Package(string sourcePath, string destPath)
        {
            bool flag = false;
            string packageexe = @"E:\CCNet\Codev3.0\Tools\AppPacker\bin\Release\AppPacker.exe";
            ProcessStartInfo startinfo;
            Process process;
            string cmd;
            try
            {
                cmd = string.Format("{0} {1}", sourcePath, destPath);
                startinfo = new ProcessStartInfo();
                startinfo.FileName = packageexe;
                startinfo.Arguments = cmd;                          //设置命令参数
                startinfo.WindowStyle = ProcessWindowStyle.Normal;  //隐藏 WinRAR 窗口
                startinfo.RedirectStandardOutput = true;
                startinfo.UseShellExecute = false;

                // Do not create the black window.
                startinfo.CreateNoWindow = true;

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

        public static bool SitePacker(string sourcePath, string destPath)
        {
            return SitePacker(sourcePath, destPath, string.Empty);
        }

        public static bool SitePacker(string sourcePath, string destPath, string setting)
        {
            bool flag = false;
            string packageexe = @"E:\CCNet\Codev3.0\Tools\SitePacker\SitePackerConsole\bin\Release\SitePacker.exe";
            ProcessStartInfo startinfo;
            Process process;
            string cmd;
            try
            {
                if (string.IsNullOrEmpty(setting))
                {
                    cmd = string.Format("-dir:{0} -output:{1}", sourcePath, destPath);
                }
                else
                {
                    cmd = string.Format("-dir:{0} -output:{1} {2}", sourcePath, destPath, setting);
                }
                startinfo = new ProcessStartInfo();
                startinfo.FileName = packageexe;
                startinfo.Arguments = cmd;                          //设置命令参数
                startinfo.WindowStyle = ProcessWindowStyle.Normal;  //隐藏 WinRAR 窗口
                startinfo.RedirectStandardOutput = true;
                startinfo.UseShellExecute = false;

                // Do not create the black window.
                startinfo.CreateNoWindow = true;

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
