using ArtWiz.Domain.Base;
using ArtWiz.ViewModel.Base;
using ArtWiz.ViewModel.PakEditor.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using WizMachine.Services.Base;
using WizMachine.Services.Utils.NativeEngine.Managed;

namespace ArtWiz.ViewModel.PakEditor
{
    internal class PakFileItemViewModel : PakItemViewModel, ILoadPakFileCallback
    {
        private string _crc = "Chưa xác định";
        private string _filePath;
        private string _mappingPath;
        private string _pakTime = "Chưa xác định";
        private string _pakTimeSave = "Chưa xác định";
        private int _blockCount = -1;
        protected int _loadingProgress;

        public override PakItemLoadingStatus LoadingStatus
        {
            get => base.LoadingStatus;
            set
            {
                base.LoadingStatus = value;
                Invalidate();
                Invalidate(nameof(ReloadPakVisibility));
                Invalidate(nameof(RemoveFilePakVisibility));
                Invalidate(nameof(LoadingStatusToString));
                Invalidate(nameof(LoadingProgressBarVisibility));
                Invalidate(nameof(ItemLoadingStatusVisibility));
                //Invalidate(nameof(StartLoadingButtonVisibility));
            }
        }

        private ObservableCollection<PakBlockItemViewModel> _pakBlocks = new ObservableCollection<PakBlockItemViewModel>();
        private PakViewModelManager _viewModelManager;
        private PakBlockItemViewModel? _currentSelectedPakBlock;
        public ObservableCollection<PakBlockItemViewModel> PakBlocks
        {
            get => _pakBlocks;
            set
            {
                _pakBlocks = value;
                Invalidate();
            }
        }
        public string FilePath { get => _filePath; }

        public PakBlockItemViewModel? CurrentSelectedPakBlock
        {
            get => _currentSelectedPakBlock;
            set
            {
                if (_currentSelectedPakBlock != null
                    && _currentSelectedPakBlock.BitmapViewerVM is BlockAnimationViewerViewModel vm)
                {
                    if (vm.IsPlayingAnimation)
                    {
                        vm.IsPlayingAnimation = false;
                    }
                }
                _currentSelectedPakBlock = value;
                Invalidate();
            }
        }

        public string MappingPath
        {
            get => _mappingPath;
            set
            {
                _mappingPath = value;
                Invalidate();
                Invalidate(nameof(MappingStatusToString));
            }
        }

        //[Bindable(true)]
        //public Visibility StartLoadingButtonVisibility
        //{
        //    get
        //    {
        //        if (LoadingStatus == PakItemLoadingStatus.NONE)
        //        {
        //            return Visibility.Collapsed;
        //        }
        //        else
        //        {
        //            return Visibility.Visible;
        //        }
        //    }
        //}

        [Bindable(true)]
        public Visibility LoadingProgressBarVisibility
        {
            get
            {
                if (LoadingStatus == PakItemLoadingStatus.LOADED ||
                    LoadingStatus == PakItemLoadingStatus.NONE)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        [Bindable(true)]
        public Visibility ReloadPakVisibility
        {
            get
            {
                if (LoadingStatus == PakItemLoadingStatus.ERROR || LoadingStatus == PakItemLoadingStatus.LOADED)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }


        [Bindable(true)]
        public Visibility ItemLoadingStatusVisibility
        {
            get
            {
                if (LoadingStatus == PakItemLoadingStatus.NONE
                    || LoadingStatus == PakItemLoadingStatus.LOADED
                    || LoadingStatus == PakItemLoadingStatus.ERROR)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        [Bindable(true)]
        public Visibility RemoveFilePakVisibility
        {
            get
            {
                if (LoadingStatus == PakItemLoadingStatus.LOADED ||
                    LoadingStatus == PakItemLoadingStatus.LOADING ||
                    LoadingStatus == PakItemLoadingStatus.ERROR)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        [Bindable(true)]
        public string LoadingStatusToString
        {
            get
            {
                switch (LoadingStatus)
                {
                    case PakItemLoadingStatus.LOADING:
                        return "loading...";
                    case PakItemLoadingStatus.LOADED:
                        return "Thành công";
                    case PakItemLoadingStatus.NONE:
                        return "Chưa bắt đầu";
                    case PakItemLoadingStatus.PREPARING:
                        return "preparing...";
                }
                return "";
            }
        }

        [Bindable(true)]
        public string ItemName
        {
            get
            {
                return Path.GetFileName(_filePath);
            }
        }

        [Bindable(true)]
        public string FileSize
        {
            get
            {
                return FormatFileSize(_itemSizeInBytes);
            }
        }

        [Bindable(true)]
        public int LoadingProgress
        {
            get
            {
                return _loadingProgress;
            }
            set
            {
                _loadingProgress = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public string MappingStatusToString
        {
            get
            {
                if (string.IsNullOrEmpty(_mappingPath))
                {
                    return "Chưa map";
                }
                else
                {
                    return "Đã map";
                }
                // return "Đã xảy ra lỗi trong quá trình mapping";
            }
        }

        [Bindable(true)]
        public string BlockCount
        {
            get
            {
                if (_blockCount == -1)
                {
                    return "Chưa xác định";
                }
                else
                {
                    return $"{_blockCount} khối";
                }
            }
            set
            {
                _blockCount = int.Parse(value);
                Invalidate();
            }
        }

        [Bindable(true)]
        public string PakTime
        {
            get
            {
                return _pakTime;
            }
            set
            {
                _pakTime = value;
                Invalidate();
            }
        }

        [Bindable(true)]
        public string PakTimeSave
        {
            get
            {
                return _pakTimeSave;
            }
            set
            {
                _pakTimeSave = value;
                Invalidate();
            }
        }
        [Bindable(true)]
        public string CRC
        {
            get
            {
                return _crc;
            }
            set
            {
                _crc = value;
                Invalidate();
            }
        }

        public Dispatcher ViewDispatcher => ViewModelOwner.ViewDispatcher;

        public PakFileItemViewModel(PakViewModelManager viewModelManager, BaseParentsViewModel parents, string filePath) : base(parents)
        {
            _viewModelManager = viewModelManager;
            _filePath = filePath;

            if (File.Exists(_filePath))
            {
                _itemSizeInBytes = new FileInfo(_filePath).Length;
            }
            else
            {
                _itemSizeInBytes = 0;
            }

        }

        public void StartLoadPakFileToWorkManagerAsync()
        {
            if (File.Exists(_filePath) && LoadingStatus == PakItemLoadingStatus.NONE)
            {
                LoadingStatus = PakItemLoadingStatus.PREPARING;
                PakWorkManager.LoadPakFileToWorkManagerAsync(_filePath, this);
            }
            else
            {
                LoadingStatus = PakItemLoadingStatus.ERROR;
            }
        }

        public void SetFilePath(string filePath)
        {
            _filePath = filePath;
            if (File.Exists(_filePath))
            {
                _itemSizeInBytes = new FileInfo(_filePath).Length;
            }
            else
            {
                _itemSizeInBytes = 0;
            }
            InvalidateAll();
        }

        public void OnSessionCreated(Bundle? bundle)
        {
            LoadingStatus = PakItemLoadingStatus.LOADING;
            BlockCount = (bundle?.GetInt(PakContract.EXTRA_BLOCK_COUNT_KEY) ?? -1).ToString();
            PakTime = bundle?.GetString(PakContract.EXTRA_PAK_TIME_KEY) ?? "";
            PakTimeSave = bundle?.GetString(PakContract.EXTRA_PAK_TIME_SAVE_KEY) ?? "";
            CRC = bundle?.GetString(PakContract.EXTRA_PAK_CRC_KEY) ?? "";
        }

        public void OnBlockLoaded(Bundle? bundle)
        {
            if (bundle != null)
            {
                var isSpr = bundle.GetBoolean(PakContract.EXTRA_BLOCK_IS_SPR_KEY) ?? false;
                var blockId = bundle.GetString(PakContract.EXTRA_BLOCK_ID_KEY) ?? "unknown";
                var blockSize = bundle.GetInt(PakContract.EXTRA_BLOCK_SIZE_KEY) ?? 0;
                var blockName = bundle.GetString(PakContract.EXTRA_BLOCK_FILE_NAME_KEY) ?? "unknown";
                var blockIndex = bundle.GetInt(PakContract.EXTRA_BLOCK_INDEX_KEY) ?? 0;


                var blockItemViewModel = _viewModelManager.CreatePakBlockViewModel(this, blockName: blockName,
                    isSpr: isSpr,
                    blockId: blockId,
                    blockSize: blockSize);
                PakBlocks.Add(blockItemViewModel);
            }
        }

        public void OnBlockLoadCompleted()
        {
        }

        public void OnLoadFailed()
        {
            LoadingStatus = PakItemLoadingStatus.ERROR;
        }

        public void OnProgressChanged(int newProgress)
        {
            LoadingProgress = newProgress;
        }

        public void OnFinishJob()
        {
            LoadingStatus = PakItemLoadingStatus.LOADED;
        }

        public void OnLoadCompleted(Bundle? bundle)
        {
        }
    }

}
