using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ArtWiz.Domain;
using ArtWiz.Domain.Base;
using ArtWiz.Data.Domain.UpdateManager;
using static ArtWiz.Domain.UpdateManager;

namespace ArtWizTest.Domain
{
    [TestFixture]
    public class UpdateManagerTest
    {
        private UpdateManager _updateManager;

        [SetUp]
        public void Setup()
        {
            _updateManager = new UpdateManager();
        }

        #region CheckForUpdateAsync test
        [Test]
        public async Task Test_NullVersionData()
        {
            var customUpdateManager = new UpdateManagerWithMockHttp(@"{
  ""latestVersion"": ""1.1.1.1"",
  ""downloadUrl"": ""https://artwiz.com/download"",
  ""releaseNotes"": ""Fix bugs and improve performance.""
}", "1.5.3.0");
            var result = await customUpdateManager.CheckForUpdateAsync();
            Assert.AreEqual(UpdateResultErrorCode.FAILED_TO_GET_VERSION_DATA_FROM_SERVER, result.ErrorCode);
        }


        [Test]
        public async Task Test_FailedToGetVersionDataFromServer()
        {
            var faultyUpdateManager = new UpdateManagerWithFaultyHttp();
            var result = await faultyUpdateManager.CheckForUpdateAsync();
            Assert.AreEqual(UpdateResultErrorCode.UNKNOWN_EXCEPTION, result.ErrorCode);
        }

        [Test]
        public async Task Test_BranchNotFound()
        {
            var jsonData = JsonSerializer.Serialize(new Dictionary<string, UpdateInfo>
            {
                { "2.x", new UpdateInfo { LatestVersion = "2.1.0.0",
                    DownloadUrl = "https://mockupdate.com/update.zip",
                    ReleaseNotes = "New features" } }
            });

            var customUpdateManager = new UpdateManagerWithMockHttp(jsonData, "1.5.3.0");
            var result = await customUpdateManager.CheckForUpdateAsync();
            Assert.AreEqual(UpdateResultErrorCode.BRANCH_NOT_FOUND, result.ErrorCode);
        }

        [Test]
        public async Task Test_NoUpdateAvailable()
        {
            var jsonData = JsonSerializer.Serialize(new Dictionary<string, UpdateInfo>
            {
                { "1.x", new UpdateInfo { LatestVersion = "1.5.3.0", DownloadUrl = "", ReleaseNotes = "" } }
            });

            var customUpdateManager = new UpdateManagerWithMockHttp(jsonData, "1.5.3.0");
            var result = await customUpdateManager.CheckForUpdateAsync();
            Assert.IsFalse(result.IsNeedToUpdate);
        }

        [Test]
        public async Task Test_UpdateAvailableButNotForced()
        {
            var jsonData = JsonSerializer.Serialize(new Dictionary<string, UpdateInfo>
            {
                { "1.x", new UpdateInfo { LatestVersion = "1.5.3.1", DownloadUrl = "https://mockupdate.com/update.zip", ReleaseNotes = "Bug fix" } }
            });

            var customUpdateManager = new UpdateManagerWithMockHttp(jsonData, "1.5.3.0");
            var result = await customUpdateManager.CheckForUpdateAsync();
            Assert.IsTrue(result.IsNeedToUpdate);
            Assert.IsFalse(result.NeedToForceUpdate);
        }

        [Test]
        public async Task Test_ForcedUpdateAvailable()
        {
            var jsonData = JsonSerializer.Serialize(new Dictionary<string, UpdateInfo>
            {
                { "1.x", new UpdateInfo { LatestVersion = "1.5.4.0", DownloadUrl = "https://mockupdate.com/update.zip", ReleaseNotes = "New features" } }
            });

            var customUpdateManager = new UpdateManagerWithMockHttp(jsonData, "1.5.3.0");
            var result = await customUpdateManager.CheckForUpdateAsync();
            Assert.IsTrue(result.IsNeedToUpdate);
            Assert.IsTrue(result.NeedToForceUpdate);
        }

        #endregion
    }

    public class UpdateManagerWithMockHttp : UpdateManager
    {
        private readonly string _mockJson;
        private readonly string _mockVersion;

        public UpdateManagerWithMockHttp(string mockJson, string mockVersion)
        {
            _mockJson = mockJson;
            _mockVersion = mockVersion;
        }

        protected override async Task<string> GetStringFromHttpUrl(string url, HttpClient client)
        {
            return await Task.FromResult(_mockJson);
        }

        protected override string GetCurrentVersion()
        {
            return _mockVersion;
        }
    }

    public class UpdateManagerWithFaultyHttp : UpdateManager
    {
        protected override async Task<string> GetStringFromHttpUrl(string url, HttpClient client)
        {
            throw new Exception("Server error");
        }
    }
}
