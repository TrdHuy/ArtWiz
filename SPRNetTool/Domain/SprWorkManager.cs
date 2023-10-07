using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace SPRNetTool.Domain
{
    public class SprWorkManager : BaseDomain, ISprWorkManager
    {
        private SprFileHead FileHead;
        private Palette PaletteData = new Palette();
        private long FrameDataBegPos = -1;
        private FrameRGBA[]? FrameData;

        private bool IsCacheEmpty => FrameDataBegPos == -1;

        #region public interface
        SprFileHead ISprWorkManager.FileHead => FileHead;
        bool ISprWorkManager.IsCacheEmpty => IsCacheEmpty;
        void ISprWorkManager.InitCache()
        {
            FrameData = null;
            FrameDataBegPos = -1;
            FileHead = new SprFileHead();
            PaletteData = new Palette();
        }

        void ISprWorkManager.InitFromFileHead(US_SprFileHead us_fileHead)
        {
            FileHead = new SprFileHead(us_fileHead.GetVersionInfo(),
                us_fileHead.GlobalWidth,
                us_fileHead.GlobalHeight,
                us_fileHead.OffX,
                us_fileHead.OffY, us_fileHead.FrameCounts,
                us_fileHead.ColorCounts,
                us_fileHead.DirectionCount,
                us_fileHead.Interval,
                us_fileHead.GetReserved());
            FrameData = new FrameRGBA[us_fileHead.FrameCounts];
            PaletteData = new Palette(us_fileHead.ColorCounts);
            FrameDataBegPos = Marshal.SizeOf(typeof(US_SprFileHead)) + us_fileHead.ColorCounts * 3;

        }

        void ISprWorkManager.InitPaletteDataFromFileStream(FileStream fs, US_SprFileHead fileHead)
        {
            fs.Position = Marshal.SizeOf(typeof(US_SprFileHead));
            for (int i = 0; i < fileHead.ColorCounts; i++)
            {
                PaletteData.Data[i].Red = (byte)fs.ReadByte();
                PaletteData.Data[i].Green = (byte)fs.ReadByte();
                PaletteData.Data[i].Blue = (byte)fs.ReadByte();
            }
        }

        void ISprWorkManager.SetFrameOffset(short offsetY, short offsetX, uint frameIndex)
        {
            if (frameIndex >= 0 && frameIndex < FileHead.FrameCounts && FrameData != null)
            {
                if (offsetY != FrameData[frameIndex].frameOffY
                    || offsetX != FrameData[frameIndex].frameOffX)
                {
                    FrameData[frameIndex].frameOffY = offsetY;
                    FrameData[frameIndex].frameOffX = offsetX;
                    var globalData = InitGlobalizedFrameData(frameIndex);
                    if (globalData == null) throw new Exception("Failed to set new frame offset");
                    FrameData[frameIndex].globalFrameData = globalData;
                }
            }
        }

        void ISprWorkManager.SetSprInterval(ushort interval)
        {
            if (IsCacheEmpty) return;
            FileHead.Interval = interval;
        }

        void ISprWorkManager.InitFrameData(FileStream fs)
        {
            if (FrameData == null) return;
            for (uint i = 0; i < FileHead.FrameCounts; i++)
            {
                var decodedFrameData = InitDecodedFrameData(fs, i, out ushort frameWidth,
                        out ushort frameHeight, COLORMODE.RGBA, out ushort frameOffX,
                        out ushort frameOffY);

                if (decodedFrameData == null) throw new Exception("Failed to init decoded frame data!");

                FrameData[i].frameHeight = frameHeight;
                FrameData[i].frameWidth = frameWidth;
                FrameData[i].frameOffY = (short)frameOffY;
                FrameData[i].frameOffX = (short)frameOffX;
                FrameData[i].decodedFrameData = decodedFrameData;
                var globalData = InitGlobalizedFrameData(i);
                if (globalData == null) throw new Exception("Failed to init global frame data!");
                FrameData[i].globalFrameData = globalData;
            }
        }

        FrameRGBA? ISprWorkManager.GetFrameData(uint index)
        {
            if (index < FileHead.FrameCounts)
            {
                return FrameData?[index];
            }
            return null;
        }

        byte[]? ISprWorkManager.GetByteArrayFromHeader()
        {
            return FileHead.ToUnsafe().ToByteArray();
        }

        byte[]? ISprWorkManager.GetByteArrayFromAllFramesOffsetInfo(byte[][] encryptedFramesData)
        {
            var frameOffsetInfoStructSize = Marshal.SizeOf(typeof(FrameOffsetInfo));
            var result = new byte[encryptedFramesData.Length * frameOffsetInfoStructSize];
            for (int frameIndex = 0; frameIndex < encryptedFramesData.Length; frameIndex++)
            {
                FrameOffsetInfo frameOffsetInfo = new FrameOffsetInfo();
                for (int i = 0; i < frameIndex; i++)
                {
                    frameOffsetInfo.FrameOffset += (uint)encryptedFramesData[i].Length;
                }
                frameOffsetInfo.DataLength = (uint)encryptedFramesData[frameIndex].Length;
                frameOffsetInfo.CopyStructToArray(result, frameIndex * frameOffsetInfoStructSize);
            }
            return result;
        }

        byte[]? ISprWorkManager.GetByteArrayFromPaletteData()
        {
            return PaletteData.Data.SelectMany(it => new byte[] { it.Red, it.Green, it.Blue }).ToArray();
        }

        byte[]? ISprWorkManager.GetByteArrayFromEncyptedFrameData(int i)
        {
            return FrameData?[i].Let((it) =>
            {
                return EncryptFrameData(it.decodedFrameData
                    , PaletteData.Data
                    , it.frameWidth
                    , it.frameHeight
                    , (ushort)it.frameOffX
                    , (ushort)it.frameOffY);
            });
        }

        #endregion

        #region standalone api
        private byte FindPaletteIndex(PaletteColor targetColor, PaletteColor[] paletteData)
        {
            for (int i = 0; i < paletteData.Length; i++)
            {
                if (targetColor.Red == paletteData[i].Red &&
                    targetColor.Green == paletteData[i].Green &&
                    targetColor.Blue == paletteData[i].Blue)
                {
                    return (byte)i;
                }
            }

            return 0;
        }

        private byte[]? EncryptFrameData(PaletteColor[] pixelArray, PaletteColor[] paletteData
                   , ushort frameWidth, ushort frameHeight, ushort frameOffX, ushort frameOffY)
        {

            var encryptedFrameDataList = new List<byte>();

            var frameInfo = new FrameInfo();
            frameInfo.OffX = frameOffX;
            frameInfo.OffY = frameOffY;
            frameInfo.Height = frameHeight;
            frameInfo.Width = frameWidth;
            frameInfo.CopyStructToList(encryptedFrameDataList);

            for (int i = 0; i < pixelArray.Length;)
            {
                byte size = 0;
                byte alpha = pixelArray[i].Alpha;
                if (alpha == 0)
                {
                    while (i < pixelArray.Length && pixelArray[i].Alpha == 0 && size < 255)
                    {
                        i++;
                        size++;

                        if (i % frameWidth == 0)
                        {
                            break;
                        }
                    }
                    encryptedFrameDataList.Add(size);
                    encryptedFrameDataList.Add(alpha);
                }
                else
                {
                    List<byte> temp = new List<byte>();
                    while (i < pixelArray.Length && pixelArray[i].Alpha == alpha && size < 255)
                    {
                        byte index = FindPaletteIndex(pixelArray[i], paletteData);
                        temp.Add(index);
                        i++;
                        size++;

                        if (i % frameWidth == 0)
                        {
                            break;
                        }
                    }
                    encryptedFrameDataList.Add(size);
                    encryptedFrameDataList.Add(alpha);
                    encryptedFrameDataList.AddRange(temp);
                }
            }
            return encryptedFrameDataList.ToArray();
        }

        private byte[]? EncryptedSprFile(List<byte[]> encryptedFrameData,
            PaletteColor[] paletteData,
            ushort globalWidth,
            ushort globalHeight,
            ushort globalOffX,
            ushort globalOffY,
            ushort direction,
            ushort interval,
            byte[] reserved)
        {

            void WritePaletteColorToByteList(PaletteColor color, List<byte> list)
            {
                list.Add(color.Red);
                list.Add(color.Green);
                list.Add(color.Blue);
            }

            void WriteFrameOffsetInfoList(List<byte[]> encryptedFrameDatas, List<byte> list, int index)
            {
                FrameOffsetInfo frameOffsetInfo = new FrameOffsetInfo();
                for (int i = 0; i < index; i++)
                {
                    frameOffsetInfo.FrameOffset += (uint)encryptedFrameDatas[i].Length;
                }
                frameOffsetInfo.DataLength = (uint)encryptedFrameDatas[index].Length;
                frameOffsetInfo.CopyStructToList(list);
            }

            if (paletteData.Length > 256 && encryptedFrameData.Count > ushort.MaxValue)
            {
                throw new Exception("Failed to encrypt SPR file");
            }
            US_SprFileHead fileHead = new US_SprFileHead();
            fileHead.SetVersionInfoStr(new char[] { 'S', 'P', 'R', '\0' });
            fileHead.SetReserved(reserved);
            fileHead.Interval = interval;
            fileHead.FrameCounts = (ushort)encryptedFrameData.Count;
            fileHead.GlobalHeight = globalHeight;
            fileHead.GlobalWidth = globalWidth;
            fileHead.OffX = globalOffX;
            fileHead.OffY = globalOffY;
            fileHead.DirectionCount = direction;
            fileHead.ColorCounts = (ushort)paletteData.Length;

            List<byte> encryptedFileData = new List<byte>();

            // write file head
            fileHead.CopyStructToList(encryptedFileData);

            // write color palette
            foreach (var color in paletteData)
            {
                WritePaletteColorToByteList(color, encryptedFileData);
            }

            // write frame offset info
            for (int i = 0; i < encryptedFrameData.Count; i++)
            {
                WriteFrameOffsetInfoList(encryptedFrameData, encryptedFileData, i);
            }

            // write frame data
            for (int i = 0; i < encryptedFrameData.Count; i++)
            {
                encryptedFileData.AddRange(encryptedFrameData[i]);
            }

            return encryptedFileData.ToArray();
        }
        #endregion

        private PaletteColor[]? InitGlobalizedFrameData(uint index)
        {
            var decodedFrameData = FrameData?[index].decodedFrameData;
            if (decodedFrameData == null)
            {
                return null;
            }

            long globalDataLen = FileHead.GlobalHeight * FileHead.GlobalWidth;
            PaletteColor[] globalData = new PaletteColor[globalDataLen];
            long frameOffX = FrameData?[index].frameOffX ?? 0;
            long frameOffY = FrameData?[index].frameOffY ?? 0;
            long frameHeight = FrameData?[index].frameHeight ?? 0;
            long frameWidth = FrameData?[index].frameWidth ?? 0;

            // TODO: Dynamic global background color
            for (long datidx = 0; datidx < (long)globalDataLen; datidx++)
            {
                globalData[datidx].Red = 0xFF;
                globalData[datidx].Green = 0xFF;
                globalData[datidx].Blue = 0xFF;
                globalData[datidx].Alpha = 0xFF;
            }

            for (long hi = frameOffY < 0 ? 0 : frameOffY; hi < FileHead.GlobalHeight && hi < frameOffY + frameHeight; hi++)
            {
                for (long wi = frameOffX < 0 ? 0 : frameOffX; wi < FileHead.GlobalWidth && wi < frameOffX + frameWidth; wi++)
                {
                    globalData[hi * FileHead.GlobalWidth + wi].Red = decodedFrameData[(hi - frameOffY) * frameWidth + (wi - frameOffX)].Red;
                    globalData[hi * FileHead.GlobalWidth + wi].Green = decodedFrameData[(hi - frameOffY) * frameWidth + (wi - frameOffX)].Green;
                    globalData[hi * FileHead.GlobalWidth + wi].Blue = decodedFrameData[(hi - frameOffY) * frameWidth + (wi - frameOffX)].Blue;
                    globalData[hi * FileHead.GlobalWidth + wi].Alpha = decodedFrameData[(hi - frameOffY) * frameWidth + (wi - frameOffX)].Alpha;
                }
            }
            return globalData;
        }

        private PaletteColor[]? InitDecodedFrameData(FileStream fs,
            uint index,
            out ushort frameWidth,
            out ushort frameHeight,
            COLORMODE mod,
            out ushort frameOffX,
            out ushort frameOffY)
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
            PaletteColor transcol = new PaletteColor(0XFF, 0XFF, 0XFF, 0X00);

            var frameOffsetInfo = fs.BinToStruct<FrameOffsetInfo>(FrameDataBegPos + Marshal.SizeOf(typeof(FrameOffsetInfo)) * index);

            var frameBeginPos = FrameDataBegPos + (frameOffsetInfo?.FrameOffset ?? 0) + Marshal.SizeOf(typeof(FrameOffsetInfo)) * FileHead.FrameCounts;
            var datalength = frameOffsetInfo?.DataLength ?? 0;

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

            var encryptedData = new byte[datalength];
            frameInfo?.CopyStructToArray(encryptedData, 0);

            PaletteColor[] decData = new PaletteColor[decdatalength];
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
                encryptedData[i + 8] = (byte)size;
                int alpha = fs.ReadByte();
                encryptedData[i + 9] = (byte)alpha;

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
                        encryptedData[i + 10] = (byte)colorIndex;
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

            //TODO: remove me
#if DEBUG
            var encryptData2 = EncryptFrameData(decData, PaletteData.Data, frameWidth, frameHeight, frameOffX, frameOffY) ?? throw new Exception("Failed to decrypt SPR");
            if (!this.AreByteArraysEqual(encryptData2, encryptedData))
            {
                throw new Exception("Failed to decrypted");
            }
#endif
            return decData;
        }
    }

    public enum COLORMODE
    {
        RGB, RGBA, BGRA
    }
}