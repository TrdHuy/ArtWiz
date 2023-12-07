using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;

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
        private SprFileHeadCache? _modifiedSprFileHeadCache = null;
        public SprFileHeadCache modifiedSprFileHeadCache
        {
            get
            {
                if (_modifiedSprFileHeadCache == null)
                {
                    _modifiedSprFileHeadCache = new SprFileHeadCache(this);
                }
                return _modifiedSprFileHeadCache;
            }
        }

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

        public bool IsNotEmpty()
        {
            return VersionInfo != null && VersionInfo.Length != 0;
        }

        public static SprFileHead CreateSprFileHead()
        {
            return new SprFileHead(versionInfo: new byte[] { (byte)'S', (byte)'P', (byte)'R', (byte)'\0' },
                globalWidth: 0,
                globalHeight: 0,
                offX: 0,
                offY: 0,
                frameCounts: 0,
                colorCounts: 0,
                directionCount: 0,
                interval: 0,
                reserved: new byte[12]);
        }
        public class SprFileHeadCache
        {
            private SprFileHead sprFileHead;
            public SprFileHeadCache(SprFileHead initData)
            {
                var versionInfo = new byte[initData.VersionInfo.Length];
                var reserved = new byte[initData.Reserved.Length];
                Array.Copy(initData.VersionInfo, versionInfo, initData.VersionInfo.Length);
                Array.Copy(initData.Reserved, reserved, initData.Reserved.Length);
                sprFileHead = new SprFileHead(
                     versionInfo,
                     initData.GlobalWidth,
                     initData.GlobalHeight,
                     initData.OffX,
                     initData.OffY,
                     initData.FrameCounts,
                     initData.ColorCounts,
                     initData.DirectionCount,
                     initData.Interval,
                     reserved
                );
            }

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
            public ushort Interval
            {
                get
                {
                    return sprFileHead.Interval;
                }
                set
                {
                    sprFileHead.Interval = value;
                }
            }
            public ushort FrameCounts
            {
                get
                {
                    return sprFileHead.FrameCounts;
                }
                set
                {
                    sprFileHead.FrameCounts = value;
                }
            }

            public SprFileHead ToSprFileHead()
            {
                return sprFileHead;
            }

            public US_SprFileHead ToUnsafe()
            {
                var reserved = sprFileHead.Reserved;
                var versionInfo = sprFileHead.VersionInfo;
                var unsafeSpr = new US_SprFileHead()
                {
                    GlobalHeight = sprFileHead.GlobalHeight,
                    GlobalWidth = sprFileHead.GlobalWidth,
                    OffX = sprFileHead.OffX,
                    OffY = sprFileHead.OffY,
                    FrameCounts = sprFileHead.FrameCounts,
                    ColorCounts = sprFileHead.ColorCounts,
                    DirectionCount = sprFileHead.DirectionCount,
                    Interval = Interval
                };
                unsafeSpr.SetReserved(reserved);
                unsafeSpr.SetVersionInfoStr(versionInfo);
                return unsafeSpr;
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

        public static bool operator ==(PaletteColor a, PaletteColor b)
        {
            return a.Blue == b.Blue && a.Green == b.Green && a.Red == b.Red && a.Alpha == b.Alpha;
        }

        public static bool operator !=(PaletteColor a, PaletteColor b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (obj is PaletteColor)
                {
                    return this == (PaletteColor)obj;
                }
                else
                {
                    return false;
                }
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public struct Palette
    {
        private PaletteCache? _modifiedCache = null;
        public class PaletteCache
        {
            private Palette modifiedPalette;
            public PaletteCache(Palette originPalette)
            {
                this.modifiedPalette = new Palette(originPalette.Size);
                Array.Copy(originPalette.Data, modifiedPalette.Data, originPalette.Size);
            }

            public PaletteColor this[int index]
            {
                get
                {
                    if ((uint)index >= (uint)modifiedPalette.Size)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    return modifiedPalette.Data[index];
                }

                set
                {
                    if ((uint)index >= (uint)modifiedPalette.Size)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    modifiedPalette.Data[index] = value;
                }
            }

            public PaletteColor[] Data
            {
                get
                {
                    return modifiedPalette.Data;
                }
            }

        }

        public int Size;//Palette_Colour counts, must less than 257
        public PaletteColor[] Data { get; }
        public PaletteCache modifiedPalette
        {
            get
            {
                if (_modifiedCache == null)
                {
                    _modifiedCache = new PaletteCache(this);
                }
                return _modifiedCache;
            }
        }

        public Palette(int size)
        {
            Size = size;
            Data = new PaletteColor[Size];
        }

        public Palette(PaletteColor[] data)
        {
            Size = data.Length;
            Data = new PaletteColor[Size];
            Array.Copy(data, Data, Size);
        }

        public bool IsContain(PaletteColor color)
        {
            foreach (var item in Data)
            {
                if (color.Blue == item.Blue &&
                    color.Green == item.Green &&
                    color.Red == item.Red) return true;
            }
            return false;
        }

        public bool IsSame(Palette palette)
        {
            foreach (var item in palette.Data)
            {
                if (!IsContain(item)) return false;
            }
            return true;
        }

        public static bool operator ==(Palette a, Palette b)
        {
            if (a.Size != b.Size) return false;
            else
            {
                for (int i = 0; i < a.Size; i++)
                {
                    if (!b.IsContain(a.Data[i])) return false;
                }
            }
            return true;
        }

        public static bool operator !=(Palette a, Palette b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (obj is Palette)
                {
                    return this == (Palette)obj;
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
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
        private FrameRGBACache? _modifiedFrameRGBACache;
        public ushort frameWidth { get; set; }
        public ushort frameHeight { get; set; }
        public short frameOffX { get; set; }
        public short frameOffY { get; set; }
        public bool isInsertedFrame { get; set; }
        public byte[] originDecodedBGRAData { get; set; }
        public PaletteColor[] originDecodedFrameData { get; set; }
        public FrameRGBACache modifiedFrameRGBACache
        {
            get
            {
                if (_modifiedFrameRGBACache == null)
                {
                    _modifiedFrameRGBACache = new FrameRGBACache(this);

                    var modifiedData = new PaletteColor[originDecodedFrameData.Length];
                    Array.Copy(originDecodedFrameData, modifiedData, originDecodedFrameData.Length);
                    _modifiedFrameRGBACache.modifiedFrameData = modifiedData;

                    var modifiedBGRAData = new byte[originDecodedBGRAData.Length];
                    Array.Copy(originDecodedBGRAData, modifiedBGRAData, originDecodedBGRAData.Length);
                    _modifiedFrameRGBACache.modifiedBGRAData = modifiedBGRAData;

                }
                return _modifiedFrameRGBACache;
            }
        }

        public bool IsFrameSizeChanged()
        {
            return modifiedFrameRGBACache.frameWidth != frameWidth || modifiedFrameRGBACache.frameHeight != frameHeight;
        }

        public class FrameRGBACache
        {
            private FrameRGBA frameRGBA;
            private Palette paletteData;

            public Dictionary<Color, long>? RgbCountableSource { get; set; }
            public Dictionary<int, List<long>>? PaletteIndexToPixelIndexMap { get; set; }

            public FrameRGBACache(FrameRGBA originData)
            {
                frameRGBA = new FrameRGBA()
                {
                    isInsertedFrame = originData.isInsertedFrame,
                    frameWidth = originData.frameWidth,
                    frameHeight = originData.frameHeight,
                    frameOffX = originData.frameOffX,
                    frameOffY = originData.frameOffY,
                };
            }

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

            public byte[] modifiedBGRAData
            {
                get
                {
                    return frameRGBA.originDecodedBGRAData;
                }
                set
                {
                    frameRGBA.originDecodedBGRAData = value;
                }
            }


            public FrameRGBA toFrameRGBA()
            {
                return frameRGBA;
            }

            public void SetCopiedPaletteData(Palette palette)
            {
                paletteData = new Palette(palette.Data);
            }

            public Palette GetFramePaletteData()
            {
                return paletteData;
            }

            #region palette color changed cache
            private Dictionary<int, (PaletteColor, PaletteColor)>? _paletteColorChangeIndexCache;
            private Dictionary<int, (PaletteColor, PaletteColor)> paletteColorChangeIndexCache
            {
                get
                {
                    if (_paletteColorChangeIndexCache == null)
                    {
                        _paletteColorChangeIndexCache = new Dictionary<int, (PaletteColor, PaletteColor)>();
                    }
                    return _paletteColorChangeIndexCache;
                }
            }
            public bool IsPaletteColorChanged { get; private set; }
            public void ResetPaletteColorChangedIndex()
            {
                paletteColorChangeIndexCache.Clear();
                IsPaletteColorChanged = false;
            }

            public void SetPaletteColorChangedIndex(int index, PaletteColor oldColor, PaletteColor newColor)
            {
                if (paletteColorChangeIndexCache.ContainsKey(index))
                {
                    paletteColorChangeIndexCache[index] = new(paletteColorChangeIndexCache[index].Item1, newColor);
                }
                else
                {
                    paletteColorChangeIndexCache[index] = new(oldColor, newColor);
                }
                IsPaletteColorChanged = true;
            }
            public int[] GetPaletteColorChangedIndex()
            {
                return paletteColorChangeIndexCache.Select(it => it.Key).ToArray();
            }
            public (PaletteColor, PaletteColor)[] GetChangedPaletteColors()
            {
                return paletteColorChangeIndexCache
                    .Select(it =>
                    {
                        PaletteColor oldColor = it.Value.Item1;
                        PaletteColor newColor = it.Value.Item2;
                        (PaletteColor, PaletteColor) t = new(oldColor, newColor);
                        return t;
                    }).ToArray();
            }
            #endregion
        }
    }
}
