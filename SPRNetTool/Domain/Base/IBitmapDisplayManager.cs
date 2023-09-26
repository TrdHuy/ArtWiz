using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SPRNetTool.Domain.Base
{
    public interface IBitmapDisplayManager : IObservableDomain
    {
        void OpenBitmapFromFile(string filePath, bool countPixelColor);

        Task<BitmapSource?> OptimzeImageColor(Dictionary<Color, long> countableColorSource
            , BitmapSource oldBmpSource
            , int colorSize
            , int colorDifferenceDelta
            , bool isUsingAlpha
            , int colorDifferenceDeltaForCalculatingAlpha
            , Color backgroundForBlendColor);

        void SetCurrentlyDisplayedFrameOffset(ushort frameOffX, ushort frameOffY);

        void StartSprAnimation();

        void StopSprAnimation();

        void SetCurrentlyDisplayedSprFrameIndex(uint index);
    }
}
