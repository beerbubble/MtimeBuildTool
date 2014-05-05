using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MtimeBuildTool.Helper
{
    public class MachineAccountHelper
    {
        public static Dictionary<string, AccountModel> accountDic = new Dictionary<string, AccountModel>();

        public void Init()
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
    }
}
