using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

namespace ArtWiz.View.Widgets.CodeBlock
{
    public class LineNumberViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private double _lineHeight;
        private int _lineIndex;
        public double LineHeight
        {
            get
            {
                return _lineHeight;
            }
            set
            {

                _lineHeight = value;
                OnPropertyChanged("LineHeight");
            }
        }

        public int LineIndex
        {
            get
            {
                return _lineIndex;
            }
            set
            {

                _lineIndex = value;
                OnPropertyChanged("LineIndex");
            }
        }

    }

    /// <summary>
    /// Interaction logic for CodeBlock.xaml
    /// </summary>
    public partial class CodeBlock : VerticalVirtualizingPanel
    {
        public static readonly DependencyProperty TextProperty =
           DependencyProperty.Register("Text",
               typeof(string),
               typeof(CodeBlock),
               new PropertyMetadata("", OnTextPropChanged));

        private static void OnTextPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CodeBlock cb)
            {
                cb.ContentTextBox.Text = e.NewValue as string;
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty NumericLineHeightProperty =
           DependencyProperty.Register("NumericLineHeight",
               typeof(double),
               typeof(CodeBlock),
               new PropertyMetadata(double.NaN));

        public double NumericLineHeight
        {
            get { return (double)GetValue(NumericLineHeightProperty); }
            set { SetValue(NumericLineHeightProperty, value); }
        }

        public override Canvas PART_ContentCanvasContainer => ContentCanvasContainer;

        public override Canvas PART_MainCanvasContainer => MainCanvasContainer;

        public override FrameworkElement CreateItemView()
        {
            var ele = base.CreateItemView();
            if (_desiredItemSize != Size.Empty && _desiredItemSize.Width != 0)
            {
                ele.Width = _desiredItemSize.Width;
            }
            return ele;
        }
        private ObservableCollection<LineNumberViewModel> mInternalLineNumberSource = new ObservableCollection<LineNumberViewModel>();
        private ScrollViewer? _mTextBoxScrollViewerCache = null;
        private ScrollViewer? mTextBoxScrollViewer
        {
            get
            {
                if (_mTextBoxScrollViewerCache == null)
                {
                    ScrollViewer? FindScrollViewer(DependencyObject parent)
                    {
                        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                        {
                            var child = VisualTreeHelper.GetChild(parent, i);
                            if (child is ScrollViewer scrollViewer)
                            {
                                return scrollViewer;
                            }
                            else
                            {
                                var result = FindScrollViewer(child);
                                if (result != null) return result;
                            }
                        }
                        return null;
                    }
                    _mTextBoxScrollViewerCache = FindScrollViewer(ContentTextBox);
                }
                return _mTextBoxScrollViewerCache;
            }
        }
        public CodeBlock()
        {
            InitializeComponent();
            ContentTextBox.Loaded += ContentTextBox_Initialized;
        }

        private void MTextBoxScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                ScrollOwner.ScrollToVerticalOffset(e.VerticalOffset);
            }

            //var caretRect = ContentTextBox.GetRectFromCharacterIndex(ContentTextBox.CaretIndex);
            //var lineHeight = caretRect.Height;
            //var absoluteCaretY = caretRect.Y + mTextBoxScrollViewer?.VerticalOffset ?? 0;
            //var newCaretLineIndex = absoluteCaretY / lineHeight + 1;
            //if (newCaretLineIndex != CurrentCaretLineIndex)
            //{
            //    CurrentCaretLineIndex = (int)newCaretLineIndex;

            //    var visibleRect = new Rect(HorizontalOffset, VerticalOffset, ViewportWidth, ViewportHeight);
            //    caretRect.Y = absoluteCaretY;

            //    Debug.WriteLine($"CurrentCaretLineIndex={CurrentCaretLineIndex}");
            //    var isContain = visibleRect.Contains(caretRect);
            //    if (!isContain && mTextBoxScrollViewer != null)
            //    {
            //        double targetOffset;

            //        if (absoluteCaretY < visibleRect.Y)
            //        {
            //            // Caret nằm trên vùng nhìn thấy, cuộn lên trên
            //            targetOffset = absoluteCaretY;
            //        }
            //        else
            //        {
            //            // Caret nằm dưới vùng nhìn thấy, cuộn xuống dưới
            //            targetOffset = absoluteCaretY - ViewportHeight + lineHeight;
            //        }

            //        // Cuộn tới vị trí cần thiết
            //        ScrollOwner.ScrollToVerticalOffset(targetOffset);
            //    }
            //}
        }

        private void ContentTextBox_Initialized(object? sender, EventArgs e)
        {
            Text = "1\n2\n3\n4\n5\n1\n2\n3\n4\n5\n1\n2\n3\n4\n5\n1\n2\n3\n4\n5\n1\n2\n3\n4\n5\n1\n2\n3\n4\n5\n";
            mTextBoxScrollViewer.ScrollChanged += MTextBoxScrollViewer_ScrollChanged;
        }

        protected override void SetDesiredItemSize(Size constraintSize, Size desiredSize)
        {
            //Set cái này để kích thước giữa các item bằng chiều cao của line height
            desiredSize.Height = MeasureLineHeight();
            desiredSize.Width = PART_MainCanvasContainer.ActualWidth;
            base.SetDesiredItemSize(constraintSize, desiredSize);
        }


        #region VirtualPanel scroll management
        protected override void OnScrollOwnerChanged(ScrollViewer? oldViewer, ScrollViewer? newViewer)
        {
            base.OnScrollOwnerChanged(oldViewer, newViewer);
            if (oldViewer != null)
                oldViewer.ScrollChanged -= ScrollOwner_ScrollChanged;
            if (newViewer != null)
                newViewer.ScrollChanged += ScrollOwner_ScrollChanged;
        }

        private void ScrollOwner_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Debug.WriteLine($"huytd1: VerticalChange={e.VerticalChange} " +
                $"VerticalOffset={e.VerticalOffset} " +
                $"ExtentHeightChange={e.ExtentHeightChange} " +
                $"ExtentHeight={e.ExtentHeight} " +
                $"ViewportHeight={e.ViewportHeight} " +
                $"ViewportHeightChange={e.ViewportHeightChange}");
            if (e.VerticalChange != 0)
                mTextBoxScrollViewer?.ScrollToVerticalOffset(e.VerticalOffset);
        }
        #endregion

        #region Text content management
        private void ContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLineNumbers();
        }

        private void UpdateLineNumbers()
        {
            if (ContentTextBox == null /*|| LineNumberListBox == null*/) return;
            // Tính chiều cao của một dòng trong TextBox
            NumericLineHeight = MeasureLineHeight();
            // Lấy số lượng dòng hiện tại trong TextBox
            int lineCount = ContentTextBox.LineCount;

            mInternalLineNumberSource = new ObservableCollection<LineNumberViewModel>();
            for (int i = 1; i <= lineCount; i++)
            {
                mInternalLineNumberSource.Add(new LineNumberViewModel() { LineHeight = NumericLineHeight, LineIndex = i });
            }
            SetItems(mInternalLineNumberSource);
        }


        #endregion

        #region Caret Index management
        private int CurrentCaretLineIndex { get; set; } = -1;
        private void ContentTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
        private double GetCaretVerticalOffset()
        {
            if (ContentTextBox == null) return 0;

            var rect = ContentTextBox.GetRectFromCharacterIndex(ContentTextBox.CaretIndex);
            return rect.Top + mTextBoxScrollViewer?.VerticalOffset ?? 0;
        }
        #endregion


        private double MeasureLineHeight()
        {
            var formattedText = new FormattedText(
                "Sample",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                ContentTextBox.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);
            double lineHeight = formattedText.Height;
            return lineHeight;
        }


        private void ContentTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            double newOffset = ScrollOwner.VerticalOffset - (e.Delta / 3.0);
            ScrollOwner.ScrollToVerticalOffset(newOffset);
        }
    }
}
