using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using ArtWiz.Utils;

namespace ArtWiz.View.Widgets
{
    internal class ClippingBorder : Border
    {
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);
            Rect rect = new Rect(finalSize);
            Rect rect2 = HelperDeflateRect(rect, BorderThickness);
            Radii radii = new Radii(CornerRadius, BorderThickness, outer: false);
            if (!DoubleUtil.IsZero(rect2.Width) && !DoubleUtil.IsZero(rect2.Height))
            {
                var clip = GenerateGeometry(rect2, radii);
                if (Child != null)
                {
                    Child.Clip = clip;
                }
            }

            return size;
        }
        private struct Radii
        {
            internal double LeftTop;

            internal double TopLeft;

            internal double TopRight;

            internal double RightTop;

            internal double RightBottom;

            internal double BottomRight;

            internal double BottomLeft;

            internal double LeftBottom;

            internal Radii(CornerRadius radii, Thickness borders, bool outer)
            {
                double num = 0.5 * borders.Left;
                double num2 = 0.5 * borders.Top;
                double num3 = 0.5 * borders.Right;
                double num4 = 0.5 * borders.Bottom;
                if (outer)
                {
                    if (DoubleUtil.IsZero(radii.TopLeft))
                    {
                        LeftTop = (TopLeft = 0.0);
                    }
                    else
                    {
                        LeftTop = radii.TopLeft + num;
                        TopLeft = radii.TopLeft + num2;
                    }
                    if (DoubleUtil.IsZero(radii.TopRight))
                    {
                        TopRight = (RightTop = 0.0);
                    }
                    else
                    {
                        TopRight = radii.TopRight + num2;
                        RightTop = radii.TopRight + num3;
                    }
                    if (DoubleUtil.IsZero(radii.BottomRight))
                    {
                        RightBottom = (BottomRight = 0.0);
                    }
                    else
                    {
                        RightBottom = radii.BottomRight + num3;
                        BottomRight = radii.BottomRight + num4;
                    }
                    if (DoubleUtil.IsZero(radii.BottomLeft))
                    {
                        BottomLeft = (LeftBottom = 0.0);
                        return;
                    }
                    BottomLeft = radii.BottomLeft + num4;
                    LeftBottom = radii.BottomLeft + num;
                }
                else
                {
                    LeftTop = Math.Max(0.0, radii.TopLeft - num);
                    TopLeft = Math.Max(0.0, radii.TopLeft - num2);
                    TopRight = Math.Max(0.0, radii.TopRight - num2);
                    RightTop = Math.Max(0.0, radii.TopRight - num3);
                    RightBottom = Math.Max(0.0, radii.BottomRight - num3);
                    BottomRight = Math.Max(0.0, radii.BottomRight - num4);
                    BottomLeft = Math.Max(0.0, radii.BottomLeft - num4);
                    LeftBottom = Math.Max(0.0, radii.BottomLeft - num);
                }
            }
        }

        private static Rect HelperDeflateRect(Rect rt, Thickness thick)
        {
            return new Rect(0, 0, Math.Max(0.0, rt.Width - thick.Left - thick.Right), Math.Max(0.0, rt.Height - thick.Top - thick.Bottom));
        }

        private static Geometry GenerateGeometry(Rect rect, Radii radii)
        {
            Point point = new Point(radii.LeftTop, 0.0);
            Point point2 = new Point(rect.Width - radii.RightTop, 0.0);
            Point point3 = new Point(rect.Width, radii.TopRight);
            Point point4 = new Point(rect.Width, rect.Height - radii.BottomRight);
            Point point5 = new Point(rect.Width - radii.RightBottom, rect.Height);
            Point point6 = new Point(radii.LeftBottom, rect.Height);
            Point point7 = new Point(0.0, rect.Height - radii.BottomLeft);
            Point point8 = new Point(0.0, radii.TopLeft);
            if (point.X > point2.X)
            {
                double x = (point.X = radii.LeftTop / (radii.LeftTop + radii.RightTop) * rect.Width);
                point2.X = x;
            }
            if (point3.Y > point4.Y)
            {
                double y = (point3.Y = radii.TopRight / (radii.TopRight + radii.BottomRight) * rect.Height);
                point4.Y = y;
            }
            if (point5.X < point6.X)
            {
                double x2 = (point5.X = radii.LeftBottom / (radii.LeftBottom + radii.RightBottom) * rect.Width);
                point6.X = x2;
            }
            if (point7.Y < point8.Y)
            {
                double y2 = (point7.Y = radii.TopLeft / (radii.TopLeft + radii.BottomLeft) * rect.Height);
                point8.Y = y2;
            }
            Vector vector = new Vector(rect.TopLeft.X, rect.TopLeft.Y);
            point += vector;
            point2 += vector;
            point3 += vector;
            point4 += vector;
            point5 += vector;
            point6 += vector;
            point7 += vector;
            point8 += vector;
            string pathData = $"M {point.X} {point.Y} " +
                   $"L {point2.X} {point2.Y} ";
            double num5 = rect.TopRight.X - point2.X;
            double num6 = point3.Y - rect.TopRight.Y;
            if (!DoubleUtil.IsZero(num5) || !DoubleUtil.IsZero(num6))
            {
                pathData += $"A {num5} {num6} 0 0 1 {point3.X} {point3.Y} ";
            }
            pathData += $"L {point4.X} {point4.Y} ";
            num5 = rect.BottomRight.X - point5.X;
            num6 = rect.BottomRight.Y - point4.Y;
            if (!DoubleUtil.IsZero(num5) || !DoubleUtil.IsZero(num6))
            {
                pathData += $"A {num5} {num6} 0 0 1 {point5.X} {point5.Y} ";
            }
            pathData += $"L {point6.X} {point6.Y} ";
            num5 = point6.X - rect.BottomLeft.X;
            num6 = rect.BottomLeft.Y - point7.Y;
            if (!DoubleUtil.IsZero(num5) || !DoubleUtil.IsZero(num6))
            {
                pathData += $"A {num5} {num6} 0 0 1 {point7.X} {point7.Y} ";
            }
            pathData += $"L {point8.X} {point8.Y} ";
            num5 = point.X - rect.TopLeft.X;
            num6 = point8.Y - rect.TopLeft.Y;
            if (!DoubleUtil.IsZero(num5) || !DoubleUtil.IsZero(num6))
            {
                pathData += $"A {num5} {num6} 0 0 1 {point.X} {point.Y} ";
            }
            pathData += "Z";
            return Geometry.Parse(pathData);
        }

    }

}
