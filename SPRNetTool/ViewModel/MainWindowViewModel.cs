using ArtWiz.ViewModel.Base;
namespace ArtWiz.ViewModel
{
    internal class MainWindowViewModel : ArtWizWindowViewModel
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
            IsTitleBarHide = false;
        }
    }
}
