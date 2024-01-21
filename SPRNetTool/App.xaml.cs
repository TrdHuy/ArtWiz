using ArtWiz.LogUtil;
using System.Windows;
using WizMachine;

namespace ArtWiz
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            EngineKeeper.Init(Logger.LogWriter);
        }
    }
}
