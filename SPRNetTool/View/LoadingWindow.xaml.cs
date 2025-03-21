using ArtWiz.View.Base;
using ArtWiz.ViewModel;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ArtWiz.View
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        private double offsetX = 0d;
        private double offsetY = 0d;
        public LoadingWindow()
        {
            InitializeComponent();
            var ctx = new ArtWizWindowViewModel();
            DataContext = ctx;
        }

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
            this.Left = newX + offsetX; // mOwnerWindowLocationOffsetX là khoảng cách ngang giữa A và B
            this.Top = newY + offsetY; // mOwnerWindowLocationOffsetY là khoảng cách dọc giữa A và B
        }

        protected override void OnClosed(EventArgs e)
        {
            Owner.LocationChanged -= Owner_LocationChanged;
            base.OnClosed(e);
            (Owner as IWindowViewer)?.DisableWindow(false);
        }

        public async void Show(Func<Action<double>, Task> block, Action? callback = null, int delay = 1000)
        {
            base.Show();
            offsetX = Left - Owner.Left;
            offsetY = Top - Owner.Top;
            await Task.Delay(delay);
            await block.Invoke(OnProgressBarChanged);
            callback?.Invoke();
            this.Close();
        }

        private void OnProgressBarChanged(double progress)
        {
            Dispatcher.Invoke(() =>
            {
                TaskProgressbar.Value = progress;
            });
        }
    }
}
