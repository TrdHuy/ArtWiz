namespace SPRNetTool.ViewModel.CommandVM
{
    public interface IDebugPageCommand
    {
        void OnPlayPauseAnimationSprClicked();

        void OnSaveCurrentWorkManagerToFileSprClicked(string filePath);

        void OnIncreaseFrameOffsetXButtonClicked();

        void OnIncreaseCurrentlyDisplayedSprFrameIndex();
        void OnDecreaseCurrentlyDisplayedSprFrameIndex();
    }
}
