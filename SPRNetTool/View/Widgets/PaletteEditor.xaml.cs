using SPRNetTool.Utils;
using SPRNetTool.ViewModel.Widgets;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SPRNetTool.View.Widgets
{
    /// <summary>
    /// Interaction logic for PaletteViewer.xaml
    /// </summary>
    public partial class PaletteEditor : UserControl
    {
        public class PaletteEditorEventChangedArgs
        {
            public IPaletteEditorColorItemViewModel? Item { get; private set; }
            public Color OldColor { get; private set; }
            public Color NewColor { get; private set; }
            public int ColorIndex { get; private set; }

            public bool Handled { get; set; }
            public PaletteEditorEventChangedArgs(IPaletteEditorColorItemViewModel? item,
                Color oldColor,
                Color newColor,
                int colorIndex)
            {
                Item = item;
                OldColor = oldColor;
                NewColor = newColor;
                ColorIndex = colorIndex;    
            }
        }

        public delegate void PaletteEditorHandler(object sender, PaletteEditorEventChangedArgs arg);
        public static void AddPreviewColorItemChangeHandler(DependencyObject element, PaletteEditorHandler handler)
        {
            element.IfIs<PaletteEditor>(it =>
            {
                it.mPreviewColorItemChange += handler;
            });
        }

        public static void RemovePreviewColorItemChangeHandler(DependencyObject element, PaletteEditorHandler handler)
        {
            element.IfIs<PaletteEditor>(it =>
            {
                it.mPreviewColorItemChange -= handler;
            });
        }

        public static readonly DependencyProperty ColorItemSourceProperty =
           DependencyProperty.Register(
               "ColorItemSource",
               typeof(IEnumerable<IPaletteEditorColorItemViewModel>),
               typeof(PaletteEditor),
           new FrameworkPropertyMetadata(defaultValue: default(IEnumerable<IPaletteEditorColorItemViewModel>),
               flags: FrameworkPropertyMetadataOptions.AffectsRender,
               new PropertyChangedCallback(OnColorItemSourceChangedCallback)));

        private static void OnColorItemSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.IfIs<PaletteEditor>(it =>
            {
                it.ColorsList.ItemsSource = e.NewValue as IEnumerable;
            });
        }
        public IEnumerable ColorItemSource
        {
            get { return (IEnumerable)GetValue(ColorItemSourceProperty); }
            set { SetValue(ColorItemSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedColorItemProperty =
           DependencyProperty.Register(
               "SelectedColorItem",
               typeof(IPaletteEditorColorItemViewModel),
               typeof(PaletteEditor),
           new PropertyMetadata(defaultValue: default(IPaletteEditorColorItemViewModel)));

        public IPaletteEditorColorItemViewModel SelectedColorItem
        {
            get { return (IPaletteEditorColorItemViewModel)GetValue(SelectedColorItemProperty); }
            private set { SetValue(SelectedColorItemProperty, value); }
        }

        private event PaletteEditorHandler? mPreviewColorItemChange;


        public PaletteEditor()
        {
            InitializeComponent();
        }

        private void ColorsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                e.AddedItems?[0]?.IfIs<IPaletteEditorColorItemViewModel>(it =>
                {
                    SetSelectedColorItem(it);
                });
            }
        }

        private void SetSelectedColorItem(IPaletteEditorColorItemViewModel selectedItem)
        {
            RedSlider.ValueChanged -= OnValueChangedBySliding;
            GreenSlider.ValueChanged -= OnValueChangedBySliding;
            BlueSlider.ValueChanged -= OnValueChangedBySliding;
            SelectedColorItem = selectedItem;
            RedSlider.Value = selectedItem.ColorBrush.Color.R;
            GreenSlider.Value = selectedItem.ColorBrush.Color.G;
            BlueSlider.Value = selectedItem.ColorBrush.Color.B;

            RedSlider.ValueChanged += OnValueChangedBySliding;
            GreenSlider.ValueChanged += OnValueChangedBySliding;
            BlueSlider.ValueChanged += OnValueChangedBySliding;
        }

        private void OnValueChangedBySliding(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SelectedColorItem?.Apply(it =>
            {
                var oldColor = it.ColorBrush.Color;
                var newColor = it.ColorBrush.Color;
                if (sender == RedSlider)
                {
                    newColor = Color.FromRgb((byte)e.NewValue,
                        it.ColorBrush.Color.G,
                        it.ColorBrush.Color.B);
                }
                else if (sender == GreenSlider)
                {
                    newColor = Color.FromRgb(it.ColorBrush.Color.R,
                        (byte)e.NewValue,
                        it.ColorBrush.Color.B);
                }
                else if (sender == BlueSlider)
                {
                    newColor = Color.FromRgb(it.ColorBrush.Color.R,
                        it.ColorBrush.Color.G,
                        (byte)e.NewValue);
                }
                var arg = new PaletteEditorEventChangedArgs(it, oldColor, newColor, ColorsList.SelectedIndex);
                mPreviewColorItemChange?.Invoke(this, arg);
                if (!arg.Handled)
                {
                    it.ColorBrush.Color = newColor;
                }
            });


        }
    }
}
