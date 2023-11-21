using SPRNetTool.Domain.Base;
using SPRNetTool.Utils;

namespace SPRNetTool.ViewModel.Base
{
    public abstract class BaseViewModel : BaseNotifier, IDomainObserver, IDomainAccessors
    {
        #region Modules
        private IBitmapDisplayManager? bitmapDisplayManager;
        private ISprWorkManagerCore? sprWorkManager;

        protected IBitmapDisplayManager BitmapDisplayManager
        {
            get
            {
                return bitmapDisplayManager ?? IDomainAccessors
                    .DomainContext
                    .GetDomain<IBitmapDisplayManager>()
                    .Also(it => bitmapDisplayManager = it);
            }
        }

        protected ISprWorkManagerCore SprWorkManager
        {
            get
            {
                return sprWorkManager ??
                    IDomainAccessors
                    .DomainContext
                    .GetDomain<ISprWorkManagerCore>()
                    .Also(it => sprWorkManager = it);
            }
        }
        #endregion
        void IDomainObserver.OnDomainChanged(IDomainChangedArgs args)
        {
            OnDomainChanged(args);
        }


        protected virtual void OnDomainChanged(IDomainChangedArgs args) { }



    }
}
