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

namespace SPRNetTool.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ObservableCollection<ColorItemViewModel> _rawOriginalSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<OptimizedColorItemViewModel>? _rawOptimizedSource = null;
        private ObservableCollection<ColorItemViewModel>? _rawResultRGBSource = null;

        private ObservableCollection<ColorItemViewModel> _orignalColorSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<OptimizedColorItemViewModel>? _optimizedSource = null;
        private ObservableCollection<ColorItemViewModel>? _resultRGBSource = null;

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

        public MainWindowViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_rawOriginalSource, new object());
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
            InvalidateAll();
        }

        #region OriginalSource
        public async Task SetColorSource(Dictionary<Color, long> colorsSource)
        {
            _cachedOrderByCount = null;
            _cachedOrderByDescendingCount = null;
            _cachedOrderByRGB = null;
            _rawOriginalSource.Clear();
            await Task.Run(() =>
            {
                foreach (var color in colorsSource)
                {
                    var newColor = color.Key;
                    _rawOriginalSource.Add<ColorItemViewModel>(new ColorItemViewModel { ItemColor = newColor, Count = color.Value });
                }
            });

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


        public void SetResultRGBColorSource(ObservableCollection<ColorItemViewModel> resultRGBColorSource)
        {
            _rawResultRGBSource = resultRGBColorSource;
            ResultRGBSource = resultRGBColorSource;
        }

        public void ResetResultRGBColorSourceOrder()
        {
            ResultRGBSource = _rawResultRGBSource;
        }
    }
}
