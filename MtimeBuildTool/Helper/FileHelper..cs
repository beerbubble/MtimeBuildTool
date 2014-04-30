using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MtimeBuildTool.Helper
{
    public class FileHelper
    {
        public void EditConfig(RuleItem rule)
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
    }
}
