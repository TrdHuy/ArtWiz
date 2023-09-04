using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRNetTool.Domain.Base
{
    public interface IDomainChangedArgs { }
    public interface IDomainObserver
    {
        void OnDomainChanged(IDomainChangedArgs args);
    }
}
