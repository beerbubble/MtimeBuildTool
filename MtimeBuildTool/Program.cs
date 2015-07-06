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

            CmdExecute cmdExecute = new CmdExecute();

            //string project = "MtimeDataService";

            //��ȡ��ǰ�������Ŀ
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
            Log.WriteMessageByProject(projectModel, "��ʼɾ����ѹ��������");
            
            Log.WriteMessageByProject(projectModel, "վ�㲿�ֿ�ʼ��");

            #region WebSite
            if (!string.IsNullOrEmpty(projectModel.SiteSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "ɾ��Ŀ¼��ʼ��");
                DirectoryHelper.DirectoryRemove(projectModel.LocalSitePath, false);
                Log.WriteMessageByProject(projectModel, "ɾ��Ŀ¼��ɣ�");

                Log.WriteMessageByProject(projectModel, "����Ŀ¼��ʼ��");
                DirectoryHelper.DirectoryCopy(projectModel.SiteSourcePath, projectModel.LocalSitePath);
                Log.WriteMessageByProject(projectModel, "����Ŀ¼��ɣ�");
            }
            if (Directory.Exists(projectModel.LocalSitePath + "config"))
                File.Copy(@"C:\MtimeConfig\SiteUrlsServer.config", projectModel.LocalSitePath + @"config\SiteUrlsServer.config", true);

            //��ʱ�����ݿ��޸��滻
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
            //    //�����ļ�����
            //    File.SetAttributes(filePath, FileAttributes.Archive);

            //    Console.WriteLine("�޸�:" + File.GetAttributes(filePath).ToString());

            //    try
            //    {
            //        sbSource = File.ReadAllText(filePath, encoding);

            //    }
            //    catch (Exception)
            //    {
            //        Console.WriteLine("���ļ��쳣");
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
            //    //�����ļ�����
            //    File.SetAttributes(filePath, FileAttributes.Archive);

            //    Console.WriteLine("�޸�:" + File.GetAttributes(filePath).ToString());

            //    try
            //    {
            //        sbSource = File.ReadAllText(filePath, encoding);

            //    }
            //    catch (Exception)
            //    {
            //        Console.WriteLine("���ļ��쳣");
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
                Log.WriteMessageByProject(projectModel, "ѹ��Ŀ¼��ʼ��");
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
                Log.WriteMessageByProject(projectModel, "ѹ��Ŀ¼��ɣ�");

                Log.WriteMessageByProject(projectModel, "�����汾�ŵ�Զ��Ŀ¼��ʼ��");
                CopyToStaticDirectory(projectModel);
                Log.WriteMessageByProject(projectModel, "�����汾�ŵ�Զ��Ŀ¼��ɣ�");
            }

            if (includeRule)
            {
                RuleAction(projectRule, "WebSite");
            }

            if (!string.IsNullOrEmpty(projectModel.RemoteSitePath))
            {
                Log.WriteMessageByProject(projectModel, "����վ�㵽Զ��Ŀ¼��ʼ��");
                DirectoryHelper.DirectoryRemove(projectModel.RemoteSitePath, false);
                DirectoryHelper.DirectoryCopy(projectModel.LocalSitePath, projectModel.RemoteSitePath);
                Log.WriteMessageByProject(projectModel, "����վ�㵽Զ��Ŀ¼��ɣ�");
            }

            Log.WriteMessageByProject(projectModel, "վ�㲿����ɣ�");
            #endregion

            if (!string.IsNullOrEmpty(projectModel.ServiceSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "���񲿷ֿ�ʼ��");

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

                //��ʱ��ӷ���汾��
                try
                {
                    string errorMessage = ClientCompress.Process(projectModel.LocalServicePath, false);
                }
                catch (Exception e)
                {
                    Log.WriteMessage(e.Message);
                }

                //��ʱ�����ݿ��޸��滻

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
                //    //�����ļ�����
                //    File.SetAttributes(filePath, FileAttributes.Archive);

                //    Console.WriteLine("�޸�:" + File.GetAttributes(filePath).ToString());

                //    try
                //    {
                //        sbSource = File.ReadAllText(filePath, encoding);

                //    }
                //    catch (Exception)
                //    {
                //        Console.WriteLine("���ļ��쳣");
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
                        Log.WriteMessageByProject(projectModel, "ɾ������Զ��Ŀ¼��ʼ��");
                        DirectoryHelper.DirectoryRemove(projectModel.RemoteServicePath);
                        Log.WriteMessageByProject(projectModel, "ɾ������Զ��Ŀ¼������");
                        toDeleteRemote = false;
                    }
                    catch (Exception e)
                    {
                        Log.WriteMessageByProject(projectModel, "ɾ������Զ��Ŀ¼�쳣��");
                        Thread.Sleep(3000);
                    }
                }

                DirectoryHelper.DirectoryCopy(projectModel.LocalServicePath, projectModel.RemoteServicePath);
                ServiceAction(projectModel, ActionType.Start);
                Log.WriteMessageByProject(projectModel, "���񲿷���ɣ�");
            }
            if (!string.IsNullOrEmpty(projectModel.ToolSourcePath))
            {
                Log.WriteMessageByProject(projectModel, "���߲��ֿ�ʼ��");
                DirectoryHelper.DirectoryRemove(projectModel.LocalToolPath);
                DirectoryHelper.DirectoryCopy(projectModel.ToolSourcePath, projectModel.LocalToolPath);
                if (!projectModel.Name.ToLower().Contains("kiosk") && !projectModel.Name.ToLower().Contains("fake"))
                    File.Copy(@"C:\MtimeConfig\SiteUrlsServer.config", projectModel.LocalToolPath + @"config\SiteUrlsServer.config", true);

                foreach (string f in Directory.GetFiles(projectModel.LocalToolPath, "*.exe.config"))
                {
                    Log.WriteMessageByProject(projectModel, f);
                    Log.WriteMessageByProject(projectModel, "�滻��ʼ");
                    ReplaceContent(f, @"192.168.1.29\\MTIMESQLSERVER", "192.168.50.104");
                    ReplaceContent(f, @"192.168.1.29", "192.168.50.104");
                    ReplaceContent(f, @"mtimecache1213", "mtimeuser0301");
                    ReplaceContent(f, @"mtimecache", "mtimeuser");
                    Log.WriteMessageByProject(projectModel, "�滻����");
                }

                //��ʱ�����ݿ��޸��滻
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


        private static void ReplaceContent(string filePath, string replaceTarget, string replaceValue)
        {
            if (File.Exists(filePath))
            {
                Encoding encoding = new UTF8Encoding(false);

                string sbSource = string.Empty;
                string sbOutput = string.Empty;


                //string filePath = projectModel.LocalSitePath + "web.config";
                //�����ļ�����
                File.SetAttributes(filePath, FileAttributes.Archive);

                Console.WriteLine("�޸�:" + File.GetAttributes(filePath).ToString());

                try
                {
                    sbSource = File.ReadAllText(filePath, encoding);

                }
                catch (Exception)
                {
                    Console.WriteLine("���ļ��쳣");
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
