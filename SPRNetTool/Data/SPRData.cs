using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SPRNetTool.Data
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct US_SprFileHead
    {
        fixed byte VersionInfo[4];
        public short GlobleWidth;
        public short GlobleHeight;
        public short OffX;
        public short OffY;
        public short FrameCounts;
        public short ColourCounts;
        public short DirectionCount;
        public short Interval;
        fixed byte Reserved[12];

        public string GetVersionInfoStr()
        {
            char[] versionCharArray = new char[4];
            for (int i = 0; i < 4; i++)
            {
                versionCharArray[i] = (char)VersionInfo[i];
            }
            string versionString = new string(versionCharArray);
            return versionString;
        }

        public byte[] GetVersionInfo()
        {
            byte[] versionCharArray = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                versionCharArray[i] = (byte)VersionInfo[i];
            }
            return versionCharArray;
        }

        public byte[] GetReserved()
        {
            byte[] reserved = new byte[12];
            for (int i = 0; i < 12; i++)
            {
                reserved[i] = (byte)Reserved[i];
            }
            return reserved;
        }
    }

    public struct SprFileHead
    {
        public byte[] VersionInfo;
        public short GlobleWidth;
        public short GlobleHeight;
        public short OffX;
        public short OffY;
        public short FrameCounts;
        public short ColourCounts;
        public short DirectionCount;
        public short Interval;
        public byte[] Reserved;

        public SprFileHead(byte[] versionInfo,
            short globleWidth,
            short globleHeight,
            short offX,
            short offY,
            short frameCounts,
            short colourCounts,
            short directionCount,
            short interval,
            byte[] reserved)
        {
            VersionInfo = versionInfo;
            GlobleWidth = globleWidth;
            GlobleHeight = globleHeight;
            OffX = offX;
            OffY = offY;
            FrameCounts = frameCounts;
            ColourCounts = colourCounts;
            DirectionCount = directionCount;
            Interval = interval;
            Reserved = reserved;
        }
    }

    public struct PaletteColour
    {
        public byte Blue = 0x00;
        public byte Green = 0x00;
        public byte Red = 0x00;
        public byte Alpha = 0x00;

        public PaletteColour(byte blue, byte green, byte red, byte alpha)
        {
            Blue = blue;
            Green = green;
            Red = red;
            Alpha = alpha;
        }
    }
    public struct Palette
    {
        public int Size;//Palette_Colour counts, must less than 257
        public PaletteColour[] Data;

        public Palette(int size)
        {
            Size = size;
            Data = new PaletteColour[256];
        }
    };

    public struct FrameOffsetInfo
    {
        public int FrameOffset;
        public int DataLenght;
    }

    public struct FrameInfo
    {
        public short Width;
        public short Height;
        public short OffX;
        public short OffY;
    }

    public struct FRAMERGBA
    {
        public int frameWidth;
        public int frameHeight;
        public int frameOffX;
        public int frameOffY;

        public PaletteColour[] decodedFrameData;
        public PaletteColour[] globleFrameData;
    }

}
