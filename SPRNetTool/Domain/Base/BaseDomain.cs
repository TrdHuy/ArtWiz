using System.Collections.Generic;

namespace SPRNetTool.Domain.Base
{
    public abstract class BaseDomain : IObservableDomain, IDomainAdapter, IDomainAccessors
    {
        private List<IDomainObserver> domainObservers = new List<IDomainObserver>();

        List<IDomainObserver> IObservableDomain.GetDomainObserver()
        {
            return domainObservers;
        }

        protected void NotifyChanged(IDomainChangedArgs args)
        {
            foreach (var domainObserver in domainObservers)
            {
                domainObserver.OnDomainChanged(args);
            }
        }
    }
}
