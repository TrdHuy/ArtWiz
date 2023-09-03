using SPRNetTool.View.Base;
using SPRNetTool.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static SPRNetTool.View.InputWindow;

namespace SPRNetTool.View
{
    public class InputWindowItemHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var refListView = values[0] as ListView;
            var src = values[1] as ObservableCollection<ItemViewModel>;
            var item = values[2] as ItemViewModel;
            var index = src?.IndexOf(item) ?? 0;
            var itemHeight = (refListView?.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem)?.DesiredSize.Height;
            return itemHeight ?? 30d;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {


        public enum ContentType
        {
            TEXT, CHECKBOX
        }
        public enum Res
        {
            CANCEL, AGREE
        }
        public class InputBuilder
        {
            // Title, description, input type, condition, ContentType(equal input type), value changed callback
            private List<(string, string, (string, bool), (Func<string, string, bool>?, Func<bool>?), ContentType, Action<ObservableCollection<ItemViewModel>, bool>?)> res
                = new List<(string, string, (string, bool), (Func<string, string, bool>?, Func<bool>?), ContentType, Action<ObservableCollection<ItemViewModel>, bool>?)>();

            public InputBuilder Add(string tilte, string description, string inputDefault, Func<string, string, bool> condition)
            {
                res.Add((tilte, description, (inputDefault, false), (condition, null), ContentType.TEXT, null));
                return this;
            }

            public InputBuilder Add(string tilte, string description, bool inputDefault, Func<bool> condition, Action<ObservableCollection<ItemViewModel>, bool>? callback = null)
            {
                res.Add((tilte, description, ("", inputDefault), (null, condition), ContentType.CHECKBOX, callback));
                return this;
            }

            public List<(string, string, (string, bool), (Func<string, string, bool>?, Func<bool>?), ContentType, Action<ObservableCollection<ItemViewModel>, bool>?)> Build()
            {
                return res;
            }
        }


        public class ItemViewModel : BaseViewModel
        {
            private string _content = "";
            private bool _isDisabled = false;
            private string _title = "";
            private string _description = "";
            private bool _checkContent = false;
            private ContentType _contentType = ContentType.TEXT;
            public string Title
            {
                get { return _title; }
                set
                {
                    _title = value;
                    Invalidate();
                }
            }
            public bool IsDisabled
            {
                get { return _isDisabled; }
                set
                {
                    _isDisabled = value;
                    Invalidate();
                }
            }
            public string Content
            {
                get { return _content; }
                set
                {
                    _content = value;
                    Invalidate();
                }
            }
            public string Description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    Invalidate();
                }
            }

            public bool CheckContent
            {
                get { return _checkContent; }
                set
                {
                    _checkContent = value;
                    Invalidate();
                }
            }

            public ContentType ContentType
            {
                get { return _contentType; }
                set
                {
                    _contentType = value;
                    Invalidate();
                }
            }

            public Func<string, string, bool>? TextCondition { get; set; }
            public Func<bool>? CheckCondition { get; set; }
            public Action<ObservableCollection<ItemViewModel>, bool>? CheckChangedCallback { get; set; }
        }

        private ObservableCollection<ItemViewModel> InputSource = new ObservableCollection<ItemViewModel>();

        private Action<Dictionary<string, object>>? AgreeButtonClicked;
        private Action? CancelButtonClicked;
        private Res curRes = Res.CANCEL;

        public InputWindow(List<(string, string, (string, bool), (Func<string, string, bool>?, Func<bool>?), ContentType, Action<ObservableCollection<ItemViewModel>, bool>?)> src
            , Window? owner = null
            , Action<Dictionary<string, object>>? agreeButtonClicked = null
            , Action? cancelButtonClicked = null)
        {
            InitializeComponent();
            if (src.Count == 0) throw new Exception("Source is empty");
            Owner = owner;

            AgreeButtonClicked = agreeButtonClicked;
            CancelButtonClicked = cancelButtonClicked;

            foreach (var item in src)
            {

                InputSource.Add(new ItemViewModel()
                {
                    Description = item.Item2,
                    Title = item.Item1,
                    ContentType = item.Item5,
                    Content = item.Item3.Item1,
                    CheckContent = item.Item3.Item2,
                    TextCondition = item.Item4.Item1,
                    CheckCondition = item.Item4.Item2,
                    CheckChangedCallback = item.Item6
                });
            }
            TitleListView.ItemsSource = InputSource;
            InputListView.ItemsSource = InputSource;
            foreach (var item in InputSource)
            {
                item.CheckChangedCallback?.Invoke(InputSource, Convert.ToBoolean(item.CheckContent));
            }
        }

        public new Res Show()
        {
            if (Owner != null)
            {
                (Owner as INetView)?.DisableWindow(true);
            }
            base.ShowDialog();
            return curRes;
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            CancelButtonClicked?.Invoke();
            curRes = Res.CANCEL;
            Close();
        }

        private void ButtonAgreeClick(object sender, RoutedEventArgs e)
        {
            var newSource = new Dictionary<string, object>();
            foreach (var item in InputSource)
            {
                if (item.ContentType == ContentType.TEXT)
                {
                    newSource.Add(item.Title, item.Content);
                }
                else if (item.ContentType == ContentType.CHECKBOX)
                {
                    newSource.Add(item.Title, item.CheckContent);
                }
            }
            AgreeButtonClicked?.Invoke(newSource);
            curRes = Res.AGREE;
            Close();
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var context = (sender as TextBox)?.DataContext as ItemViewModel;
            if (context != null)
            {
                var shouldContinue = context.TextCondition?.Invoke((sender as TextBox)?.Text ?? "", e.Text) ?? true;
                e.Handled = !shouldContinue;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            (Owner as INetView)?.DisableWindow(false);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var context = (sender as CheckBox)?.DataContext as ItemViewModel;
            var isChecked = (sender as CheckBox)?.IsChecked;

            if (context != null && isChecked != null)
            {
                context.CheckChangedCallback?.Invoke(InputSource, isChecked ?? false);
            }
        }
    }
}
