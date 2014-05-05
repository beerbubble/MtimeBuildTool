using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using MtimeBuildTool.Utility;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Security;
using Microsoft.Win32.SafeHandles;

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
            if (path.StartsWith(@"\\"))
            {
                using (Impersonation im = new Impersonation())
                {
                    AccountModel am = MachineAccountHelper.accountDic["192.168.50.22"];
                    im.Impersonate(am);
                    // If the destination directory doesn't exist, create it. 
                    Console.WriteLine("Directory Start1");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    var dir = new DirectoryInfo(path);

                    // Get the files in the directory and copy them to the new location.
                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        file.Attributes = FileAttributes.Normal;
                        int i = 0;
                        while (i < 5)
                        {
                            try
                            {
                                file.Delete();
                                break;
                            }
                            catch (Exception ex)
                            {
                            }
                            Thread.Sleep(2000);
                            i++;
                        }

                    }

                    DirectoryInfo[] dirs = dir.GetDirectories();
                    // If copying subdirectories, copy them and their contents to new location. 
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        DeleteFileSystemInfo(subdir);
                    }
                }
            }
            else
            {
                // If the destination directory doesn't exist, create it. 
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var dir = new DirectoryInfo(path);

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    file.Attributes = FileAttributes.Normal;
                    int i = 0;
                    while (i < 5)
                    {
                        try
                        {
                            file.Delete();
                            break;
                        }
                        catch (Exception ex)
                        {
                        }
                        Thread.Sleep(2000);
                        i++;
                    }

                }

                DirectoryInfo[] dirs = dir.GetDirectories();
                // If copying subdirectories, copy them and their contents to new location. 
                foreach (DirectoryInfo subdir in dirs)
                {
                    DeleteFileSystemInfo(subdir);
                }
            }
        }

        //public static void DirectoryFilesRemove(string path,bool remote)
        //{
        //    SafeTokenHandle safeTokenHandle;

        //    const int LOGON32_PROVIDER_DEFAULT = 0;
        //    //This parameter causes LogonUser to create a primary token. 
        //    const int LOGON32_LOGON_INTERACTIVE = 2;
        //    const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        //    bool returnValue = LogonUser("administrator", "192.168.50.22", "112233", LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

        //    Console.WriteLine(returnValue);

        //    if (returnValue == true)
        //    {
        //        WindowsIdentity newId = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
        //        using (WindowsImpersonationContext impersonatedUser = newId.Impersonate())
        //        {
        //            DirectoryFilesRemove(path);
        //        }
        //    }
        //}

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

        //public static void CreateDir(string destDirName)
        //{

        //    while (!Directory.Exists(destDirName))
        //    {
        //        Directory.CreateDirectory(destDirName);
        //        Thread.Sleep(1 * 1000);
        //    }

        //}

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);
        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            {
            }

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }
    }


}
