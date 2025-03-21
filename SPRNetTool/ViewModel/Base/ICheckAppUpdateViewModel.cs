using ArtWiz.Data.Domain.UpdateManager;
using ArtWiz.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtWiz.ViewModel.Base
{
    internal interface ICheckAppUpdateViewModel : IDomainAccessors
    {
        async Task<UpdateResult> CheckAppUpdateAsync()
        {
            var updateManager = DomainContext.GetDomain<IUpdateManager>();
            return await updateManager.CheckForUpdateAsync();
        }
    }
}
