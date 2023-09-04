using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRNetTool.Domain.Base
{
    public interface IObservableDomain
    {
        protected List<IDomainObserver> GetDomainObserver();

        void RegisterObserver(IDomainObserver domainObserver)
        {
            var domainObservers = GetDomainObserver();
            if (!domainObservers.Contains(domainObserver))
            {
                domainObservers.Add(domainObserver);
            }
        }

        void UnregisterObserver(IDomainObserver domainObserver)
        {
            var domainObservers = GetDomainObserver();
            if (domainObservers.Contains(domainObserver))
            {
                domainObservers.Remove(domainObserver);
            }
        }
                     
    }
}
