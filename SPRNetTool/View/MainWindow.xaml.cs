using SPRNetTool.Utils;
using SPRNetTool.View.Base;
using SPRNetTool.View.Pages;
using System.Windows;
using System.Windows.Threading;

namespace SPRNetTool.View
{
    public enum MainWindowTagID
    {
        OptimizeList_RGBHeader,
        OptimizeList_ARGBHeader,
        OptimizeList_CombineRGBHeader,
        OriginalList_RGBHeader,
        OriginalList_CountHeader,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IWindowViewer
    {
        public object ViewModel => DataContext;

        public Dispatcher ViewElementDispatcher => Dispatcher;

        private DebugPage? debugPage = null;
        
        public MainWindow()
        {
            InitializeComponent();
        }


        public void DisableWindow(bool isDisabled)
        {
            if (isDisabled)
            {
                DisableLayer.Visibility = Visibility.Visible;
            }
            else
            {
                DisableLayer.Visibility = Visibility.Collapsed;
            }
        }


        private void MenuItemDebugPageClick(object sender, RoutedEventArgs e)
        {
            PageContentPresenter.Content = debugPage ?? new DebugPage(this).Also((it) => debugPage = it);
        }
    }


}


