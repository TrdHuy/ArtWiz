using Moq;
using ArtWiz.Domain;
using ArtWiz.Domain.Base;
using ArtWiz.Domain.Utils;
using ArtWiz.Utils;
using ArtWizTest.Utils;
using WizMachine;
using WizMachine.Data;

namespace ArtWizTest.Domain
{
    internal class SprWorkManagerTest
    {
        private class SprWorkManagerTestObject : SprWorkManager, ISprWorkManager
        {

            public FrameRGBA[]? GetFrameDataCache()
            {
                FrameRGBA[] cache = new FrameRGBA[sprWorkManagerService.FileHead.FrameCounts];
                for (int i = 0; i < sprWorkManagerService.FileHead.FrameCounts; i++)
                {
                    cache[i] = sprWorkManagerService.GetFrameData((uint)i) ?? throw new Exception();
                }
                return cache;
            }
        }


        private string _sprFilePath = "Resources\\test.spr";
        private string _binFilePath = "Resources\\test.bin";
        private string _pngFilePath = "Resources\\test.png";
        private string _12345sprFilePath = "Resources\\12345.spr";
        private string _alphaFilePath = "Resources\\alpha.spr";
        private string _1binFilePath = "Resources\\1.bin";
        private string _1_319x319binFilePath = "Resources\\1_319x319.bin";
        private string _2binFilePath = "Resources\\2.bin";
        private string _3binFilePath = "Resources\\3.bin";
        private string _4binFilePath = "Resources\\4.bin";
        private string _5binFilePath = "Resources\\5.bin";
        private SprWorkManagerTestObject sprWorkManagerTestObject;
        private ISprWorkManager sprWorkManager;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var mockStreamWriter = new Mock<StreamWriter>("output.txt").Object ?? throw new Exception();
            EngineKeeper.Init(mockStreamWriter);
        }


        [SetUp]
        public void Setup()
        {
            sprWorkManagerTestObject = new SprWorkManagerTestObject();
            sprWorkManager = (ISprWorkManager)sprWorkManagerTestObject;
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void test_SaveCurrentWorkToSpr()
        {
            using (FileStream fs = new FileStream(_12345sprFilePath, FileMode.Open, FileAccess.Read))
            {
                var initResult = sprWorkManager.InitWorkManagerFromSprFile(fs);
                Assert.That(initResult);
                var frameRGBAs = sprWorkManagerTestObject.GetFrameDataCache();
                Assert.NotNull(frameRGBAs);
                Assert.That(frameRGBAs[0].modifiedFrameRGBACache.frameWidth == 300);
                Assert.That(frameRGBAs[0].modifiedFrameRGBACache.frameHeight == 300);
                // Change frameSize 
                frameRGBAs[0].modifiedFrameRGBACache.frameWidth = 319;
                frameRGBAs[0]!.modifiedFrameRGBACache.frameHeight = 319;

                sprWorkManager.SaveCurrentWorkToSpr("Resources\\test_SaveCurrentWorkToSpr.spr", true);
            }

            using (FileStream fs = new FileStream("Resources\\test_SaveCurrentWorkToSpr.spr", FileMode.Open, FileAccess.Read))
            {
                var initResult = sprWorkManager.InitWorkManagerFromSprFile(fs);
                Assert.That(initResult);

                var frameData1Byte = sprWorkManagerTestObject.GetFrameDataCache()![0].originDecodedBGRAData;
                var frameData1FromFile = TestUtil.ReadBytesFromFile(_1_319x319binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData1Byte, frameData1FromFile!));
            }
        }

        [Test]
        public void test_SaveBitmapSourceToSprFile()
        {
            string imagePath = _pngFilePath.FullPath();
            var bmpSource = sprWorkManager.LoadBitmapFromFile(imagePath);
            Assert.NotNull(bmpSource);
            sprWorkManager.SaveBitmapSourceToSprFile(bmpSource, "Resources\\test_SaveBitmapSourceToSprOutput.spr");

            using (FileStream fs = new FileStream("Resources\\test_SaveBitmapSourceToSprOutput.spr", FileMode.Open, FileAccess.Read))
            {
                var initResult = sprWorkManager.InitWorkManagerFromSprFile(fs);
                Assert.That(initResult);
                Assert.That(sprWorkManager.FileHead.GlobalHeight * sprWorkManager.FileHead.GlobalWidth == 90000);

                var frameData1Byte = sprWorkManagerTestObject.GetFrameDataCache()![0].originDecodedBGRAData;
                var frameData1FromFile = TestUtil.ReadBytesFromFile(_binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData1Byte, frameData1FromFile!));
            }
        }

        [Test]
        public void test_SaveCurrentWorkToSpr_AlphaFile()
        {
            var cacheFrameData = new byte[0];
            using (FileStream fs = new FileStream(_alphaFilePath, FileMode.Open, FileAccess.Read))
            {
                var initResult = sprWorkManager.InitWorkManagerFromSprFile(fs);
                Assert.That(initResult);
                var frameRGBAs = sprWorkManagerTestObject.GetFrameDataCache();
                Assert.NotNull(frameRGBAs);
                Assert.NotNull(frameRGBAs[0].originDecodedBGRAData);
                cacheFrameData = new byte[frameRGBAs[0].originDecodedBGRAData.Length];
                Array.Copy(frameRGBAs[0].originDecodedBGRAData,
                    cacheFrameData,
                    frameRGBAs[0].originDecodedBGRAData.Length);

                sprWorkManager.SaveCurrentWorkToSpr("Resources\\test_SaveCurrentWorkToSpr_AlphaFile.spr", true);
            }

            using (FileStream fs = new FileStream("Resources\\test_SaveCurrentWorkToSpr_AlphaFile.spr", FileMode.Open, FileAccess.Read))
            {
                var initResult = sprWorkManager.InitWorkManagerFromSprFile(fs);
                Assert.That(initResult);

                Assert.That(TestUtil.AreByteArraysEqual(cacheFrameData, sprWorkManagerTestObject
                    .GetFrameDataCache()![0].originDecodedBGRAData));
            }
        }

        [Test]
        public void test_InitWorkManagerFromSprFile_file12345()
        {
            using (FileStream fs = new FileStream(_12345sprFilePath, FileMode.Open, FileAccess.Read))
            {
                var initResult = sprWorkManager.InitWorkManagerFromSprFile(fs);
                Assert.That(initResult);
                Assert.That(sprWorkManager.FileHead.GlobalHeight * sprWorkManager.FileHead.GlobalWidth == 90000);


                var frameData1Byte = sprWorkManagerTestObject.GetFrameDataCache()![0].originDecodedBGRAData;
                var frameData1FromFile = TestUtil.ReadBytesFromFile(_1binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData1Byte, frameData1FromFile!));


                var frameData2Byte = sprWorkManagerTestObject.GetFrameDataCache()![1].originDecodedBGRAData;
                var frameData2FromFile = TestUtil.ReadBytesFromFile(_2binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData2Byte, frameData2FromFile!));


                var frameData3Byte = sprWorkManagerTestObject.GetFrameDataCache()![2].originDecodedBGRAData;
                var frameData3FromFile = TestUtil.ReadBytesFromFile(_3binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData3Byte, frameData3FromFile!));


                var frameData4Byte = sprWorkManagerTestObject.GetFrameDataCache()![3].originDecodedBGRAData;
                var frameData4FromFile = TestUtil.ReadBytesFromFile(_4binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData4Byte, frameData4FromFile!));


                var frameData5Byte = sprWorkManagerTestObject.GetFrameDataCache()![4].originDecodedBGRAData;
                var frameData5FromFile = TestUtil.ReadBytesFromFile(_5binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData5Byte, frameData5FromFile!));
            }
        }

        [Test]
        public void test_InitWorkManagerFromSprFile()
        {
            using (FileStream fs = new FileStream(_sprFilePath, FileMode.Open, FileAccess.Read))
            {
                var initResult = sprWorkManager.InitWorkManagerFromSprFile(fs);
                Assert.That(initResult);
                Assert.That(sprWorkManager.FileHead.GlobalHeight * sprWorkManager.FileHead.GlobalWidth == 90000);
            }
        }
    }
}
