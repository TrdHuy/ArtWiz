using WizMachine.Data;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.Utils;
using SPRNetTool.ViewModel.Base;
using System.ComponentModel;
using System.Windows.Threading;
using static SPRNetTool.Domain.BitmapDisplayMangerChangedArg.ChangedEvent;

namespace SPRNetTool.ViewModel.Widgets
{
    class FileHeadEditorViewModel : BaseSubViewModel, IFileHeadEditorViewModel
    {
        private SprFileHead _sprFileHead;
        private FrameRGBA _sprFrameData;
        private int _currentFrameIndex = 0;
        private int _pixelHeight = 0;
        private int _pixelWidth = 0;
        private bool _isSpr;

        [Bindable(true)]
        public SprFileHead FileHead
        {
            get => _sprFileHead;
            set
            {
                _sprFileHead = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public int CurrentFrameIndex
        {
            get => _currentFrameIndex;
            set
            {
                _currentFrameIndex = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public FrameRGBA CurrentFrameData
        {
            get => _sprFrameData;
            set
            {
                _sprFrameData = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public int PixelHeight
        {
            get => _pixelHeight; set
            {
                _pixelHeight = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public int PixelWidth
        {
            get => _pixelWidth; set
            {
                _pixelWidth = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public bool IsSpr
        {
            get => _isSpr; set
            {
                _isSpr = value;
                Invalidate();
            }
        }

        public FileHeadEditorViewModel(BaseParentsViewModel parents) : base(parents)
        {
            BitmapDisplayManager.RegisterObserver(this);
        }

        ~FileHeadEditorViewModel()
        {
            BitmapDisplayManager.UnregisterObserver(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            BitmapDisplayManager.UnregisterObserver(this);
        }

        protected override void OnDomainChanged(IDomainChangedArgs args)
        {
            if (IsViewModelDestroyed) return;

            switch (args)
            {
                case BitmapDisplayMangerChangedArg castArgs:

                    if (castArgs.Event.HasFlag(SPR_WORKSPACE_RESET))
                    {
                        FileHead = new SprFileHead();
                        CurrentFrameData = new FrameRGBA();
                    }
                    else if (castArgs.Event.HasFlag(IS_PLAYING_ANIMATION_CHANGED))
                    {
                        if (castArgs.IsPlayingAnimation == true)
                        {
                            var dispatcherPriority = DispatcherPriority.Background;
                            if (FileHead.Interval > 20)
                            {
                                dispatcherPriority = DispatcherPriority.Render;
                            }

                            if (IsViewModelDestroyed) return;

                            ViewModelOwner?.ViewDispatcher.Invoke(() =>
                            {
                                CurrentFrameIndex = (int)castArgs.CurrentDisplayingFrameIndex;
                            }, dispatcherPriority);
                        }
                        else if (castArgs.IsPlayingAnimation == false)
                        {
                            CurrentFrameData = castArgs.SprFrameData ?? CurrentFrameData;
                            CurrentFrameIndex = (int)castArgs.CurrentDisplayingFrameIndex;
                        }
                    }
                    else
                    {
                        if (castArgs.Event.HasFlag(SPR_FILE_HEAD_CHANGED))
                        {
                            IsSpr = castArgs.CurrentSprFileHead != null;
                            castArgs.CurrentSprFileHead?.Apply(it => FileHead = it.modifiedSprFileHeadCache.ToSprFileHead());
                        }

                        if (castArgs.Event.HasFlag(CURRENT_DISPLAYING_FRAME_INDEX_CHANGED))
                        {
                            CurrentFrameIndex = (int)castArgs.CurrentDisplayingFrameIndex;
                        }

                        if (castArgs.Event.HasFlag(SPR_FRAME_DATA_CHANGED))
                        {
                            CurrentFrameData = castArgs.SprFrameData?.modifiedFrameRGBACache?.toFrameRGBA() ?? new FrameRGBA();
                        }
                    }
                    break;
            }
        }

    }
}
