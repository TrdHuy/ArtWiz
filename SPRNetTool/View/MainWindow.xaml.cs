using Microsoft.Win32;
using SPRNetTool.Data;
using SPRNetTool.Domain;
using SPRNetTool.Utils;
using SPRNetTool.View.Base;
using SPRNetTool.View.Pages;
using SPRNetTool.ViewModel;
using SPRNetTool.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static SPRNetTool.View.InputWindow;

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


