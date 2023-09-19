namespace SPRNetTool.View.Base
{
    public interface IPageViewer : IViewerElement
    {
        IWindowViewer OwnerWindow { get; }
    }
}
