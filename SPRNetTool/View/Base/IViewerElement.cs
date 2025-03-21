using System.Windows.Threading;

namespace ArtWiz.View.Base
{
    public interface IViewerElement
    {
        public Dispatcher ViewElementDispatcher { get; }
        public object ViewModel { get; }

        public void NotifyMessage(int msg, object data)
        {
            ViewElementDispatcher.BeginInvoke(() =>
            {
                OnReceivedMessage(msg, data);
            });
        }

        void OnReceivedMessage(int msg, object data);
    }
}
