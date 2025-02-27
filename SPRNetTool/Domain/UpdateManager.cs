using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ArtWiz.Data.Domain.UpdateManager;
using ArtWiz.Domain.Base;
using ArtWiz.LogUtil;

namespace ArtWiz.Domain
{
    public class UpdateManager : BaseDomain, IUpdateManager
    {
        private static Logger logger = new Logger(nameof(UpdateManager));
        private const string UpdateInfoUrl = "https://raw.githubusercontent.com/Dezone99/ArtWiz-VersionHub/refs/heads/main/latest-version.json";

        public UpdateManager()
        {
        }

        public async Task<UpdateResult> CheckForUpdateAsync()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    string jsonData = await GetStringFromHttpUrl(UpdateInfoUrl, client);
                    Dictionary<string, UpdateInfo>? versionData;
                    try
                    {
                        versionData = JsonSerializer.Deserialize<Dictionary<string, UpdateInfo>>(jsonData);
                    }
                    catch (JsonException ex)
                    {
                        logger.E($"Invalid JSON format: {ex.Message}");
                        versionData = null;
                    }

                    if (versionData == null)
                        return UpdateResult.Error(UpdateResultErrorCode.FAILED_TO_GET_VERSION_DATA_FROM_SERVER);

                    string currentVersion = GetCurrentVersion();
                    string branch = GetVersionBranch(currentVersion);

                    if (!versionData.ContainsKey(branch))
                    {
                        logger.E($"No update information found for branch: {branch}");
                        return UpdateResult.Error(UpdateResultErrorCode.BRANCH_NOT_FOUND);
                    }

                    var updateInfo = versionData[branch];
                    string latestVersion = updateInfo?.LatestVersion ?? "";

                    bool isNewVersion = IsNewVersion(latestVersion, currentVersion);
                    bool needToForceUpdate = IsMajorMinorPatchChanged(latestVersion, currentVersion);

                    return new UpdateResult(isNeedToUpdate: isNewVersion,
                             needToForceUpdate: needToForceUpdate,
                             downloadUrl: updateInfo?.DownloadUrl ?? "",
                             releaseNotes: updateInfo?.ReleaseNotes ?? "",
                             latestVersion: latestVersion);
                }
            }
            catch (Exception ex)
            {
                logger.E($"Error checking for update: {ex.Message}");
                return UpdateResult.Error(UpdateResultErrorCode.UNKNOWN_EXCEPTION);
            }
        }

        public async Task<string> DownloadAndApplyUpdateAsync(string downloadUrl)
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), "Updater");
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

                return tempFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download failed: {ex.Message}");
                return string.Empty;
            }
        }

        //public async Task DownloadAndApplyUpdateAsync(string updateUrl)
        //{
        //    try
        //    {
        //        string tempFilePath = "update.zip";

        //        using (var client = new HttpClient())
        //        {
        //            using (var response = await client.GetAsync(updateUrl))
        //            using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        //            {
        //                await response.Content.CopyToAsync(fileStream);
        //            }

        //            string extractPath = "update_temp";
        //            if (Directory.Exists(extractPath))
        //                Directory.Delete(extractPath, true);

        //            System.IO.Compression.ZipFile.ExtractToDirectory(tempFilePath, extractPath);

        //            foreach (string file in Directory.GetFiles(extractPath))
        //            {
        //                string destFile = Path.Combine(".", Path.GetFileName(file));
        //                File.Move(file, destFile, true);
        //            }

        //            Directory.Delete(extractPath, true);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Update failed: {ex.Message}");
        //    }
        //}

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
            [JsonPropertyName("latestVersion")]
            public string? LatestVersion { get; set; }

            [JsonPropertyName("downloadUrl")]
            public string? DownloadUrl { get; set; }

            [JsonPropertyName("releaseNotes")]
            public string? ReleaseNotes { get; set; }
        }
    }
}
