using SPRNetTool.Utils;
using System;
using System.Windows.Media;

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
        public void test2()
        {
            List<int> s = new List<int> { 1, 3, 3, 3, 4, 5, 6 };
            s.ReduceSameItem().ToList;

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