﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mtime.Log;

namespace MtimeBuildTool.Helper
{
    public static class Log
    {
        private const string fileName = "MtimeBuildTool";

        public static void WriteMessage(string message)
        {
            WriteMessageByFileName(fileName, message);
        }

        public static void WriteMessageByFileName(string fileName, string message)
        {
            LogHelper.WriteMessage(fileName, message);
        }
    }
}