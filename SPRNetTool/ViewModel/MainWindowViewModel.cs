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
        private ObservableCollection<ColorItemViewModel> _rawSource = new ObservableCollection<ColorItemViewModel>();

        private ObservableCollection<ColorItemViewModel> _ColorSource = new ObservableCollection<ColorItemViewModel>();
        private ObservableCollection<ColorItemViewModel>? _optimizedSource = null;

        [Bindable(true)]
        public ObservableCollection<ColorItemViewModel> ColorSource
        {
            get { return _ColorSource; }
            set
            {
                _ColorSource = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public ObservableCollection<ColorItemViewModel>? OptimizedColorSource
        {
            get { return _optimizedSource; }
            set
            {
                _optimizedSource = value;
                Invalidate();
            }
        }

        public MainWindowViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_rawSource, new object());
        }

        public void ResetViewModel()
        {
            _rawSource.Clear();
            _ColorSource.Clear();
            _optimizedSource = null;
            _cachedOrderByCount = null;
            _cachedOrderByDescendingCount = null;
            _cachedOrderByRGB = null;
            InvalidateAll();
        }

        public async Task SetColorSource(Dictionary<Color, long> colorsSource)
        {
            _cachedOrderByCount = null;
            _cachedOrderByDescendingCount = null;
            _rawSource.Clear();
            await Task.Run(() =>
            {
                foreach (var color in colorsSource)
                {
                    var newColor = color.Key;
                    _rawSource.Add<ColorItemViewModel>(new ColorItemViewModel { ItemColor = newColor, Count = color.Value });
                }
            });

            ColorSource = _rawSource;
        }

        public void ResetOrder()
        {
            ColorSource = _rawSource;
        }

        private ObservableCollection<ColorItemViewModel>? _cachedOrderByCount = null;
        private ObservableCollection<ColorItemViewModel>? _cachedOrderByDescendingCount = null;
        public void OrderByCount(bool isSetToDisplaySource = true)
        {
            if (_cachedOrderByCount == null)
                _cachedOrderByCount = _rawSource.OrderBy(x => x.Count).ToIndexableObservableCollection();

            if (isSetToDisplaySource)
                ColorSource = _cachedOrderByCount;
        }
        public ObservableCollection<ColorItemViewModel> OrderByDescendingCount(bool isSetToDisplaySource = true)
        {
            if (_cachedOrderByDescendingCount == null)
                _cachedOrderByDescendingCount = _rawSource.OrderByDescending(x => x.Count).ToIndexableObservableCollection();

            if (isSetToDisplaySource)
                ColorSource = _cachedOrderByDescendingCount;

            return _cachedOrderByDescendingCount;
        }

        private ObservableCollection<ColorItemViewModel>? _cachedOrderByRGB = null;
        public void OrderByRGB()
        {
            if (_cachedOrderByRGB == null)
                _cachedOrderByRGB = _rawSource.OrderBy(x => x.RGBValue).ToIndexableObservableCollection();
            ColorSource = _cachedOrderByRGB;
        }
    }
}
