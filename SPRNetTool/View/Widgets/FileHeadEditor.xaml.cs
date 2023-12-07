using SPRNetTool.Utils;
using SPRNetTool.View.Pages;
using SPRNetTool.ViewModel.CommandVM;
using SPRNetTool.ViewModel.Widgets;
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
    public enum FileHeadEditorTagId
    {
        SPRInfo_GlobalWidthParam,
        SPRInfo_GlobalHeightParam,
        SPRInfo_GlobalOffsetXParam,
        SPRInfo_GlobalOffsetYParam,
        SPRInfo_FrameWidthParam,
        SPRInfo_FrameHeightParam,
        SPRInfo_FrameOffXParam,
        SPRInfo_FrameOffYParam,
        SPRInfo_FrameIndexParam,
        SPRInfo_IntervalParam,
    }

    public partial class FileHeadEditor : UserControl
    {
        private IDebugPageCommand? commandVM;

        public static readonly DependencyProperty ViewModelProperty =
           DependencyProperty.Register(
               "ViewModel",
               typeof(IFileHeadEditorViewModel),
               typeof(FileHeadEditor),
               new FrameworkPropertyMetadata(default(IFileHeadEditorViewModel),
                   FrameworkPropertyMetadataOptions.AffectsMeasure, propertyChangedCallback: OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public IFileHeadEditorViewModel? ViewModel
        {
            get { return GetValue(ViewModelProperty) as IFileHeadEditorViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        public FileHeadEditor()
        {
            DataContextChanged += FileHeadEditor_DataContextChanged;
            InitializeComponent();
        }

        private void FileHeadEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            commandVM = DataContext.IfIsThenAlso<IDebugPageCommand>((it) => it);
        }

        private void OnPlusParamClick(object sender, RoutedEventArgs e)
        {
            (sender as ParamEditor)?.Tag.IfIs<FileHeadEditorTagId>((tag) =>
            {
                switch (tag)
                {
                    case FileHeadEditorTagId.SPRInfo_GlobalWidthParam:
                        {
                            commandVM?.OnIncreaseSprGlobalWidthButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalHeightParam:
                        {
                            commandVM?.OnIncreaseSprGlobalHeightButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalOffsetXParam:
                        {
                            commandVM?.OnIncreaseSprGlobalOffsetXButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalOffsetYParam:
                        {
                            commandVM?.OnIncreaseSprGlobalOffsetYButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_IntervalParam:
                        {
                            commandVM?.OnIncreaseIntervalButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameIndexParam:
                        {
                            commandVM?.OnIncreaseCurrentlyDisplayedSprFrameIndex();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameWidthParam:
                        {
                            commandVM?.OnIncreaseFrameWidthButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameHeightParam:
                        {
                            commandVM?.OnIncreaseFrameHeightButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameOffXParam:
                        {
                            commandVM?.OnIncreaseFrameOffsetXButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameOffYParam:
                        {
                            commandVM?.OnIncreaseFrameOffsetYButtonClicked();
                            break;
                        }
                }
            });
        }

        private void OnMinusParamClick(object sender, RoutedEventArgs e)
        {
            (sender as ParamEditor)?.Tag.IfIs<FileHeadEditorTagId>((tag) =>
            {
                switch (tag)
                {
                    case FileHeadEditorTagId.SPRInfo_GlobalWidthParam:
                        {
                            commandVM?.OnDecreaseSprGlobalWidthButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalHeightParam:
                        {
                            commandVM?.OnDecreaseSprGlobalHeightButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalOffsetXParam:
                        {
                            commandVM?.OnDecreaseSprGlobalOffsetXButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalOffsetYParam:
                        {
                            commandVM?.OnDecreaseSprGlobalOffsetYButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_IntervalParam:
                        {
                            commandVM?.OnDecreaseIntervalButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameIndexParam:
                        {
                            commandVM?.OnDecreaseCurrentlyDisplayedSprFrameIndex();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameWidthParam:
                        {
                            commandVM?.OnDecreaseFrameWidthButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameHeightParam:
                        {
                            commandVM?.OnDecreaseFrameHeightButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameOffXParam:
                        {
                            commandVM?.OnDecreaseFrameOffsetXButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameOffYParam:
                        {
                            commandVM?.OnDecreaseFrameOffsetYButtonClicked();
                            break;
                        }
                }
            });
        }

        private void OnPreviewTextContentParamUpdated(object sender, TextContentUpdatedEventArgs e)
        {
            (sender as ParamEditor)?.Tag.IfIs<FileHeadEditorTagId>(tag =>
            {
                switch (tag)
                {
                    case FileHeadEditorTagId.SPRInfo_GlobalWidthParam:
                        commandVM?.SetSprGlobalSize(width: (ushort)Convert.ToUInt32(e.NewText));
                        break;
                    case FileHeadEditorTagId.SPRInfo_GlobalHeightParam:
                        commandVM?.SetSprGlobalSize(height: (ushort)Convert.ToUInt32(e.NewText));
                        break;
                    case FileHeadEditorTagId.SPRInfo_GlobalOffsetXParam:
                        commandVM?.SetSprGlobalOffset(offX: (short)Convert.ToInt32(e.NewText));
                        break;
                    case FileHeadEditorTagId.SPRInfo_GlobalOffsetYParam:
                        commandVM?.SetSprGlobalOffset(offY: (short)Convert.ToInt32(e.NewText));
                        break;
                    case FileHeadEditorTagId.SPRInfo_FrameHeightParam:
                        commandVM?.SetSprFrameSize(height: (ushort)Convert.ToInt32(e.NewText));
                        break;
                    case FileHeadEditorTagId.SPRInfo_FrameWidthParam:
                        commandVM?.SetSprFrameSize(width: (ushort)Convert.ToInt32(e.NewText));
                        break;
                    case FileHeadEditorTagId.SPRInfo_FrameOffYParam:
                        commandVM?.SetSprFrameOffset(offY: (short)Convert.ToInt32(e.NewText));
                        break;
                    case FileHeadEditorTagId.SPRInfo_FrameOffXParam:
                        commandVM?.SetSprFrameOffset(offX: (short)Convert.ToInt32(e.NewText));
                        break;
                    case FileHeadEditorTagId.SPRInfo_IntervalParam:
                        commandVM?.SetSprInterval((ushort)Convert.ToInt32(e.NewText));
                        break;
                }
                e.Handled = true;
            });
        }

        private void OnPlusMouseHold(object sender, Utils.MouseHoldEventArgs args)
        {
            (sender as ParamEditor)?.Tag.IfIs<FileHeadEditorTagId>((tag) =>
            {
                switch (tag)
                {

                    case FileHeadEditorTagId.SPRInfo_FrameIndexParam:
                        {
                            commandVM?.OnIncreaseCurrentlyDisplayedSprFrameIndex();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameOffXParam:
                        {
                            commandVM?.OnIncreaseFrameOffsetXButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameOffYParam:
                        {
                            commandVM?.OnIncreaseFrameOffsetYButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }

                    case FileHeadEditorTagId.SPRInfo_IntervalParam:
                        {
                            commandVM?.OnIncreaseIntervalButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameWidthParam:
                        {
                            commandVM?.OnIncreaseFrameWidthButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameHeightParam:
                        {
                            commandVM?.OnIncreaseFrameHeightButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }

                    case FileHeadEditorTagId.SPRInfo_GlobalOffsetXParam:
                        {
                            commandVM?.OnIncreaseSprGlobalOffsetXButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalOffsetYParam:
                        {
                            commandVM?.OnIncreaseSprGlobalOffsetYButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalWidthParam:
                        {
                            commandVM?.OnIncreaseSprGlobalWidthButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalHeightParam:
                        {
                            commandVM?.OnIncreaseSprGlobalHeightButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                }
            });
        }

        private void OnMinusMouseHold(object sender, Utils.MouseHoldEventArgs args)
        {
            (sender as ParamEditor)?.Tag.IfIs<FileHeadEditorTagId>((tag) =>
            {
                switch (tag)
                {

                    case FileHeadEditorTagId.SPRInfo_FrameIndexParam:
                        {
                            commandVM?.OnDecreaseCurrentlyDisplayedSprFrameIndex();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameOffXParam:
                        {
                            commandVM?.OnDecreaseFrameOffsetXButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameOffYParam:
                        {
                            commandVM?.OnDecreaseFrameOffsetYButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }

                    case FileHeadEditorTagId.SPRInfo_IntervalParam:
                        {
                            commandVM?.OnDecreaseIntervalButtonClicked();
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameWidthParam:
                        {
                            commandVM?.OnDecreaseFrameWidthButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_FrameHeightParam:
                        {
                            commandVM?.OnDecreaseFrameHeightButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }

                    case FileHeadEditorTagId.SPRInfo_GlobalOffsetXParam:
                        {
                            commandVM?.OnDecreaseSprGlobalOffsetXButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalOffsetYParam:
                        {
                            commandVM?.OnDecreaseSprGlobalOffsetYButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalWidthParam:
                        {
                            commandVM?.OnDecreaseSprGlobalWidthButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                    case FileHeadEditorTagId.SPRInfo_GlobalHeightParam:
                        {
                            commandVM?.OnDecreaseSprGlobalHeightButtonClicked((uint)(1 + args.HoldingCounter / 5));
                            break;
                        }
                }
            });
        }
    }
}
