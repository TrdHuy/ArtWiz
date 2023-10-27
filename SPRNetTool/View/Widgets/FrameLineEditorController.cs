﻿using SPRNetTool.LogUtil;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace SPRNetTool.View.Widgets
{
    internal class EllipseController : IDisposable
    {
        public delegate void EllipseHandler(EllipseController sender);
        public delegate void OnDraggingMouseEllipseHandler(EllipseController sender, double X, double Y, MouseEventArgs e);
        public delegate void OnDraggingMouseUpEnteredEllipseHandler(EllipseController sender, EllipseController enteredEllipse);
        private bool isEllipseDraggedEnter;
        private Size currentSize;
        public Point currentPost = new Point(-1, -1);

        public Size Size { get => currentSize; }
        public Point Position { get => currentPost; }

        public Ellipse MainEllipse { get; private set; }
        public Canvas ContainerCanvas { get; private set; }
        public Ellipse? DraggedMouseEllipse { get; private set; }
        public uint CurrentIndex { get; private set; }

        public event EllipseHandler? PreviewRemovingEllipse;
        public event OnDraggingMouseEllipseHandler? OnDraggingMouseEllipse;
        public event OnDraggingMouseUpEnteredEllipseHandler? OnDraggingMouseUpEnteredEllipse;
        private Canvas ownerCanvas;
        private bool isMousePressed = false;
        private EllipseController? dragEnteredEllipse;
        private TextBlock contentView;
        private uint initIndex;
        private uint frameCount;
        private EllipseController(Canvas ownerCanvas, uint index, uint frameCount)
        {
            this.initIndex = index;
            this.CurrentIndex = index;
            this.frameCount = frameCount;
            MainEllipse = new Ellipse()
            {
                MinWidth = 5,
                MinHeight = 5,
                Width = 20,
                Height = 20,
                Stroke = new SolidColorBrush(Color.FromArgb(0xff, 0x2B, 0xC8, 0xc8)),
                StrokeThickness = 0,
                Fill = Brushes.Black,
            };
            this.ownerCanvas = ownerCanvas;
            contentView = new TextBlock();
            contentView.Text = index.ToString();
            contentView.Foreground = Brushes.Red;

            ContainerCanvas = new Canvas();
            ContainerCanvas.Children.Add(MainEllipse);
            ContainerCanvas.Children.Add(contentView);
            ContainerCanvas.MouseLeftButtonDown += ContainerCanvas_MouseLeftButtonDown;
            ContainerCanvas.MouseLeave += ContainerCanvas_MouseLeave;
            ContainerCanvas.MouseEnter += ContainerCanvas_MouseEnter;
            ContainerCanvas.Loaded += ContainerCanvas_Loaded;
            ContainerCanvas.ContextMenu = new ContextMenu();

            var removeFrameMenuItem = new MenuItem()
            {
                Header = "Remove this frame",
            };
            removeFrameMenuItem.Click += (s, e) =>
            {
                PreviewRemovingEllipse?.Invoke(this);
            };
            ContainerCanvas.ContextMenu.Items.Add(removeFrameMenuItem);
        }


        public void Dispose()
        {
            ContainerCanvas.Children.Remove(MainEllipse);
            ContainerCanvas.Children.Remove(contentView);
            ContainerCanvas.MouseLeftButtonDown -= ContainerCanvas_MouseLeftButtonDown;
            ContainerCanvas.MouseLeave -= ContainerCanvas_MouseLeave;
            ContainerCanvas.MouseEnter -= ContainerCanvas_MouseEnter;
            ContainerCanvas.Loaded -= ContainerCanvas_Loaded;
        }

        public static EllipseController CreateController(Canvas ownerCanvas, uint index, uint frameCount)
        {
            return new EllipseController(ownerCanvas, index, frameCount);
        }

        public void SetDragEllipseEnter(bool isEnter, EllipseController dragEnteredEllipse)
        {
            if (dragEnteredEllipse.isEllipseDraggedEnter != isEnter)
            {
                Logger.Raw.D($"SetDragEllipseEnter:{initIndex} to {dragEnteredEllipse.initIndex}");
                if (isEnter)
                {
                    dragEnteredEllipse.BeginAnimation(Color.FromArgb(0xff, 0x0D, 0x44, 0x47), 2);
                    this.dragEnteredEllipse = dragEnteredEllipse;
                    Logger.Raw.D($"Entering {dragEnteredEllipse.initIndex}");
                }
                else
                {
                    dragEnteredEllipse.BeginAnimation(Colors.Black, 0);
                    if (this.dragEnteredEllipse == dragEnteredEllipse)
                    {
                        this.dragEnteredEllipse = null;
                    }
                    Logger.Raw.D($"Out {dragEnteredEllipse.initIndex}");

                }
                dragEnteredEllipse.isEllipseDraggedEnter = isEnter;
            }
        }

        public void ChangeDisplayIndex(bool isShowCurrentIndex)
        {
            void handleSizeChangedAfterChangeText(object sender, SizeChangedEventArgs e)
            {
                contentView.SizeChanged -= handleSizeChangedAfterChangeText;
                ArrangeTextContent();
            }
            contentView.SizeChanged += handleSizeChangedAfterChangeText;
            contentView.Text = isShowCurrentIndex ? CurrentIndex.ToString() : initIndex.ToString();
        }


        public void ArrangeEllipse()
        {
            var offset = 20;
            ArrangeTextContent();

            var x = CurrentIndex * ((ownerCanvas.ActualWidth - MainEllipse.ActualWidth - 2 * offset) / (double)(frameCount > 1 ? (frameCount - 1) : 1)) + offset;
            var y = ownerCanvas.ActualHeight * 0.5d - MainEllipse.ActualHeight * 0.5d;
            Canvas.SetLeft(ContainerCanvas, x);
            Canvas.SetTop(ContainerCanvas, y);
            currentPost = new Point(x, y);
        }

        public DoubleAnimation ReArrangeEllipseWithAnimation()
        {
            if (currentPost.X == -1)
            {
                throw new Exception("Ellipse not initialized yet");
            }
            var offset = 20;
            ArrangeTextContent();

            var newX = CurrentIndex * ((ownerCanvas.ActualWidth - MainEllipse.ActualWidth - 2 * offset) / (double)(frameCount > 1 ? (frameCount - 1) : 1)) + offset;
            var newY = ownerCanvas.ActualHeight * 0.5d - MainEllipse.ActualHeight * 0.5d;

            DoubleAnimation xAnim = new DoubleAnimation(newX, TimeSpan.FromSeconds(0.1));
            Storyboard.SetTarget(xAnim, ContainerCanvas);
            Storyboard.SetTargetProperty(xAnim, new PropertyPath("(Canvas.Left)"));
            xAnim.Completed += (s, e) =>
            {
                Canvas.SetLeft(ContainerCanvas, newX);
                currentPost = new Point(newX, newY);
                Canvas.SetTop(ContainerCanvas, newY);
            };
            xAnim.FillBehavior = FillBehavior.Stop;
            return xAnim;
        }

        public void SwitchEllipsePos(EllipseController otherEllipse)
        {
            var oldIndex = CurrentIndex;
            CurrentIndex = otherEllipse.CurrentIndex;
            otherEllipse.CurrentIndex = oldIndex;
            ArrangeEllipse();
            otherEllipse.ArrangeEllipse();
        }

        public DoubleAnimation[] SwitchEllipsePosWithAnimation(EllipseController otherEllipse)
        {
            var oldIndex = CurrentIndex;
            CurrentIndex = otherEllipse.CurrentIndex;
            otherEllipse.CurrentIndex = oldIndex;
            var anim1 = ReArrangeEllipseWithAnimation();
            var anim2 = otherEllipse.ReArrangeEllipseWithAnimation();
            return new DoubleAnimation[] { anim1, anim2 };
        }

        public void SetNewIndex(uint index)
        {
            CurrentIndex = index;
        }

        public void SetNewFrameCount(uint frameCount)
        {
            this.frameCount = frameCount;
        }

        private void ContainerCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            ArrangeEllipse();
        }

        private void ContainerCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            BeginAnimation(Color.FromArgb(0xff, 0x0D, 0x44, 0x47), 2);
            MainEllipse.Cursor = Cursors.Hand;
        }

        private void ContainerCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isMousePressed)
            {
                BeginAnimation(Colors.Black, 0);
                MainEllipse.Cursor = Cursors.Arrow;
            }
        }

        private void ContainerCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMousePressed = true;
            CreateTempEllipseWhenMouseDown(MainEllipse, ownerCanvas, e);
        }


        private void ArrangeTextContent()
        {
            Canvas.SetLeft(contentView, (MainEllipse.ActualWidth / 2) - (contentView.ActualWidth / 2));
            Canvas.SetTop(contentView, (MainEllipse.ActualHeight / 2) - (contentView.ActualHeight / 2));
            currentSize = new Size(MainEllipse.ActualWidth, MainEllipse.ActualHeight);
        }

        private void BeginAnimation(Color toColor, double toThickness)
        {
            var animationStoryboard = new Storyboard();
            ColorAnimation fillAnimation = new ColorAnimation(toColor, TimeSpan.FromSeconds(0.1));
            Storyboard.SetTarget(fillAnimation, MainEllipse);
            Storyboard.SetTargetProperty(fillAnimation, new PropertyPath("(Ellipse.Fill).(SolidColorBrush.Color)"));
            animationStoryboard.Children.Add(fillAnimation);
            DoubleAnimation strokeThicknessAnimation = new DoubleAnimation(toThickness, TimeSpan.FromSeconds(0.1));
            Storyboard.SetTarget(strokeThicknessAnimation, MainEllipse);
            Storyboard.SetTargetProperty(strokeThicknessAnimation, new PropertyPath("(Ellipse.StrokeThickness)"));
            animationStoryboard.Children.Add(strokeThicknessAnimation);
            animationStoryboard.FillBehavior = FillBehavior.HoldEnd;
            MainEllipse.BeginStoryboard(animationStoryboard);
        }

        private void CreateTempEllipseWhenMouseDown(Ellipse referEllipse, Canvas referCanvasContainer, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(referCanvasContainer);

            var tempEllipse = new Ellipse()
            {
                Height = referEllipse.ActualHeight,
                Width = referEllipse.ActualWidth,
                Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0xD8, 0xD8)),
                Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x5e, 0x5e, 0x5e)),
                StrokeThickness = 2,

            };

            tempEllipse.MouseLeftButtonUp += MouseLeftButtonUp;
            tempEllipse.MouseMove += MouseMove;

            void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                if (dragEnteredEllipse != null)
                {
                    Logger.Raw.D($"Up on: {dragEnteredEllipse.initIndex}");

                    OnDraggingMouseUpEnteredEllipse?.Invoke(this, dragEnteredEllipse);
                    dragEnteredEllipse.BeginAnimation(Colors.Black, 0);
                    dragEnteredEllipse.isEllipseDraggedEnter = false;
                    dragEnteredEllipse = null;
                }

                tempEllipse.Cursor = Cursors.Arrow;
                isMousePressed = false;
                tempEllipse.ReleaseMouseCapture();
                referCanvasContainer.Children.Remove(tempEllipse);
                ContainerCanvas_MouseLeave(referEllipse, e);
                DraggedMouseEllipse = null;
                tempEllipse.MouseLeftButtonUp -= MouseLeftButtonUp;
                tempEllipse.MouseMove -= MouseMove;
            }

            void MouseMove(object sender, MouseEventArgs e)
            {
                if (isMousePressed)
                {
                    Point newPosition = e.GetPosition(referCanvasContainer);
                    double deltaX = newPosition.X - mousePosition.X;
                    double deltaY = newPosition.Y - mousePosition.Y;

                    double newLeft = Canvas.GetLeft(tempEllipse) + deltaX;
                    double newTop = Canvas.GetTop(tempEllipse) + deltaY;

                    Canvas.SetLeft(tempEllipse, newLeft);
                    Canvas.SetTop(tempEllipse, newTop);

                    mousePosition = newPosition;
                    OnDraggingMouseEllipse?.Invoke(this, newLeft, newTop, e);
                }
            }

            referCanvasContainer.Children.Add(tempEllipse);
            Canvas.SetLeft(tempEllipse, Canvas.GetLeft(ContainerCanvas));
            Canvas.SetTop(tempEllipse, Canvas.GetTop(ContainerCanvas));
            tempEllipse.CaptureMouse();
            tempEllipse.Cursor = Cursors.Hand;
            DraggedMouseEllipse = tempEllipse;
        }

    }

    internal class FrameLineContextMenuController : IDisposable
    {
        public delegate void FrameLineContextMenuHandler(int newIndex, double cursorX, double cursorY);

        public event FrameLineContextMenuHandler? PreviewAddingNewFrame;
        private ContextMenu editorMenu;
        private Canvas containerCanvas;
        MenuItem insertFrameItem;
        private double contextMenuPosX;
        private double contextMenuPosY;
        private List<EllipseController> ellipseControllersCache;
        private Rect leftOffsetCanvasRect;
        private Rect rightOffsetCanvasRect;

        public FrameLineContextMenuController(List<EllipseController> ellipseControllersCache, Canvas containerCanvas)
        {
            this.containerCanvas = containerCanvas;
            this.ellipseControllersCache = ellipseControllersCache;

            editorMenu = new ContextMenu();
            insertFrameItem = new MenuItem() { Header = "Insert new frame" };
            editorMenu.Items.Add(insertFrameItem);

            containerCanvas.ContextMenu = editorMenu;
            containerCanvas.ContextMenuOpening += ContainerCanvas_ContextMenuOpening;
            insertFrameItem.Click += InsertFrameItem_Click;
            containerCanvas.SizeChanged += ContainerCanvas_SizeChanged;
        }

        private void ContainerCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            leftOffsetCanvasRect = new Rect(x: 0, y: 0,
                width: FrameLineController.HEAD_OFFSET,
                height: containerCanvas.ActualHeight);
            rightOffsetCanvasRect = new Rect(x: containerCanvas.ActualWidth - FrameLineController.HEAD_OFFSET,
                y: 0,
                width: FrameLineController.HEAD_OFFSET,
                height: containerCanvas.ActualHeight);
        }

        private void InsertFrameItem_Click(object sender, RoutedEventArgs e)
        {
            var headOffset = FrameLineController.HEAD_OFFSET;
            var ellipseSize = FrameLineController.SIZE_PER_ELLIPSE;
            var frameCount = ellipseControllersCache.Count;
            var containerWidth = containerCanvas.ActualWidth;
            if (frameCount == 0)
            {
                PreviewAddingNewFrame?.Invoke(0, contextMenuPosX, contextMenuPosY);
                return;
            }

            if (IsMouseInZone(new Point(contextMenuPosX, contextMenuPosY), leftOffsetCanvasRect))
            {
                PreviewAddingNewFrame?.Invoke(0, contextMenuPosX, contextMenuPosY);
                return;
            }
            else if (IsMouseInZone(new Point(contextMenuPosX, contextMenuPosY), rightOffsetCanvasRect))
            {
                PreviewAddingNewFrame?.Invoke(frameCount, contextMenuPosX, contextMenuPosY);
                return;
            }

            var ellipseDistance = (containerWidth
                - 2 * headOffset - frameCount * ellipseSize) / (frameCount - 1);
            var ellipseDistanceFromCenter = (containerWidth
                - 2 * headOffset
                - ellipseSize)
                / (frameCount - 1);
            var newFrameIndex = (int)((contextMenuPosX -
                headOffset -
                ellipseSize / 2) / ellipseDistanceFromCenter);

            // kiểm tra Khi click giữa 2 ellipse
            if (contextMenuPosX > (newFrameIndex + 1) * ellipseSize + ellipseDistance * newFrameIndex + headOffset)
            {
                newFrameIndex += 1;
            }
            else if (Double.IsInfinity(ellipseDistance) && frameCount == 1 &&
                contextMenuPosX > ellipseSize + headOffset && contextMenuPosX < containerWidth - headOffset)
            {
                newFrameIndex += 1;
            }

            PreviewAddingNewFrame?.Invoke(newFrameIndex, contextMenuPosX, contextMenuPosY);
        }

        private void ContainerCanvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            contextMenuPosX = e.CursorLeft;
            contextMenuPosY = e.CursorTop;
        }

        public void Dispose()
        {
            containerCanvas.ContextMenuOpening -= ContainerCanvas_ContextMenuOpening;
            insertFrameItem.Click -= InsertFrameItem_Click;
            containerCanvas.SizeChanged -= ContainerCanvas_SizeChanged;
        }

        private bool IsMouseInZone(Point mousePos, Rect zone)
        {
            return zone.Contains(mousePos);
        }
    }

    // TODO: Implement virtualizing canvas to optimize view performance 
    internal class FrameLineController : IDisposable
    {
        public const double SIZE_PER_ELLIPSE = 20d;
        public const double MIN_ELLIPSE_DISTANCE = 5d;
        public const double HEAD_OFFSET = 20d;

        public delegate void OnFrameIndexSwitchedHandler(uint frameIndex1, uint frameIndex2);

        public event OnFrameIndexSwitchedHandler? OnFrameIndexSwitched;
        private Rect leftScrollRect;
        private Rect rightScrollRect;

        private Canvas containerCanvas;
        private ScrollViewer containerScroll;

        private uint initFrameCount;
        private SemaphoreSlim autoScrollSemaphore = new SemaphoreSlim(1, 1);
        private double calculatedFrameLineMinimumWidth = 0;

        private List<EllipseController> ellipseControllersCache
            = new List<EllipseController>();
        private FrameLineContextMenuController contextMenuController;

        public FrameLineController(ScrollViewer containerScroll, Canvas containerCanvas, uint initFrameCount)
        {
            contextMenuController = new FrameLineContextMenuController(ellipseControllersCache, containerCanvas);
            this.containerScroll = containerScroll;
            this.containerCanvas = containerCanvas;
            this.initFrameCount = initFrameCount;
            this.calculatedFrameLineMinimumWidth = CaculateFrameLineMinimumWidth(initFrameCount);
            containerScroll.SizeChanged += ContainerScroll_SizeChanged;
            containerCanvas.SizeChanged += ContainerCanvas_SizeChanged;
            containerCanvas.MouseWheel += ContainerCanvas_MouseWheel;
            contextMenuController.PreviewAddingNewFrame += ContextMenuController_PreviewAddingNewFrame;
        }

        #region override callback
        private void ContextMenuController_PreviewAddingNewFrame(int newIndex, double cursorX, double cursorY)
        {
            InsertFrame((uint)newIndex);
        }

        private void ContainerScroll_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var scrollBar = containerScroll.Template.FindName("PART_HorizontalScrollBar", containerScroll) as ScrollBar;
            leftScrollRect = new Rect(0, 0, 20, containerScroll.ActualHeight - scrollBar?.ActualHeight ?? 0);
            rightScrollRect = new Rect(x: containerScroll.ActualWidth - 20,
                y: 0,
                width: 20,
                height: containerScroll.ActualHeight - scrollBar?.ActualHeight ?? 0);
        }

        private void ContainerCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                var currentFrameLineWidth = containerCanvas.ActualWidth;
                if (e.Delta > 0)
                {
                    currentFrameLineWidth += 10;
                }
                else
                {
                    currentFrameLineWidth -= 10;
                    if (currentFrameLineWidth < calculatedFrameLineMinimumWidth)
                    {
                        currentFrameLineWidth = calculatedFrameLineMinimumWidth;
                    }
                }
                containerCanvas.Width = currentFrameLineWidth;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (e.Delta > 0)
                {
                    containerScroll.ScrollToHorizontalOffset(containerScroll.HorizontalOffset - 10);
                }
                else
                {
                    containerScroll.ScrollToHorizontalOffset(containerScroll.HorizontalOffset + 10);
                }
            }
        }

        private void Controller_OnDraggingMouseUpEnteredEllipse(EllipseController sender,
            EllipseController enteredEllipse)
        {
            var oldIndex = (int)sender.CurrentIndex;
            var newIndex = (int)enteredEllipse.CurrentIndex;

            // No animation switch
            //sender.SwitchEllipsePos(enteredEllipse);
            //ellipseControllersCache[oldIndex] = enteredEllipse;
            //ellipseControllersCache[newIndex] = sender;

            // Switch with animation
            var animationStoryboard = new Storyboard();
            var anim = sender.SwitchEllipsePosWithAnimation(enteredEllipse);
            animationStoryboard.Children.Add(anim[0]);
            animationStoryboard.Children.Add(anim[1]);
            animationStoryboard.FillBehavior = FillBehavior.Stop;
            animationStoryboard.Completed += (s, e) =>
            {
                ellipseControllersCache[oldIndex] = enteredEllipse;
                ellipseControllersCache[newIndex] = sender;
                OnFrameIndexSwitched?.Invoke(sender.CurrentIndex, enteredEllipse.CurrentIndex);
            };
            containerCanvas.BeginStoryboard(animationStoryboard);
        }

        private void Controller_PreviewRemovingEllipse(EllipseController sender)
        {
            RemoveFrame(sender.CurrentIndex);
        }

        private void Controller_OnDraggingMouseEllipse(EllipseController sender, double X, double Y, MouseEventArgs e)
        {
            var posScr = e.GetPosition(containerScroll);
            if (IsMouseInZone(posScr, leftScrollRect))
            {
                EnableAutoHorizontalScroll(isEnable: true, isLeft: true);
            }
            else if (IsMouseInZone(posScr, rightScrollRect))
            {
                EnableAutoHorizontalScroll(isEnable: true, isLeft: false);
            }
            else
            {
                EnableAutoHorizontalScroll(isEnable: false, isLeft: false);
            }

            var isOverlappedAlready = false;
            foreach (var controller in ellipseControllersCache)
            {
                if (controller != sender)
                {
                    if (!isOverlappedAlready && IsOverlap(X, Y, sender.Size.Width, sender.Size.Height,
                        controller.Position.X,
                        controller.Position.Y,
                        controller.Size.Width,
                        controller.Size.Height))
                    {
                        sender.SetDragEllipseEnter(true, controller);
                        isOverlappedAlready = true;
                    }
                    else
                    {
                        sender.SetDragEllipseEnter(false, controller);
                    }
                }
            }
        }

        private void ContainerCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Rearrange
            foreach (var kp in ellipseControllersCache)
            {
                kp.ArrangeEllipse();
            }
        }

        #endregion

        public int FrameCount => ellipseControllersCache.Count;

        public void ChangeDisplayIndex(bool isShowCurrentIndex)
        {
            foreach (var ctr in ellipseControllersCache)
            {
                ctr.ChangeDisplayIndex(isShowCurrentIndex);
            }
        }

        public void SetTotalFrameCount(uint frameCount)
        {
            if (ellipseControllersCache.Count != frameCount)
            {
                calculatedFrameLineMinimumWidth = CaculateFrameLineMinimumWidth(frameCount);
                if (containerCanvas.ActualWidth < calculatedFrameLineMinimumWidth)
                {
                    containerCanvas.Width = calculatedFrameLineMinimumWidth;
                }
            }

            ClearAllEllipse();
            for (uint i = 0; i < frameCount; i++)
            {
                var index = i;
                var controller = CreateAndSetupController(i, frameCount);
                containerCanvas.Children.Add(controller.ContainerCanvas);
                ellipseControllersCache.Add(controller);
            }
        }

        public void RemoveFrame(uint frameIndex)
        {
            if (frameIndex >= ellipseControllersCache.Count)
            {
                throw new Exception();
            }

            var controller = ellipseControllersCache[(int)frameIndex];
            containerCanvas.Children.Remove(controller.ContainerCanvas);
            ellipseControllersCache.Remove(controller);
            controller.Dispose();

            var newCount = ellipseControllersCache.Count;

            // Re-calculate frame line min with for resize canvas feature
            calculatedFrameLineMinimumWidth = CaculateFrameLineMinimumWidth((uint)newCount);

            var animationStoryboard = new Storyboard();
            for (uint i = 0; i < newCount; i++)
            {
                if (i >= frameIndex)
                {
                    ellipseControllersCache[(int)i].SetNewIndex(ellipseControllersCache[(int)i].CurrentIndex - 1);
                    ellipseControllersCache[(int)i].SetNewFrameCount((uint)newCount);
                    animationStoryboard.Children.Add(ellipseControllersCache[(int)i].ReArrangeEllipseWithAnimation());
                }
                else
                {
                    ellipseControllersCache[(int)i].SetNewFrameCount((uint)newCount);
                    animationStoryboard.Children.Add(ellipseControllersCache[(int)i].ReArrangeEllipseWithAnimation());
                }
            }

            animationStoryboard.FillBehavior = FillBehavior.Stop;
            containerCanvas.BeginStoryboard(animationStoryboard);
        }

        public void InsertFrame(uint frameIndex)
        {
            if (frameIndex > ellipseControllersCache.Count)
            {
                throw new Exception();
            }

            var controller = CreateAndSetupController(frameIndex, (uint)ellipseControllersCache.Count + 1);

            containerCanvas.Children.Add(controller.ContainerCanvas);
            ellipseControllersCache.Insert((int)frameIndex, controller);

            var newCount = ellipseControllersCache.Count;
            // Re-calculate frame line min with for resize canvas feature
            calculatedFrameLineMinimumWidth = CaculateFrameLineMinimumWidth((uint)newCount);

            var animationStoryboard = new Storyboard();
            for (uint i = 0; i < newCount; i++)
            {

                if (i > frameIndex)
                {
                    ellipseControllersCache[(int)i].SetNewIndex(ellipseControllersCache[(int)i].CurrentIndex + 1);
                    ellipseControllersCache[(int)i].SetNewFrameCount((uint)newCount);
                    animationStoryboard.Children.Add(ellipseControllersCache[(int)i].ReArrangeEllipseWithAnimation());
                }
                else if (i < frameIndex)
                {
                    ellipseControllersCache[(int)i].SetNewFrameCount((uint)newCount);
                    ellipseControllersCache[(int)i].ArrangeEllipse();
                    animationStoryboard.Children.Add(ellipseControllersCache[(int)i].ReArrangeEllipseWithAnimation());
                }
            }

            animationStoryboard.FillBehavior = FillBehavior.Stop;
            animationStoryboard.Completed += (s, e) =>
            {
                if (containerCanvas.ActualWidth < calculatedFrameLineMinimumWidth)
                {
                    containerCanvas.Width = calculatedFrameLineMinimumWidth;
                }
            };
            containerCanvas.BeginStoryboard(animationStoryboard);
        }

        public void AddNewFrame()
        {
            var frameIndex = ellipseControllersCache.Count;

            var controller = CreateAndSetupController((uint)frameIndex,
                (uint)ellipseControllersCache.Count + 1);

            containerCanvas.Children.Add(controller.ContainerCanvas);
            ellipseControllersCache.Add(controller);

            var newCount = ellipseControllersCache.Count;
            // Re-calculate frame line min with for resize canvas feature
            calculatedFrameLineMinimumWidth = CaculateFrameLineMinimumWidth((uint)newCount);

            var animationStoryboard = new Storyboard();
            for (uint i = 0; i < newCount; i++)
            {
                if (i < frameIndex)
                {
                    ellipseControllersCache[(int)i].SetNewFrameCount((uint)newCount);
                    ellipseControllersCache[(int)i].ArrangeEllipse();
                    animationStoryboard.Children.Add(ellipseControllersCache[(int)i].ReArrangeEllipseWithAnimation());
                }
            }

            animationStoryboard.FillBehavior = FillBehavior.Stop;
            animationStoryboard.Completed += (s, e) =>
            {
                if (containerCanvas.ActualWidth < calculatedFrameLineMinimumWidth)
                {
                    containerCanvas.Width = calculatedFrameLineMinimumWidth;
                }
            };
            containerCanvas.BeginStoryboard(animationStoryboard);
        }

        private EllipseController CreateAndSetupController(uint index, uint frameCount)
        {
            var controller = EllipseController.CreateController(ownerCanvas: containerCanvas, index, frameCount);
            controller.OnDraggingMouseEllipse += Controller_OnDraggingMouseEllipse;
            controller.OnDraggingMouseUpEnteredEllipse += Controller_OnDraggingMouseUpEnteredEllipse;
            controller.PreviewRemovingEllipse += Controller_PreviewRemovingEllipse; ;
            return controller;
        }

        private void ClearAllEllipse()
        {
            foreach (var kp in ellipseControllersCache)
            {
                containerCanvas.Children.Remove(kp.ContainerCanvas);
                kp.Dispose();
                kp.OnDraggingMouseEllipse -= Controller_OnDraggingMouseEllipse;
                kp.OnDraggingMouseUpEnteredEllipse -= Controller_OnDraggingMouseUpEnteredEllipse;
                kp.PreviewRemovingEllipse -= Controller_PreviewRemovingEllipse;
            }
            ellipseControllersCache.Clear();
        }

        private async void EnableAutoHorizontalScroll(bool isEnable, bool isLeft)
        {
            if (!isEnable && autoScrollSemaphore.CurrentCount == 1)
            {
                return;
            }
            if (autoScrollSemaphore.CurrentCount == 0)
            {
                if (!isEnable)
                {
                    autoScrollSemaphore.Release();
                }
                return;
            }
            await autoScrollSemaphore.WaitAsync();
            if (isLeft)
            {
                while (containerScroll.HorizontalOffset > 0
                   && autoScrollSemaphore.CurrentCount == 0)
                {
                    var currentOffset = containerScroll.HorizontalOffset - 6;
                    containerScroll.ScrollToHorizontalOffset(currentOffset);
                    await Task.Delay(10);
                }
            }
            else
            {
                while (containerScroll.HorizontalOffset < containerScroll.ExtentWidth
                    && autoScrollSemaphore.CurrentCount == 0)
                {
                    var currentOffset = containerScroll.HorizontalOffset + 6;
                    containerScroll.ScrollToHorizontalOffset(currentOffset);
                    await Task.Delay(10);
                }

            }
            if (autoScrollSemaphore.CurrentCount == 0)
            {
                autoScrollSemaphore.Release();
            }
        }

        public void Dispose()
        {
            ClearAllEllipse();
            containerCanvas.SizeChanged -= ContainerCanvas_SizeChanged;
            containerCanvas.MouseWheel -= ContainerCanvas_MouseWheel;
            containerScroll.SizeChanged -= ContainerScroll_SizeChanged;
            contextMenuController.PreviewAddingNewFrame -= ContextMenuController_PreviewAddingNewFrame;

        }

        #region utils
        private double CaculateFrameLineMinimumWidth(uint frameCount)
        {
            var sizePerEllipse = SIZE_PER_ELLIPSE;
            var minEllipseDistance = MIN_ELLIPSE_DISTANCE;
            var headOffset = HEAD_OFFSET;

            if (frameCount == 0)
            {
                return headOffset * 2;
            }
            return sizePerEllipse * frameCount + minEllipseDistance * (frameCount - 1) + headOffset * 2;
        }

        private bool IsOverlap(double Xa, double Ya, double Wa, double Ha,
           double Xb, double Yb, double Wb, double Hb)
        {
            return (Xa + Wa > Xb) && (Xa < Xb + Wb) && (Ya + Ha > Yb) && (Ya < Yb + Hb);
        }

        private bool IsMouseInZone(Point mousePos, Rect zone)
        {
            return zone.Contains(mousePos);
        }
        #endregion
    }
}
