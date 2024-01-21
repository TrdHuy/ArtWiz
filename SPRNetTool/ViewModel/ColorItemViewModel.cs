using ArtWiz.Domain.Base;
using ArtWiz.Utils;
using ArtWiz.ViewModel.Base;
using System.Windows.Media;

namespace ArtWiz.ViewModel
{

    public class ColorItemViewModel : BaseViewModel, IIndexableViewModel
    {

        // TODO: dynamic this
        private Color _blendedBGColor = Colors.White;

        public ColorItemViewModel()
        {

        }
        public ColorItemViewModel(Color blendedBGColor)
        {
            _blendedBGColor = blendedBGColor;
        }

        private Color _itemColor;
        private long _count;
        private long _index;
        private Color _combinedColor;
        public Color ItemColor
        {
            get
            {
                return _itemColor;
            }
            set
            {

                _itemColor = value;
                if (_itemColor.A < 255)
                {
                    _combinedColor = _itemColor.BlendColors(_blendedBGColor);
                }
                else
                {
                    _combinedColor = _itemColor;
                }

                Invalidate();
                Invalidate(nameof(CombinedRGBValue));
                Invalidate(nameof(CombinedColorValue));
                Invalidate(nameof(RGBValue));
                Invalidate(nameof(RGBAValue));
                Invalidate(nameof(ColorValue));
            }
        }

        public Color CombinedColor
        {
            get
            {
                return _combinedColor;
            }
        }

        public long Count
        {
            get
            {
                return _count;
            }
            set
            {

                _count = value;
                Invalidate();
            }
        }

        public SolidColorBrush ColorValue
        {
            get
            {
                return new SolidColorBrush(_itemColor);
            }
        }

        public SolidColorBrush CombinedColorValue
        {
            get
            {
                return new SolidColorBrush(_combinedColor);
            }
        }

        public string RGBValue
        {
            get
            {
                return $"{_itemColor.R},{_itemColor.G},{_itemColor.B}";
            }
        }

        public string RGBAValue
        {
            get
            {
                return $"{_itemColor.R},{_itemColor.G},{_itemColor.B},{_itemColor.A}";
            }
        }

        public string CombinedRGBValue
        {
            get
            {
                return $"{_combinedColor.R},{_combinedColor.G},{_combinedColor.B}";
            }
        }

        public long Index
        {
            get => _index; set
            {
                _index = value;
                Invalidate();
            }
        }

        protected override void OnDomainChanged(IDomainChangedArgs args)
        {
        }
    }

    public class OptimizedColorItemViewModel : ColorItemViewModel
    {
        public OptimizedColorItemViewModel() : base() { }
        public OptimizedColorItemViewModel(Color blendedBGColor) : base(blendedBGColor) { }

        private Color _expectedColor;
        private bool _isEmpty;
        public Color ExpectedColor
        {
            get
            {
                return _expectedColor;
            }
            set
            {
                _expectedColor = value;
                _isEmpty = true;
                Invalidate();
                Invalidate(nameof(IsEmpty));
            }
        }

        public bool IsEmpty
        {
            get
            {
                return _isEmpty;
            }
        }

        public SolidColorBrush ExpectedColorValue
        {
            get
            {
                return new SolidColorBrush(_expectedColor);
            }
        }

        public string ExpectedRGBValue
        {
            get
            {
                return $"{_expectedColor.R},{_expectedColor.G},{_expectedColor.B}";
            }
        }

    }
}
