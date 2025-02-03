using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace ArtWiz.View.Utils
{
    public static class BorderClipHelper
    {
        public static readonly DependencyProperty IsUseClipGeometryProperty =
            DependencyProperty.RegisterAttached(
                "IsUseClipGeometry",
                typeof(bool),
                typeof(BorderClipHelper),
                new PropertyMetadata(false, OnIsUseClipGeometryChanged));

        public static bool GetIsUseClipGeometry(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsUseClipGeometryProperty);
        }

        public static void SetIsUseClipGeometry(DependencyObject obj, bool value)
        {
            obj.SetValue(IsUseClipGeometryProperty, value);
        }

        private static void OnIsUseClipGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border )
            {
                if((bool)e.NewValue)
                {
                    border.Loaded += (s, ev) => ApplyClip(border);
                    border.SizeChanged += (s, ev) => ApplyClip(border); // Đảm bảo cập nhật clip khi kích thước thay đổi
                }
                else
                {
                    border.Loaded -= (s, ev) => ApplyClip(border);
                    border.SizeChanged -= (s, ev) => ApplyClip(border); // Đảm bảo cập nhật clip khi kích thước thay đổi

                }

            }
          
        }

        private static void ApplyClip(Border border)
        {
            double width = border.ActualWidth;
            double height = border.ActualHeight;

            // Nếu kích thước chưa có giá trị hợp lệ, không áp dụng Clip
            if (width <= 0 || height <= 0) return;

            double borderThickness = Math.Max(border.BorderThickness.Left, border.BorderThickness.Top);
            double cornerRadius = Math.Max(0, border.CornerRadius.TopLeft - (borderThickness / 2));

            double clipWidth = width - borderThickness;
            double clipHeight = height - borderThickness;

            border.Clip = new RectangleGeometry
            {
                Rect = new Rect(borderThickness / 2, borderThickness / 2, clipWidth, clipHeight),
                RadiusX = cornerRadius,
                RadiusY = cornerRadius
            };
        }
    }
}
