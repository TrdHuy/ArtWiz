using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ArtWiz.Data;
using ArtWiz.Data.Domain.UpdateManager;
using ArtWiz.Domain.Base;
using ArtWiz.LogUtil;

namespace ArtWiz.Domain
{
    public class UpdateManager : BaseDomain, IUpdateManager
    {
        private static Logger mLogger = new Logger(nameof(UpdateManager));
        private const string mUpdateInfoUrl = "https://raw.githubusercontent.com/Dezone99/ArtWiz-VersionHub/refs/heads/main/versions.json";
        private static readonly Cache<UpdateResult?> mUpdateCache = new Cache<UpdateResult?>(TimeSpan.FromMinutes(10));
        private static readonly SemaphoreSlim mUpdateLock = new SemaphoreSlim(1, 1);

        public UpdateManager()
        {
        }

        public async Task<UpdateResult> CheckForUpdateAsync()
        {
            if (mUpdateCache.IsValid)
            {
                mLogger.I("Using cached update check result.");
                return mUpdateCache.Value!;
            }

            // Chỉ cho phép một luồng chạy
            await mUpdateLock.WaitAsync();
            try
            {
                if (mUpdateCache.IsValid)
                {
                    mLogger.I("Using cached update check result (after waiting).");
                    return mUpdateCache.Value!;
                }

                mLogger.I("Checking for update...");

                using (var client = GetHttpClient())
                {
                    await Task.Delay(5000);
                    //string jsonData = await GetStringFromHttpUrl(mUpdateInfoUrl, client);
                    string jsonData = @"{
  ""1.x"": [
    {
      ""version"": ""1.0.8.2"",
      ""releaseNotes"": ""Release new version"",
      ""downloadUrl"": [
""https://github.com/TrdHuy/ArtWiz/releases/download/product_v1.0.0.9/ArtWiz.rar""
]
    },
    {
      ""version"": ""1.0.0.9"",
      ""releaseNotes"": ""First release."",
      ""downloadUrl"": [
        ""https://github.com/TrdHuy/ArtWiz/releases/download/product_v1.0.0.9/ArtWiz.rar""
      ]
    }
  ]
}";
                    Dictionary<string, List<UpdateInfo>>? versionData;

                    try
                    {
                        versionData = JsonSerializer.Deserialize<Dictionary<string, List<UpdateInfo>>>(jsonData);
                    }
                    catch (JsonException ex)
                    {
                        mLogger.E($"Invalid JSON format: {ex.Message}");
                        return UpdateResult.Error(UpdateResultErrorCode.FAILED_TO_GET_VERSION_DATA_FROM_SERVER);
                    }

                    if (versionData == null || versionData.Count == 0)
                        return UpdateResult.Error(UpdateResultErrorCode.FAILED_TO_GET_VERSION_DATA_FROM_SERVER);

                    string currentVersion = GetCurrentVersion();
                    string branch = GetVersionBranch(currentVersion);

                    if (!versionData.ContainsKey(branch))
                    {
                        mLogger.E($"No update information found for branch: {branch}");
                        return UpdateResult.Error(UpdateResultErrorCode.BRANCH_NOT_FOUND);
                    }

                    var branchVersions = versionData[branch];

                    var latestUpdate = branchVersions
                        .OrderByDescending(v => Version.Parse(v.Version))
                        .FirstOrDefault();

                    if (latestUpdate == null)
                        return UpdateResult.Error(UpdateResultErrorCode.NO_UPDATE_AVAILABLE);

                    string latestVersion = latestUpdate.Version;

                    bool isNewVersion = IsNewVersion(latestVersion, currentVersion);
                    bool needToForceUpdate = IsMajorMinorPatchChanged(latestVersion, currentVersion);

                    mUpdateCache.Value = new UpdateResult(isNeedToUpdate: isNewVersion,
                        needToForceUpdate: needToForceUpdate,
                        downloadUrl: latestUpdate.DownloadUrl.FirstOrDefault() ?? "",
                        releaseNotes: latestUpdate.ReleaseNotes,
                        latestVersion: latestVersion);

                    return mUpdateCache.Value;
                }
            }
            catch (Exception ex)
            {
                mLogger.E($"Error checking for update: {ex.Message}");
                return UpdateResult.Error(UpdateResultErrorCode.UNKNOWN_EXCEPTION);
            }
            finally
            {
                mUpdateLock.Release();
            }
        }
        public async Task<string> DownloadAndApplyUpdateAsync(string downloadUrl)
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), "Updater");
            var installPath = AppDomain.CurrentDomain.BaseDirectory;

            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            string tempFilePath = Path.Combine(tempDirectory, "update.zip");

            try
            {
                using (var client = GetHttpClient())
                {
                    using (var response = await client.GetAsync(downloadUrl))
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }

                StartUpdater(tempFilePath, installPath);
                return tempFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download failed: {ex.Message}");
                return string.Empty;
            }
        }


        public void StartUpdater(string zipFilePath, string installPath)
        {
            string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ArtUpdater.exe");

            if (!File.Exists(updaterPath))
            {
                Console.WriteLine("Updater.exe not found!");
                return;
            }
            var prioritySearchingProcess = "ArtWiz";
            // Đảm bảo đường dẫn có dấu cách được xử lý đúng
            installPath = installPath.TrimEnd('\\'); // Xóa dấu \ ở cuối nếu có
            string arguments = $"\"{zipFilePath}\" \"{installPath}\" \"{prioritySearchingProcess}\"";


            Process.Start(new ProcessStartInfo
            {
                FileName = updaterPath,
                Arguments = arguments,
                UseShellExecute = false
            });

            // Thoát ứng dụng chính để Updater có thể ghi đè file
            Environment.Exit(0);
        }




        protected virtual async Task<string> GetStringFromHttpUrl(string url, HttpClient client)
        {
            return await client.GetStringAsync(url);
        }

        protected virtual HttpClient GetHttpClient()
        {
            return new HttpClient();
        }

        protected virtual string GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? throw new Exception("Failed to get current assembly version!");
        }

        private string GetVersionBranch(string version)
        {
            var parts = version.Split('.');
            return parts[0] + ".x"; // Tạo branch như "1.x" hoặc "2.x"
        }

        private bool IsNewVersion(string latest, string current)
        {
            return string.Compare(latest, current) > 0;
        }



        private bool IsMajorMinorPatchChanged(string latest, string current)
        {
            try
            {
                Version latestVersion = new Version(latest);
                Version currentVersion = new Version(current);

                // Bắt buộc cập nhật nếu Major, Minor hoặc Patch thay đổi (x.y.z)
                return latestVersion.Major > currentVersion.Major ||
                       latestVersion.Minor > currentVersion.Minor ||
                       latestVersion.Build > currentVersion.Build;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing versions: {ex.Message}");
                return false;
            }
        }

        public class UpdateInfo
        {
            [JsonPropertyName("version")]
            public string Version { get; set; }

            [JsonPropertyName("downloadUrl")]
            public List<string> DownloadUrl { get; set; }

            [JsonPropertyName("releaseNotes")]
            public string ReleaseNotes { get; set; }
        }
    }
}
