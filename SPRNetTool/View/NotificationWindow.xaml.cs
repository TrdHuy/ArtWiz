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
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {

        private readonly Action<bool> _onUserResponse;

        public NotificationWindow(string title, string message, Action<bool> onUserResponse)
        {
            InitializeComponent();
            DataContext = this;
            NotificationTitle = title;
            NotificationMessage = message;
            _onUserResponse = onUserResponse;
        }

        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _onUserResponse?.Invoke(true);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _onUserResponse?.Invoke(false);
            Close();
        }

        public static void ShowNotificationPopup(string title, string message, Action<bool> onUserResponse)
        {
            var popup = new NotificationWindow(title, message, onUserResponse);
            popup.ShowDialog();
        }
    }
}
