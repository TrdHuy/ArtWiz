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
    public interface ISprWorkManager : IObservableDomain, IDomainAdapter
    {

        #region public API

        /// <summary>
        /// Thay đổi global size của file spr
        /// </summary>
        /// <param name="width">giá trị chiều rộng mới</param>
        /// <param name="height">giá trị chiều cao mới</param>
        void SetGlobalSize(ushort width, ushort height);

        /// <summary>
        /// Thay đổi global offset của file spr
        /// </summary>
        /// <param name="offsetY">giá trị offset Y mới</param>
        /// <param name="offsetX">giá trị offset X mới</param>
        void SetGlobalOffset(short offsetX, short offsetY);

        /// <summary>
        /// Thay đổi frame offset theo frame index 
        /// </summary>
        /// <param name="offsetY">giá trị offset Y mới</param>
        /// <param name="offsetX">giá trị offset X mới</param>
        /// <param name="frameIndex">index của frame cần thay đổi</param>
        void SetFrameOffset(short offsetY, short offsetX, uint frameIndex);

        /// <summary>
        /// Thay đổi frame size theo frame index
        /// </summary>
        /// <param name="frameWidth">Chiều rộng mới của frame</param>
        /// <param name="frameHeight">Chiều cao mới của frame</param>
        /// <param name="frameIndex">index của frame cần thay đổi</param>
        /// <param name="extendingColor">màu sử dụng cho việc mở rộng ảnh</param>
        void SetFrameSize(ushort frameWidth, ushort frameHeight, uint frameIndex, Color? extendingColor = null);

        /// <summary>
        /// Thay đổi khoảng thời gian giữa các frame khi hiển thị animation
        /// </summary>
        /// <param name="interval">khoảng thời gian mới</param>
        void SetSprInterval(ushort interval);

        /// <summary>
        /// file head của spr đang được load trong work manager hiện tại
        /// </summary>
        SprFileHead FileHead { get; }

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
        /// Lấy frame RGBA theo giá trị index trong list frame data đã được khởi tạo
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        FrameRGBA? GetFrameData(uint index);

        /// <summary>
        /// Lấy dữ liệu global palette color theo frame index 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        PaletteColor[]? GetGlobalFrameColorData(uint index, out bool isFrameRedrawed);

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
                    try
                    {
                        fs.Write(GetByteArrayFromHeader(isModifiedData)
                            ?? throw new Exception("Failed to get byte array from header!"));

                        // TODO: Get PaletteData from modified cache
                        fs.Write(GetByteArrayFromPaletteData()
                            ?? throw new Exception("Failed to get byte array from palette data!"));

                        byte[][] allFramesData = new byte[FileHead.FrameCounts][];
                        for (int i = 0; i < FileHead.FrameCounts; i++)
                        {
                            allFramesData[i] = GetByteArrayFromEncryptedFrameData(i, isModifiedData)
                                ?? throw new Exception($"Failed to get byte array from encrypted frame data: index={i}!");
                        }

                        fs.Write(GetByteArrayFromAllFramesOffsetInfo(allFramesData)
                           ?? throw new Exception("Failed to get byte array from frame offset info!"));

                        for (int i = 0; i < FileHead.FrameCounts; i++)
                        {
                            fs.Write(allFramesData[i]);
                        }

                        Logger.Raw.D($"Save current work to spr file successfully: frameCount={FileHead.FrameCounts}");

                    }
                    catch (Exception ex)
                    {
                        Logger.Raw.E(ex.Message);
                    }
                }
            }
        }
        #endregion

        #region public standalone API
        public void SaveBitmapSourceToSprFile(BitmapSource bitmapSource, string filePath)
        {
            var palettePixelArray = this.ConvertBitmapSourceToPaletteColorArray(bitmapSource);
            var countablePaletteColors = this.CountColorsTolist(bitmapSource);

            if (countablePaletteColors.Count > 256)
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
            var paletteColorArray = countablePaletteColors.Select(it =>
                new PaletteColor(it.Item1.B, it.Item1.G, it.Item1.R, it.Item1.A)).ToArray();

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
        protected bool IsCacheEmpty { get; }
        protected byte[]? GetByteArrayFromEncryptedFrameData(int i, bool isModifiedData);
        protected byte[]? GetByteArrayFromHeader(bool isModifiedData);
        protected byte[]? GetByteArrayFromAllFramesOffsetInfo(byte[][] allEncryptedFramesData);
        protected byte[]? GetByteArrayFromPaletteData();

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
