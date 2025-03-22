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

            string mockUrl = "https://raw.githubusercontent.com/Dezone99/ArtWiz-VersionHub/refs/heads/main/versions.json";
            string mockJson = @"{
  ""latestVersion"": ""1.1.1.1"",
  ""downloadUrl"": ""https://artwiz.com/download"",
  ""releaseNotes"": ""Fix bugs and improve performance.""
}";

            _mockHttp.When(mockUrl)
                 .Respond(req => new HttpResponseMessage
                 {
                     StatusCode = HttpStatusCode.OK,
                     Content = new StringContent(mockJson, Encoding.UTF8, "application/json")
                 });
            var customUpdateManager = new UpdateManagerWithMockMockHttpMessageHandler(_mockHttp);
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
            string mockUrl = "https://raw.githubusercontent.com/Dezone99/ArtWiz-VersionHub/refs/heads/main/versions.json";
            string mockJson = @"{
  ""2.x"": [
    {
      ""version"": ""2.1.0.9"",
      ""downloadUrl"": [
        ""https://github.com/TrdHuy/ArtWiz/releases/download/product_v1.0.0.9/ArtWiz.rar""
      ],
      ""releaseNotes"": ""First release.""
    }
  ]
}";
            _mockHttp.When(mockUrl)
                 .Respond(req => new HttpResponseMessage
                 {
                     StatusCode = HttpStatusCode.OK,
                     Content = new StringContent(mockJson, Encoding.UTF8, "application/json")
                 });

            var customUpdateManager = new UpdateManagerWithMockMockHttpMessageHandler(_mockHttp);
            var result = await customUpdateManager.CheckForUpdateAsync();
            Assert.AreEqual(UpdateResultErrorCode.BRANCH_NOT_FOUND, result.ErrorCode);
        }

        [Test]
        public async Task Test_NoUpdateAvailable()
        {

            string mockUrl = "https://raw.githubusercontent.com/Dezone99/ArtWiz-VersionHub/refs/heads/main/versions.json";
            string mockJson = @"{
  ""1.x"": [
    {
      ""version"": ""1.5.3.0"",
      ""downloadUrl"": [
        ""https://github.com/TrdHuy/ArtWiz/releases/download/product_v1.0.0.9/ArtWiz.rar""
      ],
      ""releaseNotes"": ""First release.""
    }
  ]
}";
            _mockHttp.When(mockUrl)
                 .Respond(req => new HttpResponseMessage
                 {
                     StatusCode = HttpStatusCode.OK,
                     Content = new StringContent(mockJson, Encoding.UTF8, "application/json")
                 });

            var customUpdateManager = new UpdateManagerWithMockMockHttpMessageHandler(_mockHttp);
            customUpdateManager.MockCurrentVersion = "1.5.3.0";
            var result = await customUpdateManager.CheckForUpdateAsync();
            Assert.IsFalse(result.IsNeedToUpdate);
        }

        [Test]
        public async Task Test_UpdateAvailableButNotForced()
        {
            string mockUrl = "https://raw.githubusercontent.com/Dezone99/ArtWiz-VersionHub/refs/heads/main/versions.json";
            string mockJson = @"{
  ""1.x"": [
    {
      ""version"": ""1.5.3.1"",
      ""downloadUrl"": [
        ""https://github.com/TrdHuy/ArtWiz/releases/download/product_v1.0.0.9/ArtWiz.rar""
      ],
      ""releaseNotes"": ""First release.""
    }
  ]
}";

            _mockHttp.When(mockUrl)
                 .Respond(req => new HttpResponseMessage
                 {
                     StatusCode = HttpStatusCode.OK,
                     Content = new StringContent(mockJson, Encoding.UTF8, "application/json")
                 });
            var customUpdateManager = new UpdateManagerWithMockMockHttpMessageHandler(_mockHttp);
            customUpdateManager.MockCurrentVersion = "1.5.3.0";
            var result = await customUpdateManager.CheckForUpdateAsync();
            Assert.IsTrue(result.IsNeedToUpdate);
            Assert.IsFalse(result.NeedToForceUpdate);
        }


        [Test]
        public async Task Test_ForcedUpdateAvailable()
        {
            string mockUrl = "https://raw.githubusercontent.com/Dezone99/ArtWiz-VersionHub/refs/heads/main/versions.json";
            string mockJson = @"{
  ""1.x"": [
    {
      ""version"": ""1.5.4.0"",
      ""downloadUrl"": [
        ""https://github.com/TrdHuy/ArtWiz/releases/download/product_v1.0.0.9/ArtWiz.rar""
      ],
      ""releaseNotes"": ""First release.""
    }
  ]
}";
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

    public class UpdateManagerWithMockMockHttpMessageHandler : UpdateManager
    {
        public string? MockCurrentVersion { get; set; } = null;
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

        protected override string GetCurrentVersion()
        {
            if (MockCurrentVersion != null)
            {
                return MockCurrentVersion;
            }
            return base.GetCurrentVersion();
        }

        protected override void StartUpdater(string zipFilePath, string installPath)
        {
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
