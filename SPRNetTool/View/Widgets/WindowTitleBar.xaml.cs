using SPRNetTool.Utils;
using SPRNetTool.ViewModel.CommandVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SPRNetTool.View.Widgets
{
    /// <summary>
    /// Interaction logic for WindowTitleBar.xaml
    /// </summary>
    public partial class WindowTitleBar : UserControl
    {
        private IMainWindowCommand? viewModelCommand;
        public WindowTitleBar()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModelCommand = DataContext as IMainWindowCommand;
            DataContextChanged -= OnDataContextChanged;
        }

        private void MenuItemPageClick(object sender, RoutedEventArgs e)
        {
            if (sender == SprWorkSpceMenu)
            {
                viewModelCommand?.OnSprWorkSpaceMenuItemClick();
            }
            else if (sender == DeveloperModeMenu)
            {
                viewModelCommand?.OnDebugModeMenuItemClick();
            }
        }
    }
}
