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

            public PaletteEditorEventChangedArgs(IPaletteEditorColorItemViewModel? item,
                Color oldColor,
                Color newColor)
            {
                Item = item;
                OldColor = oldColor;
                NewColor = newColor;
            }
        }

        public delegate void PaletteEditorHandler(object sender, PaletteEditorEventChangedArgs arg);
        public static void AddColorItemChangedHandler(DependencyObject element, PaletteEditorHandler handler)
        {
            element.IfIs<PaletteEditor>(it =>
            {
                it.mColorItemChanged += handler;
            });
        }

        public static void RemoveColorItemChangedHandler(DependencyObject element, PaletteEditorHandler handler)
        {
            element.IfIs<PaletteEditor>(it =>
            {
                it.mColorItemChanged -= handler;
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

        private event PaletteEditorHandler? mColorItemChanged;
        private IPaletteEditorColorItemViewModel? mSelectedItem;
        public IEnumerable ColorItemSource
        {
            get { return (IEnumerable)GetValue(ColorItemSourceProperty); }
            set { SetValue(ColorItemSourceProperty, value); }
        }

        public PaletteEditor()
        {
            InitializeComponent();
        }

        private void ColorsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.AddedItems?[0]?.IfIs<IPaletteEditorColorItemViewModel>(it =>
            {
                SetSelectedColorItem(it);
            });
        }

        private void SetSelectedColorItem(IPaletteEditorColorItemViewModel selectedItem)
        {
            RedSlider.ValueChanged -= OnValueChangedBySliding;
            GreenSlider.ValueChanged -= OnValueChangedBySliding;
            BlueSlider.ValueChanged -= OnValueChangedBySliding;
            mSelectedItem = selectedItem;
            RedSlider.Value = selectedItem.ColorBrush.Color.R;
            GreenSlider.Value = selectedItem.ColorBrush.Color.G;
            BlueSlider.Value = selectedItem.ColorBrush.Color.B;
            SelectedColorView.Background = selectedItem.ColorBrush;
            SelectedColorHexTextView.Text = selectedItem.ColorBrush.Color.ToString();
            SelectedColorHexTextView.Fill = selectedItem.ColorBrush;

            RedSlider.ValueChanged += OnValueChangedBySliding;
            GreenSlider.ValueChanged += OnValueChangedBySliding;
            BlueSlider.ValueChanged += OnValueChangedBySliding;
        }

        private void OnValueChangedBySliding(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mSelectedItem?.Apply(it =>
            {
                var oldColor = it.ColorBrush.Color;
                if (sender == RedSlider)
                {
                    it.ColorBrush.Color = Color.FromRgb((byte)e.NewValue,
                        it.ColorBrush.Color.G,
                        it.ColorBrush.Color.B);
                }
                else if (sender == GreenSlider)
                {
                    it.ColorBrush.Color = Color.FromRgb(it.ColorBrush.Color.R,
                        (byte)e.NewValue,
                        it.ColorBrush.Color.B);
                }
                else if (sender == BlueSlider)
                {
                    it.ColorBrush.Color = Color.FromRgb(it.ColorBrush.Color.R,
                        it.ColorBrush.Color.G,
                        (byte)e.NewValue);
                }
                SelectedColorHexTextView.Text = it.ColorBrush.Color.ToString();
                mColorItemChanged?.Invoke(this, new PaletteEditorEventChangedArgs(it,
                    oldColor,
                    it.ColorBrush.Color));
            });


        }
    }
}
