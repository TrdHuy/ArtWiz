namespace SPRNetTool.ViewModel.CommandVM
{
    public interface IDebugPageCommand
    {
        void OnPlayPauseAnimationSprClicked();

        void OnSaveCurrentWorkManagerToFileSprClicked(string filePath);

        void OnDecreaseFrameHeightButtonClicked(uint delta = 1);
        void OnIncreaseFrameHeightButtonClicked(uint delta = 1);
        void OnIncreaseFrameOffsetXButtonClicked(uint delta = 1);
        void OnDecreaseFrameOffsetXButtonClicked(uint delta = 1);

        void OnIncreaseFrameWidthButtonClicked(uint delta = 1);
        void OnDecreaseFrameWidthButtonClicked(uint delta = 1);

        void OnIncreaseFrameOffsetYButtonClicked(uint delta = 1);
        void OnDecreaseFrameOffsetYButtonClicked(uint delta = 1);

        void OnIncreaseCurrentlyDisplayedSprFrameIndex();
        void OnDecreaseCurrentlyDisplayedSprFrameIndex();

        void OnDecreaseIntervalButtonClicked();
        void OnIncreaseIntervalButtonClicked();

        void OnSaveCurrentDisplayedBitmapSourceToSpr(string filePath);

    }
}
