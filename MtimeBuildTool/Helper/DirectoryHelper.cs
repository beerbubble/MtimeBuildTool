using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace MtimeBuildTool.Helper
{
    public class DirectoryHelper
    {
        public static void DirectoryRemove(string path)
        {
            var dir = new DirectoryInfo(path);

            Log.WriteMessage(string.Format("目录是否存在: {0}", dir.Exists.ToString()));

            int i = 0;
            while (i < 5)
            {
                try
                {
                    DeleteFileSystemInfo(dir);
                    break;
                }
                catch (Exception ex)
                {
                }
                Thread.Sleep(3000);
                i++;
            }
            Log.WriteMessage("Remove Success！");
        }

        public static void DirectoryFilesRemove(string path)
        {
            var dir = new DirectoryInfo(path);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If copying subdirectories, copy them and their contents to new location. 
            foreach (DirectoryInfo subdir in dirs)
            {
                DeleteFileSystemInfo(subdir);
            }
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static void DeleteFileSystemInfo(FileSystemInfo fileSystemInfo)
        {
            var directoryInfo = fileSystemInfo as DirectoryInfo;
            if (directoryInfo != null)
            {
                foreach (var childInfo in directoryInfo.GetFileSystemInfos())
                {
                    DeleteFileSystemInfo(childInfo);
                }
            }

            fileSystemInfo.Attributes = FileAttributes.Normal;
            fileSystemInfo.Delete();
        }


        public static void CreateDir(string destDirName)
        {

            while (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                Thread.Sleep(1 * 1000);
            }

        }
    }


}
