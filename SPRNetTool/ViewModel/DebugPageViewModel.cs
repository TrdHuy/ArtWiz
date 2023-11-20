using SPRNetTool.Data;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.LogUtil;
using SPRNetTool.Utils;
using SPRNetTool.ViewModel.Base;
using SPRNetTool.ViewModel.CommandVM;
using SPRNetTool.ViewModel.Widgets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static SPRNetTool.Domain.BitmapDisplayMangerChangedArg.ChangedEvent;
using static SPRNetTool.Domain.SprFrameCollectionChangedArg.ChangedEvent;

namespace SPRNetTool.ViewModel
{
    public class DebugPageViewModel : BaseViewModel, IDebugPageCommand
    {
        private BitmapSource? _currentDisplayedBitmapSource;
        private BitmapSource? _currentDisplayedOptimizedBitmapSource;
        private ObservableCollection<ColorItemViewModel> _rawOriginalSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<OptimizedColorItemViewModel>? _rawOptimizedSource = null;
        private ObservableCollection<ColorItemViewModel>? _rawResultRGBSource = null;

        private ObservableCollection<ColorItemViewModel> _orignalDisplayedColorSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<OptimizedColorItemViewModel>? _optimizedDisplayedSource = null;
        private ObservableCollection<ColorItemViewModel>? _displayedRgbCompositeResultSource = null;
        private CustomObservableCollection<IFrameViewModel>? _framesSource;
        private ObservableCollection<IPaletteEditorColorItemViewModel>? _paletteColorItemSource;

        private bool _isPlayingAnimation = false;
        private int _pixelWidth = 0;
        private int _pixelHeight = 0;
        private SprFileHead _sprFileHead;
        private FrameRGBA _sprFrameData;
        private uint _currentFrameIndex = 0;
        private bool _isSpr = false;
        private IBitmapViewerViewModel bitmapViewerVM;

        [Bindable(true)]
        public IBitmapViewerViewModel BitmapViewerVM
        {
            get => bitmapViewerVM;
        }

        [Bindable(true)]
        public CustomObservableCollection<IFrameViewModel>? FramesSource
        {
            get
            {
                return _framesSource;
            }
            set
            {
                if (_framesSource != value)
                {
                    _framesSource = value;
                    Invalidate();
                }
            }
        }

        [Bindable(true)]
        public ObservableCollection<IPaletteEditorColorItemViewModel>? PaletteColorItemSource
        {
            get
            {
                return _paletteColorItemSource;
            }
            set
            {
                if (_paletteColorItemSource != value)
                {
                    _paletteColorItemSource = value;
                    Invalidate();
                }
            }
        }

        [Bindable(true)]
        public uint CurrentFrameIndex
        {
            get
            {
                return _currentFrameIndex;
            }
            set
            {
                if (_currentFrameIndex != value)
                {
                    _currentFrameIndex = value;
                    Invalidate();
                }
            }
        }

        [Bindable(true)]
        public bool IsSpr
        {
            get
            {
                return _isSpr;
            }
            set
            {
                if (_isSpr != value)
                {
                    _isSpr = value;
                    Invalidate();
                }
            }
        }


        [Bindable(true)]
        public SprFileHead SprFileHead
        {
            get
            {
                return _sprFileHead;
            }
            private set
            {
                _sprFileHead = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public FrameRGBA SprFrameData
        {
            get
            {
                return _sprFrameData;
            }
            private set
            {
                _sprFrameData = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public int PixelWidth
        {
            get
            {
                return _pixelWidth;
            }
            set
            {
                if (_pixelWidth != value)
                {
                    _pixelWidth = value;
                    Invalidate();
                }
            }
        }
        [Bindable(true)]
        public int PixelHeight
        {
            get
            {
                return _pixelHeight;
            }
            set
            {
                if (_pixelHeight != value)
                {
                    _pixelHeight = value;
                    Invalidate();
                }
            }
        }

        [Bindable(true)]
        public bool IsPlayingAnimation
        {
            get
            {
                return _isPlayingAnimation;
            }
            set
            {
                if (_isPlayingAnimation != value)
                {
                    _isPlayingAnimation = value;
                    Invalidate();
                }

            }
        }

        [Bindable(true)]
        public ObservableCollection<ColorItemViewModel> OriginalColorSource
        {
            get { return _orignalDisplayedColorSource; }
            private set
            {
                _orignalDisplayedColorSource = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ObservableCollection<ColorItemViewModel>? ResultRGBSource
        {
            get { return _displayedRgbCompositeResultSource; }
            private set
            {
                _displayedRgbCompositeResultSource = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ObservableCollection<OptimizedColorItemViewModel>? OptimizedColorSource
        {
            get { return _optimizedDisplayedSource; }
            private set
            {
                _optimizedDisplayedSource = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public BitmapSource? CurrentlyDisplayedBitmapSource
        {

            get { return _currentDisplayedBitmapSource; }
            set
            {
                _currentDisplayedBitmapSource = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public BitmapSource? CurrentlyDisplayedOptimizedBitmapSource
        {

            get { return _currentDisplayedOptimizedBitmapSource; }
            private set
            {
                _currentDisplayedOptimizedBitmapSource = value;
                Invalidate();
            }
        }



        public DebugPageViewModel()
        {
            bitmapViewerVM = new BitmapViewerViewModel();
            BindingOperations.EnableCollectionSynchronization(_rawOriginalSource, new object());
            BitmapDisplayManager.RegisterObserver(this);
        }

        ~DebugPageViewModel()
        {
            BitmapDisplayManager.UnregisterObserver(this);
        }

        public void ResetViewModel()
        {
            _rawOriginalSource.Clear();
            _orignalDisplayedColorSource.Clear();
            _rawResultRGBSource = null;
            _displayedRgbCompositeResultSource = null;
            _rawOptimizedSource = null;
            _cachedOptimizedOrderByCombinedRGB = null;
            _cachedOptimizedOrderByRGB = null;
            _cachedOrderByCount = null;
            _cachedOrderByDescendingCount = null;
            _cachedOrderByRGB = null;
            _currentDisplayedBitmapSource = null;
            InvalidateAll();
        }

        #region OriginalSource
        public async Task SetColorSource(Dictionary<Color, long>? colorsSource)
        {
            _countableColorSource = colorsSource;
            _cachedOrderByCount = null;
            _cachedOrderByDescendingCount = null;
            _cachedOrderByRGB = null;
            _rawOriginalSource.Clear();

            if (colorsSource != null)
            {
                await Task.Run(() =>
                {
                    foreach (var color in colorsSource)
                    {
                        var newColor = color.Key;
                        _rawOriginalSource.Add<ColorItemViewModel>(new ColorItemViewModel { ItemColor = newColor, Count = color.Value });
                    }
                });
            }
            OriginalColorSource = _rawOriginalSource;
        }

        public void ResetOrder()
        {
            OriginalColorSource = _rawOriginalSource;
        }

        private ObservableCollection<ColorItemViewModel>? _cachedOrderByCount = null;
        private ObservableCollection<ColorItemViewModel>? _cachedOrderByDescendingCount = null;
        public void OrderByCount(bool isSetToDisplaySource = true)
        {
            if (_cachedOrderByCount == null)
                _cachedOrderByCount = _rawOriginalSource.OrderBy(x => x.Count).ToIndexableObservableCollection();

            if (isSetToDisplaySource)
                OriginalColorSource = _cachedOrderByCount;
        }
        public ObservableCollection<ColorItemViewModel> OrderByDescendingCount(bool isSetToDisplaySource = true)
        {
            if (_cachedOrderByDescendingCount == null)
                _cachedOrderByDescendingCount = _rawOriginalSource.OrderByDescending(x => x.Count).ToIndexableObservableCollection();

            if (isSetToDisplaySource)
                OriginalColorSource = _cachedOrderByDescendingCount;

            return _cachedOrderByDescendingCount;
        }

        private ObservableCollection<ColorItemViewModel>? _cachedOrderByRGB = null;
        public void OrderByRGB()
        {
            if (_cachedOrderByRGB == null)
                _cachedOrderByRGB = _rawOriginalSource.OrderBy(x => x.RGBValue).ToIndexableObservableCollection();
            OriginalColorSource = _cachedOrderByRGB;
        }
        #endregion

        #region OptimizedColorSource 
        public void SetOptimizedColorSource(ObservableCollection<OptimizedColorItemViewModel> optimizedSource)
        {
            _cachedOptimizedOrderByRGB = null;
            _cachedOptimizedOrderByCombinedRGB = null;
            _cachedOptimizedOrderByARGB = null;
            _rawOptimizedSource = optimizedSource;
            OptimizedColorSource = optimizedSource;
        }

        public void ResetOptimizedOrder()
        {
            _rawOptimizedSource?.ReIndexObservableCollection();
            OptimizedColorSource = _rawOptimizedSource;
        }
        private ObservableCollection<OptimizedColorItemViewModel>? _cachedOptimizedOrderByRGB = null;
        public void OptimizedOrderByRGB()
        {
            if (_cachedOptimizedOrderByRGB == null)
                _cachedOptimizedOrderByRGB = _rawOptimizedSource?.OrderBy(x => x.RGBValue).ToIndexableObservableCollection();
            _cachedOptimizedOrderByRGB?.ReIndexObservableCollection();
            OptimizedColorSource = _cachedOptimizedOrderByRGB;
        }

        private ObservableCollection<OptimizedColorItemViewModel>? _cachedOptimizedOrderByARGB = null;
        public void OptimizedOrderByARGB()
        {
            if (_cachedOptimizedOrderByARGB == null)
                _cachedOptimizedOrderByARGB = _rawOptimizedSource?.OrderBy(x => x.RGBAValue).ToIndexableObservableCollection();
            _cachedOptimizedOrderByARGB?.ReIndexObservableCollection();
            OptimizedColorSource = _cachedOptimizedOrderByARGB;
        }

        private ObservableCollection<OptimizedColorItemViewModel>? _cachedOptimizedOrderByCombinedRGB = null;
        public void OptimizedOrderByCombinedRGB()
        {
            if (_cachedOptimizedOrderByCombinedRGB == null)
                _cachedOptimizedOrderByCombinedRGB = _rawOptimizedSource?.OrderBy(x => x.CombinedRGBValue).ToIndexableObservableCollection();
            _cachedOptimizedOrderByCombinedRGB?.ReIndexObservableCollection();
            OptimizedColorSource = _cachedOptimizedOrderByCombinedRGB;
        }

        #endregion

        #region ResultRGBSource
        public void SetResultRGBColorSource(ObservableCollection<ColorItemViewModel> resultRGBColorSource)
        {
            _rawResultRGBSource = resultRGBColorSource;
            ResultRGBSource = resultRGBColorSource;
        }

        public void ResetResultRGBColorSourceOrder()
        {
            ResultRGBSource = _rawResultRGBSource;
        }
        #endregion

        protected async override void OnDomainChanged(IDomainChangedArgs args)
        {
            if (IsViewModelDestroyed) return;

            switch (args)
            {
                case BitmapDisplayMangerChangedArg castArgs:
                    if (castArgs.Event.HasFlag(IS_PLAYING_ANIMATION_CHANGED))
                    {
                        if (castArgs.IsPlayingAnimation == true)
                        {
                            var dispatcherPriority = DispatcherPriority.Background;
                            if (SprFileHead.Interval > 20)
                            {
                                dispatcherPriority = DispatcherPriority.Render;
                            }

                            if (IsViewModelDestroyed) return;

                            ViewModelOwner?.ViewDispatcher.Invoke(() =>
                            {
                                IsPlayingAnimation = true;
                                CurrentlyDisplayedBitmapSource = castArgs.CurrentDisplayingSource;
                                CurrentFrameIndex = castArgs.CurrentDisplayingFrameIndex;
                            }, dispatcherPriority);
                        }
                        else if (castArgs.IsPlayingAnimation == false)
                        {
                            SprFrameData = castArgs.SprFrameData ?? SprFrameData;
                            CurrentFrameIndex = castArgs.CurrentDisplayingFrameIndex;
                            IsPlayingAnimation = false;
                            CurrentlyDisplayedBitmapSource = castArgs.CurrentDisplayingSource;

                            if (castArgs.CurrentColorSource != null)
                            {
                                await SetColorSource(castArgs.CurrentColorSource);
                            }
                        }
                    }
                    else
                    {
                        if (castArgs.Event.HasFlag(SPR_FILE_PALETTE_CHANGED))
                        {
                            castArgs.PaletteData?.Apply(it =>
                            {
                                ViewModelOwner?.ViewDispatcher.Invoke(() =>
                                {
                                    PaletteColorItemSource = new ObservableCollection<IPaletteEditorColorItemViewModel>();
                                    foreach (var pColor in it.Data)
                                    {
                                        PaletteColorItemSource.Add(new PaletteEditorColorItemViewModel(
                                           new SolidColorBrush(Color.FromRgb(pColor.Red,
                                           pColor.Green, pColor.Blue))));
                                    }
                                });

                            });
                        }

                        if (castArgs.Event.HasFlag(SPR_FILE_HEAD_CHANGED))
                        {
                            IsSpr = castArgs.CurrentSprFileHead != null;
                            castArgs.CurrentSprFileHead?.Apply(it => SprFileHead = it.modifiedSprFileHeadCache.ToSprFileHead());
                        }

                        if (castArgs.Event.HasFlag(CURRENT_DISPLAYING_SOURCE_CHANGED))
                        {
                            CurrentlyDisplayedBitmapSource = castArgs.CurrentDisplayingSource;
                            PixelWidth = castArgs.CurrentDisplayingSource?.PixelWidth ?? 0;
                            PixelHeight = castArgs.CurrentDisplayingSource?.PixelHeight ?? 0;
                        }

                        if (castArgs.Event.HasFlag(CURRENT_DISPLAYING_FRAME_INDEX_CHANGED))
                        {
                            CurrentFrameIndex = castArgs.CurrentDisplayingFrameIndex;
                        }

                        if (castArgs.Event.HasFlag(CURRENT_COLOR_SOURCE_CHANGED))
                        {
                            await SetColorSource(castArgs.CurrentColorSource);
                        }

                        if (castArgs.Event.HasFlag(SPR_FRAME_DATA_CHANGED))
                        {
                            SprFrameData = castArgs.SprFrameData?.modifiedFrameRGBACache?.toFrameRGBA() ?? new FrameRGBA();
                        }

                        if (castArgs.Event.HasFlag(SPR_FRAME_COLLECTION_CHANGED))
                        {
                            var collectionChangedArg = castArgs.SprFrameCollectionChangedArg;
                            if (collectionChangedArg == null)
                            {
                                break;
                            }

                            if (collectionChangedArg.Event.HasFlag(TOTAL_FRAME_COUNT_CHANGED))
                            {
                                var newSrc = new CustomObservableCollection<IFrameViewModel>();
                                for (int i = 0; i < collectionChangedArg.FrameCount; i++)
                                {
                                    newSrc.Add(new FrameViewModel());
                                }
                                FramesSource = newSrc;
                            }

                            if (collectionChangedArg.Event.HasFlag(FRAME_SWITCHED) && FramesSource != null)
                            {
                                FramesSource.SwitchItem((int)collectionChangedArg.FrameSwitched1Index, (int)collectionChangedArg.FrameSwitched2Index);
                            }

                            if (collectionChangedArg.Event.HasFlag(FRAME_REMOVED) && FramesSource != null)
                            {
                                FramesSource.RemoveAt(collectionChangedArg.OldFrameIndex);
                            }

                            if (collectionChangedArg.Event.HasFlag(FRAME_INSERTED) && FramesSource != null)
                            {
                                ViewModelOwner?.ViewDispatcher.Invoke(() =>
                                {
                                    FramesSource.Insert(collectionChangedArg.NewFrameIndex, new FrameViewModel());
                                });
                            }
                        }
                    }
                    break;
            }
        }

        public async Task OpenImageFromFileAsync(string filePath)
        {
            await Task.Run(() =>
            {
                BitmapDisplayManager.OpenBitmapFromFile(filePath, true);
            });

        }

        public Dictionary<Color, long>? _countableColorSource;

        public void OptimizeImageColor(int colorSize
            , int colorDifferenceDelta
            , bool isUsingAlpha
            , int colorDifferenceDeltaForCalculatingAlpha
            , Color backgroundForBlendColor)
        {
            (_countableColorSource!, CurrentlyDisplayedBitmapSource!).ApplyIfNotNull((colorSource, bitmapSource) =>
            {
                var scr = colorSource.Select(kp => (kp.Key, kp.Value)).ToList();
                var optimizedBitmap = BitmapDisplayManager.OptimzeImageColor(scr
                     , bitmapSource
                     , colorSize
                     , colorDifferenceDelta
                     , isUsingAlpha
                     , colorDifferenceDeltaForCalculatingAlpha
                     , backgroundForBlendColor
                     , out List<Color> selectedColors
                     , out List<Color> selectedAlphaColors
                     , out List<Color> expectedRGBColors);
                var selectedList = new ObservableCollection<OptimizedColorItemViewModel>();

                int i = 0;
                foreach (var color in selectedColors)
                {
                    if (i < colorSize)
                    {
                        selectedList.Add<OptimizedColorItemViewModel>(new OptimizedColorItemViewModel()
                        {
                            ItemColor = color
                        });
                    }
                    else
                    {
                        selectedList.Add<OptimizedColorItemViewModel>(new OptimizedColorItemViewModel()
                        {
                            ItemColor = selectedAlphaColors[i - colorSize],
                            ExpectedColor = expectedRGBColors[i - colorSize]
                        }); ;
                    }
                    i++;
                }
                SetOptimizedColorSource(selectedList);

                CurrentlyDisplayedOptimizedBitmapSource = optimizedBitmap?.Also(it =>
                {
                    BitmapDisplayManager.CountBitmapColors(it).Apply(it =>
                    {
                        var newCountedSrc = new ObservableCollection<ColorItemViewModel>();
                        foreach (var color in it)
                        {
                            var newColor = color.Key;
                            newCountedSrc.Add<ColorItemViewModel>(new ColorItemViewModel { ItemColor = newColor, Count = color.Value });
                        }
                        SetResultRGBColorSource(newCountedSrc);
                    });
                });
            });
        }


        #region Command region
        void IDebugPageCommand.OnPlayPauseAnimationSprClicked()
        {
            if (!IsSpr) return;

            if (!IsPlayingAnimation)
            {
                BitmapDisplayManager.StartSprAnimation();
            }
            else
            {
                BitmapDisplayManager.StopSprAnimation();
            }
        }

        void IDebugPageCommand.OnSaveCurrentWorkManagerToFileSprClicked(string filePath)
        {
            if (!IsSpr) return;
            SprWorkManager.SaveCurrentWorkToSpr(filePath, true);
        }

        #region Change frame offset command
        private TaskPool ModifySizeAndOffsetTaskPool = new TaskPool(cores: 1);
        void IDebugPageCommand.OnIncreaseFrameWidthButtonClicked(uint delta)
        {
            if (!IsSpr) return;
            SetFrameSize((int)delta, 0);

        }

        void IDebugPageCommand.OnDecreaseFrameWidthButtonClicked(uint delta)
        {
            if (!IsSpr) return;
            SetFrameSize(-(int)delta, 0);
        }

        void IDebugPageCommand.OnIncreaseFrameHeightButtonClicked(uint delta)
        {
            if (!IsSpr) return;
            SetFrameSize(0, (int)delta);

        }

        void IDebugPageCommand.OnDecreaseFrameHeightButtonClicked(uint delta)
        {
            if (!IsSpr) return;
            SetFrameSize(0, -(int)delta);
        }

        void IDebugPageCommand.OnIncreaseFrameOffsetXButtonClicked(uint delta)
        {
            if (!IsSpr) return;
            SetFrameOffset((int)delta, 0);
        }

        void IDebugPageCommand.OnDecreaseFrameOffsetXButtonClicked(uint delta)
        {
            if (!IsSpr) return;
            SetFrameOffset(-(int)delta, 0);
        }
        void IDebugPageCommand.OnIncreaseFrameOffsetYButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            SetFrameOffset(0, (int)delta);

        }

        void IDebugPageCommand.OnDecreaseFrameOffsetYButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            SetFrameOffset(0, -(int)delta);
        }
        void IDebugPageCommand.OnIncreaseSprGlobalOffsetXButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            SetGlobalOffset((int)delta, 0);
        }
        void IDebugPageCommand.OnDecreaseSprGlobalOffsetXButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            SetGlobalOffset(-(int)delta, 0);
        }
        void IDebugPageCommand.OnIncreaseSprGlobalOffsetYButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            SetGlobalOffset(0, (int)delta);
        }
        void IDebugPageCommand.OnDecreaseSprGlobalOffsetYButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            SetGlobalOffset(0, -(int)delta);
        }
        void IDebugPageCommand.OnIncreaseSprGlobalWidthButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            SetGlobalSize((int)delta, 0);
        }
        void IDebugPageCommand.OnDecreaseSprGlobalWidthButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            SetGlobalSize(-(int)delta, 0);
        }
        void IDebugPageCommand.OnIncreaseSprGlobalHeightButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            SetGlobalSize(0, (int)delta);
        }
        void IDebugPageCommand.OnDecreaseSprGlobalHeightButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            SetGlobalSize(0, -(int)delta);
        }

        void IDebugPageCommand.SetSprGlobalSize(ushort? width, ushort? height)
        {
            if (!IsSpr) return;

            var nW = width ?? SprFileHead.GlobalWidth;
            var nH = height ?? SprFileHead.GlobalHeight;

            SetGlobalSize(nW - SprFileHead.GlobalWidth, nH - SprFileHead.GlobalHeight);
        }

        void IDebugPageCommand.SetSprGlobalOffset(short? offX, short? offY)
        {
            if (!IsSpr) return;

            var nOffX = offX ?? SprFileHead.OffX;
            var nOffY = offY ?? SprFileHead.OffY;

            SetGlobalOffset(nOffX - SprFileHead.OffX, nOffY - SprFileHead.OffY);
        }

        void IDebugPageCommand.SetSprFrameSize(ushort? width, ushort? height)
        {
            if (!IsSpr) return;

            var nW = width ?? SprFrameData.frameWidth;
            var nH = height ?? SprFrameData.frameHeight;

            SetFrameSize(nW - SprFrameData.frameWidth, nH - SprFrameData.frameHeight);
        }

        void IDebugPageCommand.SetSprFrameOffset(short? offX, short? offY)
        {
            if (!IsSpr) return;

            var nOffX = offX ?? SprFrameData.frameOffX;
            var nOffY = offY ?? SprFrameData.frameOffY;

            SetFrameOffset(nOffX - SprFrameData.frameOffX, nOffY - SprFrameData.frameOffY);
        }

        private void SetFrameOffset(int deltaX, int deltaY)
        {
            var newOffX = (short)(SprFrameData.modifiedFrameRGBACache.frameOffX + deltaX);
            var newOffY = (short)(SprFrameData.modifiedFrameRGBACache.frameOffY + deltaY);
            BitmapDisplayManager.SetCurrentlyDisplayedFrameOffset(newOffX, newOffY);
        }

        private void SetGlobalOffset(int deltaX, int deltaY)
        {
            var newOffX = (short)(SprFileHead.modifiedSprFileHeadCache.offX + deltaX);
            var newOffY = (short)(SprFileHead.modifiedSprFileHeadCache.offY + deltaY);
            BitmapDisplayManager.SetSprGlobalOffset(newOffX, newOffY);
        }

        private void SetGlobalSize(int deltaWidth, int deltaHeight)
        {
            var tempWidth = SprFileHead.modifiedSprFileHeadCache.globalWidth + deltaWidth;
            var tempHeight = SprFileHead.modifiedSprFileHeadCache.globalHeight + deltaHeight;
            var newWidth = (ushort)(tempWidth < 0 ? 0 : tempWidth);
            var newHeight = (ushort)(tempHeight < 0 ? 0 : tempHeight);
            BitmapDisplayManager.SetSprGlobalSize(newWidth, newHeight);
        }

        private void SetFrameSize(int deltaWidth, int deltaHeight)
        {
            var tempWidth = SprFrameData.modifiedFrameRGBACache.frameWidth + deltaWidth;
            var tempHeight = SprFrameData.modifiedFrameRGBACache.frameHeight + deltaHeight;
            var newWidth = (ushort)(tempWidth < 0 ? 0 : tempWidth);
            var newHeight = (ushort)(tempHeight < 0 ? 0 : tempHeight);
            BitmapDisplayManager.SetCurrentlyDisplayedFrameSize(newWidth, newHeight);
        }
        #endregion

        void IDebugPageCommand.OnIncreaseCurrentlyDisplayedSprFrameIndex()
        {
            if (!IsSpr) return;

            if (CurrentFrameIndex < SprFileHead.modifiedSprFileHeadCache.FrameCounts - 1)
            {
                BitmapDisplayManager.SetCurrentlyDisplayedSprFrameIndex(CurrentFrameIndex + 1);
            }
            else
            {
                BitmapDisplayManager.SetCurrentlyDisplayedSprFrameIndex(0);
            }
        }

        void IDebugPageCommand.OnDecreaseCurrentlyDisplayedSprFrameIndex()
        {
            if (CurrentFrameIndex >= 1)
            {
                BitmapDisplayManager.SetCurrentlyDisplayedSprFrameIndex(CurrentFrameIndex - 1);
            }
            else
            {
                BitmapDisplayManager.SetCurrentlyDisplayedSprFrameIndex((uint)SprFileHead.FrameCounts - 1);
            }
        }

        void IDebugPageCommand.OnDecreaseIntervalButtonClicked()
        {
            if (!IsSpr) return;

            if (SprFileHead.Interval > 0)
            {
                BitmapDisplayManager.SetSprInterval((ushort)(SprFileHead.Interval - 1));
            }
        }

        void IDebugPageCommand.SetSprInterval(ushort interval)
        {
            if (!IsSpr) return;

            BitmapDisplayManager.SetSprInterval(interval);
        }

        void IDebugPageCommand.OnIncreaseIntervalButtonClicked()
        {
            if (!IsSpr) return;

            if (SprFileHead.Interval < 1000)
            {
                BitmapDisplayManager.SetSprInterval((ushort)(SprFileHead.Interval + 1));
            }
        }

        void IDebugPageCommand.OnSaveCurrentDisplayedBitmapSourceToSpr(string filePath)
        {
            if (IsSpr) return;
            CurrentlyDisplayedBitmapSource?.Apply(it =>
            {
                BitmapDisplayManager.OptimzeImageColorNA256(it)?.Apply(it =>
                {
                    SprWorkManager.SaveBitmapSourceToSprFile(it, filePath);
                });
            });
        }

        void IDebugPageCommand.OnImportCurrentDisplaySourceToNextFrameOfSprWorkSpace()
        {
            if (CurrentlyDisplayedBitmapSource != null)
            {
                if (SprFileHead.VersionInfo == null)
                {
                    BitmapDisplayManager.InsertFrame(0, CurrentlyDisplayedBitmapSource);
                }
                else
                {
                    BitmapDisplayManager.InsertFrame(SprFileHead.modifiedSprFileHeadCache.FrameCounts, CurrentlyDisplayedBitmapSource);
                }
                BitmapDisplayManager.ChangeCurrentDisplayMode(isSpr: true);
            }
        }

        bool IDebugPageCommand.OnSwitchFrameClicked(uint frameIndex1, uint frameIndex2)
        {
            return BitmapDisplayManager.SwitchFrame(frameIndex1, frameIndex2);
        }

        bool IDebugPageCommand.OnRemoveFrameClicked(uint frameIndex)
        {
            return BitmapDisplayManager.DeleteFrame(frameIndex);
        }

        bool IDebugPageCommand.OnInsertFrameClicked(uint frameIndex, string filePath)
        {
            return BitmapDisplayManager.InsertFrame(frameIndex, filePath);
        }

        void IDebugPageCommand.OnFramePointerClick(uint frameIndex)
        {
            if (!IsSpr) return;

            if (CurrentFrameIndex != frameIndex && frameIndex < SprFileHead.FrameCounts)
            {
                BitmapDisplayManager.SetCurrentlyDisplayedSprFrameIndex(frameIndex);
            }
        }
        #endregion


        public override void OnDestroy()
        {
            base.OnDestroy();
            bitmapViewerVM.OnDestroy();
        }
    }

    public class FrameViewModel : IFrameViewModel
    {
    }

    public class PaletteEditorColorItemViewModel : BaseViewModel, IPaletteEditorColorItemViewModel
    {
        private SolidColorBrush mBrush;
        public PaletteEditorColorItemViewModel(SolidColorBrush brush)
        {
            mBrush = brush;
        }

        public SolidColorBrush ColorBrush
        {
            get => mBrush;
            set
            {
                mBrush = value;
                Invalidate();
            }
        }
    }
}
