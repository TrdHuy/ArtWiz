using Microsoft.Win32;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.Utils;
using SPRNetTool.View.Base;
using SPRNetTool.View.Utils;
using SPRNetTool.ViewModel;
using SPRNetTool.ViewModel.Base;
using SPRNetTool.ViewModel.CommandVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static SPRNetTool.View.InputWindow;

namespace SPRNetTool.View.Pages
{
    public enum DebugPageTagID
    {
        OptimizeList_RGBHeader,
        OptimizeList_ARGBHeader,
        OptimizeList_CombineRGBHeader,
        OriginalList_RGBHeader,
        OriginalList_CountHeader,

        SPRInfo_PlayButton,
        SPRInfo_FrameIndexPlusButton,
        SPRInfo_FrameIndexMinusButton,
        SPRInfo_FrameOffsetXPlusButton,
        SPRInfo_FrameOffsetXMinusButton,
        SPRInfo_FrameOffsetYPlusButton,
        SPRInfo_FrameOffsetYMinusButton,
        SPRInfo_IntervalMinusButton,
        SPRInfo_IntervalPlusButton,
        SPRInfo_FrameWidthMinusButton,
        SPRInfo_FrameWidthPlusButton,
        SPRInfo_FrameHeightMinusButton,
        SPRInfo_FrameHeightPlusButton,
        SPRInfo_GlobalWidthMinusButton,
        SPRInfo_GlobalWidthPlusButton,
        SPRInfo_GlobalHeightMinusButton,
        SPRInfo_GlobalHeightPlusButton,
        SPRInfo_GlobalOffsetXPlusButton,
        SPRInfo_GlobalOffsetXMinusButton,
        SPRInfo_GlobalOffsetYPlusButton,
        SPRInfo_GlobalOffsetYMinusButton,

        ImageInfo_ExportToSingleFrameSprFile,
    }

    public partial class DebugPage : BasePageViewer
    {
        private Window ownerWindow;

        public override object ViewModel => DataContext;

        //TODO: remove this because it belong to domain layer
        private ISprWorkManager workManager = new SprWorkManager();
        private DebugPageViewModel viewModel;
        private IDebugPageCommand? commandVM;


        public DebugPage(IWindowViewer ownerWindow) : base(ownerWindow)
        {
            InitializeComponent();
            viewModel = (DebugPageViewModel)DataContext;
            this.ownerWindow = (Window)ownerWindow;
            commandVM = DataContext.IfIsThenAlso<IDebugPageCommand>((it) => it);
        }

        private void OpenImageClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Tệp ảnh |*.png;*.jpg;*.jpeg;*.gif;*.spr";
            if (openFileDialog.ShowDialog() == true)
            {
                BitmapSource? bmpSource = null;
                string imagePath = openFileDialog.FileName;

                string fileExtension = Path.GetExtension(imagePath).ToLower();
                if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".spr")
                {
                    LoadingWindow l = new LoadingWindow(ownerWindow);
                    l.Show(block: async () =>
                    {
                        await viewModel.OpenImageFromFileAsync(imagePath);
                        bmpSource = viewModel.CurrentlyDisplayedBitmapSource;

                        Debug.WriteLine($"WxH= {bmpSource?.PixelWidth * bmpSource?.PixelHeight}");
                    });
                }
            }
        }

        private void OptimizeImageColorClick(object sender, RoutedEventArgs e)
        {
            InputBuilder builder = new InputBuilder();
            var colorCountKey = "Số màu (max = 256)";
            var colorCountDef = "256";
            var deltaKey = "Độ chênh lệch tối đa giữa 2 màu";
            var deltaDes =
                "Độ chênh lệch tối đa giữa 2 màu là tham số thể xác định màu chưa được chọn tiếp theo có nên add vào list các màu\n" +
                "được chọn không.\n\n" +
                "Ví dụ:\n" +
                "Màu đã chọn RGB (10,10,10)\n" +
                "Màu chưa chọn tiếp theo (11,11,11)\n" +
                "Độ chênh lệch = 1,7 < delta = 10 => Màu này sẽ không được chọn vì giống màu đã chọn (10,10,10)";
            var deltaDef = "100";
            var isUsingAlphaKey = "Sử dụng alpha để tính được nhiều màu cho palette";
            var isUsingAlphaDef = false;
            var deltaForCompareRecalculateKey = "Độ chênh lệch màu ARGB";
            var deltaForCompareRecalculateDes =
                "Độ chênh lệch màu ARGB là tham số để xác định màu tiếp theo có cần cân nhắc để tính giá trị cho kênh alpha hay không.\n" +
                "Nếu giữa màu đã chọn và màu chưa được chọn tiếp theo có độ chênh lệch nhỏ hơn 'Độ chênh lệch màu ARGB'\n" +
                "thì màu chưa được chọn tiếp theo sẽ được cân nhắc để tính giá trị Alpha từ màu đã chọn.\n\n" +
                "Ví dụ:\n" +
                "Màu đã chọn RGB (10,10,10)\n" +
                "Màu chưa chọn tiếp theo (11,11,11)\n" +
                "Độ chênh lệch = 1,7 < delta = 10 => Màu này sẽ được cân nhắc tính giá trị alpha dựa theo Màu đã chọn (10,10,10)";
            var deltaForCompareRecalculateDef = "10";
            var backgroundForBlendColorKey = "Màu nền cho blend";
            var backgroundForBlendColorDes = "Màu nền được dùng cho việc blend màu foreground với kênh alpha.\n" +
                "https://stackoverflow.com/questions/1855884/determine-font-color-based-on-background-color";

            var srcInput = builder.AddTextInputOption(colorCountKey
                , colorCountKey
                , colorCountDef
                , (cur, input) => input.Any(char.IsNumber) && Convert.ToInt32(cur + input) <= 256)
                .AddTextInputOption(deltaKey
                , deltaDes
                , deltaDef
                , (cur, input) => input.Any(char.IsNumber))
                .AddCheckBoxOption(isUsingAlphaKey
                , isUsingAlphaKey
                , isUsingAlphaDef
                , () => true
                , (src, isChecked) =>
                {
                    src[3].IsDisabled = !isChecked;
                    src[4].IsDisabled = !isChecked;
                })
                .AddTextInputOption(deltaForCompareRecalculateKey
                , deltaForCompareRecalculateDes
                , deltaForCompareRecalculateDef
                , (cur, input) => input.Any(char.IsNumber) && Convert.ToInt32(cur + input) <= 500)
                .AddComboBoxOption(backgroundForBlendColorKey
                , backgroundForBlendColorDes
                , new List<string> { "WHITE (255,255,255)", "BLACK (0,0,0)" }
                , 0
                , () => true
                , (cur, input) => { })
                .Build();

            int colorSize = 256;
            int delta = 100;
            var isUsingAlpha = false;
            var deltaForCompareRecalculate = 10;
            var backgroundForBlendColor = Colors.White;

            InputWindow inputWindow = new InputWindow(srcInput, ownerWindow, (res) =>
            {
                colorSize = Convert.ToInt32(res[colorCountKey]);
                delta = Convert.ToInt32(res[deltaKey]);
                isUsingAlpha = Convert.ToBoolean(res[isUsingAlphaKey]);
                deltaForCompareRecalculate = Convert.ToInt32(res[deltaForCompareRecalculateKey]);
                switch (Convert.ToInt32(res[backgroundForBlendColorKey]))
                {
                    case 0:
                        backgroundForBlendColor = Colors.White;
                        break;
                    case 1:
                        backgroundForBlendColor = Colors.Black;
                        break;
                }
            });
            var res = inputWindow.Show();
            if (res == Res.CANCEL) return;

            LoadingWindow l = new LoadingWindow(ownerWindow, "Optimizing!");
            l.Show(block: async () =>
            {
                if (viewModel.OriginalColorSource.Count == 0) return;
                await Task.Run(() =>
                {
                    viewModel.OptimizeImageColor(colorSize: colorSize,
                    colorDifferenceDelta: delta,
                    isUsingAlpha: isUsingAlpha,
                    colorDifferenceDeltaForCalculatingAlpha: deltaForCompareRecalculate,
                    backgroundForBlendColor: backgroundForBlendColor);
                });
            });
        }

        private int originalCountClick = 0;
        private int originalRgbClick = 0;
        private int optimizeCombinedRgbClick = 0;
        private int optimizeRgbClick = 0;
        private int optimizeArgbClick = 0;
        private void HeaderMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var tag = (sender as TextBlock)?.Tag;
            if (tag == null || !(tag is DebugPageTagID)) return;
            tag = (DebugPageTagID)tag;
            switch (tag)
            {
                case DebugPageTagID.OptimizeList_CombineRGBHeader:
                    if (optimizeCombinedRgbClick == 0)
                    {
                        viewModel.OptimizedOrderByCombinedRGB();
                        optimizeCombinedRgbClick = 1;
                    }
                    else if (optimizeCombinedRgbClick == 1)
                    {
                        viewModel.ResetOptimizedOrder();
                        optimizeCombinedRgbClick = 0;
                    }
                    break;
                case DebugPageTagID.OptimizeList_RGBHeader:
                    if (optimizeRgbClick == 0)
                    {
                        viewModel.OptimizedOrderByRGB();
                        optimizeRgbClick = 1;
                    }
                    else if (optimizeRgbClick == 1)
                    {
                        viewModel.ResetOptimizedOrder();
                        optimizeRgbClick = 0;
                    }
                    break;
                case DebugPageTagID.OptimizeList_ARGBHeader:
                    if (optimizeArgbClick == 0)
                    {
                        viewModel.OptimizedOrderByARGB();
                        optimizeArgbClick = 1;
                    }
                    else if (optimizeArgbClick == 1)
                    {
                        viewModel.ResetOptimizedOrder();
                        optimizeArgbClick = 0;
                    }
                    break;
                case DebugPageTagID.OriginalList_CountHeader:
                    if (originalCountClick == 0)
                    {
                        viewModel.OrderByCount();
                        originalCountClick = 1;
                    }
                    else if (originalCountClick == 1)
                    {
                        viewModel.OrderByDescendingCount();
                        originalCountClick = 2;
                    }
                    else if (originalCountClick == 2)
                    {
                        viewModel.ResetOrder();
                        originalCountClick = 0;
                    }
                    break;
                case DebugPageTagID.OriginalList_RGBHeader:
                    if (originalRgbClick == 0)
                    {
                        viewModel.OrderByRGB();
                        originalRgbClick = 1;
                    }
                    else if (originalRgbClick == 1)
                    {
                        viewModel.ResetOrder();
                        originalRgbClick = 0;
                    }
                    break;
            }

        }

        private void ClearClick(object sender, RoutedEventArgs e)
        {
            viewModel.ResetViewModel();
            StaticImageView.Source = null;
            StaticImageView2.Source = null;
        }

        private void TestBtnClick(object sender, RoutedEventArgs e)
        {
            var newColor = Color.FromArgb(127, 20, 30, 40).BlendColors(Colors.White);


            var background = Colors.White;
            var foreGround = Color.FromRgb(100, 179, 150);
            var combinedColor = Color.FromRgb(0, 0, 0);

            var colorDistance = foreGround.CalculateEuclideanDistance(combinedColor);
            var alphaChanel = foreGround.FindAlphaColors(background, combinedColor, out byte averageAbsoluteDeviation);

            colorDistance = Color.FromArgb(alphaChanel, 100, 179, 150).CalculateEuclideanDistance(combinedColor);

        }

        private async void ResizeImageClick(object sender, RoutedEventArgs e)
        {
            if (StaticImageView.Source == null) return;
            var image = StaticImageView.Source as BitmapSource;
            if (image == null) return;

            var oldBytes = image.ToRawByteArray();
            var oldColorSrc = await BitmapUtil.CountColorsFromByteArrayAsync(oldBytes, image.Format);
            viewModel.SetColorSource(oldColorSrc);
            double newHeight = 300d;
            double newWidth = 300d;
            double oldHeight = (double)image.PixelHeight;
            double oldWidth = (double)image.PixelWidth;
            TransformedBitmap resizedBitmap = new TransformedBitmap(image, new ScaleTransform(newWidth / oldWidth, newHeight / oldHeight));
            StaticImageView2.Source = resizedBitmap;

            var newBytes = resizedBitmap.ToRawByteArray();
            var newColorSrc = await BitmapUtil.CountColorsFromByteArrayAsync(newBytes, resizedBitmap.Format);
            viewModel.SetResultRGBColorSource(await CreateColorSourceItems(newColorSrc));

        }

        private async Task<ObservableCollection<ColorItemViewModel>> CreateColorSourceItems(Dictionary<Color, long> src)
        {
            var newCountedSrc = new ObservableCollection<ColorItemViewModel>();
            await Task.Run(() =>
            {
                foreach (var color in src)
                {
                    var newColor = color.Key;
                    newCountedSrc.Add<ColorItemViewModel>(new ColorItemViewModel { ItemColor = newColor, Count = color.Value });
                }
            });
            return newCountedSrc;
        }

        private void OnRunLeftMouseUp(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBlock)?.Tag.IfIs<DebugPageTagID>((tag) =>
            {
                switch (tag)
                {
                    case DebugPageTagID.ImageInfo_ExportToSingleFrameSprFile:
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            if (saveFileDialog.ShowDialog() == true)
                            {
                                LoadingWindow l = new LoadingWindow(ownerWindow, "Saving to Spr file!");
                                l.Show(block: async () =>
                                {
                                    await Task.Run(() =>
                                    {
                                        commandVM?.OnSaveCurrentDisplayedBitmapSourceToSpr(Path.ChangeExtension(saveFileDialog.FileName, "spr"));
                                    });
                                });
                            }
                            break;
                        }
                }
            });
            (sender as Run)?.Tag.IfIs<DebugPageTagID>((tag) =>
            {
                switch (tag)
                {
                    case DebugPageTagID.SPRInfo_PlayButton:
                        {
                            commandVM?.OnPlayPauseAnimationSprClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameIndexMinusButton:
                        {
                            commandVM?.OnDecreaseCurrentlyDisplayedSprFrameIndex();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameIndexPlusButton:
                        {
                            commandVM?.OnIncreaseCurrentlyDisplayedSprFrameIndex();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameOffsetXMinusButton:
                        {
                            commandVM?.OnDecreaseFrameOffsetXButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameOffsetXPlusButton:
                        {
                            commandVM?.OnIncreaseFrameOffsetXButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameOffsetYMinusButton:
                        {
                            commandVM?.OnDecreaseFrameOffsetYButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameOffsetYPlusButton:
                        {
                            commandVM?.OnIncreaseFrameOffsetYButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_IntervalPlusButton:
                        {
                            commandVM?.OnIncreaseIntervalButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_IntervalMinusButton:
                        {
                            commandVM?.OnDecreaseIntervalButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameWidthMinusButton:
                        {
                            commandVM?.OnDecreaseFrameWidthButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameWidthPlusButton:
                        {
                            commandVM?.OnIncreaseFrameWidthButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameHeightMinusButton:
                        {
                            commandVM?.OnDecreaseFrameHeightButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameHeightPlusButton:
                        {
                            commandVM?.OnIncreaseFrameHeightButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalOffsetXMinusButton:
                        {
                            commandVM?.OnDecreaseSprGlobalOffsetXButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalOffsetXPlusButton:
                        {
                            commandVM?.OnIncreaseSprGlobalOffsetXButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalOffsetYMinusButton:
                        {
                            commandVM?.OnDecreaseSprGlobalOffsetYButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalOffsetYPlusButton:
                        {
                            commandVM?.OnIncreaseSprGlobalOffsetYButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalWidthMinusButton:
                        {
                            commandVM?.OnDecreaseSprGlobalWidthButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalWidthPlusButton:
                        {
                            commandVM?.OnIncreaseSprGlobalWidthButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalHeightMinusButton:
                        {
                            commandVM?.OnDecreaseSprGlobalHeightButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalHeightPlusButton:
                        {
                            commandVM?.OnIncreaseSprGlobalHeightButtonClicked();
                            break;
                        }
                }
            });
        }

        private void SaveCurrentSourceClick(object sender, RoutedEventArgs e)
        {
            var builder = new InputBuilder();
            var SavingTitle = "Lưu với định dạng";
            var SavingDes = "Save";
            List<string> SavingOptions = new List<string>() { "jpg", "png", "spr" };
            var inputSrc = builder.AddRadioOptions(SavingTitle, SavingDes, SavingOptions).Build();
            var checkedContent = "";
            InputWindow inputWindow = new InputWindow(inputSrc, ownerWindow, (res) =>
            {
                if (res != null)
                {
                    foreach (var item in res)
                    {
                        if (item.Key != null) checkedContent = Convert.ToString(item.Value);
                        break;
                    }
                }
            });
            Res res = inputWindow.Show();
            if (res == Res.CANCEL) return;
        }

        private void OnRunMouseHold(object sender, MouseHoldEventArgs args)
        {
            (sender as Run)?.Tag.IfIs<DebugPageTagID>((tag) =>
            {
                switch (tag)
                {

                    case DebugPageTagID.SPRInfo_FrameIndexMinusButton:
                        {
                            commandVM?.OnDecreaseCurrentlyDisplayedSprFrameIndex();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameIndexPlusButton:
                        {
                            commandVM?.OnIncreaseCurrentlyDisplayedSprFrameIndex();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameOffsetXMinusButton:
                        {
                            commandVM?.OnDecreaseFrameOffsetXButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameOffsetXPlusButton:
                        {
                            commandVM?.OnIncreaseFrameOffsetXButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameOffsetYMinusButton:
                        {
                            commandVM?.OnDecreaseFrameOffsetYButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameOffsetYPlusButton:
                        {
                            commandVM?.OnIncreaseFrameOffsetYButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_IntervalPlusButton:
                        {
                            commandVM?.OnIncreaseIntervalButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_IntervalMinusButton:
                        {
                            commandVM?.OnDecreaseIntervalButtonClicked();
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameWidthMinusButton:
                        {
                            commandVM?.OnDecreaseFrameWidthButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameWidthPlusButton:
                        {
                            commandVM?.OnIncreaseFrameWidthButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameHeightMinusButton:
                        {
                            commandVM?.OnDecreaseFrameHeightButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_FrameHeightPlusButton:
                        {
                            commandVM?.OnIncreaseFrameHeightButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalOffsetXMinusButton:
                        {
                            commandVM?.OnDecreaseSprGlobalOffsetXButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalOffsetXPlusButton:
                        {
                            commandVM?.OnIncreaseSprGlobalOffsetXButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalOffsetYMinusButton:
                        {
                            commandVM?.OnDecreaseSprGlobalOffsetYButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalOffsetYPlusButton:
                        {
                            commandVM?.OnIncreaseSprGlobalOffsetYButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalWidthMinusButton:
                        {
                            commandVM?.OnDecreaseSprGlobalWidthButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalWidthPlusButton:
                        {
                            commandVM?.OnIncreaseSprGlobalWidthButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalHeightMinusButton:
                        {
                            commandVM?.OnDecreaseSprGlobalHeightButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case DebugPageTagID.SPRInfo_GlobalHeightPlusButton:
                        {
                            commandVM?.OnIncreaseSprGlobalHeightButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                }
            });
        }
    }
}
