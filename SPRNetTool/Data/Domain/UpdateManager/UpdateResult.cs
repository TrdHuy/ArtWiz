using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ArtWiz.Data.Domain.UpdateManager.UpdateResultErrorCode;

namespace ArtWiz.Data.Domain.UpdateManager
{
    public class UpdateResult
    {
        public bool IsNeedToUpdate { get; private set; }
        public bool NeedToForceUpdate { get; private set; }
        public string DownloadUrl { get; private set; } = "";
        public string ReleaseNotes { get; private set; } = "";
        public string LatestVersion { get; private set; } = "";
        public UpdateResultErrorCode ErrorCode { get; private set; } = NONE;


        public static UpdateResult Error(UpdateResultErrorCode code)
        {
            return new UpdateResult
            {
                ErrorCode = code
            };
        }

        public UpdateResult()
        {
            IsNeedToUpdate = false;
            NeedToForceUpdate = false;
            DownloadUrl = "";
            ReleaseNotes = "";
            LatestVersion = "";
        }

        public UpdateResult(bool isNeedToUpdate,
            bool needToForceUpdate,
            string downloadUrl,
            string releaseNotes,
            string latestVersion)
        {
            IsNeedToUpdate = isNeedToUpdate;
            NeedToForceUpdate = needToForceUpdate;
            DownloadUrl = downloadUrl;
            ReleaseNotes = releaseNotes;
            LatestVersion = latestVersion;
        }
    }
}
