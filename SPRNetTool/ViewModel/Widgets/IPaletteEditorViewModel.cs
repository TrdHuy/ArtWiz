using ArtWiz.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ArtWiz.ViewModel.Widgets
{
    public interface IPaletteEditorViewModel : IArtWizViewModel
    {
        public ObservableCollection<IPaletteEditorColorItemViewModel>? PaletteColorItemSource { get; set; }

    }

    public interface IPaletteEditorColorItemViewModel
    {
        SolidColorBrush ColorBrush { get; set; }
    }
}
