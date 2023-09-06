using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRNetTool.View.Base
{
    public interface IPageViewer : IViewerElement
    {
        IWindowViewer OwnerWindow { get; }
    }
}
