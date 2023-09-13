using SPRNetTool.Data;
using SPRNetTool.Utils;
using System.IO;

namespace SPRNetTool.Domain.Base
{
    public interface ISprWorkManager : IObservableDomain
    {
        SprFileHead FileHead { get; }
        bool InitWorkManager(FileStream fs)
        {
            var temp = fs.BinToStruct<US_SprFileHead>(0);

            if (temp != null && (temp?.GetVersionInfoStr().StartsWith("SPR") ?? false))
            {
                var header = (US_SprFileHead)temp;
                Init();
                InitFromFileHead(header);
                InitPaletteDataFromFileStream(fs, header);

                InitFrameData(fs);
                return true;
            }
            else
            {
                return false;
            }
        }

        FRAMERGBA? GetFrameData(int index);

        protected void Init();

        protected void InitFromFileHead(US_SprFileHead fileHead);

        protected void InitPaletteDataFromFileStream(FileStream fs, US_SprFileHead fileHead);

        protected void InitFrameData(FileStream fs);
    }
}
