using System.Windows.Threading;

namespace SPRNetTool.ViewModel.Base
{
    public interface IArtWizViewModelOwner
    {
        Dispatcher ViewDispatcher { get; }

    }
}
