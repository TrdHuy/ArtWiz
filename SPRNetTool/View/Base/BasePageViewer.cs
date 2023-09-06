using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SPRNetTool.View.Base
{
    public abstract class BasePageViewer : UserControl, IPageViewer
    {
        private IWindowViewer _ownerWindow;

        public IWindowViewer OwnerWindow => _ownerWindow;

        public abstract object ViewModel { get; }

        public BasePageViewer(IWindowViewer ownerWindow)
        {
            _ownerWindow = ownerWindow;
        }
    }
}
