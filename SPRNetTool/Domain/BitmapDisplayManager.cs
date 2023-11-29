﻿using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.LogUtil;
using SPRNetTool.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static SPRNetTool.Domain.BitmapDisplayMangerChangedArg.ChangedEvent;
using static SPRNetTool.Domain.SprFrameCollectionChangedArg.ChangedEvent;
using static SPRNetTool.Domain.SprPaletteChangedArg.ChangedEvent;

namespace SPRNetTool.Domain
{
    public class BitmapDisplayManager : BaseDomain, IBitmapDisplayManager
    {
        private static Logger logger = new Logger("BitmapDisplayManager");
        private ISprWorkManagerAdvance? sprWorkManagerInstance;
        protected ISprWorkManagerAdvance SprWorkManager
        {
            get
            {
                return sprWorkManagerInstance ??
                    IDomainAccessors
                    .DomainContext
                    .GetDomain<ISprWorkManagerAdvance>()
                    .Also(it => sprWorkManagerInstance = it);
            }
        }

        private class BitmapSourceCache
        {
            public bool IsPlaying { get; set; }
            public bool IsSprImage { get; set; }
            public BitmapSource? DisplayedBitmapSource { get; set; }
            public Dictionary<Color, long>? DisplayedColorSource { get; set; }
            public BitmapSource?[]? AnimationSourceCaching { get; set; }
            public uint? CurrentFrameIndex { get; set; }
            public CancellationTokenSource? AnimationTokenSource { get; set; }

            public Dictionary<Color, long>?[]? ColorSourceCaching { get; set; }
        }

        private BitmapSourceCache DisplayedBitmapSourceCache { get; } = new BitmapSourceCache();

        #region public interface

        void IBitmapDisplayManager.SetNewColorToPalette(uint paletteIndex, Color newColor)
        {
            if (!SprWorkManager.IsWorkSpaceEmpty)
            {
                var currentFrameIndex = DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0;
                if (SprWorkManager.GetFrameData(currentFrameIndex)?.isInsertedFrame ?? false)
                {
                    SprWorkManager.SetNewColorToInsertedFramePalette((int)currentFrameIndex,
                        paletteIndex,
                        newColor.R,
                        newColor.G,
                        newColor.B);
                }
                else
                {
                    SprWorkManager.SetNewColorToGlobalPalette(paletteIndex, newColor.R, newColor.G, newColor.B);
                }

                if (InvalidateDisplayBitmapSourceCache(DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0))
                {
                    NotifyChanged(new BitmapDisplayMangerChangedArg(
                        changedEvent: CURRENT_DISPLAYING_SOURCE_CHANGED
                         | CURRENT_COLOR_SOURCE_CHANGED
                         | SPR_FILE_PALETTE_CHANGED,
                         currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                         colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                         paletteChangedArg: new SprPaletteChangedArg(
                             changedEvent: COLOR_CHANGED,
                             colorChangedIndex: paletteIndex,
                             newColor: newColor)));
                }
            }
        }

        void IBitmapDisplayManager.ChangeCurrentDisplayMode(bool isSpr)
        {
            if (isSpr && !SprWorkManager.IsWorkSpaceEmpty)
            {
                var frameIndex = DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0;
                DisplayedBitmapSourceCache.IsSprImage = true;
                DisplayedBitmapSourceCache.AnimationSourceCaching?[frameIndex].IfNullThenLet(() =>
                    CreateBitmapSourceFromDecodedFrameData(frameIndex));
                DisplayedBitmapSourceCache.DisplayedBitmapSource =
                    DisplayedBitmapSourceCache.AnimationSourceCaching?[frameIndex];
                DisplayedBitmapSourceCache.ColorSourceCaching?[frameIndex].IfNullThenLet(() =>
                    DisplayedBitmapSourceCache.DisplayedBitmapSource?.Let(it => this.CountColorsToDictionary(it)));
                DisplayedBitmapSourceCache.DisplayedColorSource =
                    DisplayedBitmapSourceCache.ColorSourceCaching?[frameIndex];

                NotifyChanged(new BitmapDisplayMangerChangedArg(
                   changedEvent: CURRENT_DISPLAYING_SOURCE_CHANGED
                       | CURRENT_COLOR_SOURCE_CHANGED
                       | SPR_FILE_HEAD_CHANGED
                       | SPR_FRAME_DATA_CHANGED
                       | SPR_FRAME_COLLECTION_CHANGED
                       | SPR_FRAME_SIZE_CHANGED
                       | SPR_FRAME_OFFSET_CHANGED
                       | SPR_GLOBAL_SIZE_CHANGED
                       | SPR_GLOBAL_OFFSET_CHANGED,
                   currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                   colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                   sprFileHead: SprWorkManager.FileHead,
                   sprFrameData: SprWorkManager.GetFrameData(frameIndex),
                   sprFrameCollectionChangedArg: new SprFrameCollectionChangedArg(changedEvent: TOTAL_FRAME_COUNT_CHANGED,
                       frameCount: (uint)(DisplayedBitmapSourceCache.AnimationSourceCaching?.Length ?? 0))));
            }
            else if (!isSpr)
            {
                // TODO: Implement change jpg here
            }
        }

        private BitmapSource? CreateBitmapSourceFromDecodedFrameData(uint frameIndex)
        {
            var frameInfo = SprWorkManager.GetFrameData(frameIndex) ?? throw new Exception();

            return SprWorkManager.GetDecodedBGRAData(frameIndex)?
                .Let((it) => this.GetBitmapFromRGBArray(it
                    , frameInfo.frameWidth
                    , frameInfo.frameHeight, PixelFormats.Bgra32))
                .Also((it) => it.Freeze());
        }

        bool IBitmapDisplayManager.InsertFrame(uint frameIndex, BitmapSource bmpSource)
        {
            return InsertBimapSourceToSprWorkSpace(frameIndex, bmpSource);
        }

        bool IBitmapDisplayManager.InsertFrame(uint frameIndex, string filePath)
        {
            var bitmapSource = this.LoadBitmapFromFile(filePath, isFreeze: true)
                ?? throw new Exception($"Failed to load bitmap from path {filePath}.");
            return InsertBimapSourceToSprWorkSpace(frameIndex, bitmapSource, filePath);
        }

        bool IBitmapDisplayManager.DeleteFrame(uint frameIndex)
        {
            if (!DisplayedBitmapSourceCache.IsSprImage) return false;
            return SprWorkManager.RemoveFrame(frameIndex).Also(success =>
            {
                if (success)
                {
                    var bmpSrc = new BitmapSource?[SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts];
                    for (int i = 0, j = 0; i < SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts + 1; i++)
                    {
                        if (i != frameIndex)
                        {
                            bmpSrc[j++] = DisplayedBitmapSourceCache.AnimationSourceCaching?[i];
                        }
                    }
                    DisplayedBitmapSourceCache.AnimationSourceCaching = bmpSrc;

                    var colorSrc = new Dictionary<Color, long>?[SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts];
                    for (int i = 0, j = 0; i < SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts + 1; i++)
                    {
                        if (i != frameIndex)
                        {
                            colorSrc[j++] = DisplayedBitmapSourceCache.ColorSourceCaching?[i];
                        }
                    }
                    DisplayedBitmapSourceCache.ColorSourceCaching = colorSrc;


                    if (DisplayedBitmapSourceCache.CurrentFrameIndex == frameIndex)
                    {
                        var newFrameIndex = frameIndex == SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts ? frameIndex - 1 : frameIndex;

                        if (DisplayedBitmapSourceCache.AnimationSourceCaching.Length > 0)
                        {
                            DisplayedBitmapSourceCache.AnimationSourceCaching[newFrameIndex] =
                            DisplayedBitmapSourceCache.AnimationSourceCaching[newFrameIndex].IfNullThenLet(() =>
                               CreateBitmapSourceFromDecodedFrameData(newFrameIndex));
                            DisplayedBitmapSourceCache.DisplayedBitmapSource = DisplayedBitmapSourceCache.AnimationSourceCaching[newFrameIndex];
                            DisplayedBitmapSourceCache.CurrentFrameIndex = newFrameIndex;

                            DisplayedBitmapSourceCache.DisplayedBitmapSource?.Apply(it =>
                            {
                                DisplayedBitmapSourceCache.ColorSourceCaching[newFrameIndex] =
                                    DisplayedBitmapSourceCache.ColorSourceCaching[newFrameIndex].IfNullThenLet(() =>
                                    this.CountColorsToDictionary(it));
                            });
                            DisplayedBitmapSourceCache.DisplayedColorSource = DisplayedBitmapSourceCache.ColorSourceCaching[newFrameIndex];
                        }
                        else
                        {
                            DisplayedBitmapSourceCache.DisplayedBitmapSource = null;
                            DisplayedBitmapSourceCache.CurrentFrameIndex = 0;
                            DisplayedBitmapSourceCache.DisplayedColorSource = null;
                        }

                        NotifyChanged(new BitmapDisplayMangerChangedArg(
                            changedEvent: CURRENT_DISPLAYING_FRAME_INDEX_CHANGED
                            | CURRENT_DISPLAYING_SOURCE_CHANGED
                            | CURRENT_COLOR_SOURCE_CHANGED,
                            colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                            currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                            currentDisplayFrameIndex: newFrameIndex));
                    }

                    NotifyChanged(new BitmapDisplayMangerChangedArg(
                        changedEvent: SPR_FILE_HEAD_CHANGED | SPR_FRAME_COLLECTION_CHANGED,
                        sprFileHead: SprWorkManager.FileHead,
                        sprFrameCollectionChangedArg: new SprFrameCollectionChangedArg(
                            changedEvent: FRAME_REMOVED,
                            oldFrameIndex: (int)frameIndex
                        )
                    ));
                }
            });
        }

        bool IBitmapDisplayManager.SwitchFrame(uint frameIndex1, uint frameIndex2)
        {
            if (!DisplayedBitmapSourceCache.IsSprImage) return false;

            return SprWorkManager.SwitchFrame(frameIndex1, frameIndex2).Also(success =>
            {
                if (success)
                {
                    if (DisplayedBitmapSourceCache.CurrentFrameIndex == frameIndex1)
                    {
                        DisplayedBitmapSourceCache.CurrentFrameIndex = frameIndex2;
                        NotifyChanged(new BitmapDisplayMangerChangedArg(
                            changedEvent: CURRENT_DISPLAYING_FRAME_INDEX_CHANGED,
                            currentDisplayFrameIndex: frameIndex2));
                    }
                    else if (DisplayedBitmapSourceCache.CurrentFrameIndex == frameIndex2)
                    {
                        DisplayedBitmapSourceCache.CurrentFrameIndex = frameIndex1;
                        NotifyChanged(new BitmapDisplayMangerChangedArg(
                            changedEvent: CURRENT_DISPLAYING_FRAME_INDEX_CHANGED,
                            currentDisplayFrameIndex: frameIndex1));
                    }

                    NotifyChanged(new BitmapDisplayMangerChangedArg(changedEvent: SPR_FRAME_COLLECTION_CHANGED,
                        sprFrameCollectionChangedArg: new SprFrameCollectionChangedArg(
                            changedEvent: FRAME_SWITCHED,
                            frameSwitch1Index: frameIndex1,
                            frameSwitch2Index: frameIndex2)));
                }
            });
        }

        void IBitmapDisplayManager.SetSprInterval(ushort interval)
        {
            SprWorkManager.SetSprInterval((ushort)interval);
            NotifyChanged(new BitmapDisplayMangerChangedArg(
                changedEvent: SPR_FILE_HEAD_CHANGED,
                sprFileHead: SprWorkManager.FileHead));
        }

        void IBitmapDisplayManager.SetSprGlobalSize(ushort width, ushort height)
        {
            if (!DisplayedBitmapSourceCache.IsSprImage) return;

            SprWorkManager.SetGlobalSize(width, height);

            uint index = DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0;

            if (InvalidateDisplayBitmapSourceCache(index))
            {
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                    changedEvent: SPR_FILE_HEAD_CHANGED
                        | CURRENT_COLOR_SOURCE_CHANGED
                        | SPR_GLOBAL_SIZE_CHANGED,
                    sprFileHead: SprWorkManager.FileHead,
                    colorSource: DisplayedBitmapSourceCache.DisplayedColorSource));
            }
        }

        void IBitmapDisplayManager.SetSprGlobalOffset(short offX, short offY)
        {
            if (!DisplayedBitmapSourceCache.IsSprImage) return;

            SprWorkManager.SetGlobalOffset(offX, offY);

            uint index = DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0;

            if (InvalidateDisplayBitmapSourceCache(index))
            {
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                    changedEvent: SPR_FILE_HEAD_CHANGED
                        | CURRENT_COLOR_SOURCE_CHANGED
                        | SPR_GLOBAL_OFFSET_CHANGED,
                    sprFileHead: SprWorkManager.FileHead,
                    colorSource: DisplayedBitmapSourceCache.DisplayedColorSource));
            }
        }

        void IBitmapDisplayManager.SetCurrentlyDisplayedSprFrameIndex(uint index)
        {
            if (index < 0 || index >= SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts) return;
            var currentFrameIndex = DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0;
            var isShouldNotifyPaletteChanged =
                SprWorkManager.GetFrameData(index)?.isInsertedFrame == true
                || (SprWorkManager.GetFrameData(currentFrameIndex)?.isInsertedFrame == true
                    && SprWorkManager.GetFrameData(index)?.isInsertedFrame == false);
            InitAnimationSourceCacheIfAsynchronous();
            if (InvalidateDisplayBitmapSourceCache(index))
            {
                var changeEvent = CURRENT_DISPLAYING_SOURCE_CHANGED
                     | CURRENT_COLOR_SOURCE_CHANGED
                     | CURRENT_DISPLAYING_FRAME_INDEX_CHANGED
                     | SPR_FRAME_DATA_CHANGED
                     | SPR_FRAME_SIZE_CHANGED
                     | SPR_FRAME_OFFSET_CHANGED;
                var palette = SprWorkManager.GetFrameData(index)?.isInsertedFrame == true ?
                    SprWorkManager.GetFrameData(index)?
                                .modifiedFrameRGBACache
                                .GetFramePaletteData() : SprWorkManager.PaletteData;
                if (isShouldNotifyPaletteChanged)
                {
                    changeEvent |= SPR_FILE_PALETTE_CHANGED;
                }

                NotifyChanged(new BitmapDisplayMangerChangedArg(
                    changedEvent: changeEvent,
                     currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                     colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                     currentDisplayFrameIndex: index,
                     sprFrameData: SprWorkManager.GetFrameData(index),
                     paletteChangedArg: isShouldNotifyPaletteChanged ? new SprPaletteChangedArg(
                         changedEvent: NEWLY_ADDED,
                         palette: palette) : null));
            }

        }

        void IBitmapDisplayManager.SetCurrentlyDisplayedFrameSize(ushort frameWidth, ushort frameHeight)
        {
            InitAnimationSourceCacheIfAsynchronous();
            uint index = DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0;
            SprWorkManager.SetFrameSize(frameWidth, frameHeight, index);

            if (InvalidateDisplayBitmapSourceCache(index))
            {
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                    changedEvent: CURRENT_COLOR_SOURCE_CHANGED
                        | SPR_FRAME_DATA_CHANGED
                        | SPR_FRAME_SIZE_CHANGED,
                    colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                    sprFrameData: SprWorkManager.GetFrameData(index)));
            }
        }

        void IBitmapDisplayManager.SetCurrentlyDisplayedFrameOffset(short frameOffX, short frameOffY)
        {
            InitAnimationSourceCacheIfAsynchronous();
            uint index = DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0;
            SprWorkManager.SetFrameOffset(frameOffY, frameOffX, index);

            if (InvalidateDisplayBitmapSourceCache(index))
            {
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                    changedEvent: CURRENT_COLOR_SOURCE_CHANGED
                        | SPR_FRAME_DATA_CHANGED
                        | SPR_FRAME_OFFSET_CHANGED,
                    colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                    sprFrameData: SprWorkManager.GetFrameData(index)));
            }
        }

        async void IBitmapDisplayManager.StartSprAnimation()
        {
            if (!DisplayedBitmapSourceCache.IsPlaying && DisplayedBitmapSourceCache.IsSprImage
                && SprWorkManager.FileHead.FrameCounts > 1)
            {
                DisplayedBitmapSourceCache.IsPlaying = true;

                InitAnimationSourceCacheIfAsynchronous();

                await PlayAnimation();
            }
        }

        void IBitmapDisplayManager.StopSprAnimation()
        {
            if (DisplayedBitmapSourceCache.IsPlaying && DisplayedBitmapSourceCache.IsSprImage
                && SprWorkManager.FileHead.FrameCounts > 1)
            {
                DisplayedBitmapSourceCache.IsPlaying = false;
                DisplayedBitmapSourceCache.AnimationTokenSource?.Cancel();
            }
        }

        void IBitmapDisplayManager.OpenBitmapFromFile(string filePath, bool countPixelColor)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();
            if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
            {
                DisplayedBitmapSourceCache.DisplayedBitmapSource = this.LoadBitmapFromFile(filePath)?.Let(it =>
                {
                    var extractedData = this.ConvertBitmapSourceToByteArray(it);
                    var extractedSource = this.GetBitmapFromRGBArray(extractedData,
                        it.PixelWidth,
                        it.PixelHeight,
                        it.Format);
                    extractedSource.Freeze();
                    return extractedSource;
                })
                .Also((it) =>
                {

                    DisplayedBitmapSourceCache.IsSprImage = false;
                    DisplayedBitmapSourceCache.IsPlaying = false;
                    DisplayedBitmapSourceCache.CurrentFrameIndex = null;
                    if (countPixelColor)
                    {
                        DisplayedBitmapSourceCache.DisplayedColorSource = this.CountColorsToDictionary(it);
                    }
                });
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                   changedEvent: CURRENT_DISPLAYING_SOURCE_CHANGED
                        | CURRENT_COLOR_SOURCE_CHANGED
                        | SPR_FILE_HEAD_CHANGED
                        | SPR_FRAME_DATA_CHANGED,
                   currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                   colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                   sprFileHead: null,
                   isPlayingAnimation: false));
                return;
            }
            else if (fileExtension == ".spr")
            {
                DisplayedBitmapSourceCache.DisplayedBitmapSource = OpenSprFile(filePath)?.Also((it) =>
                {
                    InitAnimationSourceCacheIfAsynchronous();
                    Debug.Assert(DisplayedBitmapSourceCache.ColorSourceCaching != null);
                    Debug.Assert(DisplayedBitmapSourceCache.AnimationSourceCaching != null);
                    DisplayedBitmapSourceCache.AnimationSourceCaching[0] = it;

                    DisplayedBitmapSourceCache.CurrentFrameIndex = 0;
                    DisplayedBitmapSourceCache.IsSprImage = true;
                    if (countPixelColor)
                    {
                        DisplayedBitmapSourceCache.ColorSourceCaching[0] = this.CountColorsToDictionary(it);
                        DisplayedBitmapSourceCache.DisplayedColorSource = DisplayedBitmapSourceCache.ColorSourceCaching[0];
                    }
                });
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                    changedEvent: CURRENT_DISPLAYING_SOURCE_CHANGED
                        | CURRENT_COLOR_SOURCE_CHANGED
                        | SPR_FILE_HEAD_CHANGED
                        | SPR_FRAME_DATA_CHANGED
                        | SPR_FRAME_COLLECTION_CHANGED
                        | SPR_FILE_PALETTE_CHANGED
                        | SPR_GLOBAL_SIZE_CHANGED
                        | SPR_GLOBAL_OFFSET_CHANGED
                        | SPR_FRAME_SIZE_CHANGED
                        | SPR_FRAME_OFFSET_CHANGED,
                    currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                    colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                    sprFileHead: SprWorkManager.FileHead,
                    sprFrameData: SprWorkManager.GetFrameData(0),
                    paletteChangedArg: new SprPaletteChangedArg(
                        changedEvent: NEWLY_ADDED,
                        palette: SprWorkManager.PaletteData),
                    sprFrameCollectionChangedArg: new SprFrameCollectionChangedArg(
                        changedEvent: TOTAL_FRAME_COUNT_CHANGED,
                        frameCount: (uint)(DisplayedBitmapSourceCache.AnimationSourceCaching?.Length ?? 0))));
                return;
            }


        }

        #endregion

        private void InitAnimationSourceCacheIfAsynchronous()
        {
            if (DisplayedBitmapSourceCache.AnimationSourceCaching == null ||
                            DisplayedBitmapSourceCache.AnimationSourceCaching.Length != SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts)
            {
                DisplayedBitmapSourceCache.AnimationSourceCaching = new BitmapSource?[SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts];
                DisplayedBitmapSourceCache.ColorSourceCaching = new Dictionary<Color, long>?[SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts];
            }
        }

        private BitmapSource? OpenSprFile(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (SprWorkManager.InitWorkManagerFromSprFile(fs))
                    {
                        return CreateBitmapSourceFromDecodedFrameData(0);
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
            DisplayedBitmapSourceCache.AnimationTokenSource = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                Stopwatch stopwatch = new Stopwatch();

                uint frameIndex = DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0;
                DisplayedBitmapSourceCache.CurrentFrameIndex = frameIndex;

                while (DisplayedBitmapSourceCache.IsPlaying && DisplayedBitmapSourceCache.AnimationSourceCaching != null)
                {
                    stopwatch.Restart();
                    DisplayedBitmapSourceCache.AnimationSourceCaching[frameIndex] =
                    DisplayedBitmapSourceCache.AnimationSourceCaching[frameIndex].IfNullThenLet(() =>
                        CreateBitmapSourceFromDecodedFrameData(frameIndex));
                    DisplayedBitmapSourceCache.DisplayedBitmapSource = DisplayedBitmapSourceCache.AnimationSourceCaching[frameIndex];

                    NotifyChanged(new BitmapDisplayMangerChangedArg(
                        changedEvent: IS_PLAYING_ANIMATION_CHANGED
                            | CURRENT_DISPLAYING_SOURCE_CHANGED
                            | CURRENT_DISPLAYING_FRAME_INDEX_CHANGED
                            | SPR_FRAME_DATA_CHANGED,
                        currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                        isPlayingAnimation: true,
                        currentDisplayFrameIndex: frameIndex,
                        animationInterval: SprWorkManager.FileHead.modifiedSprFileHeadCache.Interval,
                        sprFrameData: SprWorkManager.GetFrameData(frameIndex)));
                    DisplayedBitmapSourceCache.CurrentFrameIndex++;
                    frameIndex++;
                    if (frameIndex == SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts)
                    {
                        frameIndex = 0;
                        DisplayedBitmapSourceCache.CurrentFrameIndex = 0;
                    }
                    int delayTime = SprWorkManager.FileHead.modifiedSprFileHeadCache.Interval - (int)stopwatch.ElapsedMilliseconds;
                    if (delayTime > 0)
                    {
                        try
                        {
                            await Task.Delay(delayTime, DisplayedBitmapSourceCache.AnimationTokenSource.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            DisplayedBitmapSourceCache.IsPlaying = false;
                            break;
                        }
                    }
                }

                if (frameIndex > 0)
                {
                    DisplayedBitmapSourceCache.CurrentFrameIndex--;
                    frameIndex--;
                }
                else if (frameIndex == 0)
                {
                    frameIndex = (uint)(SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts - 1);
                    DisplayedBitmapSourceCache.CurrentFrameIndex = frameIndex;
                }

                DisplayedBitmapSourceCache.ColorSourceCaching?
                           .Apply(it => it[frameIndex] = it[frameIndex]
                           .IfNullThenLet(() => DisplayedBitmapSourceCache.DisplayedBitmapSource?
                           .Let(it => this.CountColorsToDictionary(it))));
                DisplayedBitmapSourceCache.DisplayedColorSource = DisplayedBitmapSourceCache.ColorSourceCaching?[frameIndex];
                DisplayedBitmapSourceCache.AnimationTokenSource = null;

                NotifyChanged(new BitmapDisplayMangerChangedArg(
                        changedEvent: IS_PLAYING_ANIMATION_CHANGED
                            | CURRENT_DISPLAYING_SOURCE_CHANGED
                            | CURRENT_COLOR_SOURCE_CHANGED
                            | CURRENT_DISPLAYING_FRAME_INDEX_CHANGED
                            | SPR_FRAME_DATA_CHANGED,
                        currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                        isPlayingAnimation: false,
                        colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                        currentDisplayFrameIndex: frameIndex,
                        sprFrameData: SprWorkManager.GetFrameData(frameIndex)));
            });
        }

        private bool InsertBimapSourceToSprWorkSpace(uint frameIndex, BitmapSource bitmapSource, string? filePath = null)
        {
            var countSource = this.CountColorsToDictionary(bitmapSource);
            if (countSource.Count > 256)
            {
                bitmapSource = (this as IBitmapDisplayManager).OptimzeImageColorNA256(bitmapSource)
                    ?? throw new Exception($"Failed to optimize image colors.");
            }

            var palettePixelArray = this.ConvertBitmapSourceToPaletteColorArray(bitmapSource,
                out Dictionary<Color, long> countableSource,
                out Palette palette,
                out byte[] bgraBytesData,
                out Dictionary<int, List<long>> paletteColorIndexToPixelIndexMap)
                ?? (filePath != null ?
                    throw new Exception($"Failed to load bitmap from path {filePath}") :
                    throw new Exception($"Failed to load bitmap"));


            if (SprWorkManager.InsertFrame(frameIndex
                , (ushort)bitmapSource.PixelWidth
                , (ushort)bitmapSource.PixelHeight
                , palettePixelArray
                , bgraBytesData
                , palette
                , countableSource
                , paletteColorIndexToPixelIndexMap))
            {
                // Update current displaying bitmap
                var bmpSrc = new BitmapSource?[SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts];
                for (int i = 0, j = 0; i < SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts; i++)
                {
                    if (i < frameIndex)
                    {
                        bmpSrc[j++] = DisplayedBitmapSourceCache.AnimationSourceCaching?[i];
                    }
                    else if (i == frameIndex)
                    {
                        bmpSrc[j++] = null;
                    }
                    else if (i > frameIndex)
                    {
                        bmpSrc[j++] = DisplayedBitmapSourceCache.AnimationSourceCaching?[i - 1];
                    }
                }
                DisplayedBitmapSourceCache.AnimationSourceCaching = bmpSrc;
                DisplayedBitmapSourceCache.AnimationSourceCaching[frameIndex]
                    = DisplayedBitmapSourceCache.AnimationSourceCaching[frameIndex].IfNullThenLet(() =>
                           CreateBitmapSourceFromDecodedFrameData(frameIndex));
                DisplayedBitmapSourceCache.DisplayedBitmapSource = DisplayedBitmapSourceCache.AnimationSourceCaching[frameIndex];
                DisplayedBitmapSourceCache.CurrentFrameIndex = frameIndex;

                // Update current displaying color source
                var colorSrc = new Dictionary<Color, long>?[SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts];
                for (int i = 0, j = 0; i < SprWorkManager.FileHead.modifiedSprFileHeadCache.FrameCounts; i++)
                {
                    if (i < frameIndex)
                    {
                        colorSrc[j++] = DisplayedBitmapSourceCache.ColorSourceCaching?[i];
                    }
                    else if (i == frameIndex)
                    {
                        colorSrc[j++] = null;
                    }
                    else if (i > frameIndex)
                    {
                        colorSrc[j++] = DisplayedBitmapSourceCache.ColorSourceCaching?[i - 1];
                    }
                }
                DisplayedBitmapSourceCache.ColorSourceCaching = colorSrc;
                DisplayedBitmapSourceCache.ColorSourceCaching[frameIndex]
                    = DisplayedBitmapSourceCache.ColorSourceCaching[frameIndex].IfNullThenLet(() =>
                        this.CountColorsToDictionary(DisplayedBitmapSourceCache
                            .DisplayedBitmapSource ?? throw new Exception("DisplayedBitmapSource must not be null here")));
                DisplayedBitmapSourceCache.DisplayedColorSource
                    = DisplayedBitmapSourceCache.ColorSourceCaching[frameIndex];

                NotifyChanged(new BitmapDisplayMangerChangedArg(
                            changedEvent: CURRENT_DISPLAYING_FRAME_INDEX_CHANGED
                            | CURRENT_DISPLAYING_SOURCE_CHANGED
                            | CURRENT_COLOR_SOURCE_CHANGED
                            | SPR_FILE_HEAD_CHANGED
                            | SPR_FILE_PALETTE_CHANGED
                            | SPR_FRAME_COLLECTION_CHANGED,
                            sprFileHead: SprWorkManager.FileHead,
                            currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                            currentDisplayFrameIndex: frameIndex,
                            colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                            paletteChangedArg: new SprPaletteChangedArg(
                                changedEvent: NEWLY_ADDED,
                                palette: SprWorkManager
                                .GetFrameData(DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0)?
                                .modifiedFrameRGBACache
                                .GetFramePaletteData()),
                            sprFrameCollectionChangedArg: new SprFrameCollectionChangedArg(
                                changedEvent: FRAME_INSERTED,
                                newFrameIndex: (int)frameIndex
                        )));


                return true;
            }

            return false;
        }

        private bool InvalidateDisplayBitmapSourceCache(uint index)
        {
            return DisplayedBitmapSourceCache.AnimationSourceCaching?.Let(it =>
            {
                if (it[index] == null)
                {
                    it[index] = CreateBitmapSourceFromDecodedFrameData(index) ?? throw new Exception();
                    DisplayedBitmapSourceCache.ColorSourceCaching?
                       .Apply(it2 =>
                       {
                           it2[index] = this.CountColorsToDictionary(it[index]!);
                       });
                }
                else if (SprWorkManager.GetFrameData(index)?.modifiedFrameRGBACache.IsPaletteColorChanged == true)
                {
                    var changedPaletteColors = SprWorkManager.GetFrameData(index)?.modifiedFrameRGBACache.GetChangedPaletteColors();
                    it[index] = CreateBitmapSourceFromDecodedFrameData(index) ?? throw new Exception();
                    DisplayedBitmapSourceCache.ColorSourceCaching?
                      .Apply(it2 =>
                      {
                          if (it2[index] == null)
                          {
                              it2[index] = this.CountColorsToDictionary(it[index]!);
                          }
                          else
                          {
                              changedPaletteColors?.FoEach(it =>
                              {
                                  var cache = it2[index]!;
                                  var oldColor = Color.FromRgb(
                                      it.Item1.Red,
                                      it.Item1.Green,
                                      it.Item1.Blue);
                                  var newColor = Color.FromRgb(
                                     it.Item2.Red,
                                     it.Item2.Green,
                                     it.Item2.Blue);
                                  var count = cache[oldColor];
                                  cache.Remove(oldColor);
                                  cache.Add(newColor, count);
                              });
                          }
                      });
                }

                DisplayedBitmapSourceCache.DisplayedBitmapSource = it[index];
                DisplayedBitmapSourceCache.CurrentFrameIndex = index;
                DisplayedBitmapSourceCache.DisplayedColorSource = DisplayedBitmapSourceCache.ColorSourceCaching?[index];
                return true;
            }) ?? false;
        }

        protected override void NotifyChanged(IDomainChangedArgs args)
        {
            base.NotifyChanged(args);
            var changedEvent = ((BitmapDisplayMangerChangedArg)args).Event;
            logger.D($"ChangedEvent: dec={changedEvent},bin={Convert.ToString((int)changedEvent, 2)}");
        }
    }

    public class BitmapDisplayMangerChangedArg : IDomainChangedArgs
    {
        public enum ChangedEvent
        {
            CURRENT_DISPLAYING_SOURCE_CHANGED = 0b1,
            CURRENT_COLOR_SOURCE_CHANGED = 0b10,
            IS_PLAYING_ANIMATION_CHANGED = 0b100,
            SPR_FILE_HEAD_CHANGED = 0b1000,
            CURRENT_DISPLAYING_FRAME_INDEX_CHANGED = 0b10000,
            SPR_FRAME_DATA_CHANGED = 0b100000,
            SPR_FRAME_OFFSET_CHANGED = 0b1000000,
            SPR_FRAME_SIZE_CHANGED = 0b10000000,
            SPR_GLOBAL_OFFSET_CHANGED = 0b100000000,
            SPR_GLOBAL_SIZE_CHANGED = 0b1000000000,
            SPR_FRAME_COLLECTION_CHANGED = 0b10000000000,
            SPR_FILE_PALETTE_CHANGED = 0b100000000000,
        }

        public ChangedEvent Event { get; private set; }
        public BitmapSource? CurrentDisplayingSource { get; private set; }
        public Dictionary<Color, long>? CurrentColorSource { get; private set; }
        public bool? IsPlayingAnimation { get; private set; }
        public SprFileHead? CurrentSprFileHead { get; private set; }
        public uint CurrentDisplayingFrameIndex { get; private set; }
        public FrameRGBA? SprFrameData { get; private set; }
        public uint SprFrameCount { get; private set; }
        public uint AnimationInterval { get; private set; }
        public SprPaletteChangedArg? PaletteChangedArg { get; private set; }
        public SprFrameCollectionChangedArg? SprFrameCollectionChangedArg { get; private set; }

        public BitmapDisplayMangerChangedArg(ChangedEvent changedEvent, BitmapSource? currentDisplayingSource = null,
            Dictionary<Color, long>? colorSource = null,
            SprFileHead? sprFileHead = null,
            bool? isPlayingAnimation = null,
            uint currentDisplayFrameIndex = 0,
            SprFrameCollectionChangedArg? sprFrameCollectionChangedArg = null,
            FrameRGBA? sprFrameData = null,
            uint animationInterval = 0,
            SprPaletteChangedArg? paletteChangedArg = null)
        {
            CurrentDisplayingSource = currentDisplayingSource;
            CurrentColorSource = colorSource;
            CurrentSprFileHead = sprFileHead;
            IsPlayingAnimation = isPlayingAnimation;
            CurrentDisplayingFrameIndex = currentDisplayFrameIndex;
            SprFrameData = sprFrameData;
            SprFrameCollectionChangedArg = sprFrameCollectionChangedArg;
            PaletteChangedArg = paletteChangedArg;
            AnimationInterval = animationInterval;
            Event = changedEvent;
        }
    }

    public class SprFrameCollectionChangedArg
    {
        public enum ChangedEvent
        {
            TOTAL_FRAME_COUNT_CHANGED = 0b1,
            FRAME_INSERTED = 0b10,
            FRAME_REMOVED = 0b100,
            FRAME_SWITCHED = 0b1000,
        }

        public ChangedEvent Event { get; private set; }
        public int NewFrameIndex { get; private set; }
        public int OldFrameIndex { get; private set; }
        public uint FrameCount { get; private set; }
        public uint FrameSwitched1Index { get; private set; }
        public uint FrameSwitched2Index { get; private set; }

        public SprFrameCollectionChangedArg(ChangedEvent changedEvent,
            uint frameCount = 0,
            int newFrameIndex = 0,
            uint frameSwitch1Index = 0,
            uint frameSwitch2Index = 0,
            int oldFrameIndex = 0)
        {
            Event = changedEvent;
            FrameCount = frameCount;
            NewFrameIndex = newFrameIndex;
            OldFrameIndex = oldFrameIndex;
            FrameSwitched1Index = frameSwitch1Index;
            FrameSwitched2Index = frameSwitch2Index;
        }
    }

    public class SprPaletteChangedArg
    {
        public enum ChangedEvent
        {
            NEWLY_ADDED = 0b1,
            COLOR_CHANGED = 0b10,
        }

        public ChangedEvent Event { get; private set; }
        public uint ColorChangedIndex { get; private set; }
        public Color NewColor { get; private set; }
        public Palette? Palette { get; private set; }

        public SprPaletteChangedArg(ChangedEvent changedEvent,
            Palette? palette = null,
            uint colorChangedIndex = 0,
            Color newColor = default)
        {
            Event = changedEvent;
            ColorChangedIndex = colorChangedIndex;
            Palette = palette;
            NewColor = newColor;
        }
    }

}
