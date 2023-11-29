using SPRNetTool.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace SPRNetTool.ViewModel.Widgets
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
