using SPRNetTool.Utils;
using System.Windows;
using System.Windows.Controls;
using static SPRNetTool.View.Widgets.FrameLineController;

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
                it.Controller.OnPreviewFrameIndexSwitched += handler;
            });
        }

        public static void RemoveOnPreviewFrameIndexSwitchedHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewFrameIndexSwitched -= handler;
            });
        }

        public static void AddOnPreviewAddingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewAddingFrame += handler;
            });
        }

        public static void RemoveOnPreviewAddingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewAddingFrame -= handler;
            });
        }

        public static void AddOnPreviewRemovingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewRemovingFrame += handler;
            });
        }

        public static void RemoveOnPreviewRemovingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewRemovingFrame -= handler;
            });
        }

        public static readonly DependencyProperty FrameCountProperty =
            DependencyProperty.Register(
                "FrameCount",
                typeof(uint),
                typeof(FrameLineEditor),
                new PropertyMetadata(default(uint), OnFrameCountChanged));

        private static void OnFrameCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.IfIs<FrameLineEditor>(it =>
            {
                if (!it.IsLoaded)
                {
                    return;
                }
                it.Controller.SetTotalFrameCount((uint)e.NewValue);
            });
        }

        public uint FrameCount
        {
            get { return (uint)GetValue(FrameCountProperty); }
            set { SetValue(FrameCountProperty, value); }
        }

        private FrameLineController Controller;

        public FrameLineEditor()
        {
            InitializeComponent();
            Controller = new FrameLineController(ScrView, MainCanvas, FrameCount);
            this.Loaded += FrameLineEditor_Loaded;
        }

        private void FrameLineEditor_Loaded(object sender, RoutedEventArgs e)
        {
            Controller.SetTotalFrameCount(FrameCount);
        }
    }
}
