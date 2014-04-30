using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace MtimeBuildTool.Utility
{
    public class RemoteExecute
    {
        private ConnectionOptions connOptions;
        private ManagementScope theScope;

        public RemoteExecute(string Ip, string userName, string password)
        {
            this.connOptions = new ConnectionOptions();
            this.connOptions.Impersonation = ImpersonationLevel.Impersonate;
            this.connOptions.EnablePrivileges = true;
            this.connOptions.Username = "administrator";
            this.connOptions.Password = "1";

            this.theScope = new ManagementScope(string.Format(@"\\{0}\root\cimv2", Ip), connOptions);
        }

        public bool Connect()
        {
            try
            {
                theScope.Connect();
                return true;
            }
            catch (Exception e)
            {
                return false;
                //throw new Exception("Management Connect to remote machine " + remoteComputerName + " as user " + strUserName + " failed with the following error " + e.Message);
            }
        }

        public Dictionary<string, List<ProcessModel>> GetProcessList()
        {

            ObjectQuery theQuery = new ObjectQuery("SELECT * FROM Win32_Process");

            ManagementObjectSearcher theSearcher = new ManagementObjectSearcher(theScope, theQuery);

            ManagementObjectCollection theCollection = theSearcher.Get();

            //return theCollection;

            Dictionary<string, List<ProcessModel>> dic = new Dictionary<string, List<ProcessModel>>();

            string[] sep = { "\n", "\t" };
            foreach (ManagementObject theCurObject in theCollection)
            {
                string caption = theCurObject.GetText(TextFormat.Mof);
                string[] split = caption.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                string name = string.Empty;
                int processId = 0;
                string commandLine = string.Empty;
                string executablePath = string.Empty;
                // Iterate through the splitter
                for (int i = 0; i < split.Length; i++)
                {

                    if (split[i].Split('=').Length > 1)
                    {
                        string[] procDetails = split[i].Split('=');
                        procDetails[1] = procDetails[1].Replace(@"""", "");
                        procDetails[1] = procDetails[1].Replace(';', ' ');
                        procDetails[1] = procDetails[1].Trim();
                        switch (procDetails[0].Trim().ToLower())
                        {
                            case "executablepath":
                                executablePath = procDetails[1].Replace("\\\\", @"\");
                                break;
                            case "commandline":
                                commandLine = procDetails[1].Replace("\\\\", @"\");
                                break;
                            case "caption":
                                break;
                            case "csname":
                                break;
                            case "description":
                                break;
                            case "name":
                                name = procDetails[1];
                                break;
                            case "priority":
                                break;
                            case "processid":
                                processId = Convert.ToInt32(procDetails[1]);
                                break;
                            case "sessionid":

                                break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(name))
                {
                    var model = new ProcessModel()
                        {
                            ProcessId = processId,
                            ManagementObj = theCurObject,
                            CommandLine = commandLine,
                            ExecutablePath = executablePath
                        };


                    List<ProcessModel> modelList;
                    if (dic.TryGetValue(name, out modelList))
                    {
                        modelList.Add(model);
                    }
                    else
                    {
                        modelList = new List<ProcessModel>();
                        modelList.Add(model);

                        dic.Add(name, modelList);

                    }
                }
                //Console.WriteLine(sb.ToString());
            }

            foreach (var item in dic)
            {
                foreach (var model in item.Value)
                {
                    Console.WriteLine(model.ExecutablePath);
                }
            }
            return dic;
        }


        public void KillProcess(ManagementObject managementObj)
        {
            managementObj.InvokeMethod("Terminate", null);
        }

        public void StartProcess(string command)
        {
            ManagementPath p = new ManagementPath("Win32_Process");
            ManagementClass processClass = new ManagementClass(theScope, p, null);
            ManagementBaseObject inParams =
            processClass.GetMethodParameters("Create");
            inParams["CommandLine"] = command;//@"C:\Inetpub\MtimeService\Staticize\MtimeMessageProcessor.exe -autostart";
            ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, null);
        }
    }

    public class ProcessModel
    {
        public string CommandLine { get; set; }
        public string ExecutablePath { get; set; }
        public int ProcessId { get; set; }
        public ManagementObject ManagementObj { get; set; }
    }
}
