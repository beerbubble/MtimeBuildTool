using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Mtime.Community.Utility;
using Mtime.Community.Utility.Helper;

namespace MtimeClientCompress.Components
{
    public class ClientCompress
    {
        private static string Local_StaticResourceServer =
            System.Configuration.ConfigurationManager.AppSettings["Local_StaticResourceServer"];

        private static string Server_StaticResourceServer =
            System.Configuration.ConfigurationManager.AppSettings["Server_StaticResourceServer"];
        static bool IsGrayImage = SafeConvert.ToBoolean(
                    System.Configuration.ConfigurationManager.AppSettings["IsGrayImage"]);

        public static string Process(string folderPath)
        {
            return Process(folderPath, true);
        }
        public static string Process(string folderPath, bool createVerFolder)
        {
            //2014.04.15 修改根目录文件拷贝异常bug
            folderPath = folderPath.TrimEnd('\\');

            //获取当前站点本身的名字，如 showtime， 用于和主站区分
            string configFilePath = Path.Combine(folderPath, "web.config");
            if (File.Exists(configFilePath))
            {
                XmlDocument configDocument = new XmlDocument();
                configDocument.Load(configFilePath);
                XmlNode appSettingNode = configDocument.SelectSingleNode(@"/configuration/appSettings");
                if (appSettingNode != null)
                {
                    XmlNode node = appSettingNode.SelectSingleNode("add[@key='SubStaticResourceServerName']");
                    if (node != null && node.Attributes["value"] != null)
                    {
                        string subStaticResourceServerName = node.Attributes["value"].Value;
                        Local_StaticResourceServer += subStaticResourceServerName + "/";
                        Server_StaticResourceServer += subStaticResourceServerName + "/";
                    }
                }
            }

            FileInfoCollection files = new FileInfoCollection();
            //打包
            CreatePackFile(folderPath, files);
            //创建版本
            string VERSION = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            FileHelper.SaveFileText(VERSION, folderPath + "\\VERSION.txt");
            //为Build创建一个GB2312版本文件
            FileHelper.SaveFileText(VERSION, folderPath + "\\Build.txt", true);
            //获取所有文件
            GetAllFiles(folderPath, files);
            StringBuilder errorFileStringBuilder = new StringBuilder();
            List<string> handleStaticFiles = new List<string>();
            for (int i = 0, count = files.Count; i < count; i++)
            {
                FileInfo fileInfo = files[i];
                string fileName = fileInfo.FullName;
                string folderName = fileInfo.DirectoryName;
                if (fileName.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase) ||
                    fileName.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase)
                )
                {
                    HandleJsAndCss(fileInfo, fileName, folderName, errorFileStringBuilder, handleStaticFiles);
                }
                //
                if (fileName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase))
                {
                    JpegCompressor.Process(fileName);
                }
                if (fileName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                {
                    PngCompressor.Process(fileName);
                }
            }

            if (createVerFolder)
            {
                //生成静态图片和Css
                //本地测试
                string staticResourceDirectoryPathLocal = folderPath + "\\" + VERSION + "\\local\\" + VERSION + "\\";

                Directory.CreateDirectory(staticResourceDirectoryPathLocal);

                string sourceImagePath = folderPath + "\\images\\";
                string targetImagePath = staticResourceDirectoryPathLocal + "\\images\\";
                CopyDirectory(sourceImagePath, targetImagePath);
                //
                string sourceImagePath08 = folderPath + "\\images08\\";
                string targetImagePath08 = staticResourceDirectoryPathLocal + "\\images08\\";
                CopyDirectory(sourceImagePath08, targetImagePath08);
                //
                string sourceCssPath = folderPath + "\\css\\";
                string targetCssPath = staticResourceDirectoryPathLocal + "\\css\\";
                CopyDirectory(sourceCssPath, targetCssPath);
                //
                string sourceFlashPath = folderPath + "\\flash\\";
                string targetFlashPath = staticResourceDirectoryPathLocal + "\\flash\\";
                CopyDirectory(sourceFlashPath, targetFlashPath);
                //
                //string sourceJsPath = folderPath + "\\js\\";
                //string targetJsPath = staticResourceDirectoryPathLocal + "\\js\\";
                //CopyDirectory ( sourceJsPath, targetJsPath );

                HandleStaticCssFiles(targetCssPath, Local_StaticResourceServer, VERSION);

                //线上
                string staticResourceDirectoryPathServer = folderPath + "\\" + VERSION + "\\server\\" + VERSION + "\\";

                Directory.CreateDirectory(staticResourceDirectoryPathServer);

                string sourceImagePathServer = folderPath + "\\images\\";
                string targetImagePathServer = staticResourceDirectoryPathServer + "\\images\\";
                CopyDirectory(sourceImagePathServer, targetImagePathServer);
                //
                string sourceImagePath08Server = folderPath + "\\images08\\";
                string targetImagePath08Server = staticResourceDirectoryPathServer + "\\images08\\";
                CopyDirectory(sourceImagePath08Server, targetImagePath08Server);
                //
                string sourceCssPathServer = folderPath + "\\css\\";
                string targetCssPathServer = staticResourceDirectoryPathServer + "\\css\\";
                CopyDirectory(sourceCssPathServer, targetCssPathServer);
                //
                string sourceFlashPathServer = folderPath + "\\flash\\";
                string targetFlashPathServer = staticResourceDirectoryPathServer + "\\flash\\";
                CopyDirectory(sourceFlashPathServer, targetFlashPathServer);
                //
                //string sourceJsPathServer = folderPath + "\\js\\";
                //string targetJsPathServer = staticResourceDirectoryPathServer + "\\js\\";
                //CopyDirectory ( sourceJsPathServer, targetJsPathServer );

                HandleStaticCssFiles(targetCssPathServer, Server_StaticResourceServer, VERSION);

                //获取其它资源
                files = new FileInfoCollection();
                GetOtherResourceFiles(folderPath, files);
                foreach (FileInfo fileInfo in files)
                {
                    string fileName = fileInfo.FullName;
                    string folderName = fileInfo.DirectoryName;
                    string name = fileInfo.Name;
                    //本地
                    string localDestFolderName = staticResourceDirectoryPathLocal + folderName.Replace(folderPath, string.Empty) + "\\";
                    string localDescFilename = localDestFolderName + name;
                    if (!File.Exists(localDescFilename))
                    {
                        if (!Directory.Exists(localDestFolderName))
                        {
                            try
                            {
                                Directory.CreateDirectory(localDestFolderName);
                            }
                            catch (Exception e)
                            {
                                string path = localDestFolderName;
                            }
                        }
                        File.Copy(fileName, localDescFilename);
                    }
                    //线上
                    string serverDestFolderName = staticResourceDirectoryPathServer + folderName.Replace(folderPath, string.Empty) + "\\";
                    string serverDescFilename = serverDestFolderName + name;
                    if (!File.Exists(serverDescFilename))
                    {
                        if (!Directory.Exists(serverDestFolderName))
                        {
                            Directory.CreateDirectory(serverDestFolderName);
                        }
                        File.Copy(fileName, serverDescFilename);
                    }

                }
            }

            return errorFileStringBuilder.ToString();
        }

        private static void HandleStaticCssFiles(string targetCssPath, string serverUrl, string VERSION)
        {
            FileInfoCollection files = new FileInfoCollection();
            GetAllFiles(targetCssPath, files);
            for (int i = 0, count = files.Count; i < count; i++)
            {
                FileInfo fileInfo = files[i];
                string fileName = fileInfo.FullName;
                if (fileName.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase))
                {
                    bool gb2312 = false;
                    string fileContent;
                    if (fileName.IndexOf("_gb.") > 0)
                    {
                        //gb2312
                        gb2312 = true;
                        using (StreamReader sr = new StreamReader(fileInfo.FullName, System.Text.Encoding.GetEncoding("gb2312")))
                        {
                            fileContent = sr.ReadToEnd();
                        }

                    }
                    else
                    {
                        using (StreamReader sr = fileInfo.OpenText())
                        {
                            fileContent = sr.ReadToEnd();
                        }
                    }
                    fileContent = ModifyImagePathInCss(fileContent, serverUrl, VERSION);

                    File.Delete(fileName);
                    FileHelper.SaveFileText(fileContent, fileName, gb2312);
                    //
                    string gzipFileName = fileInfo.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(fileName) + ".gz" + Path.GetExtension(fileName).Substring(1);
                    string deflateFileName = fileInfo.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(fileName) + ".deflate.gz" + Path.GetExtension(fileName).Substring(1);
                    FileHelper.SaveZipFileText(fileContent, gzipFileName, gb2312);
                    FileHelper.SaveDeflateCompressFileText(fileContent, deflateFileName, gb2312);
                }
            }
        }

        static string ModifyImagePathInCss(string fileContent, string serverUrl, string VERSION)
        {
            fileContent = fileContent.ToLower();
            fileContent = fileContent.Replace("url(../images/", "url(" + serverUrl + VERSION + "/images/");
            fileContent = fileContent.Replace("\"../images/", "\"" + serverUrl + VERSION + "/images/");
            fileContent = fileContent.Replace("'../images/", "'" + serverUrl + VERSION + "/images/");
            fileContent = fileContent.Replace("url(/images/", "url(" + serverUrl + VERSION + "/images/");
            fileContent = fileContent.Replace("\"/images/", "\"" + serverUrl + VERSION + "/images/");
            fileContent = fileContent.Replace("'/images/", "'" + serverUrl + VERSION + "/images/");
            fileContent = fileContent.Replace("url(/images08/", "url(" + serverUrl + VERSION + "/images08/");
            fileContent = fileContent.Replace("\"/images08/", "\"" + serverUrl + VERSION + "/images08/");
            fileContent = fileContent.Replace("'/images08/", "'" + serverUrl + VERSION + "/images08/");
            fileContent = fileContent.Replace("/css/", "" + serverUrl + VERSION + "/css/");
            return fileContent;
        }



        static void CreatePackFile(string path, FileInfoCollection files)
        {
            List<CombinationFileInfo> packFiles = FileConfig.GetConfig().GetFiles();
            if (packFiles != null && packFiles.Count > 0)
            {
                for (int i = 0, count = packFiles.Count; i < count; i++)
                {
                    CombinationFileInfo packFile = packFiles[i];
                    CreatePackFile(path, packFile.Filename, packFile.IncludeFilenames.ToArray(), files);
                }
            }
        }

        static private void CreatePackFile(string path, string packJsFile, string[] jsFiles, FileInfoCollection files)
        {
            bool jsFileExist = true;
            for (int i = 0, count = jsFiles.Length; i < count; i++)
            {
                string file = path + jsFiles[i];
                if (!File.Exists(file))
                {
                    jsFileExist = false;
                    break;
                }
            }
            if (jsFileExist)
            {
                bool isJs = packJsFile.EndsWith(".js");
                string systemFile = path + packJsFile, systemFileContent = string.Empty;
                for (int i = 0, count = jsFiles.Length; i < count; i++)
                {
                    string file = path + jsFiles[i];
                    systemFileContent += FileHelper.GetFileText(file);
                    if (isJs)
                    {
                        systemFileContent += ";";
                    }
                }
                FileHelper.SaveFileText(systemFileContent, systemFile);
                FileHelper.SaveFileText(systemFileContent, systemFile + "_o");
                files.Add(new FileInfo(systemFile));
            }
        }

        static void HandleJsAndCss(FileInfo fileInfo, string fileName, string folderName, StringBuilder sb, List<string> handleStaticFiles)
        {
            try
            {
                File.SetAttributes(fileName, FileAttributes.Archive);
                string gzipFileName = folderName + "\\" + Path.GetFileNameWithoutExtension(fileName) + ".gz" + Path.GetExtension(fileName).Substring(1);
                string fileContent = string.Empty;
                bool gb2312 = false;
                if (fileName.IndexOf("_gb.") > 0)
                {
                    //gb2312
                    gb2312 = true;
                    using (StreamReader sr = new StreamReader(fileInfo.FullName, System.Text.Encoding.GetEncoding("gb2312")))
                    {
                        fileContent = sr.ReadToEnd();
                    }
                }
                else
                {
                    using (StreamReader sr = fileInfo.OpenText())
                    {
                        fileContent = sr.ReadToEnd();
                    }
                }
                string appPath = System.Windows.Forms.Application.StartupPath;
                //Console.WriteLine ( appPath );
                string compressFilecontent = string.Empty;
                if (!YuiCompressor.Compress(appPath, gb2312, fileInfo.FullName, ref fileContent))
                {
                    sb.Append(fileName);
                    sb.Append(Environment.NewLine);
                }
                FileHelper.SaveZipFileText(fileContent, gzipFileName, gb2312);
                if (fileContent.Length > 0)
                {
                    handleStaticFiles.Add(fileName);
                }
                File.Delete(fileName);
                FileHelper.SaveFileText(fileContent, fileName, gb2312);
            }
            catch (Exception exception)
            {
                Logger.Current.Log("UnknownError.txt", fileName + ":" + exception.Message);
                ExceptionService.Current.Handle(exception);
            }

        }

        #region Helper

        static void GetAllFiles(string path, FileInfoCollection files)
        {
            if (!Directory.Exists(path))
            {
                return;
            }
            DirectoryInfo directory = new DirectoryInfo(path);
            DirectoryInfo[] subDirectorys = directory.GetDirectories("*.*");
            FileInfo[] subFiles = directory.GetFiles();
            for (int i = 0, count = subFiles.Length; i < count; i++)
            {
                if (subFiles[i].FullName.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase) ||
                    subFiles[i].FullName.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase) ||
                    subFiles[i].FullName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
                    subFiles[i].FullName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) ||
                    //subFiles [i].FullName.EndsWith ( ".gif", StringComparison.InvariantCultureIgnoreCase ) ||
                    subFiles[i].FullName.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase))
                {
                    files.Add(subFiles[i]);
                }
            }
            foreach (DirectoryInfo directoryInfo in subDirectorys)
            {
                GetAllFiles(directoryInfo.FullName, files);
            }
        }

        static IEnumerable<string> GetDirectoryDecendants(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                yield return file;
            }
            foreach (var directory in Directory.GetDirectories(path))
            {
                foreach (var file in GetDirectoryDecendants(directory))
                {
                    yield return file;
                }
            }
        }

        static void GetOtherResourceFiles(string path, FileInfoCollection files)
        {
            if (!Directory.Exists(path))
            {
                return;
            }
            DirectoryInfo directory = new DirectoryInfo(path);
            DirectoryInfo[] subDirectorys = directory.GetDirectories("*.*");
            FileInfo[] subFiles = directory.GetFiles();
            for (int i = 0, count = subFiles.Length; i < count; i++)
            {
                if (subFiles[i].FullName.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase) ||
                      subFiles[i].FullName.EndsWith(".gzjs", StringComparison.InvariantCultureIgnoreCase) ||
                      subFiles[i].FullName.EndsWith(".js_o", StringComparison.InvariantCultureIgnoreCase))
                {
                    files.Add(subFiles[i]);
                }
            }
            foreach (DirectoryInfo directoryInfo in subDirectorys)
            {
                GetOtherResourceFiles(directoryInfo.FullName, files);
            }
        }
        public static void CopyDirectory(string Src, string Dst)
        {
            if (!Directory.Exists(Src))
            {
                return;
            }
            String[] Files;
            if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar)
                Dst += Path.DirectorySeparatorChar;
            if (!Directory.Exists(Dst))
                Directory.CreateDirectory(Dst);
            Files = Directory.GetFileSystemEntries(Src);
            foreach (string Element in Files)
            {
                // Sub directories

                if (Directory.Exists(Element))
                    CopyDirectory(Element, Dst + Path.GetFileName(Element));
                // Files in directory

                else
                    File.Copy(Element, Dst + Path.GetFileName(Element), true);
            }
        }


        #endregion

    }
}
