using System.Runtime.InteropServices;
using static SPRNetTool.Data.FrameRGBA;

namespace SPRNetTool.Data
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct US_SprFileHead
    {
        fixed byte VersionInfo[4];
        public ushort GlobalWidth;
        public ushort GlobalHeight;
        public short OffX;
        public short OffY;
        public ushort FrameCounts;
        public ushort ColorCounts;
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

        public void SetVersionInfoStr(byte[] versionInfo)
        {
            for (int i = 0; i < 4; i++)
            {
                VersionInfo[i] = versionInfo[i];
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
        public byte[] VersionInfo { get; set; }
        public ushort GlobalWidth { get; set; }
        public ushort GlobalHeight { get; set; }
        public short OffX { get; set; }
        public short OffY { get; set; }
        public ushort FrameCounts { get; set; }
        public ushort ColorCounts { get; set; }
        public ushort DirectionCount { get; set; }
        public ushort Interval { get; set; }
        public byte[] Reserved { get; set; }
        public SprFileHeadCache? modifiedSprFileHeadCache { get; set; } = null;

        public SprFileHead(byte[] versionInfo,
            ushort globalWidth,
            ushort globalHeight,
            short offX,
            short offY,
            ushort frameCounts,
            ushort colorCounts,
            ushort directionCount,
            ushort interval,
            byte[] reserved)
        {
            VersionInfo = versionInfo;
            GlobalWidth = globalWidth;
            GlobalHeight = globalHeight;
            OffX = offX;
            OffY = offY;
            FrameCounts = frameCounts;
            ColorCounts = colorCounts;
            DirectionCount = directionCount;
            Interval = interval;
            Reserved = reserved;
        }

        public US_SprFileHead ToUnsafe()
        {
            var reserved = Reserved;
            var versionInfo = VersionInfo;
            var unsafeSpr = new US_SprFileHead()
            {
                GlobalHeight = GlobalHeight,
                GlobalWidth = GlobalWidth,
                OffX = OffX,
                OffY = OffY,
                FrameCounts = FrameCounts,
                ColorCounts = ColorCounts,
                DirectionCount = DirectionCount,
                Interval = Interval
            };
            unsafeSpr.SetReserved(reserved);
            unsafeSpr.SetVersionInfoStr(versionInfo);
            return unsafeSpr;
        }

        public class SprFileHeadCache
        {
            private SprFileHead sprFileHead;
            public ushort globalWidth
            {
                get
                {
                    return sprFileHead.GlobalWidth;
                }
                set
                {
                    sprFileHead.GlobalWidth = value;
                }
            }
            public ushort globalHeight
            {
                get
                {
                    return sprFileHead.GlobalHeight;
                }
                set
                {
                    sprFileHead.GlobalHeight = value;
                }
            }
            public short offX
            {
                get
                {
                    return sprFileHead.OffX;
                }
                set
                {
                    sprFileHead.OffX = value;
                }
            }
            public short offY
            {
                get
                {
                    return sprFileHead.OffY;
                }
                set
                {
                    sprFileHead.OffY = value;
                }
            }
            public SprFileHead toSprFileHead()
            {
                return sprFileHead;
            }

            public void InitFromSprFileHead(SprFileHead initData)
            {
                sprFileHead = initData;
            }
        }
    }

    public struct PaletteColor
    {
        public byte Blue = 0x00;
        public byte Green = 0x00;
        public byte Red = 0x00;
        public byte Alpha = 0x00;

        public PaletteColor(byte blue, byte green, byte red, byte alpha)
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
        public PaletteColor[] Data;

        public Palette(int size)
        {
            Size = size;
            Data = new PaletteColor[256];
        }
    };

    public struct FrameOffsetInfo
    {
        public uint FrameOffset;
        public uint DataLength;
    }

    public struct FrameInfo
    {
        public ushort Width;
        public ushort Height;
        public ushort OffX;
        public ushort OffY;
    }

    public struct FrameRGBA
    {
        public ushort frameWidth { get; set; }
        public ushort frameHeight { get; set; }
        public short frameOffX { get; set; }
        public short frameOffY { get; set; }

        public byte[] encryptedFrameData { get; set; }
        public PaletteColor[] originDecodedFrameData { get; set; }
        public PaletteColor[] globalFrameData { get; set; }

        public FrameRGBACache? modifiedFrameRGBACache { get; set; }

        public class FrameRGBACache
        {
            private FrameRGBA frameRGBA;

            public ushort frameWidth
            {
                get
                {
                    return frameRGBA.frameWidth;
                }
                set
                {
                    frameRGBA.frameWidth = value;
                }
            }
            public ushort frameHeight
            {
                get
                {
                    return frameRGBA.frameHeight;
                }
                set
                {
                    frameRGBA.frameHeight = value;
                }
            }
            public short frameOffX
            {
                get
                {
                    return frameRGBA.frameOffX;
                }
                set
                {
                    frameRGBA.frameOffX = value;
                }
            }
            public short frameOffY
            {
                get
                {
                    return frameRGBA.frameOffY;
                }
                set
                {
                    frameRGBA.frameOffY = value;
                }
            }

            public byte[] encryptedFrameData
            {
                get
                {
                    return frameRGBA.encryptedFrameData;
                }
                set
                {
                    frameRGBA.encryptedFrameData = value;
                }
            }
            public PaletteColor[] modifiedFrameData
            {
                get
                {
                    return frameRGBA.originDecodedFrameData;
                }
                set
                {
                    frameRGBA.originDecodedFrameData = value;
                }
            }
            public PaletteColor[] globalFrameData
            {
                get
                {
                    return frameRGBA.globalFrameData;
                }
                set
                {
                    frameRGBA.globalFrameData = value;
                }
            }

            public FrameRGBA toFrameRGBA()
            {
                return frameRGBA;
            }

            public void InitFrameRGBA(FrameRGBA initData)
            {
                frameRGBA = initData;
            }
        }

        public bool isNeedToRedrawGlobalFrameData { get; set; }
    }


}
