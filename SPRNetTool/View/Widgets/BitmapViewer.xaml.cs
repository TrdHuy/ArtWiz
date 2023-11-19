using SPRNetTool.Utils;
using SPRNetTool.ViewModel.Widgets;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SPRNetTool.View.Widgets
{
    /// <summary>
    /// Interaction logic for BitmapViewer.xaml
    /// </summary>
    public partial class BitmapViewer : UserControl
    {
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
        }

    }

    public class BitmapViewerInternal : UserControl
    {
        public static readonly DependencyProperty GlobalOffXProperty =
           DependencyProperty.Register(
               "GlobalOffX",
               typeof(uint),
               typeof(BitmapViewerInternal),
               new FrameworkPropertyMetadata(0u,
                   FrameworkPropertyMetadataOptions.AffectsMeasure,
                   new PropertyChangedCallback(OnSizePropertyChanged)));

        public uint GlobalOffX
        {
            get { return Convert.ToUInt16(GetValue(GlobalOffXProperty)); }
            set { SetValue(GlobalOffXProperty, value); }
        }

        public static readonly DependencyProperty GlobalOffYProperty =
            DependencyProperty.Register(
                "GlobalOffY",
                typeof(uint),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0u, OnSizePropertyChanged));

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
                new FrameworkPropertyMetadata(300u,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnSizePropertyChanged)));

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
                new PropertyMetadata(200u, OnSizePropertyChanged));

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
                typeof(uint),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0u, OnSizePropertyChanged));

        public uint FrameOffX
        {
            get { return Convert.ToUInt16(GetValue(FrameOffXProperty)); }
            set { SetValue(FrameOffXProperty, value); }
        }

        public static readonly DependencyProperty FrameOffYProperty =
            DependencyProperty.Register(
                "FrameOffY",
                typeof(uint),
                typeof(BitmapViewerInternal),
                new PropertyMetadata(0u, OnSizePropertyChanged));

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
                                new PropertyChangedCallback(OnFrameSourceChanged),
                                null),
                        null);

        public ImageSource FrameSource
        {
            get { return (ImageSource)GetValue(FrameSourceProperty); }
            set { SetValue(FrameSourceProperty, value); }
        }

        private static void OnFrameSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.IfIs<BitmapViewer>(it =>
            {
                it.SprFrameImage.Source = e.NewValue as ImageSource;
            });
        }
    }
}
