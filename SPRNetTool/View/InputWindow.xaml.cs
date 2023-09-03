using SPRNetTool.View.Base;
using SPRNetTool.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SPRNetTool.View
{
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
            private List<(string, (string, bool), (Func<string, string, bool>?, Func<bool>?), ContentType)> res
                = new List<(string, (string, bool), (Func<string, string, bool>?, Func<bool>?), ContentType)>();

            public InputBuilder Add(string tilte, string inputDefault, Func<string, string, bool> condition)
            {
                res.Add((tilte, (inputDefault, false), (condition, null), ContentType.TEXT));
                return this;
            }

            public InputBuilder Add(string tilte, bool inputDefault, Func<bool> condition)
            {
                res.Add((tilte, ("", inputDefault), (null, condition), ContentType.CHECKBOX));
                return this;
            }

            public List<(string, (string, bool), (Func<string, string, bool>?, Func<bool>?), ContentType)> Build()
            {
                return res;
            }
        }


        class ItemViewModel : BaseViewModel
        {
            private string _content = "";
            private bool _checkContent = false;
            private ContentType _contentType = ContentType.TEXT;
            public string Content
            {
                get { return _content; }
                set
                {
                    _content = value;
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
        }

        private ObservableCollection<ItemViewModel> TitleSource = new ObservableCollection<ItemViewModel>();
        private ObservableCollection<ItemViewModel> InputSource = new ObservableCollection<ItemViewModel>();

        private Action<Dictionary<string, object>>? AgreeButtonClicked;
        private Action? CancelButtonClicked;
        private Res curRes = Res.CANCEL;

        public InputWindow(List<(string, (string, bool), (Func<string, string, bool>?, Func<bool>?), ContentType)> src
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
                TitleSource.Add(new ItemViewModel() { Content = item.Item1 });
                InputSource.Add(new ItemViewModel()
                {
                    ContentType = item.Item4,
                    Content = item.Item2.Item1,
                    CheckContent = item.Item2.Item2,
                    TextCondition = item.Item3.Item1,
                    CheckCondition = item.Item3.Item2
                });
            }
            TitleListView.ItemsSource = TitleSource;
            InputListView.ItemsSource = InputSource;

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
            int i = 0;
            foreach (var item in TitleSource)
            {
                if (InputSource[i].ContentType == ContentType.TEXT)
                {
                    newSource.Add(item.Content, InputSource[i++].Content);
                }
                else if (InputSource[i].ContentType == ContentType.CHECKBOX)
                {
                    newSource.Add(item.Content, InputSource[i++].CheckContent);
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
    }
}
