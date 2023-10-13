using SPRNetTool.Data;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.Utils;
using System.Windows.Media;
using static SPRNetTool.Utils.BitmapUtil;
namespace SPRNetToolTest.Utils
{
    public class BitmapUtilTest
    {
        [SetUp]
        public void Setup()
        {
        }

        struct MS
        {
            public int x;
            public int y;
        }

        public enum Variable
        {
            NUM1 = 0b00000001,
            NUM2 = 0b00000010,
            NUM3 = 0b00000100,
            NUM4 = 0b00001000,
        }
        [Test]
        public void test_()
        {
            var frame = new FrameRGBA();
            frame.modifiedFrameRGBACache = new FrameRGBA.FrameRGBACache();
            frame.modifiedFrameRGBACache.frameRGBA.frameOffX = 192;
            frame.decodedFrameData = new PaletteColor[2];
            FrameRGBA frame2;

            var frame3 = frame;
            frame3.frameOffX = 19;
            frame3.decodedFrameData[0].Alpha = 100;
            frame3.modifiedFrameRGBACache.frameRGBA.frameOffX = 19;
            var x = 1;
        }

        [Test]
        public void test_SaveBitmapSourceToSprFile()
        {
            string imagePath = "Resources\\test.png".FullPath();
            var bmpSource = LoadBitmapFromFile(imagePath);
            Assert.NotNull(bmpSource);
            ISprWorkManager swm = new SprWorkManager();
            swm.SaveBitmapSourceToSprFile(bmpSource, "Resources\\test.spr");
        }

        [Test]
        public void test_ConvertBitmapSourceToPaletteColorArray()
        {
            string imagePath = "Resources\\test.png".FullPath();
            var bmpSource = LoadBitmapFromFile(imagePath);
            var bdm = new BitmapDisplayManager();
            Assert.NotNull(bmpSource);
            var bytearray = bdm.ConvertBitmapSourceToByteArray(bmpSource);
            var palarray = bdm.ConvertBitmapSourceToPaletteColorArray(bmpSource);
            var palarrayToByte = bdm.ConvertPaletteColourArrayToByteArray(palarray);
            Assert.That(bdm.AreByteArraysEqual(bytearray, palarrayToByte));
        }

        [Test]
        public void test()
        {
            var n1 = Convert.ToInt64(Variable.NUM1);
            Variable num = Variable.NUM1 | Variable.NUM2 | Variable.NUM3;
            if (num.HasAllFlagsOf(Variable.NUM1, Variable.NUM2))
            {
                var x = 1;
            }

            if (num.HasFlag(Variable.NUM2 | Variable.NUM4))
            {
                var x = 1;
            }

            if (num.HasFlag(Variable.NUM3))
            {
                var x = 1;
            }
            if (num.HasFlag(Variable.NUM4))
            {
                var x = 1;
            }
        }

        [Test]
        public void TestLoadBitmapSourceFromFile()
        {
            string imagePath = "Resources\\test.png".FullPath();
            var bmpSource = LoadBitmapFromFile(imagePath);
            Assert.NotNull(bmpSource);
            Assert.That(bmpSource.PixelWidth * bmpSource.PixelHeight, Is.EqualTo(90000));
        }

        [Test]
        public async Task TestCountColors()
        {
            string imagePath = "Resources\\test.png".FullPath();
            var bmpSource = LoadBitmapFromFile(imagePath);
            Assert.NotNull(bmpSource);
            var src = await CountColorsAsync(bmpSource);
            Assert.That(src.Count, Is.EqualTo(4));

            var redColor = Color.FromArgb(255, 237, 28, 36);
            var yellowColor = Color.FromArgb(255, 255, 242, 0);
            var greenColor = Color.FromArgb(255, 34, 177, 76);
            var blueColor = Color.FromArgb(255, 0, 162, 232);

            Assert.That(src[redColor], Is.EqualTo(21456));
            Assert.That(src[yellowColor], Is.EqualTo(23999));
            Assert.That(src[greenColor], Is.EqualTo(22499));
            Assert.That(src[blueColor], Is.EqualTo(22046));
        }
    }
}