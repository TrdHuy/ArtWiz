using Microsoft.Win32;
using SPRNetTool.Data;
using SPRNetTool.Domain;
using SPRNetTool.Utils;
using SPRNetTool.View.Base;
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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INetView
    {
        WorkManager workManager = new WorkManager();
        MainWindowViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = (MainWindowViewModel)DataContext;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "Resources/cuukiem.spr".FullPath();
            try
            {

                US_SprFileHead header;
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var temp = fs.BinToStruct<US_SprFileHead>(0);

                    if (temp != null && (temp?.GetVersionInfoStr().StartsWith("SPR") ?? false))
                    {
                        header = (US_SprFileHead)temp;
                        workManager.Init();
                        workManager.InitFromFileHead(header);
                    }
                    else
                    {
                        return;
                    }
                    workManager.PaletteData = new Palette(workManager.FileHead.ColourCounts);

                    fs.Position = Marshal.SizeOf(typeof(US_SprFileHead));
                    for (int i = 0; i < workManager.FileHead.ColourCounts; i++)
                    {
                        workManager.PaletteData.Data[i].Red = (byte)fs.ReadByte();
                        workManager.PaletteData.Data[i].Green = (byte)fs.ReadByte();
                        workManager.PaletteData.Data[i].Blue = (byte)fs.ReadByte();
                    }
                    workManager.FrameDataBegPos = fs.Position;

                    workManager.InitFrameData(fs);

                    //if (decdata != null)
                    //    Extension.Print2DArrayToFile(decdata, frameHeight, frameWidth, "test.txt");
                    var data = workManager.FrameData?[0];
                    if (data != null)
                    {
                        //Extension.Print2DArrayToFile(data?.decodedFrameData, data?.frameHeight ?? 0, data?.frameWidth ?? 0, "dec.txt");
                        BitmapUtil.Print2DArrayToFile(data?.globleFrameData, workManager.FileHead.GlobleHeight, workManager.FileHead.GlobleWidth, "glb.txt");
                        var byteData = BitmapUtil.ConvertPaletteColourArrayToByteArray(data?.globleFrameData);
                        var bmpSrc = BitmapUtil.GetBitmapFromRGBArray(byteData, workManager.FileHead.GlobleWidth, workManager.FileHead.GlobleHeight, PixelFormats.Bgra32);
                        StaticImageView.Source = bmpSrc;
                        BitmapUtil.CountColors(bmpSrc, out long argbCount, out long rgbCount, out Dictionary<Color, long> src);

                        var ditheringBmp = BitmapUtil.ApplyDithering(bmpSrc, 100);
                        //CountColors(ditheringBmp, out long argbCount2, out long rgbCount2);
                        StaticImageView2.Source = ditheringBmp;

                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
            }
        }


        private void OpenImageClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Tệp ảnh (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                BitmapSource? bmpSource = null;
                LoadingWindow l = new LoadingWindow(this);
                l.Show(block: async () =>
                {
                    string imagePath = openFileDialog.FileName;
                    bmpSource = BitmapUtil.LoadBitmapFromFile(imagePath);
                    StaticImageView.Source = bmpSource;
                    if (bmpSource == null) { return; }
                    var src = await BitmapUtil.CountColorsAsync(bmpSource);
                    await viewModel.SetColorSource(src);

                    Debug.WriteLine($"WxH= {bmpSource.PixelWidth * bmpSource.PixelHeight}");
                });

            }
        }

        private void OptimizeImageColorClick(object sender, RoutedEventArgs e)
        {
            InputBuilder builder = new InputBuilder();
            var colorCountKey = "Số màu (max = 256)";
            var colorCountDef = "256";
            var deltaKey = "Độ chênh lệch tối đa giữa 2 màu";
            var deltaDef = "100";

            var srcInput = builder.Add(colorCountKey
                , colorCountDef
                , (cur, input) => input.Any(char.IsNumber) && Convert.ToInt32(cur + input) <= 256)
                .Add(deltaKey
                , deltaDef
                , (cur, input) => input.Any(char.IsNumber))
                .Build();

            int colorSize = 256;
            int delta = 100;

            InputWindow inputWindow = new InputWindow(srcInput, this, (res) =>
            {
                colorSize = Convert.ToInt32(res[colorCountKey]);
                delta = Convert.ToInt32(res[deltaKey]);
            });
            var res = inputWindow.Show();
            if (res == Res.CANCEL) return;

            LoadingWindow l = new LoadingWindow(this, "Optimizing!");
            l.Show(block: async () =>
            {
                if (viewModel.ColorSource.Count == 0) return;

                await Task.Run(async () =>
                {
                    var orderedList = viewModel.OrderByDescendingCount(isSetToDisplaySource: false).ToList();
                    var selectedList = new ObservableCollection<ColorItemViewModel>();
                    var selectedColorList = new List<Color>();

                    while (selectedList.Count < colorSize && orderedList.Count > 0 && delta >= 0)
                    {
                        for (int i = 0; i < orderedList.Count; i++)
                        {
                            var item = orderedList[i];
                            var shouldAdd = true;
                            foreach (var item2 in selectedList)
                            {
                                if (item.ItemColor.CalculateEuclideanDistance(item2.ItemColor) < delta)
                                {
                                    shouldAdd = false;
                                    break;
                                }
                            }
                            if (shouldAdd)
                            {
                                selectedList.Add<ColorItemViewModel>(new ColorItemViewModel()
                                {
                                    ItemColor = item.ItemColor,
                                    Count = item.Count
                                });
                                selectedColorList.Add(item.ItemColor);
                                orderedList.RemoveAt(i);
                                i--;
                            }

                            if (selectedList.Count >= colorSize) break;
                        }
                        delta -= 2;
                    }
                    viewModel.OptimizedColorSource = selectedList;

                    BitmapSource? oldBmpSource = null;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        oldBmpSource = StaticImageView.Source as BitmapSource;

                    }));
                    if (selectedColorList.Count == colorSize && oldBmpSource != null)
                    {
                        var newBmpSrc = BitmapUtil.FloydSteinbergDithering(oldBmpSource, selectedColorList);
                        newBmpSrc?.Freeze();
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            StaticImageView2.Source = newBmpSrc;
                        }));

                        if (newBmpSrc != null)
                        {
                            var src = await BitmapUtil.CountColorsAsync(newBmpSrc);
                            var newCountedSrc = new ObservableCollection<ColorItemViewModel>();
                            await Task.Run(() =>
                            {
                                foreach (var color in src)
                                {
                                    var newColor = color.Key;
                                    newCountedSrc.Add<ColorItemViewModel>(new ColorItemViewModel { ItemColor = newColor, Count = color.Value });
                                }
                            });

                            viewModel.OptimizedColorSource = newCountedSrc;
                        }


                    }
                });
            });

        }

        private int countClick = 0;
        private void ColorCountHeaderMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (countClick == 0)
            {
                viewModel.OrderByCount();
                countClick = 1;
            }
            else if (countClick == 1)
            {
                viewModel.OrderByDescendingCount();
                countClick = 2;
            }
            else if (countClick == 2)
            {
                viewModel.ResetOrder();
                countClick = 0;
            }
        }

        private int rgbClick = 0;
        private void RGBHeaderMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (rgbClick == 0)
            {
                viewModel.OrderByRGB();
                rgbClick = 1;
            }
            else if (rgbClick == 1)
            {
                viewModel.ResetOrder();
                rgbClick = 0;
            }
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

        private void ClearClick(object sender, RoutedEventArgs e)
        {
            viewModel.ResetViewModel();
            StaticImageView.Source = null;
            StaticImageView2.Source = null;
        }
    }


}


