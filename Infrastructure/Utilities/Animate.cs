//-----------------------------------------------------------------------
// <copyright file="Animate.cs" company="WPA">
//     Copyright (c) WPA. All rights reserved.
// </copyright>
// <author>Tudor</author>
//-----------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Infrastructure.Utilities
{
    /// <summary>
    /// Animate static class declaration
    /// </summary>
    public static class Animate
    {
        #region PUBLIC METHODS
        /// <summary>
        /// Slides the specified old content.
        /// </summary>
        /// <param name="oldContent">The old content.</param>
        /// <param name="newContent">The new content.</param>
        /// <param name="container">The container.</param>
        /// <param name="time">The time value.</param>
        /// <param name="animationDirection">The animation direction.</param>
        /// <param name="easingFunction">The easing function.</param>
        /// <param name="whenDone">The when done.</param>
        public static void Slide(object oldContent, object newContent, ContentControl container, double time, Direction animationDirection, IEasingFunction easingFunction, EventHandler whenDone = null)
        {
            var visual = new Border();
            visual.Background = CreateBrushFromVisual(container.Content as FrameworkElement, ((FrameworkElement)container.Content).ActualWidth, ((FrameworkElement)container.Content).ActualHeight);
            visual.VerticalAlignment = VerticalAlignment.Top;

            if (container.Parent is Grid)
            {
                ((Grid)container.Parent).Children.Add(visual);
                container.Content = newContent;

                var visualTransform = new TranslateTransform();
                visual.RenderTransform = visualTransform;

                var newContentTransform = new TranslateTransform();
                container.RenderTransform = newContentTransform;

                switch (animationDirection)
                {
                    case Direction.Left:
                        newContentTransform.BeginAnimation(
                            TranslateTransform.XProperty,
                            CreateDoubleAnimationWithEasing(
                            container.ActualWidth,
                            0,
                            time,
                            easingFunction,
                            whenDone));

                        visualTransform.BeginAnimation(
                            TranslateTransform.XProperty,
                            CreateDoubleAnimation(
                            0,
                            -container.ActualWidth,
                            time,
                            (o, e) => ((Grid)container.Parent).Children.Remove(visual)));


                        break;

                    case Direction.Right:

                        newContentTransform.BeginAnimation(TranslateTransform.XProperty, CreateDoubleAnimationWithEasing(-container.ActualWidth, 0, time, easingFunction, whenDone));

                        visualTransform.BeginAnimation(
                            TranslateTransform.XProperty,
                            CreateDoubleAnimation(
                            0,
                            container.ActualWidth,
                            time,
                            (o, e) => ((Grid)container.Parent).Children.Remove(visual)));

                        break;
                }
            }
            else
            {
                MessageBox.Show("For animation feature, please replace the content container with a Grid");
            }
        }

        /// <summary>
        /// Scales from center.
        /// </summary>
        /// <param name="oldXScale">The old X scale.</param>
        /// <param name="newXScale">The new X scale.</param>
        /// <param name="oldYScale">The old Y scale.</param>
        /// <param name="newYScale">The new Y scale.</param>
        /// <param name="transformOrigin">The transform origin.</param>
        /// <param name="content">The content.</param>
        /// <param name="time">The time value.</param>
        /// <param name="easingFunction">The easing function.</param>
        /// <param name="whenDone">The when done.</param>
        public static void Scale(double oldXScale, double newXScale, double oldYScale, double newYScale, Point transformOrigin, object content, double time, IEasingFunction easingFunction, EventHandler whenDone = null)
        {
            var element = (FrameworkElement)content;
            var scaleTransform = new ScaleTransform();
            element.RenderTransform = scaleTransform;
            element.RenderTransformOrigin = transformOrigin;

            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, CreateDoubleAnimationWithEasing(oldYScale, newYScale, time, easingFunction));
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, CreateDoubleAnimationWithEasing(oldXScale, newXScale, time, easingFunction, whenDone));
        }

        /// <summary>
        /// Scales from center.
        /// </summary>
        /// <param name="oldXScale">The old X scale.</param>
        /// <param name="newXScale">The new X scale.</param>
        /// <param name="oldYScale">The old Y scale.</param>
        /// <param name="newYScale">The new Y scale.</param>
        /// <param name="transformOrigin">The transform origin.</param>
        /// <param name="content">The content.</param>
        /// <param name="time">The time value.</param>
        /// <param name="easingFunction">The easing function.</param>
        /// <param name="whenDone">The when done.</param>
        public static void ScaleLayout(double oldXScale, double newXScale, double oldYScale, double newYScale, Point transformOrigin, object content, double time, IEasingFunction easingFunction, EventHandler whenDone = null)
        {
            var element = (FrameworkElement)content;
            var scaleTransform = new ScaleTransform();
            element.LayoutTransform = scaleTransform;
            element.RenderTransformOrigin = transformOrigin;

            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, CreateDoubleAnimationWithEasing(oldYScale, newYScale, time, easingFunction));
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, CreateDoubleAnimationWithEasing(oldXScale, newXScale, time, easingFunction, whenDone));
        }

        /// <summary>
        /// Bounces the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="whenDone">The when done.</param>
        public static void Bounce(UIElement element, Direction direction, EventHandler whenDone = null)
        {
            var translateTransform = new TranslateTransform();
            element.RenderTransform = translateTransform;
            switch (direction)
            {
                case Direction.Up:
                    translateTransform.BeginAnimation(
                        TranslateTransform.YProperty,
                        CreateDoubleAnimation(
                        0,
                        -15,
                        0.2,
                        (o, e) => translateTransform.BeginAnimation(TranslateTransform.YProperty, CreateDoubleAnimationWithEasing(-15, 0, 0.8, new BounceEase { Bounces = 3, EasingMode = EasingMode.EaseOut })),
                        whenDone));
                    break;

                case Direction.Down:
                    translateTransform.BeginAnimation(
                        TranslateTransform.YProperty,
                        CreateDoubleAnimation(
                        0,
                        15,
                        0.2,
                        (o, e) => translateTransform.BeginAnimation(TranslateTransform.YProperty, CreateDoubleAnimationWithEasing(15, 0, 0.8, new BounceEase { Bounces = 3, EasingMode = EasingMode.EaseOut })),
                        whenDone));
                    break;
            }
        }

        /// <summary>
        /// Fades the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="from">The value to be faded from.</param>
        /// <param name="to">The value to be faded to.</param>
        /// <param name="time">The time value.</param>
        /// <param name="whenDone">The when done.</param>
        public static void Fade(UIElement element, double from, double to, double time, EventHandler whenDone = null)
        {
            element.BeginAnimation(UIElement.OpacityProperty, CreateDoubleAnimation(from, to, time, null, whenDone));
        }

        /// <summary>
        /// Translates the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="horizontal">if set to <c>true</c> [horizontal].</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="time">The time.</param>
        /// <param name="whenDone">The when done.</param>
        public static void Translate(UIElement element, bool horizontal, double from, double to, double time, EventHandler whenDone = null)
        {
            var translateTransform = new TranslateTransform();
            element.RenderTransform = translateTransform;

            translateTransform.BeginAnimation(horizontal ? TranslateTransform.XProperty : TranslateTransform.YProperty, CreateDoubleAnimation(from, to, time, whenDone));
        }

        /// <summary>
        /// Creates the animation that moves content in or out of view.
        /// </summary>
        /// <param name="from">The starting value of the animation.</param>
        /// <param name="to">The end value of the animation.</param>
        /// <param name="time">The animation duration</param>
        /// <param name="whenDoubleAnimationDone">The when double animation done.</param>
        /// <returns>The animation object</returns>
        /// TODO: refactor this to a private method that's being used by a specific public animation method (fade,slide etc)
        public static AnimationTimeline CreateDoubleAnimation(double from, double to, double time, EventHandler whenDoubleAnimationDone = null)
        {
            var duration = new Duration(TimeSpan.FromSeconds(time));
            var animation = new DoubleAnimation(from, to, duration);

            if (whenDoubleAnimationDone != null)
            {
                animation.Completed += whenDoubleAnimationDone;
            }

            animation.Freeze();
            return animation;
        }
        #endregion

        #region PRIVATE METHODS
        /// <summary>
        /// Creates the animation that moves content in or out of view.
        /// </summary>
        /// <param name="from">The starting value of the animation.</param>
        /// <param name="to">The end value of the animation.</param>
        /// <param name="time">The animation duration</param>
        /// <param name="whenDoubleAnimationDone">The when double animation done.</param>
        /// <param name="whenParentAnimationDone">The when parent animation done.</param>
        /// <returns>The animation object</returns>
        /// TODO: refactor this to a private method that's being used by a specific public animation method (fade,slide etc)
        private static AnimationTimeline CreateDoubleAnimation(double from, double to, double time, EventHandler whenDoubleAnimationDone = null, EventHandler whenParentAnimationDone = null)
        {
            var duration = new Duration(TimeSpan.FromSeconds(time));
            var animation = new DoubleAnimation(from, to, duration);

            if (whenDoubleAnimationDone != null)
            {
                animation.Completed += whenDoubleAnimationDone;
            }

            if (whenParentAnimationDone != null)
            {
                animation.Completed += whenParentAnimationDone;
            }

            animation.Freeze();
            return animation;
        }

        /// <summary>
        /// Creates the animation that moves content in or out of view.
        /// </summary>
        /// <param name="from">The starting value of the animation.</param>
        /// <param name="to">The end value of the animation.</param>
        /// <param name="time">The animation duration</param>
        /// <param name="easingFunction">The easing function.</param>
        /// <param name="whenDoubleAnimationDone">The when double animation done.</param>
        /// <param name="whenParentAnimationDone">The when parent animation done.</param>
        /// <returns>The animation object</returns>
        /// TODO: refactor this to a private method that's being used by a specific public animation method (fade,slide etc)
        private static AnimationTimeline CreateDoubleAnimationWithEasing(double from, double to, double time, IEasingFunction easingFunction, EventHandler whenDoubleAnimationDone = null, EventHandler whenParentAnimationDone = null)
        {
            var duration = new Duration(TimeSpan.FromSeconds(time));
            var animation = new DoubleAnimation(from, to, duration);

            if (easingFunction != null)
            {
                animation.EasingFunction = easingFunction;
            }

            if (whenDoubleAnimationDone != null)
            {
                animation.Completed += whenDoubleAnimationDone;
            }

            if (whenParentAnimationDone != null)
            {
                animation.Completed += whenParentAnimationDone;
            }

            animation.Freeze();
            return animation;
        }

        /// <summary>
        /// Creates the brush from visual.
        /// </summary>
        /// <param name="v">The visual object.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The image brush created from the visual</returns>
        public static ImageBrush CreateBrushFromVisual(Visual v, double width, double height)
        {
            try
            {
                ImageBrush brush = null;

                if (height > 0 && width > 0)
                {
                    var target = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Default);
                    target.Render(v);
                    brush = new ImageBrush(target);
                    brush.Stretch = Stretch.None;
                    brush.Freeze();
                }

                return brush;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        #endregion
    }
}
