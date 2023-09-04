using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRNetTool.Domain.Base
{
    public interface IBitmapDisplayManager
    {
        void OpenBitmapFromFile(string filePath, bool countPixelColor);
    }
}
