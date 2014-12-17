using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Shared.Animation
{
    public abstract class AnimationBase
    {
        #region Constructor

        public AnimationBase()
        {
        }

        #endregion

        #region Properties

        protected Storyboard _Storyboard = null;
        protected FrameworkElement AnimationTarget = null;

        #endregion

        #region Animation

        public virtual void Stop()
        {
            this._Storyboard.Stop();
        }

        protected static void EnsureTransform(FrameworkElement cell)
        {
            CompositeTransform transform = cell.RenderTransform as CompositeTransform;
            if (transform == null)
            {
                cell.RenderTransform = transform = new CompositeTransform();
                cell.RenderTransformOrigin = new Point(0.5d, 0.5d);
            }
        }

        #endregion

        #region Events

        protected Action<FrameworkElement> AnimationCompleted = null;

        #endregion

    }
}
