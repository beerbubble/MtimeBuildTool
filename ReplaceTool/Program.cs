using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ReplaceTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding encoding = new UTF8Encoding(false);

            string sbSource = string.Empty;
            string sbOutput = string.Empty;

            string filePath = args[0];

            Console.WriteLine("FilePath: " + filePath);

            Console.WriteLine("初始:" + File.GetAttributes(filePath).ToString());

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

            for (int i = 1; i < args.Length; i++)
            {
                string[] argArray = args[i].Split('$');

                Console.WriteLine(i + " replaceTarget: " + argArray[0] + "======== replaceValue: " + argArray[1]);

                string replaceTarget = argArray[0];

                string replaceValue = argArray[1];

                if (Regex.IsMatch(sbSource, replaceTarget, RegexOptions.IgnoreCase))
                {
                    sbSource = Regex.Replace(sbSource, replaceTarget, replaceValue);
                }
            }


            try
            {
                File.WriteAllText(filePath, sbSource, encoding);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }


            Console.WriteLine("替换完成");
        }
    }
}
