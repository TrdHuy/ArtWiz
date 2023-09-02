using SPRNetTool.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace SPRNetTool.ViewModel
{
    
    public class ColorItemViewModel : BaseViewModel, IIndexableViewModel
    {
        private Color _itemColor;
        private long _count;
        private long _index;

        public Color ItemColor
        {
            get
            {
                return _itemColor;
            }
            set
            {

                _itemColor = value;
                Invalidate();
                Invalidate(nameof(RGBValue));
                Invalidate(nameof(RGBAValue));
                Invalidate(nameof(ColorValue));
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

        public long Index
        {
            get => _index; set
            {
                _index = value;
                Invalidate();
            }
        }
    }
}
