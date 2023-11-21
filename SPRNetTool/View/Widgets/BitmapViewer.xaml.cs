using SPRNetTool.ViewModel.Widgets;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SPRNetTool.View.Widgets
{
    /// <summary>
    /// Interaction logic for BitmapViewer.xaml
    /// </summary>
    public partial class BitmapViewer : UserControl
    {
        private ImageSource BlackBagroundImage { get; }
            = new BitmapImage(new Uri(@"/Resources/spr_global_background.png", UriKind.Relative));
        private ImageSource TransparentBagroundImage { get; }
           = new BitmapImage(new Uri(@"/Resources/spr_global_transparent_background.png", UriKind.Relative));

        public static readonly DependencyProperty ViewModelProperty =
           DependencyProperty.Register(
               "ViewModel",
               typeof(IBitmapViewerViewModel),
               typeof(BitmapViewer),
               new FrameworkPropertyMetadata(default(IBitmapViewerViewModel),
                   FrameworkPropertyMetadataOptions.AffectsMeasure, propertyChangedCallback: OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public IBitmapViewerViewModel? ViewModel
        {
            get { return GetValue(ViewModelProperty) as IBitmapViewerViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        public BitmapViewer()
        {
            InitializeComponent();
            BitmapViewerContainerInternal.GlobalBackgroundSource = BlackBagroundImage;
            if (TransparenDecodedFrameBackgroundButton.IsChecked == true)
            {
                DecodedFrameBackground.Fill = new SolidColorBrush(Colors.Transparent);
                DecodedFrameBackground2.Fill = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                DecodedFrameBackground.Fill = new SolidColorBrush(Colors.White);
                DecodedFrameBackground2.Fill = new SolidColorBrush(Colors.White);
            }
        }

        private void FitToScreenButtonClick(object sender, RoutedEventArgs e)
        {
            if (FitToScreenButton.IsChecked == true)
            {
                StretchContainer.Visibility = Visibility.Visible;
                NoStretchContainer.Visibility = Visibility.Collapsed;
            }
            else
            {
                StretchContainer.Visibility = Visibility.Collapsed;
                NoStretchContainer.Visibility = Visibility.Visible;
                LayoutBoundButton.IsChecked = false;
                StretchContainer.Margin = new Thickness(0);
                LayoutBoundRect.StrokeThickness = 0;
            }
        }

        private void TransparentBackgroundButtonClick(object sender, RoutedEventArgs e)
        {
            if (TransparentBackgroundButton.IsChecked == true)
            {
                BitmapViewerContainerInternal.GlobalBackgroundSource
                  = TransparentBagroundImage;
            }
            else
            {
                BitmapViewerContainerInternal.GlobalBackgroundSource
                    = BlackBagroundImage;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BitmapViewerContainerInternal.ViewBoxZoomDelta = e.NewValue;
        }

        private void ShowLayoutBoundButtonClick(object sender, RoutedEventArgs e)
        {
            if (LayoutBoundButton.IsChecked == true)
            {
                StretchContainer.Margin = new Thickness(2);
                LayoutBoundRect.StrokeThickness = 2;
            }
            else
            {
                StretchContainer.Margin = new Thickness(0);
                LayoutBoundRect.StrokeThickness = 0;
            }
        }

        private void TransparenDecodedFrameBackgroundButtonClick(object sender, RoutedEventArgs e)
        {
            if (TransparenDecodedFrameBackgroundButton.IsChecked == true)
            {
                DecodedFrameBackground.Fill = new SolidColorBrush(Colors.Transparent);
                DecodedFrameBackground2.Fill = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                DecodedFrameBackground.Fill = new SolidColorBrush(Colors.White);
                DecodedFrameBackground2.Fill = new SolidColorBrush(Colors.White);
            }
        }
    }

    public class BitmapViewerInternal : UserControl
    {
        public static readonly DependencyProperty GlobalOffXProperty =
           DependencyProperty.Register(
                "GlobalOffX",
                typeof(int),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0, OnSizePropertyChanged));

        public uint GlobalOffX
        {
            get { return Convert.ToUInt16(GetValue(GlobalOffXProperty)); }
            set { SetValue(GlobalOffXProperty, value); }
        }

        public static readonly DependencyProperty GlobalOffYProperty =
            DependencyProperty.Register(
                "GlobalOffY",
                typeof(int),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0, OnSizePropertyChanged));

        public uint GlobalOffY
        {
            get { return Convert.ToUInt16(GetValue(GlobalOffYProperty)); }
            set { SetValue(GlobalOffYProperty, value); }
        }

        public static readonly DependencyProperty GlobalWidthProperty =
            DependencyProperty.Register(
                "GlobalWidth",
                typeof(uint),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0u, OnSizePropertyChanged));

        public uint GlobalWidth
        {
            get { return Convert.ToUInt16(GetValue(GlobalWidthProperty)); }
            set { SetValue(GlobalWidthProperty, value); }
        }

        public static readonly DependencyProperty GlobalHeightProperty =
            DependencyProperty.Register(
                "GlobalHeight",
                typeof(uint),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0u, OnSizePropertyChanged));

        public uint GlobalHeight
        {
            get { return Convert.ToUInt16(GetValue(GlobalHeightProperty)); }
            set { SetValue(GlobalHeightProperty, value); }
        }

        public static readonly DependencyProperty FrameHeightProperty =
            DependencyProperty.Register(
                "FrameHeight",
                typeof(uint),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0u, OnSizePropertyChanged));

        public uint FrameHeight
        {
            get { return Convert.ToUInt16(GetValue(FrameHeightProperty)); }
            set { SetValue(FrameHeightProperty, value); }
        }

        public static readonly DependencyProperty FrameWidthProperty =
            DependencyProperty.Register(
                "FrameWidth",
                typeof(uint),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0u, OnSizePropertyChanged));

        public uint FrameWidth
        {
            get { return Convert.ToUInt16(GetValue(FrameWidthProperty)); }
            set { SetValue(FrameWidthProperty, value); }
        }

        public static readonly DependencyProperty FrameOffXProperty =
            DependencyProperty.Register(
                "FrameOffX",
                typeof(int),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0, OnSizePropertyChanged));

        public uint FrameOffX
        {
            get { return Convert.ToUInt16(GetValue(FrameOffXProperty)); }
            set { SetValue(FrameOffXProperty, value); }
        }

        public static readonly DependencyProperty FrameOffYProperty =
            DependencyProperty.Register(
                "FrameOffY",
                typeof(int),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0, OnSizePropertyChanged));

        public uint FrameOffY
        {
            get { return Convert.ToUInt16(GetValue(FrameOffYProperty)); }
            set { SetValue(FrameOffYProperty, value); }
        }

        private static void OnSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty FrameSourceProperty =
                DependencyProperty.Register(
                        "FrameSource",
                        typeof(ImageSource),
                        typeof(BitmapViewerInternal),
                        new FrameworkPropertyMetadata(
                                null,
                                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                                new PropertyChangedCallback(OnImageSourceChanged),
                                null),
                        null);

        public ImageSource FrameSource
        {
            get { return (ImageSource)GetValue(FrameSourceProperty); }
            set { SetValue(FrameSourceProperty, value); }
        }

        public static readonly DependencyProperty GlobalBackgroundSourceProperty =
                DependencyProperty.Register(
                        "GlobalBackgroundSource",
                        typeof(ImageSource),
                        typeof(BitmapViewerInternal),
                        new FrameworkPropertyMetadata(
                                new BitmapImage(new Uri(@"/Resources/spr_global_background.png", UriKind.Relative)),
                                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                                new PropertyChangedCallback(OnImageSourceChanged),
                                null),
                        null);

        public ImageSource GlobalBackgroundSource
        {
            get { return (ImageSource)GetValue(GlobalBackgroundSourceProperty); }
            set { SetValue(GlobalBackgroundSourceProperty, value); }
        }

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public static readonly DependencyProperty ViewBoxZoomDeltaProperty =
            DependencyProperty.Register(
                "ViewBoxZoomDelta",
                typeof(double),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(1d));

        public double ViewBoxZoomDelta
        {
            get { return Convert.ToDouble(GetValue(ViewBoxZoomDeltaProperty)); }
            set { SetValue(ViewBoxZoomDeltaProperty, value); }
        }
    }
}
