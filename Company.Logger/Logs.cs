using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Logger
{
    public class Logs
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static void LogInfo(string message)
        {
            Logger.Info(message);
        }
        public static void LogError(string message)
        {
            Logger.Error(message);
        }
        public static void LogDebug(string message)
        {
            Logger.Debug(message);
        }
        public static void LogWarning(string message)
        {
            Logger.Warn(message);
        }
        public static void LogFatal(string message)
        {
            Logger.Fatal(message);
        }
        public static void LogTrace(string message)
        {
            Logger.Trace(message);
        }
        public static void LogError(Exception ex)
        {
            Logger.Error(ex.Message+ex.StackTrace);
        }
       
    }
}
