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
                Log.WriteMessage("����Ŀ�������������");
                return;
            }
#endif

            //��ʼ����Ŀ����
            //InitProjectMap();
            //InitMachineAccount();
            //InitMtimePublishRule(); 

            Log.WriteMessage(string.Format("��Ŀ��:{0}", ProjectMapHelper.ProjectDic.Count));
            Log.WriteMessage(string.Format("�����˺���:{0}", MachineAccountHelper.AccountDic.Count));

            //string project = "MtimeChannel";

            //��ȡ��ǰ�������Ŀ
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
            Log.WriteMessageByProject(projectModel, "��ʼɾ����ѹ��������");
            //ɾ����Ŀ��ر���Ŀ¼
            if (!string.IsNullOrEmpty(projectModel.SiteSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "վ�㲿�ֿ�ʼ��");

                Log.WriteMessageByProject(projectModel, "ɾ��Ŀ¼��ʼ��");
                DirectoryHelper.DirectoryRemove(projectModel.LocalSitePath, false);
                Log.WriteMessageByProject(projectModel, "ɾ��Ŀ¼��ɣ�");

                Log.WriteMessageByProject(projectModel, "����Ŀ¼��ʼ��");
                DirectoryHelper.DirectoryCopy(projectModel.SiteSourcePath, projectModel.LocalSitePath);
                File.Copy(@"C:\MtimeConfig\SiteUrlsServer.config", projectModel.LocalSitePath + @"config\SiteUrlsServer.config", true);
                Log.WriteMessageByProject(projectModel, "����Ŀ¼��ɣ�");

                if (includeRule)
                {
                    RuleAction(projectRule, "WebSite");
                }

                if (!string.IsNullOrEmpty(projectModel.StaticPath))
                {
                    Log.WriteMessageByProject(projectModel, "ѹ��Ŀ¼��ʼ��");
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
                    Log.WriteMessageByProject(projectModel, "ѹ��Ŀ¼��ɣ�");

                    Log.WriteMessageByProject(projectModel, "�����汾�ŵ�Զ��Ŀ¼��ʼ��");
                    CopyToStaticDirectory(projectModel);
                    Log.WriteMessageByProject(projectModel, "�����汾�ŵ�Զ��Ŀ¼��ɣ�");
                }

                if (!string.IsNullOrEmpty(projectModel.RemoteSitePath))
                {
                    Log.WriteMessageByProject(projectModel, "����վ�㵽Զ��Ŀ¼��ʼ��");
                    DirectoryHelper.DirectoryRemove(projectModel.RemoteSitePath, false);
                    DirectoryHelper.DirectoryCopy(projectModel.LocalSitePath, projectModel.RemoteSitePath);
                    Log.WriteMessageByProject(projectModel, "����վ�㵽Զ��Ŀ¼��ɣ�");
                }

                Log.WriteMessageByProject(projectModel, "վ�㲿����ɣ�");
            }
            if (!string.IsNullOrEmpty(projectModel.ServiceSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "���񲿷ֿ�ʼ��");
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
                Log.WriteMessageByProject(projectModel, "���񲿷���ɣ�");
            }
            if (!string.IsNullOrEmpty(projectModel.ToolSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "���߲��ֿ�ʼ��");
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
                Log.WriteMessageByProject(projectModel, "���߲�����ɣ�");
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
                Log.WriteMessageByProject(projectModel, "�޷���ȡ���������˺ţ���˶��˺������ļ���");
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
            //2014-2-10 С��Ҫ������staticĿ¼����local�汾�����Խ���վ�汾����ǰ����
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
