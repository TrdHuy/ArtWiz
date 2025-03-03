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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length >= 2)
            {
                ZipFilePath = e.Args[0];
                InstallPath = e.Args[1];
            }
            // FOR TEST ONLY 
            ZipFilePath = "D:\\Workspace\\Temp\\ArtWiz\\SPRNetToolTest\\Resources\\ArtWiz.zip";
            InstallPath = "D:\\Workspace\\Temp\\ArtWiz\\SPRNetTool\\bin\\x64\\Debug\\ForTestUpdater";
            mLogger.I($"ArtUpdater started with ZIP File: {ZipFilePath}, Install Path: {InstallPath}");
        }
    }

}
