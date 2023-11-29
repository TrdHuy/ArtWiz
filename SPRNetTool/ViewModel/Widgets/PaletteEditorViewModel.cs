using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain;
using SPRNetTool.Utils;
using SPRNetTool.ViewModel.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static SPRNetTool.Domain.BitmapDisplayMangerChangedArg.ChangedEvent;
using static SPRNetTool.Domain.SprFrameCollectionChangedArg.ChangedEvent;
using static SPRNetTool.Domain.SprPaletteChangedArg.ChangedEvent;
using System.Windows.Media;

namespace SPRNetTool.ViewModel.Widgets
{
    internal class PaletteEditorViewModel : BaseSubViewModel, IPaletteEditorViewModel
    {
        private ObservableCollection<IPaletteEditorColorItemViewModel>? _paletteColorItemSource;

        [Bindable(true)]
        public ObservableCollection<IPaletteEditorColorItemViewModel>? PaletteColorItemSource
        {
            get
            {
                return _paletteColorItemSource;
            }
            set
            {
                if (_paletteColorItemSource != value)
                {
                    _paletteColorItemSource = value;
                    Invalidate();
                }
            }
        }

        public PaletteEditorViewModel(BaseParentsViewModel parents) : base(parents)
        {
            BitmapDisplayManager.RegisterObserver(this);
        }

        ~PaletteEditorViewModel()
        {
            BitmapDisplayManager.UnregisterObserver(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            BitmapDisplayManager.UnregisterObserver(this);
        }

        protected override void OnDomainChanged(IDomainChangedArgs args)
        {
            if (IsViewModelDestroyed) return;

            switch (args)
            {
                case BitmapDisplayMangerChangedArg castArgs:
                    if (!castArgs.Event.HasFlag(IS_PLAYING_ANIMATION_CHANGED))
                    {
                        if (castArgs.Event.HasFlag(SPR_FILE_PALETTE_CHANGED))
                        {
                            castArgs.PaletteChangedArg?.Apply(it =>
                            {
                                if (it.Event.HasFlag(NEWLY_ADDED))
                                {
                                    ViewModelOwner?.ViewDispatcher.Invoke(() =>
                                    {
                                        PaletteColorItemSource = new ObservableCollection<IPaletteEditorColorItemViewModel>();

                                        it.Palette?.Data?.FoEach(pColor =>
                                        {
                                            PaletteColorItemSource.Add(new PaletteEditorColorItemViewModel(
                                               new SolidColorBrush(Color.FromRgb(pColor.Red,
                                               pColor.Green, pColor.Blue))));
                                        });
                                    });
                                }

                                if (it.Event.HasFlag(COLOR_CHANGED) && PaletteColorItemSource != null)
                                {
                                    PaletteColorItemSource[(int)it.ColorChangedIndex].ColorBrush.Color = it.NewColor;
                                }
                            });
                        }
                    }
                    break;
            }
        }

    }
}
