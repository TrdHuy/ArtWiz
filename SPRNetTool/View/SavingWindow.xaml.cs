using SPRNetTool.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using static SPRNetTool.View.InputWindow;

namespace SPRNetTool.View
{
    /// <summary>
    /// Interaction logic for SavingWindow.xaml
    /// </summary>
    public partial class SavingWindow : Window
    {
        private ObservableCollection<ItemViewModel> InputSource = new ObservableCollection<ItemViewModel>();

        private Action<string>? AgreeButtonClicked;
        private Action? CancelButtonClicked;
        private Res curRes = Res.CANCEL;
        private string? CheckedContent = null;
        public SavingWindow(
            List<InputBuilder.InputOption> src,
            Window? owner = null,
            Action<string>? agreeClick = null
            )
       {
            InitializeComponent();           
            if (src.Count == 0) throw new Exception("Source is empty");
            Owner = owner;
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
                                return ContentType.RADIOBUTTON;
                        }
                        return ContentType.TEXT;
                    }),
                    CheckContent = item.Let((it) =>
                    {
                        switch (it)
                        {
                            case InputBuilder.ComboInputOption:
                                return (it as InputBuilder.CheckBoxInputOption)?.InputDefault ?? false;
                            case InputBuilder.RadioInputOption:
                                return false;
                        }
                        return false;
                    }),
                    Content = item.Let((it) =>
                    {
                        switch (it)
                        {
                            case InputBuilder.RadioInputOption:
                                return (it as InputBuilder.RadioInputOption)?.Content ?? "";
                        }
                        return "";
                    }),
                    GroupName = item.Let((it) =>
                    {
                        switch (it)
                        { 
                            case InputBuilder.RadioInputOption:
                                return (it as InputBuilder.RadioInputOption)?.GroupName ?? "";
                        }
                        return "";
                    })
                    
                };
                InputSource.Add(newItemVM);
            }
            TitleListView.ItemsSource = InputSource;
            AgreeButtonClicked = agreeClick;
        }


       public new Res Show()
       {
            base.ShowDialog();
            return curRes;
       }

       public void CancelClick(object sender, RoutedEventArgs e)
       {
            curRes = Res.CANCEL;
            Close();
       } 
        
       public void AgreeClick(object sender, RoutedEventArgs e) 
       {
            AgreeButtonClicked?.Invoke(CheckedContent ?? "");
            curRes = Res.AGREE;
            Close();
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

