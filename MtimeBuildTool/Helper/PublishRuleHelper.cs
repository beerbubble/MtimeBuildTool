using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MtimeBuildTool.Const;

namespace MtimeBuildTool.Helper
{
    public class PublishRuleHelper
    {
        private static Dictionary<string, Dictionary<string, List<RuleItem>>> publishRuleDic = new Dictionary<string, Dictionary<string, List<RuleItem>>>();

        static PublishRuleHelper()
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreWhitespace = true;
            readerSettings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + "/config/PublishRule.xml", readerSettings);
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            XmlNodeList ruleList = doc.SelectNodes("/Rules/Rule");

            foreach (XmlNode node in ruleList)
            {
                string name = node.Attributes["Name"].Value;
                publishRuleDic.Add(name, new Dictionary<string, List<RuleItem>>());
                foreach (XmlNode item in node.ChildNodes)
                {
                    publishRuleDic[name].Add(item.Name, getRuleItem(item));
                }
            }
        }

        public static Dictionary<string, Dictionary<string, List<RuleItem>>> PublishRuleDic
        {
            get
            {
                return publishRuleDic;
            }
        }

        private static List<RuleItem> getRuleItem(XmlNode node)
        {
            List<RuleItem> list = new List<RuleItem>();

            foreach (XmlNode ruleItem in node.ChildNodes)
            {
                RuleType tmpType = (RuleType)Enum.Parse(typeof(RuleType), ruleItem.Attributes["Type"].Value);
                string tmpDir = string.Empty;
                if (ruleItem.Attributes["dir"] != null)
                {
                    tmpDir = ruleItem.Attributes["dir"].Value;
                }

                string tmpFile = string.Empty;
                if (ruleItem.Attributes["file"] != null)
                {
                    tmpFile = ruleItem.Attributes["file"].Value;
                }

                string tmpFind = string.Empty;
                if (ruleItem.Attributes["find"] != null)
                {
                    tmpFind = ruleItem.Attributes["find"].Value;
                }

                string tmpReplace = string.Empty;
                if (ruleItem.Attributes["replace"] != null)
                {
                    tmpReplace = ruleItem.Attributes["replace"].Value;
                }

                string tmpXpath = string.Empty;
                if (ruleItem.Attributes["xpath"] != null)
                {
                    tmpXpath = ruleItem.Attributes["xpath"].Value;
                }

                string tmpAttribute = string.Empty;
                if (ruleItem.Attributes["attribute"] != null)
                {
                    tmpAttribute = ruleItem.Attributes["attribute"].Value;
                }

                string tmpValue = string.Empty;
                if (ruleItem.Attributes["value"] != null)
                {
                    tmpValue = ruleItem.Attributes["value"].Value;
                }

                string tmpSourceFileName = string.Empty;
                if (ruleItem.Attributes["sourceFileName"] != null)
                {
                    tmpSourceFileName = ruleItem.Attributes["sourceFileName"].Value;
                }

                string tmpDestFileName = string.Empty;
                if (ruleItem.Attributes["destFileName"] != null)
                {
                    tmpDestFileName = ruleItem.Attributes["destFileName"].Value;
                }

                list.Add(new RuleItem()
                {
                    Type = tmpType,
                    Dir = tmpDir,
                    File = tmpFile,
                    Find = tmpFind,
                    Replace = tmpReplace,
                    Xpath = tmpXpath,
                    Attribute = tmpAttribute,
                    Value = tmpValue,
                    SourceFileName=tmpSourceFileName,
                    DestFileName = tmpDestFileName
                });
            }

            return list;
        }
    }

    public class RuleItem
    {
        public RuleType Type { get; set; }
        public string Dir { get; set; }
        public string File { get; set; }
        public string Find { get; set; }
        public string Replace { get; set; }
        public string Xpath { get; set; }
        public string Attribute { get; set; }
        public string Value { get; set; }
        public string SourceFileName { get; set; }
        public string DestFileName { get; set; }
    }
}
