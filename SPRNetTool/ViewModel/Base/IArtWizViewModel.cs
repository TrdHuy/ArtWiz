using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRNetTool.ViewModel.Base
{
    public interface IArtWizViewModel
    {
        void OnCreate(IArtWizViewModelOwner owner);
    }
}
