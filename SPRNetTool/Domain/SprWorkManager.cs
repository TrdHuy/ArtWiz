using SPRNetTool.Domain.Base;
using WizMachine;

namespace SPRNetTool.Domain
{
    public class SprWorkManager : BaseDomain, ISprWorkManager
    {
        protected WizMachine.Services.Base.ISprWorkManagerAdvance sprWorkManagerService;

        WizMachine.Services.Base.ISprWorkManagerAdvance
            ISprWorkManager.SprWorkManagerService => sprWorkManagerService;

        public SprWorkManager()
        {
            sprWorkManagerService = EngineKeeper.GetSprWorkManagerService();
        }
    }
}
