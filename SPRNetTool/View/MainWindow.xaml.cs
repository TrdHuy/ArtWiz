using ArtWiz.Utils;
using ArtWiz.View.Base;
using ArtWiz.View.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace ArtWiz.View
{
    public partial class MainWindow : BaseArtWizWindow
    {
        private DebugPage? debugPage = null;
        private SprEditorPage? sprEditorPage = null;
        private double previousePageContentScrollViewHeightCache = -1d;

        public MainWindow()
        {
            InitializeComponent();
            SetPageContent(sprEditorPage ?? new SprEditorPage((IWindowViewer)this).Also((it) => sprEditorPage = it));
        }

        private void SetPageContent(object content)
        {
            PageContentPresenter.Content = content;
            var chrome = WindowChrome.GetWindowChrome(this);
            if (content is SprEditorPage)
            {
                chrome.ResizeBorderThickness = new Thickness(0);
            }
            else
            {
                chrome.ResizeBorderThickness = new Thickness(10);
            }
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _devModeMenuItem!.Click += MenuItemClick;
            _sprWorkSpaceItem!.Click += MenuItemClick;
        }

        private void MenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender == _devModeMenuItem)
            {
                SetPageContent(debugPage ?? new DebugPage((IWindowViewer)this).Also((it) => debugPage = it));
            }
            else if (sender == _sprWorkSpaceItem)
            {
                SetPageContent(sprEditorPage ?? new SprEditorPage((IWindowViewer)this).Also((it) => sprEditorPage = it));
            }
        }

        private void OnWindowStateChanged(object sender, System.EventArgs e)
        {
            if (_windowSizeManager.OldState == WindowState.Normal &&
                WindowState == WindowState.Maximized &&
                PageContentPresenter.Content == sprEditorPage &&
                PageContentScrollViewer.VerticalScrollBarVisibility != ScrollBarVisibility.Visible)
            {
                previousePageContentScrollViewHeightCache = PageContentScrollViewer.ActualHeight;
            }
            else if (_windowSizeManager.OldState == WindowState.Maximized &&
                WindowState == WindowState.Normal &&
                previousePageContentScrollViewHeightCache != -1d &&
                sprEditorPage != null &&
                previousePageContentScrollViewHeightCache >= sprEditorPage.MinHeight)
            {
                PageContentScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                PageContentScrollViewer.UpdateLayout();
                PageContentScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }
    }


}


