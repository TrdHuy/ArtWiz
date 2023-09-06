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
        private double offsetX = 0d;
        private double offsetY = 0d;
        public LoadingWindow(Window owner, string tilte = "Loading")
        {
            this.Owner = owner;
            InitializeComponent();
            TitleView.Text = tilte;

            if (Owner != null || Owner is IWindowViewer)
            {
                Owner.LocationChanged += Owner_LocationChanged;
                (Owner as IWindowViewer)?.DisableWindow(true);
            }
        }

        private void Owner_LocationChanged(object? sender, EventArgs e)
        {
            // Lấy vị trí mới của window A
            double newX = Owner.Left;
            double newY = Owner.Top;
            // Di chuyển window B tương ứng với vị trí của window A
            this.Left = newX + offsetX; // offsetX là khoảng cách ngang giữa A và B
            this.Top = newY + offsetY; // offsetY là khoảng cách dọc giữa A và B
        }

        protected override void OnClosed(EventArgs e)
        {
            Owner.LocationChanged -= Owner_LocationChanged;
            base.OnClosed(e);
            (Owner as IWindowViewer)?.DisableWindow(false);
        }

        public async void Show(Func<Task> block, Action? callback = null, int delay = 1000)
        {
            base.Show();
            offsetX = Left - Owner.Left;
            offsetY = Left - Owner.Left;
            await Task.Delay(delay);
            await block.Invoke();
            callback?.Invoke();
            this.Close();
        }

        
    }
}
