using SPRNetTool.Utils;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        [Test]
        public void test()
        {
            MS s;
            MS? s2;
            MS? s3 = null;
            var x = 1;
        }

        [Test]
        public void TestLoadBitmapSourceFromFile()
        {
            string imagePath = "Resources\\test.png".FullPath();
            var bmpSource = BitmapUtil.LoadBitmapFromFile(imagePath);
            Assert.NotNull(bmpSource);
            Assert.That(bmpSource.PixelWidth * bmpSource.PixelHeight, Is.EqualTo(90000));
        }

        [Test]
        public async Task TestCountColors()
        {
            string imagePath = "Resources\\test.png".FullPath();
            var bmpSource = BitmapUtil.LoadBitmapFromFile(imagePath);
            Assert.NotNull(bmpSource);
            var src = await BitmapUtil.CountColorsAsync(bmpSource);
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