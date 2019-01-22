using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Soyuan.Theme.Core.Helper
{
    public class LogHelper
    {
        private static log4net.ILog logger = null;
        public static void logInfo(string logstr)
        {
            Console.WriteLine(logstr);
            if (initLog4net())
                logger.Info(logstr);
        }

        public static void logInfo(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            if (initLog4net())
                logger.InfoFormat(format, args);
        }

        public static void logError(string logstr, Exception e)
        {
            Console.WriteLine(logstr);
            if (initLog4net())
                logger.Error(logstr, e);
        }

        public static void logError(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            if (initLog4net())
                logger.ErrorFormat(format, args);
        }

        private static object objlock = new object();//初始化log用的锁
        private static bool initLog4net()
        {
            if (logger != null)
                return true;
            lock (objlock)
            {
                if (logger == null)
                {
                    ILoggerRepository repository = LogManager.CreateRepository("NETCoreRepository");
                    XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
                    logger = LogManager.GetLogger(repository.Name, "NETCorelog4net");
                    return true;
                }
            }
            return false;
        }


        //private static object objTrace1 = new object();
        //private static object objTrace2 = new object();
        //private static object objTrace3 = new object();
        //private static string strLogFile = AppDomain.CurrentDomain.BaseDirectory;  //得到保存文件路径



        ///// <summary>
        ///// 日志文件路径，绝对路径
        ///// </summary>
        //public static string LogFile
        //{
        //    set
        //    {
        //        strLogFile = value;
        //    }
        //    get
        //    {
        //        return strLogFile;
        //    }
        //}

        ///// <summary>
        ///// 保存服务器跟踪错误日志
        ///// </summary>
        ///// <param name="strMessage"></param>
        ///// <param name="strTargetSite"></param>
        ///// <param name="strSource"></param>
        ///// <param name="strStackTrace"></param>
        //public static void WriteTraceLog(string strMessage, string strTargetSite, string strSource, string strStackTrace)
        //{
        //    lock (objTrace1)
        //    {
        //        string strFileName = GetLogFile("Err");
        //        StringBuilder sbErrorMsg = new StringBuilder();
        //        sbErrorMsg.Append("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]: " + strMessage);
        //        sbErrorMsg.Append(strTargetSite);
        //        sbErrorMsg.Append(strSource);
        //        sbErrorMsg.Append(strStackTrace);
        //        StreamWriter sw = File.AppendText(strFileName);
        //        sw.WriteLine(sbErrorMsg.ToString());
        //        sw.Flush();
        //        sw.Close();
        //    }
        //}

        ///// <summary>
        ///// 保存服务器日志
        ///// </summary>
        ///// <param name="strMessage"></param>
        //public static void WriteTraceLog(string strMessage)
        //{
        //    lock (objTrace2)
        //    {
        //        string strFileName = GetLogFile("Log");
        //        StringBuilder sbErrorMsg = new StringBuilder();
        //        sbErrorMsg.Append("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]: " + strMessage);
        //        StreamWriter sw = File.AppendText(strFileName);
        //        sw.WriteLine(sbErrorMsg.ToString());
        //        sw.Flush();
        //        sw.Close();
        //    }
        //}

        ///// <summary>
        ///// 保存SQL日志
        ///// </summary>
        ///// <param name="strMessage"></param>
        //public static void WriteSqlLog(string strMessage, string filePath = "Log")
        //{
        //    lock (objTrace3)
        //    {
        //        string strFileName = GetLogFile(filePath);
        //        StringBuilder sbErrorMsg = new StringBuilder();
        //        sbErrorMsg.Append("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "]: " + strMessage);
        //        StreamWriter sw = File.AppendText(strFileName);
        //        sw.WriteLine(sbErrorMsg.ToString());
        //        sw.Flush();
        //        sw.Close();
        //    }
        //}

        //private static string GetLogFile(string strIndex)
        //{
        //    DateTime dtCurDate = DateTime.Now;
        //    string filePath = strLogFile + "" + dtCurDate.ToString("yyyyMM") + "." + strIndex;
        //    if (!Directory.Exists(filePath))
        //    {
        //        Directory.CreateDirectory(filePath);  //按月创建目录保存日志文件
        //    }


        //    string strFileName = filePath + "\\" + strIndex + dtCurDate.ToString("yyyyMMdd") + ".log";

        //    if (!System.IO.File.Exists(strFileName))
        //    {
        //        System.IO.File.Create(strFileName);
        //    }
        //    return strFileName;
        //}

        ///// <summary>
        ///// 数据接口日志
        ///// </summary>
        ///// <param name="strMessage"></param>
        //public static void WriteOperationLog(string strMessage)
        //{
        //    lock (objTrace2)
        //    {
        //        string strFileName = GetLogFile("ChuLiLog");
        //        StringBuilder sbErrorMsg = new StringBuilder();
        //        sbErrorMsg.Append("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]: " + strMessage);
        //        StreamWriter sw = File.AppendText(strFileName);
        //        sw.WriteLine(sbErrorMsg.ToString());
        //        sw.Flush();
        //        sw.Close();
        //    }
        //}

        //public void Dispose()
        //{
        //}
    }
}
