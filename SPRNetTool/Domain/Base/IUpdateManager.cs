using ArtWiz.Data.Domain.UpdateManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtWiz.Domain.Base
{
    internal interface IUpdateManager
    {
        Task<UpdateResult> CheckForUpdateAsync();
        Task<string> DownloadAndApplyUpdateAsync(string updateUrl);
    }
}
