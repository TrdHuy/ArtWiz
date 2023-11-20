using SPRNetTool.Domain.Base;
using SPRNetTool.Domain;
using SPRNetTool.Utils;
using System.Windows.Media.Imaging;
using static SPRNetTool.Domain.BitmapDisplayMangerChangedArg.ChangedEvent;

namespace SPRNetTool.ViewModel.Widgets
{
    public class BitmapViewerViewModel : BaseViewModel, IBitmapViewerViewModel
    {
        private BitmapSource? _frameSource;
        public uint _globalWidth = 0;
        public uint _globalHeight = 0;
        public int _globalOffX = 0;
        public int _globalOffY = 0;
        public uint _frameHeight = 0;
        public uint _frameWidth = 0;
        public int _frameOffX = 0;
        public int _frameOffY = 0;
        public bool _isSpr;

        public BitmapSource? FrameSource
        {
            get => _frameSource;
            set
            {
                _frameSource = value;
                Invalidate();
            }
        }
        public uint GlobalWidth
        {
            get => _globalWidth; set
            {
                _globalWidth = value;
                Invalidate();
            }
        }
        public uint GlobalHeight
        {
            get => _globalHeight; set
            {
                _globalHeight = value;
                Invalidate();
            }
        }
        public int GlobalOffX
        {
            get => _globalOffX; set
            {
                _globalOffX = value;
                Invalidate();
            }
        }
        public int GlobalOffY
        {
            get => _globalOffY; set
            {
                _globalOffY = value;
                Invalidate();
            }
        }
        public uint FrameHeight
        {
            get => _frameHeight; set
            {
                _frameHeight = value;
                Invalidate();
            }
        }
        public uint FrameWidth
        {
            get => _frameWidth; set
            {
                _frameWidth = value;
                Invalidate();
            }
        }
        public int FrameOffX
        {
            get => _frameOffX; set
            {
                _frameOffX = value;
                Invalidate();
            }
        }
        public int FrameOffY
        {
            get => _frameOffY; set
            {
                _frameOffY = value;
                Invalidate();
            }
        }
        public bool IsSpr
        {
            get => _isSpr;
            set
            {
                _isSpr = value;
                Invalidate();
            }
        }

        public BitmapViewerViewModel()
        {
            BitmapDisplayManager.RegisterObserver(this);
        }

        ~BitmapViewerViewModel()
        {
            BitmapDisplayManager.UnregisterObserver(this);
        }

        protected override void OnDomainChanged(IDomainChangedArgs args)
        {
            if (IsViewModelDestroyed) return;

            switch (args)
            {
                case BitmapDisplayMangerChangedArg castArgs:
                    if (castArgs.Event.HasFlag(IS_PLAYING_ANIMATION_CHANGED))
                    {

                    }
                    else
                    {
                        if (castArgs.Event.HasFlag(SPR_FILE_PALETTE_CHANGED))
                        {
                            // TODO: Consider to notify to update image when palette changed
                        }

                        if (castArgs.Event.HasFlag(SPR_FILE_HEAD_CHANGED))
                        {
                            IsSpr = castArgs.CurrentSprFileHead != null;
                        }

                        if (castArgs.Event.HasFlag(SPR_GLOBAL_OFFSET_CHANGED))
                        {
                            castArgs.CurrentSprFileHead?.Apply(it =>
                            {
                                GlobalOffX = it.modifiedSprFileHeadCache.offX;
                                GlobalOffY = it.modifiedSprFileHeadCache.offY;
                            });
                        }

                        if (castArgs.Event.HasFlag(SPR_GLOBAL_SIZE_CHANGED))
                        {
                            castArgs.CurrentSprFileHead?.Apply(it =>
                            {
                                GlobalHeight = it.modifiedSprFileHeadCache.globalHeight;
                                GlobalWidth = it.modifiedSprFileHeadCache.globalWidth;
                            });
                        }

                        if (castArgs.Event.HasFlag(CURRENT_DISPLAYING_SOURCE_CHANGED))
                        {
                            FrameSource = castArgs.CurrentDisplayingSource;
                            if (!IsSpr)
                            {
                                GlobalHeight = (uint)(castArgs.CurrentDisplayingSource?.PixelHeight ?? 0);
                                GlobalWidth = (uint)(castArgs.CurrentDisplayingSource?.PixelWidth ?? 0);
                                FrameHeight = (uint)(castArgs.CurrentDisplayingSource?.PixelHeight ?? 0);
                                FrameWidth = (uint)(castArgs.CurrentDisplayingSource?.PixelWidth ?? 0);
                            }

                        }

                        if (castArgs.Event.HasFlag(SPR_FRAME_OFFSET_CHANGED))
                        {
                            castArgs.SprFrameData?.Apply(it =>
                            {
                                FrameOffX = it.modifiedFrameRGBACache.frameOffX;
                                FrameOffY = it.modifiedFrameRGBACache.frameOffY;
                            });
                        }

                        if (castArgs.Event.HasFlag(SPR_FRAME_SIZE_CHANGED))
                        {
                            castArgs.SprFrameData?.Apply(it =>
                            {
                                FrameHeight = it.modifiedFrameRGBACache.frameHeight;
                                FrameWidth = it.modifiedFrameRGBACache.frameWidth;
                            });
                        }
                    }
                    break;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            BitmapDisplayManager.UnregisterObserver(this);
        }
    }


    public class BitmapViewerViewModel2 : BaseViewModel
    {
        private IBitmapViewerViewModel bitmapViewerVM;
        public IBitmapViewerViewModel BitmapViewerVM { get => bitmapViewerVM; }

        public BitmapViewerViewModel2()
        {
            bitmapViewerVM = new BitmapViewerViewModel();
        }

    }


}
