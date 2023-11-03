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

        void OnIncreaseSprGlobalOffsetXButtonClicked(uint delta = 1);
        void OnDecreaseSprGlobalOffsetXButtonClicked(uint delta = 1);
        void OnIncreaseSprGlobalOffsetYButtonClicked(uint delta = 1);
        void OnDecreaseSprGlobalOffsetYButtonClicked(uint delta = 1);

        void OnIncreaseSprGlobalWidthButtonClicked(uint delta = 1);
        void OnDecreaseSprGlobalWidthButtonClicked(uint delta = 1);
        void OnIncreaseSprGlobalHeightButtonClicked(uint delta = 1);
        void OnDecreaseSprGlobalHeightButtonClicked(uint delta = 1);

        void SetSprGlobalSize(ushort? width = null, ushort? height = null);
        void SetSprGlobalOffset(short? offX = null, short? offY = null);

        void SetSprFrameSize(ushort? width = null, ushort? height = null);
        void SetSprFrameOffset(short? offX = null, short? offY = null);
        void SetSprInterval(ushort interval);

        bool OnSwitchFrameIndex(uint frameIndex1, uint frameIndex2);

        bool OnRemoveFrameIndex(uint frameIndex);
    }
}
