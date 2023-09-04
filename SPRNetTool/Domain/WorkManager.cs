using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
using SPRNetTool.Utils;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace SPRNetTool.Domain
{
    public class WorkManager : BaseDomain
    {
        public SprFileHead FileHead;
        public Palette PaletteData = new Palette();
        public long FrameDataBegPos = -1;
        public FRAMERGBA[]? FrameData;

        public void Init()
        {
            FrameData = null;
            FrameDataBegPos = -1;
            FileHead = new SprFileHead();
            PaletteData = new Palette();
        }


        public void InitFromFileHead(US_SprFileHead us_fileHead)
        {
            FileHead = new SprFileHead(us_fileHead.GetVersionInfo(),
                us_fileHead.GlobleWidth,
                us_fileHead.GlobleHeight,
                us_fileHead.OffX,
                us_fileHead.OffY, us_fileHead.FrameCounts,
                us_fileHead.ColourCounts,
                us_fileHead.DirectionCount,
                us_fileHead.Interval,
                us_fileHead.GetReserved());
            FrameData = new FRAMERGBA[us_fileHead.FrameCounts];
        }

        public void InitFrameData(FileStream fs)
        {
            if (FrameData == null) return;
            for (int i = 0; i < FileHead.FrameCounts; i++)
            {
                var decodedFrameData = InitDecodedFrameData(fs, i, out int frameWidth,
                        out int frameHeight, COLORMODE.RGBA, out int frameOffX,
                        out int frameOffY);

                if (decodedFrameData == null) throw new Exception("Failed to init decoded frame data!");

                FrameData[i].frameHeight = frameHeight;
                FrameData[i].frameWidth = frameWidth;
                FrameData[i].frameOffY = frameOffY;
                FrameData[i].frameOffX = frameOffX;
                FrameData[i].decodedFrameData = decodedFrameData;
                var globalData = InitGlobalizedFrameData(i);
                if (globalData == null) throw new Exception("Failed to init global frame data!");
                FrameData[i].globleFrameData = globalData;
            }
        }

        private PaletteColour[]? InitDecodedFrameData(FileStream fs,
            int index,
            out int frameWidth,
            out int frameHeight,
            COLORMODE mod,
            out int frameOffX,
            out int frameOffY)
        {
            frameWidth = frameHeight = frameOffX = frameOffY = 0;
            if (index > FileHead.FrameCounts || FrameDataBegPos == -1 || FrameData == null)
            {
                return null;
            }
            switch (mod)
            {
                case COLORMODE.RGB:
                case COLORMODE.RGBA:
                case COLORMODE.BGRA:
                    {
                        break;
                    }
                default: { return null; }

            }
            PaletteColour transcol = new PaletteColour(0X00, 0X00, 0X00, 0X00);

            var frameOffsetInfo = fs.BinToStruct<FrameOffsetInfo>(FrameDataBegPos + Marshal.SizeOf(typeof(FrameOffsetInfo)) * index);

            var frameBeginPos = FrameDataBegPos + (frameOffsetInfo?.FrameOffset ?? 0) + Marshal.SizeOf(typeof(FrameOffsetInfo)) * FileHead.FrameCounts;
            var datalength = frameOffsetInfo?.DataLenght ?? 0;

            if (datalength == 0)
            {
                return null;
            }

            var frameInfo = fs.BinToStruct<FrameInfo>(frameBeginPos);
            long decdatalength = (frameInfo?.Width * frameInfo?.Height) ?? 0;
            frameWidth = frameInfo?.Width ?? 0;
            frameHeight = frameInfo?.Height ?? 0;
            frameOffX = frameInfo?.OffX ?? 0;
            frameOffY = frameInfo?.OffY ?? 0;
            if (decdatalength == 0)
            {
                return null;
            }

            PaletteColour[] decData = new PaletteColour[decdatalength];
            var frameDataPos = frameBeginPos + Marshal.SizeOf(typeof(FrameInfo));
            fs.Position = frameDataPos;
            long curdecposition = 0;
            for (long i = 0; i < datalength - 8;)
            {
                if (curdecposition > decdatalength)
                {
                    return null;
                }
                int size = fs.ReadByte();
                int alpha = fs.ReadByte();
                if (size == -1 || alpha == -1)
                {
                    return null;
                }

                if (alpha == 0x00)
                {
                    for (int j = 0; j < size; j++)
                    {
                        decData[curdecposition].Red = transcol.Red;
                        decData[curdecposition].Blue = transcol.Blue;
                        decData[curdecposition].Green = transcol.Green;
                        decData[curdecposition].Alpha = transcol.Alpha;
                        curdecposition++;
                    }
                }
                else
                {
                    for (int j = 0; j < size; j++)
                    {
                        int colorIndex = (int)fs.ReadByte();
                        if (colorIndex == -1)
                        {
                            return null;
                        }
                        i++;
                        decData[curdecposition].Red = PaletteData.Data[colorIndex].Red;
                        decData[curdecposition].Blue = PaletteData.Data[colorIndex].Blue;
                        decData[curdecposition].Green = PaletteData.Data[colorIndex].Green;
                        decData[curdecposition].Alpha = (byte)alpha;
                        curdecposition++;
                    }
                }
                i += 2;
            }
            return decData;
        }

        private PaletteColour[]? InitGlobalizedFrameData(int index)
        {
            BitmapSource v;
            var decodedFrameData = FrameData?[index].decodedFrameData;
            if (decodedFrameData == null)
            {
                return null;
            }

            long globalDataLen = FileHead.GlobleHeight * FileHead.GlobleWidth;
            PaletteColour[] globalData = new PaletteColour[globalDataLen];
            long frameOffX = FrameData?[index].frameOffX ?? 0;
            long frameOffY = FrameData?[index].frameOffY ?? 0;
            long frameHeight = FrameData?[index].frameHeight ?? 0;
            long frameWidth = FrameData?[index].frameWidth ?? 0;

            for (long datidx = 0; datidx < (long)globalDataLen; datidx++)
            {
                globalData[datidx].Red = 0xFF;
                globalData[datidx].Green = 0xFF;
                globalData[datidx].Blue = 0xFF;
                globalData[datidx].Alpha = 0xFF;
            }

            for (int hi = 0; hi < FileHead.GlobleHeight; hi++)
            {
                for (int wi = 0; wi < FileHead.GlobleWidth; wi++)
                {
                    long offwidth = wi + frameOffX;
                    long offheight = hi + frameOffY;

                    if (hi < frameHeight && wi < frameWidth &&
                        offwidth >= 0 && offwidth < FileHead.GlobleWidth &&
                        offheight >= 0 && offheight < FileHead.GlobleHeight)
                    {
                        globalData[offheight * FileHead.GlobleWidth + offwidth].Red = decodedFrameData[hi * frameWidth + wi].Red;
                        globalData[offheight * FileHead.GlobleWidth + offwidth].Green = decodedFrameData[hi * frameWidth + wi].Green;
                        globalData[offheight * FileHead.GlobleWidth + offwidth].Blue = decodedFrameData[hi * frameWidth + wi].Blue;
                        globalData[offheight * FileHead.GlobleWidth + offwidth].Alpha = decodedFrameData[hi * frameWidth + wi].Alpha;
                    }
                }
            }
            return globalData;
        }
    }

    public enum COLORMODE
    {
        RGB, RGBA, BGRA
    }
}
