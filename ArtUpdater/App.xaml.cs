using ArtUpdater.Utils;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ArtUpdater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger mLogger = new Logger("App");

        public static string ZipFilePath { get; private set; } = "";
        public static string InstallPath { get; private set; } = "";
        public static List<string> PrioritySearchingProcess { get; private set; } = new List<string>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length >= 2)
            {
                ZipFilePath = e.Args[0];
                InstallPath = e.Args[1];
                if (!string.IsNullOrEmpty(e.Args[2]))
                {
                    PrioritySearchingProcess = e.Args[2].Split(";").ToList();
                    mLogger.I($"ArtUpdater started with ZIP File: {ZipFilePath}, " +
                       $"Install Path: {InstallPath}, " +
                       $"PrioritySearchingProcess: {e.Args[2]}");
                }
                else
                {
                    mLogger.I($"ArtUpdater started with ZIP File: {ZipFilePath}, " +
                        $"Install Path: {InstallPath}");
                }

            }
            // FOR TEST ONLY 
            //    ZipFilePath = "C:\\Users\\Hp\\Desktop\\temp\\test\\bin\\Rampart\\ArtWiz\\SPRNetToolTest\\Resources\\ArtWiz.zip";
            //    InstallPath = "C:\\Users\\Hp\\Desktop\\temp\\test\\bin\\Rampart\\ArtWiz\\SPRNetTool\\bin\\x64\\Debug\\ForTestUpdater";
            //    PrioritySearchingProcess = new List<string> { "ArtWiz" };
        }
    }

}
