using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MtimePackageClean
{
    class Program
    {
        static void Main(string[] args)
        {
            CleanPackageDir(@"E:\Package\Tool\");
            CleanPackageDir(@"E:\Package\Service\");
            CleanPackageDir(@"E:\Package\WebSite\");

            Console.ReadKey();
        }

        static void CleanPackageDir(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            List<DirectoryInfo> dirs = dir.GetDirectories().ToList();

            foreach (var subDir in dirs)
            {
                Console.WriteLine("Tool SubDir:" + subDir.Name + " Count " + subDir.GetDirectories().Count());

                if (subDir.GetDirectories().Count() <= 10) continue;

                Console.WriteLine(subDir.Name + " Begin");

                List<DirectoryInfo> dateDirs = subDir.GetDirectories().OrderByDescending(dateDir => dateDir.Name).ToList();

                for (int i = 10; i < dateDirs.Count; i++)
                {
                    Console.WriteLine(dateDirs[i].Name);

                    dateDirs[i].Delete(true);
                }

                Console.WriteLine(subDir.Name + " Finish!");
            }
        }
    }
}
