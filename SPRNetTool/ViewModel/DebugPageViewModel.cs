using SPRNetTool.Data;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.Utils;
using SPRNetTool.View.Pages;
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

namespace SPRNetTool.ViewModel
{
    public class DebugPageViewModel : BaseViewModel, IDebugPageCommand
    {
        private BitmapSource? _currentDisplayingBmpSrc;
        private ObservableCollection<ColorItemViewModel> _rawOriginalSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<OptimizedColorItemViewModel>? _rawOptimizedSource = null;
        private ObservableCollection<ColorItemViewModel>? _rawResultRGBSource = null;

        private ObservableCollection<ColorItemViewModel> _orignalColorSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<OptimizedColorItemViewModel>? _optimizedSource = null;
        private ObservableCollection<ColorItemViewModel>? _resultRGBSource = null;

        private bool _isPlayingAnimation = false;
        private int _pixelWidth = 0;
        private int _pixelHeight = 0;
        private SprFileHead? _sprFileHead = null;
        private int _currentFrame = 0;

        [Bindable(true)]
        public int CurrentFrameIndex
        {
            get 
            { 
                return _currentFrame; 
            }
            set
            {
                _currentFrame = value;
                Invalidate();
            }
        }

    

        [Bindable(true)]
        public SprFileHead? SPRFileHead
        {
            get
            {
                return _sprFileHead;
            }
            set
            {
                _sprFileHead = value;
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
                _pixelWidth = value;
                Invalidate();
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
                _pixelHeight = value;
                Invalidate();
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
                _isPlayingAnimation = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ObservableCollection<ColorItemViewModel> OriginalColorSource
        {
            get { return _orignalColorSource; }
            private set
            {
                _orignalColorSource = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ObservableCollection<ColorItemViewModel>? ResultRGBSource
        {
            get { return _resultRGBSource; }
            private set
            {
                _resultRGBSource = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ObservableCollection<OptimizedColorItemViewModel>? OptimizedColorSource
        {
            get { return _optimizedSource; }
            private set
            {
                _optimizedSource = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public BitmapSource? CurrentDisplayingBmpSrc
        {
            get { return _currentDisplayingBmpSrc; }
            private set
            {
                _currentDisplayingBmpSrc = value;
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
            _orignalColorSource.Clear();
            _rawResultRGBSource = null;
            _resultRGBSource = null;
            _rawOptimizedSource = null;
            _cachedOptimizedOrderByCombinedRGB = null;
            _cachedOptimizedOrderByRGB = null;
            _cachedOrderByCount = null;
            _cachedOrderByDescendingCount = null;
            _cachedOrderByRGB = null;
            _currentDisplayingBmpSrc = null;
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
            switch (args)
            {
                case BitmapDisplayMangerChangedArg castArgs:
                    PixelWidth = castArgs.CurrentDisplayingSource?.PixelWidth ?? 0;
                    PixelHeight = castArgs.CurrentDisplayingSource?.PixelHeight ?? 0;
                    SPRFileHead = castArgs.CurrentSprFileHead;
                    if (castArgs.IsPlayingAnimation != true)
                    {
                        CurrentDisplayingBmpSrc = castArgs.CurrentDisplayingSource;
                        await SetColorSource(castArgs.CurrentColorSource);
                    }
                    else if (castArgs.IsPlayingAnimation == true)
                    {
                        
                        IsPlayingAnimation = true;
                        ViewModelOwner?.ViewDispatcher.Invoke(() =>
                        {
                            CurrentDisplayingBmpSrc = castArgs.CurrentDisplayingSource;
                            CurrentFrameIndex = castArgs.FrameIndex;
                        }, DispatcherPriority.DataBind);
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
            (_countableColorSource!, CurrentDisplayingBmpSrc!).ApplyIfNotNull((it1, it2) =>
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
            if (!IsPlayingAnimation)
            {
                BitmapDisplayManager.StartSprAnimation();
            }
            else
            {
                BitmapDisplayManager.StopSprAnimation();
            }
            IsPlayingAnimation = !IsPlayingAnimation;
        }
    }
}
