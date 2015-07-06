using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MtimeBuildTool.Helper;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using System.Configuration;
using System.IO;
using MtimeClientCompress.Components;
using MtimeBuildTool.Utility;
using MtimeBuildTool.Const;
using System.Threading;

namespace MtimeBuildTool
{
    class Program
    {

        //private static Dictionary<string, AccountModel> accountDic = new Dictionary<string, AccountModel>();
        //private static Dictionary<string, Dictionary<string, List<RuleItem>>> PublishRuleDic = new Dictionary<string, Dictionary<string, List<RuleItem>>>();
        private const string versionFileName = "VERSION.txt";
        private static readonly string vsersionFolderPath = ConfigurationManager.AppSettings["MtimeVersionFolderPath"];


        static void Main(string[] args)
        {
#if Debug
            if (args.Length < 1)
            {
                Log.WriteMessage("无项目参数，请检查命令！");
                return;
            }
#endif

            //初始化项目配置
            //InitProjectMap();
            //InitMachineAccount();
            //InitMtimePublishRule(); 

            Log.WriteMessage(string.Format("项目数:{0}", ProjectMapHelper.ProjectDic.Count));
            Log.WriteMessage(string.Format("机器账号数:{0}", MachineAccountHelper.AccountDic.Count));

            CmdExecute cmdExecute = new CmdExecute();

            //string project = "MtimeDataService";

            //获取当前部署的项目
            ProjectModel projectModel;

            //if (!ProjectMapHelper.ProjectDic.TryGetValue(project, out projectModel))
            //{
            //    Environment.Exit(1);
            //    return;
            //}

            if (!ProjectMapHelper.ProjectDic.TryGetValue(args[0], out projectModel))
            {
                Environment.Exit(1);
                return;
            }

            //Test 
            //DirectoryHelper.DirectoryFilesRemove(projectModel.StaticPath + @"static\");
            //return;

            Dictionary<string, List<RuleItem>> projectRule = new Dictionary<string, List<RuleItem>>();

            bool includeRule = PublishRuleHelper.PublishRuleDic.TryGetValue(projectModel.Name, out projectRule);

            Log.WriteMessageByProject(projectModel, "Start!");
            Log.WriteMessageByProject(projectModel, "开始删除、压缩、拷贝");
            
            Log.WriteMessageByProject(projectModel, "站点部分开始！");

            #region WebSite
            if (!string.IsNullOrEmpty(projectModel.SiteSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "删除目录开始！");
                DirectoryHelper.DirectoryRemove(projectModel.LocalSitePath, false);
                Log.WriteMessageByProject(projectModel, "删除目录完成！");

                Log.WriteMessageByProject(projectModel, "拷贝目录开始！");
                DirectoryHelper.DirectoryCopy(projectModel.SiteSourcePath, projectModel.LocalSitePath);
                Log.WriteMessageByProject(projectModel, "拷贝目录完成！");
            }
            if (Directory.Exists(projectModel.LocalSitePath + "config"))
                File.Copy(@"C:\MtimeConfig\SiteUrlsServer.config", projectModel.LocalSitePath + @"config\SiteUrlsServer.config", true);

            //临时的数据库修改替换
            ReplaceContent(projectModel.LocalSitePath + "Web.config", @"192.168.1.29\\MTIMESQLSERVER", "192.168.50.104");
            ReplaceContent(projectModel.LocalSitePath + "Web.config", @"192.168.1.29", "192.168.50.104");
            ReplaceContent(projectModel.LocalSitePath + "Web.config", @"mtimecache1213", "mtimeuser0301");
            ReplaceContent(projectModel.LocalSitePath + "Web.config", @"mtimecache", "mtimeuser");
         
            //if (File.Exists(projectModel.LocalSitePath + "Web.config"))
            //{
            //    Encoding encoding = new UTF8Encoding(false);

            //    string sbSource = string.Empty;
            //    string sbOutput = string.Empty;


            //    string filePath = projectModel.LocalSitePath + "web.config";
            //    //设置文件属性
            //    File.SetAttributes(filePath, FileAttributes.Archive);

            //    Console.WriteLine("修改:" + File.GetAttributes(filePath).ToString());

            //    try
            //    {
            //        sbSource = File.ReadAllText(filePath, encoding);

            //    }
            //    catch (Exception)
            //    {
            //        Console.WriteLine("读文件异常");
            //    }

            //    //Dictionary<string, string> replaceDic = new Dictionary<string, string>();


            //    string replaceTarget = @"192.168.1.29\\MTIMESQLSERVER";

            //    string replaceValue = "192.168.50.104";

            //    if (Regex.IsMatch(sbSource, replaceTarget, RegexOptions.IgnoreCase))
            //    {
            //        sbSource = Regex.Replace(sbSource, replaceTarget, replaceValue);
            //    }


            //    try
            //    {
            //        File.WriteAllText(filePath, sbSource, encoding);

            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.Message);

            //    }

            //}

            ReplaceContent(projectModel.LocalSitePath + "config\\Database.config", @"192.168.1.29\\MTIMESQLSERVER", "192.168.50.104");
            ReplaceContent(projectModel.LocalSitePath + "config\\Database.config", @"192.168.1.29", "192.168.50.104");
            ReplaceContent(projectModel.LocalSitePath + "config\\Database.config", @"mtimecache1213", "mtimeuser0301");
            ReplaceContent(projectModel.LocalSitePath + "config\\Database.config", @"mtimecache", "mtimeuser");

            
            var gitBranchName = cmdExecute.ExecuteCommand("git branch", projectModel.SiteSourcePath);
            var gitBranchHash = cmdExecute.ExecuteCommand("git rev-parse HEAD", projectModel.SiteSourcePath);
            File.WriteAllText(projectModel.LocalSitePath + "GitBranchName.txt", gitBranchName);
            File.WriteAllText(projectModel.LocalSitePath + "GitBranchHash.txt", gitBranchHash);

            //if (File.Exists(projectModel.LocalSitePath + "config\\Database.config"))
            //{
            //    Encoding encoding = new UTF8Encoding(false);

            //    string sbSource = string.Empty;
            //    string sbOutput = string.Empty;


            //    string filePath = projectModel.LocalSitePath + "config\\Database.config";
            //    //设置文件属性
            //    File.SetAttributes(filePath, FileAttributes.Archive);

            //    Console.WriteLine("修改:" + File.GetAttributes(filePath).ToString());

            //    try
            //    {
            //        sbSource = File.ReadAllText(filePath, encoding);

            //    }
            //    catch (Exception)
            //    {
            //        Console.WriteLine("读文件异常");
            //    }

            //    //Dictionary<string, string> replaceDic = new Dictionary<string, string>();


            //    string replaceTarget = @"192.168.1.29\\MTIMESQLSERVER";

            //    string replaceValue = "192.168.50.104";

            //    if (Regex.IsMatch(sbSource, replaceTarget, RegexOptions.IgnoreCase))
            //    {
            //        sbSource = Regex.Replace(sbSource, replaceTarget, replaceValue);
            //    }


            //    try
            //    {
            //        File.WriteAllText(filePath, sbSource, encoding);

            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.Message);

            //    }

            //}
           
                

            if (!string.IsNullOrEmpty(projectModel.StaticPath))
            {
                Log.WriteMessageByProject(projectModel, "压缩目录开始！");
                try
                {
                    string errorMessage = ClientCompress.Process(projectModel.LocalSitePath, true);
                    //                    CompressWebSite(projectModel);
                    FileInfo filemain = new FileInfo(projectModel.LocalSitePath + versionFileName);

                    if (!Directory.Exists(vsersionFolderPath + projectModel.Name))
                        Directory.CreateDirectory(vsersionFolderPath + projectModel.Name);
                    filemain.CopyTo(vsersionFolderPath + projectModel.Name + @"\" + versionFileName, true);
                }
                catch (Exception e)
                {
                    Log.WriteMessage(e.Message);
                }
                Log.WriteMessageByProject(projectModel, "压缩目录完成！");

                Log.WriteMessageByProject(projectModel, "拷贝版本号到远程目录开始！");
                CopyToStaticDirectory(projectModel);
                Log.WriteMessageByProject(projectModel, "拷贝版本号到远程目录完成！");
            }

            if (includeRule)
            {
                RuleAction(projectRule, "WebSite");
            }

            if (!string.IsNullOrEmpty(projectModel.RemoteSitePath))
            {
                Log.WriteMessageByProject(projectModel, "拷贝站点到远程目录开始！");
                DirectoryHelper.DirectoryRemove(projectModel.RemoteSitePath, false);
                DirectoryHelper.DirectoryCopy(projectModel.LocalSitePath, projectModel.RemoteSitePath);
                Log.WriteMessageByProject(projectModel, "拷贝站点到远程目录完成！");
            }

            Log.WriteMessageByProject(projectModel, "站点部分完成！");
            #endregion

            if (!string.IsNullOrEmpty(projectModel.ServiceSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "服务部分开始！");

                bool toDelete = true;

                while (toDelete)
                {
                    try
                    {
                        DirectoryHelper.DirectoryRemove(projectModel.LocalServicePath);
                        toDelete = false;
                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(3000);
                    }
                }

                DirectoryHelper.DirectoryCopy(projectModel.ServiceSourcePath, projectModel.LocalServicePath);

                File.Copy(@"C:\MtimeConfig\SiteUrlsServer.config", projectModel.LocalServicePath + @"config\SiteUrlsServer.config", true);

                //临时添加服务版本号
                try
                {
                    string errorMessage = ClientCompress.Process(projectModel.LocalServicePath, false);
                }
                catch (Exception e)
                {
                    Log.WriteMessage(e.Message);
                }

                //临时的数据库修改替换

                ReplaceContent(projectModel.LocalServicePath + "Mtime.Data.SocketService.exe.config", @"192.168.1.29\\MTIMESQLSERVER", "192.168.50.104");
                ReplaceContent(projectModel.LocalServicePath + "Mtime.Data.SocketService.exe.config", @"192.168.1.29", "192.168.50.104");
                ReplaceContent(projectModel.LocalServicePath + "Mtime.Data.SocketService.exe.config", @"mtimecache1213", "mtimeuser0301");
                ReplaceContent(projectModel.LocalServicePath + "Mtime.Data.SocketService.exe.config", @"mtimecache", "mtimeuser");

                var gitBranchNameService = cmdExecute.ExecuteCommand("git branch", projectModel.ServiceSourcePath);
                var gitBranchHashService = cmdExecute.ExecuteCommand("git rev-parse HEAD", projectModel.ServiceSourcePath);
                File.WriteAllText(projectModel.LocalServicePath + "GitBranchName.txt", gitBranchNameService);
                File.WriteAllText(projectModel.LocalServicePath + "GitBranchHash.txt", gitBranchHashService);

                //if (File.Exists(projectModel.LocalServicePath + "Mtime.Data.SocketService.exe.config"))
                //{
                //    Encoding encoding = new UTF8Encoding(false);

                //    string sbSource = string.Empty;
                //    string sbOutput = string.Empty;


                //    string filePath = projectModel.LocalServicePath + "Mtime.Data.SocketService.exe.config";
                //    //设置文件属性
                //    File.SetAttributes(filePath, FileAttributes.Archive);

                //    Console.WriteLine("修改:" + File.GetAttributes(filePath).ToString());

                //    try
                //    {
                //        sbSource = File.ReadAllText(filePath, encoding);

                //    }
                //    catch (Exception)
                //    {
                //        Console.WriteLine("读文件异常");
                //    }

                //    //Dictionary<string, string> replaceDic = new Dictionary<string, string>();


                //    string replaceTarget = @"192.168.1.29\\MTIMESQLSERVER";

                //    string replaceValue = "192.168.50.104";

                //    if (Regex.IsMatch(sbSource, replaceTarget, RegexOptions.IgnoreCase))
                //    {
                //        sbSource = Regex.Replace(sbSource, replaceTarget, replaceValue);
                //    }


                //    try
                //    {
                //        File.WriteAllText(filePath, sbSource, encoding);

                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e.Message);

                //    }

                //}
                
                if (includeRule)
                {
                    RuleAction(projectRule, "Service");
                }
                ServiceAction(projectModel, ActionType.Stop);

                bool toDeleteRemote = true;
                while (toDeleteRemote)
                {
                    try
                    {
                        Log.WriteMessageByProject(projectModel, "删除服务远程目录开始！");
                        DirectoryHelper.DirectoryRemove(projectModel.RemoteServicePath);
                        Log.WriteMessageByProject(projectModel, "删除服务远程目录结束！");
                        toDeleteRemote = false;
                    }
                    catch (Exception e)
                    {
                        Log.WriteMessageByProject(projectModel, "删除服务远程目录异常！");
                        Thread.Sleep(3000);
                    }
                }

                DirectoryHelper.DirectoryCopy(projectModel.LocalServicePath, projectModel.RemoteServicePath);
                ServiceAction(projectModel, ActionType.Start);
                Log.WriteMessageByProject(projectModel, "服务部分完成！");
            }
            if (!string.IsNullOrEmpty(projectModel.ToolSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "工具部分开始！");
                DirectoryHelper.DirectoryRemove(projectModel.LocalToolPath);
                DirectoryHelper.DirectoryCopy(projectModel.ToolSourcePath, projectModel.LocalToolPath);
                if (!projectModel.Name.ToLower().Contains("kiosk") && !projectModel.Name.ToLower().Contains("fake"))
                    File.Copy(@"C:\MtimeConfig\SiteUrlsServer.config", projectModel.LocalToolPath + @"config\SiteUrlsServer.config", true);

                foreach (string f in Directory.GetFiles(projectModel.LocalToolPath, "*.exe.config"))
                {
                    Log.WriteMessageByProject(projectModel, f);
                    Log.WriteMessageByProject(projectModel, "替换开始");
                    ReplaceContent(f, @"192.168.1.29\\MTIMESQLSERVER", "192.168.50.104");
                    ReplaceContent(f, @"192.168.1.29", "192.168.50.104");
                    ReplaceContent(f, @"mtimecache1213", "mtimeuser0301");
                    ReplaceContent(f, @"mtimecache", "mtimeuser");
                    Log.WriteMessageByProject(projectModel, "替换结束");
                }

                //临时的数据库修改替换
                ReplaceContent(projectModel.LocalToolPath + "config\\Database.config", @"192.168.1.29\\MTIMESQLSERVER", "192.168.50.104");
                ReplaceContent(projectModel.LocalToolPath + "config\\Database.config", @"192.168.1.29", "192.168.50.104");
                ReplaceContent(projectModel.LocalToolPath + "config\\Database.config", @"mtimecache1213", "mtimeuser0301");
                ReplaceContent(projectModel.LocalToolPath + "config\\Database.config", @"mtimecache", "mtimeuser");

                var gitBranchNameTool = cmdExecute.ExecuteCommand("git branch", projectModel.ToolSourcePath);
                var gitBranchHashTool = cmdExecute.ExecuteCommand("git rev-parse HEAD", projectModel.ToolSourcePath);
                File.WriteAllText(projectModel.LocalToolPath + "GitBranchName.txt", gitBranchNameTool);
                File.WriteAllText(projectModel.LocalToolPath + "GitBranchHash.txt", gitBranchHashTool);

                if (includeRule)
                {
                    RuleAction(projectRule, "Tool");
                }
                ToolAction(projectModel, ActionType.Stop);
                DirectoryHelper.DirectoryRemove(projectModel.RemoteToolPath);
                DirectoryHelper.DirectoryCopy(projectModel.LocalToolPath, projectModel.RemoteToolPath);
                if (projectModel.ForceStart)
                    ToolAction(projectModel, ActionType.Start);
                Log.WriteMessageByProject(projectModel, "工具部分完成！");
            }

        }

        private static void RuleAction(Dictionary<string, List<RuleItem>> projectRule, string type)
        {
            List<RuleItem> webRuleList = new List<RuleItem>();
            if (projectRule.TryGetValue(type, out webRuleList))
            {
                foreach (var rule in webRuleList)
                {
                    if (!string.IsNullOrEmpty(rule.Value) && rule.Value.StartsWith("$"))
                    {
                        rule.Value = VersionHelper.GetVersionVariable(rule.Value.Substring(1));
                    }
                    if (!string.IsNullOrEmpty(rule.Replace) && rule.Replace.StartsWith("$"))
                    {
                        rule.Replace = VersionHelper.GetVersionVariable(rule.Replace.Substring(1));
                    }
                    switch (rule.Type)
                    {
                        case RuleType.ReplaceContent:
                            FileHelper.ReplaceContent(rule);
                            break;
                        case RuleType.EditConfig:
                            FileHelper.EditConfig(rule);
                            break;
                        case RuleType.CopyFile:
                            File.Copy(rule.SourceFileName, rule.DestFileName, true);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static void ServiceAction(ProjectModel projectModel, ActionType actionType)
        {
            //sc stop ETicketServer
            //sc \\192.168.50.25 stop DETContractServer

            const string stopTemplate = @"sc \\{0} stop {1}";
            const string startTemplate = @"sc \\{0} start {1}";

            string command = string.Empty;

            switch (actionType)
            {
                case ActionType.Start:
                    command = string.Format(startTemplate, projectModel.ServiceRemoteIp, projectModel.ServiceName);
                    break;
                case ActionType.Stop:
                    command = string.Format(stopTemplate, projectModel.ServiceRemoteIp, projectModel.ServiceName);
                    break;
                default:
                    break;
            }

            CmdExecute cmd = new CmdExecute();
            cmd.ExecuteCommandSync(command);
        }

        private static void ToolAction(ProjectModel projectModel, ActionType actionType)
        {
            AccountModel account;
            if (!MachineAccountHelper.AccountDic.TryGetValue(projectModel.ToolRemoteIp, out account))
            {
                Log.WriteMessageByProject(projectModel, "无法获取服务器的账号，请核对账号配置文件！");
                return;
            }


            RemoteExecute remoteExecute = new RemoteExecute(projectModel.ToolRemoteIp, account.UserName, account.Password);

            switch (actionType)
            {
                case ActionType.Start:
                    string commmandPre = @"c:\Windows\System32\cmd.exe /C start /b /d """ + projectModel.RemoteToolPathForLocal + "\" ";
                    string cmdTemplate = "{0} -autostart";

                    if (projectModel.AutoStart)
                    {
                        remoteExecute.StartProcess(commmandPre + string.Format(cmdTemplate, projectModel.ProcessName));
                    }
                    else
                    {
                        string commmand = commmandPre + projectModel.ProcessName;
                        Console.WriteLine(commmand);
                        remoteExecute.StartProcess(commmand);
                    }
                    break;
                case ActionType.Stop:
                    var theDic = remoteExecute.GetProcessList();

                    List<ProcessModel> processList;
                    theDic.TryGetValue(projectModel.ProcessName, out processList);

                    if (processList != null)
                    {
                        foreach (var item in processList)
                        {
                            if (item.ExecutablePath == projectModel.RemoteToolPathForLocal + projectModel.ProcessName)
                            {
                                remoteExecute.KillProcess(item.ManagementObj);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private static void CompressWebSite(ProjectModel projectModel)
        {
            Log.WriteMessageByProject(projectModel, "Compress Start!");
            string cmdTemplate = "{0} {1}";

            string command = string.Format(cmdTemplate, ConfigurationManager.AppSettings["MtimeCompressToolPath"], projectModel.LocalSitePath);
            Log.WriteMessageByProject(projectModel, command);
            CmdExecute cmd = new CmdExecute();
            cmd.ExecuteCommandSync(command);
            Log.WriteMessageByProject(projectModel, "Compress Finish!");
        }

        private static void CopyToStaticDirectory(ProjectModel projectModel)
        {
            //2014-2-10 小东要求特殊static目录下是local版本，所以将主站版本号提前拷贝
            if (projectModel.Name == "MtimeMovieCommunityRoot")
            {
                string version = File.ReadAllText(projectModel.LocalSitePath + versionFileName);
                DirectoryHelper.DirectoryRemove(projectModel.StaticPath + @"static\");
                DirectoryHelper.DirectoryCopy(projectModel.LocalSitePath + version + @"\local\" + version, projectModel.StaticPath + @"static\");
            }


            if (!string.IsNullOrEmpty(projectModel.StaticPath))
            {
                string subVersion = System.IO.File.ReadAllText(projectModel.LocalSitePath + versionFileName);
                DirectoryHelper.DirectoryCopy(projectModel.LocalSitePath + subVersion + @"\local", projectModel.StaticPath);
                DirectoryHelper.DirectoryRemove(projectModel.LocalSitePath + subVersion + @"\local", true);
            }
        }


        private static void ReplaceContent(string filePath, string replaceTarget, string replaceValue)
        {
            if (File.Exists(filePath))
            {
                Encoding encoding = new UTF8Encoding(false);

                string sbSource = string.Empty;
                string sbOutput = string.Empty;


                //string filePath = projectModel.LocalSitePath + "web.config";
                //设置文件属性
                File.SetAttributes(filePath, FileAttributes.Archive);

                Console.WriteLine("修改:" + File.GetAttributes(filePath).ToString());

                try
                {
                    sbSource = File.ReadAllText(filePath, encoding);

                }
                catch (Exception)
                {
                    Console.WriteLine("读文件异常");
                }

                //Dictionary<string, string> replaceDic = new Dictionary<string, string>();


                //string replaceTarget = @"192.168.1.29\\MTIMESQLSERVER";

                //string replaceValue = "192.168.50.104";

                if (Regex.IsMatch(sbSource, replaceTarget, RegexOptions.IgnoreCase))
                {
                    sbSource = Regex.Replace(sbSource, replaceTarget, replaceValue);
                }


                try
                {
                    File.WriteAllText(filePath, sbSource, encoding);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                }

            }
        }
    }
}
