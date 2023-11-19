using SPRNetTool.View.Base;
using SPRNetTool.ViewModel.Widgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SPRNetTool.View.Pages
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : BasePageViewer
    {

        public override object ViewModel => DataContext;
        private WriteableBitmap testSrc;
        private byte[]? testSrcPixel = null;

        public UserControl1(IWindowViewer ownerWindow) : base(ownerWindow)
        {
            InitializeComponent();

            Task.Run(() =>
            {
                testSrc = CreateBackgroundBitmap();
                testSrc.Freeze();
                Dispatcher.Invoke(() =>
                {
                    TestImg.Source = testSrc;
                });
            });

            //frameViewer.Width = testSrc.PixelWidth;
            //frameViewer.Height = testSrc.PixelHeight;
            //ForegroundImage.Source = testSrc;
        }

        private WriteableBitmap CreateBackgroundBitmap()
        {
            int width = 200;
            int height = 200;
            int squareSize = 10;

            // Tạo WriteableBitmap với nền trong suốt
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            // Mảng pixel cho mỗi hình vuông
            byte[] squarePixels = new byte[width * height * 4];
            int idx = 0;
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if ((i / squareSize + j / squareSize) % 2 == 0)
                    {
                        squarePixels[idx] = 255;
                        squarePixels[idx + 1] = 0;
                        squarePixels[idx + 2] = 0;
                        squarePixels[idx + 3] = 255;
                    }
                    else
                    {
                        squarePixels[idx] = 0;
                        squarePixels[idx + 1] = 255;
                        squarePixels[idx + 2] = 0;
                        squarePixels[idx + 3] = 255;
                    }
                    idx += 4;
                }
            }
            // Vẽ hình vuông xen kẽ
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), squarePixels, width * 4, 0);
            return bitmap;
        }

        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private async Task<WriteableBitmap> EditBitmap(WriteableBitmap bitmap, byte newR)
        {
            await semaphoreSlim.WaitAsync();

            var newSource = await Task<WriteableBitmap>.Run(() =>
             {

                 var newBitmap = bitmap;
                 if (bitmap.IsFrozen)
                 {
                     newBitmap = bitmap.Clone();
                 }
                 int width = bitmap.PixelWidth;
                 int height = bitmap.PixelHeight;
                 int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

                 // Tạo mảng pixel
                 if (testSrcPixel == null)
                 {
                     testSrcPixel = new byte[width * height * bytesPerPixel];
                     bitmap.CopyPixels(testSrcPixel, width * bytesPerPixel, 0);
                 }

                 // Chỉnh sửa pixel: các pixel ở vị trí chiều ngang chia hết cho 2 có màu xanh lá
                 for (int y = 0; y < height; y++)
                 {
                     for (int x = 0; x < width; x++)
                     {
                         int index = (y * width + x) * bytesPerPixel;

                         // Nếu là pixel ở vị trí chiều ngang chia hết cho 2
                         if (x % 2 == 0)
                         {
                             // Gán màu xanh lá cho pixel
                             testSrcPixel[index + 2] = newR;   // Red
                         }
                     }
                 }

                 // Ghi lại pixel đã được chỉnh sửa vào bitmap
                 newBitmap.WritePixels(new Int32Rect(0, 0, width, height), testSrcPixel, width * bytesPerPixel, 0);
                 newBitmap.Freeze();
                 return newBitmap;
             });
            semaphoreSlim.Release();
            return newSource;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BVVM.BitmapViewerVM.GlobalHeight += 1;
            BVVM.BitmapViewerVM.GlobalWidth += 1;
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            BVVM.BitmapViewerVM.GlobalHeight = (uint)testSrc.PixelHeight + 50;
            BVVM.BitmapViewerVM.GlobalWidth = (uint)testSrc.PixelWidth + 50;
            BVVM.BitmapViewerVM.FrameWidth = (uint)testSrc.PixelWidth;
            BVVM.BitmapViewerVM.FrameHeight = (uint)testSrc.PixelHeight;
            BVVM.BitmapViewerVM.FrameSource = testSrc;
        }

        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                testSrc = CreateBackgroundBitmap();
                testSrc.Freeze();
                BVVM.BitmapViewerVM.GlobalHeight = (uint)testSrc.PixelHeight;
                BVVM.BitmapViewerVM.GlobalWidth = (uint)testSrc.PixelWidth;
                BVVM.BitmapViewerVM.FrameWidth = (uint)testSrc.PixelWidth;
                BVVM.BitmapViewerVM.FrameHeight = (uint)testSrc.PixelHeight;
                BVVM.BitmapViewerVM.FrameSource = testSrc;
            });
        }

        private async void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (testSrc.IsFrozen)
            {
                testSrc = await EditBitmap(testSrc, (byte)e.NewValue);
                BVVM.BitmapViewerVM.FrameSource = testSrc;
            }
            else
            {
                await EditBitmap(testSrc, (byte)e.NewValue);
            }
        }

        private void Slider_ValueChanged2(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BVVM.BitmapViewerVM.FrameOffX += (uint)(e.NewValue - e.OldValue);
        }

        private void Slider_ValueChanged3(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BVVM.BitmapViewerVM.FrameOffY += (uint)(e.NewValue - e.OldValue);
        }

        private void Slider_ValueChanged4(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BVVM.BitmapViewerVM.GlobalOffX += (uint)(e.NewValue - e.OldValue);
        }

        private void Slider_ValueChanged5(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BVVM.BitmapViewerVM.GlobalOffY += (uint)(e.NewValue - e.OldValue);
        }
    }
}

