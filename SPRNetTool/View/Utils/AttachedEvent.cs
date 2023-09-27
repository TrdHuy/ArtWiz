using SPRNetTool.LogUtil;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SPRNetTool.View.Utils
{

    public delegate void MouseHoldEventHandler(object sender, MouseHoldEventArgs args);

    public class MouseHoldEventArgs : RoutedEventArgs
    {
        public int HoldingCounter { get; private set; }

        public MouseHoldEventArgs(int counter)
        {
            HoldingCounter = counter;
        }
    }

    public static class AttachedEvent
    {
        private static Logger logger = new Logger(typeof(AttachedEvent).Name);

        #region MouseHoldEvent
        private class HoldingEventCache
        {
            public event MouseHoldEventHandler? MouseHold;

            DependencyObject sender;
            public int HandlerSize = 0;
            public bool IsCaptured = false;
            public SemaphoreSlim HoldingSemaphore = new SemaphoreSlim(1, 1);
            public CancellationTokenSource? HoldingCts = null;
            public HoldingEventCache(DependencyObject sender)
            {
                this.sender = sender;
            }
            public async void OnHolding()
            {
                logger.D($"Begin counting down to start mouse holding event!");

                if (HoldingCts != null)
                {
                    HoldingCts.Cancel();
                }
                HoldingCts = new CancellationTokenSource();
                try
                {
                    int delayBeforeEnteringHoldingMode = 1000;
                    await Task.Delay(delayBeforeEnteringHoldingMode, HoldingCts.Token);
                }
                catch (TaskCanceledException ex)
                {
                    logger.E($"Counting down task canceled!");
                    return;
                }
                try
                {
                    await HoldingSemaphore.WaitAsync();
                    int delayEachEvent = 1;
                    int holdingCounter = 0;


                    if (HoldingCts?.IsCancellationRequested != true)
                    {
                        //await Task.Run(async () =>
                        //{
                        logger.D($"On holding event, senderType={sender.GetType().FullName}");

                        while (IsCaptured)
                        {
                            MouseHold?.Invoke(sender, new MouseHoldEventArgs(holdingCounter++));

                            await Task.Delay(delayEachEvent, HoldingCts!.Token);
                            if (HoldingCts!.IsCancellationRequested)
                            {
                                break;
                            }
                        }
                        //}, HoldingCts!.Token);
                    }

                }
                catch (Exception ex)
                {
                    logger.E($"{ex.Message}");
                }
                finally
                {
                    HoldingSemaphore.Release();
                    HoldingCts = null;
                }
            }
        }

        private static Dictionary<DependencyObject, HoldingEventCache> holdingEventCacheList
            = new Dictionary<DependencyObject, HoldingEventCache>();

        public static void AddMouseHoldHandler(DependencyObject element, MouseHoldEventHandler handler)
        {
            if (!holdingEventCacheList.ContainsKey(element))
            {
                holdingEventCacheList.Add(element, new HoldingEventCache(element));
                switch (element)
                {
                    case ContentElement contentElement:
                        contentElement.MouseDown += HoldEventMouseDown;
                        contentElement.MouseUp += HoldEventMouseUp;
                        contentElement.MouseLeave += HoldEventMouseLeave;
                        break;
                    case UIElement uIElement:
                        uIElement.MouseDown += HoldEventMouseDown;
                        uIElement.MouseUp += HoldEventMouseUp;
                        uIElement.MouseLeave += HoldEventMouseLeave;
                        break;
                }
            }

            holdingEventCacheList[element].MouseHold += handler;
            holdingEventCacheList[element].HandlerSize++;
        }

        public static void RemoveMouseHoldHandler(DependencyObject element, MouseHoldEventHandler handler)
        {
            if (holdingEventCacheList.ContainsKey(element))
            {
                holdingEventCacheList[element].MouseHold -= handler;
                holdingEventCacheList[element].HandlerSize--;

                if (holdingEventCacheList[element].HandlerSize == 0)
                {
                    switch (element)
                    {
                        case ContentElement contentElement:
                            contentElement.MouseDown -= HoldEventMouseDown;
                            contentElement.MouseUp -= HoldEventMouseUp;
                            contentElement.MouseLeave -= HoldEventMouseLeave;
                            break;
                        case UIElement uIElement:
                            uIElement.MouseDown -= HoldEventMouseDown;
                            uIElement.MouseUp -= HoldEventMouseUp;
                            uIElement.MouseLeave -= HoldEventMouseLeave;
                            break;
                    }
                    holdingEventCacheList.Remove(element);
                }
            }
        }

        private static void HoldEventMouseLeave(object sender, MouseEventArgs e)
        {
            var cache = holdingEventCacheList[(DependencyObject)sender];
            if (cache != null)
            {
                cache.HoldingCts?.Cancel();
                cache.IsCaptured = false;
            }
        }

        private static void HoldEventMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cache = holdingEventCacheList[(DependencyObject)sender];
            if (cache != null)
            {
                cache.IsCaptured = true;
                cache.OnHolding();
            }
        }

        private static void HoldEventMouseUp(object sender, MouseButtonEventArgs e)
        {
            var cache = holdingEventCacheList[(DependencyObject)sender];
            if (cache != null)
            {
                cache.HoldingCts?.Cancel();
                cache.IsCaptured = false;
            }
        }
        #endregion

    }
}
