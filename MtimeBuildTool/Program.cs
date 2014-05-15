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

            //string project = "MtimeChannel";

            //获取当前部署的项目
            ProjectModel projectModel;

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
            //删除项目相关本地目录
            if (!string.IsNullOrEmpty(projectModel.SiteSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "站点部分开始！");

                Log.WriteMessageByProject(projectModel, "删除目录开始！");
                DirectoryHelper.DirectoryRemove(projectModel.LocalSitePath, false);
                Log.WriteMessageByProject(projectModel, "删除目录完成！");

                Log.WriteMessageByProject(projectModel, "拷贝目录开始！");
                DirectoryHelper.DirectoryCopy(projectModel.SiteSourcePath, projectModel.LocalSitePath);
                File.Copy(@"C:\MtimeConfig\SiteUrlsServer.config", projectModel.LocalSitePath + @"config\SiteUrlsServer.config", true);
                Log.WriteMessageByProject(projectModel, "拷贝目录完成！");

                if (includeRule)
                {
                    RuleAction(projectRule, "WebSite");
                }

                if (!string.IsNullOrEmpty(projectModel.StaticPath))
                {
                    Log.WriteMessageByProject(projectModel, "压缩目录开始！");
                    try
                    {
                        string errorMessage = ClientCompress.Process(projectModel.LocalSitePath, true);
                        //                    CompressWebSite(projectModel);
                        FileInfo filemain = new FileInfo(projectModel.LocalSitePath + versionFileName);
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

                if (!string.IsNullOrEmpty(projectModel.RemoteSitePath))
                {
                    Log.WriteMessageByProject(projectModel, "拷贝站点到远程目录开始！");
                    DirectoryHelper.DirectoryRemove(projectModel.RemoteSitePath, false);
                    DirectoryHelper.DirectoryCopy(projectModel.LocalSitePath, projectModel.RemoteSitePath);
                    Log.WriteMessageByProject(projectModel, "拷贝站点到远程目录完成！");
                }

                Log.WriteMessageByProject(projectModel, "站点部分完成！");
            }
            if (!string.IsNullOrEmpty(projectModel.ServiceSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "服务部分开始！");
                DirectoryHelper.DirectoryRemove(projectModel.LocalServicePath);
                DirectoryHelper.DirectoryCopy(projectModel.ServiceSourcePath, projectModel.LocalServicePath);
                File.Copy(@"C:\MtimeConfig\SiteUrlsServer.config", projectModel.LocalServicePath + @"config\SiteUrlsServer.config", true);
                if (includeRule)
                {
                    RuleAction(projectRule, "Service");
                }
                ServiceAction(projectModel, ActionType.Stop);
                DirectoryHelper.DirectoryRemove(projectModel.RemoteServicePath);
                DirectoryHelper.DirectoryCopy(projectModel.LocalServicePath, projectModel.RemoteServicePath);
                ServiceAction(projectModel, ActionType.Start);
                Log.WriteMessageByProject(projectModel, "服务部分完成！");
            }
            if (!string.IsNullOrEmpty(projectModel.ToolSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "工具部分开始！");
                DirectoryHelper.DirectoryRemove(projectModel.LocalToolPath);
                DirectoryHelper.DirectoryCopy(projectModel.ToolSourcePath, projectModel.LocalToolPath);
                File.Copy(@"C:\MtimeConfig\SiteUrlsServer.config", projectModel.LocalToolPath + @"config\SiteUrlsServer.config", true);
                if (includeRule)
                {
                    RuleAction(projectRule, "Tool");
                }
                ToolAction(projectModel, ActionType.Stop);
                DirectoryHelper.DirectoryRemove(projectModel.RemoteToolPath);
                DirectoryHelper.DirectoryCopy(projectModel.LocalToolPath, projectModel.RemoteToolPath);
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
                    switch (rule.Type)
                    {
                        case RuleType.ReplaceContent:
                            FileHelper.ReplaceContent(rule);
                            break;
                        case RuleType.EditConfig:
                            FileHelper.EditConfig(rule);
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
                    command = string.Format(startTemplate, projectModel.ServiceRemoteIp, projectModel.Name);
                    break;
                case ActionType.Stop:
                    command = string.Format(stopTemplate, projectModel.ServiceRemoteIp, projectModel.Name);
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
    }
}
