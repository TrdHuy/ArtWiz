using System.Windows;

namespace SPRNetTool.View
{
    /// <summary>
    /// Interaction logic for SavingWindow.xaml
    /// </summary>
    public partial class SavingWindow : Window
    {


        public SavingWindow(Window? owner = null)
        {
            InitializeComponent();
            Owner = owner;
        }


        public void Show()
        {
            base.ShowDialog();

        }

        public void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void AgreeClick(object sender, RoutedEventArgs e)
        {

        }

    }
}

