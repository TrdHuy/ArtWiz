using System.Windows.Controls;
using System.Windows;
using SPRNetTool.Utils;

namespace SPRNetTool.View.Widgets
{
    public class CollapsibleControl : UserControl
    {
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

        private Button? collapseButton;
        private TextBlock? headerTextBlock;
        private bool isCollapse = false;

        public CollapsibleControl()
        {
        }

        public override void OnApplyTemplate()
        {
            collapseButton = GetTemplateChild("CollapseButton") as Button;
            headerTextBlock = GetTemplateChild("Header") as TextBlock;
            headerTextBlock?.Apply(it => it.Text = Header);
            if (collapseButton != null)
            {
                collapseButton.Click += CollapseButton_Click;
            }
        }

        private void CollapseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            isCollapse = !isCollapse;
            Content.IfIs<UIElement>(it =>
            {
                it.Visibility = isCollapse ? Visibility.Collapsed : Visibility.Visible;
            });
        }
    }
}
