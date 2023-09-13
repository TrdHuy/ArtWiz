using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SPRNetTool.Domain
{
    public class BitmapDisplayManager : BaseDomain, IBitmapDisplayManager
    {
        protected ISprWorkManager SprWorkManager
        { get { return IDomainAccessors.DomainContext.GetDomain<ISprWorkManager>(); } }

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
            string fileExtension = Path.GetExtension(filePath).ToLower();
            if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
            {
                CurrentDisplayBitmap = this.LoadBitmapFromFile(filePath)?.Also((it) =>
                {
                    if (countPixelColor)
                    {
                        _currentDisplayingBitmap.ColorSource = this.CountColors(it);
                    }
                });
            }
            else if (fileExtension == ".spr")
            {
                CurrentDisplayBitmap = this.OpenSprFile(filePath)?.Also((it) =>
                {
                    if (countPixelColor)
                    {
                        _currentDisplayingBitmap.ColorSource = this.CountColors(it);
                    }
                });
            }

            NotifyChanged(new BitmapDisplayMangerChangedArg(_currentDisplayingBitmap.BitmapSource,
                 _currentDisplayingBitmap.ColorSource));
        }

        private BitmapSource? OpenSprFile(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (SprWorkManager.InitWorkManager(fs))
                    {
                        return SprWorkManager.GetFrameData(0)?.Let((it) =>
                        {
                            var byteData = this.ConvertPaletteColourArrayToByteArray(it.globleFrameData);
                            return this.GetBitmapFromRGBArray(byteData
                                , SprWorkManager.FileHead.GlobleWidth
                                , SprWorkManager.FileHead.GlobleHeight, PixelFormats.Bgra32)
                            .Also((it) => it.Freeze());
                        });
                    }

                    return null;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                return null;
            }
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
