using SPRNetTool.Data;
using SPRNetTool.Domain.Utils;
using SPRNetTool.LogUtil;
using SPRNetTool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SPRNetTool.Domain.Base
{
    public interface ISprWorkManagerCore : IObservableDomain, IDomainAdapter
    {

        #region public API

        void ResetWorkSpace()
        {
            InitCache();
        }

        bool IsWorkSpaceEmpty => IsCacheEmpty;

        /// <summary>
        /// file head của spr đang được load trong work manager hiện tại
        /// </summary>
        SprFileHead FileHead { get; }

        /// <summary>
        /// Palette của spr đang được load trong work manager hiện tại
        /// </summary>
        Palette PaletteData { get; }

        /// <summary>
        /// Khởi tạo work manager từ file stream của file SPR
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        bool InitWorkManagerFromSprFile(FileStream fs)
        {
            return fs.BinToStruct<US_SprFileHead>(0)?.Let((it) =>
            {
                if (it.GetVersionInfoStr().StartsWith("SPR"))
                {
                    InitCache();
                    InitFromFileHead(it);
                    InitPaletteDataFromFileStream(fs, it);
                    InitFrameData(fs);
                    return true;
                }
                return false;
            }) ?? false;

        }

        /// <summary>
        /// Lưu các giá trị hiện tại của work ra file SPR
        /// </summary>
        /// <param name="sprFilePath"></param>
        void SaveCurrentWorkToSpr(string sprFilePath, bool isModifiedData)
        {
            if (!IsCacheEmpty)
            {
                using (FileStream fs = new FileStream(sprFilePath, FileMode.Create))
                {
                    var isRecalculatePaletteColorSuccess = false;
                    Palette? newPalettData = null;
                    if (IsContainInsertedFrame())
                    {
                        isRecalculatePaletteColorSuccess = RecalculatePaletteColorForAllInsertedFrame(out newPalettData);
                        if (!isRecalculatePaletteColorSuccess)
                        {
                            throw new Exception("Can not calculate new palette colors for inserted frame!");
                        }

                        newPalettData?.Apply(it =>
                        {
                            ApplyNewPaletteToInsertedFrames(it);

                            if (IsNeedToApplyNewPaletteToOldFrames(it))
                            {
                                ApplyNewPaletteToOldFrames(it);
                            }
                        });
                    }

                    // vì sau khi tính lại bảng palette nên cần check chỉ số color count của 
                    // file head có tương đương với palette mới không
                    fs.Write(GetByteArrayFromHeader(isModifiedData: isModifiedData,
                        isApplyNewPalette: newPalettData != null,
                        colorCount: (ushort)(newPalettData?.Size ?? 0))
                        ?? throw new Exception("Failed to get byte array from header!"));

                    if (newPalettData != null)
                    {
                        newPalettData?.Data.SelectMany(it => new byte[] { it.Red, it.Green, it.Blue })
                            .ToArray()
                            .Also(it => fs.Write(it));
                    }
                    else
                    {
                        fs.Write(GetByteArrayFromPaletteData(isModifiedData)
                            ?? throw new Exception("Failed to get byte array from palette data!"));
                    }

                    byte[][] allFramesData = new byte[FileHead.modifiedSprFileHeadCache.FrameCounts][];
                    for (int i = 0; i < FileHead.modifiedSprFileHeadCache.FrameCounts; i++)
                    {
                        // TODO: cần kiểm tra có frame nào đã thay đổi kích thước hoặc offset hay
                        // không
                        allFramesData[i] = GetByteArrayFromEncryptedFrameData(i,
                            isModifiedData,
                            isRecalculatePaletteColorSuccess,
                            newPalettData)
                            ?? throw new Exception($"Failed to get byte array from encrypted frame data: index={i}!");
                    }

                    fs.Write(GetByteArrayFromAllFramesOffsetInfo(allFramesData)
                       ?? throw new Exception("Failed to get byte array from frame offset info!"));

                    for (int i = 0; i < FileHead.modifiedSprFileHeadCache.FrameCounts; i++)
                    {
                        fs.Write(allFramesData[i]);
                    }

                    Logger.Raw.D($"Save current work to spr file successfully: frameCount={FileHead.FrameCounts}");
                }
            }
        }

        #endregion

        #region public standalone API
        public void SaveBitmapSourceToSprFile(BitmapSource bitmapSource, string filePath)
        {
            var palettePixelArray = this.ConvertBitmapSourceToPaletteColorArray(bitmapSource);
            this.CountColors(
                 bitmapSource
                 , out long argbCount
                 , out long rgbCount
                 , out Dictionary<Color, long> argbSrc
                 , out HashSet<Color> rgbSrc);
            if (rgbCount > 256)
            {
                throw new Exception("cannot save bitmap to spr because its color size > 256");
            }
            if (bitmapSource.PixelWidth > ushort.MaxValue)
            {
                throw new Exception($"cannot save bitmap to spr because its width > {ushort.MaxValue}");
            }
            if (bitmapSource.PixelHeight > ushort.MaxValue)
            {
                throw new Exception($"cannot save bitmap to spr because its height > {ushort.MaxValue}");
            }
            var paletteColorArray = rgbSrc.Select(it =>
                new PaletteColor(it.B, it.G, it.R, it.A)).ToArray();

            var encryptedFrameData = EncryptFrameData(palettePixelArray,
                paletteColorArray,
                frameWidth: (ushort)bitmapSource.PixelWidth,
                frameHeight: (ushort)bitmapSource.PixelHeight,
                frameOffX: 0,
                frameOffY: 0) ?? throw new Exception("failed to encrypt frame data!");

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                var data = EncryptedSprFile(encryptedFrameData: new List<byte[]> { encryptedFrameData },
                    paletteData: paletteColorArray,
                    globalWidth: (ushort)bitmapSource.PixelWidth,
                    globalHeight: (ushort)bitmapSource.PixelHeight,
                    globalOffX: 0,
                    globalOffY: 0,
                    direction: 1,
                    interval: 0,
                    new byte[12]) ?? throw new Exception("Failed to encrypt SPR file!");
                fs.Write(data, 0, data.Length);
            }
        }
        #endregion

        #region protected API
        protected void ApplyNewPaletteToOldFrames(Palette newPaletteData);
        protected void ApplyNewPaletteToInsertedFrames(Palette newPaletteData);
        protected bool IsNeedToApplyNewPaletteToOldFrames(Palette newPaletteData);
        protected bool IsPossibleToSaveFile();
        protected bool IsContainInsertedFrame();
        protected bool RecalculatePaletteColorForAllInsertedFrame(out Palette? newPaletteData);
        protected bool IsCacheEmpty { get; }
        protected byte[]? GetByteArrayFromEncryptedFrameData(int i,
            bool isModifiedData,
            bool isUseRecalculateData,
            Palette? recalculatedPaletteData = null);
        protected byte[]? GetByteArrayFromHeader(bool isModifiedData, bool isApplyNewPalette, ushort colorCount);
        protected byte[]? GetByteArrayFromAllFramesOffsetInfo(byte[][] allEncryptedFramesData);
        protected byte[]? GetByteArrayFromPaletteData(bool isModifiedData);

        protected void InitCache();
        protected void InitFromFileHead(US_SprFileHead fileHead);
        protected void InitPaletteDataFromFileStream(FileStream fs, US_SprFileHead fileHead);
        protected void InitFrameData(FileStream fs);

        protected byte[]? EncryptFrameData(PaletteColor[] pixelArray, PaletteColor[] paletteData
                   , ushort frameWidth, ushort frameHeight, ushort frameOffX, ushort frameOffY);

        protected byte[]? EncryptedSprFile(List<byte[]> encryptedFrameData,
            PaletteColor[] paletteData,
            ushort globalWidth,
            ushort globalHeight,
            short globalOffX,
            short globalOffY,
            ushort direction,
            ushort interval,
            byte[] reserved);
        #endregion
    }
}
