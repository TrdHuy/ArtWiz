using SPRNetTool.ViewModel.Base;
using SPRNetTool.ViewModel.CommandVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRNetTool.ViewModel
{
    public class MainWindowViewModel : BaseParentsViewModel, IMainWindowCommand
    {
        private object? _pageContent;

        [Bindable(true)]
        public object? PageContent
        {
            get => _pageContent;
            set
            {
                _pageContent = value;
                Invalidate();
            }
        }

        public MainWindowViewModel()
        {

        }

        public void OnDebugModeMenuItemClick()
        {
        }

        public void OnSprWorkSpaceMenuItemClick()
        {
        }
    }
}
