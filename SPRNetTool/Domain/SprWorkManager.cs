﻿using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.LogUtil;
using SPRNetTool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace SPRNetTool.Domain
{
    public class SprWorkManager : BaseDomain, ISprWorkManager
    {
        private Logger pf_logger = new Logger("SprWorkManager_PF");
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

        void ISprWorkManager.SetFrameSize(ushort newFrameWidth, ushort newFrameHeight, uint frameIndex, Color? color)
        {
            var startTime = DateTime.Now;
            if (frameIndex >= 0 && frameIndex < FileHead.FrameCounts && FrameData != null)
            {
                var extendingColor = color ?? Colors.White;
                if (newFrameWidth != FrameData[frameIndex].frameWidth
                    || newFrameHeight != FrameData[frameIndex].frameHeight)
                {
                    PaletteColor getPaletteColorInRef(uint newX, uint newY, ushort refFrameHeight, ushort refFrameWidth, PaletteColor[] refFrameData)
                    {
                        if (newX >= refFrameWidth || newY >= refFrameHeight)
                        {
                            return new PaletteColor(blue: extendingColor.B,
                                green: extendingColor.G,
                                red: extendingColor.R,
                                alpha: extendingColor.A);
                        }
                        return refFrameData[newY * refFrameWidth + newX];
                    }

                    var newDecodedFrameData = new PaletteColor[newFrameWidth * newFrameHeight];
                    for (ushort newY = 0; newY < newFrameHeight; newY++)
                    {
                        for (ushort newX = 0; newX < newFrameWidth; newX++)
                        {
                            newDecodedFrameData[newY * newFrameWidth + newX]
                                = getPaletteColorInRef(newX,
                                    newY,
                                    refFrameHeight: FrameData[frameIndex].frameHeight,
                                    refFrameWidth: FrameData[frameIndex].frameWidth,
                                    refFrameData: FrameData[frameIndex].originDecodedFrameData);
                        }
                    }

                    var modifiedFrameRGBACache = FrameData[frameIndex]
                        .modifiedFrameRGBACache ?? new FrameRGBA
                            .FrameRGBACache()
                            .Also((it) =>
                            {
                                it.frameOffX = FrameData[frameIndex].frameOffX;
                                it.frameOffY = FrameData[frameIndex].frameOffY;
                                FrameData[frameIndex].modifiedFrameRGBACache = it;
                            });

                    var oldWidth = modifiedFrameRGBACache.frameWidth;
                    var oldHeight = modifiedFrameRGBACache.frameHeight;
                    modifiedFrameRGBACache.frameWidth = newFrameWidth;
                    modifiedFrameRGBACache.frameHeight = newFrameHeight;
                    modifiedFrameRGBACache.modifiedFrameData = newDecodedFrameData;
                    var globalData = InitGlobalizedFrameDataFromModifiedCache(frameIndex);
                    if (globalData == null) throw new Exception("Failed to set new frame offset");
                    FrameData[frameIndex].globalFrameData = globalData;

                    pf_logger.I($"set frame size from {oldWidth}x{oldHeight} to {newFrameWidth}x{newFrameHeight} in: {(DateTime.Now - startTime).TotalMilliseconds}ms");
                }
            }
        }

        void ISprWorkManager.SetFrameOffset(short offsetY, short offsetX, uint frameIndex)
        {
            var startTime = DateTime.Now;
            if (frameIndex >= 0 && frameIndex < FileHead.FrameCounts && FrameData != null)
            {
                var modifiedFrameRGBACache = FrameData[frameIndex]
                    .modifiedFrameRGBACache ?? new FrameRGBA
                        .FrameRGBACache()
                        .Also((it) =>
                        {
                            it.frameHeight = FrameData[frameIndex].frameHeight;
                            it.frameWidth = FrameData[frameIndex].frameWidth;

                            it.frameOffX = FrameData[frameIndex].frameOffX;
                            it.frameOffY = FrameData[frameIndex].frameOffY;
                            var copiedDecodedFrameData = new PaletteColor[FrameData[frameIndex].originDecodedFrameData.Length];
                            Array.Copy(FrameData[frameIndex].originDecodedFrameData,
                                copiedDecodedFrameData,
                                FrameData[frameIndex].originDecodedFrameData.Length);
                            it.modifiedFrameData = copiedDecodedFrameData;
                            FrameData[frameIndex].modifiedFrameRGBACache = it;
                        });

                if (offsetY != modifiedFrameRGBACache.frameOffY
                    || offsetX != modifiedFrameRGBACache.frameOffX)
                {
                    var oldOffsetY = modifiedFrameRGBACache.frameOffY;
                    var oldOffsetX = modifiedFrameRGBACache.frameOffX;
                    modifiedFrameRGBACache.frameOffY = offsetY;
                    modifiedFrameRGBACache.frameOffX = offsetX;
                    var globalData = InitGlobalizedFrameDataFromModifiedCache(frameIndex);
                    if (globalData == null) throw new Exception("Failed to set new frame offset");
                    FrameData[frameIndex].globalFrameData = globalData;
                    pf_logger.I($"set frame offset from {oldOffsetX}-{oldOffsetY} to {offsetX}-{offsetY} in: {(DateTime.Now - startTime).TotalMilliseconds}ms");
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
            var startTime = DateTime.Now;
            if (FrameData == null) return;
            for (uint i = 0; i < FileHead.FrameCounts; i++)
            {
                var decodedFrameData = InitDecodedFrameData(fs, i, out ushort frameWidth,
                        out ushort frameHeight, COLORMODE.RGBA, out ushort frameOffX,
                        out ushort frameOffY,
                        out PaletteColor[]? paletteColorCache);

                if (decodedFrameData == null) throw new Exception("Failed to init decoded frame data!");
                if (paletteColorCache == null) throw new Exception("Failed to init decoded frame data!");

                FrameData[i].frameHeight = frameHeight;
                FrameData[i].frameWidth = frameWidth;
                FrameData[i].frameOffY = (short)frameOffY;
                FrameData[i].frameOffX = (short)frameOffX;
                FrameData[i].modifiedFrameRGBACache = new FrameRGBA.FrameRGBACache().Also(it =>
                {
                    it.InitFrameRGBA(FrameData[i]);
                    it.modifiedFrameData = paletteColorCache;
                });
                FrameData[i].originDecodedFrameData = decodedFrameData;
                var globalData = InitGlobalizedFrameDataFromOrigin(i);
                if (globalData == null) throw new Exception("Failed to init global frame data!");
                FrameData[i].globalFrameData = globalData;
            }

            pf_logger.I($"init frame data total cost: {(DateTime.Now - startTime).TotalMilliseconds}ms");
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
            return FrameData?[i].Let(it => EncryptFrameData(it.originDecodedFrameData
                    , PaletteData.Data
                    , it.frameWidth
                    , it.frameHeight
                    , (ushort)it.frameOffX
                    , (ushort)it.frameOffY));
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

        #region protected interface
        byte[]? ISprWorkManager.EncryptFrameData(PaletteColor[] pixelArray, PaletteColor[] paletteData, ushort frameWidth, ushort frameHeight, ushort frameOffX, ushort frameOffY)
        {
            return EncryptFrameData(pixelArray,
                paletteData,
                frameWidth,
                frameHeight,
                frameOffX,
                frameOffY);
        }

        byte[]? ISprWorkManager.EncryptedSprFile(List<byte[]> encryptedFrameData,
            PaletteColor[] paletteData,
            ushort globalWidth,
            ushort globalHeight,
            ushort globalOffX,
            ushort globalOffY,
            ushort direction,
            ushort interval,
            byte[] reserved)
        {
            return EncryptedSprFile(encryptedFrameData,
                paletteData,
                globalWidth,
                globalHeight,
                globalOffX,
                globalOffY,
                direction,
                interval,
                reserved);
        }
        #endregion

        private PaletteColor[]? InitGlobalizedFrameDataFromModifiedCache(uint index)
        {
            if (FrameData != null && FrameData[index].modifiedFrameRGBACache != null)
            {
                var cache = FrameData[index].modifiedFrameRGBACache;
                if (cache != null)
                {
                    return InitGlobalizedFrameData(FileHead, cache.toFrameRGBA());
                }
            }
            return null;
        }

        private PaletteColor[]? InitGlobalizedFrameDataFromOrigin(uint index)
        {
            if (FrameData != null)
            {
                return InitGlobalizedFrameData(FileHead, FrameData[index]);
            }
            return null;
        }

        private PaletteColor[] InitGlobalizedFrameData(SprFileHead sprFileHead, FrameRGBA frameRGBA)
        {
            var startTime = DateTime.Now;
            var decodedFrameData = frameRGBA.originDecodedFrameData;
            long globalDataLen = sprFileHead.GlobalHeight * sprFileHead.GlobalWidth;
            PaletteColor[] globalData = new PaletteColor[globalDataLen];
            long frameOffX = frameRGBA.frameOffX;
            long frameOffY = frameRGBA.frameOffY;
            long frameHeight = frameRGBA.frameHeight;
            long frameWidth = frameRGBA.frameWidth;

            // TODO: Dynamic global background color
            for (long datidx = 0; datidx < (long)globalDataLen; datidx++)
            {
                globalData[datidx].Red = 0xFF;
                globalData[datidx].Green = 0xFF;
                globalData[datidx].Blue = 0xFF;
                globalData[datidx].Alpha = 0xFF;
            }

            if (decodedFrameData == null)
            {
                pf_logger.I($"init global frame data {sprFileHead.GlobalWidth}x{sprFileHead.GlobalHeight} in: {(DateTime.Now - startTime).TotalMilliseconds}ms");
                return globalData;
            }

            for (long hi = frameOffY < 0 ? 0 : frameOffY; hi < FileHead.GlobalHeight && hi < frameOffY + frameHeight; hi++)
            {
                for (long wi = frameOffX < 0 ? 0 : frameOffX; wi < FileHead.GlobalWidth && wi < frameOffX + frameWidth; wi++)
                {
                    globalData[hi * sprFileHead.GlobalWidth + wi].Red = decodedFrameData[(hi - frameOffY) * frameWidth + (wi - frameOffX)].Red;
                    globalData[hi * sprFileHead.GlobalWidth + wi].Green = decodedFrameData[(hi - frameOffY) * frameWidth + (wi - frameOffX)].Green;
                    globalData[hi * sprFileHead.GlobalWidth + wi].Blue = decodedFrameData[(hi - frameOffY) * frameWidth + (wi - frameOffX)].Blue;
                    globalData[hi * sprFileHead.GlobalWidth + wi].Alpha = decodedFrameData[(hi - frameOffY) * frameWidth + (wi - frameOffX)].Alpha;
                }
            }
            pf_logger.I($"init global frame data {sprFileHead.GlobalWidth}x{sprFileHead.GlobalHeight} in: {(DateTime.Now - startTime).TotalMilliseconds}ms");
            return globalData;
        }

        private PaletteColor[]? InitDecodedFrameData(FileStream fs,
            uint index,
            out ushort frameWidth,
            out ushort frameHeight,
            COLORMODE mod,
            out ushort frameOffX,
            out ushort frameOffY,
            out PaletteColor[]? paletteColorsCache)
        {
            var startTime = DateTime.Now;
            frameWidth = frameHeight = frameOffX = frameOffY = 0;
            if (index > FileHead.FrameCounts || FrameDataBegPos == -1 || FrameData == null)
            {
                paletteColorsCache = null;
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
                default:
                    {
                        paletteColorsCache = null;
                        return null;
                    }

            }
            PaletteColor transcol = new PaletteColor(0XFF, 0XFF, 0XFF, 0X00);

            var frameOffsetInfo = fs.BinToStruct<FrameOffsetInfo>(FrameDataBegPos + Marshal.SizeOf(typeof(FrameOffsetInfo)) * index);

            var frameBeginPos = FrameDataBegPos + (frameOffsetInfo?.FrameOffset ?? 0) + Marshal.SizeOf(typeof(FrameOffsetInfo)) * FileHead.FrameCounts;
            var datalength = frameOffsetInfo?.DataLength ?? 0;

            if (datalength == 0)
            {
                paletteColorsCache = null;
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
                paletteColorsCache = null;
                return null;
            }

            var encryptedData = new byte[datalength];
            frameInfo?.CopyStructToArray(encryptedData, 0);

            PaletteColor[] decData = new PaletteColor[decdatalength];
            paletteColorsCache = new PaletteColor[decdatalength];

            var frameDataPos = frameBeginPos + Marshal.SizeOf(typeof(FrameInfo));
            fs.Position = frameDataPos;
            long curdecposition = 0;
            for (long i = 0; i < datalength - 8;)
            {
                if (curdecposition > decdatalength)
                {
                    paletteColorsCache = null;
                    return null;
                }
                int size = fs.ReadByte();
                encryptedData[i + 8] = (byte)size;
                int alpha = fs.ReadByte();
                encryptedData[i + 9] = (byte)alpha;

                if (size == -1 || alpha == -1)
                {
                    paletteColorsCache = null;
                    return null;
                }

                if (alpha == 0x00)
                {
                    for (int j = 0; j < size; j++)
                    {
                        paletteColorsCache[curdecposition].Red = transcol.Red;
                        paletteColorsCache[curdecposition].Blue = transcol.Blue;
                        paletteColorsCache[curdecposition].Green = transcol.Green;
                        paletteColorsCache[curdecposition].Alpha = transcol.Alpha;

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
                            paletteColorsCache = null;
                            return null;
                        }
                        i++;
                        paletteColorsCache[curdecposition].Red = PaletteData.Data[colorIndex].Red;
                        paletteColorsCache[curdecposition].Blue = PaletteData.Data[colorIndex].Blue;
                        paletteColorsCache[curdecposition].Green = PaletteData.Data[colorIndex].Green;
                        paletteColorsCache[curdecposition].Alpha = (byte)alpha;

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
            pf_logger.I($"decode frame data {frameWidth}x{frameHeight} in: {(DateTime.Now - startTime).TotalMilliseconds}ms");
            return decData;
        }

    }

    public enum COLORMODE
    {
        RGB, RGBA, BGRA
    }
}