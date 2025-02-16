using ArtWiz.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtWiz.ViewModel
{
    class LoginWindowViewModel : BaseParentsViewModel
    {

        private bool _isTitleBarHidden;
        public bool IsTitleBarHidden
        {
            get { return _isTitleBarHidden; }
            set
            {
                _isTitleBarHidden = value;
                Invalidate();
            }
        }


        private bool _isCloseButtonUsedOnlyOnTitleBar;
        public bool IsCloseButtonUsedOnlyOnTitleBar
        {
            get { return _isCloseButtonUsedOnlyOnTitleBar; }
            set
            {
                _isCloseButtonUsedOnlyOnTitleBar = value;
                Invalidate();
            }
        }

        public LoginWindowViewModel()
        {
            IsCloseButtonUsedOnlyOnTitleBar = true;
        }

    }
}
