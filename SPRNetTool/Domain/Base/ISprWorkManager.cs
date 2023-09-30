using SPRNetTool.Data;
using SPRNetTool.LogUtil;
using SPRNetTool.Utils;
using System;
using System.IO;

namespace SPRNetTool.Domain.Base
{
    public interface ISprWorkManager : IObservableDomain
    {

        #region public API
        void SetFrameOffset(short offsetY, short offsetX, uint frameIndex);

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
        /// Lưu các giá trị hiện tại của work ra file SPR
        /// </summary>
        /// <param name="sprFilePath"></param>
        void SaveCurrentWorkToSpr(string sprFilePath)
        {
            if (!IsCacheEmpty)
            {
                using (FileStream fs = new FileStream(sprFilePath, FileMode.Create))
                {
                    try
                    {
                        fs.Write(GetByteArrayFromHeader()
                            ?? throw new Exception("Failed to get byte array from header!"));
                        fs.Write(GetByteArrayFromPaletteData()
                            ?? throw new Exception("Failed to get byte array from palette data!"));

                        byte[][] allFramesData = new byte[FileHead.FrameCounts][];
                        for (int i = 0; i < FileHead.FrameCounts; i++)
                        {
                            allFramesData[i] = GetByteArrayFromEncyptedFrameData(i)
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

        #region protected API
        protected bool IsCacheEmpty { get; }
        protected byte[]? GetByteArrayFromEncyptedFrameData(int i);
        protected byte[]? GetByteArrayFromHeader();
        protected byte[]? GetByteArrayFromAllFramesOffsetInfo(byte[][] allEncryptedFramesData);
        protected byte[]? GetByteArrayFromPaletteData();

        protected void InitCache();
        protected void InitFromFileHead(US_SprFileHead fileHead);
        protected void InitPaletteDataFromFileStream(FileStream fs, US_SprFileHead fileHead);
        protected void InitFrameData(FileStream fs);

        #endregion
    }
}
