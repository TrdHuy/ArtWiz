using ArtWiz.Domain;
using ArtWiz.Domain.Base;
using ArtWiz.Domain.Utils;
using ArtWiz.LogUtil;
using ArtWiz.Utils;
using ArtWiz.View.Utils;
using ArtWiz.View.Widgets;
using ArtWiz.ViewModel.Base;
using ArtWiz.ViewModel.CommandVM;
using ArtWiz.ViewModel.PakEditor.Base;
using ArtWiz.ViewModel.Widgets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WizMachine.Data;
using WizMachine.Services.Base;
using WizMachine.Services.Utils.NativeEngine.Managed;

namespace ArtWiz.ViewModel.PakEditor
{
    internal class PakViewModelManager
    {
        private Dictionary<string, PakBlockItemViewModel> _blockIdToPakBlockItemMap = new Dictionary<string, PakBlockItemViewModel>();
        private Dictionary<string, PakFileItemViewModel> _filePathToPakFileItemMap = new Dictionary<string, PakFileItemViewModel>();
        private Dictionary<PakBlockItemViewModel, PakFileItemViewModel> _blockItemToFileItemMap = new Dictionary<PakBlockItemViewModel, PakFileItemViewModel>();
        private Dictionary<PakFileItemViewModel, HashSet<PakBlockItemViewModel>> _fileItemToBlockItemMap = new Dictionary<PakFileItemViewModel, HashSet<PakBlockItemViewModel>>();
        private readonly Queue<PakBlockItemViewModel> _loadedDataBlockItemQueue;
        private int _blockDataSize;
        public PakViewModelManager(int blockDataSizeCache = 3)
        {
            _blockDataSize = blockDataSizeCache;
            _loadedDataBlockItemQueue = new Queue<PakBlockItemViewModel>();
        }

        public ObservableCollection<PakFileItemViewModel> GetPakFileItemViewModel()
        {
            foreach (var pair in _fileItemToBlockItemMap)
            {
                var pakBlockVMs = new ObservableCollection<PakBlockItemViewModel>(pair.Value);
                pair.Key.PakBlocks = pakBlockVMs;
            }
            return new ObservableCollection<PakFileItemViewModel>(_filePathToPakFileItemMap.Values);
        }

        public PakBlockItemViewModel CreatePakBlockViewModel(PakFileItemViewModel pakFileItemViewModel,
            string blockName,
            bool isSpr,
            string blockId,
            long blockSize)
        {
            var blockItemViewModel = new PakBlockItemViewModel(pakFileItemViewModel,
                    blockName: blockName,
                    isSpr: isSpr,
                    blockId: blockId,
                    blockSize: blockSize,
                    vmmanager: this
                    );
            _blockIdToPakBlockItemMap.Add(blockId, blockItemViewModel);
            _blockItemToFileItemMap.Add(blockItemViewModel, pakFileItemViewModel);
            if (!_fileItemToBlockItemMap.ContainsKey(pakFileItemViewModel))
            {
                var h = new HashSet<PakBlockItemViewModel>();
                h.Add(blockItemViewModel);
                _fileItemToBlockItemMap.Add(pakFileItemViewModel, h);
            }
            else
            {
                _fileItemToBlockItemMap[pakFileItemViewModel].Add(blockItemViewModel);
            }
            return blockItemViewModel;
        }

        public PakFileItemViewModel CreatePakItemViewModel(PakPageViewModel pageViewModel, string filePath)
        {
            var newFile = new PakFileItemViewModel(this, pageViewModel, filePath);
            _filePathToPakFileItemMap.Add(filePath, newFile);
            return newFile;
        }

        public void DeletePakFileItemViewModel(PakFileItemViewModel pakFileItem)
        {
            if (_fileItemToBlockItemMap.ContainsKey(pakFileItem))
            {
                foreach (var pakBlock in _fileItemToBlockItemMap[pakFileItem])
                {
                    _blockItemToFileItemMap.Remove(pakBlock);
                    _blockIdToPakBlockItemMap.Remove(pakBlock.BlockId);
                }
                _fileItemToBlockItemMap.Remove(pakFileItem);
            }
            _filePathToPakFileItemMap.Remove(pakFileItem.FilePath);
        }

        public PakFileItemViewModel? GetPakFileItemViewModel(string filePath)
        {
            if (!_filePathToPakFileItemMap.ContainsKey(filePath))
            {
                return null;
            }
            return _filePathToPakFileItemMap[filePath];
        }

        public (PakBlockItemViewModel, PakFileItemViewModel)? FindPakBlockById(string blockId)
        {
            if (_blockIdToPakBlockItemMap.ContainsKey(blockId))
            {
                var blockViewModel = _blockIdToPakBlockItemMap[blockId];
                return (blockViewModel, _blockItemToFileItemMap[blockViewModel]);
            }
            return null;
        }

        public void EnqueueLoadedSuccessfullyBlockData(PakBlockItemViewModel item)
        {
            if (_loadedDataBlockItemQueue.Count >= _blockDataSize)
            {
                // Remove the oldest item and dispose of it to free up memory
                var oldestItem = _loadedDataBlockItemQueue.Dequeue();
                oldestItem.Dispose();
            }

            _loadedDataBlockItemQueue.Enqueue(item);
        }
    }


    internal class PakPageViewModel : BaseParentsViewModel, IPakPageCommand, IRemovePakFileCallback
    {
        private static Logger logger = new Logger(typeof(PakPageViewModel).Name);
        private PakFileItemViewModel? _currentSelectedPakFile;
        private ObservableCollection<PakFileItemViewModel> _pakFiles;
        private PakViewModelManager _viewModelManager;
        private Visibility _searchBoxVisibility = Visibility.Visible;
        private Visibility _initPanelVisibility = Visibility.Visible;
        private Visibility _detailPanelVisibility = Visibility.Visible;
        private string _blockFolderOutputPath = "";

        public string BlockFolderOutputPath
        {
            get
            {
                return _blockFolderOutputPath;
            }
            set
            {
                _blockFolderOutputPath = value;
                Invalidate();
            }
        }
        public Visibility DetailPanelVisibility
        {
            get
            {
                return _detailPanelVisibility;
            }
            set
            {
                _detailPanelVisibility = value;
                Invalidate();
            }
        }

        public Visibility InitPanelVisibility
        {
            get
            {
                return _initPanelVisibility;
            }
            set
            {
                _initPanelVisibility = value;
                Invalidate();
            }
        }
        public Visibility SearchBoxVisibility
        {
            get
            {
                return _searchBoxVisibility;
            }
            set
            {
                _searchBoxVisibility = value;
                Invalidate();
            }
        }

        public ObservableCollection<PakFileItemViewModel> PakFiles
        {
            get => _pakFiles;
            private set
            {
                _pakFiles = value;
                Invalidate();
            }
        }

        public PakFileItemViewModel? CurrentSelectedPakFile
        {
            get => _currentSelectedPakFile;
            set
            {
                _currentSelectedPakFile = value;
                Invalidate();
            }
        }

        public Dispatcher ViewDispatcher => ViewModelOwner.ViewDispatcher;

        public PakPageViewModel()
        {
            _pakFiles = new ObservableCollection<PakFileItemViewModel>();
            _pakFiles.CollectionChanged += OnPakItemViewModelCollectionChanged;
            _viewModelManager = new PakViewModelManager();
            UpdatePakEditorComponentVisibility();
        }

        private void OnPakItemViewModelCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePakEditorComponentVisibility();
        }

        void IPakPageCommand.OnAddedPakFileClick(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                logger.E($"File path is invalid or does not exist: {filePath}");
                return;
            }

            if (PakWorkManager.IsFileAlreadyAdded(filePath))
            {
                logger.E($"File already added {filePath}");
                return;
            }

            var pakItemViewModel = _viewModelManager.CreatePakItemViewModel(this, filePath);
            pakItemViewModel.PropertyChanged += OnPakItemPropertyChanged;
            PakFiles.Add(pakItemViewModel);
        }

        private void OnPakItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is PakFileItemViewModel pIVM && e.PropertyName == nameof(PakFileItemViewModel.CurrentSelectedPakBlock))
            {
                if (pIVM.CurrentSelectedPakBlock != null && pIVM.CurrentSelectedPakBlock.IsDataLoaded() == false)
                {
                    if (pIVM.CurrentSelectedPakBlock.IsSpr)
                    {
                        pIVM.CurrentSelectedPakBlock.StartLoadingBlockData();
                    }
                    else
                    {

                    }
                }
                else
                {
                }
            }
        }

        void IPakPageCommand.OnResetSearchBox()
        {
            if (CurrentSelectedPakFile != null)
                CurrentSelectedPakFile.CurrentSelectedPakBlock = null;
            PakFiles.CollectionChanged -= OnPakItemViewModelCollectionChanged;
            PakFiles = _viewModelManager.GetPakFileItemViewModel();
            PakFiles.CollectionChanged += OnPakItemViewModelCollectionChanged;
            CurrentSelectedPakFile = null;
        }

        void IPakPageCommand.OnRemovePakFileClick(object pakFileViewModel)
        {
            pakFileViewModel.IfIs<PakFileItemViewModel>(it =>
            {
                if (PakWorkManager.IsFileAlreadyAdded(it.FilePath))
                {
                    PakWorkManager.CloseSessionAsync(it.FilePath, this);
                }
                else if (it.LoadingStatus == PakItemLoadingStatus.NONE)
                {
                    var vm = _viewModelManager.GetPakFileItemViewModel(it.FilePath);
                    if (vm != null)
                    {
                        vm.PropertyChanged -= OnPakItemPropertyChanged;
                        _viewModelManager.DeletePakFileItemViewModel(vm);
                        PakFiles.Remove(vm);
                    }
                }
            });
        }

        void IPakPageCommand.OnSearchPakBlockByPath(string blockPath)
        {
            var blockInfo = PakWorkManager.GetBlockInfoByPath(blockPath);
            if (blockInfo != null)
            {
                (PakBlockItemViewModel blockVM, PakFileItemViewModel pakFileVM)? result =
                        _viewModelManager.FindPakBlockById(blockInfo.Value.id);
                if (result.HasValue)
                {
                    var (blockVM, pakFileVM) = result.Value;
                    PakFiles.Clear();
                    PakFiles.Add(pakFileVM);
                    pakFileVM.PakBlocks.Clear();
                    pakFileVM.PakBlocks.Add(blockVM);
                    pakFileVM.CurrentSelectedPakBlock = blockVM;
                    CurrentSelectedPakFile = pakFileVM;
                }
                else
                {
                }
            }
        }

        void IPakPageCommand.OnExtractCurrentSelectedBlock()
        {
            if (CurrentSelectedPakFile != null && CurrentSelectedPakFile.CurrentSelectedPakBlock != null)
            {
                PakBlockItemViewModel pakBlock = CurrentSelectedPakFile!.CurrentSelectedPakBlock!;

                string? outputPath = _blockFolderOutputPath;
                if (string.IsNullOrEmpty(outputPath) || !Directory.Exists(outputPath))
                {
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.Description = "Chọn thư mục để lưu block đã extract:";
                        folderDialog.ShowNewFolderButton = true;

                        if (folderDialog.ShowDialog() == DialogResult.OK)
                        {
                            outputPath = folderDialog.SelectedPath;
                            _blockFolderOutputPath = outputPath;
                        }
                        else
                        {
                            logger.I("Người dùng không chọn thư mục.");
                            return;
                        }
                    }
                }

                if (pakBlock.BlockType == PakBlockType.SPR)
                {
                    outputPath = Path.Combine(outputPath, pakBlock.BlockName + ".spr");
                }
                else
                {
                    outputPath = Path.Combine(outputPath, pakBlock.BlockName + ".txt");
                }

                bool success = PakWorkManager.ExtractPakBlockById(pakBlock.BlockId, outputPath!);
                if (success)
                {
                    logger.I($"Extract block '{pakBlock.BlockId}' thành công vào '{outputPath}'.");
                }
                else
                {
                    logger.I($"Extract block '{pakBlock.BlockId}' thất bại.");
                }
            }
            else
            {
                logger.E("Không có PakFile hoặc PakBlock nào được chọn.");
            }
        }

        public void OnRemoveSuccess(object removedPakFile)
        {
            removedPakFile.IfIs<string>(it =>
            {
                var viewModel = _viewModelManager.GetPakFileItemViewModel(it);
                if (viewModel != null)
                {
                    viewModel.PropertyChanged -= OnPakItemPropertyChanged;
                    _viewModelManager.DeletePakFileItemViewModel(viewModel);
                    PakFiles.Remove(viewModel);
                }
            });
        }

        public void OnFinishJob()
        {
        }

        private void UpdatePakEditorComponentVisibility()
        {
            if (PakFiles.Count == 0)
            {
                InitPanelVisibility = Visibility.Visible;
                DetailPanelVisibility = Visibility.Collapsed;
                SearchBoxVisibility = Visibility.Collapsed;
            }
            else
            {
                InitPanelVisibility = Visibility.Collapsed;
                DetailPanelVisibility = Visibility.Visible;
                SearchBoxVisibility = Visibility.Visible;
            }
        }

    }
}
