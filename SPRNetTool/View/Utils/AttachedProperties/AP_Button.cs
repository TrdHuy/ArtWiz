using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ArtWiz.View.Utils.AttachedProperties
{
    internal static partial class AttachedProperites
    {
        #region HoverBackground
        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "HoverBackground",
                typeof(Brush),
                typeof(AttachedProperites),
                new PropertyMetadata(null, OnHoverBackgroundChanged));

        public static Brush GetHoverBackground(Button button) => (Brush)button.GetValue(HoverBackgroundProperty);
        public static void SetHoverBackground(Button button, Brush value) => button.SetValue(HoverBackgroundProperty, value);

        private static void OnHoverBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                // Xóa event handler cũ trước khi gán mới
                button.MouseEnter -= Button_HoverBackground_MouseEnter;
                button.MouseLeave -= Button_HoverBackground_MouseLeave;

                if (e.NewValue is Brush)
                {
                    button.MouseEnter += Button_HoverBackground_MouseEnter;
                    button.MouseLeave += Button_HoverBackground_MouseLeave;
                }
            }
        }

        private static void Button_HoverBackground_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = GetHoverBackground(button);
            }
        }

        private static void Button_HoverBackground_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.ClearValue(Control.BackgroundProperty);
            }
        }
        #endregion

        #region ClickBackground
        public static readonly DependencyProperty ClickBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "ClickBackground",
                typeof(Brush),
                typeof(AttachedProperites),
                new PropertyMetadata(null, OnClickBackgroundChanged));

        public static Brush GetClickBackground(Button button) => (Brush)button.GetValue(ClickBackgroundProperty);
        public static void SetClickBackground(Button button, Brush value) => button.SetValue(ClickBackgroundProperty, value);

        private static void OnClickBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                // Xóa event handler cũ trước khi gán mới
                button.PreviewMouseDown -= Button_ClickBackground_MouseDown;
                button.PreviewMouseUp -= Button_ClickBackground_MouseUp;

                if (e.NewValue is Brush)
                {
                    button.PreviewMouseDown += Button_ClickBackground_MouseDown;
                    button.PreviewMouseUp += Button_ClickBackground_MouseUp;
                }
            }
        }

        private static void Button_ClickBackground_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = GetClickBackground(button);
            }
        }

        private static void Button_ClickBackground_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                button.ClearValue(Control.BackgroundProperty);
            }
        }
        #endregion
    }

}


