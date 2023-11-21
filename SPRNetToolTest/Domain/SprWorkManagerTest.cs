using SPRNetTool.Data;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.Utils;
using SPRNetToolTest.Utils;

namespace SPRNetToolTest.Domain
{
    internal class SprWorkManagerTest
    {
        private class SprWorkManagerTestObject : SprWorkManagerCore
        {
            public long GetFrameDataBegPosCache()
            {
                return FrameDataBegPos;
            }

            public FrameRGBA[]? GetFrameDataCache()
            {
                return FrameData;
            }
        }


        private string _sprFilePath = "Resources\\test.spr";
        private string _binFilePath = "Resources\\test.bin";
        private string _pngFilePath = "Resources\\test.png";
        private string _12345sprFilePath = "Resources\\12345.spr";
        private string _1binFilePath = "Resources\\1.bin";
        private string _1_319x319binFilePath = "Resources\\1_319x319.bin";
        private string _2binFilePath = "Resources\\2.bin";
        private string _3binFilePath = "Resources\\3.bin";
        private string _4binFilePath = "Resources\\4.bin";
        private string _5binFilePath = "Resources\\5.bin";
        private ISprWorkManagerCore sprWorkManager;
        private SprWorkManagerTestObject sprWorkManagerTestObject;

        [SetUp]
        public void Setup()
        {
            sprWorkManager = new SprWorkManagerTestObject();
            sprWorkManagerTestObject = (SprWorkManagerTestObject)sprWorkManager;
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
                frameRGBAs[0].modifiedFrameRGBACache.frameHeight = 319;

                sprWorkManager.SaveCurrentWorkToSpr("Resources\\test_SaveCurrentWorkToSpr.spr", true);
            }

            using (FileStream fs = new FileStream("Resources\\test_SaveCurrentWorkToSpr.spr", FileMode.Open, FileAccess.Read))
            {
                var initResult = sprWorkManager.InitWorkManagerFromSprFile(fs);
                Assert.That(initResult);

                var frameData1Byte = TestUtil.ConvertPaletteColorArrayToByteArray(
                    sprWorkManagerTestObject.GetFrameDataCache()![0].originDecodedFrameData);
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

                var frameData1Byte = TestUtil.ConvertPaletteColorArrayToByteArray(
                   sprWorkManagerTestObject.GetFrameDataCache()![0].originDecodedFrameData);
                var frameData1FromFile = TestUtil.ReadBytesFromFile(_binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData1Byte, frameData1FromFile!));
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


                var frameData1Byte = TestUtil.ConvertPaletteColorArrayToByteArray(
                    sprWorkManagerTestObject.GetFrameDataCache()![0].originDecodedFrameData);
                var frameData1FromFile = TestUtil.ReadBytesFromFile(_1binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData1Byte, frameData1FromFile!));


                var frameData2Byte = TestUtil.ConvertPaletteColorArrayToByteArray(
                    sprWorkManagerTestObject.GetFrameDataCache()![1].originDecodedFrameData);
                var frameData2FromFile = TestUtil.ReadBytesFromFile(_2binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData2Byte, frameData2FromFile!));


                var frameData3Byte = TestUtil.ConvertPaletteColorArrayToByteArray(
                    sprWorkManagerTestObject.GetFrameDataCache()![2].originDecodedFrameData);
                var frameData3FromFile = TestUtil.ReadBytesFromFile(_3binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData3Byte, frameData3FromFile!));


                var frameData4Byte = TestUtil.ConvertPaletteColorArrayToByteArray(
                    sprWorkManagerTestObject.GetFrameDataCache()![3].originDecodedFrameData);
                var frameData4FromFile = TestUtil.ReadBytesFromFile(_4binFilePath);
                Assert.That(TestUtil.AreByteArraysEqual(frameData4Byte, frameData4FromFile!));


                var frameData5Byte = TestUtil.ConvertPaletteColorArrayToByteArray(
                    sprWorkManagerTestObject.GetFrameDataCache()![4].originDecodedFrameData);
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
