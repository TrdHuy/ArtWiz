using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SPRNetTool.LogUtil
{
    public class Logger
    {
        private const string LOG_FOLDER = "temp\\logs";
        private const string PROJECT_TAG = "ArtWiz";
        public static StreamWriter LogWriter;
        private static FileStream _logFs;
        private string classTag;

        static Logger()
        {
            var dateTimeNow = DateTime.Now.ToString("ddMMyyHHmmss");
            var logFileName =
                Assembly.GetCallingAssembly().GetName().Name + "_" +
                Assembly.GetCallingAssembly().GetName().Version + "_" +
                dateTimeNow + ".txt";

            if (!Directory.Exists(LOG_FOLDER))
            {
                Directory.CreateDirectory(LOG_FOLDER);
            }

            var filePath = LOG_FOLDER + @"\" + logFileName;

            _logFs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            LogWriter = new StreamWriter(_logFs);

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public Logger(string tag)
        {
            classTag = tag;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            LogWriter.WriteLine($"Unhandled Exception: {exception?.Message ?? ""}");
            LogWriter.WriteLine($"StackTrace: {exception?.StackTrace ?? ""}");
            LogWriter.WriteLine($"Occurred at: {DateTime.Now}");
            LogWriter.Close();
            LogWriter.Dispose();
            _logFs.Close();
            _logFs.Dispose();
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            LogWriter.Close();
            LogWriter.Dispose();
            _logFs.Close();
            _logFs.Dispose();
        }


        public void D(string message, [CallerMemberName] string caller = "")
        {
#if DEBUG
            var log = $"{DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss:fff")}\tD\t{PROJECT_TAG}\t{classTag}\t{caller}\t{message}";
            Debug.WriteLine(log);
            LogWriter.WriteLine(log);
#endif
        }

        public void I(string message, [CallerMemberName] string caller = "")
        {
            var log = $"{DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss:fff")}\tI\t{PROJECT_TAG}\t{classTag}\t{caller}\t{message}";
            Debug.WriteLine(log);
            LogWriter.WriteLine(log);
        }

        public void E(string message, [CallerMemberName] string caller = "")
        {
            var log = $"{DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss:fff")}\tE\t{PROJECT_TAG}\t{classTag}\t{caller}\t{message}";
            Debug.WriteLine(log);
            LogWriter.WriteLine(log);
        }


        public static class Raw
        {
            public static void D(string message, [CallerMemberName] string caller = "")
            {
#if DEBUG
                var log = $"{DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss:fff")}\tD\t{PROJECT_TAG}\t{caller}\t{message}";
                Debug.WriteLine(log);
                LogWriter.WriteLine(log);
#endif
            }

            public static void I(string message, [CallerMemberName] string caller = "")
            {
                var log = $"{DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss:fff")}\tI\t{PROJECT_TAG}\t{caller}\t{message}";
                Debug.WriteLine(log);
                LogWriter.WriteLine(log);
            }

            public static void E(string message, [CallerMemberName] string caller = "")
            {
                var log = $"{DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss:fff")}\tE\t{PROJECT_TAG}\t{caller}\t{message}";
                Debug.WriteLine(log);
                LogWriter.WriteLine(log);
            }
        }
    }
}
