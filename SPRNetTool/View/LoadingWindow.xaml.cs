using SPRNetTool.View.Base;
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
using System.Windows.Threading;

namespace SPRNetTool.View
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow(Window owner, string tilte = "Loading")
        {
            this.Owner = owner;
            InitializeComponent();
            TitleView.Text = tilte;

            if (Owner != null || Owner is INetView)
            {
                (Owner as INetView)?.DisableWindow(true);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            (Owner as INetView)?.DisableWindow(false);
        }

        public async void Show(Func<Task> block, Action? callback = null, int delay = 1000)
        {
            base.Show();
            await Task.Delay(delay);
            await block.Invoke();
            callback?.Invoke();
            this.Close();
        }
    }
}
