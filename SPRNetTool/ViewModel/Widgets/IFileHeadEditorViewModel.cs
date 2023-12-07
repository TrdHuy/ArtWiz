using SPRNetTool.Data;
using SPRNetTool.ViewModel.Base;

namespace SPRNetTool.ViewModel.Widgets
{
    public interface IFileHeadEditorViewModel : IArtWizViewModel
    {
        public bool IsSpr { get; set; }
        public SprFileHead FileHead { get; set; }
        public int CurrentFrameIndex { get; set; }
        public FrameRGBA CurrentFrameData { get; set; }
        public int PixelHeight { get; set; }
        public int PixelWidth { get; set; }
    }
}
