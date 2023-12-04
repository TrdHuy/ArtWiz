using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.LogUtil;
using SPRNetTool.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace SPRNetTool.Domain
{
    class SprWorkManagerAdvance : SprWorkManagerCore, ISprWorkManagerAdvance
    {
        private Logger logger = new Logger("SprWorkManagerAdvance");
        private Logger pf_logger = new Logger("SprWorkManagerAdvance_PF");

        void ISprWorkManagerAdvance.SetNewColorToGlobalPalette(uint colorIndex,
            byte R, byte G, byte B)
        {
            var oldColor = PaletteData.modifiedPalette[(int)colorIndex];
            PaletteData.modifiedPalette[(int)colorIndex] = new PaletteColor(blue: B,
                green: G,
                red: R,
                alpha: oldColor.Alpha);
            FrameData?.FoEach(it =>
            {
                if (!it.isInsertedFrame)
                {
                    it.modifiedFrameRGBACache.SetPaletteColorChangedIndex((int)colorIndex,
                       oldColor: oldColor,
                       newColor: PaletteData.modifiedPalette[(int)colorIndex]);
                }
            });
        }

        void ISprWorkManagerAdvance.SetNewColorToInsertedFramePalette(int frameIndex, uint colorIndex,
            byte R, byte G, byte B)
        {
            if (frameIndex < FileHead.modifiedSprFileHeadCache.FrameCounts && FrameData != null)
            {
                if (FrameData[frameIndex].isInsertedFrame)
                {
                    var oldColor = FrameData[frameIndex]
                        .modifiedFrameRGBACache
                        .GetFramePaletteData().Data[(int)colorIndex];

                    FrameData[frameIndex]
                        .modifiedFrameRGBACache
                        .GetFramePaletteData().Data[(int)colorIndex] = new PaletteColor(blue: B,
                        green: G,
                        red: R,
                        alpha: oldColor.Alpha);

                    FrameData[frameIndex].modifiedFrameRGBACache.SetPaletteColorChangedIndex((int)colorIndex,
                        oldColor: oldColor,
                        newColor: FrameData[frameIndex]
                            .modifiedFrameRGBACache
                            .GetFramePaletteData().Data[(int)colorIndex]);
                }
                else
                {
                    throw new Exception("Frame is not inserted");
                }
            }
            else
            {
                throw new Exception("Frame is not existed");
            }
        }

        bool ISprWorkManagerAdvance.InsertFrame(uint frameIndex,
            ushort frameWidth,
            ushort frameHeight,
            PaletteColor[] pixelData,
            byte[] bgraBytesData,
            Palette paletteData,
            Dictionary<Color, long>? rgbCountableSource,
            Dictionary<int, List<long>>? paletteColorIndexToPixelIndexMap)
        {
#if DEBUG
            this.CountColors(pixelData,
               out long argbCount,
               out long rgbCount,
               out Dictionary<Color, long> argbColorSource,
               out Dictionary<Color, long> rgbColorSource);
            if (rgbCount > 256) throw new Exception("Number of color from pixel data must be smaller or equal 256.");
            foreach (var color in pixelData)
            {
                if (!paletteData.IsContain(color))
                {
                    throw new Exception("Palette data must include color from pixel data");
                }
            }
#endif

            var frameData = new FrameRGBA()
            {
                frameHeight = frameHeight,
                frameWidth = frameWidth,
                frameOffX = 0,
                frameOffY = 0,
                isInsertedFrame = true
            };
            frameData.originDecodedFrameData = pixelData;
            frameData.originDecodedBGRAData = bgraBytesData;
            frameData.modifiedFrameRGBACache.SetCopiedPaletteData(paletteData);
            frameData.modifiedFrameRGBACache.PaletteIndexToPixelIndexMap = paletteColorIndexToPixelIndexMap;
            var newLen = (FrameData?.Length + 1) ?? 1;
            var newFramesData = new FrameRGBA[newLen];

            for (int i = 0; i < newLen; i++)
            {
                if (i < frameIndex)
                {
                    newFramesData[i] = FrameData?[i] ?? new FrameRGBA();
                }
                else if (i == frameIndex)
                {
                    newFramesData[i] = frameData;
                }
                else if (i > frameIndex)
                {
                    newFramesData[i] = FrameData?[i - 1] ?? new FrameRGBA();
                }
            }

            if (IsCacheEmpty)
            {
                FileHead = SprFileHead.CreateSprFileHead();
                FileHead.ColorCounts = (ushort)paletteData.Size;
                FileHead.GlobalHeight = frameHeight;
                FileHead.GlobalWidth = frameWidth;
                PaletteData = new Palette(paletteData.Size);
                Array.Copy(paletteData.Data, PaletteData.Data, paletteData.Size);
                FrameDataBegPos = Marshal.SizeOf(typeof(US_SprFileHead)) + FileHead.ColorCounts * 3;
            }

            FileHead.modifiedSprFileHeadCache.FrameCounts++;
            FrameData = newFramesData;
            newFramesData[frameIndex].modifiedFrameRGBACache.RgbCountableSource = rgbCountableSource;
#if DEBUG
            if (rgbCountableSource == null)
                newFramesData[frameIndex].modifiedFrameRGBACache.RgbCountableSource = rgbColorSource;
#endif
            return true;
        }

        bool ISprWorkManagerAdvance.RemoveFrame(uint frameIndex)
        {
            var fileHead = FileHead.modifiedSprFileHeadCache;
            if (frameIndex < fileHead.FrameCounts && FrameData != null)
            {
                var newFrameData = new FrameRGBA[fileHead.FrameCounts - 1];
                for (int i = 0, j = 0; i < fileHead.FrameCounts; i++)
                {
                    if (i != frameIndex)
                    {
                        newFrameData[j++] = FrameData[i];
                    }
                }
                FrameData = newFrameData;
                fileHead.FrameCounts = (ushort)FrameData.Length;
                return true;
            }
            return false;
        }

        bool ISprWorkManagerAdvance.SwitchFrame(uint frameIndex1, uint frameIndex2)
        {
            if (FrameData == null)
            {
                logger.E("Failed to switch frame index, spr was not initialized!");
                return false;
            }

            if (frameIndex1 == frameIndex2 || frameIndex1 >= FrameData.Length || frameIndex2 >= FrameData.Length)
            {
                logger.E($"Failed to switch: frameIndex1={frameIndex1}, frameIndex2={frameIndex2}, FrameData length:{FrameData.Length}");
                return false;
            }

            var tempData = FrameData[frameIndex1];
            FrameData[frameIndex1] = FrameData[frameIndex2];
            FrameData[frameIndex2] = tempData;
            return true;
        }

        void ISprWorkManagerAdvance.SetFrameSize(ushort newFrameWidth, ushort newFrameHeight, uint frameIndex, Color? color)
        {
            if (frameIndex >= 0 && frameIndex < FileHead.modifiedSprFileHeadCache.FrameCounts && FrameData != null)
            {
                var modifiedFrameRGBACache = FrameData[frameIndex].modifiedFrameRGBACache;
                if (newFrameWidth != modifiedFrameRGBACache.frameWidth
                    || newFrameHeight != modifiedFrameRGBACache.frameHeight)
                {
                    modifiedFrameRGBACache.frameWidth = newFrameWidth;
                    modifiedFrameRGBACache.frameHeight = newFrameHeight;
                }
            }
        }

        void ISprWorkManagerAdvance.SetGlobalSize(ushort width, ushort height)
        {
            var sprFileHeadCache = FileHead.modifiedSprFileHeadCache;
            if (width != sprFileHeadCache.globalWidth || height != sprFileHeadCache.globalHeight)
            {
                sprFileHeadCache.globalWidth = width;
                sprFileHeadCache.globalHeight = height;
            }
        }

        void ISprWorkManagerAdvance.SetGlobalOffset(short offsetX, short offsetY)
        {
            var sprFileHeadCache = FileHead.modifiedSprFileHeadCache;
            if (offsetX != sprFileHeadCache.offX || offsetY != sprFileHeadCache.offY)
            {
                sprFileHeadCache.offX = offsetX;
                sprFileHeadCache.offY = offsetY;
            }
        }

        void ISprWorkManagerAdvance.SetFrameOffset(short offsetY, short offsetX, uint frameIndex)
        {
            if (frameIndex >= 0 && frameIndex < FileHead.modifiedSprFileHeadCache.FrameCounts && FrameData != null)
            {
                var modifiedFrameRGBACache = FrameData[frameIndex].modifiedFrameRGBACache;

                if (offsetY != modifiedFrameRGBACache.frameOffY
                    || offsetX != modifiedFrameRGBACache.frameOffX)
                {
                    modifiedFrameRGBACache.frameOffY = offsetY;
                    modifiedFrameRGBACache.frameOffX = offsetX;
                }
            }

        }

        void ISprWorkManagerAdvance.SetSprInterval(ushort interval)
        {
            if (IsCacheEmpty || FrameData?.Length == 1) return;

            var sprFileHeadCache = FileHead.modifiedSprFileHeadCache;
            sprFileHeadCache.Interval = interval;
        }

        FrameRGBA? ISprWorkManagerAdvance.GetFrameData(uint index)
        {
            if (index < FileHead.modifiedSprFileHeadCache.FrameCounts)
            {
                return FrameData?[index];
            }
            return null;
        }

        byte[]? ISprWorkManagerAdvance.GetDecodedBGRAData(uint index,
            out List<(Color, Color, int)> rgbColorChangedArgs)
        {
            var internalColorChangedArgs = new List<(Color, Color, int)>();
            if (index < FileHead.modifiedSprFileHeadCache.FrameCounts)
            {
                if (FrameData?[index].modifiedFrameRGBACache.IsPaletteColorChanged == true)
                {
                    var colorChangedCache = FrameData[index].modifiedFrameRGBACache.GetPaletteColorChangedIndex();
                    colorChangedCache.FoEach((i, paletteIndex) =>
                    {

                        var newColor = FrameData[index].isInsertedFrame ?
                            FrameData[index].modifiedFrameRGBACache.GetFramePaletteData().Data[paletteIndex] :
                            PaletteData.modifiedPalette[paletteIndex];

                        if (FrameData[index].modifiedFrameRGBACache.PaletteIndexToPixelIndexMap?.ContainsKey(paletteIndex) == true)
                        {
                            var pixelIndexMap = FrameData[index].modifiedFrameRGBACache.PaletteIndexToPixelIndexMap?[paletteIndex] ?? throw new Exception("Missing pixel map");
                            var oldColor = FrameData[index]
                                .modifiedFrameRGBACache
                                .modifiedFrameData[pixelIndexMap[0]];

                            pixelIndexMap.FoEach(pixelIndex =>
                            {
                                FrameData[index].modifiedFrameRGBACache.modifiedBGRAData[pixelIndex * 4] = newColor.Blue;
                                FrameData[index].modifiedFrameRGBACache.modifiedBGRAData[pixelIndex * 4 + 1] = newColor.Green;
                                FrameData[index].modifiedFrameRGBACache.modifiedBGRAData[pixelIndex * 4 + 2] = newColor.Red;

                                FrameData[index].modifiedFrameRGBACache.modifiedFrameData[pixelIndex].Blue = newColor.Blue;
                                FrameData[index].modifiedFrameRGBACache.modifiedFrameData[pixelIndex].Green = newColor.Green;
                                FrameData[index].modifiedFrameRGBACache.modifiedFrameData[pixelIndex].Red = newColor.Red;
                                // Do not change alpha 
                                //FrameData[index].modifiedFrameRGBACache.modifiedBGRAData[pixelIndex * 4 + 3] = newColor.Alpha;
                            });

                            internalColorChangedArgs.Add(new(
                                Color.FromRgb(oldColor.Red, oldColor.Green, oldColor.Blue),
                                Color.FromRgb(newColor.Red, newColor.Green, newColor.Blue),
                                pixelIndexMap.Count));
                        }
                    });
                    FrameData[index].modifiedFrameRGBACache.ResetPaletteColorChangedIndex();
                }
                rgbColorChangedArgs = internalColorChangedArgs;
                return FrameData?[index].modifiedFrameRGBACache.modifiedBGRAData;
            }
            rgbColorChangedArgs = internalColorChangedArgs;
            return null;
        }
    }
}
