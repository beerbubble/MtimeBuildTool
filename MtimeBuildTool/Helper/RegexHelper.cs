using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MtimeBuildTool.Helper
{
    public static class RegexHelper
    {
        public static string RegexForIp(string regexStr)
        {
            Regex reg = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

            Match match = reg.Match(regexStr);

            return match.Value;
        }
    }
}
