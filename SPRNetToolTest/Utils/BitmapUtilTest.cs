using ManagedCuda;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.Utils;
using System.Diagnostics;
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
        public void test_e()
        {
            (byte, byte, byte, byte) getPixel(byte[] imgData, int x, int y, int w, int h)
            {
                if (x < 0 || y < 0) throw new Exception();
                if (x >= w || y >= h) throw new Exception();

                var index = (y * w + x) * 4;
                return (imgData[index], imgData[index + 1], imgData[index + 2], imgData[index + 3]);
            }

            int times = 10;
            Stopwatch stopwatch = new Stopwatch();
            var height = 300;
            var width = 300;
            var dataSize = height * width * 4;

            var intArrTt = 0l;
            var convertArrTt = 0l;
            var intLstTt = 0l;
            var convertLstTt = 0l;
            var intArrCuda = 0l;
            var convertArrCuda = 0l;

            //stopwatch.Restart();
            //var test = stopwatch.ElapsedMilliseconds;
            //stopwatch.Restart();
            //var test2 = stopwatch.ElapsedMilliseconds;
            for (int trial = 0; trial < times; trial++)
            {
                stopwatch.Restart();
                byte[] imgData = new byte[dataSize];
                for (int i = 0; i < dataSize; i++)
                {
                    imgData[i] = 0xFF;
                }
                Debug.WriteLine($"init array: {stopwatch.ElapsedMilliseconds}ms");
                intArrTt += stopwatch.ElapsedMilliseconds;
                var bitmapSource = SPRNetTool.Utils.BitmapUtil.GetBitmapFromRGBArray(imgData, width, height, PixelFormats.Bgra32);
                Debug.WriteLine($"convert bmp src from array: {stopwatch.ElapsedMilliseconds}ms");
                convertArrTt += stopwatch.ElapsedMilliseconds;


                stopwatch.Restart();
                List<byte> imgDataLst = new List<byte>();
                for (int i = 0; i < dataSize; i++)
                {
                    imgDataLst.Insert(imgDataLst.Count / 2, 0xFF);
                }
                Debug.WriteLine($"init list: {stopwatch.ElapsedMilliseconds}ms");
                intLstTt += stopwatch.ElapsedMilliseconds;

                var bitmapSource2 = SPRNetTool.Utils.BitmapUtil.GetBitmapFromRGBArray(imgDataLst.ToArray(), width, height, PixelFormats.Bgra32);
                Debug.WriteLine($"convert bmp src from list: {stopwatch.ElapsedMilliseconds}ms");
                convertLstTt += stopwatch.ElapsedMilliseconds;

                stopwatch.Restart();
                byte[] imgData3 = new byte[dataSize];
                using (CudaContext ctx = new CudaContext())
                using (CudaDeviceVariable<byte> deviceArray = new CudaDeviceVariable<byte>(dataSize))
                {
                    for (int i = 0; i < dataSize; i++)
                    {
                        deviceArray[i] = 0xFF;
                    }

                    deviceArray.CopyToHost(imgData3);
                    Debug.WriteLine($"init arr cuda: {stopwatch.ElapsedMilliseconds}ms");
                    intArrCuda += stopwatch.ElapsedMilliseconds;
                    var bitmapSource3 = SPRNetTool.Utils.BitmapUtil.GetBitmapFromRGBArray(imgData3, width, height, PixelFormats.Bgra32);
                    Debug.WriteLine($"convert bmp src from arr cuda: {stopwatch.ElapsedMilliseconds}ms");
                    convertArrCuda += stopwatch.ElapsedMilliseconds;
                }

            }

            Debug.WriteLine($"average intArrTt: {(double)intArrTt / times}ms");
            Debug.WriteLine($"average convertArrTt: {(double)convertArrTt / times}ms");
            Debug.WriteLine($"average intLstTt: {(double)intLstTt / times}ms");
            Debug.WriteLine($"average convertLstTt: {(double)convertLstTt / times}ms");
            Debug.WriteLine($"average intArrCuda: {(double)intArrCuda / times}ms");
            Debug.WriteLine($"average convertArrCuda: {(double)convertArrCuda / times}ms");
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