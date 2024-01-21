using ArtWiz.ViewModel.Base;
using System.Windows.Media.Imaging;

namespace ArtWiz.ViewModel.Widgets
{
    public interface IBitmapViewerViewModel : IArtWizViewModel
    {
        public BitmapSource? FrameSource { get; set; }
        public uint GlobalWidth { get; set; }
        public uint GlobalHeight { get; set; }
        public int GlobalOffX { get; set; }
        public int GlobalOffY { get; set; }
        public uint FrameHeight { get; set; }
        public uint FrameWidth { get; set; }
        public int FrameOffX { get; set; }
        public int FrameOffY { get; set; }
        public bool IsSpr { get; set; }
    }

}
