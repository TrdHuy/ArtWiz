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

        bool ISprWorkManagerAdvance.InsertFrame(uint frameIndex,
          ushort frameWidth,
          ushort frameHeight,
          PaletteColor[] pixelData,
          Palette paletteData,
          Dictionary<Color, long>? countableSource)
        {
#if DEBUG
            var countableColorSource = this.CountColors(pixelData);
            if (countableSource != null && !this.AreCountableSourcesEqual(countableSource, countableColorSource))
            {
                throw new Exception("failed to count color source!");
            }
            if (countableColorSource.Count > 256) throw new Exception("Number of color from pixel data must be smaller or equal 256.");
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
            newFramesData[frameIndex].modifiedFrameRGBACache.SetCopiedPaletteData(paletteData);
            newFramesData[frameIndex].globalFrameData = InitGlobalizedFrameDataFromOrigin(frameIndex)
                ?? throw new Exception("Failed to insert frame");
            newFramesData[frameIndex].modifiedFrameRGBACache.CountableSource = countableSource;
#if DEBUG
            if (countableSource == null)
                newFramesData[frameIndex].modifiedFrameRGBACache.CountableSource = countableColorSource;
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
            FrameData[frameIndex1].isNeedToRedrawGlobalFrameData = true;
            FrameData[frameIndex2].isNeedToRedrawGlobalFrameData = true;
            return true;
        }

        void ISprWorkManagerAdvance.SetFrameSize(ushort newFrameWidth, ushort newFrameHeight, uint frameIndex, Color? color)
        {
            var startTime = DateTime.Now;
            if (frameIndex >= 0 && frameIndex < FileHead.FrameCounts && FrameData != null)
            {
                var extendingColor = color ?? Colors.White;
                var modifiedFrameRGBACache = FrameData[frameIndex].modifiedFrameRGBACache;
                if (newFrameWidth != modifiedFrameRGBACache.frameWidth
                    || newFrameHeight != modifiedFrameRGBACache.frameHeight)
                {
                    PaletteColor getPaletteColorInRef(uint newX, uint newY, ushort refFrameHeight, ushort refFrameWidth, PaletteColor[] refFrameData)
                    {
                        if (newX >= refFrameWidth || newY >= refFrameHeight)
                        {
                            return new PaletteColor(blue: extendingColor.B,
                                green: extendingColor.G,
                                red: extendingColor.R,
                                alpha: extendingColor.A);
                        }
                        return refFrameData[newY * refFrameWidth + newX];
                    }

                    var newDecodedFrameData = new PaletteColor[newFrameWidth * newFrameHeight];
                    for (ushort newY = 0; newY < newFrameHeight; newY++)
                    {
                        for (ushort newX = 0; newX < newFrameWidth; newX++)
                        {
                            newDecodedFrameData[newY * newFrameWidth + newX]
                                = getPaletteColorInRef(newX,
                                    newY,
                                    refFrameHeight: FrameData[frameIndex].frameHeight,
                                    refFrameWidth: FrameData[frameIndex].frameWidth,
                                    refFrameData: FrameData[frameIndex].originDecodedFrameData);
                        }
                    }

                    var oldWidth = modifiedFrameRGBACache.frameWidth;
                    var oldHeight = modifiedFrameRGBACache.frameHeight;
                    modifiedFrameRGBACache.frameWidth = newFrameWidth;
                    modifiedFrameRGBACache.frameHeight = newFrameHeight;
                    modifiedFrameRGBACache.modifiedFrameData = newDecodedFrameData;
                    FrameData[frameIndex].isNeedToRedrawGlobalFrameData = true;

                    pf_logger.I($"set frame size from {oldWidth}x{oldHeight} to {newFrameWidth}x{newFrameHeight} in: {(DateTime.Now - startTime).TotalMilliseconds}ms");
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
                int count = FrameData?.Length ?? 0;
                FrameData?.Apply(it =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        it[i].isNeedToRedrawGlobalFrameData = true;
                    }
                });

            }
        }

        void ISprWorkManagerAdvance.SetGlobalOffset(short offsetX, short offsetY)
        {
            var sprFileHeadCache = FileHead.modifiedSprFileHeadCache;
            if (offsetX != sprFileHeadCache.offX || offsetY != sprFileHeadCache.offY)
            {
                sprFileHeadCache.offX = offsetX;
                sprFileHeadCache.offY = offsetY;
                int count = FrameData?.Length ?? 0;
                FrameData?.Apply(it =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        it[i].isNeedToRedrawGlobalFrameData = true;
                    }
                });
            }
        }

        void ISprWorkManagerAdvance.SetFrameOffset(short offsetY, short offsetX, uint frameIndex)
        {
            var startTime = DateTime.Now;
            if (frameIndex >= 0 && frameIndex < FileHead.FrameCounts && FrameData != null)
            {
                var modifiedFrameRGBACache = FrameData[frameIndex].modifiedFrameRGBACache;

                if (offsetY != modifiedFrameRGBACache.frameOffY
                    || offsetX != modifiedFrameRGBACache.frameOffX)
                {
                    var oldOffsetY = modifiedFrameRGBACache.frameOffY;
                    var oldOffsetX = modifiedFrameRGBACache.frameOffX;
                    modifiedFrameRGBACache.frameOffY = offsetY;
                    modifiedFrameRGBACache.frameOffX = offsetX;
                    FrameData[frameIndex].isNeedToRedrawGlobalFrameData = true;

                    pf_logger.I($"set frame offset from {oldOffsetX}-{oldOffsetY} to {offsetX}-{offsetY} in: {(DateTime.Now - startTime).TotalMilliseconds}ms");
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

        PaletteColor[]? ISprWorkManagerAdvance.GetGlobalFrameColorData(uint index, out bool isFrameRedrawed)
        {
            var isRedrawed = false;
            if (index < FileHead.modifiedSprFileHeadCache.FrameCounts)
            {
                FrameData?.Apply(it =>
                {
                    if (it[index].isNeedToRedrawGlobalFrameData == true)
                    {
                        var globalData = InitGlobalizedFrameDataFromModifiedCache(index);
                        if (globalData == null) throw new Exception("Failed to redraw global frame data from modified cache");
                        it[index].globalFrameData = globalData;
                        it[index].isNeedToRedrawGlobalFrameData = false;
                        isRedrawed = true;
                    }
                });
                isFrameRedrawed = isRedrawed;
                return FrameData?[index].globalFrameData;
            }
            isFrameRedrawed = false;
            return null;
        }
    }
}
