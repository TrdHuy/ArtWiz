using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            public bool isPlaying;
            public bool isSprImage;
            public BitmapSource? DisplayingBitmapSource;
            public Dictionary<Color, long>? ColorSource;
            public BitmapSource?[] AnimationSourceCaching;
            public int? currentFrame;
        }

        private CountablePxBmpSrc _currentDisplayingBitmap;

        private BitmapSource? CurrentDisplayBitmap
        {
            get { return _currentDisplayingBitmap.DisplayingBitmapSource; }
            set
            {
                _currentDisplayingBitmap.DisplayingBitmapSource = value;
            }
        }

        async void IBitmapDisplayManager.StartSprAnimation()
        {
            if (!_currentDisplayingBitmap.isPlaying && _currentDisplayingBitmap.isSprImage
                && SprWorkManager.FileHead.FrameCounts > 1)
            {
                _currentDisplayingBitmap.isPlaying = true;
                _currentDisplayingBitmap.AnimationSourceCaching = new BitmapSource?[SprWorkManager.FileHead.FrameCounts];
                await PlayAnimation();
            }
        }

        void IBitmapDisplayManager.StopSprAnimation()
        {
            if (_currentDisplayingBitmap.isPlaying && _currentDisplayingBitmap.isSprImage
                && SprWorkManager.FileHead.FrameCounts > 1)
            {
                _currentDisplayingBitmap.isPlaying = false;
            }
        }

        void IBitmapDisplayManager.OpenBitmapFromFile(string filePath, bool countPixelColor)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();
            if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
            {
                CurrentDisplayBitmap = this.LoadBitmapFromFile(filePath)?.Also((it) =>
                {
                    _currentDisplayingBitmap.isSprImage = false;
                    _currentDisplayingBitmap.isPlaying = false;
                    _currentDisplayingBitmap.currentFrame = null;
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
                    _currentDisplayingBitmap.isSprImage = true;
                    if (countPixelColor)
                    {
                        _currentDisplayingBitmap.ColorSource = this.CountColors(it);
                    }
                });
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                    currentDisplayingSource: _currentDisplayingBitmap.DisplayingBitmapSource,
                    colorSource: _currentDisplayingBitmap.ColorSource,
                    sprFileHead: SprWorkManager.FileHead,
                    isPlayingAnimation: false));
                return;
            }

            NotifyChanged(new BitmapDisplayMangerChangedArg(
                currentDisplayingSource: _currentDisplayingBitmap.DisplayingBitmapSource,
                colorSource: _currentDisplayingBitmap.ColorSource,
                sprFileHead: null,
                isPlayingAnimation: false));
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

        private BitmapSource? OpenSprFile(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (SprWorkManager.InitWorkManagerFromSprFile(fs))
                    {
                        return SprWorkManager.GetFrameData(0)?.Let((it) =>
                        {
                            var byteData = this.ConvertPaletteColourArrayToByteArray(it.globalFrameData);
                            return this.GetBitmapFromRGBArray(byteData
                                , SprWorkManager.FileHead.GlobalWidth
                                , SprWorkManager.FileHead.GlobalHeight, PixelFormats.Bgra32)
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

        private async Task PlayAnimation()
        {
            await Task.Run(async () =>
            {
                Stopwatch stopwatch = new Stopwatch();

                int frameIndex = _currentDisplayingBitmap.currentFrame ?? 0;
                _currentDisplayingBitmap.currentFrame = frameIndex;
                while (_currentDisplayingBitmap.isPlaying)
                {
                    stopwatch.Restart();
                    _currentDisplayingBitmap.AnimationSourceCaching[frameIndex] =
                    _currentDisplayingBitmap.AnimationSourceCaching[frameIndex].IfNullThenLet(() =>
                        SprWorkManager.GetFrameData(frameIndex)?
                            .Let((it) => this.GetBitmapFromRGBArray(
                                this.ConvertPaletteColourArrayToByteArray(it.globalFrameData)
                                , SprWorkManager.FileHead.GlobalWidth
                                , SprWorkManager.FileHead.GlobalHeight, PixelFormats.Bgra32))
                            .Also((it) => it.Freeze()));
                    CurrentDisplayBitmap = _currentDisplayingBitmap.AnimationSourceCaching[frameIndex++];
                    _currentDisplayingBitmap.currentFrame++;
                    //DebugPage.GlobalStaticImageView!.Dispatcher.Invoke(() =>
                    //{
                    //    DebugPage.GlobalStaticImageView!.Source = CurrentDisplayBitmap;

                    //}, DispatcherPriority.Render);
                    NotifyChanged(new BitmapDisplayMangerChangedArg(
                        currentDisplayingSource: _currentDisplayingBitmap.DisplayingBitmapSource,
                        isPlayingAnimation: true, sprFileHead: SprWorkManager.FileHead, indexFrame: frameIndex));

                    if (frameIndex == SprWorkManager.FileHead.FrameCounts)
                    {
                        frameIndex = 0;
                        _currentDisplayingBitmap.currentFrame = 0;
                    }

                    int delayTime = SprWorkManager.FileHead.Interval - (int)stopwatch.ElapsedMilliseconds;
                    if (delayTime > 0)
                    {
                        await Task.Delay(delayTime);
                    }
                }

                NotifyChanged(new BitmapDisplayMangerChangedArg(
                        currentDisplayingSource: _currentDisplayingBitmap.DisplayingBitmapSource,
                        isPlayingAnimation: false,
                        sprFileHead: SprWorkManager.FileHead,
                        colorSource: _currentDisplayingBitmap.ColorSource));
            });
        }
    }

    public class BitmapDisplayMangerChangedArg : IDomainChangedArgs
    {
        public BitmapSource? CurrentDisplayingSource { get; private set; }
        public Dictionary<Color, long>? CurrentColorSource { get; private set; }
        public bool? IsPlayingAnimation { get; private set; }
        public SprFileHead? CurrentSprFileHead { get; private set; }

        public int FrameIndex { get; private set; }

        public BitmapDisplayMangerChangedArg(BitmapSource? currentDisplayingSource = null,
            Dictionary<Color, long>? colorSource = null,
            SprFileHead? sprFileHead = null,
            bool? isPlayingAnimation = null,
            int indexFrame = 0)
        {
            CurrentDisplayingSource = currentDisplayingSource;
            CurrentColorSource = colorSource;
            CurrentSprFileHead = sprFileHead;
            IsPlayingAnimation = isPlayingAnimation;
            FrameIndex = indexFrame;
        }
    }
}
