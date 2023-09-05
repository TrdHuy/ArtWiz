using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRNetTool.Domain.Base
{
    public abstract class BaseDomain : IObservableDomain, IDomainAdapter
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
