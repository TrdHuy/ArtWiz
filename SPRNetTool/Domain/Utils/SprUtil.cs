using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WizMachine.Data;
using WizMachine.Services.Base;

namespace ArtWiz.Domain.Utils
{
    internal static class SprUtil
    {
        public static bool ParseSprData(byte[] sprData, out SprFileHead sprFileHead,
            out Palette palette,
            out int frameDataBeginPos,
            out FrameRGBA[] frameRGBA)
        {
            var initResult = ISprWorkManagerCore.ParseSprData(sprData, out sprFileHead,
                               out palette,
                               out frameDataBeginPos,
                               out frameRGBA);
            return initResult;
        }

        public unsafe static bool IsSprBlock(byte[] sprData)
        {
            if (sprData.Length < sizeof(US_SprFileHead)) return false;
            IntPtr ptr = Marshal.AllocHGlobal(sizeof(US_SprFileHead));
            Marshal.Copy(sprData, 0, ptr, sizeof(US_SprFileHead));
            US_SprFileHead result = Marshal.PtrToStructure<US_SprFileHead>(ptr);
            Marshal.FreeHGlobal(ptr);
            return result.GetVersionInfoStr() == "SPR\0";
        }
    }
}
