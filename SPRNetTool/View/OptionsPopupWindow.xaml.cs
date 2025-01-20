using ArtWiz.View.Base;
using ArtWiz.ViewModel;
using System;
using System.Collections.Generic;
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

namespace ArtWiz.View
{
    /// <summary>
    /// Interaction logic for OptionsPopupWindow.xaml
    /// </summary>
    public partial class OptionsPopupWindow : BaseArtWizWindow
    {
        private readonly Action<string> _onOptionSelected;
        private string _selectedOption = null;
        private PopupWindowViewModel _popupWindowViewModel;

        public OptionsPopupWindow(List<string> options, Action<string> onOptionSelected)
        {
            InitializeComponent();
            _onOptionSelected = onOptionSelected;
            InitializeOptions(options);
            _popupWindowViewModel = new PopupWindowViewModel();
            DataContext = _popupWindowViewModel;
        }

        private void InitializeOptions(List<string> options)
        {
            foreach (var option in options)
            {
                var radioButton = new RadioButton
                {
                    Content = option,
                    Style = FindResource("RadioButtonStyle") as Style,
                    Tag = option
                };
                radioButton.Checked += RadioButton_Checked;
                OptionsPanel.Children.Add(radioButton);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                _selectedOption = radioButton.Tag as string;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedOption))
            {
                MessageBox.Show("Please select an option.", "Selection Required",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _onOptionSelected?.Invoke(_selectedOption);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void ShowOptionsPopup(List<string> options, Action<string> onOptionSelected)
        {
            var window = new OptionsPopupWindow(options, onOptionSelected);
            window.ShowDialog();
        }
    }
}
