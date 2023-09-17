using System.Runtime.InteropServices;

namespace SPRNetTool.Data
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct US_SprFileHead
    {
        fixed byte VersionInfo[4];
        public ushort GlobleWidth;
        public ushort GlobleHeight;
        public ushort OffX;
        public ushort OffY;
        public ushort FrameCounts;
        public ushort ColourCounts;
        public ushort DirectionCount;
        public ushort Interval;
        fixed byte Reserved[12];

        public void SetVersionInfoStr(char[] versionCharArray)
        {
            for (int i = 0; i < 4; i++)
            {
                VersionInfo[i] = (byte)versionCharArray[i];
            }
        }

        public void SetReserved(byte[] reserved)
        {
            for (int i = 0; i < 12; i++)
            {
                Reserved[i] = reserved[i];
            }
        }

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
        public byte[] VersionInfo {  get; set; }
        public ushort GlobleWidth { get; set; }
        public ushort GlobleHeight { get; set; }
        public ushort OffX { get; set; }
        public ushort OffY { get; set; }
        public ushort FrameCounts { get; set; }
        public ushort ColourCounts { get; set; }
        public ushort DirectionCount { get; set; }
        public ushort Interval { get; set; }
        public byte[] Reserved { get; set; }

        public SprFileHead(byte[] versionInfo,
            ushort globleWidth,
            ushort globleHeight,
            ushort offX,
            ushort offY,
            ushort frameCounts,
            ushort colourCounts,
            ushort directionCount,
            ushort interval,
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
        public uint FrameOffset;
        public uint DataLenght;
    }

    public struct FrameInfo
    {
        public ushort Width;
        public ushort Height;
        public ushort OffX;
        public ushort OffY;
    }

    public struct FRAMERGBA
    {
        public uint frameWidth;
        public uint frameHeight;
        public uint frameOffX;
        public uint frameOffY;

        public byte[] encyptedFrameData;
        public PaletteColour[] decodedFrameData;
        public PaletteColour[] globleFrameData;
    }

}
