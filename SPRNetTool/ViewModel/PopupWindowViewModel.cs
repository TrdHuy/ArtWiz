using ArtWiz.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtWiz.ViewModel
{
    internal class PopupWindowViewModel : BaseParentsViewModel
    {
        private bool _isTitleBarHide;
        private bool _isPopupWindow;

        public bool IsPopupWindow
        {
            get { return _isPopupWindow; }
            set
            {
                _isPopupWindow = value;
                Invalidate(nameof(IsPopupWindow));
            }
        }
        public bool IsTitleBarHide
        {
            get { return _isTitleBarHide; }
            set
            {
                _isTitleBarHide = value;
                Invalidate(nameof(IsTitleBarHide));
            }
        }

        public PopupWindowViewModel()
        {
            IsTitleBarHide = true;
            IsPopupWindow = true;
        }
    }
}
