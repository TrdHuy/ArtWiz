using System;
using System.Windows;
using System.Windows.Threading;

namespace SPRNetTool.View.Base
{
    public abstract class BaseArtWizWindow : Window, IWindowViewer
    {
        public Dispatcher ViewElementDispatcher => Dispatcher;

        public object ViewModel => DataContext;

        private IWindowViewer.WindowClosedHandler? onWindowClosed;


        public virtual void DisableWindow(bool isDisabled)
        {
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            onWindowClosed?.Invoke(this);
        }

        public void AddOnWindowClosedEvent(IWindowViewer.WindowClosedHandler onWindowClosed)
        {
            this.onWindowClosed += onWindowClosed;
        }

        public void RemoveOnWindowClosedEvent(IWindowViewer.WindowClosedHandler onWindowClosed)
        {
            this.onWindowClosed -= onWindowClosed;
        }
    }
}
