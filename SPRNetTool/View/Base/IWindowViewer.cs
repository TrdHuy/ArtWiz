﻿using System.Windows;

namespace ArtWiz.View.Base
{
    public interface IWindowViewer : IViewerElement
    {
        delegate void WindowClosedHandler(Window w);

        public void AddOnWindowClosedEvent(WindowClosedHandler onWindowClosed);
        public void RemoveOnWindowClosedEvent(WindowClosedHandler onWindowClosed);

        public void DisableWindow(bool isDisabled);
    }
}
