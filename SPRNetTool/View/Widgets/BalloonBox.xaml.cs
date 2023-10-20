using SPRNetTool.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SPRNetTool.View.Widgets
{
    public class TextContentUpdatedEventArgs
    {
        public bool Handled { get; set; }

        public string NewText { get; set; } = "";
    }

    public delegate void TextContentUpdatedHandler(object sender, TextContentUpdatedEventArgs e);

    public partial class BalloonBox : UserControl
    {
        private event TextContentUpdatedHandler? TextContentUpdated;
        private event TextContentUpdatedHandler? PreviewTextContentUpdated;

        public static void AddTextContentUpdatedHandler(DependencyObject element, TextContentUpdatedHandler handler)
        {
            element.IfIs<BalloonBox>(it =>
            {
                it.TextContentUpdated += handler;
            });
        }

        public static void RemoveTextContentUpdatedHandler(DependencyObject element, TextContentUpdatedHandler handler)
        {
            element.IfIs<BalloonBox>(it =>
            {
                it.TextContentUpdated -= handler;
            });
        }

        public static void AddPreviewTextContentUpdatedHandler(DependencyObject element, TextContentUpdatedHandler handler)
        {
            element.IfIs<BalloonBox>(it =>
            {
                it.PreviewTextContentUpdated += handler;
            });
        }

        public static void RemovePreviewTextContentUpdatedHandler(DependencyObject element, TextContentUpdatedHandler handler)
        {
            element.IfIs<BalloonBox>(it =>
            {
                it.PreviewTextContentUpdated -= handler;
            });
        }

        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.Register(
                "TextContent",
                typeof(string),
                typeof(BalloonBox),
                new PropertyMetadata("0", OnChanged));

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public string TextContent
        {
            get { return GetValue(TextContentProperty).ToString() ?? ""; }
            set { SetValue(TextContentProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                "Maximum",
                typeof(int),
                typeof(BalloonBox),
                new PropertyMetadata(int.MaxValue));

        public int Maximum
        {
            get { return Convert.ToInt32(GetValue(MaximumProperty)); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                "Minimum",
                typeof(int),
                typeof(BalloonBox),
                new PropertyMetadata(int.MinValue));

        public int Minimum
        {
            get { return Convert.ToInt32(GetValue(MinimumProperty)); }
            set { SetValue(MinimumProperty, value); }
        }

        public BalloonBox()
        {
            InitializeComponent();
        }

        private void Container_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && EditBox.Visibility != Visibility.Visible)
            {
                EditBox.Visibility = Visibility.Visible;
                NoEditBox.Visibility = Visibility.Collapsed;
                EditBox.Text = TextContent;
                EditBox.Focus();
                Keyboard.AddLostKeyboardFocusHandler(EditBox, EditBox_LostKeyboardFocus);
            }
        }


        private void Container_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textContentUpdateEventArgs = new TextContentUpdatedEventArgs() { NewText = EditBox.Text };
                PreviewTextContentUpdated?.Invoke(this, textContentUpdateEventArgs);
                if (textContentUpdateEventArgs.Handled)
                {
                    Keyboard.ClearFocus();
                    return;
                }
                if (EditBox.Text == "")
                {
                    TextContent = "0";
                }
                else
                {
                    // NOTE: revert binding for TextContentProperty
                    //var binding = BindingOperations.GetBinding(this, TextContentProperty);
                    TextContent = textContentUpdateEventArgs.NewText;
                    //BindingOperations.SetBinding(this, TextContentProperty, binding);
                }
                TextContentUpdated?.Invoke(this, textContentUpdateEventArgs);
                Keyboard.ClearFocus();
            }
            else if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
            }
        }


        private void EditBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            EditBox.Visibility = Visibility.Collapsed;
            NoEditBox.Visibility = Visibility.Visible;
            Keyboard.RemoveLostKeyboardFocusHandler(EditBox, EditBox_LostKeyboardFocus);
        }

        private void EditBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(e.Text.Any(char.IsNumber)
                || EditBox.CaretIndex == 0 && e.Text == "-"))
            {
                e.Handled = true;
            }
            else if (e.Text == "-" && EditBox.CaretIndex == 0)
            {
                try
                {
                    Convert.ToInt32(EditBox.Text.Insert(0, "-"));
                }
                catch
                {
                    e.Handled = true;
                }
            }

        }

        private void EditBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EditBox.Text != "")
            {
                try
                {
                    var inputNumber = Convert.ToInt64(EditBox.Text);
                    var oldIndex = EditBox.CaretIndex;
                    if (inputNumber > Maximum)
                    {
                        EditBox.TextChanged -= EditBox_TextChanged;
                        EditBox.Text = Maximum.ToString();
                        EditBox.TextChanged += EditBox_TextChanged;
                    }
                    else if (inputNumber < Minimum)
                    {
                        EditBox.TextChanged -= EditBox_TextChanged;
                        EditBox.Text = Minimum.ToString();
                        EditBox.TextChanged += EditBox_TextChanged;
                    }
                    else
                    {
                        EditBox.TextChanged -= EditBox_TextChanged;
                        EditBox.Text = inputNumber.ToString();
                        EditBox.TextChanged += EditBox_TextChanged;
                    }
                    EditBox.CaretIndex = oldIndex;
                }
                catch { }
            }
            else
            {
                EditBox.TextChanged -= EditBox_TextChanged;
                EditBox.Text = "0";
                EditBox.TextChanged += EditBox_TextChanged;
            }
        }
    }
}
