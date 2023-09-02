using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace SPRNetTool.Domain
{
    public static class Dithering
    {
        public static Bitmap FloydSteinbergDithering(Bitmap sourceImage)
        {
            var listColorPalette = GenerateColorPalette(sourceImage, 256);

            int width = sourceImage.Width;
            int height = sourceImage.Height;

            // Tạo ảnh kết quả với định dạng pixel đầy đủ
            Bitmap resultImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // Duyệt qua từng pixel trong ảnh
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color sourceColor = sourceImage.GetPixel(x, y);
                    Color closestColor = FindClosestPaletteColor(sourceColor, listColorPalette);

                    resultImage.SetPixel(x, y, closestColor);

                    int quantizationErrorR = sourceColor.R - closestColor.R;
                    int quantizationErrorG = sourceColor.G - closestColor.G;
                    int quantizationErrorB = sourceColor.B - closestColor.B;

                    // Phân phối sai số sang các pixel lân cận
                    if (x + 1 < width)
                        PropagateError(resultImage, x + 1, y, quantizationErrorR, quantizationErrorG, quantizationErrorB, 7 / 16.0);
                    if (x - 1 >= 0 && y + 1 < height)
                        PropagateError(resultImage, x - 1, y + 1, quantizationErrorR, quantizationErrorG, quantizationErrorB, 3 / 16.0);
                    if (y + 1 < height)
                        PropagateError(resultImage, x, y + 1, quantizationErrorR, quantizationErrorG, quantizationErrorB, 5 / 16.0);
                    if (x + 1 < width && y + 1 < height)
                        PropagateError(resultImage, x + 1, y + 1, quantizationErrorR, quantizationErrorG, quantizationErrorB, 1 / 16.0);
                }
            }

            return resultImage;
        }

        private static Color FindClosestPaletteColor(Color sourceColor, List<Color> listColorPalette)
        {
            // Tìm màu gần nhất trong bảng màu giới hạn
            int minDistanceSquared = int.MaxValue;
            Color closestColor = Color.Black;

            for (int i = 0; i < 256; i++)
            {
                Color paletteColor = listColorPalette[i];
                int distanceSquared = ColorDistanceSquared(sourceColor, paletteColor);

                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    closestColor = paletteColor;
                }
            }

            return closestColor;
        }

        private static int ColorDistanceSquared(Color a, Color b)
        {
            int deltaR = a.R - b.R;
            int deltaG = a.G - b.G;
            int deltaB = a.B - b.B;

            return deltaR * deltaR + deltaG * deltaG + deltaB * deltaB;
        }

        private static void PropagateError(Bitmap sourceImage, int x, int y, int errorR, int errorG, int errorB, double factor)
        {
            Color pixelColor = sourceImage.GetPixel(x, y);
            int newR = Clamp(pixelColor.R + (int)(errorR * factor));
            int newG = Clamp(pixelColor.G + (int)(errorG * factor));
            int newB = Clamp(pixelColor.B + (int)(errorB * factor));
            Color newColor = Color.FromArgb(newR, newG, newB);
            sourceImage.SetPixel(x, y, newColor);
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        public static List<Color> GenerateColorPalette(Bitmap bitmap, int paletteSize)
        {
            Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

            int width = bitmap.Width;
            int height = bitmap.Height;

            // Duyệt qua từng pixel trong ảnh
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    if (colorCounts.ContainsKey(pixelColor))
                    {
                        colorCounts[pixelColor]++;
                    }
                    else
                    {
                        colorCounts[pixelColor] = 1;
                    }
                }
            }

            // Sắp xếp danh sách màu theo số lần xuất hiện giảm dần
            var sortedColors = colorCounts.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();

            // Chọn ra 256 màu đầu tiên để tạo color palette
            List<Color> colorPalette = sortedColors.Take(paletteSize).ToList();

            return colorPalette;
        }
    }

}

