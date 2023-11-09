using Moq;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.Utils;
using System.Windows.Media;

namespace SPRNetToolTest.Domain
{
    internal class SprWorkManagerTest
    {
        private string sprFilePath = "Resources\\test.spr";
        private string pngFilePath = "Resources\\test.png";
        private ISprWorkManager sprWorkManager;

        [SetUp]
        public void Setup()
        {
            sprWorkManager = new SprWorkManager();
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void test_SaveBitmapSourceToSprFile()
        {
            string imagePath = pngFilePath.FullPath();
            var bmpSource = sprWorkManager.LoadBitmapFromFile(imagePath);
            Assert.NotNull(bmpSource);
            sprWorkManager.SaveBitmapSourceToSprFile(bmpSource, "Resources\\test_SaveBitmapSourceToSprOutput.spr");
        }

        [Test]
        public void test_InitWorkManagerFromSprFile()
        {
            using (FileStream fs = new FileStream(sprFilePath, FileMode.Open, FileAccess.Read))
            {
                var initResult = sprWorkManager.InitWorkManagerFromSprFile(fs);
                Assert.That(initResult);
                Assert.That(sprWorkManager.FileHead.GlobalHeight * sprWorkManager.FileHead.GlobalWidth == 90000);
            }
        }
    }
}
