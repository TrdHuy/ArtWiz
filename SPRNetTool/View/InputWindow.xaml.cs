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
        public enum Res
        {
            CANCEL, AGREE
        }
        public class InputBuilder
        {
            private List<(string, string, Func<string, string, bool>)> res = new List<(string, string, Func<string, string, bool>)>();

            public InputBuilder Add(string tilte, string inputDefault, Func<string, string, bool> condition)
            {
                res.Add((tilte, inputDefault, condition));
                return this;
            }

            public List<(string, string, Func<string, string, bool>)> Build()
            {
                return res;
            }
        }


        class ItemViewModel : BaseViewModel
        {
            private string _content = "";
            public string Content
            {
                get { return _content; }
                set
                {
                    _content = value;
                    Invalidate();
                }
            }

            public Func<string, string, bool>? Condition { get; set; }
        }

        private ObservableCollection<ItemViewModel> TitleSource = new ObservableCollection<ItemViewModel>();
        private ObservableCollection<ItemViewModel> InputSource = new ObservableCollection<ItemViewModel>();

        private Action<Dictionary<string, string>>? AgreeButtonClicked;
        private Action? CancelButtonClicked;
        private Res curRes = Res.CANCEL;
        private List<(string, string, Func<string, string, bool>)> Src;
        public InputWindow(List<(string, string, Func<string, string, bool>)> src
            , Window? owner = null
            , Action<Dictionary<string, string>>? agreeButtonClicked = null
            , Action? cancelButtonClicked = null)
        {
            InitializeComponent();
            if (src.Count == 0) throw new Exception("Source is empty");
            Owner = owner;
            Src = src;

            AgreeButtonClicked = agreeButtonClicked;
            CancelButtonClicked = cancelButtonClicked;

            foreach (var item in src)
            {
                TitleSource.Add(new ItemViewModel() { Content = item.Item1 });
                InputSource.Add(new ItemViewModel()
                {
                    Content = item.Item2,
                    Condition = item.Item3
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
            var newSource = new Dictionary<string, string>();
            int i = 0;
            foreach (var item in TitleSource)
            {
                newSource.Add(item.Content, InputSource[i++].Content);
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
                var shouldContinue = context.Condition?.Invoke((sender as TextBox)?.Text ?? "", e.Text) ?? true;
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
