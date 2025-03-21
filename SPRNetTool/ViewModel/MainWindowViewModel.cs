using ArtWiz.ViewModel.Base;
namespace ArtWiz.ViewModel
{
    internal class MainWindowViewModel : ArtWizWindowViewModel, ICheckAppUpdateViewModel
    {
        public bool IsDebugMode
        {
            get
            {
                return DeviceConfigManager.IsDebugMode();
            }
        }

        public MainWindowViewModel()
        {
            IsCloseButtonUsedOnlyOnTitleBar = false;
        }
    }
}
