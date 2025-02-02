using ArtWiz.Domain.Base;
using ArtWiz.ViewModel.Base;
using ArtWiz.ViewModel.Widgets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows;
using WizMachine.Data;
using ArtWiz.ViewModel.PakEditor.Base;

namespace ArtWiz.ViewModel.PakEditor
{
    internal class PakBlockItemViewModel : PakItemViewModel, IParseBlockDataCallback, IDisposable
    {
        private string _blockName;
        private string _blockId;
        private FrameRGBA[]? _frameData;
        private SprFileHead _sprFileHead;
        private BitmapSource[]? _frameSource;
        private string _stringBlockData;
        private bool _isLoadingBlock;
        private IBitmapViewerViewModel _bitmapViewerVM;
        private IFileHeadEditorViewModel _fileHeadEditorVM;
        private PakViewModelManager _viewModelManager;
        private Visibility _sprInfoPanelCollapseButtonVisibility = Visibility.Hidden;

        public bool IsSpr { get; private set; }

        [Bindable(true)]
        public string TextData
        {
            get => _stringBlockData;
            set
            {
                _stringBlockData = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public Visibility SprInfoPanelCollapseButtonVisibility
        {
            get => _sprInfoPanelCollapseButtonVisibility;
            set
            {
                _sprInfoPanelCollapseButtonVisibility = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public IFileHeadEditorViewModel FileHeadEditorVM
        {
            get => _fileHeadEditorVM;
        }

        [Bindable(true)]
        public IBitmapViewerViewModel BitmapViewerVM
        {
            get => _bitmapViewerVM;
            set
            {
                _bitmapViewerVM = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public bool IsLoadingBlock
        {
            get
            {
                return _isLoadingBlock;
            }
            set
            {
                _isLoadingBlock = value;
                Invalidate();
            }
        }
        [Bindable(true)]
        public string BlockName
        {
            get
            {
                return _blockName;
            }
            set
            {
                _blockName = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public PakBlockType BlockType
        {
            get
            {
                return IsSpr ? PakBlockType.SPR : PakBlockType.UNKNOWN;
            }
        }

        [Bindable(true)]
        public string BlockId
        {
            get
            {
                return _blockId;
            }
        }

        [Bindable(true)]
        public string BlockSize
        {
            get
            {
                return FormatFileSize(_itemSizeInBytes);
            }
        }

        public Dispatcher ViewDispatcher => ViewModelOwner.ViewDispatcher;

        public PakBlockItemViewModel(BaseParentsViewModel parents,
            string blockName,
            bool isSpr,
            string blockId,
            long blockSize, PakViewModelManager vmmanager) : base(parents)
        {
            _fileHeadEditorVM = new FileHeadEditorViewModel(this);

            if (isSpr)
                _bitmapViewerVM = new BlockAnimationViewerViewModel(this);
            else
                _bitmapViewerVM = new BitmapViewerViewModel(this);
            _bitmapViewerVM.IsSpr = isSpr;
            _itemSizeInBytes = blockSize;
            _blockName = blockName;
            IsSpr = isSpr;
            _blockId = blockId;
            _viewModelManager = vmmanager;
        }

        public bool IsDataLoaded()
        {
            if (IsSpr)
            {
                return _frameData != null;
            }
            else
            {
                return !string.IsNullOrEmpty(_stringBlockData);
            }
        }

        public void StartLoadingBlockData()
        {
            if (IsDataLoaded())
            {
                return;
            }
            IsLoadingBlock = true;
            PakWorkManager.ParseSprBlockDataById(_blockId, this);
        }

        public void GetSprData(out SprFileHead sprFileHead, out FrameRGBA[] frameRGBAs)
        {
            sprFileHead = this._sprFileHead;
            frameRGBAs = this._frameData;
        }

        public void OnParseTextSuccessfully(string blockId, string text)
        {
            TextData = text;
            _viewModelManager.EnqueueLoadedSuccessfullyBlockData(this);
        }

        public void OnParseSprSuccessfully(string blockId, SprFileHead sprFileHead, FrameRGBA[] frameData, BitmapSource bitmapSource)
        {
            var s = new BitmapSource[sprFileHead.FrameCounts];
            s[0] = bitmapSource;
            _frameData = frameData;
            _sprFileHead = sprFileHead;
            _frameSource = s;
            IsLoadingBlock = false;
            var frameInfo = frameData[0];
            _bitmapViewerVM.FrameOffX = frameInfo.frameOffX;
            _bitmapViewerVM.FrameOffY = frameInfo.frameOffY;
            _bitmapViewerVM.FrameHeight = frameInfo.frameHeight;
            _bitmapViewerVM.FrameWidth = frameInfo.frameWidth;
            _bitmapViewerVM.GlobalWidth = sprFileHead.GlobalWidth;
            _bitmapViewerVM.GlobalHeight = sprFileHead.GlobalHeight;
            _bitmapViewerVM.GlobalOffX = sprFileHead.OffX;
            _bitmapViewerVM.GlobalOffY = sprFileHead.OffY;
            _bitmapViewerVM.FrameSource = bitmapSource;
            _viewModelManager.EnqueueLoadedSuccessfullyBlockData(this);
            SprInfoPanelCollapseButtonVisibility = Visibility.Visible;

            FileHeadEditorVM.CurrentFrameData = frameInfo;
            FileHeadEditorVM.CurrentFrameIndex = 0;
            FileHeadEditorVM.IsSpr = true;
            FileHeadEditorVM.IsEditable = false;
            FileHeadEditorVM.FileHead = sprFileHead;
        }

        public void OnFinishJob()
        {
        }

        public void Dispose()
        {
            _frameData = null;
            _frameSource = null;
            BitmapViewerVM = new BitmapViewerViewModel(this);
            TextData = "";
        }
    }

}
