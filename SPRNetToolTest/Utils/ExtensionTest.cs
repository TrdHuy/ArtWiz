using SPRNetTool.Utils;
using System.Windows.Media;

namespace SPRNetToolTest.Utils
{
    internal class ExtensionTest
    {
        [Test]
        public void test_ReduceSameItem()
        {
            var colorItem = new Color[] {
                Color.FromArgb(1,1,1,1),
                Color.FromArgb(1,1,1,1),
                Color.FromArgb(1,1,1,2),
                Color.FromArgb(1,1,1,2),
                Color.FromArgb(1,1,1,3)
            };
            var reducedList = colorItem.ReduceSameItem().ToList();

            Assert.That(reducedList.Count == 3);
            Assert.That(reducedList[0] == Color.FromArgb(1, 1, 1, 1));
            Assert.That(reducedList[1] == Color.FromArgb(1, 1, 1, 2));
            Assert.That(reducedList[2] == Color.FromArgb(1, 1, 1, 3));
        }
    }
}
