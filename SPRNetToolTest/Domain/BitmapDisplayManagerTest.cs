using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;

namespace SPRNetToolTest.Domain
{
    internal class BitmapDisplayManagerTest
    {
        private string sprFilePath = "Resources\\test.spr";
        private string pngFilePath = "Resources\\test.png";
        private IBitmapDisplayManager bitmapDisplayManager;

        [SetUp]
        public void Setup()
        {
            bitmapDisplayManager = new BitmapDisplayManager();
            bool isNeedToOpenSprFile = GetType()
                .GetMethod(TestContext.CurrentContext.Test.MethodName ?? "")?
                .GetCustomAttributes(true)
                .Any(it => it is NeedToOpenSprFileAttribute) ?? false;

            if (isNeedToOpenSprFile)
            {
                bitmapDisplayManager.OpenBitmapFromFile(sprFilePath);
            }

        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test, NeedToOpenSprFile]
        public void test_InsertFrame()
        {
        }
    }

    internal class NeedToOpenSprFileAttribute : Attribute { }
}
