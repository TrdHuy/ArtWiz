using System.Windows.Threading;

namespace SPRNetTool.View.Base
{
    public interface IViewerElement
    {
        public Dispatcher ViewElementDispatcher { get; }
        public object ViewModel { get; }
    }
}
