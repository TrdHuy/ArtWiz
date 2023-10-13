using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.LogUtil;
using SPRNetTool.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static SPRNetTool.Domain.BitmapDisplayMangerChangedArg.ChangedEvent;

namespace SPRNetTool.Domain
{
    public class BitmapDisplayManager : BaseDomain, IBitmapDisplayManager
    {
        private static Logger logger = new Logger("BitmapDisplayManager");
        protected ISprWorkManager SprWorkManager
        { get { return IDomainAccessors.DomainContext.GetDomain<ISprWorkManager>(); } }

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
        void IBitmapDisplayManager.SetSprInterval(ushort interval)
        {
            SprWorkManager.SetSprInterval((ushort)interval);
            NotifyChanged(new BitmapDisplayMangerChangedArg(
                changedEvent: SPR_FILE_HEAD_CHANGED,
                sprFileHead: SprWorkManager.FileHead));
        }

        void IBitmapDisplayManager.SetCurrentlyDisplayedSprFrameIndex(uint index)
        {
            if (index < 0 || index >= SprWorkManager.FileHead.FrameCounts) return;

            if (DisplayedBitmapSourceCache.AnimationSourceCaching == null ||
                DisplayedBitmapSourceCache.AnimationSourceCaching.Length != SprWorkManager.FileHead.FrameCounts)
            {
                DisplayedBitmapSourceCache.AnimationSourceCaching = new BitmapSource?[SprWorkManager.FileHead.FrameCounts];
                DisplayedBitmapSourceCache.ColorSourceCaching = new Dictionary<Color, long>?[SprWorkManager.FileHead.FrameCounts];
            }

            DisplayedBitmapSourceCache.AnimationSourceCaching?.Also(it =>
            {
                it[index] = it[index].IfNullThenLet(() =>
                    SprWorkManager.GetFrameData(index)?
                        .Let((it) => this.GetBitmapFromRGBArray(
                            this.ConvertPaletteColourArrayToByteArray(it.globalFrameData)
                            , SprWorkManager.FileHead.GlobalWidth
                            , SprWorkManager.FileHead.GlobalHeight, PixelFormats.Bgra32))
                        .Also((it) => it.Freeze()));
                DisplayedBitmapSourceCache.DisplayedBitmapSource = it[index];
                DisplayedBitmapSourceCache.ColorSourceCaching?
                    .Apply(it => it[index] = it[index]
                        .IfNullThenLet(() => DisplayedBitmapSourceCache.DisplayedBitmapSource?
                            .Let(it => this.CountColorsToDictionary(it))));
                DisplayedBitmapSourceCache.CurrentFrameIndex = index;
                DisplayedBitmapSourceCache.DisplayedColorSource = DisplayedBitmapSourceCache.ColorSourceCaching?[index];
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                   changedEvent: CURRENT_DISPLAYING_SOURCE_CHANGED
                    | CURRENT_COLOR_SOURCE_CHANGED
                    | SPR_FRAME_INDEX_CHANGED
                    | SPR_FRAME_DATA_CHANGED,
                    currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                    colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                    sprFrameIndex: index,
                    sprFrameData: SprWorkManager.GetFrameData(index)));
            });
        }

        void IBitmapDisplayManager.SetCurrentlyDisplayedFrameSize(ushort frameWidth, ushort frameHeight)
        {
            if (DisplayedBitmapSourceCache.AnimationSourceCaching == null ||
                DisplayedBitmapSourceCache.AnimationSourceCaching.Length != SprWorkManager.FileHead.FrameCounts)
            {
                DisplayedBitmapSourceCache.AnimationSourceCaching = new BitmapSource?[SprWorkManager.FileHead.FrameCounts];
                DisplayedBitmapSourceCache.ColorSourceCaching = new Dictionary<Color, long>?[SprWorkManager.FileHead.FrameCounts];
            }
            uint index = DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0;
            SprWorkManager.SetFrameSize(frameWidth, frameHeight, index, Colors.Aqua);

            DisplayedBitmapSourceCache.AnimationSourceCaching?.Also(it =>
            {
                it[index] = SprWorkManager.GetFrameData(index)?
                        .Let((it) => this.GetBitmapFromRGBArray(
                            this.ConvertPaletteColourArrayToByteArray(it.globalFrameData)
                            , SprWorkManager.FileHead.GlobalWidth
                            , SprWorkManager.FileHead.GlobalHeight, PixelFormats.Bgra32))
                        .Also((it) => it.Freeze());

                DisplayedBitmapSourceCache.DisplayedBitmapSource = it[index];
                DisplayedBitmapSourceCache.ColorSourceCaching?
                   .Apply(it => it[index] = it[index]
                       .IfNullThenLet(() => DisplayedBitmapSourceCache.DisplayedBitmapSource?
                           .Let(it => this.CountColorsToDictionary(it))));
                DisplayedBitmapSourceCache.CurrentFrameIndex = index;
                DisplayedBitmapSourceCache.DisplayedColorSource = DisplayedBitmapSourceCache.ColorSourceCaching?[index];
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                    changedEvent: CURRENT_DISPLAYING_SOURCE_CHANGED
                        | CURRENT_COLOR_SOURCE_CHANGED
                        | SPR_FRAME_DATA_CHANGED
                        | SPR_FRAME_SIZE_CHANGED,
                    currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                    colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                    sprFrameData: SprWorkManager.GetFrameData(index)));
            });
        }

        void IBitmapDisplayManager.SetCurrentlyDisplayedFrameOffset(short frameOffX, short frameOffY)
        {
            if (DisplayedBitmapSourceCache.AnimationSourceCaching == null ||
                DisplayedBitmapSourceCache.AnimationSourceCaching.Length != SprWorkManager.FileHead.FrameCounts)
            {
                DisplayedBitmapSourceCache.AnimationSourceCaching = new BitmapSource?[SprWorkManager.FileHead.FrameCounts];
                DisplayedBitmapSourceCache.ColorSourceCaching = new Dictionary<Color, long>?[SprWorkManager.FileHead.FrameCounts];
            }
            uint index = DisplayedBitmapSourceCache.CurrentFrameIndex ?? 0;
            SprWorkManager.SetFrameOffset(frameOffY, frameOffX, index);

            DisplayedBitmapSourceCache.AnimationSourceCaching?.Also(it =>
            {
                it[index] = SprWorkManager.GetFrameData(index)?
                        .Let((it) => this.GetBitmapFromRGBArray(
                            this.ConvertPaletteColourArrayToByteArray(it.globalFrameData)
                            , SprWorkManager.FileHead.GlobalWidth
                            , SprWorkManager.FileHead.GlobalHeight, PixelFormats.Bgra32))
                        .Also((it) => it.Freeze());

                DisplayedBitmapSourceCache.DisplayedBitmapSource = it[index];
                DisplayedBitmapSourceCache.ColorSourceCaching?
                    .Also(it => it[index] = DisplayedBitmapSourceCache.DisplayedBitmapSource?
                        .Let(it => this.CountColorsToDictionary(it)));
                DisplayedBitmapSourceCache.CurrentFrameIndex = index;
                DisplayedBitmapSourceCache.DisplayedColorSource = DisplayedBitmapSourceCache.ColorSourceCaching?[index];
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                    changedEvent: CURRENT_DISPLAYING_SOURCE_CHANGED
                        | CURRENT_COLOR_SOURCE_CHANGED
                        | SPR_FRAME_DATA_CHANGED
                        | SPR_FRAME_OFFSET_CHANGED,
                    currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                    colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                    sprFrameData: SprWorkManager.GetFrameData(index)));
            });
        }

        async void IBitmapDisplayManager.StartSprAnimation()
        {
            if (!DisplayedBitmapSourceCache.IsPlaying && DisplayedBitmapSourceCache.IsSprImage
                && SprWorkManager.FileHead.FrameCounts > 1)
            {
                DisplayedBitmapSourceCache.IsPlaying = true;

                if (DisplayedBitmapSourceCache.AnimationSourceCaching == null ||
                    DisplayedBitmapSourceCache.AnimationSourceCaching.Length != SprWorkManager.FileHead.FrameCounts)
                {
                    DisplayedBitmapSourceCache.AnimationSourceCaching = new BitmapSource?[SprWorkManager.FileHead.FrameCounts];
                    DisplayedBitmapSourceCache.ColorSourceCaching = new Dictionary<Color, long>?[SprWorkManager.FileHead.FrameCounts];
                }

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
                DisplayedBitmapSourceCache.DisplayedBitmapSource = this.LoadBitmapFromFile(filePath)?.Also((it) =>
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
                    DisplayedBitmapSourceCache.AnimationSourceCaching = new BitmapSource?[SprWorkManager.FileHead.FrameCounts];
                    DisplayedBitmapSourceCache.ColorSourceCaching = new Dictionary<Color, long>?[SprWorkManager.FileHead.FrameCounts];

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
                        | SPR_FRAME_DATA_CHANGED,
                    currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                    colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                    sprFileHead: SprWorkManager.FileHead,
                    sprFrameData: SprWorkManager.GetFrameData(0)));
                return;
            }


        }

        #endregion

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
                        SprWorkManager.GetFrameData(frameIndex)?
                            .Let((it) => this.GetBitmapFromRGBArray(
                                this.ConvertPaletteColourArrayToByteArray(it.globalFrameData)
                                , SprWorkManager.FileHead.GlobalWidth
                                , SprWorkManager.FileHead.GlobalHeight, PixelFormats.Bgra32))
                            .Also((it) => it.Freeze()));
                    DisplayedBitmapSourceCache.DisplayedBitmapSource = DisplayedBitmapSourceCache.AnimationSourceCaching[frameIndex];

                    NotifyChanged(new BitmapDisplayMangerChangedArg(
                        changedEvent: IS_PLAYING_ANIMATION_CHANGED
                            | CURRENT_DISPLAYING_SOURCE_CHANGED
                            | SPR_FRAME_INDEX_CHANGED
                            | SPR_FRAME_DATA_CHANGED,
                        currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                        isPlayingAnimation: true,
                        sprFrameIndex: frameIndex,
                        sprFrameData: SprWorkManager.GetFrameData(frameIndex)));
                    DisplayedBitmapSourceCache.CurrentFrameIndex++;
                    frameIndex++;
                    if (frameIndex == SprWorkManager.FileHead.FrameCounts)
                    {
                        frameIndex = 0;
                        DisplayedBitmapSourceCache.CurrentFrameIndex = 0;
                    }
                    int delayTime = SprWorkManager.FileHead.Interval - (int)stopwatch.ElapsedMilliseconds;
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
                DisplayedBitmapSourceCache.ColorSourceCaching?
                           .Apply(it => it[frameIndex] = it[frameIndex]
                           .IfNullThenLet(() => DisplayedBitmapSourceCache.DisplayedBitmapSource?
                           .Let(it => this.CountColorsToDictionary(it))));
                DisplayedBitmapSourceCache.DisplayedColorSource = DisplayedBitmapSourceCache.ColorSourceCaching?[frameIndex];
                DisplayedBitmapSourceCache.AnimationTokenSource = null;
                if (frameIndex > 0)
                {
                    DisplayedBitmapSourceCache.CurrentFrameIndex--;
                    frameIndex--;
                }
                NotifyChanged(new BitmapDisplayMangerChangedArg(
                        changedEvent: IS_PLAYING_ANIMATION_CHANGED
                            | CURRENT_DISPLAYING_SOURCE_CHANGED
                            | CURRENT_COLOR_SOURCE_CHANGED
                            | SPR_FRAME_INDEX_CHANGED
                            | SPR_FRAME_DATA_CHANGED,
                        currentDisplayingSource: DisplayedBitmapSourceCache.DisplayedBitmapSource,
                        isPlayingAnimation: false,
                        colorSource: DisplayedBitmapSourceCache.DisplayedColorSource,
                        sprFrameIndex: frameIndex,
                        sprFrameData: SprWorkManager.GetFrameData(frameIndex)));
            });
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
            SPR_FRAME_INDEX_CHANGED = 0b10000,
            SPR_FRAME_DATA_CHANGED = 0b100000,
            SPR_FRAME_OFFSET_CHANGED = 0b1000000,
            SPR_FRAME_SIZE_CHANGED = 0b10000000,
        }

        public ChangedEvent Event { get; private set; }
        public BitmapSource? CurrentDisplayingSource { get; private set; }
        public Dictionary<Color, long>? CurrentColorSource { get; private set; }
        public bool? IsPlayingAnimation { get; private set; }
        public SprFileHead? CurrentSprFileHead { get; private set; }
        public uint SprFrameIndex { get; private set; }
        public FrameRGBA? SprFrameData { get; private set; }

        public BitmapDisplayMangerChangedArg(ChangedEvent changedEvent, BitmapSource? currentDisplayingSource = null,
            Dictionary<Color, long>? colorSource = null,
            SprFileHead? sprFileHead = null,
            bool? isPlayingAnimation = null,
            uint sprFrameIndex = 0,
            FrameRGBA? sprFrameData = null)
        {
            CurrentDisplayingSource = currentDisplayingSource;
            CurrentColorSource = colorSource;
            CurrentSprFileHead = sprFileHead;
            IsPlayingAnimation = isPlayingAnimation;
            SprFrameIndex = sprFrameIndex;
            SprFrameData = sprFrameData;
            Event = changedEvent;
        }
    }
}
