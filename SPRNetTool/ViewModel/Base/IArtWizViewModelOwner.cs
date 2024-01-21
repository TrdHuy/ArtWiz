using System.Windows.Threading;

namespace ArtWiz.ViewModel.Base
{
    public interface IArtWizViewModelOwner
    {
        Dispatcher ViewDispatcher { get; }
    }
}
