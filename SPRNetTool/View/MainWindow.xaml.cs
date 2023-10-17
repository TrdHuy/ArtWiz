﻿using SPRNetTool.Domain;
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


