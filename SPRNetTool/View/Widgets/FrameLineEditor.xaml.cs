﻿using SPRNetTool.Utils;
using SPRNetTool.ViewModel.Widgets;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using static SPRNetTool.View.Widgets.FrameLineController;

namespace SPRNetTool.View.Widgets
{
    /// <summary>
    /// Interaction logic for FrameLineEditor.xaml
    /// </summary>
    public partial class FrameLineEditor : UserControl
    {
        public static void AddOnPreviewFrameIndexSwitchedHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewFrameIndexSwitched += handler;
            });
        }

        public static void RemoveOnPreviewFrameIndexSwitchedHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewFrameIndexSwitched -= handler;
            });
        }

        public static void AddOnPreviewAddingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewAddingFrame += handler;
            });
        }

        public static void RemoveOnPreviewAddingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewAddingFrame -= handler;
            });
        }

        public static void AddOnPreviewRemovingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewRemovingFrame += handler;
            });
        }

        public static void RemoveOnPreviewRemovingFrameHandler(DependencyObject element, FameLineHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnPreviewRemovingFrame -= handler;
            });
        }

        public static void AddOnEllipseMouseClickHandler(DependencyObject element, FameLineMouseEventHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnEllipseMouseClick += handler;
            });
        }

        public static void RemoveOnEllipseMouseClickHandler(DependencyObject element, FameLineMouseEventHandler handler)
        {
            element.IfIs<FrameLineEditor>(it =>
            {
                it.Controller.OnEllipseMouseClick -= handler;
            });
        }

        public static readonly DependencyProperty FrameSourceProperty =
            DependencyProperty.Register(
                "FrameSource",
                typeof(IEnumerable<IFramePreviewerViewModel>),
                typeof(FrameLineEditor),
            new FrameworkPropertyMetadata(defaultValue: default(IEnumerable<IFramePreviewerViewModel>),
                flags: FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnFrameSourceChangeCallback)));

        private static void OnFrameSourceChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.IfIs<FrameLineEditor>(it =>
            {
                it.SetUpFrameSource(e.NewValue as IEnumerable<IFramePreviewerViewModel>);
                e.OldValue.IfIs<IEnumerable<IFramePreviewerViewModel>>(source => it.DisposeFrameSource(source));
            });
        }

        public IEnumerable FrameSource
        {
            get { return (IEnumerable)GetValue(FrameSourceProperty); }
            set { SetValue(FrameSourceProperty, value); }
        }

        private FrameLineController Controller;

        public FrameLineEditor()
        {
            InitializeComponent();
            Controller = new FrameLineController(ScrView, MainCanvas, 0);
        }

        private void SetUpFrameSource(IEnumerable<IFramePreviewerViewModel>? frameSource)
        {
            if (frameSource == null)
            {
                Controller.SetTotalFrameCount(null);
            }
            else
            {
                frameSource.IfIs<INotifyCollectionChanged>(it =>
                {
                    it.CollectionChanged += FrameSourceCollectionChanged;
                });

                frameSource.IfIs<Collection<IFramePreviewerViewModel>>(it =>
                {
                    Controller.SetTotalFrameCount(it);
                });
            }

        }

        private void DisposeFrameSource(IEnumerable<IFramePreviewerViewModel> frameSource)
        {
            frameSource.IfIs<INotifyCollectionChanged>(it =>
            {
                it.CollectionChanged -= FrameSourceCollectionChanged;
            });
        }

        private void FrameSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e)
            {
                case CustomNotifyCollectionChangedEventArgs cast:
                    if (cast.IsSwitchAction)
                    {
                        Controller.SwitchFrame(cast.Switched1stIndex, cast.Switched2ndIndex);
                    }
                    return;
                case NotifyCollectionChangedEventArgs:
                    if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        var sizeChanged = e.OldItems?.Count ?? 0;
                        for (int i = e.OldStartingIndex; i < e.OldStartingIndex + sizeChanged; i++)
                        {
                            Controller.RemoveFrame((uint)i);
                        }
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        var sizeChanged = e.NewItems?.Count ?? 0;
                        for (int i = e.NewStartingIndex; i < e.NewStartingIndex + sizeChanged; i++)
                        {
                            if (e.NewItems != null)
                            {
                                var itemIndex = i - e.NewStartingIndex;
                                var vm = (IFramePreviewerViewModel)e.NewItems[itemIndex]!;
                                Controller.InsertFrame((uint)i, vm);
                            }
                        }
                    }
                    return;
            }

        }

    }
}
