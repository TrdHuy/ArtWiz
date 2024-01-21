using ArtWiz.View.Utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ArtWiz.View.Widgets
{
    /// <summary>
    /// Interaction logic for ParamEditor.xaml
    /// </summary>
    public partial class ParamEditor : UserControl
    {
        public event RoutedEventHandler? MinusClick;
        public event RoutedEventHandler? PlusClick;
        public event TextContentUpdatedHandler? PreviewTextContentUpdated;
        public event MouseHoldEventHandler? MinusMouseHold;
        public event MouseHoldEventHandler? PlusMouseHold;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(int),
                typeof(ParamEditor),
                new PropertyMetadata(0));

        public int Value
        {
            get { return Convert.ToInt32(GetValue(ValueProperty)); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register(
                "Max",
                typeof(int),
                typeof(ParamEditor),
                new PropertyMetadata(0));

        public int Max
        {
            get { return Convert.ToInt32(GetValue(MaxProperty)); }
            set { SetValue(MaxProperty, value); }
        }

        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register(
                "Min",
                typeof(int),
                typeof(ParamEditor),
                new PropertyMetadata(0));

        public int Min
        {
            get { return Convert.ToInt32(GetValue(MinProperty)); }
            set { SetValue(MinProperty, value); }
        }

        public ParamEditor()
        {
            InitializeComponent();
        }

        private void OnMinusClick(object sender, RoutedEventArgs e)
        {
            MinusClick?.Invoke(this, e);
        }

        private void OnPlusClick(object sender, RoutedEventArgs e)
        {
            PlusClick?.Invoke(this, e);
        }

        private void OnBalloonBoxPreviewTextContentUpdated(object sender, TextContentUpdatedEventArgs e)
        {
            PreviewTextContentUpdated?.Invoke(this, e);
        }


        private void OnPlusMouseHold(object sender, MouseHoldEventArgs args)
        {
            PlusMouseHold?.Invoke(this, args);
        }

        private void OnMinusMouseHold(object sender, MouseHoldEventArgs args)
        {
            MinusMouseHold?.Invoke(this, args);
        }
    }
}
