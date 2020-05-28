using NLog;
using System;

namespace Voith.DAQ.Common
{
    public class LogHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Debug(Exception ex, string msg = "")
        {
            Logger.Debug(ex, msg);
        }

        public static void Error(Exception ex, string msg = "")
        {
            Logger.Error(ex, msg);
        }

        public static void Info(string msg)
        {
            Logger.Info(msg);
        }
    }

}
