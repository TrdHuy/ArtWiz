using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ArtWiz.View.Utils.AttachedProperties
{
    internal static partial class AttachedProperites
    {

        #region HoverBackground
        public static readonly DependencyProperty OnButtonHoverForegroundProperty =
        DependencyProperty.RegisterAttached(
            "OnButtonHoverForeground",
            typeof(Brush),
            typeof(AttachedProperites),
            new PropertyMetadata(null, OnButtonHoverForegroundChanged));

        public static Brush GetOnButtonHoverForeground(Button button) => (Brush)button.GetValue(OnButtonHoverForegroundProperty);
        public static void SetOnButtonHoverForeground(Button button, Brush value) => button.SetValue(OnButtonHoverForegroundProperty, value);

        private static void OnButtonHoverForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                button.Initialized -= Common_Button_Initialized;
                button.Initialized += Common_Button_Initialized;
                SetButtonCache(button, IS_OVERIDE_HOVER_BACKGROUND_KEY, true);

                button.MouseEnter -= Button_HoverForeground_MouseEnter;
                button.MouseLeave -= Button_HoverForeground_MouseLeave;

                if (e.NewValue is Brush)
                {
                    button.MouseEnter += Button_HoverForeground_MouseEnter;
                    button.MouseLeave += Button_HoverForeground_MouseLeave;
                }
            }
        }

        private static void Button_HoverForeground_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (GetButtonCache(button, ORGINAL_FOREGROUND_KEY) == null)
                {
                    SetButtonCache(button, ORGINAL_FOREGROUND_KEY, button.Foreground);
                }
                button.Foreground = GetOnButtonHoverForeground(button);
            }
        }

        private static void Button_HoverForeground_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.SetValue(Button.ForegroundProperty, GetButtonCache(button, ORGINAL_FOREGROUND_KEY));
            }
        }
        #endregion


        #region HoverBackground
        public static readonly DependencyProperty OnButtonHoverBackgroundProperty =
        DependencyProperty.RegisterAttached(
            "OnButtonHoverBackground",
            typeof(Brush),
            typeof(AttachedProperites),
            new PropertyMetadata(null, OnButtonHoverBackgroundChanged));

        public static Brush GetOnButtonHoverBackground(Button button) => (Brush)button.GetValue(OnButtonHoverBackgroundProperty);
        public static void SetOnButtonHoverBackground(Button button, Brush value) => button.SetValue(OnButtonHoverBackgroundProperty, value);

        private static void OnButtonHoverBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                button.Initialized -= Common_Button_Initialized;
                button.Initialized += Common_Button_Initialized;
                SetButtonCache(button, IS_OVERIDE_HOVER_BACKGROUND_KEY, true);
                
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
                if (GetButtonCache(button, ORGINAL_BACKGROUND_KEY) == null)
                {
                    SetButtonCache(button, ORGINAL_BACKGROUND_KEY, button.Background);
                }
                button.Background = GetOnButtonHoverBackground(button);
            }
        }

        private static void Button_HoverBackground_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.SetValue(Button.BackgroundProperty, GetButtonCache(button, ORGINAL_BACKGROUND_KEY));
            }
        }
        #endregion

        #region OnButtonPressedBackground
        public static readonly DependencyProperty OnButtonPressedBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "OnButtonPressedBackground",
                typeof(Brush),
                typeof(AttachedProperites),
                new PropertyMetadata(null, OnButtonPressedBackgroundChanged));

        public static Brush GetOnButtonPressedBackground(Button button) => (Brush)button.GetValue(OnButtonPressedBackgroundProperty);
        public static void SetOnButtonPressedBackground(Button button, Brush value) => button.SetValue(OnButtonPressedBackgroundProperty, value);

        private static void OnButtonPressedBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                button.Initialized -= Common_Button_Initialized;
                button.Initialized += Common_Button_Initialized;
                SetButtonCache(button, IS_OVERIDE_PRESSED_BACKGROUND_KEY, true);
                
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
                if (GetButtonCache(button, ORGINAL_BACKGROUND_KEY) == null)
                {
                    SetButtonCache(button, ORGINAL_BACKGROUND_KEY, button.Background);
                }
                button.Background = GetOnButtonPressedBackground(button);
            }
        }

        private static void Button_ClickBackground_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                button.SetValue(Button.BackgroundProperty, GetButtonCache(button, ORGINAL_BACKGROUND_KEY));
            }
        }
        #endregion

        #region Common
        private static void Common_Button_Initialized(object? sender, System.EventArgs e)
        {
            if (sender is Button button)
            {
                button.Template = CreateDefaultWpfButtonTemplate(
                    isOverideHoverBg: (bool)(GetButtonCache(button, IS_OVERIDE_PRESSED_BACKGROUND_KEY) ?? false),
                    isOverideOnPressedBg: (bool)(GetButtonCache(button, IS_OVERIDE_PRESSED_BACKGROUND_KEY) ?? false)
                    );
            }
        }
        #endregion

        #region Utils
        private const string ORGINAL_BACKGROUND_KEY = "ORGINAL_BACKGROUND_KEY";
        private const string ORGINAL_FOREGROUND_KEY = "ORGINAL_FOREGROUND_KEY";
        private const string IS_OVERIDE_HOVER_BACKGROUND_KEY = "IS_OVERIDE_HOVER_BACKGROUND_KEY";
        private const string IS_OVERIDE_PRESSED_BACKGROUND_KEY = "IS_OVERIDE_PRESSED_BACKGROUND_KEY";
        private const string IS_OVERIDE_HOVER_FOREGROUND_KEY = "IS_OVERIDE_HOVER_FOREGROUND_KEY";

        private static readonly Dictionary<Button, Dictionary<string, object>> buttonCache = new();
        private static void SetButtonCache(Button button, string key, object value)
        {
            if (!buttonCache.ContainsKey(button))
            {
                buttonCache[button] = new Dictionary<string, object>();
                button.Unloaded += (s, e) =>
                {
                    buttonCache.Remove(button);
                };
            }
            buttonCache[button][key] = value;
        }

        private static object? GetButtonCache(Button button, string key)
        {
            return buttonCache.ContainsKey(button) && buttonCache[button].ContainsKey(key)
                ? buttonCache[button][key]
                : null;
        }

        private static ControlTemplate CreateDefaultWpfButtonTemplate(bool isOverideOnPressedBg, bool isOverideHoverBg)
        {
            Trigger CreateTrigger(DependencyProperty property, object value, DependencyProperty targetProperty, object setValue, string targetName = null)
            {
                Trigger trigger = new Trigger { Property = property, Value = value };
                Setter setter = new Setter(targetProperty, setValue);
                if (!string.IsNullOrEmpty(targetName))
                {
                    setter.TargetName = targetName;
                }
                trigger.Setters.Add(setter);
                return trigger;
            }

            ControlTemplate template = new ControlTemplate(typeof(ButtonBase));

            // Define resources
            var mouseOverBackground = new SolidColorBrush(Color.FromRgb(190, 230, 253));
            var mouseOverBorder = new SolidColorBrush(Color.FromRgb(60, 127, 177));
            var pressedBackground = new SolidColorBrush(Color.FromRgb(196, 229, 246));
            var pressedBorder = new SolidColorBrush(Color.FromRgb(44, 98, 139));
            var disabledBackground = new SolidColorBrush(Color.FromRgb(244, 244, 244));
            var disabledBorder = new SolidColorBrush(Color.FromRgb(173, 178, 181));
            var disabledForeground = new SolidColorBrush(Color.FromRgb(131, 131, 131));

            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.Name = "border";
            border.SetBinding(Border.BackgroundProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Path = new PropertyPath(Control.BackgroundProperty) });
            border.SetBinding(Border.BorderBrushProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Path = new PropertyPath(Control.BorderBrushProperty) });
            border.SetBinding(Border.BorderThicknessProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Path = new PropertyPath(Control.BorderThicknessProperty) });
            border.SetValue(Border.SnapsToDevicePixelsProperty, true);

            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.Name = "contentPresenter";
            contentPresenter.SetValue(ContentPresenter.FocusableProperty, false);
            contentPresenter.SetBinding(ContentPresenter.HorizontalAlignmentProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Path = new PropertyPath(Control.HorizontalContentAlignmentProperty) });
            contentPresenter.SetBinding(ContentPresenter.MarginProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Path = new PropertyPath(Control.PaddingProperty) });
            contentPresenter.SetBinding(ContentPresenter.SnapsToDevicePixelsProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Path = new PropertyPath(Control.SnapsToDevicePixelsProperty) });
            contentPresenter.SetBinding(ContentPresenter.VerticalAlignmentProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Path = new PropertyPath(Control.VerticalContentAlignmentProperty) });

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            // Define triggers
            template.Triggers.Add(CreateTrigger(Button.IsDefaultedProperty, true, Border.BorderBrushProperty, SystemColors.HighlightBrush));
            if (!isOverideOnPressedBg)
            {
                template.Triggers.Add(CreateTrigger(Button.IsPressedProperty, true, Border.BackgroundProperty, pressedBackground, "border"));
                template.Triggers.Add(CreateTrigger(Button.IsPressedProperty, true, Border.BorderBrushProperty, pressedBorder, "border"));
            }

            if (!isOverideHoverBg)
            {
                template.Triggers.Add(CreateTrigger(Button.IsMouseOverProperty, true, Border.BackgroundProperty, mouseOverBackground, "border"));
                template.Triggers.Add(CreateTrigger(Button.IsMouseOverProperty, true, Border.BorderBrushProperty, mouseOverBorder, "border"));
            }
            template.Triggers.Add(CreateTrigger(Button.IsEnabledProperty, false, Border.BackgroundProperty, disabledBackground, "border"));
            template.Triggers.Add(CreateTrigger(Button.IsEnabledProperty, false, Border.BorderBrushProperty, disabledBorder, "border"));
            template.Triggers.Add(CreateTrigger(Button.IsEnabledProperty, false, TextElement.ForegroundProperty, disabledForeground, "contentPresenter"));

            return template;
        }
        #endregion
    }

}


