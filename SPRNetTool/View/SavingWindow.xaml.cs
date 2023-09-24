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

