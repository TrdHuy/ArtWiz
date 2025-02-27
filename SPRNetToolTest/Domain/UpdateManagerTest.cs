using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ArtWiz.Domain;
using ArtWiz.Domain.Base;
using ArtWiz.Data.Domain.UpdateManager;
using static ArtWiz.Domain.UpdateManager;
using RichardSzalay.MockHttp;
using System.Net.Http;
using System.Text;
using System.Net;

namespace ArtWizTest.Domain
{
    [TestFixture]
    public class UpdateManagerTest
    {
        private UpdateManager _updateManager;
        private MockHttpMessageHandler _mockHttp;
        [SetUp]
        public void Setup()
        {
            _updateManager = new UpdateManager();
            _mockHttp = new MockHttpMessageHandler();
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
            string mockUrl = "https://raw.githubusercontent.com/Dezone99/ArtWiz-VersionHub/refs/heads/main/latest-version.json";
            string mockJson = JsonSerializer.Serialize(new Dictionary<string, UpdateInfo>
            {
                { "1.x", new UpdateInfo { LatestVersion = "1.5.4.0", DownloadUrl = "https://mockupdate.com/update.zip", ReleaseNotes = "New features" } }
            });
            // Cấu hình mock server trả về JSON string
            _mockHttp.When(mockUrl)
                     .Respond(req => new HttpResponseMessage
                     {
                         StatusCode = HttpStatusCode.OK,
                         Content = new StringContent(mockJson, Encoding.UTF8, "application/json")
                     });

            var customUpdateManager = new UpdateManagerWithMockMockHttpMessageHandler(_mockHttp);
            var result = await customUpdateManager.CheckForUpdateAsync();
            Assert.IsTrue(result.IsNeedToUpdate);
            Assert.IsTrue(result.NeedToForceUpdate);
        }

        #endregion

        [Test]
        public async Task Test_DownloadUpdate_FromMockHttpServer()
        {
            string mockUrl = "http://mockserver.com/update.zip";

            string localZipPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources\\ArtWiz.zip");


            // Đọc dữ liệu file ZIP
            byte[] zipBytes = File.ReadAllBytes(localZipPath);

            // Cấu hình mock HTTP server trả về file ZIP từ local
            _mockHttp.When(mockUrl)
                     .Respond("application/zip", new MemoryStream(zipBytes));

            var customUpdateManager = new UpdateManagerWithMockMockHttpMessageHandler(_mockHttp);

            // Gọi phương thức download
            string savePath = await customUpdateManager.DownloadAndApplyUpdateAsync(mockUrl);

            // Kiểm tra xem file có được lưu đúng không
            Assert.IsTrue(File.Exists(savePath));
        }
    }

    public class UpdateManagerWithMockMockHttpMessageHandler: UpdateManager
    {
        private MockHttpMessageHandler _mockHttpMessageHandler;
        public UpdateManagerWithMockMockHttpMessageHandler(MockHttpMessageHandler mockHttpMessageHandler)
        {
            _mockHttpMessageHandler = mockHttpMessageHandler;
        }

        protected override HttpClient GetHttpClient()
        {
            if (_mockHttpMessageHandler != null)
                return new HttpClient(_mockHttpMessageHandler);
            else
                return base.GetHttpClient();
        }
    }
    public class UpdateManagerWithMockHttp : UpdateManager
    {
        private readonly string _mockJson;
        private readonly string _mockVersion;
        private MockHttpMessageHandler? _mockHttpMessageHandler;
        public UpdateManagerWithMockHttp(string mockJson, string mockVersion, MockHttpMessageHandler? mockHttpMessageHandler = null)
        {
            _mockJson = mockJson;
            _mockVersion = mockVersion;
            _mockHttpMessageHandler = mockHttpMessageHandler;
        }

        protected override async Task<string> GetStringFromHttpUrl(string url, HttpClient client)
        {
            return await Task.FromResult(_mockJson);
        }

        protected override string GetCurrentVersion()
        {
            return _mockVersion;
        }

        protected override HttpClient GetHttpClient()
        {
            if (_mockHttpMessageHandler != null)
                return new HttpClient(_mockHttpMessageHandler);
            else
                return base.GetHttpClient();
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
