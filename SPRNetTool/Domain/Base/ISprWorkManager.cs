using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WizMachine.Data;

namespace ArtWiz.Domain.Base
{
    public interface ISprWorkManager : IObservableDomain, IDomainAdapter
    {

        void ResetWorkSpace()
        {
            SprWorkManagerService.ResetWorkSpace();
        }

        bool IsWorkSpaceEmpty => SprWorkManagerService.IsWorkSpaceEmpty;

        /// <summary>
        /// file head của spr đang được load trong work manager hiện tại
        /// </summary>
        SprFileHead FileHead => SprWorkManagerService.FileHead;

        /// <summary>
        /// Palette của spr đang được load trong work manager hiện tại
        /// </summary>
        Palette PaletteData => SprWorkManagerService.PaletteData;

        /// <summary>
        /// Khởi tạo work manager từ file stream của file SPR
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        bool InitWorkManagerFromSprFile(FileStream fs)
        {
            return SprWorkManagerService.InitWorkManagerFromSprFile(fs);

        }

        bool InitWorkManagerFromSprFile(string sprFilePath)
        {
            return SprWorkManagerService.InitWorkManagerFromSprFile(sprFilePath);

        }

        /// <summary>
        /// Lưu các giá trị hiện tại của work ra file SPR
        /// </summary>
        /// <param name="sprFilePath"></param>
        void SaveCurrentWorkToSpr(string sprFilePath, bool isModifiedData)
        {
            SprWorkManagerService.SaveCurrentWorkToSpr(sprFilePath, isModifiedData);
        }


        void SetNewColorToGlobalPalette(uint colorIndex,
           byte R, byte G, byte B)
        {
            SprWorkManagerService.SetNewColorToGlobalPalette(colorIndex, R, G, B);
        }

        void SetNewColorToInsertedFramePalette(int frameIndex, uint colorIndex,
           byte R, byte G, byte B)
        {
            SprWorkManagerService.SetNewColorToInsertedFramePalette(frameIndex, colorIndex, R, G, B);
        }

        /// <summary>
        /// Lấy frame RGBA theo giá trị index trong list frame data đã được khởi tạo
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        FrameRGBA? GetFrameData(uint index)
        {
            return SprWorkManagerService.GetFrameData(index);
        }

        /// <summary>
        /// Lấy dữ liệu global palette color theo frame index 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        byte[]? GetDecodedBGRAData(uint index,
            out List<(Color, Color, int)> rgbColorChangedArgs)
        {
            return SprWorkManagerService.GetDecodedBGRAData(index, out rgbColorChangedArgs);
        }

        bool InsertFrame(uint frameIndex
            , ushort frameWidth
            , ushort frameHeight
            , PaletteColor[] pixelData
            , byte[] bgraBytesData
            , Palette paletteData
            , Dictionary<Color, long>? rgbCountableSource = null
            , Dictionary<int, List<long>>? paletteColorIndexToPixelIndexMap = null)
        {
            return SprWorkManagerService.InsertFrame(frameIndex, frameWidth, frameHeight, pixelData, bgraBytesData, paletteData, rgbCountableSource, paletteColorIndexToPixelIndexMap);
        }

        /// <summary>
        /// Xóa frame theo index
        /// </summary>
        /// <param name="frameIndex">vị trí của frame cần xóa</param>
        /// <returns>true nếu xóa frame thành công</returns>
        bool RemoveFrame(uint frameIndex)
        {
            return SprWorkManagerService.RemoveFrame(frameIndex);
        }

        /// <summary>
        /// Đổi chỗ 2 frame cho nhau
        /// </summary>
        /// <param name="frameIndex1">ví trí của frame thứ nhất cần đổi</param>
        /// <param name="frameIndex2">ví trí của frame thứ hai cần đổi</param>
        /// <returns>true nếu đổi chỗ thành công</returns>
        bool SwitchFrame(uint frameIndex1, uint frameIndex2)
        {
            return SprWorkManagerService.SwitchFrame(frameIndex1, frameIndex2);
        }

        /// <summary>
        /// Thay đổi global size của file spr
        /// </summary>
        /// <param name="width">giá trị chiều rộng mới</param>
        /// <param name="height">giá trị chiều cao mới</param>
        void SetGlobalSize(ushort width, ushort height)
        {
            SprWorkManagerService.SetGlobalSize(width, height);
        }

        /// <summary>
        /// Thay đổi global offset của file spr
        /// </summary>
        /// <param name="offsetY">giá trị offset Y mới</param>
        /// <param name="offsetX">giá trị offset X mới</param>
        void SetGlobalOffset(short offsetX, short offsetY)
        {
            SprWorkManagerService.SetGlobalOffset(offsetX, offsetY);
        }

        /// <summary>
        /// Thay đổi frame offset theo frame index 
        /// </summary>
        /// <param name="offsetY">giá trị offset Y mới</param>
        /// <param name="offsetX">giá trị offset X mới</param>
        /// <param name="frameIndex">index của frame cần thay đổi</param>
        void SetFrameOffset(short offsetY, short offsetX, uint frameIndex)
        {
            SprWorkManagerService.SetFrameOffset(offsetY, offsetX, frameIndex);
        }

        /// <summary>
        /// Thay đổi frame size theo frame index
        /// </summary>
        /// <param name="frameWidth">Chiều rộng mới của frame</param>
        /// <param name="frameHeight">Chiều cao mới của frame</param>
        /// <param name="frameIndex">index của frame cần thay đổi</param>
        /// <param name="extendingColor">màu sử dụng cho việc mở rộng ảnh</param>
        void SetFrameSize(ushort frameWidth, ushort frameHeight, uint frameIndex, Color? extendingColor = null)
        {
            SprWorkManagerService.SetFrameSize(frameWidth, frameHeight, frameIndex, extendingColor);
        }

        /// <summary>
        /// Thay đổi khoảng thời gian giữa các frame khi hiển thị animation
        /// </summary>
        /// <param name="interval">khoảng thời gian mới</param>
        void SetSprInterval(ushort interval)
        {
            SprWorkManagerService.SetSprInterval(interval);
        }

        public void SaveBitmapSourceToSprFile(BitmapSource bitmapSource, string filePath)
        {
            SprWorkManagerService.SaveBitmapSourceToSprFile(bitmapSource, filePath);
        }


        protected WizMachine.Services.Base.ISprWorkManagerAdvance SprWorkManagerService { get; }
    }
}
