using SPRNetTool.Domain.Utils;
using SPRNetTool.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SPRNetTool.Domain.Base
{
    public interface IBitmapDisplayManager : IObservableDomain, IDomainAdapter
    {
        void OpenBitmapFromFile(string filePath, bool countPixelColor);

        void SetCurrentlyDisplayedFrameOffset(short frameOffX, short frameOffY);

        void StartSprAnimation();

        void StopSprAnimation();

        void SetCurrentlyDisplayedSprFrameIndex(uint index);

        void SetSprInterval(ushort interval);

        void SaveCurrentDisplaySourceToSprFile(string filePath);

        Dictionary<Color, long> CountBitmapColors(BitmapSource bitmap)
        {
            return this.CountColors(bitmap);
        }


        BitmapSource? OptimzeImageColor(List<(Color, long)> countableColorSource
            , BitmapSource oldBmpSource
            , int colorSize
            , int colorDifferenceDelta
            , bool isUsingAlpha
            , int colorDifferenceDeltaForCalculatingAlpha
            , Color backgroundForBlendColor
            , out List<Color> selectedColors
            , out List<Color> selectedAlphaColors
            , out List<Color> expectedRGBColors)
        {
            var orderedList = countableColorSource.OrderByDescending(it => it.Item2).ToList();
            var selectedColorList = new List<Color>();


            // TODO: Dynamic this
            var selectedAlphaColorsList = new List<Color>();
            var combinedRGBList = new List<Color>();
            var expectedRGBList = new List<Color>();
            var deltaDistanceForNewARGBColor = 10;
            var deltaForAlphaAvarageDeviation = 3;

            // Optimize color palette
            while (selectedColorList.Count < colorSize && orderedList.Count > 0 && colorDifferenceDelta >= 0)
            {
                for (int i = 0; i < orderedList.Count; i++)
                {
                    // For performance issue, do not use ElementAt to access the value with index
                    // use indexer instead
                    var expectedColor = orderedList[i].Item1;
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
                                    selectedAlphaColorsList.Add(Color.FromArgb(alpha, selectedColor.R, selectedColor.G, selectedColor.B));
                                    orderedList.RemoveAt(i);
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
                        orderedList.RemoveAt(i);
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
            combinedColorList = combinedColorList.ReduceSameItem().ToList();

            selectedColors = combinedColorList;
            selectedAlphaColors = selectedAlphaColorsList;
            expectedRGBColors = expectedRGBList;
            Debug.Assert(selectedColors.Count == selectedAlphaColors.Count + colorSize);
            Debug.Assert(expectedRGBColors.Count == selectedAlphaColors.Count);

            //======================================================
            //Dithering
            if (optimizedRGBCount > 0 && optimizedRGBCount <= colorSize && oldBmpSource != null)
            {
                var newBmpSrc = this.FloydSteinbergDithering(oldBmpSource, combinedColorList);
                newBmpSrc?.Freeze();
                return newBmpSrc;
            }

            return null;
        }
    }
}
