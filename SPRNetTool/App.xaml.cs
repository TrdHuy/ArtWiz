using SPRNetTool.LogUtil;
using System.Windows;
using WizMachine;

namespace SPRNetTool
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
