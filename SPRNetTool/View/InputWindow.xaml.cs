using SPRNetTool.Utils;
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
            TEXT, CHECKBOX, COMBO, RADIO
        }
        public enum Res
        {
            CANCEL, AGREE
        }
        public class InputBuilder
        {
            public abstract class InputOption
            {
                public string Title { get; private set; }
                public string Description { get; private set; }

                public InputOption(string title, string description)
                {
                    Title = title;
                    Description = description;
                }
            }

            public class TextInputOption : InputOption
            {
                public string InputDefault { get; private set; }
                public Func<string, string, bool> PreviewInputCondition { get; private set; }


                public TextInputOption(string title, string description, string inputDefault, Func<string, string, bool> previewInputCondition) : base(title, description)
                {
                    InputDefault = inputDefault;
                    PreviewInputCondition = previewInputCondition;
                }
            }

            public class CheckBoxInputOption : InputOption
            {
                public bool InputDefault { get; private set; }
                public Func<bool> SupportCondition { get; private set; }
                public Action<ObservableCollection<ItemViewModel>, bool>? Callback { get; private set; }


                public CheckBoxInputOption(string title, string description, bool inputDefault, Func<bool> supportCondition, Action<ObservableCollection<ItemViewModel>, bool>? callback = null) : base(title, description)
                {
                    InputDefault = inputDefault;
                    SupportCondition = supportCondition;
                    Callback = callback;
                }
            }

            public class ComboInputOption : InputOption
            {
                public int InputDefault { get; private set; }
                public Func<bool> SupportCondition { get; private set; }
                public List<string> Options { get; private set; }
                public Action<ObservableCollection<ItemViewModel>, int>? Callback { get; private set; }


                public ComboInputOption(string title, string description, List<string> options
                    , int defaultSelection, Func<bool> supportCondition
                    , Action<ObservableCollection<ItemViewModel>, int>? callback = null) : base(title, description)
                {
                    InputDefault = defaultSelection;
                    Options = options;
                    SupportCondition = supportCondition;
                    Callback = callback;
                }
            }
            public class RadioInputOption : InputOption
            {


                public List<string> Content { get; private set; }

                public string GroupName { get; private set; }
                public RadioInputOption(string title, string description, List<string> content, string groupName) : base(title, description)
                {

                    Content = content;
                    GroupName = groupName;
                }
            }


            private List<InputOption> options = new List<InputOption>();


            public InputBuilder AddTextInputOption(string title, string description, string inputDefault, Func<string, string, bool> condition)
            {
                options.Add(new TextInputOption(title, description, inputDefault, condition));
                return this;
            }

            public InputBuilder AddCheckBoxOption(string title, string description, bool inputDefault, Func<bool> condition, Action<ObservableCollection<ItemViewModel>, bool>? callback = null)
            {
                options.Add(new CheckBoxInputOption(title, description, inputDefault, condition, callback));
                return this;
            }

            public InputBuilder AddComboBoxOption(string title, string description, List<string> comboOptions, int defaultSelection, Func<bool> condition, Action<ObservableCollection<ItemViewModel>, int>? callback = null)
            {
                options.Add(new ComboInputOption(title, description, comboOptions, defaultSelection, condition, callback));
                return this;
            }


            public InputBuilder AddRadioOptions(string title, string description, List<string> content, string groupName)

            {
                options.Add(new RadioInputOption(title, description, content, groupName));
                return this;
            }
            public List<InputOption> Build() { return options; }
        }


        public class ItemViewModel : BaseViewModel
        {
            private ObservableCollection<string>? _comboOptions = new ObservableCollection<string>();
            private string _content = "";
            private bool _isDisabled = false;
            private int _comboSelection = 0;
            private string _title = "";
            private string _description = "";
            private bool _checkContent = false;
            private ContentType _contentType = ContentType.TEXT;
            private string _groupName = "";
            private ObservableCollection<string>? _radioOptions = new ObservableCollection<string>();

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

            public ObservableCollection<string>? ComboOptions
            {
                get { return _comboOptions; }
                set
                {
                    _comboOptions = value;
                    Invalidate();
                }
            }
            public int ComboSelection
            {
                get { return _comboSelection; }
                set
                {
                    _comboSelection = value;
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
            public string GroupName
            {
                get { return _groupName; }
                set
                {
                    _groupName = value;
                    Invalidate();
                }
            }

            public ObservableCollection<string>? RadioOptions
            {
                get { return _radioOptions; }
                set
                {
                    _radioOptions = value;
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
        private string? CheckedContent = null;
        public InputWindow(
            List<InputBuilder.InputOption> src
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
                var newItemVM = new ItemViewModel()
                {
                    Description = item.Description,
                    Title = item.Title,
                    ContentType = item.Let((it) =>
                    {
                        switch (it)
                        {
                            case InputBuilder.TextInputOption:
                                return ContentType.TEXT;
                            case InputBuilder.ComboInputOption:
                                return ContentType.COMBO;
                            case InputBuilder.CheckBoxInputOption:
                                return ContentType.CHECKBOX;
                            case InputBuilder.RadioInputOption:
                                return ContentType.RADIO;

                        }
                        return ContentType.TEXT;
                    }),
                    Content = item.Let((it) =>
                    {
                        switch (it)
                        {
                            case InputBuilder.TextInputOption:
                                return (it as InputBuilder.TextInputOption)?.InputDefault ?? "";
                        }
                        return "";
                    }),
                    ComboOptions = item.IfIsThenLet<InputBuilder.ComboInputOption, ObservableCollection<string>>(it2 =>
                             new ObservableCollection<string>(it2.Options)),
                    ComboSelection = item.IfIsThenLet<InputBuilder.ComboInputOption, int>(it2 =>
                             it2.InputDefault),
                    CheckContent = item.Let((it) =>
                    {
                        switch (it)
                        {
                            case InputBuilder.ComboInputOption:
                                return (it as InputBuilder.CheckBoxInputOption)?.InputDefault ?? false;
                        }
                        return false;
                    }),
                    TextCondition = item.Let((it) =>
                    {
                        switch (it)
                        {
                            case InputBuilder.TextInputOption:
                                return (it as InputBuilder.TextInputOption)?.PreviewInputCondition;
                        }
                        return null;
                    }),
                    CheckCondition = item.Let((it) =>
                    {
                        switch (it)
                        {
                            case InputBuilder.CheckBoxInputOption:
                                return (it as InputBuilder.CheckBoxInputOption)?.SupportCondition;
                        }
                        return null;
                    }),
                    CheckChangedCallback = item.Let((it) =>
                    {
                        switch (it)
                        {
                            case InputBuilder.CheckBoxInputOption:
                                return (it as InputBuilder.CheckBoxInputOption)?.Callback;
                        }
                        return null;
                    }),

                    GroupName = item.Let((it) =>
                    {
                        switch (it)
                        {
                            case InputBuilder.RadioInputOption:
                                return (it as InputBuilder.RadioInputOption)?.GroupName ?? "";
                        }
                        return "";

                    }),
                    RadioOptions = item.IfIsThenLet<InputBuilder.RadioInputOption, ObservableCollection<string>>(it2 =>
                             new ObservableCollection<string>(it2.Content))

                };
                InputSource.Add(newItemVM);
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
                (Owner as IWindowViewer)?.DisableWindow(true);
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
                else if (item.ContentType == ContentType.COMBO)
                {
                    newSource.Add(item.Title, item.ComboSelection);
                }
                else if (item.ContentType == ContentType.RADIO && item.CheckContent)
                {
                    newSource.Add(item.Title, item.Content);
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
            (Owner as IWindowViewer)?.DisableWindow(false);
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
        public void Radio_Checked(object sender, RoutedEventArgs e)
        {
            var context = (sender as RadioButton)?.Content;
            if (context != null)
            {
                CheckedContent = Convert.ToString(context);
            }

        }
    }
}
