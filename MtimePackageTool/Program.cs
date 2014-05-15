using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using MtimeBuildTool.Helper;
using System.IO;

namespace MtimePackageTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string project = "MtimeMovieCommunityRoot";

            //获取当前部署的项目
            ProjectModel projectModel;

            if (!ProjectMapHelper.ProjectDic.TryGetValue(project, out projectModel))
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

                using (ZipFile zip = new ZipFile())
                {
                    zip.AddDirectory(zipPath);

                    zip.Save(Path.Combine(packagePath, projectModel.SitePackageName + DateTime.Now.ToString("yyyyMMddHHmm") + ".zip"));

                    DirectoryHelper.DirectoryCopy(packagePath, Path.Combine(@"\\192.168.0.25\ftproot\mtime\upversion\FrontWeb", DateTime.Now.ToString("yyyyMMdd")));
                }
            }
        }
    }
}
