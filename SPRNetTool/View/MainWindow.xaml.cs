using SPRNetTool.Domain;
using SPRNetTool.Utils;
using SPRNetTool.View.Base;
using SPRNetTool.View.Pages;
using SPRNetTool.ViewModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SPRNetTool.View
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseArtWizWindow
    {
        private DebugPage? debugPage = null;
        private BitmapDisplayManager? bmpManager = null;
        public MainWindow()
        {
            InitializeComponent();
        }


        public override void DisableWindow(bool isDisabled)
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
            PageContentPresenter.Content = debugPage ?? new DebugPage((IWindowViewer)this).Also((it) => debugPage = it);
        }

        private void SaveCurrentBitmapToImageFile(object sender, RoutedEventArgs e)
        {
            var content = (DebugPage)PageContentPresenter.Content;
            var dataContext = (DebugPageViewModel)content.DataContext;
            bmpManager = new BitmapDisplayManager();
            BitmapSource bmpSource = dataContext.CurrentDisplayingBmpSrc ?? null;
            bmpManager.SaveBitmapSourceToFile(bmpSource);
        }
    }


}


