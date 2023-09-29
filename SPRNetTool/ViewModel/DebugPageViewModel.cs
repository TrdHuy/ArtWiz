using SPRNetTool.Data;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.Utils;
using SPRNetTool.ViewModel.Base;
using SPRNetTool.ViewModel.CommandVM;
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

namespace SPRNetTool.ViewModel
{
    public class DebugPageViewModel : BaseViewModel, IDebugPageCommand
    {
        private BitmapSource? _currentDisplayedBitmapSource;
        private ObservableCollection<ColorItemViewModel> _rawOriginalSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<OptimizedColorItemViewModel>? _rawOptimizedSource = null;
        private ObservableCollection<ColorItemViewModel>? _rawResultRGBSource = null;

        private ObservableCollection<ColorItemViewModel> _orignalDisplayedColorSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<OptimizedColorItemViewModel>? _optimizedDisplayedSource = null;
        private ObservableCollection<ColorItemViewModel>? _displayedRgbCompositeResultSource = null;

        private bool _isPlayingAnimation = false;
        private int _pixelWidth = 0;
        private int _pixelHeight = 0;
        private SprFileHead _sprFileHead;
        private FrameRGBA _sprFrameData;
        private uint _currentFrameIndex = 0;
        private bool _isSpr = false;

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
            private set
            {
                _currentDisplayedBitmapSource = value;
                Invalidate();
            }
        }


        public DebugPageViewModel()
        {
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
                                CurrentFrameIndex = castArgs.SprFrameIndex;
                            }, dispatcherPriority);
                        }
                        else if (castArgs.IsPlayingAnimation == false)
                        {
                            SprFrameData = castArgs.SprFrameData ?? SprFrameData;
                            CurrentFrameIndex = castArgs.SprFrameIndex;
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
                        if (castArgs.Event.HasFlag(SPR_FILE_HEAD_CHANGED))
                        {
                            IsSpr = castArgs.CurrentSprFileHead != null;
                            SprFileHead = castArgs.CurrentSprFileHead ?? new SprFileHead();
                        }

                        if (castArgs.Event.HasFlag(CURRENT_DISPLAYING_SOURCE_CHANGED))
                        {
                            CurrentlyDisplayedBitmapSource = castArgs.CurrentDisplayingSource;
                            PixelWidth = castArgs.CurrentDisplayingSource?.PixelWidth ?? 0;
                            PixelHeight = castArgs.CurrentDisplayingSource?.PixelHeight ?? 0;
                        }

                        if (castArgs.Event.HasFlag(SPR_FRAME_INDEX_CHANGED))
                        {
                            CurrentFrameIndex = castArgs.SprFrameIndex;
                        }

                        if (castArgs.Event.HasFlag(CURRENT_COLOR_SOURCE_CHANGED))
                        {
                            await SetColorSource(castArgs.CurrentColorSource);
                        }

                        if (castArgs.Event.HasFlag(SPR_FRAME_DATA_CHANGED))
                        {
                            SprFrameData = castArgs.SprFrameData ?? new FrameRGBA();
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

        private Dictionary<Color, long>? _countableColorSource;
        public void OptimizeImageColor(int colorSize
            , int colorDifferenceDelta
            , bool isUsingAlpha
            , int colorDifferenceDeltaForCalculatingAlpha
            , Color backgroundForBlendColor)
        {
            (_countableColorSource!, CurrentlyDisplayedBitmapSource!).ApplyIfNotNull((it1, it2) =>
            {
                BitmapDisplayManager.OptimzeImageColor(it1
                    , it2
                    , colorSize
                    , colorDifferenceDelta
                    , isUsingAlpha
                    , colorDifferenceDeltaForCalculatingAlpha
                    , backgroundForBlendColor
                    );
            });

        }

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
            SprWorkManager.SaveCurrentWorkToSpr(filePath);
        }

        void IDebugPageCommand.OnIncreaseFrameOffsetXButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            BitmapDisplayManager.SetCurrentlyDisplayedFrameOffset((short)(SprFrameData.frameOffX + (delta == 0 ? 1 : delta)), SprFrameData.frameOffY);
        }

        void IDebugPageCommand.OnDecreaseFrameOffsetXButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            BitmapDisplayManager.SetCurrentlyDisplayedFrameOffset((short)(SprFrameData.frameOffX - (delta == 0 ? 1 : delta)), SprFrameData.frameOffY);
        }
        void IDebugPageCommand.OnIncreaseFrameOffsetYButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            BitmapDisplayManager.SetCurrentlyDisplayedFrameOffset(SprFrameData.frameOffX, (short)(SprFrameData.frameOffY + (delta == 0 ? 1 : delta)));
        }

        void IDebugPageCommand.OnDecreaseFrameOffsetYButtonClicked(uint delta)
        {
            if (!IsSpr) return;

            BitmapDisplayManager.SetCurrentlyDisplayedFrameOffset(SprFrameData.frameOffX, (short)(SprFrameData.frameOffY - (delta == 0 ? 1 : delta)));
        }

        void IDebugPageCommand.OnIncreaseCurrentlyDisplayedSprFrameIndex()
        {
            if (!IsSpr) return;

            if (CurrentFrameIndex < SprFileHead.FrameCounts - 1)
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

        void IDebugPageCommand.OnIncreaseIntervalButtonClicked()
        {
            if (!IsSpr) return;

            if (SprFileHead.Interval < 1000)
            {
                BitmapDisplayManager.SetSprInterval((ushort)(SprFileHead.Interval + 1));
            }
        }
    }
}
