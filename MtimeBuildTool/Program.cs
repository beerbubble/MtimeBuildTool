﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MtimeBuildTool.Helper;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using System.Configuration;

namespace MtimeBuildTool
{
    class Program
    {
        private static Dictionary<string, ProjectModel> projectDic = new Dictionary<string, ProjectModel>();
        private static Dictionary<string, AccountModel> accountDic = new Dictionary<string, AccountModel>();

        static void Main(string[] args)
        {
            //RemoteExecute remoteExecute = new RemoteExecute("192.168.50.25", "administrator", "1");

            //string path = @"C:\Inetpub\MtimeService\Staticize\MtimeMessageProcessor.exe";

            //remoteExecute.Connect();

            //var theDic= remoteExecute.GetProcessList();

            //List<ProcessModel> modelList = theDic["MtimeMessageProcessor.exe"];

            //foreach (var item in modelList)
            //{
            //    if (item.ExecutablePath == path)
            //    {
            //        remoteExecute.KillProcess(item.ManagementObj);
            //    }

            //}

            //remoteExecute.StartProcess(@"C:\Inetpub\MtimeService\Staticize\MtimeMessageProcessor.exe -autostart");


            //if (args.Length < 1)
            //{
            //    Log.WriteMessage("项目参数为空，调用错误，请查看具体调用方式！");
            //    return;
            //}

            //初始化项目配置
            InitProjectMap();
            InitMachineAccount();

            Log.WriteMessage(string.Format("项目数:{0}", projectDic.Count));
            Log.WriteMessage(string.Format("机器账号数:{0}", accountDic.Count));

            string project = "DETContractServer";

            ////获取当前部署的项目
            ProjectModel projectModel;
            if (!projectDic.TryGetValue(project, out projectModel))
            {
                Environment.Exit(1);
                return;
            }

            //删除项目相关本地目录
            if (!string.IsNullOrEmpty(projectModel.SiteSourcePath))
            {
                DirectoryHelper.DirectoryRemove(projectModel.LocalSitePath);
                DirectoryHelper.DirectoryCopy(projectModel.SiteSourcePath, projectModel.LocalSitePath, true);
                CompressWebSite(projectModel);
                CopyToStaticDirectory(projectModel);
                DirectoryHelper.DirectoryCopy(projectModel.LocalSitePath, projectModel.RemoteSitePath, true);
            }
            if (!string.IsNullOrEmpty(projectModel.ServiceSourcePath))
            {
                DirectoryHelper.DirectoryRemove(projectModel.LocalServicePath);
                DirectoryHelper.DirectoryCopy(projectModel.ServiceSourcePath, projectModel.LocalServicePath, true);
                ServiceAction(projectModel, Action.Stop);
                DirectoryHelper.DirectoryCopy(projectModel.LocalServicePath, projectModel.RemoteServicePath, true);
                ServiceAction(projectModel, Action.Start);
            }
            if (!string.IsNullOrEmpty(projectModel.ToolSourcePath))
            {
                DirectoryHelper.DirectoryRemove(projectModel.LocalToolPath);
                DirectoryHelper.DirectoryCopy(projectModel.ToolSourcePath, projectModel.LocalToolPath, true);
            }

            ServiceAction(projectModel, Action.Stop);
        }

        private static void InitProjectMap()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/config/MtimeProjectMap.xml");

            XmlNodeList websiteList = doc.SelectNodes("/MtimeProject/WebSites/WebSite");

            foreach (XmlNode node in websiteList)
            {
                ProjectModel projectMapModel = new ProjectModel();
                projectMapModel.Name = node.Attributes["Name"].Value;
                projectMapModel.LocalSitePath = node.Attributes["LocalSitePath"].Value;
                if (node.Attributes["RemoteSitePath"] != null)
                {
                    projectMapModel.RemoteSitePath = node.Attributes["RemoteSitePath"].Value;
                    projectMapModel.SiteRemoteIp = RegexForIp(projectMapModel.RemoteSitePath);
                }
                if (node.Attributes["StaticPath"] != null)
                    projectMapModel.StaticPath = node.Attributes["StaticPath"].Value;
                if (node.Attributes["SiteSourcePath"] != null)
                    projectMapModel.SiteSourcePath = node.Attributes["SiteSourcePath"].Value;



                projectDic.Add(projectMapModel.Name, projectMapModel);
            }

            XmlNodeList serviceList = doc.SelectNodes("/MtimeProject/Services/Service");

            foreach (XmlNode node in serviceList)
            {
                ProjectModel projectMapModel = new ProjectModel();
                if (projectDic.TryGetValue(node.Attributes["Name"].Value, out projectMapModel))
                {
                    projectMapModel.LocalServicePath = node.Attributes["LocalServicePath"].Value;
                    projectMapModel.ServiceSourcePath = node.Attributes["ServiceSourcePath"].Value;
                    if (node.Attributes["RemoteServicePath"] != null)
                    {
                        projectMapModel.RemoteServicePath = node.Attributes["RemoteServicePath"].Value;
                        projectMapModel.ServiceRemoteIp = RegexForIp(projectMapModel.RemoteServicePath);
                    }
                }
                else
                {
                    ProjectModel newProjectMapModel = new ProjectModel();
                    newProjectMapModel.Name = node.Attributes["Name"].Value;
                    newProjectMapModel.LocalServicePath = node.Attributes["LocalServicePath"].Value;
                    newProjectMapModel.ServiceSourcePath = node.Attributes["ServiceSourcePath"].Value;
                    if (node.Attributes["RemoteServicePath"] != null)
                    {
                        newProjectMapModel.RemoteServicePath = node.Attributes["RemoteServicePath"].Value;
                        newProjectMapModel.ServiceRemoteIp = RegexForIp(newProjectMapModel.RemoteServicePath);
                    }

                    projectDic.Add(newProjectMapModel.Name, newProjectMapModel);
                }
            }

            XmlNodeList toolList = doc.SelectNodes("/MtimeProject/Tools/Tool");

            foreach (XmlNode node in toolList)
            {
                ProjectModel projectMapModel = new ProjectModel();
                if (projectDic.TryGetValue(node.Attributes["Name"].Value, out projectMapModel))
                {
                    projectMapModel.LocalToolPath = node.Attributes["LocalToolPath"].Value;
                    projectMapModel.ToolSourcePath = node.Attributes["ToolSourcePath"].Value;
                    if (node.Attributes["RemoteToolPath"] != null)
                    {
                        projectMapModel.RemoteToolPath = node.Attributes["RemoteToolPath"].Value;
                        projectMapModel.ToolRemoteIp = RegexForIp(projectMapModel.RemoteToolPath);
                    }

                    projectMapModel.RemoteToolPathForLocal = node.Attributes["RemoteToolPathForLocal"].Value;
                }
                else
                {
                    ProjectModel newProjectMapModel = new ProjectModel();
                    newProjectMapModel.Name = node.Attributes["Name"].Value;
                    newProjectMapModel.LocalToolPath = node.Attributes["LocalToolPath"].Value;
                    newProjectMapModel.ToolSourcePath = node.Attributes["ToolSourcePath"].Value;
                    if (node.Attributes["RemoteToolPath"] != null)
                    {
                        newProjectMapModel.RemoteToolPath = node.Attributes["RemoteToolPath"].Value;
                        newProjectMapModel.ToolRemoteIp = RegexForIp(newProjectMapModel.RemoteToolPath);
                    }
                    newProjectMapModel.RemoteToolPathForLocal = node.Attributes["RemoteToolPathForLocal"].Value;

                    projectDic.Add(newProjectMapModel.Name, newProjectMapModel);
                }
            }

        }
        private static void InitMachineAccount()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/config/MachineAccount.xml");

            XmlNodeList accountList = doc.SelectNodes("/Accounts/Account");

            foreach (XmlNode node in accountList)
            {
                AccountModel accountModel = new AccountModel();
                accountModel.Ip = node.Attributes["Ip"].Value;
                accountModel.UserName = node.Attributes["UserName"].Value;
                accountModel.Password = node.Attributes["Password"].Value;


                accountDic.Add(accountModel.Ip, accountModel);
            }

        }

        private static string RegexForIp(string regexStr)
        {
            Regex reg = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

            Match match = reg.Match(regexStr);

            return match.Value;
        }

        private static void ServiceAction(ProjectModel projectModel, Action action)
        {
            //sc stop ETicketServer
            //sc \\192.168.50.25 stop DETContractServer

            const string stopTemplate = @"sc \\{0} stop {1}";
            const string startTemplate = @"sc \\{0} start {1}";

            string command = string.Empty;

            switch (action)
            {
                case Action.Start:
                    command = string.Format(startTemplate, projectModel.ServiceRemoteIp, projectModel.Name);
                    break;
                case Action.Stop:
                    command = string.Format(stopTemplate, projectModel.ServiceRemoteIp, projectModel.Name);
                    break;
                default:
                    break;
            }

            CmdExecute cmd = new CmdExecute();
            cmd.ExecuteCommandSync(command);
        }

        private static void ToolAction(ProjectModel projectModel, Action action)
        {
            AccountModel account;
            if (!accountDic.TryGetValue(projectModel.ToolRemoteIp, out account))
            {
                Log.WriteMessage("无法获取服务器的账号，请核对账号配置文件！");
                return;
            }


            RemoteExecute remoteExecute = new RemoteExecute(projectModel.ToolRemoteIp, account.UserName, account.Password);

            switch (action)
            {
                case Action.Start:
                    string cmdTemplate = "{} -autostart";
                    string exePath = projectModel.RemoteToolPathForLocal + projectModel.ProcessName;
                    remoteExecute.StartProcess(string.Format(cmdTemplate, exePath));
                    break;
                case Action.Stop:
                    var theDic = remoteExecute.GetProcessList();

                    List<ProcessModel> processList;
                    theDic.TryGetValue(projectModel.ProcessName, out processList);

                    foreach (var item in processList)
                    {
                        if (item.ExecutablePath == projectModel.RemoteToolPathForLocal + projectModel.ProcessName)
                        {
                            remoteExecute.KillProcess(item.ManagementObj);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private static void CompressWebSite(ProjectModel projectMode)
        {
            string cmdTemplate = "{0} \"{1}\"";

            string command = string.Format(cmdTemplate, ConfigurationManager.AppSettings["MtimeCompressToolPath"], projectMode.LocalSitePath);

            CmdExecute cmd = new CmdExecute();
            cmd.ExecuteCommandSync(command);
        }

        /*
         * 1.读取主站VERSION.txt
         * 2.写入到web.config
         * 3.拷贝版本号到指定目录
         * 4.删除站点下的local目录
         * 5.对主站进行特殊处理，需要拷贝资源到static的根目录下
         */
        private static void CopyToStaticDirectory(ProjectModel projectModel)
        {
            CmdExecute cmd = new CmdExecute();

            #region 1
            string mainVersionFilePath = string.Empty;

            if (projectModel.Name.StartsWith("MtimeWap"))
            {
                mainVersionFilePath = System.Configuration.ConfigurationManager.AppSettings["MtimeWapRootPath"] + "VERSION.txt";
            }
            else
            {
                mainVersionFilePath = System.Configuration.ConfigurationManager.AppSettings["MtimeMovieCommunityRootPath"] + "VERSION.txt";
            }

            Console.WriteLine("MainVersionFilePath: {0}", mainVersionFilePath);
            string version = System.IO.File.ReadAllText(mainVersionFilePath);
            Console.WriteLine("MainVersion: " + version);
            #endregion

            #region 2
            //取当前项目的路径
            XmlDocument doc = new XmlDocument();
            doc.Load(projectModel.LocalSitePath + "Web.config");

            XmlNode node;

            if (string.IsNullOrEmpty(doc.DocumentElement.NamespaceURI))
            {
                node =
                    doc.SelectSingleNode(@"/configuration/appSettings/add[@key=""StaticResourceServersVersion""]");
            }
            else
            {
                System.Xml.XmlNamespaceManager nsmanager = new System.Xml.XmlNamespaceManager(doc.NameTable);
                nsmanager.AddNamespace("x", doc.DocumentElement.NamespaceURI);

                node =
                    doc.SelectSingleNode(@"/x:configuration/x:appSettings/x:add[@key=""StaticResourceServersVersion""]",
                                    nsmanager);
            }
            node.Attributes["value"].Value = version;
            doc.Save(projectModel.LocalSitePath + "Web.config");
            #endregion


            //2014-2-10 小东要求特殊static目录下是local版本，所以将主站版本号提前拷贝
            if (projectModel.Name == "MtimeMovieCommunityRoot")
            {
                string copyStaticCommand =
                    @"xcopy ""C:\Inetpub\wwwroot\MtimeMovieCommunityRoot\{0}\local\{0}"" ""C:\Inetpub\wwwroot\MtimeMovieCommunityStatic\static\"" /Y /I /Q /S";

                cmd.ExecuteCommandSync(string.Format(copyStaticCommand, version));
            }

            #region 3,4

            if (!string.IsNullOrEmpty(projectModel.StaticPath))
            {
                //command
                string copyCommand = @"xcopy ""{0}{1}\local"" ""{2}"" /Y /I /Q /S";

                string subVersion = System.IO.File.ReadAllText(projectModel.LocalSitePath + "VERSION.txt");

                Console.WriteLine("CopyCommand: {0}", string.Format(copyCommand, projectModel.LocalSitePath, subVersion, projectModel.StaticPath));

                cmd.ExecuteCommandSync(string.Format(copyCommand, projectModel.LocalSitePath, subVersion, projectModel.StaticPath));

                string rdCommand = @"RD /S /Q ""{0}{1}\local""";

                cmd.ExecuteCommandSync(string.Format(rdCommand, projectModel.LocalSitePath, subVersion));
            }
            #endregion
        }

    }


    public enum Action
    {
        Start,
        Stop
    }

    public class ProjectModel
    {
        public string Name { get; set; }

        public string StaticPath { get; set; }

        public string LocalSitePath { get; set; }
        public string SiteSourcePath { get; set; }
        public string RemoteSitePath { get; set; }
        public string SiteRemoteIp { get; set; }

        public string LocalServicePath { get; set; }
        public string RemoteServicePath { get; set; }
        public string ServiceSourcePath { get; set; }
        public string ServiceRemoteIp { get; set; }

        public string LocalToolPath { get; set; }
        public string RemoteToolPath { get; set; }
        public string RemoteToolPathForLocal { get; set; }
        public string ToolSourcePath { get; set; }
        public string ToolRemoteIp { get; set; }
        public string ProcessName { get; set; }
    }


    public class AccountModel
    {
        public string Ip { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
