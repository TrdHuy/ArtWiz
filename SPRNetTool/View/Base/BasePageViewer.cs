using System.Windows.Controls;
using System.Windows.Threading;

namespace SPRNetTool.View.Base
{
    public abstract class BasePageViewer : UserControl, IPageViewer
    {
        private IWindowViewer _ownerWindow;

        public IWindowViewer OwnerWindow => _ownerWindow;
        public Dispatcher ViewElementDispatcher => Dispatcher;

        public abstract object ViewModel { get; }

        public BasePageViewer(IWindowViewer ownerWindow)
        {
            _ownerWindow = ownerWindow;
        }
    }
}
