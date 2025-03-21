using ArtWiz.Domain.Base;
using ArtWiz.LogUtil;
using ArtWiz.Utils;
using System.Windows;
using WizMachine;

namespace ArtWiz
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDomainAccessors
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            EngineKeeper.Init(Logger.LogWriter);
            var updateManager = IDomainAccessors.DomainContext.GetDomain<IUpdateManager>();
            updateManager.CheckForUpdateAsync();
        }
    }
}
