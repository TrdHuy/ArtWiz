using SPRNetTool.Utils;
using SPRNetTool.View.Base;
using SPRNetTool.View.Pages;
using System.Windows;

namespace SPRNetTool.View
{
    public partial class MainWindow : BaseArtWizWindow
    {
        private DebugPage? debugPage = null;
        private SprEditorPage? sprEditorPage = null;
        public MainWindow()
        {
            InitializeComponent();
            PageContentPresenter.Content = sprEditorPage ?? new SprEditorPage((IWindowViewer)this).Also((it) => sprEditorPage = it);
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
                PageContentPresenter.Content = debugPage ?? new DebugPage((IWindowViewer)this).Also((it) => debugPage = it);
            }
            else if (sender == _sprWorkSpaceItem)
            {
                PageContentPresenter.Content = sprEditorPage ?? new SprEditorPage((IWindowViewer)this).Also((it) => sprEditorPage = it);
            }
        }
    }


}


