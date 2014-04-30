using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace MtimeBuildTool.Helper
{
    public class FileHelper
    {
        public static void EditConfig(RuleItem rule)
        {
            string configPath = rule.Dir + rule.File;
            string xpath = rule.Xpath;
            string attribute = rule.Attribute;
            string newValue = rule.Value;

            Console.WriteLine(configPath);
            Console.WriteLine(xpath);
            Console.WriteLine(attribute);
            Console.WriteLine(newValue);

            XmlDocument doc = new XmlDocument();
            doc.Load(configPath);

            XmlNode node;

            Console.WriteLine(doc.DocumentElement.NamespaceURI);

            if (string.IsNullOrEmpty(doc.DocumentElement.NamespaceURI))
            {
                node =
                    doc.SelectSingleNode(xpath);
            }
            else
            {
                System.Xml.XmlNamespaceManager nsmanager = new System.Xml.XmlNamespaceManager(doc.NameTable);
                nsmanager.AddNamespace("x", doc.DocumentElement.NamespaceURI);

                node =
                    doc.SelectSingleNode(xpath, nsmanager);
            }
            //foreach (var attributeString in node.Attributes)
            //{
            //    Console.WriteLine(attributeString.ToString());
            //}

            node.Attributes[attribute].Value = newValue;

            doc.Save(configPath);
        }

        public static void ReplaceContent(RuleItem rule)
        {
            Encoding encoding = new UTF8Encoding(false);

            string sbSource = string.Empty;
            string sbOutput = string.Empty;

            string filePath = rule.Dir + rule.File;
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

            //Dictionary<string, string> replaceDic = new Dictionary<string, string>();


            string replaceTarget = rule.Find;

            string replaceValue = rule.Replace;

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

            Console.WriteLine("替换完成");
        }
    }
}
