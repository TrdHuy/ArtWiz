﻿using ArtWiz.Utils;
using ArtWiz.ViewModel.Widgets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArtWiz.View.Widgets
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

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
               "ViewModel",
               typeof(IPaletteEditorViewModel),
               typeof(PaletteEditor),
               new FrameworkPropertyMetadata(default(IPaletteEditorViewModel),
                   FrameworkPropertyMetadataOptions.AffectsMeasure, propertyChangedCallback: OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public IPaletteEditorViewModel? ViewModel
        {
            get { return GetValue(ViewModelProperty) as IPaletteEditorViewModel; }
            set { SetValue(ViewModelProperty, value); }
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

        private event PaletteEditorHandler? mPreviewColorItemChange;

        public PaletteEditor()
        {
            InitializeComponent();
            InternalContainer.SelectedItemChanged += InternalContainer_SelectedItemChanged;
        }

        private void InternalContainer_SelectedItemChanged(object obj)
        {
            if (obj is IPaletteEditorColorItemViewModel vm)
            {
                RedSlider.ValueChanged -= OnValueChangedBySliding;
                GreenSlider.ValueChanged -= OnValueChangedBySliding;
                BlueSlider.ValueChanged -= OnValueChangedBySliding;

                RedSlider.Value = vm.ColorBrush.Color.R;
                GreenSlider.Value = vm.ColorBrush.Color.G;
                BlueSlider.Value = vm.ColorBrush.Color.B;

                RedSlider.ValueChanged += OnValueChangedBySliding;
                GreenSlider.ValueChanged += OnValueChangedBySliding;
                BlueSlider.ValueChanged += OnValueChangedBySliding;
            }
        }

        private void OnValueChangedBySliding(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            InternalContainer.SelectedColorItem?.Apply(it =>
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

    public class PaletteEditorInternal : UserControl
    {
        public event Action<object>? SourceChanged;
        public event Action<object>? SelectedItemChanged;

        public static readonly DependencyProperty SelectedColorItemProperty =
           DependencyProperty.Register(
               "SelectedColorItem",
               typeof(IPaletteEditorColorItemViewModel),
               typeof(PaletteEditorInternal),
           new PropertyMetadata(defaultValue: default(IPaletteEditorColorItemViewModel),
               propertyChangedCallback: OnPropChanged));

        public IPaletteEditorColorItemViewModel SelectedColorItem
        {
            get { return (IPaletteEditorColorItemViewModel)GetValue(SelectedColorItemProperty); }
            set { SetValue(SelectedColorItemProperty, value); }
        }

        public static readonly DependencyProperty ColorItemSourceProperty =
           DependencyProperty.Register(
               "ColorItemSource",
               typeof(IEnumerable<IPaletteEditorColorItemViewModel>),
               typeof(PaletteEditorInternal),
           new FrameworkPropertyMetadata(defaultValue: default(IEnumerable<IPaletteEditorColorItemViewModel>),
               propertyChangedCallback: OnPropChanged));

        private static void OnPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == SelectedColorItemProperty)
            {
                d.IfIs<PaletteEditorInternal>(it => it.SelectedItemChanged?.Invoke(e.NewValue));
            }
            else if (e.Property == ColorItemSourceProperty)
            {
                d.IfIs<PaletteEditorInternal>(it => it.SourceChanged?.Invoke(e.NewValue));
            }
        }

        public IEnumerable ColorItemSource
        {
            get { return (IEnumerable)GetValue(ColorItemSourceProperty); }
            set { SetValue(ColorItemSourceProperty, value); }
        }

        public PaletteEditorInternal()
        {
        }
    }
}
