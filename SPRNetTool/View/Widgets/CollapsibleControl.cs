using ArtWiz.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ArtWiz.View.Widgets
{
    public class CollapsibleControl : UserControl
    {
        public static readonly DependencyProperty CollapseVelocityProperty =
           DependencyProperty.Register(
               "CollapseVelocity",
               typeof(uint),
               typeof(CollapsibleControl),
               new PropertyMetadata(2200u));

        public uint CollapseVelocity
        {
            get { return (uint)GetValue(CollapseVelocityProperty); }
            set { SetValue(CollapseVelocityProperty, value); }
        }

        public static readonly DependencyProperty IsCollapseProperty =
           DependencyProperty.Register(
               "IsCollapse",
               typeof(bool),
               typeof(CollapsibleControl),
               new PropertyMetadata(false, OnChanged));

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public bool IsCollapse
        {
            get { return (bool)GetValue(IsCollapseProperty); }
            set { SetValue(IsCollapseProperty, value); }
        }

        public static readonly DependencyProperty ExtraHeaderContentProperty =
           DependencyProperty.Register(
               "ExtraHeaderContent",
               typeof(object),
               typeof(CollapsibleControl),
               new PropertyMetadata(default(object)));

        public object ExtraHeaderContent
        {
            get { return GetValue(ExtraHeaderContentProperty); }
            set { SetValue(ExtraHeaderContentProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
           DependencyProperty.Register(
               "Header",
               typeof(string),
               typeof(CollapsibleControl),
               new PropertyMetadata(default(string), propertyChangedCallback: OnHeaderChanged));

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.IfIs<CollapsibleControl>(it =>
            {
                it.headerTextBlock?.Apply(it => it.Text = e.NewValue.ToString());
            });
        }

        public string Header
        {
            get { return GetValue(HeaderProperty)?.ToString() ?? ""; }
            set { SetValue(HeaderProperty, value); }
        }

        private TextBlock? headerTextBlock;
        private Border? mainBoderContainer;

        public CollapsibleControl()
        {
        }

        public override void OnApplyTemplate()
        {
            headerTextBlock = GetTemplateChild("Header") as TextBlock;
            mainBoderContainer = GetTemplateChild("MainBorderContainer") as Border;
            headerTextBlock?.Apply(it => it.Text = Header);
            GetTemplateChild("CollapseButton").IfIs<Button>(it =>
            {
                it.Click -= CollapseButton_Click;
                it.Click += CollapseButton_Click;
            });
            GetTemplateChild("CollapseButton").IfIs<IconToggle>(it =>
            {
                it.Click -= CollapseButton_Click;
                it.Click += CollapseButton_Click;
            });
        }

        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsCollapse)
            {
                BeginCollapseAnimation();
            }
            else
            {
                BeginExpandAnimation();
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (Double.IsNaN(oldHeightCache))
            {
                mainBoderContainer?.Apply(it =>
                {
                    it.Measure(constraint);
                    oldHeightCache = it.DesiredSize.Height;
                    uiContentCache = mainBoderContainer.Child;
                    if (IsCollapse)
                    {
                        mainBoderContainer.Height = 0;
                        mainBoderContainer.Child = null;
                    }
                });
            }

            return base.MeasureOverride(constraint);
        }

        private double oldHeightCache = Double.NaN;
        private object? uiContentCache;
        private void BeginCollapseAnimation()
        {
            if (mainBoderContainer != null && !IsCollapse)
            {
                oldHeightCache = mainBoderContainer.ActualHeight;
                uiContentCache = mainBoderContainer.Child;
                var animationStoryboard = new Storyboard();
                var time = oldHeightCache / CollapseVelocity;
                DoubleAnimation collapseAnim = new DoubleAnimation(oldHeightCache, 0, TimeSpan.FromSeconds(time));
                Storyboard.SetTarget(collapseAnim, mainBoderContainer);
                Storyboard.SetTargetProperty(collapseAnim, new PropertyPath("(Border.Height)"));
                animationStoryboard.Children.Add(collapseAnim);
                animationStoryboard.FillBehavior = FillBehavior.HoldEnd;
                animationStoryboard.Completed += (_, _) =>
                {
                    mainBoderContainer.Child = null;
                    IsCollapse = !IsCollapse;
                };
                this.BeginStoryboard(animationStoryboard);
            }
        }

        private void BeginExpandAnimation()
        {
            if (mainBoderContainer != null && IsCollapse)
            {
                var animationStoryboard = new Storyboard();
                var time = oldHeightCache / CollapseVelocity;
                DoubleAnimation collapseAnim = new DoubleAnimation(0, oldHeightCache, TimeSpan.FromSeconds(time));
                Storyboard.SetTarget(collapseAnim, mainBoderContainer);
                Storyboard.SetTargetProperty(collapseAnim, new PropertyPath("(Border.Height)"));
                animationStoryboard.Children.Add(collapseAnim);
                animationStoryboard.FillBehavior = FillBehavior.HoldEnd;
                animationStoryboard.Completed += (_, _) =>
                {
                    uiContentCache?.IfIs<UIElement>(it => mainBoderContainer.Child = it);
                    IsCollapse = !IsCollapse;
                };
                this.BeginStoryboard(animationStoryboard);
            }
        }
    }
}
