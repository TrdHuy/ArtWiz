using System.Windows.Threading;

namespace ArtWiz.View.Base
{
    public interface IViewerElement
    {
        public Dispatcher ViewElementDispatcher { get; }
        public object ViewModel { get; }
    }
}
