using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SPRNetTool.Domain
{
    public class BitmapDisplayManager : BaseDomain, IBitmapDisplayManager
    {
        private struct CountablePxBmpSrc
        {
            public BitmapSource? BitmapSource;
            public Dictionary<Color, long>? ColorSource;
        }

        private CountablePxBmpSrc _currentDisplayingBitmap;

        private BitmapSource? CurrentDisplayBitmap
        {
            get { return _currentDisplayingBitmap.BitmapSource; }
            set
            {
                _currentDisplayingBitmap.BitmapSource = value;
            }
        }

        void IBitmapDisplayManager.OpenBitmapFromFile(string filePath, bool countPixelColor)
        {
            _currentDisplayingBitmap.BitmapSource = this.LoadBitmapFromFile(filePath);
            if (countPixelColor && CurrentDisplayBitmap != null)
            {
                _currentDisplayingBitmap.ColorSource = this.CountColors(CurrentDisplayBitmap);
            }
            else
            {
                _currentDisplayingBitmap.ColorSource = null;
            }
            NotifyChanged(new BitmapDisplayMangerChangedArg(_currentDisplayingBitmap.BitmapSource,
                 _currentDisplayingBitmap.ColorSource));
        }
    }

    public class BitmapDisplayMangerChangedArg : IDomainChangedArgs
    {
        public BitmapSource? CurrentDisplayingSource { get; private set; }
        public Dictionary<Color, long>? CurrentColorSource { get; private set; }

        public BitmapDisplayMangerChangedArg(BitmapSource? currentDisplayingSource = null,
            Dictionary<Color, long>? colorSource = null)
        {

            CurrentDisplayingSource = currentDisplayingSource;
            CurrentColorSource = colorSource;
        }
    }
}
