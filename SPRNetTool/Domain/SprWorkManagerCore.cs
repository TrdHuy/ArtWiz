using SPRNetTool.Data;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.LogUtil;
using SPRNetTool.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SPRNetTool.Domain
{
    public class SprWorkManagerCore : BaseDomain, ISprWorkManagerCore
    {
        private Logger logger = new Logger("SprWorkManagerCore");
        private Logger pf_logger = new Logger("SprWorkManagerCore_PF");

        #region caching variables
        protected SprFileHead FileHead;
        protected Palette PaletteData = new Palette();
        protected long FrameDataBegPos = -1;
        protected FrameRGBA[]? FrameData;
        #endregion

        protected bool IsCacheEmpty => FrameDataBegPos == -1;

        #region public interface
        SprFileHead ISprWorkManagerCore.FileHead
        {
            get
            {
                return FileHead;
            }
        }

        Palette ISprWorkManagerCore.PaletteData
        {
            get
            {
                return PaletteData;
            }
        }

        bool ISprWorkManagerCore.IsCacheEmpty => IsCacheEmpty;


        #endregion

        #region standalone api
        private void ApplyPaletteToFrame(Palette newPalettData, FrameRGBA it)
        {
            var dataSize = it.modifiedFrameRGBACache.modifiedBGRAData.Length;
            var newPixelData = new byte[dataSize];
            for (int i = 0; i < dataSize; i += 4)
            {
                // Must keep old alpha, because palette only apply for rgb color
                var oldAlpha = it.modifiedFrameRGBACache.modifiedBGRAData[i + 3];

                var newColor = FindClosetRGBColorInPalette(B: it.modifiedFrameRGBACache.modifiedBGRAData[i],
                    G: it.modifiedFrameRGBACache.modifiedBGRAData[i + 1],
                    R: it.modifiedFrameRGBACache.modifiedBGRAData[i + 2],
                    newPalettData);
                newPixelData[i] = newColor.Blue;
                newPixelData[i + 1] = newColor.Green;
                newPixelData[i + 2] = newColor.Red;
                newPixelData[i + 3] = oldAlpha;
            }
            it.modifiedFrameRGBACache.modifiedBGRAData = newPixelData;
        }

        private PaletteColor FindClosetRGBColorInPalette(byte B, byte G, byte R, Palette palette)
        {
            var distance = double.MaxValue;
            var outColor = new PaletteColor();
            for (int i = 0; i < palette.Size; i++)
            {
                var newDistance = this.CalculateRGBEuclideanDistance(B, G, R, palette.Data[i]);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    outColor = palette.Data[i];
                }
                if (distance == 0)
                {
                    break;
                }
            }
            return outColor;
        }

        private byte[]? EncryptFrameData(byte[] pixelArray, PaletteColor[] paletteData
                   , ushort frameWidth, ushort frameHeight, ushort frameOffX, ushort frameOffY)
        {
            byte FindPaletteIndex(byte B, byte G, byte R, PaletteColor[] paletteData)
            {
                for (int i = 0; i < paletteData.Length; i++)
                {
                    if (R == paletteData[i].Red &&
                        G == paletteData[i].Green &&
                        B == paletteData[i].Blue)
                    {
                        return (byte)i;
                    }
                }

                throw new Exception("Color not found in paletteD");
            }

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
                byte alpha = pixelArray[i + 3];
                if (alpha == 0)
                {
                    while (i < pixelArray.Length && pixelArray[i + 3] == 0 && size < 255)
                    {
                        i += 4;
                        size++;

                        if ((i / 4) % frameWidth == 0)
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
                    while (i < pixelArray.Length && pixelArray[i + 3] == alpha && size < 255)
                    {
                        byte index = FindPaletteIndex(B: pixelArray[i],
                            G: pixelArray[i + 1],
                            R: pixelArray[i + 2],
                            paletteData);
                        temp.Add(index);
                        i += 4;
                        size++;

                        if ((i / 4) % frameWidth == 0)
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
            short globalOffX,
            short globalOffY,
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
        void ISprWorkManagerCore.InitCache()
        {
            FrameData = null;
            FrameDataBegPos = -1;
            FileHead = new SprFileHead();
            PaletteData = new Palette();
        }

        void ISprWorkManagerCore.ApplyNewPaletteToOldFrames(Palette newPalettData)
        {
            FrameData?.FoEach(it =>
            {
                if (!it.isInsertedFrame)
                {
                    ApplyPaletteToFrame(newPalettData, it);
                }
            });
        }

        void ISprWorkManagerCore.ApplyNewPaletteToInsertedFrames(Palette newPalettData)
        {
            FrameData?.FoEach(it =>
            {
                if (it.isInsertedFrame)
                {
                    ApplyPaletteToFrame(newPalettData, it);
                }
            });
        }

        bool ISprWorkManagerCore.IsNeedToApplyNewPaletteToOldFrames(Palette newPalettData)
        {
            return !newPalettData.IsSame(PaletteData);
        }

        bool ISprWorkManagerCore.IsPossibleToSaveFile()
        {
#if DEBUG
            if (IsContainInsertedFrameInternal())
            {
                //FrameData?
                //    .Where(it => it.isInsertedFrame)
                //    .FoEach(it =>
                //    {
                //        if (it.modifiedFrameRGBACache.PaletteData != PaletteData)
                //        {
                //            throw new Exception("Can not save spr file, palette data of inserted frame must be same with origin palette data.");
                //        }
                //    });
            }
#endif
            return true;
        }

        void ISprWorkManagerCore.InitFromFileHead(US_SprFileHead us_fileHead)
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

        void ISprWorkManagerCore.InitPaletteDataFromFileStream(FileStream fs, US_SprFileHead fileHead)
        {
            fs.Position = Marshal.SizeOf(typeof(US_SprFileHead));
            for (int i = 0; i < fileHead.ColorCounts; i++)
            {
                PaletteData.Data[i].Alpha = byte.MaxValue;
                PaletteData.Data[i].Red = (byte)fs.ReadByte();
                PaletteData.Data[i].Green = (byte)fs.ReadByte();
                PaletteData.Data[i].Blue = (byte)fs.ReadByte();
            }
        }

        void ISprWorkManagerCore.InitFrameData(FileStream fs)
        {
            var startTime = DateTime.Now;
            if (FrameData == null) return;
            for (uint i = 0; i < FileHead.FrameCounts; i++)
            {
                var decodedFrameData = InitDecodedFrameData(fs, i, out ushort frameWidth,
                        out ushort frameHeight, ColorMode.RGBA, out ushort frameOffX,
                        out ushort frameOffY,
                        out Dictionary<int, List<long>> paletteColorIndexToPixelIndexMap,
                        out byte[]? decodedByteData);

                if (decodedFrameData == null) throw new Exception("Failed to init decoded frame data!");
                if (decodedByteData == null) throw new Exception("Failed to init decoded frame data!");

                FrameData[i] = new FrameRGBA();
                FrameData[i].frameHeight = frameHeight;
                FrameData[i].frameWidth = frameWidth;
                FrameData[i].frameOffY = (short)frameOffY;
                FrameData[i].frameOffX = (short)frameOffX;
                FrameData[i].originDecodedBGRAData = decodedByteData;

                FrameData[i].modifiedFrameRGBACache.Apply(it =>
                {
                    it.PaletteIndexToPixelIndexMap = paletteColorIndexToPixelIndexMap;
                });
            }
            pf_logger.I($"init frame data total cost: {(DateTime.Now - startTime).TotalMilliseconds}ms");
        }

        byte[]? ISprWorkManagerCore.GetByteArrayFromHeader(bool isModifiedData, bool isApplyNewPalette, ushort colorCount)
        {
            if (isModifiedData && FileHead.modifiedSprFileHeadCache != null)
            {
                if (isApplyNewPalette)
                {
                    var fileHead = FileHead.modifiedSprFileHeadCache.ToUnsafe();
                    fileHead.ColorCounts = colorCount;
                    return fileHead.ToByteArray();
                }
                return FileHead.modifiedSprFileHeadCache.ToUnsafe().ToByteArray();
            }
            return FileHead.ToUnsafe().ToByteArray();
        }

        byte[]? ISprWorkManagerCore.GetByteArrayFromAllFramesOffsetInfo(byte[][] encryptedFramesData)
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

        byte[]? ISprWorkManagerCore.GetByteArrayFromPaletteData(bool isModifiedData)
        {
            if (!isModifiedData)
            {
                return PaletteData.Data.SelectMany(it => new byte[] { it.Red, it.Green, it.Blue }).ToArray();
            }

            return PaletteData.modifiedPalette.Data.SelectMany(it => new byte[] { it.Red, it.Green, it.Blue }).ToArray();
        }

        byte[]? ISprWorkManagerCore.GetByteArrayFromEncryptedFrameData(int i
            , bool isModifiedData
            , bool isUseRecalculateData
            , Palette? recalculatedPaletteData)
        {
            if (FrameData?[i].IsFrameSizeChanged() == true)
            {
                var newFrameWidth = FrameData[i].modifiedFrameRGBACache.frameWidth;
                var newFrameHeight = FrameData[i].modifiedFrameRGBACache.frameHeight;

                PaletteColor getPaletteColorInRef(uint newX,
                    uint newY,
                    ushort refFrameHeight,
                    ushort refFrameWidth,
                    byte[] refFrameData)
                {
                    if (newX >= refFrameWidth || newY >= refFrameHeight)
                    {
                        return new PaletteColor(blue: 0,
                            green: 0,
                            red: 0,
                            alpha: 0);
                    }
                    uint pixelIndex = (newY * refFrameWidth + newX) * 4;
                    return new PaletteColor(blue: refFrameData[pixelIndex],
                            green: refFrameData[pixelIndex + 1],
                            red: refFrameData[pixelIndex + 2],
                            alpha: refFrameData[pixelIndex + 3]);
                }
                var newDecodedBGRAData = new byte[newFrameWidth * newFrameHeight * 4];
                for (ushort newY = 0; newY < newFrameHeight; newY++)
                {
                    for (ushort newX = 0; newX < newFrameWidth; newX++)
                    {
                        var color = getPaletteColorInRef(newX,
                                newY,
                                refFrameHeight: FrameData[i].frameHeight,
                                refFrameWidth: FrameData[i].frameWidth,
                                refFrameData: FrameData[i].modifiedFrameRGBACache.modifiedBGRAData);
                        newDecodedBGRAData[(newY * newFrameWidth + newX) * 4] = color.Blue;
                        newDecodedBGRAData[(newY * newFrameWidth + newX) * 4 + 1] = color.Green;
                        newDecodedBGRAData[(newY * newFrameWidth + newX) * 4 + 2] = color.Red;
                        newDecodedBGRAData[(newY * newFrameWidth + newX) * 4 + 3] = color.Alpha;
                    }
                }
                FrameData[i].modifiedFrameRGBACache.modifiedBGRAData = newDecodedBGRAData;
            }

            if (isUseRecalculateData && recalculatedPaletteData != null)
            {
                return FrameData?[i].Let(it =>
                    EncryptFrameData(it.modifiedFrameRGBACache.modifiedBGRAData
                       , recalculatedPaletteData?.Data!
                       , it.modifiedFrameRGBACache.frameWidth
                       , it.modifiedFrameRGBACache.frameHeight
                       , (ushort)it.modifiedFrameRGBACache.frameOffX
                       , (ushort)it.modifiedFrameRGBACache.frameOffY));
            }
            return FrameData?[i].Let(it => (isModifiedData && it.modifiedFrameRGBACache != null) ?
                EncryptFrameData(it.modifiedFrameRGBACache.modifiedBGRAData
                    , PaletteData.modifiedPalette.Data, it.modifiedFrameRGBACache.frameWidth
                    , it.modifiedFrameRGBACache.frameHeight
                    , (ushort)it.modifiedFrameRGBACache.frameOffX
                    , (ushort)it.modifiedFrameRGBACache.frameOffY) :
                EncryptFrameData(it.originDecodedBGRAData
                    , PaletteData.Data
                    , it.frameWidth
                    , it.frameHeight
                    , (ushort)it.frameOffX
                    , (ushort)it.frameOffY));
        }

        byte[]? ISprWorkManagerCore.EncryptFrameData(byte[] pixelArray,
            PaletteColor[] paletteData,
            ushort frameWidth,
            ushort frameHeight,
            ushort frameOffX,
            ushort frameOffY)
        {
            return EncryptFrameData(pixelArray,
                paletteData,
                frameWidth,
                frameHeight,
                frameOffX,
                frameOffY);
        }

        byte[]? ISprWorkManagerCore.EncryptedSprFile(List<byte[]> encryptedFrameData,
            PaletteColor[] paletteData,
            ushort globalWidth,
            ushort globalHeight,
            short globalOffX,
            short globalOffY,
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

        bool ISprWorkManagerCore.IsContainInsertedFrame()
        {
            return IsContainInsertedFrameInternal();
        }

        bool ISprWorkManagerCore.RecalculatePaletteColorForAllInsertedFrame(out Palette? newPalettData)
        {
            if (FrameData == null)
            {
                newPalettData = null;
                return false;
            }

            // Tính lại toàn bộ lượng màu sử dụng trên từng frame
            // kể cả frame ban đầu và frame được insert mới
            var countableSource = FrameData
                .Select(it => it.modifiedFrameRGBACache.RgbCountableSource
                    ?? this.CountBGRAColors(it.modifiedFrameRGBACache.modifiedBGRAData,
                        out _,
                        out _,
                        out _,
                        out _)
                        .Also(it2 => it.modifiedFrameRGBACache.RgbCountableSource = it2))
                .ToArray();
            var newPaletteSource = this.SelectMostUsePaletteColorFromCountableColorSource(
                colorDifferenceDelta: 100,
                amount: 256,
                countableSource);

            newPalettData = new Palette(newPaletteSource);
            return true;
        }
        #endregion

        private bool IsContainInsertedFrameInternal()
        {
            return FrameData?.Any(it => it.isInsertedFrame) ?? false;
        }

        private PaletteColor[]? InitDecodedFrameData(FileStream fs,
            uint index,
            out ushort frameWidth,
            out ushort frameHeight,
            ColorMode mod,
            out ushort frameOffX,
            out ushort frameOffY,
            out Dictionary<int, List<long>> paletteColorIndexToPixelIndexMap,
            out byte[]? bgraDecodedBytes)
        {
            paletteColorIndexToPixelIndexMap = new Dictionary<int, List<long>>();
            var transcolColorIndex = -1;
            paletteColorIndexToPixelIndexMap[transcolColorIndex] = new List<long>();
            bgraDecodedBytes = null;
            var startTime = DateTime.Now;
            frameWidth = frameHeight = frameOffX = frameOffY = 0;
            if (index > FileHead.FrameCounts || FrameDataBegPos == -1 || FrameData == null)
            {
                return null;
            }
            switch (mod)
            {
                case ColorMode.RGB:
                case ColorMode.RGBA:
                case ColorMode.BGRA:
                    {
                        break;
                    }
                default:
                    {
                        return null;
                    }

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
            bgraDecodedBytes = new byte[decdatalength * 4];
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

                        bgraDecodedBytes[curdecposition * 4] = transcol.Blue;
                        bgraDecodedBytes[curdecposition * 4 + 1] = transcol.Green;
                        bgraDecodedBytes[curdecposition * 4 + 2] = transcol.Red;
                        bgraDecodedBytes[curdecposition * 4 + 3] = transcol.Alpha;
                        paletteColorIndexToPixelIndexMap[transcolColorIndex].Add(curdecposition);
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

                        bgraDecodedBytes[curdecposition * 4] = PaletteData.Data[colorIndex].Blue;
                        bgraDecodedBytes[curdecposition * 4 + 1] = PaletteData.Data[colorIndex].Green;
                        bgraDecodedBytes[curdecposition * 4 + 2] = PaletteData.Data[colorIndex].Red;
                        bgraDecodedBytes[curdecposition * 4 + 3] = (byte)alpha;

                        if (paletteColorIndexToPixelIndexMap.ContainsKey(colorIndex))
                        {
                            paletteColorIndexToPixelIndexMap[colorIndex].Add(curdecposition);
                        }
                        else
                        {
                            paletteColorIndexToPixelIndexMap[colorIndex] = new List<long> { curdecposition };
                        }

                        curdecposition++;
                    }
                }
                i += 2;
            }

            //TODO: remove me
#if DEBUG
            var encryptData2 = EncryptFrameData(bgraDecodedBytes, PaletteData.Data, frameWidth, frameHeight, frameOffX, frameOffY) ?? throw new Exception("Failed to decrypt SPR");
            if (!this.AreByteArraysEqual(encryptData2, encryptedData))
            {
                throw new Exception("Failed to decrypted");
            }

            var totalPixelIndexCount = paletteColorIndexToPixelIndexMap.Sum(it => it.Value.Count);
            Debug.Assert(totalPixelIndexCount == frameHeight * frameWidth);
#endif
            pf_logger.I($"decode frame data {frameWidth}x{frameHeight} in: {(DateTime.Now - startTime).TotalMilliseconds}ms");
            return decData;
        }
    }

    public enum ColorMode
    {
        RGB, RGBA, BGRA
    }
}