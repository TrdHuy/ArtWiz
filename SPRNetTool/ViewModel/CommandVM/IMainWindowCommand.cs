using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRNetTool.ViewModel.CommandVM
{
    internal interface IMainWindowCommand
    {
        void OnDebugModeMenuItemClick();
        void OnSprWorkSpaceMenuItemClick();

    }
}
