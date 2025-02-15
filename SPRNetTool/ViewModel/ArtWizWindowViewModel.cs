using ArtWiz.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtWiz.ViewModel
{
    class ArtWizWindowViewModel : BaseParentsViewModel
    {

        private bool _isTitleBarHide;
        public bool IsTitleBarHide
        {
            get { return _isTitleBarHide; }
            set
            {
                _isTitleBarHide = value;
                Invalidate(nameof(IsTitleBarHide));
            }
        }

        public ArtWizWindowViewModel()
        {
            IsTitleBarHide = true;
        }

    }
}
