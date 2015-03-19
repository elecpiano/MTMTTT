using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Shared.Utility
{
    public static class DelayExecutor
    {
        private static DispatcherTimer timer = null;

        public static void Delay(double delayMilliseconds,Action action)
        {

            if (timer == null)
            {
                timer = new DispatcherTimer();
            }

            timer.Interval = TimeSpan.FromMilliseconds(delayMilliseconds);
            timer.Tick += (ss, ee) =>
            {
                timer.Stop();
                if (action!=null)
                {
                    action();
                }
            };
            timer.Start();
        }

    }
}
