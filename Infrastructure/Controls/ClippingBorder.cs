//-----------------------------------------------------------------------
// <copyright file="ClippingBorder.cs" company="WPA">
//     Copyright (c) WPA. All rights reserved.
// </copyright>
// <author>Tudor</author>

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Infrastructure.Controls
{
    /// <summary>
    /// ClippingBorder class implementation
    /// </summary>
    [DebuggerStepThrough]
    public class ClippingBorder : Border
    {
        #region PRIVATE FIELDS
        /// <summary>
        /// The clipRectangle field
        /// </summary>
        private RectangleGeometry clipRectangle = new RectangleGeometry();

        /// <summary>
        /// The old clip field
        /// </summary>
        private object oldClip;
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Gets or sets the Child property
        /// </summary>
        public override UIElement Child
        {
            get
            {
                return base.Child;
            }

            set
            {
                if (this.Child != value)
                {
                    if (this.Child != null)
                    {
                        // Restore original clipping
                        this.Child.SetValue(UIElement.ClipProperty, this.oldClip);
                    }

                    if (value != null)
                    {
                        this.oldClip = value.ReadLocalValue(UIElement.ClipProperty);
                    }
                    else
                    {
                        // If we dont set it to null we could leak a Geometry object
                        this.oldClip = null;
                    }

                    base.Child = value;
                }
            }
        }
        #endregion

        #region OVERRIDES
        /// <summary>
        /// Override for the OnRender methods
        /// </summary>
        /// <param name="dc">The drawing context</param>
        [DebuggerStepThrough]
        protected override void OnRender(DrawingContext dc)
        {
            this.OnApplyChildClip();
            base.OnRender(dc);
        }
        #endregion

        #region PROTECTED METHODS
        /// <summary>
        /// Method called when child clip is applied
        /// </summary>
        [DebuggerStepThrough]
        protected virtual void OnApplyChildClip()
        {
            var child = this.Child;
            if (child != null)
            {
                this.clipRectangle.RadiusX = this.clipRectangle.RadiusY = Math.Max(0.0, this.CornerRadius.TopLeft - (this.BorderThickness.Left * 0.5));
                this.clipRectangle.Rect = new Rect(this.Child.RenderSize);
                child.Clip = this.clipRectangle;
            }
        }
        #endregion
    }
}