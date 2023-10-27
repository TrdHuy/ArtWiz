using SPRNetTool.Utils;
using SPRNetTool.View.Base;
using SPRNetTool.View.Pages;
using System.Collections.ObjectModel;
using System.Windows;

namespace SPRNetTool.View
{

    public partial class MainWindow : BaseArtWizWindow
    {
        private DebugPage? debugPage = null;
        public MainWindow()
        {
            InitializeComponent();
            //var collection = new ObservableCollection<string>();
            //for (int i = 0; i < 1000; i++)
            //{
            //    collection.Add($"item{i}");
            //}
            //LV.ItemsSource = collection;
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


