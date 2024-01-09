using SPRNetTool.Utils;
using SPRNetTool.ViewModel.Widgets;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using static SPRNetTool.View.Widgets.FrameLineEditorVirtualizingPanel;

namespace SPRNetTool.View.Widgets
{
    /// <summary>
    /// Interaction logic for FrameLineEditor.xaml
    /// </summary>
    public partial class FrameLineEditor : UserControl
    {
        public static void AddOnPreviewFrameIndexSwitchedHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                //it.Controller.OnPreviewFrameIndexSwitched += handler;
            });
        }

        public static void RemoveOnPreviewFrameIndexSwitchedHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                //it.Controller.OnPreviewFrameIndexSwitched -= handler;
            });
        }

        public static void AddOnPreviewAddingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                //it.Controller.OnPreviewAddingFrame += handler;
            });
        }

        public static void RemoveOnPreviewAddingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                //it.Controller.OnPreviewAddingFrame -= handler;
            });
        }

        public static void AddOnPreviewRemovingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                //it.Controller.OnPreviewRemovingFrame += handler;
            });
        }

        public static void RemoveOnPreviewRemovingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                //it.Controller.OnPreviewRemovingFrame -= handler;
            });
        }

        public static void AddOnEllipseMouseClickHandler(DependencyObject element, FameLineMouseEventHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                //it.Controller.OnEllipseMouseClick += handler;
            });
        }

        public static void RemoveOnEllipseMouseClickHandler(DependencyObject element, FameLineMouseEventHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                //it.Controller.OnEllipseMouseClick -= handler;
            });
        }

        public static readonly DependencyProperty FrameSourceProperty =
            DependencyProperty.Register(
                "FrameSource",
                typeof(IEnumerable<IFramePreviewerViewModel>),
                typeof(FrameLineEditor),
            new FrameworkPropertyMetadata(defaultValue: default(IEnumerable<IFramePreviewerViewModel>),
                flags: FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnFrameSourceChangeCallback)));

        private static void OnFrameSourceChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.IfIs<FrameLineEditor>(it =>
            {
                it.SetUpFrameSource(e.NewValue as IEnumerable<IFramePreviewerViewModel>);
                e.OldValue.IfIs<IEnumerable<IFramePreviewerViewModel>>(source => it.DisposeFrameSource(source));
            });
        }

        public IEnumerable FrameSource
        {
            get { return (IEnumerable)GetValue(FrameSourceProperty); }
            set { SetValue(FrameSourceProperty, value); }
        }


        public FrameLineEditor()
        {
            InitializeComponent();
        }

        private void SetUpFrameSource(IEnumerable<IFramePreviewerViewModel>? frameSource)
        {
            frameSource?.IfIs<Collection<IFramePreviewerViewModel>>(it => myPanel.SetUpSource(it));
        }

        private void DisposeFrameSource(IEnumerable<IFramePreviewerViewModel> frameSource)
        {
            frameSource.IfIs<INotifyCollectionChanged>(it =>
            {
                it.CollectionChanged -= FrameSourceCollectionChanged;
            });
        }

        private void FrameSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
        }

    }
}
