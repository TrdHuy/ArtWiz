using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SPRNetTool.LogUtil
{
    public class Logger
    {
        private const string LOG_FOLDER = "temp\\logs";
        private const string TAG = "ArtWiz";
        private static Logger _instance = new Logger();
        private StreamWriter _logWriter;
        private FileStream _logFs;

        static Logger()
        {
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            _instance._logWriter.WriteLine($"Unhandled Exception: {exception?.Message ?? ""}");
            _instance._logWriter.WriteLine($"StackTrace: {exception?.StackTrace ?? ""}");
            _instance._logWriter.WriteLine($"Occurred at: {DateTime.Now}");
            _instance._logWriter.Close();
            _instance._logWriter.Dispose();
            _instance._logFs.Close();
            _instance._logFs.Dispose();
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            _instance._logWriter.Close();
            _instance._logWriter.Dispose();
            _instance._logFs.Close();
            _instance._logFs.Dispose();
        }

        private Logger()
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
            _logWriter = new StreamWriter(_logFs);
        }

        public static void D(string message)
        {
            var log = $"{DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss:fff")}\tD\t{TAG}\t{message}";
            Debug.WriteLine(log);
#if DEBUG
            _instance._logWriter.WriteLine(log);
#endif
        }

        public static void I(string message)
        {
            var log = $"{DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss:fff")}\tI\t{TAG}\t{message}";
            Debug.WriteLine(log);
            _instance._logWriter.WriteLine(log);
        }

        public static void E(string message)
        {
            var log = $"{DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss:fff")}\tE\t{TAG}\t{message}";
            Debug.WriteLine(log);
            _instance._logWriter.WriteLine(log);
        }

    }
}
