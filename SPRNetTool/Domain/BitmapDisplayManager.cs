using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.Utils;
using SPRNetTool.View;
using SPRNetTool.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static SPRNetTool.View.InputWindow;

namespace SPRNetTool.Domain
{
    public class BitmapDisplayManager : BaseDomain, IBitmapDisplayManager
    {
        private struct CountablePxBmpSrc
        {
            public BitmapSource? BitmapSource;
            public Dictionary<Color, long>? ColorSource;
        }

        private CountablePxBmpSrc _currentDisplayingBitmap;

        private BitmapSource? CurrentDisplayBitmap
        {
            get { return _currentDisplayingBitmap.BitmapSource; }
            set
            {
                _currentDisplayingBitmap.BitmapSource = value;
            }
        }

        void IBitmapDisplayManager.OpenBitmapFromFile(string filePath, bool countPixelColor)
        {
            _currentDisplayingBitmap.BitmapSource = this.LoadBitmapFromFile(filePath);
            if (countPixelColor && CurrentDisplayBitmap != null)
            {
                _currentDisplayingBitmap.ColorSource = this.CountColors(CurrentDisplayBitmap);
            }
            else
            {
                _currentDisplayingBitmap.ColorSource = null;
            }
            NotifyChanged(new BitmapDisplayMangerChangedArg(_currentDisplayingBitmap.BitmapSource,
                 _currentDisplayingBitmap.ColorSource));
        }


        async Task<BitmapSource?> IBitmapDisplayManager.OptimzeImageColor(Dictionary<Color, long> countableColorSource
            , BitmapSource oldBmpSource
            , int colorSize
            , int colorDifferenceDelta
            , bool isUsingAlpha
            , int colorDifferenceDeltaForCalculatingAlpha
            , Color backgroundForBlendColor)
        {
            return await Task.Run<BitmapSource?>(() =>
            {
                 var orderedList = countableColorSource.OrderByDescending(kp => kp.Value).ToDictionary(kp => kp.Key, kp => kp.Value);
                 var selectedColorList = new List<Color>();


                 // TODO: Dynamic this
                 var selectedColorRecalculatedAlapha = new List<Color>();
                 var combinedRGBList = new List<Color>();
                 var expectedRGBList = new List<Color>();
                 var deltaDistanceForNewARGBColor = 10;
                 var deltaForAlphaAvarageDeviation = 3;

                 // Optimize color palette
                 while (selectedColorList.Count < colorSize && orderedList.Count > 0 && colorDifferenceDelta >= 0)
                 {
                     for (int i = 0; i < orderedList.Count; i++)
                     {
                         var expectedColor = orderedList.ElementAt(i).Key;
                         var shouldAdd = true;
                         foreach (var selectedColor in selectedColorList)
                         {
                             var distance = this.CalculateEuclideanDistance(expectedColor, selectedColor);
                             if (distance < colorDifferenceDelta)
                             {
                                 if (isUsingAlpha && distance < colorDifferenceDeltaForCalculatingAlpha)
                                 {
                                     var alpha = this.FindAlphaColors(selectedColor, backgroundForBlendColor, expectedColor, out byte averageAbsoluteDeviation);
                                     var newRGBColor = this.BlendColors(Color.FromArgb(alpha, selectedColor.R, selectedColor.G, selectedColor.B), backgroundForBlendColor);
                                     var distanceNewRGBColor = this.CalculateEuclideanDistance(newRGBColor, expectedColor);
                                     if (averageAbsoluteDeviation <= deltaForAlphaAvarageDeviation && distanceNewRGBColor <= deltaDistanceForNewARGBColor)
                                     {
                                         expectedRGBList.Add(expectedColor);
                                         combinedRGBList.Add(newRGBColor);
                                         selectedColorRecalculatedAlapha.Add(Color.FromArgb(alpha, selectedColor.R, selectedColor.G, selectedColor.B));
                                         orderedList.Remove(expectedColor);
                                         i--;
                                     }
                                 }
                                 shouldAdd = false;
                                 break;
                             }
                         }
                         if (shouldAdd)
                         {
                             selectedColorList.Add(expectedColor);
                             orderedList.Remove(expectedColor);
                             i--;
                         }

                         if (selectedColorList.Count >= colorSize) break;
                     }
                     colorDifferenceDelta -= 2;
                 }

                 //Combine RGB and ARGB color to selected list
                 var optimizedRGBCount = selectedColorList.Count;
                 var combinedColorList = selectedColorList.ToList().Also((it) => it.AddRange(combinedRGBList));

                 //reduce same combined color
                 combinedColorList = combinedColorList.GroupBy(c => c).Select(g => g.First()).ToList();


                 //======================================================
                 //Dithering
                 if (optimizedRGBCount > 0 && optimizedRGBCount <= colorSize && oldBmpSource != null)
                 {
                     var newBmpSrc = this.FloydSteinbergDithering(oldBmpSource, combinedColorList);
                     newBmpSrc?.Freeze();
                     return newBmpSrc;
                 }

                 return null;
             });

        }
    }

    public class BitmapDisplayMangerChangedArg : IDomainChangedArgs
    {
        public BitmapSource? CurrentDisplayingSource { get; private set; }
        public Dictionary<Color, long>? CurrentColorSource { get; private set; }

        public BitmapDisplayMangerChangedArg(BitmapSource? currentDisplayingSource = null,
            Dictionary<Color, long>? colorSource = null)
        {

            CurrentDisplayingSource = currentDisplayingSource;
            CurrentColorSource = colorSource;
        }
    }
}
