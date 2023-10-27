using SPRNetTool.Utils;
using System.Windows;
using System.Windows.Controls;

namespace SPRNetTool.View.Widgets
{
    /// <summary>
    /// Interaction logic for FrameLineEditor.xaml
    /// </summary>
    public partial class FrameLineEditor : UserControl
    {
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
                it.frameLineController.SetTotalFrameCount((uint)e.NewValue);
            });
        }

        public uint FrameCount
        {
            get { return (uint)GetValue(FrameCountProperty); }
            set { SetValue(FrameCountProperty, value); }
        }

        private FrameLineController frameLineController;

        public FrameLineEditor()
        {
            InitializeComponent();
            frameLineController = new FrameLineController(ScrView, MainCanvas, FrameCount);
            this.Loaded += FrameLineEditor_Loaded;
        }

        private void FrameLineEditor_Loaded(object sender, RoutedEventArgs e)
        {
            frameLineController.SetTotalFrameCount(FrameCount);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            frameLineController.RemoveFrame(3);
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            frameLineController.InsertFrame(3);
        }

        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            frameLineController.SetTotalFrameCount(FrameCount);
        }

        bool isO = false;
        private void Button_Click4(object sender, RoutedEventArgs e)
        {
            isO = !isO;
            frameLineController.ChangeDisplayIndex(isO);
        }
    }
}
