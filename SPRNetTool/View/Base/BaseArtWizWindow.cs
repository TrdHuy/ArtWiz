using ArtWiz.Utils;
using ArtWiz.ViewModel.Base;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ArtWiz.View.Base
{
    public abstract class BaseArtWizWindow : CyberWindow, IWindowViewer, IArtWizViewModelOwner
    {
        #region Public properites
        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(BaseArtWizWindow),
                                          new FrameworkPropertyMetadata(
                                                new CornerRadius(),
                                                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender),
                                          new ValidateValueCallback(IsCornerRadiusValid));
        private static bool IsCornerRadiusValid(object value)
        {
            CornerRadius cr = (CornerRadius)value;
            return (cr.IsValid(false, false, false, false));
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        #endregion

        public Dispatcher ViewElementDispatcher => Dispatcher;

        public object ViewModel => DataContext;

        public Dispatcher ViewDispatcher => Dispatcher;

        private IWindowViewer.WindowClosedHandler? onWindowClosed;


        public virtual void DisableWindow(bool isDisabled)
        {
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            onWindowClosed?.Invoke(this);
            ViewModel.IfIs<IArtWizViewModel>((it) => it.OnArtWizViewModelOwnerDestroy());
        }

        public void AddOnWindowClosedEvent(IWindowViewer.WindowClosedHandler onWindowClosed)
        {
            this.onWindowClosed += onWindowClosed;
        }

        public void RemoveOnWindowClosedEvent(IWindowViewer.WindowClosedHandler onWindowClosed)
        {
            this.onWindowClosed -= onWindowClosed;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ViewModel.IfIs<IArtWizViewModel>((it) => it.OnArtWizViewModelOwnerCreate(this));
        }

        public virtual void OnReceivedMessage(int msg, object data)
        {
        }
    }
}
