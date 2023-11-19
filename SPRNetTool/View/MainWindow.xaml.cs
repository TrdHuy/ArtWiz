using SPRNetTool.Utils;
using SPRNetTool.View.Base;
using SPRNetTool.View.Pages;
using System.Windows;

namespace SPRNetTool.View
{
    public partial class MainWindow : BaseArtWizWindow
    {
        private DebugPage? debugPage = null;
        private UserControl1? testPage = null;
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //debugPage = null;
            PageContentPresenter.Content = testPage ?? new UserControl1((IWindowViewer)this).Also((it) => testPage = it);
        }
    }


}


