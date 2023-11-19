using SPRNetTool.Domain.Base;
using SPRNetTool.Domain;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using SPRNetTool.Utils;
using static SPRNetTool.Domain.BitmapDisplayMangerChangedArg.ChangedEvent;
using SPRNetTool.ViewModel.Base;

namespace SPRNetTool.ViewModel.Widgets
{
    public interface IBitmapViewerViewModel : INotifyPropertyChanged
    {
        public BitmapSource? FrameSource { get; set; }
        public uint GlobalWidth { get; set; }
        public uint GlobalHeight { get; set; }
        public uint GlobalOffX { get; set; }
        public uint GlobalOffY { get; set; }
        public uint FrameHeight { get; set; }
        public uint FrameWidth { get; set; }
        public uint FrameOffX { get; set; }
        public uint FrameOffY { get; set; }
        public bool IsSpr { get; set; }
    }

}
