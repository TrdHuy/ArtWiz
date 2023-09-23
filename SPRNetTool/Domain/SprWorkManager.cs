﻿using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
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
        private FRAMERGBA[]? FrameData;
        SprFileHead ISprWorkManager.FileHead => FileHead;
        bool ISprWorkManager.IsCacheEmpty => FrameDataBegPos == -1;

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
            FrameData = new FRAMERGBA[us_fileHead.FrameCounts];
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

        void ISprWorkManager.InitFrameData(FileStream fs)
        {
            if (FrameData == null) return;
            for (int i = 0; i < FileHead.FrameCounts; i++)
            {
                var decodedFrameData = InitDecodedFrameData(fs, i, out ushort frameWidth,
                        out ushort frameHeight, COLORMODE.RGBA, out ushort frameOffX,
                        out ushort frameOffY);

                if (decodedFrameData == null) throw new Exception("Failed to init decoded frame data!");

                FrameData[i].frameHeight = frameHeight;
                FrameData[i].frameWidth = frameWidth;
                FrameData[i].frameOffY = frameOffY;
                FrameData[i].frameOffX = frameOffX;
                FrameData[i].decodedFrameData = decodedFrameData;
                var globalData = InitGlobalizedFrameData(i);
                if (globalData == null) throw new Exception("Failed to init global frame data!");
                FrameData[i].globalFrameData = globalData;
            }
        }

        FRAMERGBA? ISprWorkManager.GetFrameData(int index)
        {
            if (index < FileHead.FrameCounts)
            {
                return FrameData?[index];
            }
            return null;
        }

        private PaletteColor[]? InitDecodedFrameData(FileStream fs,
            int index,
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
            PaletteColor transcol = new PaletteColor(0X00, 0X00, 0X00, 0X00);

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
            if (!AreByteArraysEqual(encryptData2, encryptedData))
            {
                throw new Exception("Failed to decrypted");
            }
#endif
            return decData;
        }

        public byte[]? EncryptedSprFile(List<byte[]> encryptedFrameData,
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

        private static bool AreByteArraysEqual(byte[] array1, byte[] array2)
        {
            // Nếu mảng có chiều dài khác nhau, chúng không giống nhau
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                // So sánh từng phần tử của hai mảng
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            // Nếu không có phần tử nào khác nhau, chúng giống nhau
            return true;
        }

        private byte FindPaletteIndex(PaletteColor targetColor, PaletteColor[] paletteData)
        {
            for (byte i = 0; i < paletteData.Length; i++)
            {
                if (targetColor.Red == paletteData[i].Red &&
                    targetColor.Green == paletteData[i].Green &&
                    targetColor.Blue == paletteData[i].Blue)
                {
                    return i;
                }
            }

            return 0;
        }

        public byte[]? EncryptFrameData(PaletteColor[] pixelArray, PaletteColor[] paletteData
                   , ushort frameWidth, ushort frameHeigth, ushort frameOffX, ushort frameOffY)
        {

            var encryptedFrameDataList = new List<byte>();

            var frameInfo = new FrameInfo();
            frameInfo.OffX = frameOffX;
            frameInfo.OffY = frameOffY;
            frameInfo.Height = frameHeigth;
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

        private PaletteColor[]? InitGlobalizedFrameData(int index)
        {
            BitmapSource v;
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

            for (long datidx = 0; datidx < (long)globalDataLen; datidx++)
            {
                globalData[datidx].Red = 0xFF;
                globalData[datidx].Green = 0xFF;
                globalData[datidx].Blue = 0xFF;
                globalData[datidx].Alpha = 0xFF;
            }

            for (int hi = 0; hi < FileHead.GlobalHeight; hi++)
            {
                for (int wi = 0; wi < FileHead.GlobalWidth; wi++)
                {
                    long offwidth = wi + frameOffX;
                    long offheight = hi + frameOffY;

                    if (hi < frameHeight && wi < frameWidth &&
                        offwidth >= 0 && offwidth < FileHead.GlobalWidth &&
                        offheight >= 0 && offheight < FileHead.GlobalHeight)
                    {
                        globalData[offheight * FileHead.GlobalWidth + offwidth].Red = decodedFrameData[hi * frameWidth + wi].Red;
                        globalData[offheight * FileHead.GlobalWidth + offwidth].Green = decodedFrameData[hi * frameWidth + wi].Green;
                        globalData[offheight * FileHead.GlobalWidth + offwidth].Blue = decodedFrameData[hi * frameWidth + wi].Blue;
                        globalData[offheight * FileHead.GlobalWidth + offwidth].Alpha = decodedFrameData[hi * frameWidth + wi].Alpha;
                    }
                }
            }
            return globalData;
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
                    , it.frameOffX
                    , it.frameOffY);
            });
        }
    }

    public enum COLORMODE
    {
        RGB, RGBA, BGRA
    }
}