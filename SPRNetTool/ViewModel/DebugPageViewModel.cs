using SPRNetTool.Data;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.Utils;
using SPRNetTool.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SPRNetTool.ViewModel
{
    public class DebugPageViewModel : BaseViewModel
    {
        private BitmapSource? _currentDisplayingBmpSrc;
        private ObservableCollection<ColorItemViewModel> _rawOriginalSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<OptimizedColorItemViewModel>? _rawOptimizedSource = null;
        private ObservableCollection<ColorItemViewModel>? _rawResultRGBSource = null;

        private ObservableCollection<ColorItemViewModel> _orignalColorSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<OptimizedColorItemViewModel>? _optimizedSource = null;
        private ObservableCollection<ColorItemViewModel>? _resultRGBSource = null;

        private int _pixelWidth = 0;
        private int _pixelHeight = 0;
        private ushort _globleWidth = 0;
        private ushort _globleHeight = 0;
        private ushort _offX = 0;
        private ushort _offY = 0;
        private ushort _frameCounts = 0;
        private ushort _colourCounts = 0;
        private ushort _directionCount = 0;
        private ushort _interval = 0;

       

        [Bindable(true)]
        public ushort GlobleWidth
        {
            get
            {
                return _globleWidth;
            }
            set
            {
                _globleWidth = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ushort GlobleHeight
        {
            get
            {
                return _globleHeight;
            }
            set
            {
                _globleHeight = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ushort OffX
        {
            get
            {
                return _offX;
            }
            set
            {
                _offX = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ushort OffY
        {
            get
            {
                return _offY;
            }
            set
            {
                _offY = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ushort FrameCounts
        {
            get
            {
                return _frameCounts;
            }
            set
            {
                _frameCounts = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ushort ColourCounts
        {
            get
            {
                return _colourCounts;
            }
            set
            {
                _colourCounts = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ushort DirectionCount
        {
            get
            {
                return _directionCount;
            }
            set
            {
                _directionCount = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ushort Interval
        {
            get
            {
                return _interval;
            }
            set
            {
                _interval = value;
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
                    CurrentDisplayingBmpSrc = castArgs.CurrentDisplayingSource;
                    await SetColorSource(castArgs.CurrentColorSource);
                    PixelWidth = CurrentDisplayingBmpSrc?.PixelWidth ?? 0;
                    PixelHeight = CurrentDisplayingBmpSrc?.PixelHeight ?? 0;
                    GlobleWidth = castArgs.CurrentSprFileHead?.GlobleWidth ?? 0;
                    GlobleHeight = castArgs.CurrentSprFileHead?.GlobleHeight ?? 0;
                    OffX = castArgs.CurrentSprFileHead?.OffX ?? 0;
                    OffY = castArgs.CurrentSprFileHead?.OffY ?? 0;
                    FrameCounts = castArgs.CurrentSprFileHead?.FrameCounts ?? 0;
                    ColourCounts = castArgs.CurrentSprFileHead?.ColourCounts ?? 0;
                    DirectionCount = castArgs.CurrentSprFileHead?.DirectionCount ?? 0;
                    Interval = castArgs.CurrentSprFileHead?.Interval ?? 0;
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
    }
}
