using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace MtimeBuildTool.Helper
{
    public class ProjectMapHelper
    {
        private static Dictionary<string, ProjectModel> projectDic = new Dictionary<string, ProjectModel>();

        static ProjectMapHelper()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/config/ProjectMap.xml");

            XmlNodeList websiteList = doc.SelectNodes("/MtimeProject/WebSites/WebSite");

            foreach (XmlNode node in websiteList)
            {
                ProjectModel projectMapModel = new ProjectModel();
                projectMapModel.Name = node.Attributes["Name"].Value;
                projectMapModel.LocalSitePath = node.Attributes["LocalSitePath"].Value;
                if (node.Attributes["RemoteSitePath"] != null)
                {
                    projectMapModel.RemoteSitePath = node.Attributes["RemoteSitePath"].Value;
                    projectMapModel.SiteRemoteIp = RegexHelper.RegexForIp(projectMapModel.RemoteSitePath);
                }
                if (node.Attributes["StaticPath"] != null)
                    projectMapModel.StaticPath = node.Attributes["StaticPath"].Value;
                if (node.Attributes["SiteSourcePath"] != null)
                    projectMapModel.SiteSourcePath = node.Attributes["SiteSourcePath"].Value;
                if (node.Attributes["LocalSitePackagePath"] != null)
                    projectMapModel.LocalSitePackagePath = node.Attributes["LocalSitePackagePath"].Value;

                if (node.Attributes["SitePackageName"] != null)
                    projectMapModel.SitePackageName = node.Attributes["SitePackageName"].Value;
                

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
                    projectMapModel.ServiceName = node.Attributes["ServiceName"].Value;
                    if (node.Attributes["RemoteServicePath"] != null)
                    {
                        projectMapModel.RemoteServicePath = node.Attributes["RemoteServicePath"].Value;
                        projectMapModel.ServiceRemoteIp = RegexHelper.RegexForIp(projectMapModel.RemoteServicePath);
                    }

                    if (node.Attributes["LocalServicePackagePath"] != null)
                        projectMapModel.LocalServicePackagePath = node.Attributes["LocalServicePackagePath"].Value;


                    if (node.Attributes["ServicePackageName"] != null)
                        projectMapModel.ServicePackageName = node.Attributes["ServicePackageName"].Value;
                }
                else
                {
                    ProjectModel newProjectMapModel = new ProjectModel();
                    newProjectMapModel.Name = node.Attributes["Name"].Value;
                    newProjectMapModel.LocalServicePath = node.Attributes["LocalServicePath"].Value;
                    newProjectMapModel.ServiceSourcePath = node.Attributes["ServiceSourcePath"].Value;
                    newProjectMapModel.ServiceName = node.Attributes["ServiceName"].Value;
                    if (node.Attributes["RemoteServicePath"] != null)
                    {
                        newProjectMapModel.RemoteServicePath = node.Attributes["RemoteServicePath"].Value;
                        newProjectMapModel.ServiceRemoteIp = RegexHelper.RegexForIp(newProjectMapModel.RemoteServicePath);
                    }

                    if (node.Attributes["LocalServicePackagePath"] != null)
                        newProjectMapModel.LocalServicePackagePath = node.Attributes["LocalServicePackagePath"].Value;

                    if (node.Attributes["ServicePackageName"] != null)
                        newProjectMapModel.ServicePackageName = node.Attributes["ServicePackageName"].Value;

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
                        projectMapModel.ToolRemoteIp = RegexHelper.RegexForIp(projectMapModel.RemoteToolPath);
                    }

                    projectMapModel.RemoteToolPathForLocal = node.Attributes["RemoteToolPathForLocal"].Value;
                    projectMapModel.ProcessName = node.Attributes["ProcessName"].Value;
                    projectMapModel.AutoStart = bool.Parse(node.Attributes["AutoStart"].Value);

                    if (node.Attributes["LocalToolPackagePath"] != null)
                        projectMapModel.LocalToolPackagePath = node.Attributes["LocalToolPackagePath"].Value;

                    if (node.Attributes["ToolPackageName"] != null)
                        projectMapModel.ToolPackageName = node.Attributes["ToolPackageName"].Value;
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
                        newProjectMapModel.ToolRemoteIp = RegexHelper.RegexForIp(newProjectMapModel.RemoteToolPath);
                    }
                    newProjectMapModel.RemoteToolPathForLocal = node.Attributes["RemoteToolPathForLocal"].Value;
                    newProjectMapModel.ProcessName = node.Attributes["ProcessName"].Value;
                    newProjectMapModel.AutoStart = bool.Parse(node.Attributes["AutoStart"].Value);

                    if (node.Attributes["LocalToolPackagePath"] != null)
                        projectMapModel.LocalToolPackagePath = node.Attributes["LocalToolPackagePath"].Value;
                    if (node.Attributes["ToolPackageName"] != null)
                        projectMapModel.ToolPackageName = node.Attributes["ToolPackageName"].Value;

                    projectDic.Add(newProjectMapModel.Name, newProjectMapModel);
                }
            }
        }

        public static Dictionary<string, ProjectModel> ProjectDic
        {
            get
            {
                return projectDic;
            }
        }
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
        public string ServiceName { get; set; }
        

        public string LocalToolPath { get; set; }
        public string RemoteToolPath { get; set; }
        public string RemoteToolPathForLocal { get; set; }
        public string ToolSourcePath { get; set; }
        public string ToolRemoteIp { get; set; }
        public string ProcessName { get; set; }
        public bool AutoStart { get; set; }

        public string LocalSitePackagePath { get; set; }
        public string SitePackageName { get; set; }
        
        public string LocalServicePackagePath { get; set; }
        public string ServicePackageName { get; set; }
        
        public string LocalToolPackagePath { get; set; }
        public string ToolPackageName { get; set; }

    }

}
