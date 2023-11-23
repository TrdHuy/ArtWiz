using SPRNetTool.Data;
using System.Collections.Generic;
using System.Windows.Media;

namespace SPRNetTool.Domain.Base
{
    public interface ISprWorkManagerAdvance : ISprWorkManagerCore
    {
        void SetNewColorToPalette(uint colorIndex,
           byte R, byte G, byte B);

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
        byte[]? GetDecodedBGRAData(uint index);

        bool InsertFrame(uint frameIndex
            , ushort frameWidth
            , ushort frameHeight
            , PaletteColor[] pixelData
            , Palette paletteData
            , Dictionary<Color, long>? countableSource = null);

        /// <summary>
        /// Xóa frame theo index
        /// </summary>
        /// <param name="frameIndex">vị trí của frame cần xóa</param>
        /// <returns>true nếu xóa frame thành công</returns>
        bool RemoveFrame(uint frameIndex);

        /// <summary>
        /// Đổi chỗ 2 frame cho nhau
        /// </summary>
        /// <param name="frameIndex1">ví trí của frame thứ nhất cần đổi</param>
        /// <param name="frameIndex2">ví trí của frame thứ hai cần đổi</param>
        /// <returns>true nếu đổi chỗ thành công</returns>
        bool SwitchFrame(uint frameIndex1, uint frameIndex2);

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
    }
}
