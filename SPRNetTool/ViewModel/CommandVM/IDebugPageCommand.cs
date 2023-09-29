using System.Security.Cryptography;

namespace SPRNetTool.ViewModel.CommandVM
{
    public interface IDebugPageCommand
    {
        void OnPlayPauseAnimationSprClicked();

        void OnSaveCurrentWorkManagerToFileSprClicked(string filePath);

        void OnIncreaseFrameOffsetXButtonClicked(uint delta = 1);
        void OnDecreaseFrameOffsetXButtonClicked(uint delta = 1);

        void OnIncreaseFrameOffsetYButtonClicked(uint delta = 1);
        void OnDecreaseFrameOffsetYButtonClicked(uint delta = 1);

        void OnIncreaseCurrentlyDisplayedSprFrameIndex();
        void OnDecreaseCurrentlyDisplayedSprFrameIndex();

        void OnDecreaseIntervalButtonClicked();
        void OnIncreaseIntervalButtonClicked();
    }
}
