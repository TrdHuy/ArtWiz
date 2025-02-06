using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

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
        private ScrollViewer? mTextBoxScrollViewerCache = null;
        private CodeBlockTextBoxDraggingController mCodeBlockCaretController;
        private CustomCaretAdorner mAdorner;
        private ScrollViewer? mTextBoxScrollViewer
        {
            get
            {
                if (mTextBoxScrollViewerCache == null)
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
                    mTextBoxScrollViewerCache = FindScrollViewer(ContentTextBox);
                }
                return mTextBoxScrollViewerCache;
            }
        }
        public CodeBlock()
        {
            InitializeComponent();
            mCodeBlockCaretController = new CodeBlockTextBoxDraggingController(ContentTextBox);
            mAdorner = new CustomCaretAdorner(mCodeBlockCaretController);
            Loaded += CodeBlock_Loaded;
            Unloaded += CodeBlock_Unloaded;
        }

        private void AttachCustomCaret()
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(ContentTextBox);
            if (adornerLayer != null)
            {
                mAdorner = new CustomCaretAdorner(mCodeBlockCaretController);
                adornerLayer.Add(mAdorner);
            }
        }

        private void CodeBlock_Unloaded(object sender, RoutedEventArgs e)
        {
            if (mAdorner != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(ContentTextBox);
                adornerLayer?.Remove(mAdorner);
            }
        }

        private void CodeBlock_Loaded(object sender, RoutedEventArgs e)
        {
            //Text = "1\n2\n3\n4\n5\n1\n2\n3\n4\n5\n1\n2\n3\n4\n5\n1\n2\n3\n4\n5\n1\n2\n3\n4\n5\n1\n2\n3\n4\n5\n";
            if (mTextBoxScrollViewer != null)
                mTextBoxScrollViewer.ScrollChanged += CodeTextBoxScrollViewer_ScrollChanged;
            AttachCustomCaret();
        }

        private void CodeTextBoxScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
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

        protected override Size MeasureOverride(Size constraint)
        {
            var res = base.MeasureOverride(constraint);
            _viewportWidth = ContentTextBox.ViewportWidth;
            _extentWidth = ContentTextBox.ExtentWidth;
            ScrollOwner.InvalidateScrollInfo();
            return res;
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
            //Debug.WriteLine($"huytd1: VerticalChange={e.VerticalChange} " +
            //    $"VerticalOffset={e.VerticalOffset} " +
            //    $"ExtentHeightChange={e.ExtentHeightChange} " +
            //    $"ExtentHeight={e.ExtentHeight} " +
            //    $"ViewportHeight={e.ViewportHeight} " +
            //    $"ViewportHeightChange={e.ViewportHeightChange}");
            if (e.VerticalChange != 0)
                mTextBoxScrollViewer?.ScrollToVerticalOffset(e.VerticalOffset);
            if (e.HorizontalChange != 0)
                mTextBoxScrollViewer?.ScrollToHorizontalOffset(e.HorizontalOffset);
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

    public class CodeBlockTextBoxDraggingController
    {
        private TextBox ContentTextBox;
        public CodeBlockTextBoxDraggingController(TextBox codeTextBox)
        {
            ContentTextBox = codeTextBox;
            ContentTextBox.SelectionChanged += ContentTextBox_SelectionChanged;
            ContentTextBox.PreviewMouseMove += ContentTextBox_PreviewMouseMove;
        }

        public TextBox CodeBlockTextBox { get => ContentTextBox; }
        public bool IsMouseDraggingUp { get => _isMouseDraggingUp; }
        public bool IsMouseDraggingLeft { get => _isMouseDraggingLeft; }
        public int SelectionStart { get => _selectionStart; }
        public int SelectionEnd { get => _selectionEnd; }
        public int SelectionLength { get => ContentTextBox.SelectionLength; }

        private int _selectionStart = 0;
        private int _selectionEnd = 0;
        private bool _isMouseDraggingUp = false;
        private bool _isMouseDraggingLeft = false;
        private double _lastMouseY = 0;
        private double _lastMouseX = 0;

        private void ContentTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (ContentTextBox.SelectionLength > 0)
            {
                _selectionStart = ContentTextBox.SelectionStart;
                _selectionEnd = _selectionStart + ContentTextBox.SelectionLength;
            }
        }

        // Bắt sự kiện MouseMove để xác định hướng kéo
        private void ContentTextBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine($"huytd1: ContentTextBox_PreviewMouseMove e.LeftButton={e.LeftButton}");

            if (e.LeftButton == MouseButtonState.Pressed && ContentTextBox.SelectionLength > 0)
            {
                // Lấy vị trí con trỏ chuột tương đối trong TextBox
                var currentPoint = e.GetPosition(ContentTextBox);
                _isMouseDraggingUp = currentPoint.Y < _lastMouseY;
                _isMouseDraggingLeft = currentPoint.X < _lastMouseX;
                _lastMouseY = currentPoint.Y;
                _lastMouseX = currentPoint.X;
            }
            else if (ContentTextBox.SelectionLength == 0)
            {
                _isMouseDraggingUp = false;
                _isMouseDraggingLeft = false;
            }
        }

    }

    public class CustomCaretAdorner : Adorner
    {
        private TextBox _textBox;
        private Pen _caretPen;
        private double _caretHeight;
        private double _caretAbsX;
        private double _caretAbsY;
        private bool _isCaretVisible = true;
        private DispatcherTimer _caretBlinkTimer;
        private ScrollViewer? _textBoxScrollViewerCache;
        private ScrollViewer? mTextBoxScrollViewer
        {
            get
            {
                if (_textBoxScrollViewerCache == null)
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
                    _textBoxScrollViewerCache = FindScrollViewer(_textBox);
                }
                return _textBoxScrollViewerCache;
            }
        }
        private CodeBlockTextBoxDraggingController mCodeBlockTextBoxDraggingController;
        public CustomCaretAdorner(CodeBlockTextBoxDraggingController codeBlockTextBoxDraggingController) : base(codeBlockTextBoxDraggingController.CodeBlockTextBox)
        {
            mCodeBlockTextBoxDraggingController = codeBlockTextBoxDraggingController;
            _textBox = codeBlockTextBoxDraggingController.CodeBlockTextBox;
            _caretPen = new Pen(Brushes.Red, 2);
            // Ẩn caret cũ
            _textBox.CaretBrush = new SolidColorBrush(Colors.Transparent);

            _textBox.SelectionChanged += UpdateCaretPosition;
            _textBox.TextChanged += UpdateCaretPosition;
            _textBox.Loaded += OnTexboxLoaded;
            _textBox.GotFocus += StartCaretBlinking;
            _textBox.LostFocus += StopCaretBlinking;

            _caretBlinkTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _caretBlinkTimer.Tick += (s, e) => ToggleCaretVisibility();
        }

        private void OnTexboxLoaded(object sender, RoutedEventArgs e)
        {
            if (mTextBoxScrollViewer != null)
                mTextBoxScrollViewer.ScrollChanged += OnTextBoxScrollViewerChanged;
        }

        private void OnTextBoxScrollViewerChanged(object sender, ScrollChangedEventArgs e)
        {
            InvalidateVisual();
        }

        private void UpdateCaretPosition(object sender, EventArgs e)
        {
            if (_textBox.IsFocused)
            {
                var caretRect = _textBox.GetRectFromCharacterIndex(_textBox.CaretIndex);
                _caretAbsX = caretRect.X + _textBox.HorizontalOffset;
                _caretAbsY = caretRect.Y + _textBox.VerticalOffset;
                _caretHeight = caretRect.Height;
                InvalidateVisual();
            }
        }


        private void ToggleCaretVisibility()
        {
            _isCaretVisible = !_isCaretVisible;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (_textBox.IsFocused && _isCaretVisible)
            {
                if (mCodeBlockTextBoxDraggingController.SelectionLength > 0)
                {
                    var caretIndex = _textBox.CaretIndex;
                    if (mCodeBlockTextBoxDraggingController.IsMouseDraggingUp ||
                        mCodeBlockTextBoxDraggingController.IsMouseDraggingLeft)
                    {
                        caretIndex = mCodeBlockTextBoxDraggingController.SelectionStart;
                    }
                    else
                    {
                        caretIndex = mCodeBlockTextBoxDraggingController.SelectionEnd;
                    }
                    var caretRect = _textBox.GetRectFromCharacterIndex(caretIndex);
                    var caretAbsX = caretRect.X + _textBox.HorizontalOffset;
                    var caretAbsY = caretRect.Y + _textBox.VerticalOffset;
                    var caretHeight = caretRect.Height;
                    dc.DrawLine(_caretPen, new Point(caretAbsX - _textBox.HorizontalOffset, caretAbsY - _textBox.VerticalOffset),
                        new Point(caretAbsX - _textBox.HorizontalOffset, caretAbsY + caretHeight - _textBox.VerticalOffset));
                }
                else
                {
                    dc.DrawLine(_caretPen, new Point(_caretAbsX - _textBox.HorizontalOffset, _caretAbsY - _textBox.VerticalOffset),
                        new Point(_caretAbsX - _textBox.HorizontalOffset, _caretAbsY + _caretHeight - _textBox.VerticalOffset));
                }
            }
        }

        private void StartCaretBlinking(object sender, RoutedEventArgs e)
        {
            _caretBlinkTimer.Start();
            _isCaretVisible = true;
            InvalidateVisual();
        }

        private void StopCaretBlinking(object sender, RoutedEventArgs e)
        {
            _caretBlinkTimer.Stop();
            _isCaretVisible = false;
            InvalidateVisual();
        }
    }
}