using SPRNetTool.Utils;
using SPRNetTool.View.Base;
using SPRNetTool.View.Pages;
using SPRNetTool.ViewModel;
using SPRNetTool.ViewModel.Widgets;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace SPRNetTool.View
{
    public class VM : BaseViewModel, IPaletteEditorColorItemViewModel
    {
        private SolidColorBrush mBrush;
        public VM(SolidColorBrush brush)
        {
            mBrush = brush;
        }
        public SolidColorBrush ColorBrush
        {
            get => mBrush;
            set
            {
                mBrush = value;
                Invalidate();
            }
        }
    }
    public partial class MainWindow : BaseArtWizWindow
    {
        private DebugPage? debugPage = null;
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

    }


}


