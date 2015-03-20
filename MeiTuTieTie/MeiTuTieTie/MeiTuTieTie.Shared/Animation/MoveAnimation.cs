﻿using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Shared.Animation
{
    public class MoveAnimation : AnimationBase
    {
        #region Constructor

        public MoveAnimation()
        {
            Init();
        }

        public static MoveAnimation PickupAnimationNonPooling()
        {
            return new MoveAnimation();
        }

        #endregion

        #region Properties

        private DoubleAnimationUsingKeyFrames _Animation_X = null;
        private DoubleAnimationUsingKeyFrames _Animation_Y = null;

        private EasingDoubleKeyFrame _KeyFrame_x_from = null;
        private EasingDoubleKeyFrame _KeyFrame_x_to = null;
        private EasingDoubleKeyFrame _KeyFrame_y_from = null;
        private EasingDoubleKeyFrame _KeyFrame_y_to = null;

        private double TargetX = 0;
        private double TargetY = 0;

        private static Stack<MoveAnimation> AnimationPool = new Stack<MoveAnimation>();

        #endregion

        #region Animation

        private void Init()
        {
            _Storyboard = new Storyboard();
            _Storyboard.Completed += _Storyboard_Completed;

            /***animation x***/
            _Animation_X = new DoubleAnimationUsingKeyFrames();

            /*key frame x from*/
            _KeyFrame_x_from = new EasingDoubleKeyFrame();
            _KeyFrame_x_from.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            _KeyFrame_x_from.Value = 0;
            _Animation_X.KeyFrames.Add(_KeyFrame_x_from);

            /*key frame x to*/
            _KeyFrame_x_to = new EasingDoubleKeyFrame();
            _KeyFrame_x_to.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            _KeyFrame_x_to.Value = 1;
            _Animation_X.KeyFrames.Add(_KeyFrame_x_to);

            Storyboard.SetTargetProperty(_Animation_X, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");

            _Storyboard.Children.Add(_Animation_X);


            /***animation y***/
            _Animation_Y = new DoubleAnimationUsingKeyFrames();

            /*key frame 1*/
            _KeyFrame_y_from = new EasingDoubleKeyFrame();
            _KeyFrame_y_from.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            _KeyFrame_y_from.Value = 0;
            _Animation_Y.KeyFrames.Add(_KeyFrame_y_from);

            /*key frame 2*/
            _KeyFrame_y_to = new EasingDoubleKeyFrame();
            _KeyFrame_y_to.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1));
            _KeyFrame_y_to.Value = 1;
            _Animation_Y.KeyFrames.Add(_KeyFrame_y_to);

            Storyboard.SetTargetProperty(_Animation_Y, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            _Storyboard.Children.Add(_Animation_Y);
        }

        public static void SetPosition(FrameworkElement cell, double x, double y)
        {
            EnsureTransform(cell);
            cell.RenderTransform.SetValue(CompositeTransform.TranslateXProperty, x);
            cell.RenderTransform.SetValue(CompositeTransform.TranslateYProperty, y);
        }

        public static MoveAnimation MoveTo(FrameworkElement cell,
            double x, double y,
            double duration,
            EasingFunctionBase easing = null,
            Action<FrameworkElement> completed = null)
        {
            MoveAnimation animation = null;
            if (AnimationPool.Count == 0)
            {
                animation = new MoveAnimation();
            }
            else
            {
                animation = AnimationPool.Pop();
            }
            animation.InstanceMoveTo(cell, x, y, duration, easing, completed);
            return animation;
        }

        public static MoveAnimation MoveBy(FrameworkElement cell,
            double x, double y,
            double duration,
            EasingFunctionBase easing = null,
            Action<FrameworkElement> completed = null)
        {
            MoveAnimation animation = null;
            if (AnimationPool.Count == 0)
            {
                animation = new MoveAnimation();
            }
            else
            {
                animation = AnimationPool.Pop();
            }

            animation.InstanceMoveBy(cell, x, y, duration, easing, completed);
            return animation;
        }

        public static void MoveBy(FrameworkElement cell, double x, double y)
        {
            EnsureTransform(cell);

            CompositeTransform transform = cell.RenderTransform as CompositeTransform;
            var fromX = transform.TranslateX;
            var toX = fromX + x;
            var fromY = transform.TranslateY;
            var toY = fromY + y;

            SetPosition(cell, toX, toY);
        }

        public static MoveAnimation MoveFromTo(FrameworkElement cell,
            double from_x, double from_y,
            double to_x, double to_y,
            double duration,
            EasingFunctionBase easing = null,
            Action<FrameworkElement> completed = null)
        {
            MoveAnimation animation = null;
            if (AnimationPool.Count == 0)
            {
                animation = new MoveAnimation();
            }
            else
            {
                animation = AnimationPool.Pop();
            }

            animation.InstanceMoveFromTo(cell, from_x, from_y, to_x, to_y, duration, easing, completed);
            return animation;
        }

        public void InstanceMoveBy(FrameworkElement cell,
            double x, double y,
            double duration,
            EasingFunctionBase easing = null,
            Action<FrameworkElement> completed = null)
        {
            EnsureTransform(cell);

            CompositeTransform transform = cell.RenderTransform as CompositeTransform;
            var fromX = transform.TranslateX;
            var toX = fromX + x;
            var fromY = transform.TranslateY;
            var toY = fromY + y;

            this.InstanceMoveTo(cell, toX, toY, duration, easing, completed);
        }

        public void InstanceMoveFromTo(FrameworkElement cell,
            double from_x, double from_y,
            double to_x, double to_y,
            double duration,
            EasingFunctionBase easing = null,
            Action<FrameworkElement> completed = null)
        {
            EnsureTransform(cell);

            SetPosition(cell, from_x, from_y);
            this.InstanceMoveTo(cell, to_x, to_y, duration, easing, completed);
        }

        public void InstanceMoveTo(FrameworkElement cell, double x, double y, double duration, EasingFunctionBase easing = null, Action<FrameworkElement> completed = null)
        {
            this.Animate(cell, x, y, duration, easing, completed);
        }

        private void Animate(FrameworkElement cell, double x, double y, double duration, EasingFunctionBase easing = null, Action<FrameworkElement> completed = null)
        {
            AnimationTarget = cell;
            TargetX = x;
            TargetY = y;
            AnimationCompleted = completed;

            if (_Storyboard == null)
            {
                Init();
            }
            else
            {
                _Storyboard.Stop();
            }

            /*time*/
            _KeyFrame_x_to.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration));
            _KeyFrame_y_to.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration));

            /*value*/
            CompositeTransform transform = cell.RenderTransform as CompositeTransform;
            _KeyFrame_x_from.Value = transform.TranslateX;
            _KeyFrame_x_to.Value = x;
            _KeyFrame_y_from.Value = transform.TranslateY;
            _KeyFrame_y_to.Value = y;

            /*easing*/
            _KeyFrame_x_from.EasingFunction = easing;
            _KeyFrame_y_from.EasingFunction = easing;
            _KeyFrame_x_to.EasingFunction = easing;
            _KeyFrame_y_to.EasingFunction = easing;

            Storyboard.SetTarget(_Animation_X, AnimationTarget);
            Storyboard.SetTarget(_Animation_Y, AnimationTarget);

            _Storyboard.Begin();
        }

        private void _Storyboard_Completed(object sender, object e)
        {
            SetPosition(AnimationTarget, TargetX, TargetY);

            if (AnimationCompleted != null)
            {
                AnimationCompleted(AnimationTarget);
            }

            AnimationPool.Push(this);
            AnimationTarget = null;
        }

        #endregion

    }
}
