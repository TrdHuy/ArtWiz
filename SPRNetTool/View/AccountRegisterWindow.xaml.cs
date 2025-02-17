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
    /// Interaction logic for AccountRegisterWindow.xaml
    /// </summary>
    public partial class AccountRegisterWindow : BaseArtWizWindow
    {
        private ArtWizWindowViewModel _accountRegisterWindowViewModel;
        public AccountRegisterWindow()
        {
            InitializeComponent();
            _accountRegisterWindowViewModel = new ArtWizWindowViewModel();
            DataContext = _accountRegisterWindowViewModel;
        }
    }
}
