﻿namespace ArtWiz.Domain.Base
{
    public interface IDomainChangedArgs { }
    public interface IDomainObserver
    {
        void OnDomainChanged(IDomainChangedArgs args);
    }
}
