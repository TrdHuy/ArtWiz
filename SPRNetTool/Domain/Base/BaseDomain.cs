﻿using System.Collections.Generic;

namespace ArtWiz.Domain.Base
{
    public abstract class BaseDomain : IObservableDomain, IDomainAdapter, IDomainAccessors
    {
        private List<IDomainObserver> domainObservers = new List<IDomainObserver>();

        List<IDomainObserver> IObservableDomain.GetDomainObserver()
        {
            return domainObservers;
        }

        protected virtual void NotifyChanged(IDomainChangedArgs args)
        {
            foreach (var domainObserver in domainObservers)
            {
                domainObserver.OnDomainChanged(args);
            }
        }
    }
}
